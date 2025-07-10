using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class Nb1SadFieldDescriptionProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Factory is required", () => new Nb1SadFieldDescriptionProvider(null));
	}

	public void TestGetDescriptionBySequenceNumber()
	{
		var nb1Provider = new Nb1SadFieldDescriptionProvider(Factory);

		CombineAssertions("Assert GetDescriptionBySequenceNumber in many scenarios", () =>
		{
			AssertEquals("Register Code", nb1Provider.GetDescriptionBySequenceNumber("7"));
			AssertEquals("Item Number", nb1Provider.GetDescriptionBySequenceNumber("12"));
			AssertEquals("Register Code", nb1Provider.GetDescriptionBySequenceNumber("27"));
			AssertEquals("Register Code", nb1Provider.GetDescriptionBySequenceNumber("47"));
			AssertEquals("Net Mass", nb1Provider.GetDescriptionBySequenceNumber("25"));
			AssertEquals("CIN", nb1Provider.GetDescriptionBySequenceNumber("37"));
			AssertEquals("Supplementary Unit", nb1Provider.GetDescriptionBySequenceNumber("86"));
			AssertEquals("Supplementary Unit", nb1Provider.GetDescriptionBySequenceNumber("26"));

			AssertEquals("", nb1Provider.GetDescriptionBySequenceNumber(""));
			AssertEquals("", nb1Provider.GetDescriptionBySequenceNumber(null));
		});
	}

	public void TestFieldStartingIndex()
	{
		var nb1Provider = new Nb1SadFieldDescriptionProvider_ForTest(Factory);
		AssertEquals(7, nb1Provider.FieldStartingIndex_Exposed);
	}

	public void TestGetCorrelationsForNotRepeatedFieldsExposed()
	{
		var nb1Provider = new Nb1SadFieldDescriptionProvider_ForTest(Factory);
		Assert(!nb1Provider.GetCorrelationsForNotRepeatedFieldsExposed().Any());
	}
}

class Nb1SadFieldDescriptionProvider_ForTest : Nb1SadFieldDescriptionProvider
{
	public Nb1SadFieldDescriptionProvider_ForTest(BusinessObjectFactory factory) : base(factory)
	{
	}

	public int FieldStartingIndex_Exposed => base.FieldStartingIndex;

	public IEnumerable<CodeDescriptionPair> GetCorrelationsForNotRepeatedFieldsExposed() => GetCorrelationsForNotRepeatedFields();
}
