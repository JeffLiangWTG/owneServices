using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageAmendmentHandler : AutoEntryAmendmentHandler
{
	public TemporaryStorageAmendmentHandler(BusinessObjectFactory factory) : base(factory)
	{
	}

	public override ZBool ShowTotalEntryLines => false;

	protected override ZString HumanReadableNameCore => Res.GetString("E0FF6C58-64E7-4C41-8131-8F262B4733AA", "Temporary Storage Amendment");
}
