using System.Collections.Generic;
using System.Linq;
using Enterprise.Core;
using Enterprise.Core.Modules;
using Enterprise.Registry.Business;

namespace Enterprise.Security.Provider
{
	class VisualizerFormsSecurityInfoProvider : StmMenuItemsSecurityInfoProvider
	{
		public VisualizerFormsSecurityInfoProvider(SecurityInfoProvider parent, INamedModule module, StmMenuItemsForSecurity menuItems)
			: base(parent, module, new VisualizerFormsCheckpointHelper())
		{
			this.module = module;
			this.menuItems = menuItems;
		}

		readonly INamedModule module;
		readonly StmMenuItemsForSecurity menuItems;

		public override IEnumerable<SecurityInfoProvider> GetChildren()
		{
			var checkpoints = new List<SecurityInfoProvider>();

			for (int i = 0; i < menuItems.Count; i++)
			{
				checkpoints.Add(GetCheckpoint(i));
			}

			return checkpoints.Where(x => x != null);
		}

		VisualizerFormSecurityInfoProvider GetCheckpoint(int index)
		{
			var item = menuItems.GetValue(index);
			if (item.SU_MenuName == Constants.AddtionalHouseBillTypeMenu.YusenHBLMenuName
				|| item.SU_MenuName == Constants.AddtionalHouseBillTypeMenu.DHLHBLMenuName)
			{
				var registryItem = FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.Value
				.Cast<AdditionalHouseBillOfLadingType>()
				.FirstOrDefault(x => (x.Code == Constants.AddtionalHouseBillTypeMenu.Code.YusenHBL && item.SU_MenuName == Constants.AddtionalHouseBillTypeMenu.YusenHBLMenuName)
								|| (x.Code == Constants.AddtionalHouseBillTypeMenu.Code.DHLHBL && item.SU_MenuName == Constants.AddtionalHouseBillTypeMenu.DHLHBLMenuName));

				if (registryItem != null && !registryItem.Enable)
				{
					return null;
				}
			}

			return new VisualizerFormSecurityInfoProvider(this, module, menuItems, index, new VisualizerFormsCheckpointHelper());
		}
	}
}
