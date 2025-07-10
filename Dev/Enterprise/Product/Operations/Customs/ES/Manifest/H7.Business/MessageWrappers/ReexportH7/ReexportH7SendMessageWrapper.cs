using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class ReexportH7SendMessageWrapper : H7CommonSendMessageWrapper, IReexportH7MessageDataProvider
	{
		public ReexportH7SendMessageWrapper(AsycudaBill bill, ICertificateProvider certificate, ZString operationCode)
			: base(bill, certificate)
		{
			this.operationCode = operationCode;
		}

		public ZString OperationCode => operationCode;

		readonly ZString operationCode;

		public IPartyNameProvider Declarant => new H7PartyNameWrapper(Bill.Header.Declarant);

		public IReadOnlyCollection<ZString> DeclarationMRNCodes =>
			Bill.MovementReferenceNumber.IsEmpty ? Array.Empty<ZString>() : new List<ZString> { Bill.MovementReferenceNumber }.AsReadOnly();
	}
}
