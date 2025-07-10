using System;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestsSubclassesOf(typeof(NctsMovementForm))]
	public abstract class NctsMovementFormAbstractTest<TParent> : ZFormBasherTest
		where TParent : NctsHeader
	{
		[CaptureMemoryDumpForDisposableLeak]
		public override void TestBashingForm()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = StackTraceEnabled;

			try
			{
				base.TestBashingForm();
			}
			catch (Exception e)
			{
				DisposableLeakListener.Instance.AdditionalInformation += $"<strong>Here we got an exception {e},</strong> message: {e.Message}, stack trace: {e.StackTrace}<br/>";
				throw;
			}
		}

		[RequiresSTA]
		public void TestSupportsInvoicingPluginAkaBillingTab()
		{
			using (var form = (NctsMovementForm)GetFormToBashCore())
			{
				form.Show();
				var jobInvoicingPlugin = form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				AssertNoExceptionThrown("Invoicing (billing) tab page", () => jobInvoicingPlugin.SelectTabPage());
			}
		}

		protected sealed override Form GetFormToBashCore()
		{
			var header = Factory.New<TParent>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			PerformExtraNctsHeaderConfiguration(header);
			Factory.Save();
			var form = (NctsMovementForm)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()), header);
			form.ControllerID = ControllerIDs.Customs.EU.NctsMovementController;
			form.Disposed += (sender, args) => DisposableLeakListener.Instance.AdditionalInformation += $"<strong>{sender} was truly disposed,</strong> see {System.Environment.StackTrace}<br/>";
			return form;
		}

		protected virtual void PerformExtraNctsHeaderConfiguration(TParent header) { }

		protected virtual bool StackTraceEnabled => true;
	}
}
