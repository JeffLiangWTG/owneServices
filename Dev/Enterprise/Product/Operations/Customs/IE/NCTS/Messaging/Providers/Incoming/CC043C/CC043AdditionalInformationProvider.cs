using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043AdditionalInformationProvider
	{
		public CC043AdditionalInformationProvider(AdditionalInformationType02 additionalInformation)
		{
			this.additionalInformation = Argument.NotNull(additionalInformation, nameof(additionalInformation));
		}

		readonly AdditionalInformationType02 additionalInformation;

		public ZString SequenceNumber => additionalInformation.SequenceNumber ?? ZString.Empty;
		public ZString Code => additionalInformation.Code ?? ZString.Empty;
		public ZString Text => additionalInformation.Text ?? ZString.Empty;
	}
}
