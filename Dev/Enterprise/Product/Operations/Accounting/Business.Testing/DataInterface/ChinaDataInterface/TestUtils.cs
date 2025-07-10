using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public static class TestUtils
	{
		public static AccGLAccountDescriptor AddGLHeaderDescriptor(BusinessObjectFactory factory, ZGuid gLPK, string accountNum, string accountDescr, string debitCredit, string reportCategory)
		{
			AccGLAccountDescriptor gLHeaderToAdd = factory.New(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			gLHeaderToAdd.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			gLHeaderToAdd.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			gLHeaderToAdd.AJ_AccountDescription = accountDescr;
			gLHeaderToAdd.AJ_LocalAccountNumber = accountNum;
			gLHeaderToAdd.AJ_DebitCredit = debitCredit;
			gLHeaderToAdd.AJ_ReportCategory = reportCategory;
			gLHeaderToAdd.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			gLHeaderToAdd.ParentGLHeaderPK = gLPK;
			return gLHeaderToAdd;
		}
	}
}