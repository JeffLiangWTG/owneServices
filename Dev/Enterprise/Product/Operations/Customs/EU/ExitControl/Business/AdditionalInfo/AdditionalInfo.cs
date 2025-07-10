using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class AdditionalInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
		, IUcc6ValueProvider
	{
		public AdditionalInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("A50E0D09-FFA1-4CCB-A6B6-396F087DCFAA", Caption = "Kind")]
		[ReadOnly(true)]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set => base.CSI_SubType = value;
		}

		[ResourceStringData("F71795BA-5289-4EAD-B731-64E766540E01", Caption = "Full Type")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		[ResourceStringData("59624856-08F5-476A-9E68-2E638064E6B3", Caption = "Reference")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		[ResourceStringData("BB4F6A01-EA48-462A-9FB8-45CA7549495B", ShortCaption = "Seq. No.", Caption = "Sequence Number", MediumCaption = "Seq Number", FullDescription = "Additional Document Sequence Number")]
		public override ZInt CSI_ItemNumber
		{
			get => base.CSI_ItemNumber;
			set => base.CSI_ItemNumber = value;
		}

		[ResourceStringData("0B2E9700-EA05-4AE6-BEAC-8C9411D46EC5", ShortCaption = "Status", Caption = "Status", MediumCaption = "Status", FullDescription = "Additional Document Status")]
		public override ZString CSI_Status
		{
			get => base.CSI_Status;
			set => base.CSI_Status = value;
		}

		public CusExitReportItem CusExitReportItem => (CusExitReportItem)Parent;

		protected IUcc6ValueProvider Ucc6ValueProvider => (IUcc6ValueProvider)Parent;

		public new AdditionalInfoLookups Lookups => (AdditionalInfoLookups)base.Lookups;

		public new AdditionalInfoValidation Validation => (AdditionalInfoValidation)base.Validation;

		public ZBool StatusIsMissing => CSI_Status == DiscrepanciesStatusCodeList.Codes.Missing;

		public ZBool StatusIsDifferencesToDeclared => CSI_Status == DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

		public bool IsUCC6 => IsUCC6Core;
		protected virtual bool IsUCC6Core => Ucc6ValueProvider?.IsUCC6 ?? false;

		protected override Customs.Business.CusSupportingInfoLookups GetNewLookups() => IsUCC6
			? new AdditionalInfoUcc6Lookups(this)
			: new AdditionalInfoLookups(this);

		protected override Customs.Business.CusSupportingInfoValidation GetNewValidation() => new AdditionalInfoValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
		}
	}
}
