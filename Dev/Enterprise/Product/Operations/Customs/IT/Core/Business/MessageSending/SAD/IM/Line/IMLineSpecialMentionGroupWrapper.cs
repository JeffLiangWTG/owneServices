using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class IMLineSpecialMentionGroupWrapper : SADSpecialMentionGroupCommonWrapper, IIMLineSpecialMentionGroup
{
	public IMLineSpecialMentionGroupWrapper(CusEntryLine entryLine)
		: base(entryLine)
	{
	}

	public IPreviousAdministrativeReference PreviousProcedure
	{
		get
		{
			if (PreviousProcedureDocument != null)
			{
				return new SADPreviousAdministrativeReferenceWrapper(
					PreviousProcedureDocument.CSI_Procedure,
					PreviousProcedureDocument.ReferenceNumberWithoutCin,
					PreviousProcedureDocument.ReferenceNumberCin,
					PreviousProcedureDocument.CSI_DateOfIssue.Date,
					PreviousProcedureDocument.CSI_Status,
					PreviousProcedureDocument.CSI_CustomsOffice,
					PreviousProcedureDocument.CSI_LineNo
				);
			}
			return SADPreviousAdministrativeReferenceWrapper.Empty();
		}
	}

	public ZString SteelType => entryLine.SteelType == SteelTypeList.Codes._0 ? ZString.Empty : entryLine.SteelType;
}
