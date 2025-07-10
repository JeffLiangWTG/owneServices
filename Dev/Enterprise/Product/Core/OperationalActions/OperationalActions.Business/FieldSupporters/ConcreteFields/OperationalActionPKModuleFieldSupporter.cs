using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed partial class OperationalActionPKModuleFieldSupporter : OperationalActionFieldSupporter
	{
		public OperationalActionPKModuleFieldSupporter(string field, bool readOnly, BusinessObjectCollectionProvider constructCollection)
			: base(field, readOnly)
		{
			if (constructCollection == null)
			{
				throw new ArgumentNullException(nameof(constructCollection));
			}

			this.constructCollection = constructCollection;
		}

		public BusinessObjectCollectionProvider ConstructCollection
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return constructCollection; }
		}

		protected override void PopulateDefaultingStrategies(IList<IFieldDefaultingStrategy> strategies)
		{
			strategies.Add(new FixedPKModuleFieldDefaultingStrategy(this));
		}

		protected override RunnerField NewRunnerFieldCore(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor)
		{
			return new RunnerPKModuleField(factory, descriptor, this);
		}

		protected override string AsFilterStringCore(IZType value, BusinessObjectFactory factory)
		{
			IBusinessObjectCollection collection = ConstructCollection(factory);
			BusinessObject businessObject = null;
			if (collection is IActiveBusinessObjectCollection)
			{
				businessObject = collection.FindByPK((ZGuid)value);
			}
			else
			{
				var boccollection = ((BusinessObjectCollection)collection);
				var type = boccollection.TypeOfElements;
				var schema = BusinessObjectFactory.GetTableSchemaFromType(type);
				var pkcolumn = schema.PK;
				boccollection.Load(new ZQuery(pkcolumn, value));
				businessObject = collection.FindByPK((ZGuid)value);
			}
			return businessObject == null ? null : CodePropertyAttribute.CodeFromBusinessObject(businessObject).ToString();
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly BusinessObjectCollectionProvider constructCollection;
	}
}

#region Test
#if DEBUG

#region Self Checking

namespace Enterprise.Services.OperationalActions.Business
{
	using System.Collections.Generic;
	using Enterprise.ZArchitecture.ComponentModel;
	using Enterprise.ZArchitecture.Modules;

	public partial class OperationalActionPKModuleFieldSupporter
	{
		protected override void PerformSelfCheckForTesting(BusinessObjectFactory factoryForTesting, List<string> errors)
		{
			base.PerformSelfCheckForTesting(factoryForTesting, errors);

			IBusinessObjectCollection collection = ConstructCollection(factoryForTesting);
			ModuleIdentifier moduleID = ZMetaData.GetModuleId(collection);

			if (moduleID == null || moduleID == ModuleIDs.NotAssigned)
			{
				errors.Add(string.Format("ModuleID not defined for '{0}'", collection.GetType().Name));
			}
			else
			{
				if (collection == null)
				{
					errors.Add("The ConstructCollection delegate returned null");
				}
				else
				{
					if (collection.Factory != factoryForTesting)
					{
						errors.Add("ConstructCollection returned a collection with the wrong factory");
					}

					if (collection.IsLoaded)
					{
						errors.Add("ConstructCollection returned a collection that was already loaded");
					}
				}
			}
		}
	}
}

#endregion

#endif
#endregion
