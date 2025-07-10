using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(ImportFromTemporaryStorageRegisterModuleForm))]
sealed class ImportFromTemporaryStorageRegisterModuleFormTest : ZFormBasherTest
{
	[RequiresSTA]
	public void TestMinimumSize()
	{
		CreateModuleWithNecessaryBOs();
		AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1116, 439, isInStandardDpi: true), moduleForm.Size);
	}

	[RequiresSTA]
	public void TestCancel_Button_Click()
	{
		CreateBusinessObject("REF123", 1, 2);
		Factory.Save();
		CreateModuleWithNecessaryBOs();

		CombineAssertions(() =>
		{
			filterModule.PerformSearch_ForTest();
			AssertEquals("RegLines to Select", 1, FilterControl.GridCollection.Count);

			FilterControl.Grid.Select(0);
			moduleForm.Cancel_Button.PerformClick();
			AssertEquals("Not added", 0, previousDocuments.Count);
			AssertEquals("Form is closed after clicking button", expected: false, moduleForm.Visible);
		});
	}

	[RequiresSTA]
	public void TestOK_Button_Click()
	{
		var (regHeader, _) = CreateBusinessObject("REF123", 1, 2);
		var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
		regLine2.SRL_LineNumber = 2;
		regLine2.SRL_PackagesRemaining = 4;
		regLine2.SRL_LimitDate = ZDate.Today.AddDays(2);

		CreateBusinessObject("REF456", 3, 6);
		Factory.Save();
		CreateModuleWithNecessaryBOs();

		CombineAssertions(() =>
		{
			filterModule.PerformSearch_ForTest();
			FilterControl.Grid.Select(0);
			FilterControl.Grid.Select(2);

			moduleForm.OK_Button.PerformClick();
			AssertEquals("Added to PreviousDocuments", 2, previousDocuments.Count);
			AssertPreviousDocument(1, Constants.PreviousDocumentSubTypeList.REG, "REF123", 2);
			AssertPreviousDocument(3, Constants.PreviousDocumentSubTypeList.REG, "REF456", 6);

			AssertEquals("Form is closed after clicking button", expected: false, moduleForm.Visible);
		});

		void AssertPreviousDocument(int lineNo, string subType, string referenceNumber, decimal quantity)
		{
			var previousDocument = previousDocuments.Cast<CusSupportingInfo>().Single(x => x.CSI_LineNo == lineNo);
			AssertEquals($"CSI_SubType of line {lineNo}", subType, previousDocument.CSI_SubType);
			AssertEquals($"CSI_ReferenceNumber of line {lineNo}", referenceNumber, previousDocument.CSI_ReferenceNumber);
			AssertEquals($"CSI_Quantity of line {lineNo}", quantity, previousDocument.CSI_Quantity);
		}
	}

	[RequiresSTA]
	public void TestOK_Button_Click_NoSelectedLines()
	{
		CreateBusinessObject("REF123", 1, 2);
		Factory.Save();
		CreateModuleWithNecessaryBOs();

		CombineAssertions(() =>
		{
			filterModule.PerformSearch_ForTest();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			moduleForm.OK_Button.PerformClick();
			AssertEquals("LastMessage", "Please select Register lines.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Form isn't closed after clicking button", expected: true, moduleForm.Visible);
		});
	}

	[RequiresSTA]
	public void TestOK_Button_Click_ExceedMaximumRecords()
	{
		CreateBusinessObject("REF123", 1, 2);
		CreateBusinessObject("REF456", 3, 6);
		Factory.Save();
		CreateModuleWithNecessaryBOs();

		for (int i = 0; i <= 997; i++)
		{
			var previousDocument = previousDocuments.AddNew();
			previousDocument.CSI_LineNo = i;
		}

		CombineAssertions(() =>
		{
			filterModule.PerformSearch_ForTest();
			FilterControl.Grid.Select(0);
			FilterControl.Grid.Select(1);

			moduleForm.OK_Button.PerformClick();
			AssertEquals("LastMessage", "Exceeding the maximum records 999, selected lines count is 2 and existing Docs count is 998.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Form isn't closed after clicking button", expected: true, moduleForm.Visible);
		});
	}

	[RequiresSTA]
	public void TestOK_Button_Click_ExistsOnlyAndEmptyDoc()
	{
		CreateBusinessObject("REF123", 1, 2);
		CreateBusinessObject("REF456", 3, 6);
		Factory.Save();
		CreateModuleWithNecessaryBOs();

		var previousDocument = previousDocuments.AddNew();
		CombineAssertions(() =>
		{
			filterModule.PerformSearch_ForTest();
			FilterControl.Grid.Select(0);
			FilterControl.Grid.Select(1);

			moduleForm.OK_Button.PerformClick();
			AssertEquals("PreviousDocuments count", 2, previousDocuments.Count);
			AssertEquals("Empty PreviousDocument wasn't deleted", expected: false, previousDocument.IsDeleted);
			AssertEquals($"CSI_LineNo", 1, previousDocument.CSI_LineNo);
		});
	}

	[RequiresSTA]
	public void TestOK_Button_Click_ExistsOnlyAndEmptyDoc_ExceedMaximumRecords()
	{
		for (var i = 0; i <= 999; i++)
		{
			CreateBusinessObject($"REF{i}", i + 1, 2);
		}
		Factory.Save();
		CreateModuleWithNecessaryBOs();

		var previousDocument = previousDocuments.AddNew();
		CombineAssertions(() =>
		{
			filterModule.PerformSearch_ForTest();
			for (var i = 0; i <= 999; i++)
			{
				FilterControl.Grid.Select(i);
			}
			moduleForm.OK_Button.PerformClick();
			AssertEquals("LastMessage", "Exceeding the maximum records 999, selected lines count is 1000 and existing Docs count is 0.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Form isn't closed after clicking button", expected: true, moduleForm.Visible);
		});
	}

	protected override Form GetFormToBashCore() => new ImportFromTemporaryStorageRegisterModuleForm();

	protected override void TearDown()
	{
		base.TearDown();
		moduleForm?.Dispose();
		parentForm?.Dispose();
		filterModule?.Dispose();
	}
	JobDeclarationForTest jobDeclaration;
	CusEntryInstruction cusEntryInstruction;
	ICusSupportingInfoCollection<CusSupportingInfo> previousDocuments;
	ImportFromTemporaryStorageRegisterModuleForm moduleForm;
	ZForm parentForm;
	ZFilterModule filterModule;

	ZFilterStripControl FilterControl => moduleForm.FindSingle<ZFilterStripControl>();

	(CusTempStorageRegHeader, CusTempStorageRegLine) CreateBusinessObject(string reference, int lineNo, int packagesRemaining)
	{
		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "SUM";
		regHeader.SRH_Reference = reference;
		var regLine = regHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = lineNo;
		regLine.SRL_PackagesRemaining = packagesRemaining;
		regLine.SRL_LimitDate = ZDate.Today.AddDays(2);
		return (regHeader, regLine);
	}

	void CreateModuleWithNecessaryBOs()
	{
		jobDeclaration = Factory.NewWithValidTestData<JobDeclarationForTest>();
		cusEntryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		filterModule = ZModuleFactory.Instance.Create(ModuleIDs.Customs.ImportFromTemporaryStorageRegister) as ZFilterModule;
		parentForm = new ZForm(jobDeclaration);
		filterModule.SetFormsModalTo(parentForm);
		moduleForm = filterModule.ShowPopup() as ImportFromTemporaryStorageRegisterModuleForm;
		previousDocuments = new CusSupportingInfoCollection<CusSupportingInfo>(cusEntryInstruction, "PRE");
		moduleForm.PreviousDocuments = previousDocuments;
		moduleForm.Show(parentForm);
	}

	sealed class JobDeclarationForTest : BaseJobDeclarationWithEntryInstructions, ICanImportFromTemporaryStorageRegister
	{
		public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public string TemporaryStorageApplicationCode => "SUM";

		public string DepartureCustomsOfficeCode => null;

		protected override AutologState AutoLoggingState => AutologState.NotLogged;
	}
}
