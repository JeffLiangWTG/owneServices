using CargoWise.Integration;
using CargoWise.Types;
using static Enterprise.Customs.JP.Common.JPOutputInformationCodeList;

namespace Enterprise.Customs.JP.Common;

public partial class EDIMessage
{
	public ZBool IsExportPermitMessage => JPExportPermitCodeList.ContainsCode(EM_Calc_OutputInformationCode);

	public ZBool IsImportPermitMessage => Constants.DocumentMessageCodes.ImportPermitMessageCodes.Contains(EM_Calc_OutputInformationCode);

	public ZBool IsInspectionInformationMessage => JPInspectionInformationCodes.ContainsCode(EM_Calc_OutputInformationCode);

	public ZBool IsMismatchInformationMessage => EM_Calc_OutputInformationCode == JPOutputInformationCodeList.Codes.AAS0180;

	public ZBool IsMoveInNoticeMessage => EM_Calc_OutputInformationCode == JPOutputInformationCodeList.Codes.AAT0040;

	public ZBool IsTransshipmentNoticeSubmissionInformationMessage => EM_Calc_OutputInformationCode == JPOutputInformationCodeList.Codes.SAS0120;

	public ZBool IsCancellataionOfTransshipmentReportMessage => EM_Calc_OutputInformationCode == JPOutputInformationCodeList.Codes.SAS0740;

	public ZBool IsHBLCargoCancellationInformationMessage => EM_Calc_OutputInformationCode == JPOutputInformationCodeList.Codes.SAS0731;

	public ZBool IsEACNoticeInformationMessage => Constants.EACNoticeMessageCodes.Contains(EM_Calc_OutputInformationCode);

	public ZBool IsHBLCargoRegistrationInformationMessage => EM_Calc_OutputInformationCode == JPOutputInformationCodeList.Codes.SAS0711;

	public ICodeDescriptionPairList JPExportPermitCodeList => Factory.GetCachedValue<JPExportPermitCodeList>();

	public ICodeDescriptionPairList JPInspectionInformationCodes => Factory.GetCachedValue<JPInspectionInformationCodeList>();
}
