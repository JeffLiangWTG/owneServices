using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.DIF.Business.Testing
{
	sealed class CADIFDocumentProviderTest : TestCaseWithFactory
	{
		public void TestGetDIFDocument()
		{
			ObjectFactory.Get<Integration.Customs.CA.ICACustomsDataRegistry>().AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
			var cACompany = Factory.New<GlbCompany>();
			cACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			cACompany.GC_Code = "CA1";
			var branch1 = cACompany.Branches.AddNew();
			branch1.GB_Code = "GB1";
			var uSCompany = Factory.New<GlbCompany>();
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_Code = "US1";
			var branch2 = uSCompany.Branches.AddNew();
			branch2.GB_Code = "US1";

			OrgHeader orgheader = Factory.New<OrgHeader>();
			orgheader.OH_RL_NKClosestPort = "CALAX";
			orgheader.OH_Code = "OH1";

			var permit = new TestHelper(Factory).GetCusPermitHeader();
			permit[CusPermitHeaderSchema.CPH_QtyValIndicator] = "BTH";
			permit[CusPermitHeaderSchema.CPH_OH_PermitHolder] = orgheader.PK;
			var disHost = (IDISHost)permit;
			var hostWrapper = new DIFHostWrapper(disHost as ICADIFHost);
			var requiredDocument = disHost.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocNumber = "DN21";
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.CommercialInvoice;

			var difDocument = new DIFDocument(hostWrapper);
			var eDoc = ((IDocManagerSupport)disHost).DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Test", Core.Constants.RefDocTypes.CommercialInvoice);
			eDoc.Description = "SOME DESCRIPTION";
			difDocument.EDocsDocumentPK = eDoc.UniqueKey;
			JobRequiredDocumentAddInfo addInfo = Factory.New<JobRequiredDocumentAddInfo>();
			addInfo.EX_EQ_RequiredDocument = difDocument.RequiredDocument.PK;
			addInfo.EX_ReferenceNumber = "REF1";
			addInfo.EX_GC_Company = cACompany.PK;
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;

			JobRequiredDocumentAddInfo addInfo2 = Factory.New<JobRequiredDocumentAddInfo>();
			addInfo2.EX_EQ_RequiredDocument = difDocument.RequiredDocument.PK;
			addInfo2.EX_ReferenceNumber = "REF2";
			addInfo2.EX_GC_Company = cACompany.PK;
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;

			JobRequiredDocumentAddInfo addInfo3 = Factory.New<JobRequiredDocumentAddInfo>();
			addInfo3.EX_EQ_RequiredDocument = difDocument.RequiredDocument.PK;
			addInfo3.EX_ReferenceNumber = "REF3";
			addInfo3.EX_GC_Company = uSCompany.PK;
			addInfo3.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			Factory.Save();

			var provider = ObjectFactory.Get<ICADIFDocumentProvider>();
			AssertNotNull(provider.GetDIFDocument(Factory, "REF1", Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, cACompany.PK));
			AssertNotNull(provider.GetDIFDocument(Factory, "REF2", Core.Constants.Customs.DocumentImageSystemIDs.US_DIS, cACompany.PK));
			AssertNull(provider.GetDIFDocument(Factory, "REF3", Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, uSCompany.PK));
			AssertNull(provider.GetDIFDocument(Factory, "REF3", Core.Constants.Customs.DocumentImageSystemIDs.US_DIS, cACompany.PK));
		}

		[UseSnapshotProtection]
		public void TestGetDocumentIDList()
		{
			ObjectFactory.Get<Integration.Customs.CA.ICACustomsDataRegistry>().AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			DbConnection connection = ((IDbConnected)Factory).Connection;
			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				var companyFountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber12345");
				companyFountain.SetNext(Factory, 1000);
				var difFountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber12345DIF");
				difFountain.SetNext(Factory, 777);
				transactionManager.CommitTransaction();
			}

			var disWrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration);
			var declaration = (ICADIFHost)JobDeclaration;
			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.CommercialInvoice;
			var eDoc = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Test", Core.Constants.RefDocTypes.CommercialInvoice);
			eDoc.Description = "SOME DESCRIPTION";

			var document1 = disWrapper.DISDocuments.AddNew();
			document1.DocumentDescription = "BOB THE BUILDER";
			document1.EDocsDocumentPK = eDoc.UniqueKey;
			document1.DocumentType = "5001";

			var document2 = disWrapper.DISDocuments.AddNew();
			document2.DocumentDescription = "BOB THE BUILDER";
			document2.EDocsDocumentPK = eDoc.UniqueKey;
			document2.DocumentType = "5002";
			Factory.Save();

			var host = (ICADIFHost)new BusinessObjectFactory().Load<Integration.Customs.CA.IJobDeclaration>(JobDeclaration.PK);
			var provider = ObjectFactory.Get<ICADIFDocumentProvider>();
			var list = provider.GetDIFDocuments(host);
			AssertEquals(2, list.Count());
			AssertNotNull(list.FirstOrDefault(x => x.DocumentType == "5002"));
			AssertNotNull(list.FirstOrDefault(x => x.DocumentType == "5001"));
		}

		BusinessObject jobDeclaration;
		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
