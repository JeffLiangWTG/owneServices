using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.IL.Business;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.IL.GUI
{
	public class ConsolCustomsMessagingPlugIn : CustomsMessagingPlugInBase
	{
		readonly ForwardingConsol consol;

		public ConsolCustomsMessagingPlugIn(ForwardingConsol consol) : base(consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			consol.JK_RL_NKDischargePortInfo.ValueChanged += OnChangeTheVisibilityRequired;
			ChangeTheVisibility();
		}

		protected override IEnumerable<MenuItem> GetMenuItems()
		{
			if (ILCustomsDataRegistry.Instance.EnableILGatePassMovements.Value)
			{
				var gatePassMovementMenuItem = new ZMenuItem(GatepassMovementName);
				gatePassMovementMenuItem.AddFormsMenuItems(consol, ModuleIdentifier, CreateSendGatePassMovementMenuItemInfos());

				yield return gatePassMovementMenuItem;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				consol.JK_RL_NKDischargePortInfo.ValueChanged -= OnChangeTheVisibilityRequired;
			}
		}

		protected override bool IsEnabled => consol.DischargePort?.Country?.Code.ToString() == Core.Constants.CountryCodes.Israel;

		protected override ModuleIdentifier ModuleIdentifier => ModuleIDs.JobConsol;

		IEnumerable<IMenuItemInfo> CreateSendGatePassMovementMenuItemInfos()
		{
			yield return new SystemMenuItemInfo
			{
				ID = ConsolSystemFormMenuItems.SendGatePassMovementPK
			};
		}

		static string GatepassMovementName => ResString.GetMultilingualString("FBE98AF1-9BEB-4616-B48F-FA5A52F8F36B", "Gatepass Movement");
	}
}
