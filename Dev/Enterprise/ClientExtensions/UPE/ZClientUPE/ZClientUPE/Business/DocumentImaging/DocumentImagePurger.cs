
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.DocumentImaging
{
	public class DocumentImagePurger
	{
		internal int PurgeDocumentsForTest()
		{
			return PurgeDocuments(CancellationToken.None);
		}

		public int PurgeDocuments(CancellationToken token)
		{
			int result = 0;
			int count = 0;
			do
			{
				token.ThrowIfCancellationRequested();
				count = PurgeDocumentBatch();
				result += count;
			}
			while (count > 0);
			return result;
		}

		#region Implementation

		int PurgeDocumentBatch()
		{
			int result = 0;
			var existingDocumentDBs = new DocManagerDBHelper().GetStorageDocDbNumbersIncludingMainDb();
			foreach (int dBNumber in existingDocumentDBs)
			{
				result += PurgeDocumentBatch(dBNumber);
			}
			return result;
		}

		int PurgeDocumentBatch(int dBNumber)
		{
			int result = 0;
			DocumentFactory factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(StorageMain));

			ZDBOnlySubQuery cusHAWBSubQuery = new ZDBOnlySubQuery(typeof(UPECusHAWB), CusHAWBSchema.PK);
			var branches = UPETools.Instance.UPECustomisationBranches(true).ToArray();
			ZDBOnlySubQuery mawbSubQuery = new ZDBOnlySubQuery(typeof(UPECusMAWB), CusMAWBSchema.PK);
			mawbSubQuery.FilterByForeignKey(CusMAWBSchema.CM_GB, branches);
			cusHAWBSubQuery.AddSubQuery(CusHAWBSchema.CS_CM, mawbSubQuery, JoinCondition.And);

			ZDBOnlySubQuery jobDecSubQuery = new ZDBOnlySubQuery(typeof(UPEJobDeclaration), JobDeclarationSchema.PK);
			jobDecSubQuery.FilterByForeignKey(JobDeclarationSchema.JE_GB, branches);

			query.AddSubQuery(StorageMainSchema.SM_ParentFK, cusHAWBSubQuery, JoinCondition.Or);
			query.AddSubQuery(StorageMainSchema.SM_ParentFK, jobDecSubQuery, JoinCondition.Or);
			query.AddToFilter(JoinCondition.And, StorageMainSchema.SM_DB, SQLComparisonOperator.Equal, dBNumber);

			string dBName = factory.GetDatabaseName(dBNumber);
			query.AddFilterAndZSQLParameterCollection("(SELECT MAX(SC_SystemLastEditTimeUtc) FROM " + dBName + "..StorageDocs WHERE SC_SM=SM_PK) <= @LastActivityDateTime",
				new ZSqlParameterCollection(ZSqlParameter.New("@LastActivityDateTime", ZDateTime.Now.AddMonths(-3), StorageDocsSchema.SC_SystemLastEditTimeUtc)), JoinCondition.And);
			query.AddFilterAndZSQLParameterCollection("EXISTS (SELECT null FROM " + dBName + "..StorageDocs WHERE SC_SM=SM_PK AND SC_DocType = 'CIV')", new ZSqlParameterCollection(), JoinCondition.And);

			query.MaximumRows = 100;
			StorageMain[] cusHAWBDocuments = (StorageMain[])factory.Load(typeof(StorageMain), query);

			foreach (StorageMain storageMain in cusHAWBDocuments)
			{
				result += storageMain.eDocs.Count;
				storageMain.eDocs.RemoveAndDeleteAll();
			}
			factory.Save();
			return result;
		}

		#endregion
	}
}
