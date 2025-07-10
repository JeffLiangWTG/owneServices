using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.AE.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(AsycudaBillLookups))]
sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
{
	[ExpectNoExceptions]
	public void TestAECargoTypeList()
	{
		var bill = Factory.NewWithValidTestData<AsycudaBill>();
		var cachedList = Factory.GetCachedValue<AECargoTypeList>();
		var lookedUpList = bill.Lookups.AECargoTypeList;
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(lookedUpList, Is.SameAs(cachedList), "AECargoTypeList is cached");
			NUnit.Framework.Assert.That(lookedUpList.CodesAsString, Is.EqualTo("1, 2, 3, 4, 9, 13, 19, 20, 21"), "AECargoTypeList values");
		});
	}

	[ExpectNoExceptions]
	public void TestServiceRequirementCodeList()
	{
		var helper = new UAEUniversalReferenceTestHelper(Factory);
		helper.SetupServiceRequirements(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest);

		var bill = Factory.New<AsycudaBill>();
		var cachedList = bill.Lookups.ServiceRequirementCodeList;
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(cachedList.CodesAsString, Is.EqualTo("ABC"), "ServiceRequirementCodeList values");
			NUnit.Framework.Assert.That(bill.Lookups.ServiceRequirementCodeList, Is.SameAs(cachedList), "ServiceRequirementCodeList is cached");
		});
	}

	[ExpectNoExceptions]
	public void TestCustomsStatusList()
	{
		var helper = new UAEUniversalReferenceTestHelper(Factory);
		helper.SetupCustomsStatuses(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest);

		CombineAssertions(() =>
		{
			AssertLookupsList("", Array.Empty<string>());
			AssertLookupsList(AEConstants.Messaging.MessageTypes.CONTRL, new[] { "ACK", "ERR" });
			AssertLookupsList(AEConstants.Messaging.MessageTypes.CUSRES, new[] { "ABC" });
		});

		void AssertLookupsList(string messageStatus, string[] expectedCodes)
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_MessageStatus = messageStatus;
			var cachedList = bill.Lookups.CustomsStatusList;
			NUnit.Framework.Assert.That(cachedList.GetAllCodes(), Is.EquivalentTo(expectedCodes), $"Message status ({messageStatus})");
			NUnit.Framework.Assert.That(bill.Lookups.CustomsStatusList, Is.SameAs(cachedList), $"{messageStatus} List cached");
		}
	}

	[ExpectNoExceptions]
	public void TestCustomsOriginPortList()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		NUnit.Framework.Assert.That(header.MasterBill.Lookups.CustomsOriginPortList, Is.TypeOf<RefUNLOCOCollection>());
	}

	[ExpectNoExceptions]
	public void TestPackageTypeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType("PKG", "PKG");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedArabEmirates, "PKG", "T", "Bag, super bulk", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedArabEmirates);
		Factory.Save();

		var bill = Factory.New<AsycudaBill>();
		var cachedList = bill.Lookups.PackageTypeList;
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(cachedList.GetAllCodes(), Is.EquivalentTo(new[] { "T" }));
			NUnit.Framework.Assert.That(bill.Lookups.PackageTypeList, Is.SameAs(cachedList), "List cached");
		});
	}

	[ExpectNoExceptions]
	public void TestNegotiableList()
	{
		var bill = Factory.NewWithValidTestData<AsycudaBill>();
		var cachedList = Factory.GetCachedValue<NegotiableList>();
		var lookedUpList = bill.Lookups.NegotiableList;
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(lookedUpList, Is.SameAs(cachedList), "NegotiableList is cached");
			NUnit.Framework.Assert.That(lookedUpList.GetAllCodes(), Is.EquivalentTo(new[] { "Y", "N" }));
		});
	}

	[ExpectNoExceptions]
	public void TestSplitBills() => CombineAssertions(() =>
	{
		var bill = Factory.New<AsycudaBill>();
		NUnit.Framework.Assert.That(bill.Lookups.SplitBills, Is.TypeOf<AsycudaBillFindBoxCollection>());
		var collection = (AsycudaBillFindBoxCollection)bill.Lookups.SplitBills;
		NUnit.Framework.Assert.That(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Country/Region:Property"), Is.EqualTo((ZBool)true));
		NUnit.Framework.Assert.That(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("House Bill Number:Property"), Is.EqualTo((ZBool)true));
	});
}
