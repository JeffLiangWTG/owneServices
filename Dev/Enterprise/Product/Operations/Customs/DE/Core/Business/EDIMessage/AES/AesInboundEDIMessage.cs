using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class AesInboundEDIMessage<TIDataProvider> : DEEDIMessage, IInboundMessageDataProvider<TIDataProvider>
	   where TIDataProvider : IDataProvider
	{
		public AesInboundEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return ObjectFactory.GetType<Integration.Customs.DE.ICusEntryHeader>();
		}

		public virtual TIDataProvider DataProvider => this.DataProvider<TIDataProvider>();

		public virtual List<AttachedDocument> AttachedDocuments => this.AttachedDocuments<TIDataProvider>();

		public (bool Success, ResponseMessageDetails ResponseMessageDetails) GetResponseMessageDetails(string applicationReference)
		{
			var success = AesResponseMessageDetails.Instance.ResponseMessages.TryGetValue(applicationReference, out var responseMessageDetails);
			return (success, responseMessageDetails);
		}

		protected override string GetMessageReferenceNumber() => string.Empty;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.DECustomsAesSystem;
			EM_MessageType = EDIMessageTypeList.Codes.AES;
		}
	}
}
