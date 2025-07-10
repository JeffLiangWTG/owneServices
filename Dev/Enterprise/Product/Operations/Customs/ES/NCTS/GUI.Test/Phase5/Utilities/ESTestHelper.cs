using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	public static class ESTestHelper
	{
		public static IDisposable TemporarilySetTransitionPeriod(bool isActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod,
																	  Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
																	  ZDateTime.Today,
																	  isActive);

		public static ZTabPage GetTabPage(this ZUserControl userControl, string tabControlName, string tabToFind)
		{
			var tabControl = (ZTabControl)userControl.Controls.Find(tabControlName, searchAllChildren: true).FirstOrDefault();
			return tabControl.GetTabPage(tabToFind);
		}
	}
}
