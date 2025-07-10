using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.PrintProcessing;

namespace Enterprise.DocumentScanning.Business
{
	public class NumberedBusinessObjectFactory : BusinessObjectFactory
	{
		/// <summary>
		/// Use this overload to create a new numbered business object factory 
		/// Keeps track of which DB it is connected to (Odyssey is DB 0, Odyssey_SD00x is number x)
		/// </summary>
		/// <param name="Number"></param>
		/// <param name="MasterFactory"></param>
		public NumberedBusinessObjectFactory(int number, DocumentFactory masterFactory)
			: this(new DocManagerDBHelper().GetDatabaseName(number))
		{
			this.DBNumber = number;
			this.MasterFactory = masterFactory;
		}

		public NumberedBusinessObjectFactory(DbConnection connection, DocumentFactory masterFactory, bool canBeSavedWithoutMasterFactory = false)
			: base(connection)
		{
			this.MasterFactory = masterFactory;
			this.canBeSavedWithoutMasterFactory = canBeSavedWithoutMasterFactory;
		}

		NumberedBusinessObjectFactory(string dbName)
			: base(dbName)
		{
			DBName = dbName;
		}

		protected NumberedBusinessObjectFactory()
		{
			this.DBNumber = 0;
			this.DBName = new DocManagerDBHelper().GetDatabaseName(DBNumber);
			this.MasterFactory = this as DocumentFactory;
		}

		public readonly DocumentFactory MasterFactory;
		public int DBNumber { get; private set; }
		public string DBName { get; private set; }
#if DEBUG
		public
#else
		internal
#endif
		void ChangeDbNumberToAWriteableOne(int newNumber)
		{
			if (DBNumber != newNumber)
			{
				DBNumber = newNumber;
				DBName = MasterFactory.GetDatabaseName(newNumber);

				((IReadonlyDatabaseSupport)_rowFactoryDoNotUseDirectly).ChangeDatabaseNameToAWriteableOne(DBName);
			}
		}

		public StorageDocsBase NewWithParent(Type bizOType)
		{
			StorageDocsBase newObject = (StorageDocsBase)New(bizOType);
			MasterFactory.CreateParentFor(newObject);
			return newObject;
		}

#if DEBUG
		public
#else
		internal

#endif
		StorageMain CreateParentFor(StorageDocsBase newElement)
		{
			var newParent = New(typeof(StorageMain)) as StorageMain;
			newElement.SC_SM = newParent.PK;
			newElement.HasChanges = false;
			return newParent;
		}

		protected override bool NotifyOfSaveBeforeAnyFactorySaveBegins()
		{
			return true;
		}

		#region save

		readonly bool canBeSavedWithoutMasterFactory;

		protected override IChangedTableNames SaveInTransactionCore()
		{
			if (MasterFactory.TransactionStartedTime.IsEmpty && !canBeSavedWithoutMasterFactory)
			{
				ErrorReporter.ReportOnce("NumberedBusinessObjectFactory.Save()", "Child factory (SD_xxx database) is being saved without the Master factory. Call Save() on the Master factory to ensure all factories get saved.");
			}

			try
			{
				return base.SaveInTransactionCore();
			}
			catch (Exception ex)
			{
				ConvertToEdocsOffLine(ex);
				throw;
			}
		}

		protected void ConvertToEdocsOffLine(Exception initialException)
		{
			var ex = initialException;
			while (ex != null)
			{
				if (ex is SqlException sqlException && SqlFailureChecker.IsAcceptableFailure(sqlException))
				{
					throw new EDocsOffLineException("EDocs currently offline - contact your system administrator" + System.Environment.NewLine + "Reason : " + initialException.Message, initialException);
				}
				ex = ex.InnerException;
			}
		}

		#endregion

		public override BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
		{
			return new NumberedBusinessObjectFactory(DBNumber, MasterFactory)
			{
				NameForDebugging = "eDocsFactory for DB:" + DBNumber.ToString()
			};
		}

		protected override void InvalidateCachedPropertiesCore()
		{
			base.InvalidateCachedPropertiesCore();
			if (MasterFactory != null && MasterFactory != this)
			{
				MasterFactory.InvalidateCachedProperties();
			}
		}

		#region Loading / Creating Objects

		public override BusinessObject Load(Type bizOType, ZGuid pK)
		{
			if (ShouldLoadIntoDocFactory(bizOType))
			{
				return base.Load(bizOType, pK);
			}
			else
			{
				return MasterFactory.FactoryForEverythingExceptEDocs.Load(bizOType, pK);
			}
		}

		public override BusinessObject[] Load(Type bizOType, ZQuery sQLFilter)
		{
			if (ShouldLoadIntoDocFactory(bizOType))
			{
				return base.Load(bizOType, sQLFilter);
			}
			else
			{
				return MasterFactory.FactoryForEverythingExceptEDocs.Load(bizOType, sQLFilter);
			}
		}

		public override BusinessObject New(Type bizOType, Guid initialisingPk)
		{
			if (ShouldLoadIntoDocFactory(bizOType))
			{
				return base.New(bizOType, initialisingPk);
			}
			else
			{
				return MasterFactory.FactoryForEverythingExceptEDocs.New(bizOType, initialisingPk);
			}
		}

		public override BusinessObject LoadFromUniqueKey(Type bizOType, CargoWise.Schema.SchemaColumn uniqueKeyColumn, IZType uniqueKeyValue)
		{
			if (ShouldLoadIntoDocFactory(bizOType))
			{
				return base.LoadFromUniqueKey(bizOType, uniqueKeyColumn, uniqueKeyValue);
			}
			else
			{
				return MasterFactory.FactoryForEverythingExceptEDocs.LoadFromUniqueKey(bizOType, uniqueKeyColumn, uniqueKeyValue);
			}
		}

		protected override BusinessObject CreateBusinessObject(System.Data.DataRow row, Type bizOType, ITypeDeciderContext typeDeciderContext = null)
		{
			if (ShouldLoadIntoDocFactory(bizOType))
			{
				return base.CreateBusinessObject(row, bizOType, typeDeciderContext);
			}
			else
			{
				return CreateBusinessObject(row, bizOType, MasterFactory.FactoryForEverythingExceptEDocs, typeDeciderContext);
			}
		}

		bool ShouldLoadIntoDocFactory(Type bizOType)
		{
			return typeof(ICanBeSavedByDocumentFactory).IsAssignableFrom(bizOType);
		}

		#endregion
	}
}
