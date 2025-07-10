using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	/// <summary>
	/// A stub section configuration for when we have deprecated an old section configuration.
	/// </summary>
	public class UnknownBoardSectionConfiguration : NonPersistentBusinessObject, IBoardSectionConfigurationBizo
	{
		public UnknownBoardSectionConfiguration()
		{
		}

		public UnknownBoardSectionConfiguration(BusinessObjectFactory factory)
		 : base(factory)
		{
		}

		public ZString SectionName => Res.GetString("5637ae76-e424-4563-afcb-2d936b40028e", "Unknown Section");

		public ZPropertyInfo SectionNameInfo => GetZPropertyInfo(nameof(SectionName));

		public void CopyConfigurationPropertiesToNewSection(IBMBoardSection section)
		{
		}
	}
}
