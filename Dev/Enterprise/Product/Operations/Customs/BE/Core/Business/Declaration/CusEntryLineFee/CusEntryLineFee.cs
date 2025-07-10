using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.BE.Business.Declaration;

public class CusEntryLineFee : EU.Business.Declaration.CusEntryLineFee, Integration.Customs.BE.ICusEntryLineFee
{
	public CusEntryLineFee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusEntryLineFeeValidation Validation => (CusEntryLineFeeValidation)base.Validation;

	public new CusEntryLineFeeLookups Lookups => (CusEntryLineFeeLookups)base.Lookups;

	protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups() => new CusEntryLineFeeLookups(this);

	protected override Customs.Business.CusEntryLineFeeValidation GetNewValidation() => new CusEntryLineFeeValidation(this);

	[ResourceStringData("F44BD3B3-28CF-4970-A394-42947F1CD728", Caption = "[UCC 4/3] Type")]
	public override ZString CF_ChargeType { get => base.CF_ChargeType; set => base.CF_ChargeType = value; }

	[ResourceStringData("16BEAFC9-053C-4C55-85A7-7728FD8ADD79", Caption = "[UCC 4/8] Method of payment")]
	public override ZString CF_MethodOfPayment { get => base.CF_MethodOfPayment; set => base.CF_MethodOfPayment = value; }

	[ResourceStringData("2A27DC68-4989-49BE-98A6-5DA331D5B5BF", Caption = "[UCC 4/4] Total amount")]
	public override ZDecimal CF_ChargeAmount { get => base.CF_ChargeAmount; set => base.CF_ChargeAmount = value; }

	[ResourceStringData("4C816E91-24C3-4C07-8B75-024AABBA7AA9", Caption = "Tax Rate")]
	public override ZDecimal CF_Rate { get => base.CF_Rate; set => base.CF_Rate = value; }

	[ResourceStringData("50389015-0225-43B9-848F-CF8C447D01ED", Caption = "Method of Calculation")]
	public override ZString CF_MethodOfCalculation { get => base.CF_MethodOfCalculation; set => base.CF_MethodOfCalculation = value; }

	[ResourceStringData("3186BEF5-82B9-443D-9FBF-AD9F3214C701", Caption = "Base Amount")]
	public override ZDecimal CF_BaseValue { get => base.CF_BaseValue; set => base.CF_BaseValue = value; }

	[ResourceStringData("E31D406A-916D-4475-90B7-6B7690112E10", Caption = "Action")]
	public override ZString CF_RateOverrideReasonCode => base.CF_RateOverrideReasonCode;
}
