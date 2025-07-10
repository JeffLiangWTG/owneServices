using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public static class FactoryExtension
{
	public static CusEntryNumber NewCusEntryNumber(this BusinessObjectFactory factory, BusinessObject parentBizObj, ZString entryType, ZString entryNum, ZDateTime? issueDate, string entryLineReference = null)
	{
		var cusEntryNumber = factory.New<CusEntryNumber>();
		cusEntryNumber.CE_EntryType = entryType;
		cusEntryNumber.CE_ParentID = parentBizObj.PK;
		cusEntryNumber.CE_ParentTable = parentBizObj.TableName;
		cusEntryNumber.CE_Category = "CUS";
		cusEntryNumber.CE_EntryNum = entryNum;
		cusEntryNumber.CE_EntryLineReference = entryLineReference;
		if (issueDate.HasValue)
		{
			cusEntryNumber.CE_IssueDate = issueDate.Value;
		}
		return cusEntryNumber;
	}

	public static CusAuthorisationHeader NewAuthorisation(this BusinessObjectFactory factory, OrgHeader permitHolder, ZString number, ZString type, OrgAddress permitAddress = null)
	{
		var authorisation = factory.New<CusAuthorisationHeader>();
		authorisation.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		authorisation.CPH_StartDate = ZDate.Today.AddDays(-1);
		authorisation.CPH_EndDate = ZDate.Today.AddDays(1);
		authorisation.CPH_Number = number;
		authorisation.CPH_OH_PermitHolder = permitHolder.PK;
		authorisation.CPH_OA_AppliesTo = permitAddress?.PK ?? ZGuid.Empty;
		authorisation.CPH_Type = type;
		return authorisation;
	}
}
