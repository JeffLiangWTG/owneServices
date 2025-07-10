using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Business
{
	public partial class OperationalActionNKModuleFieldSupporter : OperationalActionTextFieldSupporter
	{
		public OperationalActionNKModuleFieldSupporter(string field, bool readOnly, int maxLength, BusinessObjectCollectionProvider constructCollection)
			: base(field, readOnly, maxLength)
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
			strategies.Add(new FixedNKModuleFieldDefaultingStrategy(this));
		}

		protected override RunnerField NewRunnerFieldCore(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor)
		{
			return new RunnerNKModuleField(factory, descriptor, this);
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

	public partial class OperationalActionNKModuleFieldSupporter
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

					if (Attribute.GetCustomAttribute(collection.TypeOfElements, typeof(CodePropertyAttribute)) == null)
					{
						errors.Add("The element type does not include a CodeProperty attribute");
					}
				}
			}
		}
	}
}

#endregion

#endif
#endregion
