using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.ComponentModel
{
	public abstract class ZMetaDataTypes : MetaDataTypes
	{
		public const string ModuleId = "ModuleID";

		static ZMetaDataTypes()
		{
			RegisterTypes();
		}

		internal static void RegisterTypes()
		{
			MetaDataType.RegisterMetaDataType(new MetaDataMandatoryType(ModuleId, typeof(ModuleIdentifier), ModuleIDs.NotAssigned, new string[] { ZMetaDataTypes.ListDataSource }));
		}
	}
}
