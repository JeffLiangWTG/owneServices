using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public class EXPCTLLineProvider : IEXPCTLLine
	{
		public EXPCTLLineProvider(DEXPLDTypeOfControls line)
		{
			this.line = Argument.NotNull(line, nameof(line));
		}
		readonly DEXPLDTypeOfControls line;

		public string SequenceNumber => line.sequenceNumber;

		public string ControlMeasureType => line.type.XmlEnumToString();

		public string Annotation => line.text;
	}
}
