using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class OneStopCarrierPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddRange(ObjectFactory.Get<Enterprise.Freight.Integration.IOneStopCarrierCodePairListProvider>().GetOneStopCarrierCodePairListForFilter());
			return result;
		}
	}
}
