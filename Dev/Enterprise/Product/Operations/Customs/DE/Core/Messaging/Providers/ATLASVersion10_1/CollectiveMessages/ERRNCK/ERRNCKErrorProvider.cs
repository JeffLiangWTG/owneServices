using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public sealed class ERRNCKErrorProvider : IERRNCKError
	{
		public ERRNCKErrorProvider(DEERRFError error)
		{
			this.error = Argument.NotNull(error, nameof(error));
		}
		readonly DEERRFError error;

		public string Code => error.Code;

		public string Pointer => error.Pointer;

		public string Text => error.Text;

		public string OriginalValue => error.OriginalValue;
	}
}
