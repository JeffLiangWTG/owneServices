using System;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public abstract class MessageHeaderProvider<T> : IEMCSMessageHeader where T : IEMCSHeader
	{
		protected MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration)
		{
			this.emcsJobDeclaration = Argument.NotNull(emcsJobDeclaration, nameof(emcsJobDeclaration));
		}
		protected readonly EMCSJobDeclaration emcsJobDeclaration;

		public string Recipient => SenderAndRecipientCode;

		public string Sender => SenderAndRecipientCode;

		public DateTime PreparationDateAndTime => ZDateTime.UtcNow.ToDateTime();

		public string MessageIdentifier => EDIMessage.MessageNumberPlaceHolder;

		public string CorrelationIdentifier => EDIMessage.MessageNumberPlaceHolder;

		public IEMCSHeader Header => header ?? (header = (IEMCSHeader)Activator.CreateInstance(typeof(T), emcsJobDeclaration));
		protected IEMCSHeader header;

		const string SenderAndRecipientCode = "NDEA.GB";
	}
}
