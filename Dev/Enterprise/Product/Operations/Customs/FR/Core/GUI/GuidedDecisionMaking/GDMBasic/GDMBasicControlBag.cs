using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.GDM
{
	public class GDMBasicControlBag : ControlBag
	{
		public static GDMBasicControlBag Instance => instance ?? (instance = new GDMBasicControlBag());

		[ThreadStatic]
		static GDMBasicControlBag instance;

		protected override Control CreateTemplate() => new GDMBasicUserControl();

		GDMBasicControlBag()
		{
			RegionOrTerritoryOfDestinationDropEdit = RegisterControl(nameof(GDMBasicUserControl.RegionOrTerritoryOfDestinationDropEdit));
		}

		public ControlReference RegionOrTerritoryOfDestinationDropEdit { get; }
	}
}
