using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.JP.Common
{
	public class MessageNumberStrategy : IMessageNumberStrategy
	{
		public MessageNumberStrategy(EDIMessage message)
		{
			this.message = message;
		}

		public MessageNumberStrategy(EDIMessage message, IJPInboundMessageHeader header)
		{
			this.message = message;
			this.header = header;
		}

		readonly EDIMessage message;
		readonly IJPInboundMessageHeader header;
		const string ReceiveMessagePrefix = "JR";

		string IMessageNumberStrategy.GetMessageReferenceNumber()
		{
			if (message.IsTransmitMessage)
			{
				return new StringBuilder()
					.Append(message.ProcedureCode.PadRight(5, '0'))
					.Append((message.EM_LinkedObject as IInputReferenceProvider)?.InputReference.SubstringSafe(0, 10))
					.Append(GetNextEDIFACTNumber())
					.ToString();
			}
			else
			{
				return new StringBuilder()
					.Append(ReceiveMessagePrefix)
					.Append(header != null ? EDIMessage.GetEffectiveProcedureCode(header.ProcedureCode).PadRight(5, '0') : null)
					.Append(header != null ? Regex.Replace(header.OutputInformationCode?.PadRight(7, '_') ?? string.Empty, "[^a-zA-Z0-9]{1}", "_") : null)
					.Append(header != null ? new ZString(header.InputReference).SubstringSafe(0, 10) : null)
					.Append(GetNextEDIFACTNumber())
					.ToString();
			}
		}

		string GetNextEDIFACTNumber()
		{
			var factory = message.Factory;
			var currentCompany = GlbCompany.CurrentCompany;
			var sender = currentCompany.LicenceEnterpriseCode + currentCompany.LicenceServerID;
			var receiver = EDIMessage.MessageDestination;

			return Env.NumberFountains.EDIFACTNumberFountain("M", sender, receiver).GetNextFormatted(factory).PadLeft(11, '0');
		}
	}
}
