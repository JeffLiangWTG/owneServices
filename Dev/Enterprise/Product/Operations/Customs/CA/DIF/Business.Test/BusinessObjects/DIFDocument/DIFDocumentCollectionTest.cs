using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DIF.Business.Testing
{
	[TestedType(typeof(DIFDocumentCollection))]
	sealed class DIFDocumentCollectionTest : DISDocumentCollectionBaseTest<DIFDocumentCollection, DIFDocument>
	{
		public void TestCreateNonPersistentBusinessObject()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = new TestHelper(Factory).GetJobDeclaration();
			((BaseJobDeclaration)declaration).JE_OH_Importer = importer.PK;
			var requiredDocument1 = ((ICADIFHost)declaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();

			importer.CustomsCodes.AddNew("AID", "AID1000001", "CA");
			Factory.Save();

			var hostWrapper = new DIFHostWrapper((ICADIFHost)declaration);
			var difDocument = hostWrapper.DISDocuments.AddNew();

			AssertEquals(ZString.Empty, difDocument.BusinessNumber);

			importer.CustomsCodes.AddNew("BRM", "BRM1000001", "CA");
			Factory.Save();

			var difDocument2 = hostWrapper.DISDocuments.AddNew();

			AssertEquals("BRM1000001", difDocument2.BusinessNumber);
		}

		protected override IDISHost GetDISHost() => JobDeclaration as IDISHost;

		protected override string xml1 => @"<DIFDocument xmlns=""http://www.cargowise.com/Schemas/DIFDocument""><DocumentDescription>ABC</DocumentDescription></DIFDocument>";

		protected override string xml2 => @"<DIFDocument xmlns=""http://www.cargowise.com/Schemas/DIFDocument""><DocumentDescription>DEF</DocumentDescription></DIFDocument>";

		protected override string ApplicationCode => Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DIFDocument(HostWrapper);

		protected override DIFDocumentCollection GetCollectionToTest() => new DIFDocumentCollection(HostWrapper);

		DIFHostWrapper hostWrapper;
		DIFHostWrapper HostWrapper => hostWrapper ?? (hostWrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration));

		BusinessObject jobDeclaration;
		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
