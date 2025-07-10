using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Manifest.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3EDIMessageCollection : EDIMessageCollection
	{
		public G3EDIMessageCollection(ASYCUDA.Business.AsycudaManifestHeader manifest, BusinessObjectFactory factory) : base(manifest, factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = new ZQuery(EDIMessageSchema.EM_MessageType, G3MessageTypes.Codes.G3Revoke);
			result.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_MessageType, G3MessageTypes.Codes.G3Declaration);
			return result;
		}
	}
}
