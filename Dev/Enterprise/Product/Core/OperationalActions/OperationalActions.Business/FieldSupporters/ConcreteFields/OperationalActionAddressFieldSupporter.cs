using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed partial class OperationalActionAddressFieldSupporter : OperationalActionFieldSupporter
	{
		public OperationalActionAddressFieldSupporter(string field, bool readOnly, AddressType defaultAddressType, BusinessObjectCollectionProvider constructCollection)
			: base(field, readOnly)
		{
			if (constructCollection == null)
			{
				throw new ArgumentNullException(nameof(constructCollection));
			}

			this.defaultAddressType = defaultAddressType;
			this.constructCollection = constructCollection;
		}

		public AddressType DefaultAddressType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return defaultAddressType; }
		}

		public BusinessObjectCollectionProvider ConstructCollection
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return constructCollection; }
		}

		protected override RunnerField NewRunnerFieldCore(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor)
		{
			return new RunnerAddressField(factory, descriptor, this);
		}

		protected override string AsFilterStringCore(IZType value, BusinessObjectFactory factory)
		{
			return null;
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly AddressType defaultAddressType;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly BusinessObjectCollectionProvider constructCollection;
	}
}

#region Test
#if DEBUG

#region Self Checking

namespace Enterprise.Services.OperationalActions.Business
{
	partial class OperationalActionAddressFieldSupporter
	{
		protected override void PerformSelfCheckForTesting(BusinessObjectFactory factoryForTesting, List<string> errors)
		{
			base.PerformSelfCheckForTesting(factoryForTesting, errors);

			if (!Enum.IsDefined(typeof(AddressType), defaultAddressType))
			{
				errors.Add(string.Format("Invalid AddressType ({0}).", defaultAddressType));
			}

			IBusinessObjectCollection collection = ConstructCollection(factoryForTesting);

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

				Type elementType = BusinessObjectCollection.GetElementTypeFromCollectionType(collection.GetType());
				if (!typeof(OrgHeader).IsAssignableFrom(elementType))
				{
					errors.Add("The collection element type is inapproperate, it needs to be assignable to OrgHeader");
				}
			}
		}
	}
}

#endregion

#endif
#endregion
