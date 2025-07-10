using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DE.Business
{
	public class AtlasInboundEDIMessage<TIDataProvider> : DEEDIMessage, IInboundMessageDataProvider<TIDataProvider>
	   where TIDataProvider : IDataProvider
	{
		public AtlasInboundEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public virtual TIDataProvider DataProvider => this.DataProvider<TIDataProvider>();

		public virtual List<AttachedDocument> AttachedDocuments => this.AttachedDocuments<TIDataProvider>();

		public virtual (bool Success, ResponseMessageDetails ResponseMessageDetails) GetResponseMessageDetails(string applicationReference)
		{
			var success = ATLASResponseMessageDetails.Instance.ResponseMessages.TryGetValue(applicationReference, out var responseMessageDetails);
			return (success, responseMessageDetails);
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return ObjectFactory.GetType<Integration.Customs.DE.ICusTempStorageRegHeader>();
		}

		protected override string GetMessageReferenceNumber() => string.Empty;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.DECustomsAtlasSystem;
		}
	}
}
