using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class CustomsFieldDescriptionsProviderTest : TestCaseWithFactory
{
	public void TestGetDescriptionByReferenceCode()
	{
		CombineAssertions("Assertions for SadFieldDescriptionList", () =>
		{
			AssertEquals("When Factory is null, description should be", ZString.Empty, CustomsFieldDescriptionsProvider.GetDescriptionByReferenceCode<SadFieldDescriptionList>(null, ""));
			AssertEquals("When reference code is null, description should be", ZString.Empty, CustomsFieldDescriptionsProvider.GetDescriptionByReferenceCode<SadFieldDescriptionList>(Factory, ""));
			AssertEquals("When reference code is T, description should be", "Total Item Taxed Amount", CustomsFieldDescriptionsProvider.GetDescriptionByReferenceCode<SadFieldDescriptionList>(Factory, "T"));
			AssertEquals("When reference code is invalid, description should be", ZString.Empty, CustomsFieldDescriptionsProvider.GetDescriptionByReferenceCode<SadFieldDescriptionList>(Factory, "XX"));
		});

		CombineAssertions("Assertions for NbSadFieldDescriptionList", () =>
		{
			AssertEquals("When Factory is null, description should be", ZString.Empty, CustomsFieldDescriptionsProvider.GetDescriptionByReferenceCode<NbSadFieldDescriptionList>(null, ""));
			AssertEquals("When reference code is null, description should be", ZString.Empty, CustomsFieldDescriptionsProvider.GetDescriptionByReferenceCode<NbSadFieldDescriptionList>(Factory, ""));
			AssertEquals("When reference code is T, description should be", "Supplementary Unit", CustomsFieldDescriptionsProvider.GetDescriptionByReferenceCode<NbSadFieldDescriptionList>(Factory, "T"));
			AssertEquals("When reference code is PA1, description should be", "Register Code", CustomsFieldDescriptionsProvider.GetDescriptionByReferenceCode<NbSadFieldDescriptionList>(Factory, "PA1"));
			AssertEquals("When reference code is invalid, description should be", ZString.Empty, CustomsFieldDescriptionsProvider.GetDescriptionByReferenceCode<NbSadFieldDescriptionList>(Factory, "XX"));
		});
	}

	public void TestConstructor()
	{
		CombineAssertions("", () =>
		{
			AssertExceptionThrown<ArgumentNullException>("Create new CustomsFieldDescriptionsProvider with null factory", () => new CustomsFieldDescriptionsProvider<InterchangeHeaderDescriptionList>(null));
			AssertNoExceptionThrown("Create new CustomsFieldDescriptionsProvider with factory", () => new CustomsFieldDescriptionsProvider<InterchangeHeaderDescriptionList>(Factory));
		});
	}
}
