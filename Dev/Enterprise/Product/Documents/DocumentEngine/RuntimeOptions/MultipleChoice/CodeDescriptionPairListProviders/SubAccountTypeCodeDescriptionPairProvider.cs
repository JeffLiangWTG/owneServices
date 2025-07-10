using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class SubAccountTypeCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var list = new Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.SubAccountTypeList();
			return list;
		}
	}
}
