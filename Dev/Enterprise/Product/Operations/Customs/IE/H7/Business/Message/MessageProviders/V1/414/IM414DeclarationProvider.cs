using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM414DeclarationProvider : IIM414DeclarationType
	{
		public IM414DeclarationProvider(MessageSendingObject messageSendingObject)
		{
			this.messageSendingObject = messageSendingObject;
		}
		protected readonly MessageSendingObject messageSendingObject;

		#region IIM414DeclarationType

		public string MRN => messageSendingObject.Bill.MovementReferenceNumber;

		public string DateOfInvalidationRequest => ZDateTime.Now.ToDateTime().ToString("yyyyMMdd");

		public string InvalidationReason => messageSendingObject.AmendmentInvalidationReason;

		#endregion

		public ICustomsOffices02 CustomsOffices => CachedValueHelper.GetValue(ref customsOfficesCached, () => new CustomsOffices02Provider((AsycudaManifestHeader)messageSendingObject.Bill.Header));
		CachedValue<ICustomsOffices02> customsOfficesCached;

		public IParties Parties => CachedValueHelper.GetValue(ref partiesCached, () => new PartiesProvider((AsycudaManifestHeader)messageSendingObject.Bill.Header));
		CachedValue<IParties> partiesCached;
	}
}
