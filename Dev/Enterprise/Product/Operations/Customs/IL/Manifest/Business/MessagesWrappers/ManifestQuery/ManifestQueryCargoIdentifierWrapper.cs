using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_820;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class ManifestQueryCargoIdentifierWrapper : ICargoIdentifier
	{
		ManifestQueryCargoIdentifierWrapper(AsycudaManifestQueryMessageSendingObject messageSendingObject)
		{
			this.messageSendingObject = messageSendingObject;
		}

		internal static ICargoIdentifier NewOrNull(AsycudaManifestQueryMessageSendingObject messageSendingObject)
			=> messageSendingObject == null ? null : new ManifestQueryCargoIdentifierWrapper(messageSendingObject);

		public string CargoIdentifierKey1 => messageSendingObject.ManifestNumber;

		public string CargoIdentifierKey2 => messageSendingObject.ParentDealNumber;

		public string CargoIdentifierKey3 => null;

		public int CargoIdentifierType => int.Parse(IL.Business.Constants.CargoIdentifierType.SeaDealImport);

		readonly AsycudaManifestQueryMessageSendingObject messageSendingObject;
	}
}
