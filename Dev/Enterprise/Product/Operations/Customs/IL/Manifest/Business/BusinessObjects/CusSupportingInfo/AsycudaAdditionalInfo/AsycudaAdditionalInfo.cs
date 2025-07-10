using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaAdditionalInfo : AsycudaBaseAdditionalInfo
	{
		public AsycudaAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.AsycudaAdditionalInfo.StatementType", Caption = "Statement Type")]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaAdditionalInfoLookups.ReferenceNumberList))]
		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.AsycudaAdditionalInfo.StatementCode", Caption = "Statement Code")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaAdditionalInfoLookups.DescriptionList))]
		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.AsycudaAdditionalInfo.Content", Caption = "Content")]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

		public ZString ReferenceNumberFieldType
		{
			get
			{
				switch (CSI_Code)
				{
					case Constants.AsycudaAdditionalInfoCodes.ExporterTypeID:
					case Constants.AsycudaAdditionalInfoCodes.NightStop:
					case Constants.AsycudaAdditionalInfoCodes.IsCooling:
					case Constants.AsycudaAdditionalInfoCodes.MultipleDeals:
					case Constants.AsycudaAdditionalInfoCodes.CargoType:
					case Constants.AsycudaAdditionalInfoCodes.ActionCode:
						return nameof(FieldType.TextDropEdit);
					case Constants.AsycudaAdditionalInfoCodes.UNLOCO:
						return nameof(FieldType.TextCodeFindBox);
					default:
						return nameof(FieldType.Text);
				}
			}
		}

		public ZString DescriptionFieldType
		{
			get
			{
				switch (CSI_Code)
				{
					case Constants.AsycudaAdditionalInfoCodes.IsDirectDelivery:
						return nameof(FieldType.TextDropEdit);
					default:
						return nameof(FieldType.Text);
				}
			}
		}

		public new AsycudaAdditionalInfoLookups Lookups => (AsycudaAdditionalInfoLookups)base.Lookups;

		public new AsycudaAdditionalInfoValidation Validation => (AsycudaAdditionalInfoValidation)base.Validation;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		}

		protected override CusSupportingInfoLookups GetNewLookups() => new AsycudaAdditionalInfoLookups(this);

		protected override CusSupportingInfoValidation GetNewValidation() => new AsycudaAdditionalInfoValidation(this);
	}
}
