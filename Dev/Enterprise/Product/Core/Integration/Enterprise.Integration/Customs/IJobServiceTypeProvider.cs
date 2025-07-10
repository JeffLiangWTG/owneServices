using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IJobServiceTypeProvider
		{
			ICodeDescriptionPairList GetJobServiceTypes();
			ZBool ServiceTypeNeedsToBeUnique(ZString serviceType);
		}

		public static partial class TW
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface IJobServiceTypeProvider : Customs.IJobServiceTypeProvider
			{
			}
		}
	}
}
