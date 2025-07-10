using CargoWise.Types;
using Enterprise.Integration.Freight;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class AU
		{
			public interface IContainerMessagingData
			{
				ICommonContainer GetCommonContainer { get; }
				ZString GetErrorText();
				ZString GetWarningText();
			}
		}
	}
}
