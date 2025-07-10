using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsAmendmentHandler : AutoEntryAmendmentHandler
{
	public NctsAmendmentHandler(BusinessObjectFactory factory) : base(factory)
	{
	}

	public override ZBool ShowTotalEntryLines => false;

	protected override ZString HumanReadableNameCore => Res.GetString("482CA6B9-2225-45D5-A2EE-87887D6E14D0", "NCTS Amendment");
}
