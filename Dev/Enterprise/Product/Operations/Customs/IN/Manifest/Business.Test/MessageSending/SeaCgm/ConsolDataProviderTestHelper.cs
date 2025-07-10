using System;
using CargoWise.Types;
using Enterprise.Customs.IN.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.SeaCgm.Testing;

sealed class ConsolDataProviderTestHelper : Assertion
{
	public void TestCarnNumber(Func<string> getCarnNumber)
	{
		var registryItem = INCustomsDataRegistry.Instance.INConsolAgentRegistrationNumber;
		CombineAssertions(() =>
		{
			AssertEquals("When CARN in Registry is empty", string.Empty, getCarnNumber());

			const string expectedValue = "ABC";
			using (registryItem.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, expectedValue))
			{
				AssertEquals("When CARN in Registry is set", expectedValue, getCarnNumber());
			}
		});
	}

	public void TestCustomHouseCode(CGMAsycudaManifestHeader header, Func<string> getCustomHouseCode)
	{
		CombineAssertions(() =>
		{
			AssertEquals("When AMA_CustomsOffice is empty", string.Empty, getCustomHouseCode());

			header.AMA_CustomsOffice = "XYZ";
			AssertEquals("When AMA_CustomsOffice is set", "XYZ", getCustomHouseCode());
		});
	}
	public void TestIgmDate(CGMAsycudaManifestHeader header, Func<DateTime?> getIgmDate)
	{
		CombineAssertions(() =>
		{
			AssertNull("When header IgmDate is empty", getIgmDate());

			header.ImportGeneralManifestDate = new ZDate(2024, 4, 8);
			AssertEquals("When header IgmDate is set", new DateTime(2024, 4, 8), getIgmDate());
		});
	}

	public void TestIgmNumber(CGMAsycudaManifestHeader header, Func<string> getIgmNumber)
	{
		CombineAssertions(() =>
		{
			AssertEquals("When header IgmNumber is empty", string.Empty, getIgmNumber());

			header.ImportGeneralManifestNumber = "XYZ";
			AssertEquals("When header IgmNumber is set", "XYZ", getIgmNumber());
		});
	}
	public void TestImoCodeOfVessel(CGMAsycudaManifestHeader header, Func<string> getImoCodeOfVessel)
	{
		CombineAssertions(() =>
		{
			AssertEquals("When header ImoCodeOfVessel is empty", string.Empty, getImoCodeOfVessel());

			header.AMA_LloydsNumber = "XYZ";
			AssertEquals("When header ImoCodeOfVessel is set", "XYZ", getImoCodeOfVessel());
		});
	}

	public void TestLineNumber(CGMAsycudaManifestHeader header, Func<int?> getLineNumber)
	{
		CombineAssertions(() =>
		{
			AssertEquals($"Default MasterBill ABL_SequenceNumber", 0, getLineNumber());

			header.MasterBill.ABL_SequenceNumber = 1;
			AssertEquals($"MasterBill ABL_SequenceNumber is set", 1, getLineNumber());
		});
	}

	public void TestSubLineNumber(CGMAsycudaBill bill, Func<int?> getSubLineNumber)
	{
		CombineAssertions(() =>
		{
			AssertEquals("for first bill", 1, getSubLineNumber());

			var header = bill.Header;
			header.Bills.AddNew();
			header.Bills.Remove(bill);
			header.Bills.Add(bill);
			AssertEquals("for second bill", 2, getSubLineNumber());
		});
	}

	public void TestVesselCode(CGMAsycudaManifestHeader header, Func<string> getVesselCode)
	{
		CombineAssertions(() =>
		{
			AssertEquals("When header VesselCode is empty", string.Empty, getVesselCode());

			header.AMA_RadioCallSign = "XYZ";
			AssertEquals("When header VesselCode is set", "XYZ", getVesselCode());
		});
	}

	public void TestVoyageNumber(CGMAsycudaManifestHeader header, Func<string> getVoyageNumber)
	{
		CombineAssertions(() =>
		{
			AssertEquals("When header VoyageNumber is empty", string.Empty, getVoyageNumber());

			header.AMA_Voyage = "XYZ";
			AssertEquals("When header VoyageNumber is set", "XYZ", getVoyageNumber());
		});
	}
}
