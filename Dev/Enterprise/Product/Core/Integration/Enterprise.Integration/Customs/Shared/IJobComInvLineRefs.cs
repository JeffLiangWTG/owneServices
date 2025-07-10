using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IJobComInvLineRefs
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }
				ZInt JG_ClusterKey { get; set; }

				ZGuid JG_JI { get; set; }
				ZString JG_ReferenceNumber { get; set; }
				ZString JG_ReferenceType { get; set; }
			}
		}
	}
}
