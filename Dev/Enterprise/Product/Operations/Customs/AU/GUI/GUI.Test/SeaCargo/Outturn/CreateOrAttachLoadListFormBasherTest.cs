using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	[TestedType(typeof(CreateOrAttachLoadListForm))]
	sealed class CreateOrAttachLoadListFormBasherTest : ZFormBasherTest
	{
		public void TestGridId()
		{
			using (var form = (CreateOrAttachLoadListForm)GetFormToBashCore())
			{
				var grid1 = (ZGrid)form.Controls.Find("zGrid1", true).First();
				var grid2 = (ZGrid)form.Controls.Find("zGrid2", true).First();
				AssertEquals("GridLayout1gjPwJa8I1VxBav3CF7EiQ==", grid1.GridId);
				AssertEquals("GridLayoutZi+B58dWA+Vzji5gJ3Kw/w==", grid2.GridId);
			}
		}

		public void TestUpdateButtons()
		{
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			orgProxy.OH_IsUnpackDepot = true;
			orgProxy.OH_IsPackDepot = true;
			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_OH_Forwarder = CreateForwarder();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "1234";
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = CreateConsignee();
			shipment.JS_GoodsDescription = "DESC";
			Factory.Save();
			using (var form = new CreateOrAttachLoadListForm(consol))
			{
				form.Show();
				AssertEquals("Back Button Disabled", false, form.backButton.Enabled);
				form.mainTabControl.SelectedIndex = form.mainTabControl.TabCount - 1;
				AssertEquals("Back Button Enabled", true, form.backButton.Enabled);
				AssertEquals("Next Button Text", "Finish", form.nextButton.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.NextButton_Click(null, EventArgs.Empty);
				Assert("Button Click Resulted in error message", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Back Button Enabled", true, form.backButton.Enabled);
				form.BackButton_Click(null, EventArgs.Empty);
				AssertEquals("Next Button Text", "&Next >", form.nextButton.Text);
				var transport = consol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_JX = CreateImportSailing();
				consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
				consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;
				consol.Containers[0].JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
				form.NextButton_Click(null, EventArgs.Empty);
				AssertEquals("Next Button Text", "Finish", form.nextButton.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.NextButton_Click(null, EventArgs.Empty);
				AssertNoErrors("Pre:Condition No Errors on Consol", consol);
				Assert("Button Click Resulted in error message", !UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Dialog Result of Form", DialogResult.OK, form.DialogResult);
			}

			ZGuid CreateImportSailing()
			{
				var carrier = Factory.New<OrgHeader>();
				carrier.OH_IsShippingLine = true;
				carrier.OH_IsShippingProvider = true;
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
				voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("SOUTHERN CROSS MARU", Factory).First().RV_FK;
				voyage.JV_VoyageFlight = "123";
				voyage.JV_OH_Line = carrier.PK;
				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "NZAKL";
				var dest = voyage.Destinations.AddNew();
				dest.JB_RL_NKPortOfDischarge = "AUSYD";
				dest.JB_E_ARV = ZDateTime.Now;
				voyage.GenerateSailings();
				return voyage.Sailings[0].PK;
			}

			ZGuid CreateConsignee()
			{
				var result = Factory.New<OrgHeader>();
				result.OH_IsConsignee = true;
				result.OH_FullName = "Consignee";
				result.MainAddress.OA_Address1 = "Consignee Address 1";
				result.OH_RL_NKClosestPort = "AUSYD";
				return result.PK;
			}

			ZGuid CreateForwarder()
			{
				var result = Factory.New<OrgHeader>();
				result.OH_IsForwarder = true;
				result.OH_IsDebtor = true;
				result.OH_FullName = "Forwader";
				result.MainAddress.OA_Address1 = "Forwader Address 1";
				result.OH_RL_NKClosestPort = "AUSYD";
				return result.PK;
			}
		}

		protected override Form GetFormToBashCore() => new CreateOrAttachLoadListForm(Factory.New<CFSLoadListConsol>());

		protected override bool AllowHasChangesOnFormOpen => true;
	}
}
