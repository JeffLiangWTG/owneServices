using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(CustomsSummaryModule))]
sealed class CustomsSummaryModuleTest : ZModuleBasherWithFetchHintsTest
{
	public void TestLicenceAndSecurityCheckPoint() => CombineAssertions(() =>
	{
		using var module = new CustomsSummaryModule();
		AssertEquals("Licence Checkpoint", Env.Licence.ImportBroker, module.LicenceCheckPoint);
		AssertEquals("SecurityCheckpoint", Env.Security.CustomsDeclarationEnquiryEdit, module.SecurityCheckpoint);
	});

	public void TestStatementModuleAllows() => CombineAssertions(() =>
	{
		using var module = new CustomsSummaryModule();
		AssertEquals("module.AllowNew", false, module.AllowNew);
		AssertEquals("module.AllowEdit", true, module.AllowEdit);
		AssertEquals("module.AllowDelete", false, module.AllowDelete);
		AssertEquals("module.AllowView", true, module.AllowView);
		AssertEquals("module.AllowUniversalCopy", false, module.AllowUniversalCopy);
		AssertEquals("module.AllowCopyFilterGridHyperlinkToClipboard", false, module.AllowCopyFilterGridHyperlinkToClipboard);
	});

	public override void TestModuleShowsAndCanSearch()
	{
		CreateStatementForFetchHintTest(0);
		Factory.Save();
		base.TestModuleShowsAndCanSearch();
	}

	public void TestDefaultSort()
	{
		using var module = new CustomsSummaryModule();
		module.PerformSearch_ForTest();
		AssertEquals(new SortInfo(nameof(CustomsSummaryLine.ProcessDate), ListSortDirection.Descending), module.GridCollection.SortInformation);
	}

	protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CH.CustomsSummary;

	protected override string CountryCode => Core.Constants.CountryCodes.Switzerland;

	protected override bool HasController() => true;

	protected override ZFilterModule CreateModuleForFetchHintsTest() => new CustomsSummaryModule();

	protected override void SetupDataForFetchHintsTest()
	{
		for (var i = 0; i < 20; i++)
		{
			CreateStatementForFetchHintTest(i);
		}
		Factory.Save();
	}

	void CreateStatementForFetchHintTest(int i)
	{
		var line = CustomsSummaryTestHelper.CreateSummaryLine(Factory);
		line.B3_EntryNum = i.ToString();
		line.B3_Status = "SNT";
		line.B3_BrokerReference = i.ToString();
		line.StatementHeader.B2_StatementNumber = i.ToString();
		line.StatementHeader.B2_ProcessDate = ZDateTime.Now.AddDays(i);
		line.StatementHeader.B2_AccountNo = i.ToString();
		line.LineCharge.B4_ChargeType = "VVM";
		line.LineCharge.B4_ReferenceNumber = i.ToString();
		line.LineCharge.B4_ChargeAmount = i;
	}
}
