using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Interface.G3;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3RevokeGoodMessageWrapper : G3CommonSendMessageWrapper, IG3RevokeGoodMessageDataProvider
	{
		public G3RevokeGoodMessageWrapper(IEnumerable<AsycudaBill> bills, IDictionary<ZGuid, IDocumentsCommon> revokeReasonDictionary, ICertificateProvider certificate, string localReferenceNumber) : base(bills, certificate, localReferenceNumber)
		{
			this.revokeReasonDictionary = Argument.NotNull(revokeReasonDictionary, nameof(revokeReasonDictionary));
		}

		readonly IDictionary<ZGuid, IDocumentsCommon> revokeReasonDictionary;

		public IG3RevokeGoodHeader Header => CachedValueHelper.GetValue(ref goodHeader, () => new G3RevokeGoodHeaderWrapper(bills, revokeReasonDictionary, localReferenceNumber));
		CachedValue<IG3RevokeGoodHeader> goodHeader;
	}
}
