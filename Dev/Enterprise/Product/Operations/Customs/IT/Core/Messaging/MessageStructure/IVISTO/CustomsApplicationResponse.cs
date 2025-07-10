using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageStructure.IVISTO;

public class CustomsApplicationResponse : IMrnProvider
{
	public ZString RecordType { get; protected set; }
	public ZString MessageCode { get; protected set; }
	public ZString Mrn { get; protected set; }
	public ZString ExportCustomsOffice { get; protected set; }
	public ZString EffectiveExitCustomsOffice { get; protected set; }
	public ZString EffectiveExitCustomsOfficeName { get; protected set; }
	public ZDate ExitOrRejectedExitDate { get; protected set; }
	public ZString ExitCustomsOfficeResult { get; protected set; }

	internal void Load(ZString line)
	{
		RecordType = line.SubStringAndTrim(0, 1);
		MessageCode = line.SubStringAndTrim(1, 8);
		Mrn = line.SubStringAndTrim(9, 18);
		ExportCustomsOffice = line.SubStringAndTrim(27, 8);
		EffectiveExitCustomsOffice = line.SubStringAndTrim(35, 8);
		EffectiveExitCustomsOfficeName = line.SubStringAndTrim(43, 35);
		ExitOrRejectedExitDate = (ZDate)line.SubStringAndTrim(78, 8).ParseToDateTimeWithFormat("ddMMyyyy");
		ExitCustomsOfficeResult = line.SubStringAndTrim(86, 40);
	}
}
