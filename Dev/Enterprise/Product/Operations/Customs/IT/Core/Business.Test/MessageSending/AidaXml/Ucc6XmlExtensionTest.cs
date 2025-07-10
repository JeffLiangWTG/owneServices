using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Testing;

sealed class Ucc6XmlExtensionTest : TestCaseWithFactory
{
	public void TestRemoveLastCharSafe()
	{
		CombineAssertions(() =>
		{
			AssertEquals(nameof(Ucc6XmlExtension.RemoveLastCharSafe), "", new ZString().RemoveLastCharSafe());
			AssertEquals(nameof(Ucc6XmlExtension.RemoveLastCharSafe), "", new ZString("A").RemoveLastCharSafe());
			AssertEquals(nameof(Ucc6XmlExtension.RemoveLastCharSafe), "ABC", new ZString("ABCD").RemoveLastCharSafe());
		});
	}

	public void TestNullIfZero()
	{
		CombineAssertions("Testing NullIfZero for ZInt", () =>
		{
			AssertNull(nameof(Ucc6XmlExtension.NullIfZero), new ZInt().NullIfZero());
			AssertEquals(nameof(Ucc6XmlExtension.NullIfZero), 3, new ZInt(3).NullIfZero());
			AssertNull(nameof(Ucc6XmlExtension.NullIfZero), new ZInt(0).NullIfZero());
		});

		CombineAssertions("Testing NullIfZero for ZDecimal", () =>
		{
			AssertNull(nameof(Ucc6XmlExtension.NullIfZero), new ZDecimal().NullIfZero());
			AssertEquals(nameof(Ucc6XmlExtension.NullIfZero), 3.33m, new ZDecimal(3.33).NullIfZero());
			AssertNull(nameof(Ucc6XmlExtension.NullIfZero), new ZDecimal(0).NullIfZero());
		});
	}

	public void TestToAdditionalInformationWrapperCollection()
	{
		AssertNull((null as IEnumerable<AdditionalInfo>).ToAdditionalInformationWrapperCollection());

		var emptyAdditionalInfoCollection = System.Array.Empty<AdditionalInfo>();
		var resultOfEmptyAdditionalInfoCollectionConversion = emptyAdditionalInfoCollection.ToAdditionalInformationWrapperCollection();
		AssertEquals("When input is an empty list, a collection with one item is expected", 1, resultOfEmptyAdditionalInfoCollectionConversion.Count);
		AssertType<NoneOfAboveAdditionalInformationWrapper>(resultOfEmptyAdditionalInfoCollectionConversion.First());

		var additionalInfoCollection = new[] { Factory.New<AdditionalInfo>() };
		AssertEquals("Count of return collection", 1, additionalInfoCollection.ToAdditionalInformationWrapperCollection().Count);
		AssertType<AdditionalInformationWrapper>(additionalInfoCollection.ToAdditionalInformationWrapperCollection().First());
	}

	public void TestToTraderOrEmpty()
	{
		CombineAssertions(() =>
		{
			AssertNull("When docAddress is null", (null as JobDocAddress).ToTraderOrEmpty());
			AssertNull("When docAddress is empty", Factory.New<JobDocAddress>().ToTraderOrEmpty());

			var validDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			AssertNotNull("When docAddress is valid", validDocAddress.ToTraderOrEmpty());
			AssertType<TraderWrapper>("When docAddress is valid and not empty", validDocAddress.ToTraderOrEmpty());
		});
	}

	public void TestZBoolToInt()
	{
		CombineAssertions(() =>
		{
			AssertEquals("False.ToInt()", 0, ZBool.False.ToInt());
			AssertEquals("True.ToInt()", 1, ZBool.True.ToInt());
		});
	}
}
