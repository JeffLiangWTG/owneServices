using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3GenericLocationWrapper : IGenericLocation
	{
		public G3GenericLocationWrapper(AsycudaManifestHeader header)
		{
			this.header = header;
		}

		readonly AsycudaManifestHeader header;

		public ZString Type => CusGoodsLocationTypeList.Codes.AuthorizedPlace;

		public ZString Qualifier => CusGoodsLocationQualifierList.Codes.AuthorizationNumber;

		public ICodedGenericLocation Coded => codedLocation ??= new G3CodedGenericLocationWrapper(header);
		G3CodedGenericLocationWrapper codedLocation;

		public IPartyAddressProvider Address => null;
	}
}
