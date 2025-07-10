using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusEntryLineFee : EU.Business.Declaration.CusEntryLineFee, Integration.Customs.NL.ICusEntryLineFee
{
	public CusEntryLineFee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusEntryLineFeeValidation Validation => (CusEntryLineFeeValidation)base.Validation;

	protected override Customs.Business.CusEntryLineFeeValidation GetNewValidation() => new CusEntryLineFeeValidation(this);

	public new CusEntryLine EntryLine => (CusEntryLine)base.EntryLine;

	protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups() => new CusEntryLineFeeLookups(this);

	public new CusEntryLineFeeLookups Lookups => (CusEntryLineFeeLookups)base.Lookups;

	[ResourceStringData("0D7267D2-D854-44A1-92BA-3EEC07651C65", Caption = "[UCC 4/3] Type")]
	public override ZString CF_ChargeType { get => base.CF_ChargeType; set => base.CF_ChargeType = value; }

	[ResourceStringData("407FA895-5234-43FE-9918-40D413EDD51B", Caption = "[UCC 4/8] Method of payment")]
	public override ZString CF_MethodOfPayment { get => base.CF_MethodOfPayment; set => base.CF_MethodOfPayment = value; }

	[ResourceStringData("57DF7BD5-02F8-4D56-B781-105DA0E8E99C", Caption = "[UCC 4/4] Total amount")]
	public override ZDecimal CF_ChargeAmount { get => base.CF_ChargeAmount; set => base.CF_ChargeAmount = value; }

	[ResourceStringData("998D0D90-52D7-4C4F-A920-63D5F8D1FC41", Caption = "Tax Rate")]
	public override ZDecimal CF_Rate { get => base.CF_Rate; set => base.CF_Rate = value; }

	[ResourceStringData("FE8E48DC-FAFF-439E-AD98-7844AFF42811", Caption = "Method of Calculation")]
	public override ZString CF_MethodOfCalculation { get => base.CF_MethodOfCalculation; set => base.CF_MethodOfCalculation = value; }

	[ResourceStringData("A06EC59D-A527-4C59-AB96-ECE0364509E2", Caption = "Base Amount")]
	public override ZDecimal CF_BaseValue { get => base.CF_BaseValue; set => base.CF_BaseValue = value; }

	[ResourceStringData("31CE2F62-323F-4D6D-B2D1-C2A87405DDCD", Caption = "Action")]
	public override ZString CF_RateOverrideReasonCode { get => base.CF_RateOverrideReasonCode; set => base.CF_RateOverrideReasonCode = value; }
}
