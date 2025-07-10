
using CargoWise.Types;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	public interface ISequencedValueObject
	{
		ZInt Sequence { get; set; }
		bool SequenceSpecified { get; set; }
	}
}
