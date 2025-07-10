using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Interface.G3;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3RevokeGoodHeaderWrapper : G3CommonHeaderWrapper, IG3RevokeGoodHeader
	{
		public G3RevokeGoodHeaderWrapper(IEnumerable<AsycudaBill> bills, IDictionary<ZGuid, IDocumentsCommon> revokeReasonDictionary, string localReferenceNumber) : base(bills, localReferenceNumber)
		{
			this.revokeReasonDictionary = Argument.NotNull(revokeReasonDictionary, nameof(revokeReasonDictionary));
		}

		readonly IDictionary<ZGuid, IDocumentsCommon> revokeReasonDictionary;

		public IReadOnlyCollection<IG3RevokeMasterConsignment> MasterConsignment => masterConsignment ??= new List<IG3RevokeMasterConsignment> { new G3RevokeMasterConsignmentWrapper(bills, revokeReasonDictionary) }.AsReadOnly();
		IReadOnlyCollection<IG3RevokeMasterConsignment> masterConsignment;
	}
}
