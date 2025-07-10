using System.Collections;

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.ComponentModel
{
	public abstract class ZMetaData : CargoWise.ComponentModel.MetaData
	{
		public static ModuleIdentifier GetModuleId(IList component)
		{
			return (ModuleIdentifier)GetMetaData(component, null, ZMetaDataTypes.ModuleId);
		}
	}
}
