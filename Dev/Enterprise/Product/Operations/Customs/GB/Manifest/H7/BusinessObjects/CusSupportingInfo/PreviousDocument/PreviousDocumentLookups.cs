using System.Collections;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.H7.Business
{
	public class PreviousDocumentLookups : EU.H7.Business.PreviousDocumentLookups
	{
		public PreviousDocumentLookups(PreviousDocument parent)
			: base(parent)
		{
		}

		public override ICollection CodeList => Factory.GetCachedValue<PreviousDocumentCodeListCDS>();

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;
	}
}
