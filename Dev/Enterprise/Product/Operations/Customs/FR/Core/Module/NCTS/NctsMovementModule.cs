using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.FR.Module.NCTS;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module
{
	public class NctsMovementModule : EU.NCTS.Module.NctsMovementModule
	{
		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new NctsMovementFilterStripBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new NctsMovementFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			if (TP5InboundInterchangeImporter.IsMenuItemVisible())
			{
				result.Add(TP5InboundInterchangeImporter.GetNewMenuItem());
			}

			return result.ToArray();
		}

		TP5InboundInterchangeImporter TP5InboundInterchangeImporter => tP5InboundInterchangeImporter ??= new TP5InboundInterchangeImporter(Factory);
		TP5InboundInterchangeImporter tP5InboundInterchangeImporter;
	}
}
