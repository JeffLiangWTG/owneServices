using System;
using CargoWise.Application;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class SchemeManagerForTestWithForm : GridColourSchemeManager
	{
		public SchemeManagerForTestWithForm(ZGrid grid)
			: base(grid)
		{
		}
		public IZGridColourCustomiseFormForTest customColorForm;

		protected override void ShowManageSchemeFormCore(GridColourScheme scheme, FilterStripBusinessObject filterStripBusinessObject, ZFilterStripCommonControl stripControl, Type businessEntityType)
		{
			customColorForm = ObjectFactory.Get<IZGridColourCustomiseFormForTest>("IZGridColourCustomiseFormForTest", scheme, filterStripBusinessObject, stripControl, businessEntityType);
			customColorForm.DisplayMode = ODisplayMode.Edit;
			customColorForm.Show();
		}
	}
}
