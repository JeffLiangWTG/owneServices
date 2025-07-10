using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class GuaranteeForEntryInstruction : EU.Business.Declaration.GuaranteeForEntryInstruction
	{
		public GuaranteeForEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new GuaranteeForEntryInstructionLookups Lookups => (GuaranteeForEntryInstructionLookups)base.Lookups;

		public new GuaranteeForEntryInstructionValidation Validation => (GuaranteeForEntryInstructionValidation)base.Validation;

		protected override CusBondDetailLookups GetNewLookups() => new GuaranteeForEntryInstructionLookups(this);

		protected override CusBondDetailValidation GetNewValidation() => new GuaranteeForEntryInstructionValidation(this);

		[List(nameof(Lookups) + "." + nameof(GuaranteeForEntryInstructionLookups.ReferenceNumbers))]
		[ResourceStringData("27A3BD9F-0C9D-4483-94ED-E6EFE727A44B", Caption = "Guarantee")]
		public override ZGuid PW_CPH_Guarantee
		{
			get => base.PW_CPH_Guarantee;
			set
			{
				var oldValue = PW_CPH_Guarantee;
				base.PW_CPH_Guarantee = value;
				if (!IsCopying && oldValue != PW_CPH_Guarantee)
				{
					if (CusGuarantee is CusGuaranteeHeader guarantee)
					{
						PW_BondType = guarantee.CPH_Type.Left(1);
						PW_BondNumber = guarantee.CPH_Number;
						PW_Password = guarantee.MainAccessCode.Left(4);
						PW_RX_NKCurrency = guarantee.CPH_UnitOfMeasure.Left(3);
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(GuaranteeForEntryInstructionLookups.IEBondTypeList))]
		[ReadOnlyMember(nameof(PW_FieldsReadOnly))]
		[ResourceStringData("D9F64555-61F4-49BA-BAE2-7825BF0818E0", Caption = "Type")]
		public override ZString PW_BondType { get => base.PW_BondType; set => base.PW_BondType = value; }

		[ReadOnlyMember(nameof(PW_FieldsReadOnly))]
		[ResourceStringData("CC33D0D2-9516-4D77-983D-180FDB14C1C2", Caption = "Guarantee Number")]
		public override ZString PW_BondNumber { get => base.PW_BondNumber; set => base.PW_BondNumber = value; }

		[MaxLength(35)]
		[ResourceStringData("4613B014-2AC4-4DCC-A47C-E1BDEE38CD28", Caption = "Reference")]
		public override ZString PW_GuaranteeDescription { get => base.PW_GuaranteeDescription; set => base.PW_GuaranteeDescription = value; }

		[ReadOnlyMember(nameof(PW_FieldsReadOnly))]
		[ResourceStringData("307AE8C2-33CE-4AE5-917E-DC4A008DE275", Caption = "Access Code")]
		public override ZString PW_Password { get => base.PW_Password; set => base.PW_Password = value; }

		[ResourceStringData("12783A7A-FB2E-4CB1-9DDD-E0A0FDCC538F", Caption = "Amount")]
		public override ZDecimal PW_BondAmount { get => base.PW_BondAmount; set => base.PW_BondAmount = value; }

		[ReadOnlyMember(nameof(PW_FieldsReadOnly))]
		[ResourceStringData("50AB73DE-3870-424F-8680-7E6EFDC126D6", Caption = "Currency")]
		public override ZString PW_RX_NKCurrency { get => base.PW_RX_NKCurrency; set => base.PW_RX_NKCurrency = value; }

		[ResourceStringData("EA0ACDDC-AB03-4769-A97C-ABE8C24A7F38", Caption = "Customs Office")]
		public override ZString PW_BondFiledPort { get => base.PW_BondFiledPort; set => base.PW_BondFiledPort = value; }

		[List(nameof(Lookups) + "." + nameof(GuaranteeForEntryInstructionLookups.EuropeanUnionCountryList))]
		[ResourceStringData("2D0D5E25-F663-40C9-8C14-B5854AF36F80", Caption = "CC Qualifier")]
		public override ZString PW_RN_NKCountryOfIssue { get => base.PW_RN_NKCountryOfIssue; set => base.PW_RN_NKCountryOfIssue = value; }

		protected bool PW_FieldsReadOnly => !PW_CPH_Guarantee.IsEmpty;
	}
}
