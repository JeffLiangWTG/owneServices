using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public class ERRNCKErrorProvider : IERRNCKError
	{
		public ERRNCKErrorProvider(DEERRGError error)
		{
			this.error = Argument.NotNull(error, nameof(error));
		}
		readonly DEERRGError error;

		public string Code => error.errorCode;

		public string Pointer => error.errorPointer;

		public string Text => error.errorText;

		public string OriginalValue => error.originalAttributeValue;
	}
}
