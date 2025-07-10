#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	using System;
	using System.Windows.Forms;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Windows.UI.Testing;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.GUI.Testing;
	using Enterprise.ZArchitecture.Modules;

	[SuppressFormDesignerAnalysis]
	[ContainerControlBaseClass]
	[TestExcludeZWinFormsAllHaveFormBashers]
	[TestExcludeZWinFormHasTypedConstructor]
	public partial class ZEditForm : ZForm, IPreviousNextControlOverrideProvider
	{
		public ZEditForm()
		{
			Initialise();
		}

		/// <summary>
		/// Pass in a BusinessObject / BusinessObjectCollection
		/// </summary>
		/// <param name="businessEntity"></param>
		protected ZEditForm(IBusiness businessEntity)
			: base(businessEntity)
		{
			Initialise();
		}

		void Initialise()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				ZFormPostingButtonsStrategy.SetupPosting(this, SaveButtonUserControl);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

#if DEBUG
			if (!this.IsDesignMode() && AutoAddPreviousNextButtons && PreviousNextControlForTesting == null && ModuleResultsBusinessObject != null)
			{
				ErrorReporter.ReportOnce("Your form has AutoAddPreviousNextButtons = true, but the buttons are not present. Most likely, this is because you need to override ModuleID { get; } in your ZController.");
			}
#endif
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			if (SaveButtonUserControl != null && this.IsDesignMode() && SaveButtonUserControl.ForceRepositioning && ClientSize.Width > SaveButtonUserControl.Width)
			{
				SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(ClientSize.Width - SaveButtonUserControl.Width, SaveButtonUserControl.Location.Y, false);
			}
		}

		protected override void InitialiseForm()
		{
			InitializeFormCore();
			base.InitialiseForm();
		}

		protected virtual void InitializeFormCore()
		{
			InitializeComponent();
		}

		#region IPreviousNextControlOverrideProvider Members

		bool IPreviousNextControlOverrideProvider.OverridesSetPreviousNextControlParentAndPosition
		{
			get { return true; }
		}

		bool IPreviousNextControlOverrideProvider.ShouldDoBaseSetPreviousNextControlParentAndPosition
		{
			get { return false; }
		}

		bool IPreviousNextControlOverrideProvider.OverridesGetPreviousNextControl
		{
			get { return false; }
		}

		void IPreviousNextControlOverrideProvider.SetPreviousNextControlParentAndPosition(ZPreviousNextControl control)
		{
			control.Parent = PreviousNextControlForDesigner.Parent;
			control.Location = PreviousNextControlForDesigner.Location;
			Controls.Remove(PreviousNextControlForDesigner);
			PreviousNextControlForDesigner.Dispose();
		}

		ZPreviousNextControl IPreviousNextControlOverrideProvider.GetPreviousNextControl(ModuleResultsBusinessObject bizObj, ZController controller)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
