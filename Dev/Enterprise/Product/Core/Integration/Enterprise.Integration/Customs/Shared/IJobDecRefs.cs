using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IJobDecRefs
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }

				ZGuid J3_JE { get; set; }
				ZString J3_ReferenceNumber { get; set; }
				ZString J3_ReferenceType { get; set; }
			}
		}
	}
}