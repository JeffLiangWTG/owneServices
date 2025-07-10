using System.Collections;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class PreviousDocumentLookups : EU.H7.Business.PreviousDocumentLookups
{
	public PreviousDocumentLookups(PreviousDocument parent)
		: base(parent)
	{
	}

	public override ICollection CodeList => Factory.GetCachedValue<ESH7PreviousDocumentCodeList>();

	protected new PreviousDocument Parent => (PreviousDocument)base.Parent;
}
