using System.Collections.Generic;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Interface.G3;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3PresentGoodHeaderWrapper : G3CommonHeaderWrapper, IG3PresentGoodHeader
	{
		public G3PresentGoodHeaderWrapper(IEnumerable<AsycudaBill> bills, string localReferenceNumber) : base(bills, localReferenceNumber)
		{
		}

		public IReadOnlyCollection<IG3PresentGoodMasterConsignment> MasterConsignment => masterConsignment ??= new List<IG3PresentGoodMasterConsignment> { new G3PresentGoodMasterConsignmentWrapper(bills) }.AsReadOnly();
		IReadOnlyCollection<IG3PresentGoodMasterConsignment> masterConsignment;
	}
}
