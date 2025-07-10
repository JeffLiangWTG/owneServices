
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IHavePortReferences : IBusiness
		{
			IPortReferenceCollection PortReferences { get; }
			ZGuid PK { get; }
		}
	}
}
