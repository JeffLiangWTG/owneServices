using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Manifest.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class AsycudaManifestHeaderSS : AsycudaManifestHeaderBase
		, Integration.Customs.GB.GBICS.IAsycudaManifestHeader
	{
		public AsycudaManifestHeaderSS(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("1DFD9E09-952C-422D-A354-0965C6CFECD6", Caption = "Itinerary")]
		public ZBool HasAtLeastTwoItineraryRows => Itinerary.Count >= 2;

		public ZPropertyInfo HasAtLeastTwoItineraryRowsInfo => GetZPropertyInfo(nameof(HasAtLeastTwoItineraryRows));

		protected override Type GetBillTypeCore() => typeof(AsycudaBillSS);

		public new AsycudaBillCollectionSS Bills => (AsycudaBillCollectionSS)base.Bills;

		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollectionSS(this);

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderSSLookups(this);

		public new AsycudaManifestHeaderSSLookups Lookups => (AsycudaManifestHeaderSSLookups)base.Lookups;

		protected override string HumanReadableNamePrefixCore => ICSManifestTypes.Descriptions.SAS;

		public new AsycudaManifestHeaderSSValidation Validation => (AsycudaManifestHeaderSSValidation)base.Validation;

		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderSSValidation(this);

		protected override bool SpecificCircumstanceIndicator_ReadOnly => false;

		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set
			{
				base.AMA_TransportMode = value;
				Validation.ValidateAMA_VehicleRegistration();
			}
		}

		public override ZString AMA_RL_NKPortOfLoading
		{
			get => base.AMA_RL_NKPortOfLoading;
			set
			{
				var hasChanged = base.AMA_RL_NKPortOfLoading != value;
				base.AMA_RL_NKPortOfLoading = value;
				if (hasChanged)
				{
					PopulateItinerary();
					if (!value.IsEmpty)
					{
						UpdateOriginOnBills();
					}
				}
			}
		}

		public override ZString AMA_RL_NKPortOfFirstArrival
		{
			get => base.AMA_RL_NKPortOfFirstArrival;
			set
			{
				var hasChanged = base.AMA_RL_NKPortOfFirstArrival != value;
				base.AMA_RL_NKPortOfFirstArrival = value;
				if (hasChanged)
				{
					PopulateItinerary();
				}
			}
		}

		public override ZString AMA_RL_NKPortOfDischarge
		{
			get => base.AMA_RL_NKPortOfDischarge;
			set
			{
				var hasChanged = base.AMA_RL_NKPortOfDischarge != value;
				base.AMA_RL_NKPortOfDischarge = value;
				if (hasChanged)
				{
					PopulateItinerary();
					if (!value.IsEmpty)
					{
						UpdateFinalDestinationOnBills();
					}
				}
			}
		}

		void PopulateItinerary()
		{
			Itinerary.RemoveAndDeleteAll();
			string lastItinerary = string.Empty;
			if (!AMA_RL_NKPortOfLoading.IsEmpty)
			{
				lastItinerary = AddItinerary(AMA_RL_NKPortOfLoading, lastItinerary);
			}
			if (!AMA_RL_NKPortOfFirstArrival.IsEmpty)
			{
				lastItinerary = AddItinerary(AMA_RL_NKPortOfFirstArrival, lastItinerary);
			}
			if (!AMA_RL_NKPortOfDischarge.IsEmpty)
			{
				_ = AddItinerary(AMA_RL_NKPortOfDischarge, lastItinerary);
			}
		}

		void UpdateOriginOnBills()
		{
			foreach (var bill in Bills)
			{
				bill.ABL_RL_NKOrigin = AMA_RL_NKPortOfLoading;
			}
		}

		void UpdateFinalDestinationOnBills()
		{
			foreach (var bill in Bills)
			{
				bill.ABL_RL_NKFinalDestination = AMA_RL_NKPortOfDischarge;
			}
		}

		string AddItinerary(string port, string lastItinerary)
		{
			var data = port.Length > 2 ? port.Substring(0, 2) : port;
			if (data != lastItinerary)
			{
				var entry = Itinerary.AddNew();
				entry.CY_Data = data;
			}
			return data;
		}

		public override ZBool IsRoad => AMA_TransportMode == GBSSTransportTypeList.Codes.RoadFreight || AMA_TransportMode == GBSSTransportTypeList.Codes.RoroAccompanied || AMA_TransportMode == GBSSTransportTypeList.Codes.RoroUnaccompanied;

		public override ZBool IsSea => AMA_TransportMode == GBSSTransportTypeList.Codes.SeaFreight || AMA_TransportMode == GBSSTransportTypeList.Codes.InlandWaterTransport;

		[MaxLength(1)]
		[ResourceStringData("AsycudaManifestHeaderSS.SpecificCircumstanceIndicator", Caption = "Specific Circumstance Indicator")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.SpecificCircumstanceList))]
		[ReadOnlyMember(nameof(SpecificCircumstanceIndicator_ReadOnly))]
		public override ZString SpecificCircumstanceIndicator { get => base.SpecificCircumstanceIndicator; set => base.SpecificCircumstanceIndicator = value; }

		[ResourceStringData("AsycudaManifestHeaderSS.AMA_OA_Carrier", Caption = "Carrier", FullDescription = "Carrier must be entered if the carrier is different to the organization lodging the summary declaration")]
		public override ZGuid AMA_OA_Carrier { get => base.AMA_OA_Carrier; set => base.AMA_OA_Carrier = value; }

		[ResourceStringData("AsycudaManifestHeaderSS.AMA_CustomsOffice", Caption = "Customs office of Lodgement", MediumCaption = "Lodgement Office", ShortCaption = "Lodgement Office")]
		public override ZString AMA_CustomsOffice { get => base.AMA_CustomsOffice; set => base.AMA_CustomsOffice = value; }

		protected override CustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new AsycudaManifestHeaderCustomsOfficeRequirementHelper(this);

		protected override CusCodeDataValidation GetIcsOfficeCodeValidationCore(IcsOfficeCode code) => new IcsOfficeCodeValidation(code);

		protected override void SetDefaultCustomsOffice(ZString office)
		{
		}
	}
}
