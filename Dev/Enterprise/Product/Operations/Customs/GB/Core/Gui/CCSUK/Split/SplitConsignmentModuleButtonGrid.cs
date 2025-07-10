using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public class SplitConsignmentModuleButtonGrid : ZModuleButtonGrid
	{
		public SplitConsignmentModuleButtonGrid()
		{
			ShowAttachButton = false;
			ShowDetachButton = false;
			ShowEditButton = true;
			ShowNewButton = false;
			ReadOnly = false;
			InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			NameOfAGridElement = Enterprise.Customs.GB.GUI.Res.GetData("12345678-77F0-4D0E-B584-3E36F4CD5D89", "Split");
			AlwaysRequiresSaveBeforeEdit = true;
		}

		protected override bool AllowOpenInEditFormEvenIfListIsReadOnly
		{
			get { return true; }
		}
	}
}
