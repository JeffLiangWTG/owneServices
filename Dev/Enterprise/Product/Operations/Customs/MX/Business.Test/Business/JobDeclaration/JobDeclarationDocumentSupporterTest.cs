using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	sealed class JobDeclarationDocumentSupporterTest : BaseJobDeclarationDocumentSupportTest
	{
		public void TestGetDocumentWrappersInternal()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var menu = Factory.New<IStmMenuItem>();

			menu.SU_MenuName = "Cove Draft";

			var docHeader = declaration.DocumentSupporter.GetDocumentWrappers(DataContext.CusEntryHeader, menu);
			var docInvoice = declaration.DocumentSupporter.GetDocumentWrappers(DataContext.ComInvoiceHeader, menu);
			var docDeclaration = declaration.DocumentSupporter.GetDocumentWrappers(DataContext.Declaration, menu);

			CombineAssertions(() =>
			{
				AssertEquals("Doc Header should contain ", 1, docHeader.Length);
				AssertType<DocCusEntryHeader>("Type of Doc Header should be ", docHeader[0]);
				AssertEquals("Doc Invoice should contain ", 1, docInvoice.Length);
				AssertType<DocJobComInvoiceHeader>("Type of Doc Invoice should be ", docInvoice[0]);
				AssertEquals("Doc Declaration should contain ", 1, docDeclaration.Length);
				AssertType<DocDeclaration>("Type of Doc Declaration should be ", docDeclaration[0]);
			});
		}

		public void TestGetSupportedDataContexts()
		{
			var declaration = Factory.New<JobDeclaration>();
			var documentSupporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var expectedSupportedDataContext = new DataContext[] { DataContext.CusEntryHeader };
			CombineAssertions("Expected Supported DataContexts", () =>
			{
				foreach (var dataContext in expectedSupportedDataContext)
				{
					Assert($"{dataContext} should be supported", documentSupporter.IsDataContextSupported(new DataContextValueForTesting(dataContext)));
				}
			});
		}

		protected override ZString GetMainNameSpace() => "Enterprise.Customs.MX.Business.";

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForRunningDocumentsTest
		{
			get
			{
				yield return (JobDeclaration)GetDocumentSupportableBusinessObject();
			}
		}
		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			return declaration;
		}
	}
}
