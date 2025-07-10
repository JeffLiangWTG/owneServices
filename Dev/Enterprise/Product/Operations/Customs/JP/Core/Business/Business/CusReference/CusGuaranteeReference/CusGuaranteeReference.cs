using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Business
{
	public class CusGuaranteeReference : CusReference
	{
		public CusGuaranteeReference(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusReference.Schema
		{
			public new const int CFR_ReferenceMaxLength = 9;
		}

		[MaxLength(Schema.CFR_ReferenceMaxLength)]
		[ResourceStringData("788FEA69-7DED-45B5-B578-62D51DB31304", Caption = "Number")]
		public override ZString CFR_Reference { get => base.CFR_Reference; set => base.CFR_Reference = value; }

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CFR_Type = CusReferenceTypeList.Codes.GuaranteeReference;
			CFR_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
		}

		public new CusGuaranteeReferenceValidation Validation => (CusGuaranteeReferenceValidation)base.Validation;

		protected override CusReferenceValidation GetNewValidation() => new CusGuaranteeReferenceValidation(this);
	}
}
