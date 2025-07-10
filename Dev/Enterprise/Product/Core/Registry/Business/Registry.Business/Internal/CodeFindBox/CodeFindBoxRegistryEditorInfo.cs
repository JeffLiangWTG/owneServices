using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Registry.Business
{
	public class CodeFindBoxRegistryEditorInfo : RegistryEditorInfo
	{
		public delegate IBusinessObjectCollection GetFindBoxCollectionDelegate(BusinessObjectFactory factory);

		public CodeFindBoxRegistryEditorInfo(ModuleIdentifier moduleID, GetFindBoxCollectionDelegate getFindBoxCollection)
		{
			if (moduleID == null)
			{
				throw new ArgumentNullException(nameof(moduleID));
			}
			this.ModuleID = moduleID;

			if (getFindBoxCollection == null)
			{
				throw new ArgumentNullException(nameof(getFindBoxCollection));
			}
			this.getFindBoxCollection = getFindBoxCollection;
		}

		public IBusinessObjectCollection GetFindBoxCollection(BusinessObjectFactory factory)
		{
			return getFindBoxCollection(factory);
		}

		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(StringRegistryDataType); }
		}

		public readonly ModuleIdentifier ModuleID;
		readonly GetFindBoxCollectionDelegate getFindBoxCollection;
	}
}
