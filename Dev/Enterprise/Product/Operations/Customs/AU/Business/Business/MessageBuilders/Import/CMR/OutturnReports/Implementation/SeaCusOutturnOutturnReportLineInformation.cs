using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class SeaCusOutturnOutturnReportLineInformation : CusOutturnOutturnReportLineInformation, ISeaOutturnReportLineInformation
	{
		public SeaCusOutturnOutturnReportLineInformation(CusOutturn outturn)
			: base(outturn)
		{
		}

		public ZString ContainerNumber
		{
			get { return GetContainerNumber(); }
		}

		public ZString HouseBillOfLading
		{
			get { return GetHouseBillOfLading(); }
		}

		public ZString OceanBillOfLading
		{
			get { return GetOceanBillOfLading(); }
		}

		public ZString SealNumber
		{
			get { return GetSealNumber(); }
		}

		public bool SealIntactIndicator
		{
			get { return Outturn.C5_SealIntactIndicator; }
		}

		public bool VesselDischargeUnderbondIndicator
		{
			get { return false; }
		}

		public bool UnpackIndicator
		{
			get { return UnpackIndicatorCore(); }
		}

		protected virtual bool UnpackIndicatorCore()
		{
			return !Outturn.C5_ReceiptOnlyIndicator;
		}

		public ZDateTime DateTimeOfCargoReceiptUnload
		{
			get
			{
				var result = ZDateTime.Empty;
				if (OutturnPort != null &&
					OutturnPort.TimeZoneSet != null &&
					!DateTimeOfCargoReceiptUnloadCore().IsEmpty &&
					DateTimeOfCargoReceiptUnloadCore().IsValid)
				{
					result = OutturnPort.TimeZoneSet.GetCalculationTimeZone().ToUniversalTime(DateTimeOfCargoReceiptUnloadCore().ToDateTime());
				}
				else
				{
					result = DateTimeOfCargoReceiptUnloadCore();
				}

				return result;
			}
		}

		protected virtual ZDateTime DateTimeOfCargoReceiptUnloadCore()
		{
			return Outturn.Underbond.C4_DateOfArrivalIntoDestinationPremise;
		}

		public ZDateTime DateTimeOfOutturn
		{
			get
			{
				var result = ZDateTime.Empty;
				if (UnpackIndicator)
				{
					if (OutturnPort != null &&
						OutturnPort.TimeZoneSet != null &&
						!DateTimeOfOutturnCore().IsEmpty &&
						DateTimeOfOutturnCore().IsValid)
					{
						result = OutturnPort.TimeZoneSet.GetCalculationTimeZone().ToUniversalTime(DateTimeOfOutturnCore().ToDateTime());
					}
					else
					{
						result = DateTimeOfOutturnCore();
					}
				}

				return result;
			}
		}

		protected virtual ZDateTime DateTimeOfOutturnCore()
		{
			return Outturn.Underbond.C4_Outurned;
		}

		RefUNLOCO OutturnPort
		{
			get
			{
				RefUNLOCO result = null;
				if (Outturn != null && Outturn.C5_C6.IsValid)
				{
					var premiseID = (ZString)Outturn.Factory.Load<CusOutturnHeader>(Outturn.C5_C6)?.C6_OutturningPremiseID;
					if (!premiseID.IsEmpty)
					{
						var establishmentCode = Outturn.Factory.LoadTop1<CMREstablishmentCodes>(new ZQuery(CMREstablishmentCodesSchema.EC_EstablishmentCode, premiseID));
						if (establishmentCode != null)
						{
							result = Outturn.Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, establishmentCode.EC_EstablishmentPortCode));
						}
					}
				}

				if (result == null)
				{
					result = Outturn.Factory.Load<CusOutturnHeader>(Outturn.C5_C6)
						?.OutturningPremise
						?.EffectiveRelatedPortCode;

					if (result == null)
					{
						result = GlbBranch.CurrentBranch?.HomePort ?? GlbCompany.CurrentCompany.OrgProxy.UNLOCO;
					}
				}

				return result;
			}
		}

		protected virtual OrgAddress GetDestinationAddress()
		{
			return Outturn.Underbond != null ? Outturn.Underbond.DestinationAddress : null;
		}

		public ZInt Quantity
		{
			get { return QuantityCore(); }
		}

		protected virtual ZInt QuantityCore()
		{
			return 0;
		}

		public ZString QuantityUnit
		{
			get { return QuantityUnitCore(); }
		}

		protected virtual ZString QuantityUnitCore()
		{
			return ZString.Empty;
		}

		public ZString ImportCargoType
		{
			get { return GetImportCargoType(); }
		}

		public ZString PackageType
		{
			get { return GetPackageType(); }
		}

		public ZString MarksAndNumbers
		{
			get { return GetMarksAndNumbers(); }
		}

		public ZString OutturnStatus
		{
			get { return GetOutturnStatus(); }
		}

		protected abstract ZString GetContainerNumber();
		protected abstract ZString GetSealNumber();
		protected abstract ZString GetHouseBillOfLading();
		protected abstract ZString GetOceanBillOfLading();
		protected abstract ZString GetImportCargoType();
		protected abstract ZString GetPackageType();
		protected abstract ZString GetMarksAndNumbers();
		protected abstract ZString GetOutturnStatus();
	}
}
