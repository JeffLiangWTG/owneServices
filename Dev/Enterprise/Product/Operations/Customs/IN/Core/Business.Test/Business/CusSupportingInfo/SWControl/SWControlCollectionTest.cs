using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SWControlCollection))]
sealed class SWControlCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<SWControl>
{
	public void TestMaxCountValidation()
	{
		var collection = Factory.New<CusEntryInstruction>().SWControls;

		for (var i = 0; i < 9999; i++)
		{
			collection.AddNew();
		}

		CombineAssertions(() =>
		{
			AssertEquals("Maximum allowed", false, collection.HasErrors());
			AssertNoRowMessageError(collection.Last(), "The maximum number of 9999 Single Window Control has been exceeded.");

			var control = collection.AddNew();
			AssertHasRowMessageError(control, "The maximum number of 9999 Single Window Control has been exceeded.");
		});
	}

	public void TestDefaultCSI_Type()
	{
		var supportingDocument = SWControls.AddNew();
		AssertEquals("CSI_Type", CusSupportingInfoTypeList.Codes.SingleWindowControl, supportingDocument.CSI_Type);
	}

	protected override CusSupportingInfoCollection<SWControl> GetCusSupportingInfoCollection() => Factory.New<CusEntryInstruction>().SWControls;

	SWControlCollection SWControls => swControls ??= Factory.New<CusEntryInstruction>().SWControls;
	SWControlCollection swControls;
}
