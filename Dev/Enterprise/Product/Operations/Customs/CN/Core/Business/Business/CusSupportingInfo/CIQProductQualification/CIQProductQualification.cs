using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class CIQProductQualification : CusSupportingInfo
	{
		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string DocumentName = "DocumentName";
		}

		public CIQProductQualification(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new CIQProductQualificationValidation(this);
		}

		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return new CIQProductQualificationLookups(this);
		}

		public new CIQProductQualificationLookups Lookups => (CIQProductQualificationLookups)base.Lookups;

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		[List(nameof(Lookups) + "." + nameof(CIQProductQualificationLookups.CodeList))]
		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CIQProductQualification|CSI_Code", Caption = "Type", FullDescription = "Document Type")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		public bool SupportsVIN => CSI_Code == ProductQualificationCodeList.Codes._408 || CSI_Code == ProductQualificationCodeList.Codes._409 || CSI_Code == ProductQualificationCodeList.Codes._603;

		[MaxLength(40)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CIQProductQualification|CSI_ReferenceNumber", Caption = "Document Number")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		[DecimalPlaces(0)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CIQProductQualification|CSI_Quantity", Caption = "Quantity", ShortCaption = "Quantity")]
		public override ZDecimal CSI_Quantity
		{
			get => base.CSI_Quantity;
			set => base.CSI_Quantity = value;
		}

		[List(nameof(Lookups) + "." + nameof(CIQProductQualificationLookups.UnitOfMeasurementList))]
		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CIQProductQualification|CSI_UnitOfQuantity", Caption = "Unit of Quantity", ShortCaption = "Unit")]
		public override ZString CSI_UnitOfQuantity
		{
			get => base.CSI_UnitOfQuantity;
			set => base.CSI_UnitOfQuantity = value;
		}

		public ZString CSI_UnitOfQuantityDescription => Lookups.UnitOfMeasurementList.GetDescriptionFromCode(CSI_UnitOfQuantity);

		[ResourceStringData("Enterprise.Customs.CN.Business.CIQProductQualification|CSI_LineNo", Caption = "Line Number")]
		public override ZInt CSI_LineNo
		{
			get => base.CSI_LineNo;
			set => base.CSI_LineNo = value;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = Constants.CusSupportingInfoTypes.CIQProductQualification;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CIQProductQualification|DocumentName", Caption = "Document Name")]
		public ZString DocumentName => ((CodeDescriptionPairList)Lookups.CodeList).GetDescriptionFromCode(CSI_Code);

		public ZPropertyInfo DocumentNameInfo => GetZPropertyInfo(Schema.DocumentName);

		public ZString MergeKey => ZString.Join("|", new[] { CSI_Code, CSI_ReferenceNumber, new ZString(CSI_LineNo.ToString()), CSI_UnitOfQuantity });

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CIQProductQualificationFetchStrategy(this);
		}
	}
}
