using System;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class SadDocumentSupporterConfiguratorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new SadDocumentSupporterConfigurator(null));
	}

	public void TestGetDataStateBeforeRun()
	{
		var dummyConfigurator = new DummyCusEntryHeaderDocumentSupporterConfigurator();
		ObjectFactory.Substitute<IITCusEntryHeaderDocumentSupporterConfigurator>(dummyConfigurator);

		var documentSupporter = entryHeader.DocumentSupporter;
		var documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(null);
		AssertEquals("IsValid", true, documentSupporterDataState.IsValid);

		var sadHmenuItemForTesting = Factory.New<IStmMenuItem>();
		sadHmenuItemForTesting.SU_MenuName = "SADH C88";
		documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(sadHmenuItemForTesting);
		CombineAssertions("Assert GetDataStateBeforeRun for SADH Item", () =>
		{
			AssertEquals("IsValid", true, documentSupporterDataState.IsValid);
			AssertEquals("ErrorMessage", "", documentSupporterDataState.ErrorMessage);
			AssertEquals("DeclarationDocumentSupporter BGMReferenceToPrint", "DUMMY", declaration.DocumentSupporter.SadDocumentSupporter.BGMReferenceToPrint);
			AssertEquals("DeclarationDocumentSupporter LayoutStyle", "X", declaration.DocumentSupporter.SadDocumentSupporter.LayoutStyle);
		});

		dummyConfigurator.Cancel = true;
		documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(sadHmenuItemForTesting);
		AssertEquals("Assert GetDataStateBeforeRun for SADH Item Canceling action, IsValid", false, documentSupporterDataState.IsValid);
	}

	public void TestGetDataStateBeforeRun_WhenMenuItemIsSadHCopy()
	{
		var sadCopyTestInputList = new[]
		{
			(MenuName: "SADH C88 Copy C", ExpectedLayoutStyle: "C"),
			(MenuName: "SADH C88 Copy 1", ExpectedLayoutStyle: "1"),
			(MenuName: "SADH C88 Copy 3A", ExpectedLayoutStyle: "3A"),
			(MenuName: "SADH C88 Copy 3B", ExpectedLayoutStyle: "3B"),
			(MenuName: "SADH C88 Copy 6", ExpectedLayoutStyle: "6"),
			(MenuName: "SADH C88 Copy 7", ExpectedLayoutStyle: "7"),
			(MenuName: "SADH C88 Copy 8", ExpectedLayoutStyle: "8"),
			(MenuName: "SADH C88 Copy 8R", ExpectedLayoutStyle: "8R"),
			(MenuName: "SADH C88 Copy I", ExpectedLayoutStyle: "I"),
		};

		entryHeader.CH_BGMReference = "BGMReference";

		foreach (var (menuName, expectedLayoutStyle) in sadCopyTestInputList)
		{
			AssertGetDataStateBeforeRunSadHCopy(menuName, expectedLayoutStyle);
		}

		void AssertGetDataStateBeforeRunSadHCopy(string menuName, string expectedLayoutStyle)
		{
			var sadHmenuItemForTesting = Factory.New<IStmMenuItem>();
			sadHmenuItemForTesting.SU_MenuName = menuName;

			var documentSupporterDataState = entryHeader.DocumentSupporter.GetDataStateBeforeRun(sadHmenuItemForTesting);
			CombineAssertions("Assert GetDataStateBeforeRun for SADH Copy Item", () =>
			{
				AssertEquals("IsValid", true, documentSupporterDataState.IsValid);
				AssertEquals("ErrorMessage", "", documentSupporterDataState.ErrorMessage);

				var sadDocumentSupporter = declaration.DocumentSupporter.SadDocumentSupporter;
				AssertEquals("DeclarationDocumentSupporter BGMReferenceToPrint", "BGMReference", sadDocumentSupporter.BGMReferenceToPrint);
				AssertEquals("DeclarationDocumentSupporter LayoutStyle", expectedLayoutStyle, sadDocumentSupporter.LayoutStyle);
			});
		}
	}

	public void TestGetDataStateBeforeRun_WhenMenuItemIsNotSadH()
	{
		var sadHmenuItemForTesting = Factory.New<IStmMenuItem>();
		sadHmenuItemForTesting.SU_MenuName = "NOT SADH MENU ITEM";

		var documentSupporterDataState = entryHeader.DocumentSupporter.GetDataStateBeforeRun(sadHmenuItemForTesting);
		CombineAssertions("Assert GetDataStateBeforeRun for NON SADH Item", () =>
		{
			AssertEquals("IsValid", true, documentSupporterDataState.IsValid);
			AssertEquals("ErrorMessage", "", documentSupporterDataState.ErrorMessage);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	class DummyCusEntryHeaderDocumentSupporterConfigurator : IITCusEntryHeaderDocumentSupporterConfigurator
	{
		public bool Cancel { get; set; }

		public CancelEventArgs Configure(JobDeclarationSadDocumentSupporter supporter)
		{
			supporter.BGMReferenceToPrint = "DUMMY";
			supporter.LayoutStyle = "X";

			return new CancelEventArgs(Cancel);
		}
	}
}
