using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface IConsolidateLVXDeclarationSupporter
			{
				ZBool SupportConsolidateLVXDeclaration { get; }
				ZString NotSupportConsolidateLVXDeclarationReason { get; }
				IProcessor CreateConsolidateLVXDeclarationProcessor();
			}
		}
	}
}
