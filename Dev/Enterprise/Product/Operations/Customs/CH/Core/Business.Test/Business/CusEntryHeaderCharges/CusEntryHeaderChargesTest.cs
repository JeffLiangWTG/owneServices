using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusEntryHeaderCharges))]
sealed class CusEntryHeaderChargesTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew().Charges.AddNew();

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var entry = declaration.CustomsEntryHeaders.AddNew();
		var charge = entry.Charges.AddNew();
		charge.C1_ChargeAmount = 10m;
		return charge;
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew().Charges.AddNew();

	public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
	{
		return new List<ZString>() { CusEntryHeaderCharges.Schema.C1_Source };
	}

	public void TestChargeTypeDescription() => CombineAssertions(() =>
	{
		var entryHeaderCharge = EntryHeader.Charges.AddNew();
		var chargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;
		entryHeaderCharge.C1_Source = CusEntryHeaderChargesSourceCodeList.Codes.CUS;
		entryHeaderCharge.C1_ChargeType = chargeType;
		AssertEquals(chargeType, entryHeaderCharge.ChargeTypeDescription);

		chargeType = entryHeaderCharge.Lookups.ChargeTypeList.GetAllCodes().FirstOrDefault();
		var description = entryHeaderCharge.Lookups.ChargeTypeList.GetDescriptionFromCode(chargeType);
		entryHeaderCharge.C1_ChargeType = chargeType;
		AssertEquals(description, entryHeaderCharge.ChargeTypeDescription);
	});

	CusEntryHeader EntryHeader => entryHeader ??= ImportJobDeclaration.CustomsEntryHeaders.AddNew();
	CusEntryHeader entryHeader;

	JobDeclaration ImportJobDeclaration => importJobDeclaration ??= CreateNewDeclaration(JobMessageTypeList.Codes.Import);
	JobDeclaration importJobDeclaration;

	JobDeclaration CreateNewDeclaration(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		return declaration;
	}
}
