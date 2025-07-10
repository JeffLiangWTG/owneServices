using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM414HeaderProvider : IIM414Header
	{
		public IM414HeaderProvider(MessageSendingObject messageSendingObject)
		{
			this.messageSendingObject = messageSendingObject;
		}

		protected MessageSendingObject messageSendingObject;

		public IIM414DeclarationType Declaration => CachedValueHelper.GetValue(ref declarationCached, () => new IM414DeclarationProvider(messageSendingObject));
		CachedValue<IIM414DeclarationType> declarationCached;

		#region CustomsOffices

		public string CustomsOfficeLodgement => messageSendingObject.Bill.Header.AMA_CustomsOffice;

		public string PresentationCustomsOffice => null;

		#endregion

		public ICustomsOffices02 CustomsOffices => Declaration.CustomsOffices;

		public IParties Parties => Declaration.Parties;
	}
}
