using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Business
{
	public class ComprehensiveValuation : CusReference
	{
		public ComprehensiveValuation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static class ComprehensiveValuationSchema
		{
			public const string Code = "CVN";
			public const string Type = "CVS";
		}

		[ResourceStringData("JPCusReference|CFR_Reference", Caption = "Acceptance Number", ShortCaption = "Number")]
		[MaxLength(9)]
		public override ZString CFR_Reference
		{
			get => base.CFR_Reference;
			set => base.CFR_Reference = value;
		}

		public new JobComInvoiceHeader Parent => base.Parent as JobComInvoiceHeader;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CFR_Type = CusReferenceTypeList.Codes.ComprehensiveValuations;
			CFR_Code = ComprehensiveValuationSchema.Code;
			CFR_ParentTableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
		}

		public new ComprehensiveValuationValidation Validation => (ComprehensiveValuationValidation)base.Validation;

		protected override CusReferenceValidation GetNewValidation() => new ComprehensiveValuationValidation(this);
	}
}
