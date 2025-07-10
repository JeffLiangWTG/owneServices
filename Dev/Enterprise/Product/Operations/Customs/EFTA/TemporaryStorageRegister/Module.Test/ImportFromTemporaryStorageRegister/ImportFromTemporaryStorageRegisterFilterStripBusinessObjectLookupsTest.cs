using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.Testing;

[TestedType(typeof(ImportFromTemporaryStorageRegisterFilterStripBusinessObjectLookups))]
sealed class ImportFromTemporaryStorageRegisterFilterStripBusinessObjectLookupsTest : TestCaseWithFactory
{
	public void TestOwnerReferenceTypeList_HasExpectedTypes()
	{
		var ownerReferenceTypes = lookups.OwnerReferenceTypeList;
		CombineAssertions(() =>
		{
			AssertEquals("Owner Reference Types are not as expected", "AWB, ULD, ZZZ", ownerReferenceTypes.CodesAsString);
			AssertSame("Owner Reference Types are not cached", ownerReferenceTypes, lookups.OwnerReferenceTypeList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		lookups = new (new ());
	}
	ImportFromTemporaryStorageRegisterFilterStripBusinessObjectLookups lookups;
}
