
using CargoWise.EntityFramework;

using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;

namespace Enterprise.Client.Wow
{
	public class WoolworthsCusContainerCollection : BaseCusContainerCollection<CusContainer>
	{
		public WoolworthsCusContainerCollection(WoolworthsJobDeclaration declaration, BusinessObjectFactory factory) : base(declaration, factory)
		{
			Declaration = declaration;
		}

		public new readonly WoolworthsJobDeclaration Declaration;

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (Declaration.JE_TransportMode == Core.Constants.TransportModes.Air || Declaration.OnDeletingCusContainer())
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}
	}
}
