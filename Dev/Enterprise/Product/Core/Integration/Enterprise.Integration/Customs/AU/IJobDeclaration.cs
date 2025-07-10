using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class AU
		{
			public interface IJobDeclaration : IBaseJobDeclaration
			{
				bool IsQuarantine { get; }
				bool DeclarationHasEntryHeader { get; }
				ZBool IsNEXDOCSActive { get; }
			}
		}
	}
}
