using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class TransportEquipmentWrapperTest : TestCaseWithFactory
{
	public void TestConstructorWithContainerNumberAndEntryLinesAndSeals()
	{
		AssertExceptionThrown<ArgumentException>("When containerNumber parameter is empty", () => new TransportEquipmentWrapper(containerNumber: "", entryLineNumbers: ImmutableHashSet.Create<int>(), seals: ImmutableList.Create<string>()));
		AssertExceptionThrown<ArgumentException>("When entryLineNumbers parameter is null", () => new TransportEquipmentWrapper(containerNumber: "CNT", entryLineNumbers: null, seals: ImmutableList.Create<string>()));
		AssertExceptionThrown<ArgumentException>("When seals parameter is null", () => new TransportEquipmentWrapper(containerNumber: "CNT", entryLineNumbers: ImmutableHashSet.Create<int>(), seals: null));
	}

	public void TestConstructorWithAndEntryLinesAndSeals()
	{
		AssertExceptionThrown<ArgumentException>("When entryLineNumbers parameter is null", () => new TransportEquipmentWrapper(entryLineNumbers: null, seals: ImmutableList.Create<string>()));
		AssertExceptionThrown<ArgumentException>("When seals parameter is null", () => new TransportEquipmentWrapper(entryLineNumbers: ImmutableHashSet.Create<int>(), seals: null));
	}

	public void TestPropertiesWithContainerNumberAndEntryLinesAndSeals()
	{
		var entryLineNumbers = new HashSet<int>() { 1, 2, 3 };
		var seals = new HashSet<string>() { "S1", "S2", "S3" };
		var transportEquipmentWrapper = (ITransportEquipment)new TransportEquipmentWrapper("CNT1234", entryLineNumbers.ToImmutableHashSet(), seals.ToImmutableList());

		AssertTransportEquipmentWrapper(transportEquipmentWrapper
			, "CNT1234"
			, new int[] { 1, 2, 3 }
			, 3
			, new string[] { "S1", "S2", "S3" });
	}

	public void TestPropertiesWithEntryLinesAndSeals()
	{
		var entryLineNumbers = new HashSet<int>() { 1, 2, 3 };
		var seals = new HashSet<string>() { "S1", "S2", "S3" };
		var transportEquipmentWrapper = (ITransportEquipment)new TransportEquipmentWrapper(entryLineNumbers.ToImmutableHashSet(), seals.ToImmutableList());

		AssertTransportEquipmentWrapper(transportEquipmentWrapper
		, ""
		, new int[] { 1, 2, 3 }
		, 3
		, new string[] { "S1", "S2", "S3" });
	}

	void AssertTransportEquipmentWrapper(ITransportEquipment transportEquipmentWrapper
		, string expectedContainerID
		, int[] expectedLinkedGoodsItemNumbers
		, int expectedNumberOfSeals
		, string[] expectedSeals)
	{
		CombineAssertions(() =>
		{
			AssertEquals("ContainerID", expectedContainerID, transportEquipmentWrapper.ContainerID);
			AssertArrayEqualsByElements("LinkedGoodsItemNumbers", expectedLinkedGoodsItemNumbers, transportEquipmentWrapper.LinkedGoodsItemNumbers.ToArray());
			AssertEquals("NumberOfSeals", expectedNumberOfSeals, transportEquipmentWrapper.NumberOfSeals);
			AssertArrayEqualsByElements("Seals", expectedSeals, transportEquipmentWrapper.Seals.ToArray());
		});
	}
}
