using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	class CopyAndSendToCustomsJobDeclarationCopierTest : TestCaseWithFactory
	{
		[TestDate(2022, 5, 15, 16, 45, 00)]
		public void TestDeclarationCopy()
		{
			var originalDeclaration = declaration;
			const string date = "202205151645";

			using (var userContext = Env.SetTemporaryUserContext(testDeveloperUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var copiedDeclarations = CopyAndSendToCustomsJobDeclarationCopier.CreateCopy(3, ZInt.Zero, originalDeclaration);
				CombineAssertions(() =>
				{
					var countCopies = 0;
					foreach (var copiedDeclaration in copiedDeclarations)
					{
						countCopies++;

						AssertEquals("JE_DeclarationReference", "AB" + date + countCopies, copiedDeclaration.JE_DeclarationReference);
						AssertEquals("JE_MasterBill", "MBAB" + date + countCopies, copiedDeclaration.JE_MasterBill);
						AssertEquals("JE_HouseBill", "HBAB" + date + countCopies, copiedDeclaration.JE_HouseBill);
						AssertEquals("JE_OwnerRef", "ORAB" + date + countCopies, copiedDeclaration.JE_OwnerRef);

						AssertEquals("JE_OwnerRef", "VESSEL", copiedDeclaration.JE_VesselName);
						AssertEquals("JE_OwnerRef", "VOYAGE", copiedDeclaration.JE_VoyageFlightNo);
						AssertEquals("JE_OwnerRef", "LLOYDS", copiedDeclaration.JE_LloydsIMO);

						AssertEquals("Invoices[0].JZ_InvoiceNumber", "INVAB" + date + countCopies + "1", copiedDeclaration.Invoices[0].JZ_InvoiceNumber);
						AssertEquals("Invoices[1].JZ_InvoiceNumber", "INVAB" + date + countCopies + "2", copiedDeclaration.Invoices[1].JZ_InvoiceNumber);

						AssertEquals("The first invoice has the same number of lines as the original declaration", originalDeclaration.Invoices[0].InvoiceLines.Count, copiedDeclaration.Invoices[0].InvoiceLines.Count);
						AssertEquals("The second invoice has the same number of lines as the original declaration", originalDeclaration.Invoices[1].InvoiceLines.Count, copiedDeclaration.Invoices[1].InvoiceLines.Count);
					}
					AssertEquals("Number of copies created is correct", 3, countCopies);
				});
			}
		}

		[TestDate(2022, 5, 15, 16, 45, 00)]
		public void TestDeclarationCopyWithInvoiceLinesCopiedMultipleTimes()
		{
			var originalDeclaration = declaration;
			const string date = "202205151645";
			const int invoiceLineNumberOfCopies = 10;

			using (var userContext = Env.SetTemporaryUserContext(testDeveloperUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var copiedDeclarations = CopyAndSendToCustomsJobDeclarationCopier.CreateCopy(3, invoiceLineNumberOfCopies, originalDeclaration);
				CombineAssertions(() =>
				{
					var countCopies = 0;
					foreach (var copiedDeclaration in copiedDeclarations)
					{
						countCopies++;

						AssertEquals("JE_DeclarationReference", "AB" + date + countCopies, copiedDeclaration.JE_DeclarationReference);
						AssertEquals("JE_MasterBill", "MBAB" + date + countCopies, copiedDeclaration.JE_MasterBill);
						AssertEquals("JE_HouseBill", "HBAB" + date + countCopies, copiedDeclaration.JE_HouseBill);
						AssertEquals("JE_OwnerRef", "ORAB" + date + countCopies, copiedDeclaration.JE_OwnerRef);

						AssertEquals("JE_OwnerRef", "VESSEL", copiedDeclaration.JE_VesselName);
						AssertEquals("JE_OwnerRef", "VOYAGE", copiedDeclaration.JE_VoyageFlightNo);
						AssertEquals("JE_OwnerRef", "LLOYDS", copiedDeclaration.JE_LloydsIMO);

						var copiedInvoiceHeader = copiedDeclaration.Invoices[0];
						AssertEquals("Invoices[0].JZ_InvoiceNumber", "INVAB" + date + countCopies + "1", copiedInvoiceHeader.JZ_InvoiceNumber);
						AssertEquals("Invoices[1].JZ_InvoiceNumber", "INVAB" + date + countCopies + "2", copiedDeclaration.Invoices[1].JZ_InvoiceNumber);

						AssertEquals("The first invoice has x number of invoice lines", invoiceLineNumberOfCopies, copiedDeclaration.Invoices[0].InvoiceLines.Count);

						foreach (JobComInvoiceLine invoiceLine in copiedInvoiceHeader.InvoiceLines)
						{
							AssertEquals("Invoice line has 1 Supporting Document", 1, invoiceLine.SupportingDocuments.Count);
							AssertEquals("Supporting Document CSI_Code", "111", invoiceLine.SupportingDocuments[0].CSI_Code);
							AssertEquals("Invoice line has 1 Previous Document", 1, invoiceLine.PreviousDocuments.Count);
							AssertEquals("Previous Document CSI_Code", "222", invoiceLine.PreviousDocuments[0].CSI_Code);
							AssertEquals("Invoice line has 1 Additional Info", 1, invoiceLine.AdditionalInfos.Count);
							AssertEquals("Additional Info CSI_Code", "333", invoiceLine.AdditionalInfos[0].CSI_Code);
						}

						AssertEquals("The second invoice has the same number of lines  as the original declaration", originalDeclaration.Invoices[1].InvoiceLines.Count, copiedDeclaration.Invoices[1].InvoiceLines.Count);
					}
					AssertEquals("Number of copies created is correct", 3, countCopies);
				});
			}
		}

		protected override void SetUp()
		{
			testDeveloperUser = (IGlbStaff)Factory.New(ObjectFactory.GetType<IGlbStaff>());
			testDeveloperUser.GS_Code = "AB";
			testDeveloperUser.GS_LoginName = "tdu";
			testDeveloperUser.GS_FullName = "Test Developer User";
			testDeveloperUser.GS_EmailAddress = "tdu@cw1.com";
			Factory.Save();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_VesselName = "VESSEL";
			declaration.JE_VoyageFlightNo = "VOYAGE";
			declaration.JE_LloydsIMO = "LLOYDS";
			declaration.JE_HouseBill = "HB";
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
			_ = invoiceHeader2.JobComInvoiceLines.AddNew();

			var invLineSupDoc = invoiceLine.SupportingDocuments.AddNew();
			invLineSupDoc.CSI_Code = "111";
			invLineSupDoc.CSI_ReferenceNumber = "111";

			var invLinePrevDoc = invoiceLine.PreviousDocuments.AddNew();
			invLinePrevDoc.CSI_Code = "222";
			invLinePrevDoc.CSI_ReferenceNumber = "222";

			var invLineDoc = invoiceLine.AdditionalInfos.AddNew();
			invLineDoc.CSI_Code = "333";
			invLineDoc.CSI_ReferenceNumber = "333";
		}
		JobDeclaration declaration;
		IGlbStaff testDeveloperUser;
	}
}
