using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public interface IErrorsRecord
			{
				ZString ErrorMessageIdentifier { get; }
				ZString NarrativeMessage { get; }
				ZDateTime StatusDate { get; }
			}
		}
	}
}
