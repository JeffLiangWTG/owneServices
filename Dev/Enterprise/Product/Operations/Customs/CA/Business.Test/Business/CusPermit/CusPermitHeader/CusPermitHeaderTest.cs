using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusPermitHeader))]
	sealed class CusPermitHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIDISHost()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			var cusPermitHeader = documentFactory.New<CusPermitHeader>();
			cusPermitHeader.CPH_Number = "Dec1";
			var docManagerInfo = cusPermitHeader.DocManagerInfo;
			AssertEquals(0, ((IDISHost)cusPermitHeader).EDocs.Count());
			docManagerInfo.Save();
			var filter = new ZQuery(StorageMainSchema.SM_ParentFK, cusPermitHeader.PK);
			AssertNull(documentFactory.LoadTop1<IStorageMain>(filter));

			var contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			cusPermitHeader.DocManagerInfo.AddFileOrDocument(contents, "document.pdf", "ABC");

			var disHost = (IDISHost)cusPermitHeader;
			AssertEquals("DISHost", cusPermitHeader, ((IDISHostProvider)cusPermitHeader).DISHost);
			AssertEquals("BranchPK", GlbBranch.CurrentBranch.PK, disHost.BranchPK);
			AssertEquals("CompanyPK", GlbCompany.CurrentCompany.PK, disHost.CompanyPK);
			AssertEquals("ApplicationCodes Count", 1, disHost.ApplicationCodes.Count());
			AssertEquals("ApplicationCodes", Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, disHost.ApplicationCodes.First());
			AssertEquals("RequiredDocumentsProvider", cusPermitHeader, disHost.RequiredDocumentsProvider);
			AssertEquals(1, ((IDISHost)cusPermitHeader).EDocs.Count());
			AssertEquals("JobNumber", "Dec1", disHost.JobNumber);
			AssertEquals("ControllerIDProvider", cusPermitHeader, disHost.ControllerIDProvider);
			AssertEquals("ImporterName", "", disHost.ImporterName);
			AssertEquals("ErrorMessages", 0, disHost.ErrorMessages.Count());
			Assert("ShowDISFeatures", disHost.ShowDISFeatures);

			var caDifHost = (ICADIFHost)cusPermitHeader;
			AssertNotNull("ValueProvider", caDifHost.ValueProvider);
		}

		public void TestRequiredDocumentAddInfos()
		{
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var permit = Factory.New<CusPermitHeader>();
			var document1 = permit.RequiredDocuments.AddNew();
			var addinfo11 = document1.AddInfos.AddNew();
			addinfo11.EX_ReferenceNumber = "100000000011";
			addinfo11.EX_ApplicationCode = "CAD";
			addinfo11.EX_Status = Enterprise.Customs.Common.CA.DIF.StatusList.Codes.AwaitingOriginal;
			permit.CPH_StartDate = new ZDate(2018, 3, 20);
			permit.CPH_Number = "Dec1";
			permit.CPH_QtyValIndicator = "BTH";
			permit.CPH_OH_PermitHolder = orgHeader.PK;
			permit.CPH_Type = "ADJ";
			permit.CPH_SubType = ZString.Empty;

			Factory.Save();

			AssertEquals("100000000011", permit.DIFURNs);
			AssertEquals("Awaiting Original", permit.DIFMessageStatus);

			var addinfo12 = document1.AddInfos.AddNew();
			addinfo12.EX_ReferenceNumber = "100000000012";
			addinfo12.EX_ApplicationCode = "DIS";
			addinfo12.EX_Status = Enterprise.Customs.Common.CA.DIF.StatusList.Codes.AwaitingAmendment;

			var addinfo13 = document1.AddInfos.AddNew();
			addinfo13.EX_ReferenceNumber = "100000000013";
			addinfo13.EX_ApplicationCode = "CAD";
			addinfo13.EX_GC_Company = Factory.New<GlbCompany>().PK;
			addinfo13.EX_Status = Enterprise.Customs.Common.CA.DIF.StatusList.Codes.AwaitingAmendment;

			Factory.Save();

			AssertEquals("100000000011", permit.DIFURNs);
			AssertEquals("Awaiting Original", permit.DIFMessageStatus);

			var addinfo14 = document1.AddInfos.AddNew();
			addinfo14.EX_ReferenceNumber = "100000000014";
			addinfo14.EX_ApplicationCode = "CAD";
			addinfo14.EX_Status = Enterprise.Customs.Common.CA.DIF.StatusList.Codes.AwaitingAmendment;
			var document2 = permit.RequiredDocuments.AddNew();
			var addinfo21 = document2.AddInfos.AddNew();
			addinfo21.EX_ReferenceNumber = "100000000021";
			addinfo21.EX_ApplicationCode = "CAD";
			addinfo21.EX_Status = Enterprise.Customs.Common.CA.DIF.StatusList.Codes.AwaitingOriginal;

			Factory.Save();

			AssertEquals("Multiple", permit.DIFURNs);
			AssertEquals("Multiple", permit.DIFMessageStatus);
		}
	}
}
