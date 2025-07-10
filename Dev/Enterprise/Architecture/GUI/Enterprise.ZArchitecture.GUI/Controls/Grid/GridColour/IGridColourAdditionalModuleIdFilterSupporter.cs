using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IGridColourAdditionalModuleIdFilterSupporter
	{
		void AddAdditionalFilter(ZQuery moduleIdQuery, SchemaColumn schemaColumn);
	}
}
