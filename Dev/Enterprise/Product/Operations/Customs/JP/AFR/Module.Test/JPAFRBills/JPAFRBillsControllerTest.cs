using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Customs.JP.AFR.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Module.Testing
{
	[TestedType(typeof(JPAFRBillsController))]
	class JPAFRBillsControllerTest : ZControllerBasherTest
	{
		public void TestSelectAndShowBill()
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			Factory.Save();
			var controller = new JPAFRBillsController();
			using (var form = (JPAFRForm)controller.ShowEditForm(bill2))
			{
				Application.DoEvents();
				var billsUserControl = GetControl<JPAFRBillsUserControl>(form, "jpafrBillsUserControl");
				var grid = GetControl<JPAFRBillsUserControl, ZGrid>(billsUserControl, "BillsGrid");
				AssertEquals("should have selected bill2", bill2.PK, ((JPAFRBills)grid.ListManager.GetCurrent()).PK);
			}
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "JPTKY";
			header.JPH_OverrideFreightDefaults = true;
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Factory.Save();
			controller = new JPAFRBillsController();
			using (var form = (ConsolForm)controller.ShowEditForm(bill2))
			{
				Application.DoEvents();
				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Customs.JP.AFRPluggedIntoConsol);
				var userControl = (JPAFRConsolManifestUserControl)plugIn.UserControl;
				var billsUserControl = GetControl<JPAFRConsolManifestUserControl, JPAFRBillsUserControl>(userControl, "BillsDetailsUserControl");
				form.Show();
				var grid = GetControl<JPAFRBillsUserControl, ZGrid>(billsUserControl, "BillsGrid");
				AssertEquals("should have selected bill2", bill2.PK, ((JPAFRBills)grid.ListManager.GetCurrent()).PK);
			}
		}

		T GetControl<T>(JPAFRForm form, string name)
			where T : Control
		{
			return GetControl<JPAFRForm, T>(form, name);
		}

		T GetControl<P, T>(P parent, string name)
			where P : Control
			where T : Control
		{
			return (T)typeof(P).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(parent);
		}

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.JP.AFRBill);
			AssertEquals(ModuleIDs.Customs.JP.AFRBill, controller.ModuleID);
		}

		public void TestReUseExistingAFRForms()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "OTT12432";
			Factory.Save();

			var afrController = ZControllerFactory.Create(ControllerIDs.Customs.JP.AFRPluggedIntoConsol);
			var afrBillController = ZControllerFactory.Create(ControllerIDs.Customs.JP.AFRBill);

			using (var form1 = afrController.ShowEditForm(header))
			using (var form2 = afrBillController.ShowEditForm(bill))
			{
				AssertSame("Should be re-using the same form", form1, form2);
			}
		}

		public void TestAFRFormShouldHaveNewButtonDisabled()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "OTT12432";
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.Customs.JP.AFRBill);
			using (var form = (ZForm)controller.ShowEditForm(bill))
			{
				AssertEquals("Form should have NEW button disabled", ODisplayMode.NewSaved, form.DisplayMode);
			}
		}

		public void TestShowNewFormDenied()
		{
			ShowFormDelegate method = delegate(JPAFRBills bill, ZController controller)
			{
				return (ZForm)controller.ShowFormForNewEntity(bill);
			};

			GenericShowFormDeniedTest(method);
		}

		public void TestReloadNoException()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "OTT2342";
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				ShowFormAndReload(bill);

				var consol = Factory.New<ForwardingConsol>();
				header.JPH_ParentId = consol.PK;
				header.JPH_ParentTableCode = consol.TablePrefix;
				Factory.Save();
				ShowFormAndReload(bill);
			});
		}

		public void TestShowTemplateCopyFormDenied()
		{
			ShowFormDelegate method = delegate(JPAFRBills bill, ZController controller)
			{
				return (ZForm)controller.ShowTemplateCopyForm(bill);
			};

			GenericShowFormDeniedTest(method);
		}

		public void TestShowDeleteFormDenied()
		{
			ShowFormDelegate method = delegate(JPAFRBills bill, ZController controller)
			{
				return (ZForm)controller.ShowDeleteForm(bill);
			};

			GenericShowFormDeniedTest(method);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			var bo = GetBusinessObjectThatIsInTheDatabase();

			AssertEquals(bo.GetType(), controller.TypeOfTopLevelBusinessObject);
		}

		void ShowFormAndReload(JPAFRBills bill)
		{
			using (var form = (ZForm)Controller.ShowEditForm(bill))
			{
				form.Show();
				form.ReloadForm();
				var formCreatedByReloading = new ZFormUtilitiesTest().GetFormCreatedByReloading(form);
				Application.DoEvents();
				AssertEquals("should be NewSaved mode after reload", ODisplayMode.NewSaved, formCreatedByReloading.DisplayMode);
				formCreatedByReloading.Close();
			}
			using (var form = (ZForm)Controller.ShowEditForm(bill))
			{
				form.Show();
				form.DisplayMode = ODisplayMode.Edit;
				form.ReloadForm();
				var formCreatedByReloading = new ZFormUtilitiesTest().GetFormCreatedByReloading(form);
				Application.DoEvents();
				AssertEquals("Edit and reload, should be NewSaved mode after reload", ODisplayMode.NewSaved, formCreatedByReloading.DisplayMode);
				formCreatedByReloading.Close();
			}
		}

		#region Implementation

		delegate ZForm ShowFormDelegate(JPAFRBills bill, ZController controller);

		void GenericShowFormDeniedTest(ShowFormDelegate method)
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "OTT2342";
			Factory.Save();

			try
			{
				ZForm form = method(bill, Controller);
				if (form != null)
				{
					form.Dispose();
				}

				Fail("Should have thrown a ModuleGuiNotSupportedException");
			}
			catch (ModuleGuiNotSupportedException)
			{
				Assert(true);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.JP.AFRBill;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "OTT2342";
			Factory.Save();

			return bill;
		}

		#endregion
	}
}
