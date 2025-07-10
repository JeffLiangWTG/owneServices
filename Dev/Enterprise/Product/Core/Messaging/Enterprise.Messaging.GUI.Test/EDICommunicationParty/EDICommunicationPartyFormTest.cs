using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Messaging.GUI.Test
{
	[TestedType(typeof(EDICommunicationPartyForm))]
	public class EDICommunicationPartyFormTest : ZFormBasherTest
	{
		public EDICommunicationParty EDICommunicationParty
		{
			get
			{
				if (_ediCommunicationParty == null)
				{
					_ediCommunicationParty = Factory.New<EDICommunicationParty>();
				}
				return _ediCommunicationParty;
			}
		}
		public EDICommunicationParty _ediCommunicationParty;

		protected override Form GetFormToBashCore()
		{
			return new EDICommunicationPartyForm(EDICommunicationParty);
		}

		ZCheckBox activeClientCheckBox;
		ZTabControl tabCtl;
		ZUserControl inboundControl;
		ZUserControl outboundControl;
		ZLabel inboundWarningLabel;
		ZLabel outboundWarningLabel;
		ZCheckBox inboundCheckBox;
		ZCheckBox outboundCheckBox;
		ZTextBox outboundBox1;
		ZDropEdit outboundBox2;
		ZGroupBox outboundBox3;
		ZGuidFindBox inboundBox1;
		ZGuidFindBox inboundBox2;
		ZDropEdit inboundBox3;
		ZGroupBox inboundBox4; 

		public void TestEnableDisableClient()
		{
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();

			using (Form form = new EDICommunicationPartyForm(party))
			{
				form.Show();
				activeClientCheckBox = form.FindSingle<ZCheckBox>("activeClient");
				tabCtl = form.FindSingle<ZTabControl>("tabControl");

				inboundControl = form.FindSingle<ZUserControl>("InboundCommunicationPartyConfigUserControl");
				outboundControl = form.FindSingle<ZUserControl>("OutboundCommunicationPartyConfigUserControl");

				inboundWarningLabel = inboundControl.FindSingle<ZLabel>("inboundDisableWarning");
				outboundWarningLabel = outboundControl.FindSingle<ZLabel>("outboundDisableWarning");
				inboundCheckBox = inboundControl.FindSingle<ZCheckBox>("inboundActive");
				outboundCheckBox = outboundControl.FindSingle<ZCheckBox>("outboundActive");

				outboundBox1 = outboundControl.FindSingle<ZTextBox>("EndpointTextBox");
				outboundBox2 = outboundControl.FindSingle<ZDropEdit>("authorizationTypeDropEdit");
				outboundBox3 = outboundControl.FindSingle<ZGroupBox>("authorizationTypeGroupBox");

				inboundBox1 = inboundControl.FindSingle<ZGuidFindBox>("branchGuidFindBox");
				inboundBox2 = inboundControl.FindSingle<ZGuidFindBox>("departmentGuidFindBox");
				inboundBox3 = inboundControl.FindSingle<ZDropEdit>("authorizationTypeDropEdit");
				inboundBox4 = inboundControl.FindSingle<ZGroupBox>("authorizationTypeGroupBox");

				inboundCheckBox.Checked = true;
				outboundCheckBox.Checked = true;
				activeClientCheckBox.Checked = false;
				AssertInboundAndOutboundConfiguration(activeClientCheckBox.Checked);

				activeClientCheckBox.Checked = true;
				AssertInboundAndOutboundConfiguration(activeClientCheckBox.Checked);

				AssertInboundAndOutboundWarningLabelVisibility(true, false, false);
				AssertInboundAndOutboundWarningLabelVisibility(false, true, true);
				AssertInboundAndOutboundWarningLabelVisibility(true, true, false);
				AssertInboundAndOutboundWarningLabelVisibility(false, false, true);
			}
		}

		public void TestEnableBranchDisableDepartment()
		{
			var connectionType = "SUP";
			var accessTypes = new HashSet<AccessRequirement>() { AccessRequirement.SupportsInbound, AccessRequirement.RequiresBranch };

			var applicationDescriptor = new Mock<IEDIClientApplicationDescriptor>();
			applicationDescriptor.SetupGet(d => d.Code).Returns(connectionType);
			applicationDescriptor.SetupGet(d => d.Description).Returns("eAdaptorSupport");
			applicationDescriptor.SetupGet(d => d.AccessTypes).Returns(accessTypes);

			IEnumerable<IEDIClientApplicationDescriptor> applicationDescriptors = new List<IEDIClientApplicationDescriptor> { applicationDescriptor.Object };
			var mockDescriptors = new Mock<IEDIClientApplicationDescriptors>();
			mockDescriptors.Setup(d => d.GetValue(connectionType)).Returns(applicationDescriptor.Object);
			mockDescriptors.SetupGet(d => d.Values).Returns(applicationDescriptors);

			using (ObjectFactory.Substitute(mockDescriptors.Object))
			{
				var party = Factory.NewWithValidTestData<EDICommunicationParty>();
				party.ECP_ApplicationCode = connectionType;

				using (var form = new ConfigContainerForm(party))
				{
					form.Show();

					var findBoxBranch = (ZGuidFindBox)form.ConfigControl.Controls.Find("branchGuidFindBox", true).First();
					AssertEquals(true, findBoxBranch.Enabled);

					var findBoxDepartment = (ZGuidFindBox)form.ConfigControl.Controls.Find("departmentGuidFindBox", true).First();
					AssertEquals(false, findBoxDepartment.Enabled);
				}
			}
		}

		void AssertInboundAndOutboundConfiguration(bool isClientEnabled)
		{
			AssertEquals("Inbound configuration should be editable", isClientEnabled, inboundBox1.Enabled);
			AssertEquals("Inbound configuration should be editable", isClientEnabled, inboundBox2.Enabled);
			AssertEquals("Inbound configuration should be editable", isClientEnabled, inboundBox3.Enabled);
			AssertEquals("Inbound configuration should be editable", isClientEnabled, inboundBox4.Enabled);
			AssertEquals("Outbound configuration should be editable", isClientEnabled, outboundBox1.Enabled);
			AssertEquals("Outbound configuration should be editable", isClientEnabled, outboundBox2.Enabled);
			AssertEquals("Outbound configuration should be editable", isClientEnabled, outboundBox3.Enabled);
			Assert("Inbound active checkbox should be checked", inboundCheckBox.Checked);
			Assert("Inbound active checkbox should be checked", inboundCheckBox.Checked);
			Assert("Inbound active checkbox should be checked", inboundCheckBox.Checked);
			Assert("Inbound active checkbox should be checked", inboundCheckBox.Checked);
			Assert("Outbound active checkbox should be checked", outboundCheckBox.Checked);
			Assert("Outbound active checkbox should be checked", outboundCheckBox.Checked);
			Assert("Outbound active checkbox should be checked", outboundCheckBox.Checked);
			AssertInboundAndOutboundWarningLabelVisibility(isClientEnabled, true, true);
		}

		void AssertInboundAndOutboundWarningLabelVisibility(bool isClientEnabled, bool isInboundActive, bool isOutboundActive)
		{
			inboundCheckBox.Checked = isInboundActive;
			outboundCheckBox.Checked = isOutboundActive;
			activeClientCheckBox.Checked = isClientEnabled;
			tabCtl.SelectTab(0);
			AssertEquals("Inbound Disable Warning has correct visibility", isInboundActive, !inboundWarningLabel.Visible);
			tabCtl.SelectTab(1);
			AssertEquals("Outbound Disable Warning has correct visibility", isOutboundActive, !outboundWarningLabel.Visible);
		}

		class ConfigContainerForm : ZForm
		{
			public ConfigContainerForm(EDICommunicationParty party)
				: base(party)
			{
				ConfigControl = new EDICommunicationPartyUserControl();
				Controls.Add(ConfigControl);
				BindingSource.SetBindingMember(ConfigControl, ".");
			}

			public EDICommunicationPartyUserControl ConfigControl { get; }
		}
	}
}
