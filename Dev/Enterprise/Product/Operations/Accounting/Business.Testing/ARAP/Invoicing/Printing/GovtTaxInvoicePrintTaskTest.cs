using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing.Testing
{
	public class GovtTaxInvoicePrintTaskTest : TestCaseWithFactory
	{
		public void TestMenuPK()
		{
			var peruBranch = Factory.NewWithValidTestData<GlbBranch>();
			peruBranch.GB_RL_NKHomePort = "PEALD";
			var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "ARInvoice PE Factura")).PK;
			var sequencePeruTXI = TestObjectCreator.SetupComplianceSequence(menuPK, "TXI", "AAA", 1, 100, 1, peruBranch.Company.PK, peruBranch.PK);

			Factory.Save();

			var invoiceTask = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(ZGuid.Empty));

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), peruBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Peru))
			{
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				invoice1.AH_ComplianceSubType = "TXI";
				invoice1.AH_TransactionReference = "AAA000000025";
				invoice1.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("MenuPK should be to the menu setup in the compliance book", sequencePeruTXI.XD_SU_MenuItem, invoiceTask.GetMenuPK(invoice1));
			}
		}

		public void TestMenuName()
		{
			var peruBranch = Factory.NewWithValidTestData<GlbBranch>();
			peruBranch.GB_RL_NKHomePort = "PEALD";
			var vietnamBranch = Factory.NewWithValidTestData<GlbBranch>();
			vietnamBranch.GB_RL_NKHomePort = "VNBCD";
			var chinaBranch = Factory.NewWithValidTestData<GlbBranch>();
			chinaBranch.GB_RL_NKHomePort = "CNSHA";

			var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "ARInvoice PE Factura")).PK;
			var sequencePeruTXI = TestObjectCreator.SetupComplianceSequence(menuPK, "TXI", "AAA", 1, 100, 1, peruBranch.Company.PK, peruBranch.PK);

			menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "VN Govt Tax Invoice")).PK;
			var sequenceVietnamTXI = TestObjectCreator.SetupComplianceSequence(menuPK, "TXI", "BBB", 1000, 2000, 1000, vietnamBranch.Company.PK, vietnamBranch.PK);

			menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Class A Invoice Preprinted")).PK;
			var sequenceChinaTXI = TestObjectCreator.SetupComplianceSequence(menuPK, "TXI", "CCC", 10000, 20000, 10000, chinaBranch.Company.PK, chinaBranch.PK);

			Factory.Save();

			var invoiceTask = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(ZGuid.Empty));

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), peruBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Peru))
			{
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				invoice1.AH_ComplianceSubType = "TXI";
				invoice1.AH_TransactionReference = "AAA000000025";
				invoice1.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("MenuName", "ARInvoice PE Factura", invoiceTask.GetMenuNames(invoice1)[0]);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), vietnamBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			{
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				invoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				invoice1.AH_TransactionReference = "BBB000001025";
				AssertEquals("MenuName", "VN Govt Tax Invoice", invoiceTask.GetMenuNames(invoice1)[0]);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), chinaBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				invoice1.AH_TransactionReference = "CCC000010025";
				AssertEquals("MenuName", "Class A Invoice Preprinted", invoiceTask.GetMenuNames(invoice1)[0]);
			}
		}

		public void TestAddInvoicesToPackWithMultipleInvoices()
		{
			var peruBranch = Factory.NewWithValidTestData<GlbBranch>();
			peruBranch.GB_RL_NKHomePort = "PEALD";

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "ARInvoice PE Factura")).PK;
			var sequencePeruTXI = TestObjectCreator.SetupComplianceSequence(menuPK, "TXI", "AAA", 1, 100, 1, peruBranch.Company.PK, peruBranch.PK);
			menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "ARInvoice_PE_Nota_De_Credito")).PK;
			var sequencePeruTCR = TestObjectCreator.SetupComplianceSequence(menuPK, "TCR", "BBB", 200, 300, 200, peruBranch.Company.PK, peruBranch.PK);
			menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "ARInvoice_PE_Nota_De_Debito")).PK;
			var sequencePeruTCD = TestObjectCreator.SetupComplianceSequence(menuPK, "TCD", "CCC", 500, 600, 500, peruBranch.Company.PK, peruBranch.PK);
			menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "VN Govt Tax Invoice")).PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), peruBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var classAInv1 = Factory.NewWithValidTestData<ARInvoice>();
				classAInv1.AH_OH = org.PK;
				classAInv1.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
				classAInv1.AH_TransactionReference = "AAA000000025";
				classAInv1.Branch.Company.SetCountry(Core.Constants.CountryCodes.Peru);

				var classAInv2 = Factory.NewWithValidTestData<ARInvoice>();
				classAInv2.AH_OH = org.PK;
				classAInv2.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCR;
				classAInv2.AH_TransactionReference = "BBB000000225";
				classAInv2.Branch.Company.SetCountry(Core.Constants.CountryCodes.Peru);

				var classAInv3 = Factory.NewWithValidTestData<ARInvoice>();
				classAInv3.AH_OH = org.PK;
				classAInv3.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCD;
				classAInv3.AH_TransactionReference = "CCC000000525";
				classAInv3.Branch.Company.SetCountry(Core.Constants.CountryCodes.Peru);

				Factory.Save();

				var peruGovtTaxInvoiceTask = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv1.PK));
				peruGovtTaxInvoiceTask.AddInvoices_ForTestOnly(classAInv1);
				AssertNotNull("Govt Tasx Invoice Pack is not null", peruGovtTaxInvoiceTask);
				Assert("Govt Tasx Invoice Pack has value", peruGovtTaxInvoiceTask.TaskCount > 0);
				Assert("Govt Tasx Invoice Pack has task", peruGovtTaxInvoiceTask[0].Count > 0);
				AssertEquals("ARInvoice PE Factura", peruGovtTaxInvoiceTask[0][0].Name);

				peruGovtTaxInvoiceTask = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv2.PK));
				peruGovtTaxInvoiceTask.AddInvoices_ForTestOnly(classAInv2);
				AssertNotNull("Govt Tasx Invoice Pack is not null", peruGovtTaxInvoiceTask);
				Assert("Govt Tasx Invoice Pack has value", peruGovtTaxInvoiceTask.TaskCount > 0);
				Assert("Govt Tasx Invoice Pack has task", peruGovtTaxInvoiceTask[0].Count > 0);
				AssertEquals("ARInvoice_PE_Nota_De_Credito", peruGovtTaxInvoiceTask[0][0].Name);

				peruGovtTaxInvoiceTask = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv3.PK));
				peruGovtTaxInvoiceTask.AddInvoices_ForTestOnly(classAInv3);
				AssertNotNull("Govt Tasx Invoice Pack is not null", peruGovtTaxInvoiceTask);
				Assert("Govt Tasx Invoice Pack has value", peruGovtTaxInvoiceTask.TaskCount > 0);
				Assert("Govt Tasx Invoice Pack has task", peruGovtTaxInvoiceTask[0].Count > 0);
				AssertEquals("ARInvoice_PE_Nota_De_Debito", peruGovtTaxInvoiceTask[0][0].Name);
			}
		}

		public void TestAddInvoicesToPack_LegacyInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			TestAddInvoicesToPack_Core("Invoice");
		}

		public void TestAddInvoicesToPack_DocBuilderInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestAddInvoicesToPack_Core("DocBuilder Invoice");
		}

		void TestAddInvoicesToPack_Core(string sU_MenuName)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var classAInv = Factory.NewWithValidTestData<ARInvoice>();
			classAInv.AH_OH = org.PK;
			classAInv.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			Factory.Save();

			var previousRegistry = AccountingConfigurationRegistry.Instance.InvoicePrintingOption.Value;

			try
			{
				//No print context i.e. don't care
				AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.GovtTaxInvoice);
				var task = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv.PK));
				AssertEquals("There should be 1 pack", 1, task.TaskCount);
				AssertEquals("No context passed in which i.e. Context = Don't Care therefore the pack should be for Govt Tax Invoice", task.GetMenuNames(classAInv)[0], task[0].StmMenuCommand.SU_MenuName);

				AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.EnterpriseInvoice);
				task = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv.PK));
				AssertEquals("There should be 1 pack", 1, task.TaskCount);
				AssertEquals("No context passed in which i.e. Context = Don't Care therefore the pack should be for Invoice", sU_MenuName, task[0].StmMenuCommand.SU_MenuName);

				AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);
				task = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv.PK));
				AssertEquals("There should be 1 pack", 1, task.TaskCount);
				AssertEquals("There should be 2 reports in the pack", 2, task[0].Count);
				Assert("Registry item should be used. Class A Invoice deliverable should be contained in the pack", AreMenuNamesContainedInPack(task[0], task.GetMenuNames(classAInv)));
				Assert("Registry item should be used. Enterprise Invoice deliverable should be contained in the pack", AreMenuNamesContainedInPack(task[0], sU_MenuName));

				AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.GovtTaxInvoice);
				task = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv.PK) { InvoicePrintingOptionCode = GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice });
				AssertEquals("There should be 1 pack", 1, task.TaskCount);
				AssertEquals("There should be 2 reports in the pack", 2, task[0].Count);
				Assert("Registry item should be used. Class A Invoice deliverable should be contained in the pack", AreMenuNamesContainedInPack(task[0], task.GetMenuNames(classAInv)));
				Assert("Registry item should be used. Enterprise Invoice deliverable should be contained in the pack", AreMenuNamesContainedInPack(task[0], sU_MenuName));

				AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.EnterpriseInvoice);
				task = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv.PK) { InvoicePrintingOptionCode = GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice });
				AssertEquals("There should be 1 pack", 1, task.TaskCount);
				AssertEquals("There should be 2 reports in the pack", 2, task[0].Count);
				Assert("Registry item should be used. Class A Invoice deliverable should be contained in the pack", AreMenuNamesContainedInPack(task[0], task.GetMenuNames(classAInv)));
				Assert("Registry item should be used. Enterprise Invoice deliverable should be contained in the pack", AreMenuNamesContainedInPack(task[0], sU_MenuName));

				AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);
				task = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv.PK) { InvoicePrintingOptionCode = GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice });
				AssertEquals("There should be 1 pack", 1, task.TaskCount);
				AssertEquals("There should be 2 reports in the pack", 2, task[0].Count);
				Assert("Registry item should be used. Class A Invoice deliverable should be contained in the pack", AreMenuNamesContainedInPack(task[0], task.GetMenuNames(classAInv)));
				Assert("Registry item should be used. Enterprise Invoice deliverable should be contained in the pack", AreMenuNamesContainedInPack(task[0], sU_MenuName));

				task = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv.PK) { InvoicePrintingOptionCode = GovtTaxInvoicePrintTask.GovtTaxInvoice });
				AssertEquals("There should be 1 pack", 1, task.TaskCount);
				AssertEquals("No context passed in which i.e. Context = Don't Care therefore the pack should be for Govt Tax Invoice", task.GetMenuNames(classAInv)[0], task[0].StmMenuCommand.SU_MenuName);

				task = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv.PK) { InvoicePrintingOptionCode = GovtTaxInvoicePrintTask.EnterpriseInvoice });
				AssertEquals("There should be 1 pack", 1, task.TaskCount);
				AssertEquals("No context passed in which i.e. Context = Don't Care therefore the pack should be for Invoice", sU_MenuName, task[0].StmMenuCommand.SU_MenuName);

				//Post from billing context
				AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.GovtTaxInvoice);
				task = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv.PK) { Context = InvoicePrintContext.PostFromBilling });
				AssertEquals("There should be 1 pack", 1, task.TaskCount);
				AssertEquals("Context = Post from bililng so registry item should be used.  The pack should be for Govt Tax Invoice", task.GetMenuNames(classAInv)[0], task[0].StmMenuCommand.SU_MenuName);

				AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.EnterpriseInvoice);
				task = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv.PK) { Context = InvoicePrintContext.PostFromBilling });
				AssertEquals("There should be 1 pack", 1, task.TaskCount);
				AssertEquals("Context = Post from bililng so registry item should be used. The pack should be for Enterprise Invoice", sU_MenuName, task[0].StmMenuCommand.SU_MenuName);

				AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);
				task = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(classAInv.PK) { Context = InvoicePrintContext.PostFromBilling });
				AssertEquals("There should be 1 pack", 1, task.TaskCount);
				AssertEquals("There should be 2 reports in the pack", 2, task[0].Count);
				Assert("Context = Post from bililng so registry item should be used. Class A Invoice deliverable should be contained in the pack", AreMenuNamesContainedInPack(task[0], task.GetMenuNames(classAInv)));
				Assert("Context = Post from bililng so registry item should be used. Enterprise Invoice deliverable should be contained in the pack", AreMenuNamesContainedInPack(task[0], sU_MenuName));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, previousRegistry);
			}
		}

		bool AreMenuNamesContainedInPack(DocumentPack pack, params ZString[] menuNames)
		{
			bool areAllMenuContained = false;
			foreach (ZString menuName in menuNames)
			{
				areAllMenuContained = true;
				bool isMenuContained = false;
				foreach (IDeliverable deliverable in pack)
				{
					if ((deliverable.MenuItem as DocumentCommand).SU_MenuName == menuName)
					{
						isMenuContained = true;
					}
				}
				if (!isMenuContained)
				{
					areAllMenuContained = false;
				}
			}
			return areAllMenuContained;
		}

		public void TestCheckIndonesianConstraints()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_GSTAmount = 10m;

			AssertEquals("", GovtTaxInvoicePrintTask.CheckIndonesianConstraints(invoice));

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);

				AssertEquals("", GovtTaxInvoicePrintTask.CheckIndonesianConstraints(invoice));

				invoice.AH_TransactionType = TransactionTypes.CreditNote;
				AssertEquals("Transaction must be an Invoice.", GovtTaxInvoicePrintTask.CheckIndonesianConstraints(invoice));

				invoice.AH_TransactionType = TransactionTypes.Invoice;
				invoice.AH_GSTAmount = 0m;
				AssertEquals("Transaction must have an amount of tax.", GovtTaxInvoicePrintTask.CheckIndonesianConstraints(invoice));

				invoice.AH_GSTAmount = 10m;
				invoice.AH_IsCancelled = true;
				invoice.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
				AssertEquals("Reversal transaction may not be updated.", GovtTaxInvoicePrintTask.CheckIndonesianConstraints(invoice));

				invoice.AH_TransactionBelongsToGroup = ZGuid.Empty;
				AssertEquals("Reversed transaction with empty government compliance number may not be updated.", GovtTaxInvoicePrintTask.CheckIndonesianConstraints(invoice));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;
	}
}
