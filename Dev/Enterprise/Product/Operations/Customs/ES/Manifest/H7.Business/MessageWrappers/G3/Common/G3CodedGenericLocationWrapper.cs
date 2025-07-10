using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3CodedGenericLocationWrapper : ICodedGenericLocation
	{
		public G3CodedGenericLocationWrapper(AsycudaManifestHeader header)
		{
			this.header = header;
		}

		readonly AsycudaManifestHeader header;

		public ZString UNLOCOCode => null;

		public ZString CustomsOffice => null;

		public ICommonGNSS GPS => null;

		public ZString EconomicOperator => null;

		public ZString AuthorisationNumber => header.CusGoodsLocation.Address.AuthorisationNumber;

		public ZString AdditionalId => null;
	}
}
