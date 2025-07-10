using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals.Testing
{
	class GLJournalUserControlTest : TestCaseWithFactory
	{
		public void TestShowApprovalRequestControls()
		{
			using (var control = new GLJournalUserControl())
			{
				Assert("Default value of ShowApprovalRequestControls", control.ShowApprovalRequestControls);
				var approvalRequestStatusDropEdit = control.GetControl<ZDropEdit>("ApprovalRequestStatusDropEdit");
				Assert("approvalRequestStatusDropEdit.Visible", approvalRequestStatusDropEdit.Visible);

				control.ShowApprovalRequestControls = false;
				Assert("approvalRequestStatusDropEdit.Visible", !approvalRequestStatusDropEdit.Visible);
			}
		}

		public void TestJournalColumnsExists()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			string[] columns = new string[]
			{
				"AL_AG",
				"GLAccountDescription",
				"AL_GB",
				"AL_GE",
				"AL_Desc",
				"AL_RX_NKTransactionCurrency",
				"AL_ExchangeRate",
				"DebitCreditSign",
				"UnsignedOSLineAmount",
				"AL_OH",
				"UnsignedLocalLineAmount",
				"AL_Calc_FirstSubClassParent",
				"AL_Calc_FirstSubClassParentId",
				"AL_Calc_SecondSubClassParent",
				"AL_Calc_SecondSubClassParentId",
				"GLLocalCNAccountDescription",
				"GLLocalCNAccountCode",
				"BranchName",
				"DepartmentDescription",
				"UnitQuantity",
				"Units",
				"AlternateGLAccountNumber",
				"AlternateGLAccountDescription"
			};

			GLJournal journal = Factory.New<GLJournal>();
			using (var form = new ZForm(journal))
			{
				var control = ShowFormGLJ(form);

				var journalLinesGrid = control.Controls.Find("JournalLinesGrid", true)[0] as ZGrid;
				AssertNotNull("JournalLinesGrid", journalLinesGrid);

				AssertEquals(columns.Length, journalLinesGrid.ColumnStyles.Count);

				for (int i = 0; i < journalLinesGrid.ColumnStyles.Count; i++)
				{
					var columnInfo = (ZGridColumnInfo)journalLinesGrid.ColumnStyles[i];
					Assert(columnInfo.ColumnName + " should be present in columns list", columns.Contains(columnInfo.ColumnName));
				}
			}
		}

		public void TestGLJournalUserControlVisibility()
		{
			var journal = Factory.New<GLJournal>();
			AssertJournalColumnsVisibility(TransactionTypes.GLStandardJournal);
			AssertJournalColumnsVisibility(TransactionTypes.GLNoteJournal);

			void AssertJournalColumnsVisibility(string transactionType)
			{
				journal.AH_TransactionType = transactionType;
				using (var form = new ZForm(journal))
				{
					var control = ShowFormGLJ(form);

					var journalLinesGrid = control.Controls.Find("JournalLinesGrid", true)[0] as ZGrid;

					AssertEquals(!journal.IsNoteJournal, journalLinesGrid.GetColumnStyle("UnitQuantity").IsUnavailable);
					AssertEquals(!journal.IsNoteJournal, journalLinesGrid.GetColumnStyle("Units").IsUnavailable);
					AssertEquals(journal.IsNoteJournal, journalLinesGrid.GetColumnStyle("UnsignedOSLineAmount").IsUnavailable);
					AssertEquals(journal.IsNoteJournal, journalLinesGrid.GetColumnStyle("UnsignedLocalLineAmount").IsUnavailable);

					var invoiceAmountCalcEdit = form.FindSingleOrDefault<ZCalcEdit>("InvoiceAmountCalcEdit");
					AssertEquals(!journal.IsNoteJournal, invoiceAmountCalcEdit.Visible);
				}
			}
		}

		public void TestUnhookEventWhenControlIsDisposed()
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);

			var propertyInfoStorageField = journal.Factory.GetType().GetField("PropertyInfoStorage", BindingFlags.Instance | BindingFlags.NonPublic);
			var propertyInfoStorage = propertyInfoStorageField.GetValue(journal.Factory);

			var valueChangedDictionaryProperty = propertyInfoStorage.GetType().GetProperty("ValueChangedDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var valueChangedDictionary = valueChangedDictionaryProperty.GetValue(propertyInfoStorage);

			var methodInfo = valueChangedDictionary.GetType().GetMethod("TryGetValue", BindingFlags.Instance | BindingFlags.Public);

			Assert("Pre-condition", !HasValueChangedEvent());

			using (var form = new ZForm(journal))
			{
				ShowFormGLJ(form);

				Assert(HasValueChangedEvent());
			}
			Assert(!HasValueChangedEvent());

			bool HasValueChangedEvent()
			{
				var parameters = new object[] { journal.AH_TransactionTypeInfo, null };
				methodInfo.Invoke(valueChangedDictionary, parameters);
				var eventHandler = (EventHandler)parameters[1];
				return eventHandler.Method.Name == "AH_TransactionTypeInfo_ValueChanged";
			}
		}

		public void TestUnsignedLineAmountColumnExists()
		{
			var journal = Factory.New<GLJournal>();
			using (var form = new ZForm(journal))
			{
				var control = ShowFormGLJ(form);

				var journalLinesGrid = control.Controls.Find("JournalLinesGrid", true)[0] as ZGrid;
				AssertNotNull("JournalLinesGrid", journalLinesGrid);

				bool unsignedOSLineAmountExists = false;
				for (int i = 0; i < journalLinesGrid.ColumnStyles.Count; i++)
				{
					var columnInfo = (ZGridColumnInfo)journalLinesGrid.ColumnStyles[i];
					if (columnInfo.ColumnName == "UnsignedOSLineAmount")
					{
						unsignedOSLineAmountExists = true;
						break;
					}
				}
				Assert("The column named UnsignedLineAmount must exist and have its decimal places set to the currency of the current company.", unsignedOSLineAmountExists);
			}
		}

		public void TestHideGridColumnsForNonChinaCompany()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
				var journal = Factory.New<GLJournal>();
				using (var form = new ZForm(journal))
				{
					var control = ShowFormGLJ(form);

					var journalLinesGrid = control.Controls.Find("JournalLinesGrid", true)[0] as ZGrid;
					AssertNotNull("JournalLinesGrid", journalLinesGrid);

					Assert("The GLLocalCNAccountDescription column should exists for China company", journalLinesGrid.Columns.Contains("GLLocalCNAccountDescription"));
					Assert("The GLLocalCNAccountCode column should exists for China company", journalLinesGrid.Columns.Contains("GLLocalCNAccountCode"));
				}

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
				using (var form = new ZForm(journal))
				{
					var control = ShowFormGLJ(form);

					var journalLinesGrid = control.Controls.Find("JournalLinesGrid", true)[0] as ZGrid;
					AssertNotNull("JournalLinesGrid", journalLinesGrid);

					Assert("The GLLocalCNAccountDescription column should NOT exists for non-China company", !journalLinesGrid.Columns.Contains("GLLocalCNAccountDescription"));
					Assert("The GLLocalCNAccountCode column should exists NOT for non-China company", !journalLinesGrid.Columns.Contains("GLLocalCNAccountCode"));
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestSubAccountsVisibility()
		{
			var journal = Factory.NewWithValidTestData<GLJournal>();
			using (var form = new ZForm(journal))
			{
				ShowFormGLJ(form);

				var journalLinesGrid = form.FindSingleOrDefault<ZGrid>("JournalLinesGrid");

					AssertEquals("AL_Calc_FirstSubClassParent caption", "Sub Account 1 Type", journalLinesGrid.GetColumnCaption("AL_Calc_FirstSubClassParent"));
					AssertEquals("AL_Calc_FirstSubClassParentId caption", "Sub Account 1", journalLinesGrid.GetColumnCaption("AL_Calc_FirstSubClassParentId"));
					AssertEquals("AL_Calc_SecondSubClassParent caption", "Sub Account 2 Type", journalLinesGrid.GetColumnCaption("AL_Calc_SecondSubClassParent"));
					AssertEquals("AL_Calc_SecondSubClassParentId caption", "Sub Account 2", journalLinesGrid.GetColumnCaption("AL_Calc_SecondSubClassParentId"));

				var subAccountsGroupBox = form.FindSingleOrDefault<ZGroupBox>("GLJournalSubAccountsGroupBox");
				AssertNotNull("Pre-condition", subAccountsGroupBox);
				AssertEquals($"SubAccountsGroupBox should be visible", true, subAccountsGroupBox.Visible);

				var bottomPanel = form.FindSingleOrDefault<ZPanel>("BottomPanel");
				AssertEquals("bottomPanel size should be big enough to hold sub accounts grid", ControlDpiScalingHelper.NewScaledSize(952, 137, true), bottomPanel.Size);
			}
		}

		public void TestDissectionAttributesVisibility()
		{
			var journal = Factory.NewWithValidTestData<GLJournal>();
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new ZForm(journal))
			{
				ShowFormGLJ(form);

				var bottomPanelTabControl = form.FindSingleOrDefault<ZTabControl>("BottomPanelTabControl");
				bottomPanelTabControl.SelectTab(1);
				var dissectionAttributesGroupBox = form.FindSingleOrDefault<ZGroupBox>("GLJournalDissectionAttributesGroupBox");
				AssertNotNull("Pre-condition", dissectionAttributesGroupBox);
				AssertEquals($"DissectionAttributesGroupBox should be visible", true, dissectionAttributesGroupBox.Visible);

				var bottomPanel = form.FindSingleOrDefault<ZPanel>("BottomPanel");
				AssertEquals("bottomPanel size should be big enough to hold dissection attribute grid", ControlDpiScalingHelper.NewScaledSize(952, 137, true), bottomPanel.Size);
			}

			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var form = new ZForm(journal))
			{
				ShowFormGLJ(form);

				var dissectionAttributesGroupBox = form.FindSingleOrDefault<ZGroupBox>("GLJournalDissectionAttributesGroupBox");
				AssertNull(dissectionAttributesGroupBox);
			}
		}

		public void TestPostDateAndDueDateVisibility()
		{
			var journal = Factory.NewWithValidTestData<GLJournal>(); //default is GJL

			using (var form = new ZForm(journal))
			{
				var control = ShowFormGLJ(form);
				var postPeriodPostDateEdit = control.GetControl<ZDateEdit>("PostPeriodPostDateEdit");
				Assert("not PostDateEdit.Visible", !postPeriodPostDateEdit.Visible);
				var reversePeriodDueDateEdit = control.GetControl<ZDateEdit>("ReversePeriodDueDateEdit");
				Assert("not DueDateEdit.Visible", !reversePeriodDueDateEdit.Visible);

				journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
				Application.DoEvents();
				Assert("not PostDateEdit.Visible", !postPeriodPostDateEdit.Visible);
				Assert("not DueDateEdit.Visible", !reversePeriodDueDateEdit.Visible);

				journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
				Application.DoEvents();
				Assert("not PostDateEdit.Visible", !postPeriodPostDateEdit.Visible);
				Assert("not DueDateEdit.Visible", !reversePeriodDueDateEdit.Visible);

				journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
				Application.DoEvents();
				Assert("not PostDateEdit.Visible", !postPeriodPostDateEdit.Visible);
				Assert("not DueDateEdit.Visible", !reversePeriodDueDateEdit.Visible);
			}

			using (Registry.Business.AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				using (var form = new ZForm(journal))
				{
					var control = ShowFormGLJ(form);
					var postPeriodPostDateEdit = control.GetControl<ZDateEdit>("PostPeriodPostDateEdit");
					Assert("PostDateEdit.Visible", postPeriodPostDateEdit.Visible);
					Assert("not PostDateEdit.ReadOnly", !postPeriodPostDateEdit.ReadOnly);
					var reversePeriodDueDateEdit = control.GetControl<ZDateEdit>("ReversePeriodDueDateEdit");
					Assert("DueDateEdit.Visible", reversePeriodDueDateEdit.Visible);
					Assert("not DueDateEdit.ReadOnly", !reversePeriodDueDateEdit.ReadOnly);

					journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
					Application.DoEvents();
					Assert("PostDateEdit.Visible", postPeriodPostDateEdit.Visible);
					Assert("not PostDateEdit.ReadOnly", !postPeriodPostDateEdit.ReadOnly);
					Assert("not DueDateEdit.Visible", !reversePeriodDueDateEdit.Visible);

					journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
					Application.DoEvents();
					Assert("not PostDateEdit.Visible", !postPeriodPostDateEdit.Visible);
					Assert("not DueDateEdit.Visible", !reversePeriodDueDateEdit.Visible);

					journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
					Application.DoEvents();
					Assert("PostDateEdit.Visible", postPeriodPostDateEdit.Visible);
					Assert("not PostDateEdit.ReadOnly", !postPeriodPostDateEdit.ReadOnly);
					Assert("not DueDateEdit.Visible", !reversePeriodDueDateEdit.Visible);
				}
			}
		}

		GLJournalUserControl ShowFormGLJ(ZForm form)
		{
			var control = new GLJournalUserControl();
			form.Controls.Add(control);
			form.Show();
			Application.DoEvents();

			return control;
		}

		public void TestShowAlternateGLAccountNumberAndDescription_HasGLAccountSelectionAndEntry()
		{
			var chart = TestObjectCreator.CreateAlternateChart("MGT", "Management Reporting", isGlobal: true);
			TestObjectCreator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			Factory.Save();

			var glHeader = TestObjectCreator.CreateAccGLHeader("1991.01.10", "AS", "BANK ACCOUNT", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			var alternateGLAccount = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1, description: "AlternateGLAccount1");
			TestObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK);
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid());

			AssertShowAlternateGLAccountNumberAndDescription(true);
		}

		public void TestShowAlternateGLAccountNumberAndDescription_NoGLAccountSelectionAndEntry()
		{
			AssertShowAlternateGLAccountNumberAndDescription(false);
		}

		void AssertShowAlternateGLAccountNumberAndDescription(bool hasGLAccountSelectionAndEntry)
		{
			var journal = Factory.NewWithValidTestData<GLJournal>();
			using (var form = new ZForm(journal))
			{
				ShowFormGLJ(form);
				var journalLinesGrid = form.FindSingleOrDefault<ZGrid>("JournalLinesGrid");

				var alternateGLAccountNumber = journalLinesGrid.GetColumnStyle("AlternateGLAccountNumber");
				AssertNotNull(alternateGLAccountNumber);
				AssertEquals(true, hasGLAccountSelectionAndEntry ? alternateGLAccountNumber.IsVisible : alternateGLAccountNumber.IsUnavailable);

				var alternateGLAccountDescription = journalLinesGrid.GetColumnStyle("AlternateGLAccountDescription");
				AssertNotNull(alternateGLAccountDescription);
				AssertEquals(true, hasGLAccountSelectionAndEntry ? alternateGLAccountNumber.IsVisible : alternateGLAccountDescription.IsUnavailable);
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;
	}
}
