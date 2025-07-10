using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(AsycudaBillCollection))]
sealed class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
{
	[ExpectNoExceptions]
	public void TestSetDefaultsForNewChild_ABL_BolType() => CombineAssertions(() =>
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var consol = Factory.New<ForwardingConsol>();
		consol.JK_AgentType = "CLD";
		header.SetParent(consol);
		var bill = header.Bills.AddNew();
		NUnit.Framework.Assert.That(bill.ABL_BolType, Is.EqualTo("CLD").Using(CustomComparers.TypeComparison), "When Consol type is CLD, bill type should default to CLD");

		consol.JK_AgentType = "STD";
		var bill2 = header.Bills.AddNew();
		NUnit.Framework.Assert.That(bill2.ABL_BolType, Is.Not.EqualTo("CLD").Using(CustomComparers.TypeComparison), "When Consol type is not CLD, bill type should not default to CLD");

		header.SetParent(null);
		var bill3 = header.Bills.AddNew();
		NUnit.Framework.Assert.That(bill3.ABL_BolType, Is.Not.EqualTo("CLD").Using(CustomComparers.TypeComparison), "When Manifest is not created from Consol, bill type should not default to CLD");
	});

	protected override BusinessObjectCollection GetCollectionToTest() => Factory.NewWithValidTestData<AsycudaManifestHeader>().Bills;
}

