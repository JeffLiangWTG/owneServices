using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	class CustomsDocDataObjectProviderTest : TestCaseWithFactory
	{
		public void TestDV1FormIsTranslatedToFrench()
		{
			var french = Res.GetLanguageInstance(Core.SharedConstants.Languages.French);
			var frenchMock = french.UseMockData();
			frenchMock.Put("1516DFFF-4D7E-4F4A-8AB7-48F02D142261", new ResourceStringData("1516DFFF-4D7E-4F4A-8AB7-48F02D142261", "I am first French translation: 1  NAME AND ADDRESS OF THE SELLER"));
			frenchMock.Put("1A57DD7D-78D4-40E7-87F9-2EDB7C67650F", new ResourceStringData("1A57DD7D-78D4-40E7-87F9-2EDB7C67650F", "I am second French translation: 18 Total B in NATIONAL CURRENCY"));

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var provider = new CustomsDocDataObjectProvider();

			var dataObject = provider.GetDocDataObject(entry, Enterprise.DocumentVisualizer.Integration.DataContext.DV1Certificate, new DocDataObjectParameters("D.V.1", "DV1"));
			AssertDocDataObject(dataObject);

			void AssertDocDataObject(object docDataObject)
			{
				var dV1DocDataObject = (DV1DocDataObject)docDataObject;
				var entryDocDataObject = dV1DocDataObject.Entries.Single();
				AssertEquals("I am first French translation: 1  NAME AND ADDRESS OF THE SELLER", entryDocDataObject.DV1Box1Caption);
				var entryLineGroupDocDataObject = entryDocDataObject.EntryLineGroups.Single();
				AssertEquals("I am second French translation: 18 Total B in NATIONAL CURRENCY", entryLineGroupDocDataObject.DV1Box18Caption);
			}
		}

		public void TestGetEntryHeaderReturnCAED()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entry.CH_BGMReference = "entryHeaderRef";
			var parameters = new CustomsDocDataObjectProviderParametersTest { };
			var customsDocDataObjectProvider = new CustomsDocDataObjectProvider();

			var dataObject = customsDocDataObjectProvider.GetDocDataObject(entry, DataContext.FRPortsCustomsCheckCAED, parameters);
			AssertType<CAEDDataObject>(dataObject);
			var caed = (CAEDDataObject)dataObject;
			AssertEquals("We should call CAEDBuilder.Build to populate the CAED object.", "entryHeaderRef", caed.JobNumber);
		}
	}
}
