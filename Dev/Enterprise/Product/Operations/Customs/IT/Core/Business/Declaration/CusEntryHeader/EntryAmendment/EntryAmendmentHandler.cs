using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class EntryAmendmentHandler : AutoEntryAmendmentHandler
{
	public EntryAmendmentHandler(BusinessObjectFactory factory) : base(factory)
	{
	}

	public override ZBool ShowTotalEntryLines => true;

	protected override ZString HumanReadableNameCore => Res.GetString("B837118B-92B2-411A-90E7-8092FEC94EA2", "Entry Amendment");
}
