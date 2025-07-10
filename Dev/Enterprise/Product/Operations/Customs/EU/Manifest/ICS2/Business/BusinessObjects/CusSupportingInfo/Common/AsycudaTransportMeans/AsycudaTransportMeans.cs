using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaTransportMeans : CusTransportMeans
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification would hide the desired object inheritence")]
		public new class Schema : CusTransportMeans.Schema
		{
			public new const int TPM_ReferenceNumberMaxLength = 15;
		}

		public AsycudaTransportMeans(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusTransportMeansLookups GetNewLookups() => new AsycudaTransportMeansLookups(this);

		public new AsycudaTransportMeansLookups Lookups => (AsycudaTransportMeansLookups)base.Lookups;

		protected override CusTransportMeansValidation GetNewValidation() => new AsycudaTransportMeansValidation(this);

		public new AsycudaTransportMeansValidation Validation => (AsycudaTransportMeansValidation)base.Validation;

		[ResourceStringData("08C5ED8E-1A59-40F6-9E9C-32935E08F87D", Caption = "Vehicle Registration")]
		public override ZString TPM_IdentificationNumber { get => base.TPM_IdentificationNumber; set => base.TPM_IdentificationNumber = value; }

		[MaxLength(Schema.TPM_ReferenceNumberMaxLength)]
		[ResourceStringData("D201DC98-A067-4383-A681-6F891C191A66", Caption = "Trailer")]
		public override ZString TPM_ReferenceNumber { get => base.TPM_ReferenceNumber; set => base.TPM_ReferenceNumber = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaTransportMeansLookups.TypesTransportOfIdentification))]
		[ResourceStringData("765DD0BD-FBEA-4983-8DE6-069D1DE33EDD", Caption = "Type of Identification")]
		public override ZString TPM_TypeOfIdentification { get => base.TPM_TypeOfIdentification; set => base.TPM_TypeOfIdentification = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaTransportMeansLookups.MeansOfTransportTypeList))]
		[ResourceStringData("900439DC-496F-48B7-B339-2AE3A0481B82", Caption = "Type of means of Transport")]
		public override ZString TPM_TypeOfTransportMeans { get => base.TPM_TypeOfTransportMeans; set => base.TPM_TypeOfTransportMeans = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaTransportMeansLookups.CountryList))]
		[ResourceStringData("6210E2CA-E500-41BC-B806-099F9B453BF9", Caption = "Nationality")]
		public override ZString TPM_RN_NKTransportNationality { get => base.TPM_RN_NKTransportNationality; set => base.TPM_RN_NKTransportNationality = value; }

		T LoadParent<T>(string prefix) where T : class => TPM_ParentTableCode == prefix ? Factory.Load<T>(TPM_ParentID) : null;

		public AsycudaPack Pack => LoadParent<AsycudaPack>(AsycudaPackSchema.Constants.Prefix);

		public AsycudaBill Bill => LoadParent<AsycudaBill>(AsycudaBillSchema.Constants.Prefix);
	}
}
