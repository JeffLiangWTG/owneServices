using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class NbSadFieldDescriptionProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Factory is required", () => new NbSadFieldDescriptionProvider(null));
	}

	public void TestGetDescriptionBySequenceNumber()
	{
		var nbProvider = new NbSadFieldDescriptionProvider(Factory);

		CombineAssertions("Assert GetDescriptionBySequenceNumber in many scenarios", () =>
		{
			AssertEquals("Record Type", nbProvider.GetDescriptionBySequenceNumber("7"));
			AssertEquals("Annual Sequence Number", nbProvider.GetDescriptionBySequenceNumber("8"));
			AssertEquals("Item Number", nbProvider.GetDescriptionBySequenceNumber("11"));
			AssertEquals("Register Code", nbProvider.GetDescriptionBySequenceNumber("12"));
			AssertEquals("Register Code", nbProvider.GetDescriptionBySequenceNumber("32"));
			AssertEquals("Register Code", nbProvider.GetDescriptionBySequenceNumber("52"));
			AssertEquals("Item Number", nbProvider.GetDescriptionBySequenceNumber("25"));
			AssertEquals("Item Number", nbProvider.GetDescriptionBySequenceNumber("37"));
			AssertEquals("Supplementary Unit", nbProvider.GetDescriptionBySequenceNumber("91"));
			AssertEquals("Customs Office", nbProvider.GetDescriptionBySequenceNumber("26"));

			AssertEquals("", nbProvider.GetDescriptionBySequenceNumber(""));
			AssertEquals("", nbProvider.GetDescriptionBySequenceNumber(null));
		});
	}

	public void TestFieldStartingIndex()
	{
		var nbProvider = new NbSadFieldDescriptionProvider_ForTest(Factory);
		AssertEquals(12, nbProvider.FieldStartingIndex_Exposed);
	}

	public void TestGetCorrelationsForNotRepeatedFieldsExposed()
	{
		var nbProvider = new NbSadFieldDescriptionProvider_ForTest(Factory);
		AssertArrayEqualsByElements(new CodeDescriptionPair[] {
			new CodeDescriptionPair("7",NbSadFieldDescriptionList.Codes.RecordType),
			new CodeDescriptionPair("8",NbSadFieldDescriptionList.Codes.AnnualSequenceNumber),
			new CodeDescriptionPair("11",NbSadFieldDescriptionList.Codes.ItemNumber5)
		}, nbProvider.GetCorrelationsForNotRepeatedFieldsExposed().ToArray());
	}
}

class NbSadFieldDescriptionProvider_ForTest : NbSadFieldDescriptionProvider
{
	public NbSadFieldDescriptionProvider_ForTest(BusinessObjectFactory factory) : base(factory)
	{
	}

	public int FieldStartingIndex_Exposed => base.FieldStartingIndex;

	public IEnumerable<CodeDescriptionPair> GetCorrelationsForNotRepeatedFieldsExposed() => GetCorrelationsForNotRepeatedFields();
}
