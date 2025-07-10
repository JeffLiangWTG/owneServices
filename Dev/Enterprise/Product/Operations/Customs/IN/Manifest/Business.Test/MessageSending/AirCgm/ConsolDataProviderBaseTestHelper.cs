using System;
using CargoWise.Types;
using Enterprise.Customs.IN.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.AirCgm.Testing;

sealed class ConsolDataProviderBaseTestHelper : Assertion
{
	internal void TestConsolAgentId(Func<string> getConsolAgentId)
	{
		var registryItem = INCustomsDataRegistry.Instance.INConsolAgentRegistrationNumber;
		CombineAssertions(() =>
		{
			AssertEquals("When Consol Agent Id in Registry is empty", string.Empty, getConsolAgentId());

			const string expectedValue = "ABC";
			using (registryItem.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, expectedValue))
			{
				AssertEquals("When Consol Agent Id in Registry is set", expectedValue, getConsolAgentId());
			}
		});
	}

	internal void TestCustomsHouseCode(CGMAsycudaManifestHeader header, Func<string> getCustomHouseCode)
	{
		CombineAssertions(() =>
		{
			AssertEquals("When AMA_CustomsOffice is empty", string.Empty, getCustomHouseCode());

			header.AMA_CustomsOffice = "XYZ";
			AssertEquals("When AMA_CustomsOffice is set", "XYZ", getCustomHouseCode());
		});
	}

	internal void TestIgmNo(CGMAsycudaManifestHeader header, Func<string> getIgmNo)
	{
		CombineAssertions(() =>
		{
			AssertEquals("When header IgmNumber is empty", string.Empty, getIgmNo());

			header.ImportGeneralManifestNumber = "XYZ";
			AssertEquals("When header IgmNumber is set", "XYZ", getIgmNo());
		});
	}

	internal void TestIgmDate(CGMAsycudaManifestHeader header, Func<DateTime?> getIgmDate)
	{
		CombineAssertions(() =>
		{
			AssertNull("When header IgmDate is empty", getIgmDate());

			header.ImportGeneralManifestDate = new ZDate(2024, 4, 8);
			AssertEquals("When header IgmDate is set", new DateTime(2024, 4, 8), getIgmDate());
		});
	}

	internal void TestFlightNo(CGMAsycudaManifestHeader header, Func<string> getVoyageNumber)
	{
		CombineAssertions(() =>
		{
			AssertEquals("When header VoyageNumber is empty", string.Empty, getVoyageNumber());

			header.AMA_Voyage = "XYZ";
			AssertEquals("When header VoyageNumber is set", "XYZ", getVoyageNumber());
		});
	}

	internal void TestFlightOriginDate(CGMAsycudaBill masterBill, Func<DateTime?> getFlightOriginDate)
	{
		CombineAssertions(() =>
		{
			AssertNull("When MasterBill Flight Origin Date is empty", getFlightOriginDate());

			masterBill.ABL_E_ARV = new ZDate(2022, 3, 16);
			AssertEquals("When MasterBill Flight Origin Date is set", new DateTime(2022, 3, 16), getFlightOriginDate());
		});
	}

	internal void TestMawbNo(CGMAsycudaBill masterBill, Func<string> getMawbNo)
	{
		CombineAssertions(() =>
		{
			AssertEquals("When MasterBill MawbNumber is empty", string.Empty, getMawbNo());

			masterBill.ABL_BillNumber = "11221";
			AssertEquals("When MasterBill MawbNumber is set", "11221", getMawbNo());
		});
	}

	internal void TestMawbDate(CGMAsycudaBill masterBill, Func<DateTime?> getMawbDate)
	{
		CombineAssertions(() =>
		{
			AssertNull("When MasterBill MawbDate is empty", getMawbDate());

			masterBill.ABL_BillIssueDate = new ZDate(2022, 3, 16);
			AssertEquals("When MasterBill MawbDate is set", new DateTime(2022, 3, 16), getMawbDate());
		});
	}
}
