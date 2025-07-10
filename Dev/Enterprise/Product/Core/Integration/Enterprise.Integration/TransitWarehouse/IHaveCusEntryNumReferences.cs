using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs;

namespace Enterprise.Integration
{
	public interface IHaveCusEntryNumReferences : IBusiness
	{
		ICusEntryNumReferenceCollection CusEntryNumReferences { get; }
		ZGuid PK { get; }
	}
}
