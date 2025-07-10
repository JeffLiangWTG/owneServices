using System.Collections;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class PreviousDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentLookups
	{
		public PreviousDocumentLookups(PreviousDocument parent)
			: base(parent)
		{
		}

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

		public override ICollection CodeList => Factory.GetCachedValue<PreviousDocumentCodeListCDS>();
	}
}
