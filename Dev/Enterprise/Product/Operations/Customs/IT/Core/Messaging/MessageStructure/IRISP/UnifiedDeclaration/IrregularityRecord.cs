using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

public class IrregularityRecord
{
	public ZInt ItemNumber { get; protected set; }
	public ZString FieldIdentificative { get; protected set; }
	public ZInt SequentialNumberOfFieldInRecord { get; protected set; }
	public ZString ErrorType { get; protected set; }
	public ZInt ControlNumber { get; protected set; }
	public ZString ErrorDescription { get; protected set; }

	public ZString FieldSadBoxNumber
	{
		get
		{
			const char zero = '0';
			const char dot = '.';
			var firstPart = FieldIdentificative.Left(2).TrimStart(zero);
			var secondPart = FieldIdentificative.Right(2).TrimStart(zero);
			return ZString.Format("{0}.{1}", firstPart, secondPart).TrimEnd(dot);
		}
	}

	internal void Load(ZString line)
	{
		ItemNumber = ZInt.Parse(line.SubStringAndTrim(1, 2));
		FieldIdentificative = line.SubStringAndTrim(3, 4);
		SequentialNumberOfFieldInRecord = ZInt.Parse(line.SubStringAndTrim(8, 2));
		ErrorType = line.SubStringAndTrim(10, 1);
		ControlNumber = ZInt.Parse(line.SubStringAndTrim(12, 2));
		ErrorDescription = line.Substring(15).Trim();
	}
}
