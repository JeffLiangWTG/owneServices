using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestsSubclassesOf(typeof(Phase5ArrivalMovementForm))]
	public abstract class Phase5ArrivalMovementFormAbstractTest<TParent> : ZFormBasherTest
		where TParent : NctsHeader
	{
		[RequiresSTA]
		public void TestSealsStateDropEditHasListAttribute()
		{
			using (var form = (Phase5ArrivalMovementForm)GetFormToBashCore())
			{
				using (var control = new ArrivalNotificationDetailsUserControl())
				{
					var propertyToCheckName = control.FindSingle<ZDropEdit>("StateOfSealsDropEdit").BindTo.Split('.');
					var headerType = typeof(NctsHeader);
					var propertyToCheckFirstTermType = headerType.GetProperty(propertyToCheckName.First()).PropertyType;
					var propertyToCheck = propertyToCheckFirstTermType.GetProperty(propertyToCheckName.Last());
					var listAttribute = (ListAttribute)System.Attribute.GetCustomAttributes(propertyToCheck, typeof(ListAttribute), false).FirstOrDefault();
					AssertNotNull($"{propertyToCheckName} should have a List attribute", listAttribute);
				}
			}
		}

		public void TestSupportsInvoicingPluginAkaBillingTab()
		{
			using (var form = (Phase5ArrivalMovementForm)GetFormToBashCore())
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
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			PerformExtraNctsHeaderConfiguration(header);
			Factory.Save();
			var form = (Phase5ArrivalMovementForm)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()), header);
			form.ControllerID = ControllerIDs.Customs.EU.NctsMovementController;
			return form;
		}

		protected virtual void PerformExtraNctsHeaderConfiguration(TParent header) { }
	}
}
