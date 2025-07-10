using Enterprise.ZArchitecture.Core;
using IMonthlyClosingDeclarationTypeListProvider = Enterprise.Integration.Customs.DE.IMonthlyClosingDeclarationTypeListProvider;

namespace Enterprise.Customs.DE.Business
{
	public class MonthlyClosingDeclarationTypeListProvider : DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider, IMonthlyClosingDeclarationTypeListProvider
	{
		#region ICodeDescriptionPairListProvider
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			if (types == null)
			{
				types = new MonthlyClosingDeclarationTypeList();
			}
			return types;
		}
		#endregion
		MonthlyClosingDeclarationTypeList types;
	}
}
