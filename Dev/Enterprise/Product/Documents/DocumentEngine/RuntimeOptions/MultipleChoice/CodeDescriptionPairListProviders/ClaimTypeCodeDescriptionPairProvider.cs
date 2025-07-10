using CargoWise.Application;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ClaimTypeCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return (ReadOnlyCodeDescriptionPairList)ObjectFactory.Get<IAccounting>().QueryClaimTypeCodeDescriptionPairList;
		}
	}
}
