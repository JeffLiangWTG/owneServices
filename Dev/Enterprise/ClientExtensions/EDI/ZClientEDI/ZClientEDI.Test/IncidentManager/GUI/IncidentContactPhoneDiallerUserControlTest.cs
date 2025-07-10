using System.Drawing;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(IncidentContactPhoneDiallerUserControlFormForTest))]
	class IncidentContactPhoneDiallerUserControlTest : ZFormBasherTest
	{
		public void TestDefaultDialInfo()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Demo_Company";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Phone = "111111111";
			org.MainAddress.OA_Address1 = "100 Fake St";
			var branchAddress1 = org.Addresses.AddNew();
			branchAddress1.OA_Address1 = "123 Fake St";
			branchAddress1.OA_Phone = "333333333";
			var branchAddress2 = org.Addresses.AddNew();
			branchAddress2.OA_Address1 = "222 Fake St";
			branchAddress2.OA_Phone = "444444444";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Smith";
			contact.OC_Phone = "222222222";
			contact.OC_OA_OrgAddress = branchAddress2.PK;
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OC_Contact = contact.PK;
			incident.IM_OA_BranchAddress = branchAddress1.PK;
			Factory.Save();
			using (var form = new IncidentContactPhoneDiallerUserControlFormForTest(incident))
			{
				form.Show();
				var actualDialInfo = form.IncidentContactPhoneDiallerUserControl.GetDefaultDialInfo_Exposed();
				AssertEquals("222222222", actualDialInfo.Number);
				AssertEquals("Work", actualDialInfo.Description);
				contact.OC_Phone = "";
				actualDialInfo = form.IncidentContactPhoneDiallerUserControl.GetDefaultDialInfo_Exposed();
				AssertEquals("444444444", actualDialInfo.Number);
				AssertEquals("Office", actualDialInfo.Description);
				branchAddress2.OA_Phone = "";
				actualDialInfo = form.IncidentContactPhoneDiallerUserControl.GetDefaultDialInfo_Exposed();
				AssertEquals("333333333", actualDialInfo.Number);
				AssertEquals("Office", actualDialInfo.Description);
				branchAddress1.OA_Phone = "";
				actualDialInfo = form.IncidentContactPhoneDiallerUserControl.GetDefaultDialInfo_Exposed();
				AssertEquals("111111111", actualDialInfo.Number);
				AssertEquals("Office", actualDialInfo.Description);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();
			return new IncidentContactPhoneDiallerUserControlFormForTest(incident);
		}

		#region Classes
		public class IncidentContactPhoneDiallerUserControlForTest : IncidentContactPhoneDiallerUserControl
		{
			public PhoneDialInfo GetDefaultDialInfo_Exposed()
			{
				return base.GetDefaultDialInfo();
			}
		}

		public class IncidentContactPhoneDiallerUserControlFormForTest : ZForm
		{
			public IncidentContactPhoneDiallerUserControlFormForTest(IncidentMainBase incident) : base(incident)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Size = new Size(1024, 768);
				IncidentContactPhoneDiallerUserControl = new IncidentContactPhoneDiallerUserControlForTest();
				Controls.Add(IncidentContactPhoneDiallerUserControl);
				BindingSource.SetBindingMember(IncidentContactPhoneDiallerUserControl, "IM_OC_Contact");
				CaptionRenderingEnabled = true;
			}

			public IncidentContactPhoneDiallerUserControlForTest IncidentContactPhoneDiallerUserControl;
		}
		#endregion
		#endregion
	}
}
