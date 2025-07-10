using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestLoadPackingPageForMultiPacks()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			using (ZForm form = new ZForm(declaration))
			using (AUBrokerageUserControl userControl = new AUBrokerageUserControl())
			{
				form.Controls.Add(userControl);
				userControl.JobDeclaration = declaration;
				form.Show();
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				AssertNotNull("Packing Tab Page User Control has Loaded", userControl.Packing);
				AssertEquals("Packing Tab Page is BaseCustomsPackingUserControl", typeof(PackingUserControl), userControl.Packing.GetType());
			}
		}

		public void TestMessageInitiatorAfterSettingDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (AUBrokerageUserControl userControl = new AUBrokerageUserControl())
			{
				userControl.JobDeclaration = declaration;
				AssertNotNull(declaration.MessageInitiator);
			}
		}

		public void TestGroupInvoiceUserControl()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				AssertEquals("Brokerage user control has GroupInvoice control", true, testForm.CustomsBrokerageUserControl.MainTabControl.TabPages.Contains(testForm.CustomsBrokerageUserControl.InvoiceGroupingTabPage));
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Brokerage user control does not have GroupInvoice control as the job is export", false, testForm.CustomsBrokerageUserControl.MainTabControl.TabPages.Contains(testForm.CustomsBrokerageUserControl.InvoiceGroupingTabPage));
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Brokerage user control has GroupInvoice control as this is Import CMR", true, testForm.CustomsBrokerageUserControl.MainTabControl.TabPages.Contains(testForm.CustomsBrokerageUserControl.InvoiceGroupingTabPage));
				declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				AssertEquals("Brokerage user control does not have GroupInvoice control as this is legacy", false, testForm.CustomsBrokerageUserControl.MainTabControl.TabPages.Contains(testForm.CustomsBrokerageUserControl.InvoiceGroupingTabPage));
				declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				AssertEquals("Back to CMR and Group Invoice Control should be visible", true, testForm.CustomsBrokerageUserControl.MainTabControl.TabPages.Contains(testForm.CustomsBrokerageUserControl.InvoiceGroupingTabPage));
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				AssertEquals("PreCondition: IsExWarehouse", true, declaration.IsExWarehouse);
				AssertEquals("Group Invoice Control should not be visible for ExWarehouse", false, testForm.CustomsBrokerageUserControl.MainTabControl.TabPages.Contains(testForm.CustomsBrokerageUserControl.InvoiceGroupingTabPage));
			}
		}

		public void TestSupplierControlForExportJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("AUOtherSupplierHeaderUserControl", typeof(AUOtherSupplierHeaderUserControl), testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.GetType());
			}
		}

		public void TestSupplierControlForEdificeImportJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("AUOtherSupplierHeaderUserControl", typeof(AUOtherSupplierHeaderUserControl), testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.GetType());
			}
		}

		public void TestSupplierControlForCMRImportJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("AUCMRSupplierHeaderUserControl", typeof(AUCMRSupplierHeaderUserControl), testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.GetType());
			}
		}

		public void TestSupplierControlForEdificeEXWJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("AUOtherSupplierHeaderUserControl", typeof(AUOtherSupplierHeaderUserControl), testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.GetType());
			}
		}

		public void TestSupplierControlForCMREXWJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("AUCMRSupplierHeaderUserControl", typeof(AUCMRSupplierHeaderUserControl), testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.GetType());
			}
		}

		public void TestMessageUserControlType()
		{
			var declaration = JobDeclaration.New(Factory);
			using (var testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MessagesTabPage;
				AssertEquals("Import dec", typeof(AUImportDiscardedMessageUserControl), testForm.CustomsBrokerageUserControl.MessageUserControl.GetType());
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MessagesTabPage;
				AssertEquals("Export dec", typeof(AUImportDiscardedMessageUserControl), testForm.CustomsBrokerageUserControl.MessageUserControl.GetType());

				var entryNum = CusEntryNumber.New(declaration, CANType.CustomsAuthorityNumber.Code, Core.Constants.CountryCodes.Australia);
				entryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MessagesTabPage;
				AssertEquals("Export dec", typeof(ExportMessageUserControl), testForm.CustomsBrokerageUserControl.MessageUserControl.GetType());
			}
		}

		public void TestSwapingBetweenEdificeAndCMRSupplierControl()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("AUCMRSupplierHeaderUserControl", typeof(AUCMRSupplierHeaderUserControl), testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.GetType());
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("AUOtherSupplierHeaderUserControl", typeof(AUOtherSupplierHeaderUserControl), testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.GetType());
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = "CMR";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("AUCMRSupplierHeaderUserControl", typeof(AUCMRSupplierHeaderUserControl), testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.GetType());
			}
		}

		[TestDate(2004, 5, 5)]
		public void TestSwapingSupplierControlWhenNotCMR()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("AUOtherSupplierHeaderUserControl", typeof(AUOtherSupplierHeaderUserControl), testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.GetType());
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("AUOtherSupplierHeaderUserControl", typeof(AUOtherSupplierHeaderUserControl), testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.GetType());
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("AUOtherSupplierHeaderUserControl", typeof(AUOtherSupplierHeaderUserControl), testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl.GetType());
			}
		}

		public void TestInvoiceLineControlForExportJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals("InvoiceLineControl is of type AUExportInvoiceLineUserControl", typeof(AUExportInvoiceLineUserControl), testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.GetType());
			}
		}

		public void TestInvoiceLineControlForEdificeImportJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals("InvoiceLineControl is of type AUEdificeImportInvoiceLinesUserControl", typeof(AUEdificeImportInvoiceLinesUserControl), testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.GetType());
			}
		}

		public void TestInvoiceLineControlForCMRImportJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals("InvoiceLineControl is of type AUCMRImportInvoiceLineUserControl", typeof(AUCMRImportInvoiceLineUserControl), testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.GetType());
			}
		}

		public void TestInvoiceLinesControlForEdificeEXWJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals("InvoiceLineControl is of type AUEdificeImportInvoiceLinesUserControl", typeof(AUEdificeImportInvoiceLinesUserControl), testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.GetType());
			}
		}

		public void TestInvoiceLinesControlForCMREXWJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals("InvoiceLineControl is of type AUCMRImportInvoiceLineUserControl", typeof(AUCMRImportInvoiceLineUserControl), testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.GetType());
			}
		}

		public void TestSwapingBetweenEdificeAndCMRInvoiceLinesControl()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals("InvoiceLineControl is of type AUCMRImportInvoiceLineUserControl", typeof(AUCMRImportInvoiceLineUserControl), testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.GetType());
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals("InvoiceLineControl is of type AUExportInvoiceLineUserControl", typeof(AUExportInvoiceLineUserControl), testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.GetType());
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = "CMR";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals("InvoiceLineControl is of type AUCMRImportInvoiceLineUserControl", typeof(AUCMRImportInvoiceLineUserControl), testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.GetType());
			}
		}

		[TestDate(2004, 5, 5)]
		public void TestSwapingInvoiceLinesControlWhenNotCMR()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals("InvoiceLineControl is of type AUEdificeImportInvoiceLinesUserControl", typeof(AUEdificeImportInvoiceLinesUserControl), testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.GetType());
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals("InvoiceLineControl is of type AUExportInvoiceLineUserControl", typeof(AUExportInvoiceLineUserControl), testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.GetType());
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals("InvoiceLineControl is of type AUEdificeImportInvoiceLinesUserControl", typeof(AUEdificeImportInvoiceLinesUserControl), testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.GetType());
			}
		}

		public void TestExportInvoiceLinesGridShouldNotHaveDuplicateColumnStyles()
		{
			var declaration = JobDeclaration.New(Factory);
			using (AUCustomsDeclarationFormForTest form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var grid = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;
				var columns =
					from c in grid.ColumnStyles.Cast<ZGridColumnInfo>()
					group c by c.ColumnName into g
					select new
					{
						ColumnName = g.Key,
						Count = g.Count()
					}

				;
				Assert("CustomsInvoiceLinesBoundGrid should not have any duplicate column styles", !columns.Any(column => column.Count > 1));
			}
		}

		public void TestImportInvoiceLinesGridShouldNotHaveDuplicateColumnStyles()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			using (AUCustomsDeclarationFormForTest form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var grid = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;
				var columns =
					from c in grid.ColumnStyles.Cast<ZGridColumnInfo>()
					group c by c.ColumnName into g
					select new
					{
						ColumnName = g.Key,
						Count = g.Count()
					}

				;
				Assert("CustomsInvoiceLinesBoundGrid should not have any duplicate column styles", !columns.Any(column => column.Count > 1));
			}
		}

		public void TestMiscOptionsForExportJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				AssertEquals("AUEdificeMiscOptionsUserControl", typeof(AUEdificeMiscOptionsUserControl), testForm.CustomsBrokerageUserControl.MiscOptions.GetType());
			}
		}

		public void TestSMiscOptionsForEdificeImportJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				AssertEquals("AUEdificeMiscOptionsUserControl", typeof(AUEdificeMiscOptionsUserControl), testForm.CustomsBrokerageUserControl.MiscOptions.GetType());
			}
		}

		public void TestMiscOptionsForCMRImportJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				AssertEquals("AUCMRMiscOptionsUserControl", typeof(AUCMRMiscOptionsUserControl), testForm.CustomsBrokerageUserControl.MiscOptions.GetType());
			}
		}

		public void TestMiscOptionsForEdificeEXWJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				AssertEquals("AUEdificeMiscOptionsUserControl", typeof(AUEdificeMiscOptionsUserControl), testForm.CustomsBrokerageUserControl.MiscOptions.GetType());
			}
		}

		public void TestMiscOptionsForCMREXWJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				AssertEquals("AUCMRMiscOptionsUserControl", typeof(AUCMRMiscOptionsUserControl), testForm.CustomsBrokerageUserControl.MiscOptions.GetType());
			}
		}

		public void TestVisiblityForCOLSTabPage()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_MessageType = "IMP";
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				using (var testForm = new AUCustomsDeclarationFormForTest(declaration))
				{
					testForm.Show();
					AssertEquals("COLS TabPage is not visible", false, testForm.CustomsBrokerageUserControl.COLSTabPage.TabVisible);
				}

				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				var entryNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Australia.IMP, Env.CurrentCompany.Country.Code);
				entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				entryNumber.CE_EntryNum = "IMP1234";
				using (var testForm = new AUCustomsDeclarationFormForTest(declaration))
				{
					testForm.Show();
					AssertEquals("COLS TabPage is visible", true, testForm.CustomsBrokerageUserControl.COLSTabPage.TabVisible);
				}
			}
		}

		public void TestSwapingBetweenEdificeAndCMRMiscOptionsControl()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				AssertEquals("AUCMRMiscOptionsUserControl", typeof(AUCMRMiscOptionsUserControl), testForm.CustomsBrokerageUserControl.MiscOptions.GetType());
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				AssertEquals("AUEdificeMiscOptionsUserControl", typeof(AUEdificeMiscOptionsUserControl), testForm.CustomsBrokerageUserControl.MiscOptions.GetType());
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = "CMR";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				AssertEquals("AUCMRMiscOptionsUserControl", typeof(AUCMRMiscOptionsUserControl), testForm.CustomsBrokerageUserControl.MiscOptions.GetType());
			}
		}

		[TestDate(2004, 5, 5)]
		public void TestSwapingMiscOptionsControlWhenNotCMR()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				AssertEquals("AUEdificeMiscOptionsUserControl", typeof(AUEdificeMiscOptionsUserControl), testForm.CustomsBrokerageUserControl.MiscOptions.GetType());
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				AssertEquals("AUEdificeMiscOptionsUserControl", typeof(AUEdificeMiscOptionsUserControl), testForm.CustomsBrokerageUserControl.MiscOptions.GetType());
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				AssertEquals("AUEdificeMiscOptionsUserControl", typeof(AUEdificeMiscOptionsUserControl), testForm.CustomsBrokerageUserControl.MiscOptions.GetType());
			}
		}

		public void TestGetMiscOptionsUserControl()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertMiscTabContainsControlOfType("MiscOptionsUserControl should be AUCMRMiscOptionsUserControl 1", declaration, typeof(AUCMRMiscOptionsUserControl));
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertMiscTabContainsControlOfType("MiscOptionsUserControl should be AUEdificeMiscOptionsUserControl 2", declaration, typeof(AUEdificeMiscOptionsUserControl));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertMiscTabContainsControlOfType("MiscOptionsUserControl should be AUCMRMiscOptionsUserControl 3", declaration, typeof(AUCMRMiscOptionsUserControl));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertMiscTabContainsControlOfType("MiscOptionsUserControl should be AUEdificeMiscOptionsUserControl 4", declaration, typeof(AUEdificeMiscOptionsUserControl));
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			AssertMiscTabContainsControlOfType("MiscOptionsUserControl should be AUQuarantineEdificeMiscOptionsUserControl 5", declaration, typeof(AUQuarantineEdificeMiscOptionsUserControl));
		}

		public void TestCOLSTabPopupConfirmForCreatingCOLSEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Australia.IMP, Env.CurrentCompany.Country.Code);
			entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNumber.CE_EntryNum = "IMP1234";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var testForm = new ZForm(declaration))
			using (var userControl = new AUBrokerageUserControl())
			{
				testForm.Controls.Add(userControl);
				userControl.JobDeclaration = declaration;
				testForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				userControl.MainTabControl.SelectedTab = userControl.COLSTabPage;
				AssertNull("QuarantineCOLSHeader doesn't created", declaration.QuarantineCOLSHeader);
				AssertEquals("COLS entry user control is hidden", false, userControl.COLSTabPage.Controls["AUCOLSUserControl"].Visible);
				AssertEquals("COLS message user control is visible", true, userControl.COLSTabPage.Controls["AUCOLSEntryCreationMessageUserControl"].Visible);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				userControl.MainTabControl.SelectedTab = userControl.COLSTabPage;
				AssertNotNull("QuarantineCOLSHeader is created", declaration.QuarantineCOLSHeader);
				AssertEquals("COLS entry user control is visible", true, userControl.COLSTabPage.Controls["AUCOLSUserControl"].Visible);
				AssertEquals("COLS message user control is hidden", false, userControl.COLSTabPage.Controls["AUCOLSEntryCreationMessageUserControl"].Visible);
			}
		}

		void AssertMiscTabContainsControlOfType(ZString text, JobDeclaration declaration, Type requiredControlType)
		{
			bool foundCorrectType = false;
			using (ZForm form = new ZForm(declaration))
			using (AUBrokerageUserControl userControl = new AUBrokerageUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();
				userControl.LoadMiscOptionsUserControl();
				foreach (Control control in userControl.MiscOptionsTabPage.Controls)
				{
					if (control.GetType() == requiredControlType)
					{
						foundCorrectType = true;
						break;
					}
				}
			}

			Assert(text, foundCorrectType);
		}

		sealed class AUCustomsDeclarationFormForTest : ZAUCustomsDeclarationForm
		{
			public AUCustomsDeclarationFormForTest(JobDeclaration jobDeclaration) : base(jobDeclaration)
			{
			}

			public AUCustomsDeclarationFormForTest() : this(null)
			{
			}

			internal BaseCustomsDeclarationUserControl DeclarationUserControl => CustomsBrokerageUserControl.DeclarationUserControlForTesting;

			internal BaseCustomsSupplierHeaderUserControl SupplierUserControl => CustomsBrokerageUserControl.SupplierHeaderUserControl;
		}
	}
}
