using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	public class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		public void TestTypedIndexer()
		{
			InvoiceLineCompleteCollection collection = new InvoiceLineCompleteCollection(Declaration);
			JobComInvoiceLine invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceLineCompleteCollection(Declaration);
		}

		protected new JobDeclaration Declaration
		{
			get
			{
				return (JobDeclaration)base.Declaration;
			}
		}

		protected override BaseJobDeclaration GetMeANewJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		public void TestSetDefaultsForNewChild()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration1.Invoices.AddNew();
			var lineCollection1 = new InvoiceLineCompleteCollection(declaration1);
			var invoiceLine1 = lineCollection1.AddNew();
			Assert("DrawbackModality should be empty", invoiceLine1.DrawbackModality.IsEmpty);
			AssertEquals("JI_PrimaryPreference default to NORMAL", Constants.RatePreferenceType.Normal, invoiceLine1.JI_PrimaryPreference);

			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoiceLine2 = lineCollection1.AddNew();
			AssertEquals("DrawbackModality should be", DrawbackModalityList.Codes.NoDrawback, invoiceLine2.DrawbackModality);
			Assert("JI_PrimaryPreference must be empty", invoiceLine2.JI_PrimaryPreference.IsEmpty);

			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine3 = lineCollection1.AddNew();
			Assert("DrawbackModality should be empty", invoiceLine1.DrawbackModality.IsEmpty);
			AssertEquals("JI_PrimaryPreference default to NORMAL", Constants.RatePreferenceType.Normal, invoiceLine3.JI_PrimaryPreference);
		}

		public void TestSetDefaultFromPreviousLine()
		{
			var destinationDeclaration = Factory.New<JobDeclaration>();
			destinationDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_LIC";
			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			entryheader.CH_CEI_Instruction = entryInstruction.PK;
			entryheader.MovementReferenceNumberSetter("2000010000");
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			destinationDeclaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(entryInstruction) });

			var destinationHeader1 = destinationDeclaration.Invoices.AddNew();
			var destinationHeader2 = destinationDeclaration.Invoices.AddNew();
			var destinationLineCollection1 = new InvoiceLineCompleteCollection(destinationDeclaration);

			var destinationInvoiceLine1 = destinationLineCollection1.AddNew();
			destinationInvoiceLine1.JI_JZ = destinationHeader1.PK;
			var destinationInvoiceLine2 = destinationLineCollection1.AddNew();
			var destinationInvoiceLine3 = destinationLineCollection1.AddNew();
			destinationInvoiceLine3.JI_JZ = destinationHeader2.PK;
			AssertEquals("destinationInvoiceLine1.JI_JZ must be equal to destinationHeader1.PK", destinationHeader1.PK, destinationInvoiceLine1.JI_JZ);
			AssertEquals("destinationInvoiceLine2.JI_JZ must be equal to destinationHeader1.PK", destinationHeader1.PK, destinationInvoiceLine2.JI_JZ);
			AssertEquals("destinationInvoiceLine3.JI_JZ must be equal to destinationHeader2.PK", destinationHeader2.PK, destinationInvoiceLine3.JI_JZ);
		}

		public void TestAllowNew()
		{
			var destinationDeclaration = Factory.New<JobDeclaration>();
			destinationDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_LIC";
			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			entryheader.CH_CEI_Instruction = entryInstruction.PK;
			entryheader.MovementReferenceNumberSetter("2000010000");
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			destinationDeclaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(entryInstruction) });
			var destinationLineCollection = new InvoiceLineCompleteCollection(destinationDeclaration);

			Assert("destinationLineCollection must be true", destinationLineCollection.AllowNew);

			destinationDeclaration.Invoices.AddNew();
			destinationLineCollection.AddNew();
			Assert("destinationLineCollection must be True", destinationLineCollection.AllowNew);
		}

		public void TestOnLoadedGetKeyForLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var lineCollection = new InvoiceLineCompleteCollection(declaration);
			var invoiceLine = lineCollection.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_CEI = entryInstruction.PK;
			lineCollection.Load();
			AssertNotNull(invoiceLine.LastMergeKeyForImportLicenseEntry);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			invoiceLine.LastMergeKeyForImportLicenseEntry = null;
			lineCollection.Load();
			AssertNull(invoiceLine.LastMergeKeyForImportLicenseEntry);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.CustomsEntryHeaders.DeleteAll();
			invoiceLine.LastMergeKeyForImportLicenseEntry = null;
			lineCollection.Load();
			AssertNull(invoiceLine.LastMergeKeyForImportLicenseEntry);
		}
	}
}
