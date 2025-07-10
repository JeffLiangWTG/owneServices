using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.AU;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WowDocDeclaration))]
	public class WowDocDeclarationTest : DocBaseJobDeclarationAbstractTest<WoolworthsJobDeclaration, WowDocDeclaration>
	{
		public void TestSuperTypeOverridden()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			declaration.JE_DeclarationReference = "decl123";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new Enterprise.Customs.AU.Declaration.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			DocDeclaration newDoc = (DocDeclaration)declaration.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.DeclarationWithCusEntryHeaders, null)[0];
			Assert("Constructed doc wrapper of correct overridden type", newDoc is WowDocDeclaration);
		}

		public void TestAllInvoiceLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Description = "InvoiceLine1";
			invoiceLine2.JI_Description = "InvoiceLine2";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new Enterprise.Customs.AU.Declaration.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			WowDocDeclaration docDeclaration = (WowDocDeclaration)WowDocDeclaration.New(declaration, Factory);
			WowDocJobComInvoiceLine docInvoiceLine1 = docDeclaration.AllInvoiceLines[0];
			WowDocJobComInvoiceLine docInvoiceLine2 = docDeclaration.AllInvoiceLines[1];
			AssertEquals("Should have 2 invoice lines", 2, docDeclaration.AllInvoiceLines.Count);
			AssertEquals("InvoiceLine1", docInvoiceLine1.Description);
			AssertEquals("InvoiceLine2", docInvoiceLine2.Description);
		}

		public void TestCalculateCharge()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job.JH_JobNum = "123";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AccChargeCode consolChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			consolChargeCode.AC_Code = "CONSOL";
			JobCharge consolCharge = job.Charges.AddNew();
			consolCharge.JR_AC = consolChargeCode.PK;
			consolCharge.JR_LocalCostAmt = 9m;
			Factory.Save();
			WowDocDeclaration docDeclaration = (WowDocDeclaration)WowDocDeclaration.New(declaration, Factory);
			AssertEquals("ConsolCharges correct", "9", docDeclaration.ConsolCharges);
		}

		public void TestCalculateCustomRelatedCharges()
		{
			JobDeclaration declaration = TestHelper.CreateJobDecWithJobCharges(Factory);
			WowDocDeclaration docDeclaration = (WowDocDeclaration)WowDocDeclaration.New(declaration, Factory);
			AssertEquals("Origin Charges", 9.45m, docDeclaration.OriginCharges);
			AssertEquals("Destination Charges", 11m, docDeclaration.DestinationCharges);
			AssertEquals("Other Import Charges", 12m, docDeclaration.OtherImportCharges);
			AssertEquals("quarantine Charges", 13m, docDeclaration.QuarantineDetentionFumigationCharges);
			AssertEquals("detention Charges", 14m, docDeclaration.DetentionCharges);
		}

		#region Implementation

		protected override WowDocDeclaration CreateDeclarationWrapper(WoolworthsJobDeclaration declaration)
		{
			var result = (WowDocDeclaration)WowDocDeclaration.New(declaration, Factory);
			((IBODocDataProvider)result).SetDocWrapperContext(new Dictionary<string, object>()); // Will call through to OnDocWrappersContextSet().
			result.SetReportNameForTesting("Report Name");
			return result;
		}

		protected override void SetupDeclarationForDeclarationNumberTests(ZString declarationNumber)
		{
			Declaration.DeclarationNumber = declarationNumber;
		}

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Australia; }
		}

		WowTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new WowTestHelper());
			}
		}

		WowTestHelper testHelper;
		#endregion
	}
}
