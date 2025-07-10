using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;

namespace Enterprise.Customs.IE.Messaging
{
	public class NegativeAcknowledgementErrorProvider : INegativeAcknowledgementError
	{
		public NegativeAcknowledgementErrorProvider(IXmlNegativeAcknowledgement error)
		{
			this.error = Argument.NotNull(error, nameof(error));
		}

		public string LineNumber => error.ErrorLineNumber;

		public string Reason => error.ErrorReason;

		public string ColumnNumber => error.ErrorColumnNumber;

		readonly IXmlNegativeAcknowledgement error;
	}
}
