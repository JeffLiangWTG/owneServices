using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	[AllowPublicConstructor]
	public class NctsIE29DocumentWrapper : DocBaseWrapper, INctsIE29DocumentWrapper
	{
		public NctsIE29DocumentWrapper(NctsEdiMessage ediMessage, BusinessObjectFactory factoryToWrap, NctsIE29CusdecResponseData cusdecResponseData)
			: base(ediMessage, factoryToWrap)
		{
			this.factoryToWrap = factoryToWrap;
			this.cusdecResponseData = cusdecResponseData;
			this.ediMessage = ediMessage;
		}

		public static INctsIE29DocumentWrapper New(NctsEdiMessage ediMessage, BusinessObjectFactory factoryToWrap)
		{
			var ie29Parser = ediMessage.Header.GetIE29CusdecParser(ediMessage);
			if (ie29Parser != null)
			{
				var cusdecResponseData = ie29Parser.Parse();

				if (cusdecResponseData != null)
				{
					return ediMessage != null && (cusdecResponseData.IsSecurity || ediMessage.Header.IsPhase5)
								? new SecurityNctsIE29DocumentWrapper(ediMessage, factoryToWrap, cusdecResponseData)
								: new NctsIE29DocumentWrapper(ediMessage, factoryToWrap, cusdecResponseData);
				}
			}
			return null;
		}

		public static INctsIE29DocumentWrapper New(NctsEdiMessage ediMessage)
		{
			return New(ediMessage, ediMessage.Factory);
		}

		public ZBool IsPhase5 => ediMessage?.Header?.IsPhase5 ?? false;

		public DocBaseWrapperCollection<NctsIE29GoodsItemLineWrapper> Lines
		{
			get { return lines ?? (lines = new NctsIE29GoodsItemLineWrapperCollection(cusdecResponseData, ediMessage)); }
		}

		public ZString BOX1REGIME
		{
			get { return cusdecResponseData.DeclarationType; }
		}

		public ZString BOX2CONSIGNOR
		{
			get { return GetWrappedAddress(cusdecResponseData.Consignor); }
		}

		public ZString BOX2CONSIGNOREORI
		{
			get { return cusdecResponseData.Consignor.GovRegNum; }
		}

		public ZString BOX15COUNTRYOFORIGIN
		{
			get { return cusdecResponseData.CountryOfDispatch; }
		}

		public ZString BOX17COUNTRYOFDESTINATION
		{
			get { return cusdecResponseData.CountryOfDestination; }
		}

		public ZString MOVEMENTREFERENCENUMBER
		{
			get { return cusdecResponseData.MovementReferenceNumber; }
		}

		public ZString EMAILSUBJECT
		{
			get { return MOVEMENTREFERENCENUMBER; }
		}

		public ZString BOXCOFFICEOFDEPARTURE
		{
			get { return GetCustomsOfficeName(cusdecResponseData.DepartureCustomsOfficeCode); }
		}

		public ZString BOXCOFFICEOFDEPARTURECODE
		{
			get { return cusdecResponseData.DepartureCustomsOfficeCode; }
		}

		ZString GetCustomsOfficeName(string officeCode)
		{
			var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, officeCode);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			var office = Factory.LoadTop1<ZZRefCusCodeListCombined>(query);
			return office == null ? officeCode : officeCode + " (" + office.ZZD_Description + ")";
		}

		public ZString RETURNOFFICEADDRESS
		{
			get { return GetWrappedAddress(cusdecResponseData.ReturnCopiesCustomsOffice); }
		}

		public ZString BOX53OFFICEOFDESTINATION
		{
			get { return cusdecResponseData.DestinationCustomsOfficeCode; }
		}

		public ZString BOX5ITEMS
		{
			get { return cusdecResponseData.TotalNumberOfItems.ToString(); }
		}

		public ZString BOX6PACKAGES
		{
			get { return cusdecResponseData.TotalNumberOfPackages.ToString(); }
		}

		public ZString BOX8CONSIGNEE
		{
			get { return GetWrappedAddress(cusdecResponseData.Consignee); }
		}

		public ZString BOX8CONSIGNEEEORI
		{
			get { return cusdecResponseData.Consignee.GovRegNum; }
		}

		public ZString BOX18DEPARTURETRANSPORTID
		{
			get { return cusdecResponseData.MeansOfTransportAtDepartureIdentity; }
		}

		public ZString BOX18DEPARTURETRANSPORTFLAG
		{
			get { return cusdecResponseData.MeansOfTransportAtDepartureNationality; }
		}

		public ZString BOX35GROSSMASS
		{
			get { return Tools.GetFormattedDecimal(cusdecResponseData.TotalGrossMassInKilograms); }
		}

		public ZString BOX38NETTMASS
		{
			get { return Tools.GetFormattedDecimal(cusdecResponseData.TotalNettMassInKilograms); }
		}

		public ZString BOX50PRINCIPAL
		{
			get { return GetWrappedAddress(cusdecResponseData.Principal); }
		}

		public ZString BOX50PRINCIPALEORI
		{
			get { return cusdecResponseData.Principal.GovRegNum; }
		}

		public ZString BOX50SIGNATURE => ZString.Empty;

		public ZString BOX51TRANSITOFFICE1
		{
			get { return GetWrappedTransitCustomsOffices(cusdecResponseData.TransitCustomsOffices, 1); }
		}

		public ZString BOX51TRANSITOFFICE2
		{
			get { return GetWrappedTransitCustomsOffices(cusdecResponseData.TransitCustomsOffices, 2); }
		}

		public ZString BOX51TRANSITOFFICE3
		{
			get { return GetWrappedTransitCustomsOffices(cusdecResponseData.TransitCustomsOffices, 3); }
		}

		public ZString BOX51TRANSITOFFICE4
		{
			get { return GetWrappedTransitCustomsOffices(cusdecResponseData.TransitCustomsOffices, 4); }
		}

		public ZString BOX51TRANSITOFFICE5
		{
			get { return GetWrappedTransitCustomsOffices(cusdecResponseData.TransitCustomsOffices, 5); }
		}

		public ZString BOX51TRANSITOFFICE6
		{
			get { return GetWrappedTransitCustomsOffices(cusdecResponseData.TransitCustomsOffices, 6); }
		}

		public ZString BOX52GUARANTEE
		{
			get
			{  // show up to 3 guarantees
				var sb = new ZStringBuilder();
				if (cusdecResponseData.Guarantees != null)
				{
					for (int i = 0; (i < 3 && i < cusdecResponseData.Guarantees.Count); i++)
					{
						var fieldToUse = cusdecResponseData.Guarantees[i].GuaranteeReferenceNumber.IsEmpty ? cusdecResponseData.Guarantees[i].OtherGuaranteeReference : cusdecResponseData.Guarantees[i].GuaranteeReferenceNumber;
						if (!fieldToUse.IsEmpty)
						{
							sb.AppendIfNotEmpty(fieldToUse.PadRight(NctsGuarantee.Schema.PW_BondNumber2MaxLength));
						}
					}
				}
				return sb.ToStringWithDelimiterBetweenAppends(";").TrimEnd();
			}
		}

		public ZString BOX52GUARANTEEVALIDITY
		{
			get
			{
				var sb = new ZStringBuilder();
				if (cusdecResponseData.Guarantees != null)
				{
					foreach (var guarantee in cusdecResponseData.Guarantees)
					{
						sb.AppendIfNotEmpty(guarantee.ValidityLimitationOther);
					}
				}
				return new ZString(sb.ToStringWithDelimiterBetweenAppends(",")).Left(NctsGuarantee.Schema.PW_ValidityLimitationMaxLength);
			}
		}

		public ZString BOX52GUARANTEECODE
		{
			get
			{
				// show up to 3 guarantees
				var sb = new ZStringBuilder();
				if (cusdecResponseData.Guarantees != null)
				{
					for (int i = 0; (i < 3 && i < cusdecResponseData.Guarantees.Count); i++)
					{
						sb.AppendIfNotEmpty(cusdecResponseData.Guarantees[i].GuaranteeType);
					}
				}
				return sb.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		public ZString BOXCDATE
		{
			get { return cusdecResponseData.ControlResultControlDate; }
		}

		public ZBool SHOWSTAMPONBOXC => ZBool.False;

		public ZString BOXCDEPARTUREOFFICECOUNTRYCODE => ZString.Empty;

		public ZString BOXCAUTHORIZEDCONSIGNORNAME => ZString.Empty;

		public ZString BOXCAUTHORISATIONNUMBER => ZString.Empty;

		public ZString BOXCUNIQUEREFERENCENUMBER => ZString.Empty;

		public ZString BOXDRESULT
		{
			get
			{
				var resultCodeList = new NctsControlResult();
				return cusdecResponseData.ControlResultControlResultCode + " - " + resultCodeList.GetDescriptionFromCode(cusdecResponseData.ControlResultControlResultCode);
			}
		}

		public ZString BOXDSEALSAFFIXEDNUMBER
		{
			get { return cusdecResponseData.SealsNumber; }
		}

		public virtual ZString BOXDSEALSIDENTITY
		{
			get { return ConsecutiveSequenceCondenser.Condense(cusdecResponseData.Seals); }
		}

		public ZString BOXDTIMELIMITDATE
		{
			get { return cusdecResponseData.ControlResultTimeLimit; }
		}

		public ZString BOXDSIGNATURE => ZString.Empty;

		public ZString EDIENTERPRISEVERSION
		{
			get { return (NoResString)"WiseTechGlobal.com - " + Core.Constants.ProductName + (NoResString)" v" + EnterpriseInfo.VersionNumber; }
		}

		public ZString FALLBACKINFORMATION
		{
			get { return ZString.Empty; }
		}

		public ZString NOTRELEASEDWATERMARK => ZString.Empty;

		public ZString BOXDCLEARANCE => ZString.Empty;

		public ZString LOCALREFERENCENUMBER
		{
			get { return cusdecResponseData.LocalReferenceNumber; }
		}

		#region Implementation

		EnterpriseInformationRetriever EnterpriseInfo
		{
			get { return fEnterpriseInfo ?? (fEnterpriseInfo = new EnterpriseInformationRetriever()); }
		}
		EnterpriseInformationRetriever fEnterpriseInfo;

		protected ZString GetWrappedAddress(AddressResponseData address)
		{
			return address != null && !address.CompanyName.IsEmpty ? ZString.Format("{0}\n{1}\n{2}\n{3} {4}", address.CompanyName, address.Address1, address.City, address.Postcode, address.CountryCode) : ZString.Empty;
		}

		ZString GetWrappedTransitCustomsOffices(List<CustomsOfficeResponseData> officeList, int officeNumber)
		{
			var transitCustomsOffices = ZString.Empty;
			if (officeList != null)
			{
				transitCustomsOffices = officeNumber > 0 && officeNumber <= officeList.Count ? officeList[officeNumber - 1].OfficeCode : ZString.Empty;
			}
			return transitCustomsOffices;
		}

		protected NctsIE29GoodsItemLineWrapperCollection lines;
		protected BusinessObjectFactory factoryToWrap;
		protected NctsIE29CusdecResponseData cusdecResponseData;
		protected NctsEdiMessage ediMessage;

		#endregion
	}
}
