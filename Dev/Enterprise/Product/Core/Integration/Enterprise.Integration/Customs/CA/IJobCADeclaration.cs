using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface IJobCADeclaration
			{
				ZGuid PK { get; }
				ZInt CAD_ClusterKey { get; set; }
				ZGuid CAD_JE { get; set; }
				ZDateTime CAD_SystemCreateTimeUtc { get; set; }
				ZString CAD_SystemCreateUser { get; set; }
				ZDateTime CAD_SystemLastEditTimeUtc { get; set; }
				ZString CAD_SystemLastEditUser { get; set; }
			}
		}
	}
}
