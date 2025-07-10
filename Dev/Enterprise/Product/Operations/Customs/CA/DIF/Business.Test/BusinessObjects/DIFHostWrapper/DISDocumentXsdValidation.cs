using System;
using System.Data;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CA.DIF.Business.Testing
{
	sealed class DISDocumentXsdValidation : DISDocumentXsdValidationBase<DIFDocument>
	{
		protected override string GetExpectedXmlData() => string.Format(resourceRetriever.GetString(EmbeddedResourcePrefix + "DIFDocument.xml"), eDocsUniqueKey);
		ZGuid eDocsUniqueKey;

		protected override string GetExpectedXsdData() => resourceRetriever.GetString(EmbeddedResourcePrefix + "DIFDocument.xsd");

		protected override XmlSerializableNonPersistentBusinessObject GetFullyPopulatedBizObj()
		{
			var declaration = new TestHelper(Factory).GetJobDeclaration();
			var disHost = (IDISHost)declaration;
			var hostWrapper = GetDISHostWrapper(disHost);
			var requiredDocument = disHost.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocNumber = "DN21";
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			var difDocument = hostWrapper.DISDocuments.AddNew();
			difDocument.BusinessNumber = "BN0000010";
			difDocument.RequiredDocumentPK = requiredDocument.PK;
			difDocument.PGA = "HC";
			difDocument.DocumentType = "12345";
			difDocument.DocumentDescription = "desc";
			eDocsUniqueKey = eDocs.UniqueKey;
			difDocument.EDocsDocumentPK = eDocsUniqueKey;
			difDocument.Comment = "Comment";
			difDocument.DocumentNumber = "MaximumLength70";
			difDocument.EffectiveDate = new ZDateTime(2013, 12, 25, 12, 25, 25);
			difDocument.ExpiryDate = new ZDateTime(2014, 12, 25, 12, 25, 25);
			difDocument.Messages.AddNew(typeof(TestHelperEDIMessage));

			return difDocument;
		}

		protected override string XsdNamespace => DIFDocument.Constants.XmlNamespace;

		protected override XmlSerializableNonPersistentBusinessObject GetEmptyBizObj()
		{
			var declaration = new TestHelper(Factory).GetJobDeclaration();
			var disHost = (IDISHost)declaration;
			var hostWrapper = GetDISHostWrapper(disHost);

			var difDocument = hostWrapper.DISDocuments.AddNew();
			difDocument.PGA = "CBSA";
			difDocument.DocumentType = "67890";
			difDocument.Messages.AddNew(typeof(TestHelperEDIMessage));

			return difDocument;
		}

		protected override string GetExpectedEmptyXmlData() => resourceRetriever.GetString(EmbeddedResourcePrefix + "DIFDocumentWithMinimumData.xml");

		protected override DISHostWrapperBase<DIFDocument> GetDISHostWrapper(IDISHost disHost) => new DIFHostWrapper(disHost as ICADIFHost);

		protected override DIFDocument GetDISDocument(DISHostWrapperBase<DIFDocument> hostWrapper) => new DIFDocument(hostWrapper as DIFHostWrapper);

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever();
			ObjectFactory.Get<Integration.Customs.CA.ICACustomsDataRegistry>().AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			var connection = ((IDbConnected)Factory).Connection;
			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("GeneratorFountain-C-8E86F7C5", Guid.Empty);
				ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("GeneratorFountain-C-7BE2F10C", Guid.Empty);

				var companyFountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber12345");
				companyFountain.SetNext(Factory, 100);
				var difFountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber12345DIF");
				difFountain.SetNext(Factory, 777);
				transactionManager.CommitTransaction();
			}
		}
		EmbeddedResourceRetriever resourceRetriever;

		const string EmbeddedResourcePrefix = "Enterprise.Customs.CA.DIF.Business.Testing.BusinessObjects.DIFHostWrapper.TestFiles.";

		sealed class TestHelperEDIMessage : EDIMessage, IObsoleteValidation
		{
			public TestHelperEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
			{
				//do nothing
			}
		}
	}
}
