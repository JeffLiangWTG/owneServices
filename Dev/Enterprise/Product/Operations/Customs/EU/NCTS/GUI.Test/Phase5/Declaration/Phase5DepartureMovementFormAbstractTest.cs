using System;
using System.Windows.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestsSubclassesOf(typeof(Phase5DepartureMovementForm))]
	public abstract class Phase5DepartureMovementFormAbstractTest<TParent> : ZFormBasherTest
		where TParent : NctsHeader
	{
		[RequiresSTA]
		public void TestSupportsInvoicingPluginAkaBillingTab()
		{
			using (var form = (Phase5DepartureMovementForm)GetFormToBashCore())
			{
				form.Show();
				var jobInvoicingPlugin = form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				AssertNoExceptionThrown("Invoicing (billing) tab page", () => jobInvoicingPlugin.SelectTabPage());
			}
		}

		protected sealed override Form GetFormToBashCore()
		{
			var header = Factory.New<TParent>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			PerformExtraNctsHeaderConfiguration(header);
			header.Bills.AddNew().GoodsItems.AddNew().Packages.AddNew();
			Factory.Save();
			var form = (Phase5DepartureMovementForm)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()), header);
			form.ControllerID = ControllerIDs.Customs.EU.NctsMovementController;
			return form;
		}

		protected virtual void PerformExtraNctsHeaderConfiguration(TParent header) { }
	}
}
