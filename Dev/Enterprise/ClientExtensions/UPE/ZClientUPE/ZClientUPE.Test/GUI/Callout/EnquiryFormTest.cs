using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal class EnquiryFormTest : TestCaseWithFactory
	{
		public void TestProcessQueuePlugInNotExist()
		{
			Enquiry enquiry = Factory.New<Enquiry>();
			using (EnquiryForm form = new EnquiryForm(enquiry))
			{
				AssertNull("Process Queue Plug-in should not exist", form.PlugIns.GetPlugIn(ControllerIDs.ProcessQueue));
			}
		}

		public void TestPartPaymentButtonReadonly()
		{
			Enquiry enquiry = Factory.New<Enquiry>();
			using (EnquiryForm form = new EnquiryForm(enquiry))
			{
				form.Show();
				AssertEquals(true, form.PartPaymentButton.ReadOnly);
			}
		}

		public void TestFormCaption()
		{
			Enquiry enquiry = Factory.New<Enquiry>();
			enquiry.CS_HAWB = "HOUSEBILL";
			using (EnquiryForm form = new EnquiryForm(enquiry))
			{
				AssertEquals("Enquiry - HOUSEBILL", form.FormCaption);
			}
		}

		public void TestForceShipmentIntoCalloutMenuItem()
		{
			Enquiry enquiry = Factory.New<Enquiry>();
			ZBool originalValue = GlbStaff.CurrentUser.GS_IsController;
			try
			{
				GlbStaff.CurrentUser.GS_IsController = false;
				using (EnquiryForm form = new EnquiryForm(enquiry))
				{
					AssertNull("not a controller, should not be added", GetForceCalloutMenuItem(form.Menu.MenuItems));
				}

				GlbStaff.CurrentUser.GS_IsController = true;
				using (EnquiryForm form = new EnquiryForm(enquiry))
				{
					enquiry.CurrentQueue.P4_QueueName = "";
					MenuItem item = GetForceCalloutMenuItem(form.Menu.MenuItems);
					item.PerformClick();
					Application.DoEvents();
					AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.Finance, enquiry.CurrentQueue.P4_QueueName);
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = originalValue;
			}
		}

		public void TestFormIsNotResizedToLessThanMinimumSizeWhenShown()
		{
			Enquiry enquiry = Factory.New<Enquiry>();
			using (EnquiryForm form = new EnquiryForm(enquiry))
			{
				form.Show();
				Application.DoEvents();
				Assert("Width has to be greater or equal to the minimum width", form.Width >= 998);
				Assert("Height has to be greater or equal to the minimum height", form.Height >= 724);
			}
		}

		MenuItem GetForceCalloutMenuItem(Menu.MenuItemCollection menuItems)
		{
			MenuItem result = null;
			foreach (MenuItem item in menuItems)
			{
				if (item.Text == "Force Shipment into Finance")
				{
					result = item;
				}
				else
				{
					result = GetForceCalloutMenuItem(item.MenuItems);
				}

				if (result != null)
				{
					break;
				}
			}

			return result;
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
