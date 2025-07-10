using Enterprise.ZArchitecture.Core;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.GUI.Tests
{
	public static class TestHelper
	{
		public static CodeDescriptionPairListProvider GetCodesProviderForTesting()
		{
			return new CodeDescriptionPairListProvider(() =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(SharedConstants.DataProcessingType.Code.OrganizationMatching, SharedConstants.DataProcessingType.Description.OrganizationMatching);
				list.AddPair(SharedConstants.DataProcessingType.Code.ProductCodeMatching, SharedConstants.DataProcessingType.Description.ProductCodeMatching);
				list.AddPair(SharedConstants.DataProcessingType.Code.NotifyDownstreamServices, SharedConstants.DataProcessingType.Description.NotifyDownstreamServices);
				return list;
			});
		}
	}
}
