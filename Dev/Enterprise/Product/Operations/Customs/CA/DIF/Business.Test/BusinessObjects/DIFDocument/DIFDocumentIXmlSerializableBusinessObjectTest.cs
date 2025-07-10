using System;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA.DIF;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DIF.Business.Testing
{
	[TestedType(typeof(DIFDocument))]
	sealed class DIFDocumentIXmlSerializableBusinessObjectTest : XmlSerializableNonPersistentBusinessObjectTest<DIFDocument>
	{
		public void TestInitialize()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			company.GC_Code = "CA1";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "CA1";
			var declaration = (ICADIFHost)JobDeclaration;
			var requiredDocument1 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument1.EQ_DocType = Core.Constants.RefDocTypes.CommercialInvoice;
			var requiredDocument2 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument2.EQ_DocType = Core.Constants.RefDocTypes.CertificateOfOrigin;

			var hostWrapper = new DIFHostWrapper(declaration);
			var difDocument = new DIFDocument(hostWrapper);

			JobRequiredDocumentAddInfo addInfo = Factory.New<JobRequiredDocumentAddInfo>();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo.EX_GC_Company = company.PK;
			addInfo.EX_ReferenceNumber = "REF000001";
			addInfo.EX_AddInfo = "<DIFDocument xmlns=\"http://www.cargowise.com/Schemas/DIFDocument\"><PGA>12</PGA><DocumentType>5002</DocumentType><EDocsDocumentPK>feb71b42-4444-4168-8918-0e0f27d8c375</EDocsDocumentPK><DocumentNumber>DMNUMBER0001</DocumentNumber><EffectiveDate>2017-12-10T00:00:00</EffectiveDate><ExpiryDate>2017-12-12T00:00:00</ExpiryDate></DIFDocument>";
			difDocument.RequiredDocumentAddInfo = addInfo;
			difDocument.Initialize();
			AssertEquals("REF000001", difDocument.URN);
			AssertEquals("DMNUMBER0001", difDocument.DocumentNumber);
			AssertEquals("5002", difDocument.DocumentType);
			AssertEquals(new ZDateTime(2017, 12, 10), difDocument.EffectiveDate);
			AssertEquals(new ZDateTime(2017, 12, 12), difDocument.ExpiryDate);
		}

		public void TestDefaultOtherFieldsBasedOneDoc()
		{
			var declaration = (ICADIFHost)JobDeclaration;
			var requiredDocument1 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument1.EQ_DocType = Core.Constants.RefDocTypes.CommercialInvoice;
			var requiredDocument2 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument2.EQ_DocType = Core.Constants.RefDocTypes.CertificateOfOrigin;

			var hostWrapper = new DIFHostWrapper(declaration);
			var difDocument = new DIFDocument(hostWrapper);

			var eDoc = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Test", Core.Constants.RefDocTypes.CommercialInvoice);
			eDoc.Description = "SOME DESCRIPTION";
			difDocument.EDocsDocumentPK = eDoc.UniqueKey;

			AssertEquals(requiredDocument1.PK, difDocument.RequiredDocumentPK);
			AssertEquals("SOME DESCRIPTION", difDocument.DocumentDescription);
		}

		public void TestCanDelete()
		{
			var difDocument = GetNewBusinessObject() as DIFDocument;

			difDocument.Status = StatusList.Codes.ErrorAcknowledgedOriginal;
			Assert(difDocument.CanDelete);

			difDocument.Status = StatusList.Codes.AwaitingOriginal;
			Assert(!difDocument.CanDelete);
			AssertEquals("This document has been submitted to Customs. Delete is not allowed when a response from Customs is outstanding.", difDocument.ReasonForNotAbleToDelete);

			difDocument.Status = StatusList.Codes.AcknowledgedOriginal;
			Assert(!difDocument.CanDelete);
			AssertEquals("This document has been accepted by Customs. Delete is not allowed once a document is accepted.", difDocument.ReasonForNotAbleToDelete);

			difDocument.Status = StatusList.Codes.AcknowledgedWithdrawal;
			Assert(!difDocument.CanDelete);
			AssertEquals("This document has been accepted by Customs. Delete is not allowed once a document is accepted.", difDocument.ReasonForNotAbleToDelete);
		}

		public void TestRequiredDocumentPKReadOnly()
		{
			var difDocument = GetNewBusinessObject() as DIFDocument;
			Assert(!difDocument.RequiredDocumentPKInfo.ReadOnly);

			difDocument.Status = StatusList.Codes.AwaitingOriginal;
			Assert(difDocument.RequiredDocumentPKInfo.ReadOnly);

			difDocument.Status = StatusList.Codes.ErrorAcknowledgedOriginal;
			Assert(!difDocument.RequiredDocumentPKInfo.ReadOnly);

			difDocument.Status = StatusList.Codes.AcknowledgedOriginal;
			Assert(difDocument.RequiredDocumentPKInfo.ReadOnly);

			difDocument.Status = StatusList.Codes.AcknowledgedWithdrawal;
			Assert(difDocument.RequiredDocumentPKInfo.ReadOnly);
		}

		[UseSnapshotProtection]
		public void TestRequiredDocumentPKReadOnlyBasedOnURN()
		{
			ObjectFactory.Get<Integration.Customs.CA.ICACustomsDataRegistry>().AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
			var difFountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber12345DIF");
			difFountain.SetValues(Factory, minValue: 1, nextValue: 0, maxValue: 10_000);

			var decFountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber12345");
			decFountain.SetValues(Factory, minValue: 10_000, nextValue: 10_001, maxValue: 90_000);

			var difDocument = GetNewBusinessObject() as DIFDocument;
			Assert(!difDocument.RequiredDocumentPKInfo.ReadOnly);

			Factory.Save();
			AssertNotEquals(ZString.Empty, difDocument.URN);
			Assert(difDocument.RequiredDocumentPKInfo.ReadOnly);

			AssertEquals("URN is already assigned. If you proceed, this URN will be discarded. Are you sure you wish to continue?", difDocument.GetWarningBeforeBeingDeleted());
		}

		public void TestStatusDescription()
		{
			var difDocument = GetNewBusinessObject() as DIFDocument;
			AssertEquals("Not Sent", difDocument.StatusDescription);
			difDocument.Status = StatusList.Codes.AwaitingOriginal;
			AssertEquals(StatusList.Descriptions.AwaitingOriginal, difDocument.StatusDescription);
		}

		public void TestMessageSendingError()
		{
			var declaration = (ICADIFHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs1 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var eDocs2 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC×.pdf", "ABC", false);

			var hostWrapper = new DIFHostWrapper(declaration);

			var difDocument = hostWrapper.DISDocuments.AddNew();
			AssertCollectionNotContains(DIFDocumentValidation.FileNameHasInvalidCharacters, difDocument.MessageSendingError.Split('\r', '\n'));

			difDocument.EDocsDocumentPK = eDocs1.UniqueKey;
			AssertCollectionNotContains(DIFDocumentValidation.FileNameHasInvalidCharacters, difDocument.MessageSendingError.Split('\r', '\n'));

			difDocument.EDocsDocumentPK = eDocs2.UniqueKey;
			AssertCollectionContains(DIFDocumentValidation.FileNameHasInvalidCharacters, difDocument.MessageSendingError.Split('\r', '\n'));
		}

		public void TestICADIFDefaultValues()
		{
			var difDocument = GetNewBusinessObject() as DIFDocument;
			AssertEquals("DocumentNumber", "", difDocument.DocumentNumber);
			Assert("DocumentNumber_ReadOnly", !difDocument.DocumentNumberInfo.ReadOnly);
			AssertEquals("EffectiveDate", ZDate.Empty, difDocument.EffectiveDate);
			Assert("EffectiveDate_ReadOnly", !difDocument.EffectiveDateInfo.ReadOnly);
			AssertEquals("ExpiryDate", ZDate.Empty, difDocument.ExpiryDate);
			Assert("ExpiryDate_ReadOnly", !difDocument.ExpiryDateInfo.ReadOnly);

			var cusPermitHeader = new TestHelper(Factory).GetCusPermitHeader();
			var hostWrapper = new DIFHostWrapper((ICADIFHost)cusPermitHeader);
			difDocument = hostWrapper.DISDocuments.AddNew();
			AssertEquals("DocumentNumber", "Per123", difDocument.DocumentNumber);
			Assert("DocumentNumber_ReadOnly", difDocument.DocumentNumberInfo.ReadOnly);
			AssertEquals("EffectiveDate", ZDate.BrettsBirthday, difDocument.EffectiveDate);
			Assert("EffectiveDate_ReadOnly", difDocument.EffectiveDateInfo.ReadOnly);
			AssertEquals("ExpiryDate", new ZDate(2017, 1, 1), difDocument.ExpiryDate);
			Assert("ExpiryDate_ReadOnly", difDocument.ExpiryDateInfo.ReadOnly);
		}

		public void TestLPCO()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			var pgaHeader = invoiceLine.CFIAPGAHeader;
			var lpcoView = pgaHeader.LPCOViews.AddNew();
			lpcoView.CLP_Type = "123";
			lpcoView.CLP_RefNo = "456";
			lpcoView.CLP_StartDate = ZDate.Today;
			lpcoView.CLP_EndDate = ZDate.Today.AddDays(1);
			var lpco = lpcoView.LPCO;

			var difDocument = GetNewBusinessObject() as DIFDocument;
			difDocument.SetDefaultFromLPCO(lpco);
			AssertEquals(lpco, difDocument.LPCO);
			AssertEquals(lpcoView.CLP_Type, difDocument.DocumentType);
			AssertEquals(lpcoView.CLP_RefNo, difDocument.DocumentNumber);
			AssertEquals(lpcoView.CLP_StartDate, difDocument.EffectiveDate);
			AssertEquals(lpcoView.CLP_EndDate, difDocument.ExpiryDate);
			AssertEquals("CFIA", difDocument.PGA);

			var addInfo = difDocument.RequiredDocumentAddInfo;
			difDocument.RequiredDocumentAddInfo = addInfo;
			addInfo.EX_ReferenceNumber = "789";
			AssertEquals(addInfo.EX_ReferenceNumber, lpco.CLP_DIFRefNumberOrLocation);
		}

		public void TestResourceStringDataAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(DIFDocument), nameof(DIFDocument.BusinessNumber), false, attribute => attribute.Caption == "Document Owner/Stake Holder");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(DIFDocument), nameof(DIFDocument.PGA), false, attribute => attribute.Caption == "PGA");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(DIFDocument), nameof(DIFDocument.DocumentType), false, attribute => attribute.Caption == "Document Type");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(DIFDocument), nameof(DIFDocument.DocumentDescription), false, attribute => attribute.Caption == "Description");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(DIFDocument), nameof(DIFDocument.EDocsDocumentPK), false, attribute => attribute.Caption == "eDocs");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(DIFDocument), nameof(DIFDocument.Comment), false, attribute => attribute.Caption == "Comment");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(DIFDocument), nameof(DIFDocument.URN), false, attribute => attribute.Caption == "URN");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(DIFDocument), nameof(DIFDocument.DocumentNumber), false, attribute => attribute.Caption == "Ref/Permit/License No");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(DIFDocument), nameof(DIFDocument.EffectiveDate), false, attribute => attribute.Caption == "Effective Date");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(DIFDocument), nameof(DIFDocument.ExpiryDate), false, attribute => attribute.Caption == "Expiry Date");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(DIFDocument), nameof(DIFDocument.RequiredDocumentPK), false, attribute => attribute.Caption == "Tracking Document");
		}

		public void TestMessageSendingAction()
		{
			var difDocument = GetNewBusinessObject() as DIFDocument;
			var messageAction = difDocument.MessageSendingAction;
			Assert(messageAction.Send);
			var messageActions = new MessageSendingActionCollection(Factory);
			messageActions.Add(messageAction);
			AssertEquals(messageAction, messageActions[0]);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var requiredDocument = ((IDISHost)JobDeclaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			var hostWrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration);
			var difDocument = hostWrapper.DISDocuments.AddNew();
			difDocument.RequiredDocumentPK = requiredDocument.PK;
			difDocument.RequiredDocumentAddInfo = addInfo;
			return difDocument;
		}

		BusinessObject jobDeclaration;
		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
