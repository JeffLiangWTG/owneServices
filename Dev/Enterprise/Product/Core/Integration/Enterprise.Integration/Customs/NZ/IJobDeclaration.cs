using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface IJobDeclaration : IBaseJobDeclaration
			{
				ZString JE_TSWCombinedStatusDesc { get; }
				ZString JE_MessageSubType { get; set; }
			}
		}
	}
}
