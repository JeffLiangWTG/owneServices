using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsDependentCollectionBase : DependentBusinessObjectCollection<StorageDocsBase, StorageMain>, IStorageDocsBaseCollection
	{
		public StorageDocsDependentCollectionBase(StorageMain parent, BusinessObjectFactory factory)
			: base(parent, factory, true)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public DocumentFactory MasterFactory
		{
			get { return ((NumberedBusinessObjectFactory)Factory).MasterFactory; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new Exception("DO NOT CALL ADDNEW ON THIS COLLECTION! IT IS ABSTRACT! Use the AddNew(Type) overload instead");
		}

		#region IStorageDocsBaseCollection Members

		IeDoc IStorageDocsBaseCollection.GetMostRecentEDoc(string docType)
		{
			return StorageDocsCollectionHelper.GetMostRecentEDoc(docType, this);
		}

		IeDoc IStorageDocsBaseCollection.GetFromUniqueKey(Guid uniqueKey)
		{
			return StorageDocsCollectionHelper.GetFromUniqueKey(uniqueKey, this);
		}

		void IStorageDocsBaseCollection.Add(IeDoc elementToAdd)
		{
			Add((BusinessObject)elementToAdd);
		}

		bool IStorageDocsBaseCollection.Contains(IeDoc element)
		{
			return Contains((BusinessObject)element);
		}

		int IStorageDocsBaseCollection.Count
		{
			get { return Count; }
		}

		void IStorageDocsBaseCollection.Remove(IeDoc elementToRemove)
		{
			Remove((BusinessObject)elementToRemove);
		}

		IeDoc IStorageDocsBaseCollection.this[int index]
		{
			get { return this[index]; }
		}

		bool IStorageDocsBaseCollection.ContainsDocType(ZString docType)
		{
			return StorageDocsCollectionHelper.ContainsEDocType(docType, this);
		}

		#endregion

		public void SetImageDataForAllIfRequired()
		{
			foreach (var docBase in Elements)
			{
				var file = docBase as StorageFile;
				var doc = docBase as StorageDocs;

				if (file != null && file.IsSC_ImageDataOutOfSync && !file.SC_IsDeleted)
				{
					file.SetImageData();
				}
				else if (doc != null && doc.WasOpenInExternalEditor && doc.IsSC_ImageDataOutOfSync)
				{
					doc.SetImageData();
				}
			}
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			try
			{
				base.Load(alternativeAdditionalFilter);
			}
			catch (SqlException ex)
			{
				bool handled = false;

				try
				{
					if (HandleUnconvertedReadonlyDatabase(ex))
					{
						base.Load(alternativeAdditionalFilter);
						handled = true;
					}
				}
				catch { handled = false; }

				if (!handled)
				{
					throw;
				}
			}
		}

		bool HandleUnconvertedReadonlyDatabase(SqlException ex)
		{
			bool handled = false;
			var errorMatch = new CargoWise.Data.DbErrorMatch(ex);

			if (errorMatch.ExceptionType == CargoWise.Data.DbErrorType.InaccessibleFiles)
			{
				using (var adminConn = CargoWise.Data.Db.NewAdminConnection())
				using (adminConn.UseMasterDb())
				{
					string storageDocsDatabase = MasterFactory.GetDatabaseName(((NumberedBusinessObjectFactory)Factory).DBNumber);
					string sqlText = "SELECT count(*) FROM sys.databases WHERE name = @DbName AND is_read_only = 1 AND [state] = 3;";
					bool isUnconvertedReadonlyDb = (Convert.ToInt32(adminConn.ExecuteScalar(sqlText, command =>
					{
						command.AddParameter("@DbName", SqlDbType.NVarChar, 128, storageDocsDatabase);
					})) == 1);

					if (isUnconvertedReadonlyDb)
					{
						sqlText = String.Format(
							(NoResString)"ALTER DATABASE [{0}] SET read_write; ALTER DATABASE [{0}] SET read_only;",
							storageDocsDatabase);
						adminConn.ExecuteNonQuery(sqlText);
						handled = true;
					}
				}
			}

			return handled;
		}
	}
}
