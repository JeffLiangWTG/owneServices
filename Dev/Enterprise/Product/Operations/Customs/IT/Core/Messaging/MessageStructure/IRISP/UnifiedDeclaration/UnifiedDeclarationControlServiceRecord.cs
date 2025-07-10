using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

public class UnifiedDeclarationControlServiceRecord
{
	public ZString InnerText { get; protected set; }
	public ZInt NumberOfRecordsRead { get; protected set; }
	public ZInt NumberOfRecordsRejected { get; protected set; }
	public ZInt NumberOfMessagesProcessed { get; protected set; }
	public ZInt NumberOfMessagesRejected { get; protected set; }
	public ZInt NumberOfProceduresProcessed { get; protected set; }
	public ZInt NumberOfProceduresRejected { get; protected set; }

	internal void Load(ZString line)
	{
		InnerText = line;
		NumberOfRecordsRead = ZInt.Parse(line.SubStringAndTrim(4, 3));
		NumberOfRecordsRejected = ZInt.Parse(line.SubStringAndTrim(11, 3));
		NumberOfMessagesProcessed = ZInt.Parse(line.SubStringAndTrim(18, 3));
		NumberOfMessagesRejected = ZInt.Parse(line.SubStringAndTrim(25, 3));
		NumberOfProceduresProcessed = ZInt.Parse(line.SubStringAndTrim(32, 3));
		NumberOfProceduresRejected = ZInt.Parse(line.SubStringAndTrim(39, 3));
	}
}
