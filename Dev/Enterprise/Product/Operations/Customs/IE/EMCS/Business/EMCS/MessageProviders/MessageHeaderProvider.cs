using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public abstract class MessageHeaderProvider<T> : IEMCSMessageHeader
		where T : IEMCSHeader
	{
		protected MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration)
		{
			this.emcsJobDeclaration = Argument.NotNull(emcsJobDeclaration, nameof(emcsJobDeclaration));
		}
		protected readonly EMCSJobDeclaration emcsJobDeclaration;

		public string Recipient => senderAndRecipientCode;

		public string Sender => senderAndRecipientCode;

		public DateTime PreparationDateTime => ZDateTime.UtcNow.ToDateTime();

		public string TimeOfPreparation => ZDateTime.UtcNow.TimeOfDay.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture);

		public string MessageIdentifier => EDIMessage.MessageNumberPlaceHolder;

		public string CorrelationIdentifier => string.Empty;

		public IEMCSHeader Header => header ?? (header = (IEMCSHeader)Activator.CreateInstance(typeof(T), emcsJobDeclaration));
		protected IEMCSHeader header;

		const string senderAndRecipientCode = "NDEA.IE";
	}
}
