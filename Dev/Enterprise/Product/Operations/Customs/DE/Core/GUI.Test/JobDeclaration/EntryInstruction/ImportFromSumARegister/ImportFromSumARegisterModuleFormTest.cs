using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ImportFromSumARegisterModuleForm))]
	class ImportFromSumARegisterModuleFormTest : ZFormBasherTest
	{
		public void TestMinimumSize()
		{
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1116, 439, true), moduleForm.Size);
		}

		public void TestCancel_Button_Click()
		{
			CreateBusinessObject("REF123", 1, 2);
			Factory.Save();

			CombineAssertions(() =>
			{
				filterModule.PerformSearch_ForTest();
				AssertEquals("RegLines to Select", FilterControl.GridCollection.Count, 1);

				FilterControl.Grid.Select(0);
				moduleForm.Cancel_Button.PerformClick();
				AssertEquals("Not added", 0, cusEntryInstruction.PreviousDocuments.Count);
				AssertEquals("Form is closed after clicking button", false, moduleForm.Visible);
			});
		}

		public void TestOK_Button_Click()
		{
			var (regHeader, _) = CreateBusinessObject("REF123", 1, 2);
			var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_PackagesRemaining = 4;
			regLine2.SRL_LimitDate = ZDate.Today.AddDays(2);

			CreateBusinessObject("REF456", 3, 6);
			Factory.Save();

			CombineAssertions(() =>
			{
				filterModule.PerformSearch_ForTest();
				FilterControl.Grid.Select(0);
				FilterControl.Grid.Select(2);

				moduleForm.OK_Button.PerformClick();
				AssertEquals("Added to PreviousDocuments", 2, cusEntryInstruction.PreviousDocuments.Count);
				AssertPreviousDocument(1, PreviousDocSubTypeList.Codes.REG, "REF123", 2);
				AssertPreviousDocument(3, PreviousDocSubTypeList.Codes.REG, "REF456", 6);

				AssertEquals("Form is closed after clicking button", false, moduleForm.Visible);
			});

			void AssertPreviousDocument(ZInt lineNo, ZString subType, ZString referenceNumber, ZDecimal quantity)
			{
				var previousDocument = cusEntryInstruction.PreviousDocuments.Cast<PreviousDocument>().Single(x => x.CSI_LineNo == lineNo);
				AssertEquals($"CSI_SubType of line {lineNo}", subType, previousDocument.CSI_SubType);
				AssertEquals($"CSI_ReferenceNumber of line {lineNo}", referenceNumber, previousDocument.CSI_ReferenceNumber);
				AssertEquals($"CSI_Quantity of line {lineNo}", quantity, previousDocument.CSI_Quantity);
			}
		}

		public void TestOK_Button_Click_NoSelectedLines()
		{
			CreateBusinessObject("REF123", 1, 2);
			Factory.Save();

			CombineAssertions(() =>
			{
				filterModule.PerformSearch_ForTest();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				moduleForm.OK_Button.PerformClick();
				AssertEquals("LastMessage", "Please select Register lines.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form isn't closed after clicking button", true, moduleForm.Visible);
			});
		}

		public void TestOK_Button_Click_ExceedMaximumRecords()
		{
			CreateBusinessObject("REF123", 1, 2);
			CreateBusinessObject("REF456", 3, 6);
			Factory.Save();

			for (int i = 0; i <= 997; i++)
			{
				var previousDocument = cusEntryInstruction.PreviousDocuments.AddNew();
				previousDocument.CSI_LineNo = i;
			}

			CombineAssertions(() =>
			{
				filterModule.PerformSearch_ForTest();
				FilterControl.Grid.Select(0);
				FilterControl.Grid.Select(1);

				moduleForm.OK_Button.PerformClick();
				AssertEquals("LastMessage", "Exceeding the maximum records 999, selected lines count is 2 and existing Docs count is 998.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form isn't closed after clicking button", true, moduleForm.Visible);
			});
		}

		public void TestOK_Button_Click_ExistsOnlyAndEmptyDoc()
		{
			CreateBusinessObject("REF123", 1, 2);
			CreateBusinessObject("REF456", 3, 6);
			Factory.Save();

			var previousDocument = cusEntryInstruction.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				filterModule.PerformSearch_ForTest();
				FilterControl.Grid.Select(0);
				FilterControl.Grid.Select(1);

				moduleForm.OK_Button.PerformClick();
				AssertEquals("PreviousDocuments count", 2, cusEntryInstruction.PreviousDocuments.Count);
				AssertEquals("Empty PreviousDocument wasn't deleted", false, previousDocument.IsDeleted);
				AssertEquals($"CSI_LineNo", 1, previousDocument.CSI_LineNo);
			});
		}

		public void TestOK_Button_Click_ExistsOnlyAndEmptyDoc_ExceedMaximumRecords()
		{
			for (int i = 0; i <= 999; i++)
			{
				CreateBusinessObject($"REF{i}", i + 1, 2);
			}
			Factory.Save();

			var previousDocument = cusEntryInstruction.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				filterModule.PerformSearch_ForTest();
				for (int i = 0; i <= 999; i++)
				{
					FilterControl.Grid.Select(i);
				}
				moduleForm.OK_Button.PerformClick();
				AssertEquals("LastMessage", "Exceeding the maximum records 999, selected lines count is 1000 and existing Docs count is 0.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form isn't closed after clicking button", true, moduleForm.Visible);
			});
		}

		protected override Form GetFormToBashCore() => new ImportFromSumARegisterModuleForm();

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			cusEntryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			filterModule = ZModuleFactory.Instance.Create(ModuleIDs.Customs.EU.DE.ImportFromSumARegister) as ZFilterModule;
			parentForm = new ZForm(jobDeclaration);
			filterModule.SetFormsModalTo(parentForm);
			moduleForm = filterModule.ShowPopup() as ImportFromSumARegisterModuleForm;
			moduleForm.PreviousDocuments = cusEntryInstruction.PreviousDocuments;
			moduleForm.Show(parentForm);
		}

		protected override void TearDown()
		{
			base.TearDown();
			moduleForm.Dispose();
			parentForm.Dispose();
			filterModule.Dispose();
		}
		JobDeclaration jobDeclaration;
		CusEntryInstruction cusEntryInstruction;
		ImportFromSumARegisterModuleForm moduleForm;
		ZForm parentForm;
		ZFilterModule filterModule;

		ZFilterStripControl FilterControl => moduleForm.FindSingle<ZFilterStripControl>();

		(CusTempStorageRegHeader, CusTempStorageRegLine) CreateBusinessObject(ZString reference, ZInt lineNo, ZInt packagesRemaining)
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = reference;
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = lineNo;
			regLine.SRL_PackagesRemaining = packagesRemaining;
			regLine.SRL_LimitDate = ZDate.Today.AddDays(2);
			return (regHeader, regLine);
		}
	}
}
