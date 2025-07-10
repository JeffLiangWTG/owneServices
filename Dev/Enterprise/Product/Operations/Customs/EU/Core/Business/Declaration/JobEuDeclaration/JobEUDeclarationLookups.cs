//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobEUDeclarationLookups
//
//    This class should be used for overriding collections in AutoJobEUDeclarationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobEUDeclarationLookups : AutoJobEUDeclarationLookups
	{
		public JobEUDeclarationLookups(AutoJobEUDeclaration parent) : base(parent)
		{
		}

		public ICollection AgreedPlaceCodeList => Declaration.AgreedPlaceCodeSupport
			? Parent.EUD_AgreedPlaceCode.Length == 2
				? new RefCountryCollection(Factory)
				: new RefUNLOCOCollection(Factory)
			: Factory.GetAgreedPlaceCodeList(Declaration.GetDefaultDataGroupingCode());

		protected new JobEUDeclaration Parent => (JobEUDeclaration)base.Parent;

		protected JobDeclaration Declaration => Parent.Declaration;
	}
}

