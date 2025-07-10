using System.Collections.Generic;
using System.Data;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EmcsInboundEDIMessage<TIDataProvider> : DEEDIMessage, IInboundMessageDataProvider<TIDataProvider>
	   where TIDataProvider : IDataProvider
	{
		public EmcsInboundEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public virtual TIDataProvider DataProvider => this.DataProvider<TIDataProvider>();

		public virtual List<AttachedDocument> AttachedDocuments => this.AttachedDocuments<TIDataProvider>();

		public (bool Success, ResponseMessageDetails ResponseMessageDetails) GetResponseMessageDetails(string applicationReference)
		{
			var success = EmcsResponseMessageDetails.Instance.ResponseMessages.TryGetValue(applicationReference, out var responseMessageDetails);
			return (success, responseMessageDetails);
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get => base.EM_MessageInterpretation.EncodeToHTMLFormat().Replace("&gt;&lt;", "&gt;\n&lt;");
			set => base.EM_MessageInterpretation = value;
		}

		protected override string GetMessageReferenceNumber() => string.Empty;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.DECustomsEmcsSystem;
			EM_MessageType = EDIMessageTypeList.Codes.EMCS;
		}
	}
}
