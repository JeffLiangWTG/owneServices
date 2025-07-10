using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Interface.G3;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3PresentGoodMessageWrapper : G3CommonSendMessageWrapper, IG3PresentGoodMessageDataProvider
	{
		public G3PresentGoodMessageWrapper(IEnumerable<AsycudaBill> bills, ICertificateProvider certificate, string localReferenceNumber) : base(bills, certificate, localReferenceNumber)
		{
		}

		public IG3PresentGoodHeader Header => CachedValueHelper.GetValue(ref goodHeader, () => new G3PresentGoodHeaderWrapper(bills, localReferenceNumber));
		CachedValue<IG3PresentGoodHeader> goodHeader;
	}
}
