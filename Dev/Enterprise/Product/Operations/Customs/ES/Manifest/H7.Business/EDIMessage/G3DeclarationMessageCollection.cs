using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Manifest.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3DeclarationMessageCollection : BusinessObjectCollection<G3EDIMessage>
	{
		public G3DeclarationMessageCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = new ZQuery();

			result.AddToFilter(EDIMessageSchema.EM_MessageType, G3MessageTypes.Codes.G3Revoke);
			result.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_MessageType, G3MessageTypes.Codes.G3Declaration);

			return result;
		}

		protected override bool AllowNewCore => false;
	}
}
