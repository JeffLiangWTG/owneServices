using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Registry.Business.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(CusEntryHeader))]
public class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderAbstractTest
{
	public new void TestWorkflowSupportableBusinessObject()
	{
		Assert("We no longer support workflow on CusEntryHeader", true);
	}

	public void TestGetTotalChargeValueFor()
	{
		var dec = Factory.New<JobDeclaration>();
		var header = dec.ActiveEntryHeaders.AddNew();
		var lineOne = header.MergedLines.AddNew();
		lineOne.Fees.AddOrUpdate(FeeTypeList.Codes.A00, 100m);
		var lineTwo = header.MergedLines.AddNew();
		lineTwo.Fees.AddOrUpdate(FeeTypeList.Codes.B00, 200m);

		var entry = header as ICustomsChargeEntry;
		AssertNotNull(entry);
		AssertEquals(100m, entry.GetTotalChargeValueFor(entry.EntryChargeTypeList.OfType<EntryChargeType>().FirstOrDefault(x => x.Code == "DTY"), ""));
		AssertEquals(200m, entry.GetTotalChargeValueFor(entry.EntryChargeTypeList.OfType<EntryChargeType>().FirstOrDefault(x => x.Code == "REG"), ""));
	}

	protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

	protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

	protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
}
