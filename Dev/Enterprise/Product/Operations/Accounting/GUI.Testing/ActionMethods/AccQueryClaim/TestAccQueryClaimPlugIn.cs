using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.GUI.Testing
{
	public abstract class TestAccQueryClaimPlugIn : ZFormBasherTest
	{
		public class DummyForm : ZForm
		{
			public DummyForm(OrgHeader org) : base(org)
			{
				CaptionRenderingEnabled = true;
				this.Org = org;
				PlugIns.Add(ControllerIDs.ARAccQueryClaim);
			}

			protected override ZTabControl TopLevelTabControl
			{
				get	{ return TabControl; }
			}

			protected override void InitialiseForm()
			{
				base.InitialiseForm();
				InitializeComponent();
			}

			new void InitializeComponent()
			{
				base.InitializeComponent();
				this.TabControl = new ZTemplateTabControl();
				this.Controls.Add(TabControl);
			}

			#region Dispose

			protected override void Dispose(bool disposing )
			{
				if (disposing )
				{
					if (components != null)
					{
						components.Dispose();
					}
				}
				base.Dispose(disposing );
			}

			#endregion

			#region Implementation

			ZTemplateTabControl TabControl;
			protected OrgHeader Org;
			readonly System.ComponentModel.Container components;

			#endregion
		}

		public override Type FormToBashType
		{
			get { return typeof(DummyForm); }
		}

		protected override Form GetFormToBashCore()
		{
			DummyForm testForm = new DummyForm(Org);
			testForm.Height = 1200;
			testForm.Width = 1200;
			return testForm;
		}

		OrgHeader fOrg;
		protected OrgHeader Org
		{
			get
			{
				if (fOrg == null)
				{
					fOrg = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery());
				}
				return fOrg;
			}
		}
	}
}
