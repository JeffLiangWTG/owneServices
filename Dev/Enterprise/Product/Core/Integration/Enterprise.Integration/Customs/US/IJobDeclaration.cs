using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public interface IJobDeclaration : IBaseJobDeclaration
			{
				ZDateTime US_DateOfExport { get; set; }
				ZBool US_EnableENS { get; set; }
				ZString US_EntryType { get; set; }
			}
		}
	}
}