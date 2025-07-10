using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public static class ChineseUtils
	{
		public static ZString ConvertDebitCreditToChinese(ZString debitCredit)
		{
			return debitCredit.Trim() == Constants.DebitCredit.Credit ? (NoResString)"贷" : (NoResString)"借";
		}

		public static ZString GetCurrencyNameForChinese(BusinessObjectFactory factory, ZString currencyCode)
		{
			RefCurrency currency = factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, currencyCode));
			if (currency != null)
			{
				return currency.RX_DescMultilingual.ToString(SharedConstants.Languages.ChineseSimplified);
			}
			return ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China standard special word.")]
		public static ZString GetGLAccountTypeFromNumber(ZInt firstNumber, bool isOldType = true)
		{
			//return AccountingMasterFilesRegistry.Instance.ChinaAccountTypeRegistryItem.Value.GetDescriptionFromFirstNumber(FirstNumber);
			ZString gLAccountType = ZString.Empty;
			switch (firstNumber)
			{
				case 1:
					gLAccountType = "资产类";
					break;
				case 2:
					gLAccountType = "负债类";
					break;
			}

			if (isOldType)
			{
				switch (firstNumber)
				{
					case 3:
						gLAccountType = "所有者权益类";
						break;
					case 4:
						gLAccountType = "成本费用类";
						break;
					case 5:
						gLAccountType = "损益类";
						break;
				}
			}
			else
			{
				switch (firstNumber)
				{
					case 3:
						gLAccountType = "共同类";
						break;
					case 4:
						gLAccountType = "权益类";
						break;
					case 5:
						gLAccountType = "成本类";
						break;
					case 6:
						gLAccountType = "损益类";
						break;
				}
			}
			return gLAccountType;
		}
	}
}
