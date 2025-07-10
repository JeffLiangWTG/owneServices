using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Interface.G3;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3RevokeMasterConsignmentWrapper : G3MasterConsignmentWrapper, IG3RevokeMasterConsignment
	{
		public G3RevokeMasterConsignmentWrapper(IEnumerable<AsycudaBill> bills, IDictionary<ZGuid, IDocumentsCommon> revokeReasonDictionary) : base(bills)
		{
			this.revokeReasonDictionary = Argument.NotNull(revokeReasonDictionary, nameof(revokeReasonDictionary));
		}

		readonly IDictionary<ZGuid, IDocumentsCommon> revokeReasonDictionary;

		public IReadOnlyCollection<IG3RevokeHouseConsignment> HouseConsignment => houseConsignment ??= bills.Select(o => new G3RevokeHouseConsignmentWrapper(o, revokeReasonDictionary.GetValueOrDefault(o.PK))).ToList().AsReadOnly();
		IReadOnlyCollection<IG3RevokeHouseConsignment> houseConsignment;

		protected override IReadOnlyCollection<ICommonDocumentGoodsItemId> PreviousDocumentCore()
		{
			return previousDocument ?? (previousDocument = new List<ICommonDocumentGoodsItemId> { new G3CommonPreviousDocumentWrapper(header, true), new G3CommonPreviousDocumentWrapper(header, false) }.AsReadOnly());
		}
	}
}
