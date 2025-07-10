using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class JobDeclarationCollectionFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView_CustomsDocStatus() => AssertFetchForView_CustomsDocStatus(JobDeclaration.Schema.CustomsDocStatus);
		public void TestFetchForView_CustomsDocStatusDesc() => AssertFetchForView_CustomsDocStatus(JobDeclaration.Schema.CustomsDocStatusDesc);

		void AssertFetchForView_CustomsDocStatus(string columnName)
		{
			var newFac = new BusinessObjectFactory() { RefreshEnabled = false };
			var pks = new List<ZGuid>();
			for (var i = 1; i < 6; i++)
			{
				var declaration = newFac.New<JobDeclaration>();
				declaration.JE_MessageType = i % 2 == 0 ? JobMessageTypeList.Codes.Import : JobMessageTypeList.Codes.Export;
				var instruction1 = declaration.CustomsEntryInstructions.AddNew();
				var requestedDoc1 = instruction1.RequestedDocuments.AddNew();
				requestedDoc1.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
				var requestedDoc2 = instruction1.RequestedDocuments.AddNew();
				requestedDoc2.CSI_Status = RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;
				var instruction2 = declaration.CustomsEntryInstructions.AddNew();
				var requestedDoc3 = instruction2.RequestedDocuments.AddNew();
				requestedDoc3.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
				pks.Add(declaration.PK);
			}
			newFac.Save();
			newFac = new BusinessObjectFactory();
			var declarations = newFac.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.PK, pks));
			var declarationCollection = new JobDeclarationCollection(newFac, GlbCompany.CurrentCompany.PK);
			var fetchStrategy = declarationCollection.FetchStrategy;
			fetchStrategy.FetchForView(declarations, new[] { new TableColumn(JobDeclarationSchema.Constants.TableName, columnName) });
			foreach (var declaration in declarations)
			{
				_ = declaration[columnName];
			}
			AssertEquals("CusEntryInstruction hits - CEI_ClusterKey", 1, newFac.GetTableHitCount(CusEntryInstruction.Schema.TableName));
			AssertEquals("CusSupportingInfo hits", 1, newFac.GetTableHitCount(CusSupportingInfo.Schema.TableName));
		}
	}
}
