using System;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class CustomsProcedureWrapperTest : TestCaseWithFactory
{
	public void TestProcedure()
	{
		var customsProcedureWrapper = GetNewCustomsProcedureWrapper("", Array.Empty<ZString>(), addDefaultAdditionalProcedureIfNone: true);
		AssertEquals(nameof(ICustomsProcedure.Procedure), "", customsProcedureWrapper.Procedure);

		customsProcedureWrapper = GetNewCustomsProcedureWrapper("7100B51", Array.Empty<ZString>(), addDefaultAdditionalProcedureIfNone: true);
		AssertEquals(nameof(ICustomsProcedure.Procedure), "71", customsProcedureWrapper.Procedure);
	}

	public void TestPreviousProcedure()
	{
		var customsProcedureWrapper = GetNewCustomsProcedureWrapper("", Array.Empty<ZString>(), addDefaultAdditionalProcedureIfNone: true);
		AssertEquals(nameof(ICustomsProcedure.PreviousProcedure), "", customsProcedureWrapper.PreviousProcedure);

		customsProcedureWrapper = GetNewCustomsProcedureWrapper("7100B51", Array.Empty<ZString>(), addDefaultAdditionalProcedureIfNone: true);
		AssertEquals(nameof(ICustomsProcedure.PreviousProcedure), "00", customsProcedureWrapper.PreviousProcedure);
	}

	public void TestAdditionalProcedure()
	{
		var customsProcedureWrapper = GetNewCustomsProcedureWrapper("", Array.Empty<ZString>(), addDefaultAdditionalProcedureIfNone: true);
		AssertContainsExactElementsInExactOrder(nameof(ICustomsProcedure.AdditionalProcedures), new[] { "1NN" }, customsProcedureWrapper.AdditionalProcedures);

		customsProcedureWrapper = GetNewCustomsProcedureWrapper("", Array.Empty<ZString>(), addDefaultAdditionalProcedureIfNone: false);
		AssertContainsExactElementsInExactOrder(nameof(ICustomsProcedure.AdditionalProcedures), Array.Empty<string>(), customsProcedureWrapper.AdditionalProcedures);

		customsProcedureWrapper = GetNewCustomsProcedureWrapper("7100A18", Array.Empty<ZString>(), addDefaultAdditionalProcedureIfNone: true);
		AssertContainsExactElementsInExactOrder(nameof(ICustomsProcedure.AdditionalProcedures), new[] { "A18" }, customsProcedureWrapper.AdditionalProcedures);

		customsProcedureWrapper = GetNewCustomsProcedureWrapper("7100B51", new ZString[] { "7100C44", "7100E12", ZString.Empty, "7100", null }, addDefaultAdditionalProcedureIfNone: true);
		AssertContainsExactElementsInExactOrder(nameof(ICustomsProcedure.AdditionalProcedures), new[] { "B51", "C44", "E12" }, customsProcedureWrapper.AdditionalProcedures);
	}

	ICustomsProcedure GetNewCustomsProcedureWrapper(ZString procedure, ZString[] additionalProcedureCodes, bool addDefaultAdditionalProcedureIfNone) => new CustomsProcedureWrapper(procedure, additionalProcedureCodes, addDefaultAdditionalProcedureIfNone);
}
