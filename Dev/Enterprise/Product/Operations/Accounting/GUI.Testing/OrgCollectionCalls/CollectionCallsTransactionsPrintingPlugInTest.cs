using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(DummyForm))]
	public class CollectionCallsTransactionsPrintingPlugInTest : ZFormBasherTest
	{
		public class DummyForm : ZForm
		{
			public DummyForm(OrgHeader org)
				: base(org)
			{
				PlugIns.Add(ControllerIDs.OrgCollectionCalls);
				CaptionRenderingEnabled = true;
			}

			protected override ZTabControl TopLevelTabControl
			{
				get { return TabControl; }
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

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (components != null)
					{
						components.Dispose();
					}
				}
				base.Dispose(disposing);
			}

			#endregion

			#region Implementation

			ZTemplateTabControl TabControl;
			readonly System.ComponentModel.Container components;

			#endregion
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
