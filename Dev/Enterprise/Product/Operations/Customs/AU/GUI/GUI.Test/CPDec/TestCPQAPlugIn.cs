using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(DummyForm))]
	sealed class TestCPQAPlugIn : ZFormBasherTest
	{
		public void TestOrgCPQA()
		{
			using (CPQAPlugIn testPlugIn = new CPQAPlugIn(Org))
			{
				ICPQAAttachee orgCPQAAsAttachee = testPlugIn.OrgCPQA;
				AssertEquals("Collection should be loaded properly", Org, orgCPQAAsAttachee.Questions.Master);
			}
		}

		public void TestPluginIsDockedToFill()
		{
			using (CPQAPlugIn testPlugIn = new CPQAPlugIn(Org))
			{
				AssertEquals("Control is Docked to Fill", DockStyle.Fill, testPlugIn.UserControl.Dock);
			}
		}

		protected override Form GetFormToBashCore() => new DummyForm(Org);

		OrgHeader org;
		OrgHeader Org => org ?? (org = Factory.LoadTop1<OrgHeader>(new ZQuery()));

		sealed class DummyForm : ZForm
		{
			public DummyForm(OrgHeader org) : base(org)
			{
				PlugIns.Add(ControllerIDs.Customs.AU.CMRLodgementQuestion);
			}

			protected override ZTabControl TopLevelTabControl => tabControl;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				tabControl = new ZTemplateTabControl();
				Controls.Add(tabControl);
			}

			ZTemplateTabControl tabControl;
		}
	}
}
