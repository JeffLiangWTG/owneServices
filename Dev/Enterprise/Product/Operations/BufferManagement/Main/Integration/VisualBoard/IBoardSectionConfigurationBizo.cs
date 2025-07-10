using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBoardSectionConfigurationBizo : IBusiness
	{
		ZString SectionName { get; }
		ZPropertyInfo SectionNameInfo { get; }
		void CopyConfigurationPropertiesToNewSection(IBMBoardSection section);
	}
}
