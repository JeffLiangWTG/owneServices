using System.Collections.Generic;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GridLayoutContextKeyProviderHelperDuplicateKey : GridLayoutContextKeyProviderHelper
	{
		protected override IEnumerable<DataGridLayoutContextKeyProvider> GetKeyProvidersForStmModuleFilter(ZGrid grid, bool getLegacyLayouts = true)
		{
			var result = new List<DataGridLayoutContextKeyProvider>();
			result.Add(new DataGridLayoutContextKeyProvider(grid));
			result.Add(new DataGridLayoutContextKeyProvider(grid));

			return result;
		}
	}
}
