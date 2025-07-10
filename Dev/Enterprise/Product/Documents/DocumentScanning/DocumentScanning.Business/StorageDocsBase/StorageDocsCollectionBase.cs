using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public abstract class StorageDocsCollectionBase<T> : BusinessObjectCollection<T> where T : StorageDocsBase
	{
		protected StorageDocsCollectionBase(NumberedBusinessObjectFactory factory)
			: base(factory)
		{
		}

#pragma warning disable 0809
		[Obsolete("You should use LoadWithAdditionalFiltering() to avoid deleting the additional filter on StorageDocsCollectionBase")]
		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			base.Load(alternativeAdditionalFilter);
		}
#pragma warning restore 0809

		/// <summary>
		/// Ensures that only documents that are supported in this country are loaded into the collection.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be a part of SQL expression.")]
		protected override ZQuery CreateAdditionalFilter()
		{
			ZDBOnlyQuery mainQuery = new ZDBOnlyQuery(typeof(StorageDocs));

			if (ExcludeDeletedDocuments)
			{
				mainQuery.AddToFilter(StorageDocsSchema.SC_IsDeleted, false);
			}

			DocManagerDBHelper dbHelper = new DocManagerDBHelper();
			string sQLFilterQuery = String.Format("{0} IN (SELECT {1} FROM {2} {3})",
				StorageDocsSchema.SC_SM.Name,
				StorageMainSchema.PK.Name,
				dbHelper.GetTableNameWithDatabasePrefix(0, StorageMainSchema.Constants.TableName),
				new CountrySpecificRefTypesQuery().GetAsWhereAndOrderByClause(true));

			mainQuery.AddFilterAndZSQLParameterCollection(sQLFilterQuery, new ZSqlParameterCollection());

			return mainQuery;
		}

		public ZBool ExcludeDeletedDocuments
		{
			get { return excludeDeletedDocuments; }
			set
			{
				excludeDeletedDocuments = value;
				if (IsLoaded)
				{
					Load();
				}
			}
		}
		ZBool excludeDeletedDocuments;

		public DocumentFactory MasterFactory
		{
			get { return ((NumberedBusinessObjectFactory)Factory).MasterFactory; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			if (IsAddNewSupported)
			{
				return base.AddNewCore();
			}
			else
			{
				return null;
			}
		}

		protected virtual bool IsAddNewSupported
		{
			get { throw new NotSupportedException("StorageDocsCollectionBase does not support AddNew - only the subclasses of this class can support it."); }
		}
	}

	public class StorageDocsCollectionBase : StorageDocsCollectionBase<StorageDocsBase>
	{
		public StorageDocsCollectionBase(NumberedBusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
