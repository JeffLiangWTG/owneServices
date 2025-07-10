using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common.Testing;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	internal class FactoryChangeSet : IFactoryChangeSet
	{
		readonly ICollection<BusinessObject> dirtyObjects;

		internal FactoryChangeSet(ICollection<BusinessObject> dirtyObjects)
		{
			this.dirtyObjects = dirtyObjects;
		}

		public IObjectChangeSet[] GetChangedObjects()
		{
			var result = new List<ObjectChangeSet>();
			var factoriesDictionary = new Dictionary<Type, BusinessObjectFactory>();
			var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();

			foreach (var sessionInstance in dirtyObjects)
			{
				if (sessionInstance.Row.RowState == DataRowState.Modified || sessionInstance.Row.RowState == DataRowState.Deleted)
				{
					if (!factoriesDictionary.TryGetValue(sessionInstance.Factory.GetType(), out var factory))
					{
						factory = sessionInstance.CreateNewFactory();
						factoriesDictionary.Add(sessionInstance.Factory.GetType(), factory);
					}
					var databaseInstance = LoadDatabaseEdition(sessionInstance, factory);

					var changeSet = new ObjectChangeSet(sessionInstance, databaseInstance, schemaResolver);
					result.Add(changeSet);

					if (changeSet.IsExistsInDatabase)
					{
						changeSet.Populate();
					}
				}
			}

			return result.ToArray();
		}

		public BusinessObject[] GetAddedObjects()
		{
			return dirtyObjects.Where(bizo => bizo.Row.RowState == DataRowState.Added).ToArray();
		}

		internal static BusinessObject LoadDatabaseEdition(BusinessObject obj, BusinessObjectFactory factory = null)
		{
			factory = factory ?? obj.CreateNewFactory();
			if (factory != obj.Factory)
			{
				factory.RefreshEnabled = false;
			}

			IBusinessObjectReload reloadable = obj as IBusinessObjectReload;
			BusinessObject result = reloadable != null && !obj.IsDeleted ? reloadable.Reload(factory) : factory.LoadFromDatabase(obj.GetType(), obj.PK);

			if (result != null)
			{
				obj.CopyTransientProperties(result);
			}

			return result;
		}
	}
}
