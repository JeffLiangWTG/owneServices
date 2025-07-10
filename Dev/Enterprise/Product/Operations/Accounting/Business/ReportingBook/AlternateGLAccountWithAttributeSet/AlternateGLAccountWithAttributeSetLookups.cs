using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AlternateGLAccountWithAttributeSetLookups : ZLookups
	{
		public AlternateGLAccountWithAttributeSetLookups(AlternateGLAccountWithAttributeSet parent) : base(parent)
		{
		}

		public CodeDescriptionPairList AccountTypeList => BaseLookUps.AccountTypeList;

		public CodeDescriptionPairList DebitCreditList => BaseLookUps.DebitCreditList;

		public CodeDescriptionPairList ReportSectionList
		{
			get
			{
				var reportSectionList = BaseLookUps.ReportSectionList;
				if (Parent is AlternateGLAccountWithAttributeSet alternateGLAccountWithAttributeSet && alternateGLAccountWithAttributeSet.Chart != null && alternateGLAccountWithAttributeSet.Chart.AAC_BalanceSheetStyle == AccAlternateChartLookups.BalanceSheetStyleCode.EAL)
				{
					reportSectionList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.Assets, AccGLHeader.Constants.SectionTypes.Descriptions.Assets);
					reportSectionList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.Liabilities, AccGLHeader.Constants.SectionTypes.Descriptions.Liabilities);
				}
				else
				{
					reportSectionList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.Liabilities, BaseAlternateGLAccountLookups.Constants.ELASectionTypes.Descriptions.Liabilities);
					reportSectionList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.Assets, BaseAlternateGLAccountLookups.Constants.ELASectionTypes.Descriptions.Assets);
				}

				return reportSectionList;
			}
		}

		public AccAlternateGLAccountCollection AlternateNums => BaseLookUps.AlternateNums;

		public AccAlternateGLAccountCollection ConsolidationNums => BaseLookUps.ConsolidationNums;

		public AccAlternateGLAccountCollection HeaderDependsOnTotals => BaseLookUps.HeaderDependsOnTotals;

		public AccAlternateGLAccountCollection PercentNums => BaseLookUps.PercentNums;

		BaseAlternateGLAccountLookups BaseLookUps => baseLookUps ?? (baseLookUps = new BaseAlternateGLAccountLookups(Parent.Factory));

		BaseAlternateGLAccountLookups baseLookUps;
	}
}
