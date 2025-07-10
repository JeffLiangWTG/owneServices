using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	[CodeProperty(Schema.C4_SendersMessageReference)]
	[DescriptionProperty("Description")]
	public abstract class CusUnderbond : Customs.Business.CusUnderbond, Integration.Customs.GB.CCSUK.ICusUnderbond
	{
		public CusUnderbond(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public abstract ZString Description { get; }

		public abstract ZString RemovalTypeHuman { get; }

		public ZString AgentsReference
		{
			get { return C4_SendersMessageReference.Replace("U", "").Substring(0, 8); }
			set
			{
				if (value.StartsWith("U"))
				{
					C4_SendersMessageReference = value;
				}
				else
				{
					C4_SendersMessageReference = "U" + value;
				}
			}
		}

		public override bool ReadOnly
		{
			get { return (WholeAwb != null && WholeAwb.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.OnwardTawbCarrier)) || C4_Status == EDIMessage.Status.Cancelled; }
			set { base.ReadOnly = value; }
		}

		public static readonly new TypeDecider TypeDecider = new CusUnderbondTypeDecider();

		public new CusUnderbondValidation Validation => (CusUnderbondValidation)base.Validation;

		public new CusUnderbondLookups Lookups => (CusUnderbondLookups)base.Lookups;
		protected override Customs.Business.CusUnderbondLookups GetNewLookups() => new CusUnderbondLookups(this);

		protected override ZString HumanReadableNameCore => Description;
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LicenseRestrictionIndCore = ZString.Empty; // HMRC accreditation rule 3a
			C4_MovementReason = RemovalTypeAttributeHelper.RemovalTypeCode;
			DefaultPackagesFromParent();
		}

		public override ZGuid C4_ParentID
		{
			get { return base.C4_ParentID; }
			set
			{
				base.C4_ParentID = value;
				HandleSettingOfParent();
			}
		}

		protected virtual void HandleSettingOfParent()
		{
			DefaultPackagesFromParent();
		}

		protected virtual void DefaultPackagesFromParent()
		{
			if (WholeAwb != null && !IsInDatabase)
			{
				NoPackagesExpected = WholeAwb.NumberOfPiecesExpected;
			}
		}

		protected RemovalTypeAttribute RemovalTypeAttributeHelper
		{
			get { return typeAttribute ?? (typeAttribute = RemovalTypeAttribute.Get(this.GetType())); }
		}
		RemovalTypeAttribute typeAttribute;

		// Current data - read only, taken from Cus*Awb parent.  All subtypes have these fields. 
		public ZString AirportOfReceipt
		{
			get { return WholeAwb != null ? WholeAwb.CargoTerminalOperatorAirport : ZString.Empty; }
		}

		public ZString AirwaybillNumber
		{
			get { return WholeAwb != null ? WholeAwb.MasterBill : ZString.Empty; }
		}

		public ZString HouseWaybillNumber
		{
			get { return Hawb != null ? Hawb.CS_HAWB : ZString.Empty; }
		}

		public ZString Shed
		{
			get { return WholeAwb != null ? WholeAwb.CargoTerminalOperator : ZString.Empty; }
		}

		// Onward data - read/write CusUnderbond table

		// Common onward data that all subtypes have
		[List(nameof(Lookups) + "." + nameof(CusUnderbondLookups.NonUkAirportsCollection))]
		public ZString AirportOrCountryOfDestination
		{
			get { return C4_RL_NKDischargePort; }
			set { C4_RL_NKDischargePort = value; }
		}
		public ZPropertyInfo AirportOrCountryOfDestinationInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(AirportOrCountryOfDestination), x => C4_RL_NKDischargePortInfo); }
		}

		public ZString AirportOrCountryOfDestination_IATA
		{
			get
			{
				var placeId = AirportOrCountryOfDestination;
				if (placeId.Length == 5)
				{
					var port = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, placeId);
					placeId = (port == null) ? placeId.Right(3) : port.RL_IATA;
				}
				return placeId;
			}
		}

		public ZInt NoPackagesExpected
		{
			get { return C4_PiecesManifested; }
			set { C4_PiecesManifested = value; }
		}

		// Onward data that not all types have
		protected ZString OnwardModeCore
		{
			get { return C4_ModeOfMovement; }
			set { C4_ModeOfMovement = value; }
		}  // Not in elements table, but in mesage spec

		protected ZString NewShedIdCore
		{
			get { return C4_DischargePremiseID; }
			set { C4_DischargePremiseID = value; }
		}

		/// <summary>
		/// Port of shipment is the LOCAL (GB) port of the transhipment.... thebase fieldname is 'destination'.... this is an abuse
		/// </summary>
		protected ZString PortOfShipmentCore
		{
			get { return C4_RL_NKTranshipDestPort; }
			set { C4_RL_NKTranshipDestPort = value; }
		}

		protected ZString CountryOfDestinationCore
		{
			get { return AirportOrCountryOfDestination.Left(2); }
		}

		protected ZString OnwardCarrierCore
		{
			get { return C4_FlightNo.Left(2); }
			set { C4_FlightNo = value.Left(2); }
		}

		protected ZString OnwardAirWaybillNumberCore
		{
			get { return C4_MAWB; }
			set { C4_MAWB = value; }
		}

		/// <summary>
		/// C4_UnderbondBySeaVoyage is the base column but we abuse this to hold the LICENCE indicator flag. Tut tut.   
		/// </summary>
		protected ZString LicenseRestrictionIndCore
		{
			get { return C4_UnderbondBySeaVoyage; }
			set { C4_UnderbondBySeaVoyage = value; }
		}

		public override ZPropertyInfo C4_UnderbondBySeaVoyageInfo
		{
			get { return GetZPropertyInfo(Schema.C4_UnderbondBySeaVoyage, "Licence/Restricted Indicator"); }
		}

		public ZBool ParentDoesNotHaveSplits
		{
			get { return WholeAwb != null && !WholeAwb.HasSplits; }
		}

		[ReadOnlyMember(nameof(ParentDoesNotHaveSplits))]
		[List(nameof(Lookups) + "." + nameof(CusUnderbondLookups.ParentsSplitsThatAreNotLocked))]
		public ZString SplitReferenceToWhichThisRemovalPertains
		{
			get { return C4_PackageType; }
			set
			{
				C4_PackageType = value;
				if (!value.IsEmpty && WholeAwb != null && WholeAwb.HasSplits && WholeAwb.Splits[value] != null)
				{
					this.NoPackagesExpected = WholeAwb.Splits[value].NumberOfPiecesExpected;
				}
			}
		}
		public ZPropertyInfo SplitReferenceToWhichThisRemovalPertainsInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SplitReferenceToWhichThisRemovalPertains), x => C4_PackageTypeInfo); }
		}

		CusHAWB hawb;

		internal // this goes away when property {ICcsukCusAwb Awb} is added 
		CusHAWB Hawb
		{
			get
			{
				return hawb ?? (hawb = Factory.Load<CusHAWB>(C4_ParentID));
			}
		}

		public ICcsukCusAwb Awb { get; private set; }

		public ICcsukCusAwb WholeAwb
		{
			get
			{
				ICcsukCusAwb result = null;
				if (Hawb != null)
				{
					if (Hawb.CS_IsMasterHouse)
					{
						result = Hawb.MAWB; // for basics
					}
					else
					{
						result = hawb;
					}
				}
				Awb = result;
				if (!SplitReferenceToWhichThisRemovalPertains.IsEmpty && result != null && result.HasSplits && result.Splits[SplitReferenceToWhichThisRemovalPertains] != null)
				{
					Awb = result.Splits[SplitReferenceToWhichThisRemovalPertains];
				}
				return result;
			}
		}

		protected void WriteCorePropertiesForInterpretation(HtmlTableCreator table)
		{
			table.WriteRow("Airport of receipt", AirportOfReceipt);
			table.WriteRow("Current Shed", Shed);
			if (AirportOrCountryOfDestination.Length == 5)
			{
				table.WriteRow("Airport of Destination (& IATA)", string.Format("{0} ({1})", AirportOrCountryOfDestination, AirportOrCountryOfDestination_IATA));
			}
			else
			{
				table.WriteRow("Airport of Destination", AirportOrCountryOfDestination);
			}
			table.WriteRow("NPX", NoPackagesExpected);
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			C4_SendersMessageReference = "";
			LicenseRestrictionIndCore = "N";
		}
#endif
	}
}
