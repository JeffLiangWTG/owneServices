using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public partial class NetworkDiagramForm : ZTemplateForm
	{
		public NetworkDiagramForm()
		{
			InitializeComponent();
		}

		public NetworkDiagramForm(BMNCNShape diagramShape)
			: base(diagramShape)
		{
			InitializeComponent();

			var searchEventHandler = (diagramShape as INetworkEntity).IsDiagramWithRibbon ? new EventHandler(SwitchFocusToFinder) : new EventHandler(OpenFinderForm);
			((IFileMenuItemsProvider)this).ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("d7a349d8-ee1b-47f4-bcba-18df89e60018", "Search"), searchEventHandler, Shortcut.CtrlF));

			Saved += NetworkDiagramForm_Saved;
			FormClosed += NetworkDiagramForm_Close;
			Shown += NetworkDiagramForm_Shown;
		}

		void OpenFinderForm(object sender, EventArgs e)
		{
			OpenFinderForm();
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if ((keyData & (Keys.Alt | Keys.Control)) == Keys.None)
			{
				Keys keyCode = keyData & Keys.KeyCode;

				if (keyCode == Keys.Escape)
				{
					return true;
				}
			}
			return base.ProcessDialogKey(keyData);
		}

#if DEBUG
		public
#endif
		void OpenFinderForm()
		{
			NetworkDiagramControl?.NetworkUserControl?.OpenFinderForm();
		}

		void SwitchFocusToFinder(object sender, EventArgs e)
		{
			SwitchFocusToFinder();
		}

		void SwitchFocusToFinder()
		{
			NetworkDiagramControl.SwitchFocusToFinder();
		}

		void NetworkDiagramForm_Shown(object sender, EventArgs e)
		{
			ValidateLoopsOnOpen();
		}

		void ValidateLoopsOnOpen()
		{
			if (Network.DiagramShape.ShouldValidateLoopsOnOpen && !Network.ValidateLoopsAndCheckThereAreNoErrors())
			{
				ShowErrorsDialog();
			}
		}

		void NetworkDiagramForm_Saved(object sender, EventArgs e)
		{
			if (Network != null)
			{
				Network.Refresh(RefreshType.Saved);
			}
		}

		void NetworkDiagramForm_Close(object sender, EventArgs e)
		{
			if (Network != null)
			{
				Network.Refresh(RefreshType.Close);
				CloseFinderForm();
			}
		}

#if DEBUG
		public
#endif
		void CloseFinderForm()
		{
			NetworkDiagramControl?.NetworkUserControl?.CloseFinderForm();
		}

		public new BMNCNShape DataSource => (BMNCNShape)base.DataSource;

		public NetworkViewModel NetworkViewModel => NetworkDiagramControl?.NetworkViewModel;

		public JobNetwork Network => NetworkDiagramControl?.Network;

		#region ZTemplateForm Overrides

		protected override bool ShowAuditTab => true;

		#endregion

		#region ZForm Overrides

		protected override IBusiness BusinessEntityForValidation
		{
			get { return Network?.DiagramEntity ?? (IBusiness)DataSource; }
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			using (Network?.DelayPropertyChanged())
			{
				return base.ValidateAndSave();
			}
		}

		public override string FormCaption
		{
			get
			{
				if (DataSource != null && !DataSource.Name.IsNullOrEmpty())
				{
					return DataSource.IsInDatabase
						? DataSource.BNS_Name.ToString()
						: Res.GetString("116A0460-09AC-4DBC-ABE0-3D055E059B06", "Network Diagram");
				}

				return Res.GetString("21770B82-C324-498A-9AE4-AECBD77F810C", "Untitled Diagram");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);

			if (!Globals.IsTest || QueueGC)
			{
				UserIdleWorker.QueueWorkItem(null, new MethodInvoker(CollectAfterDiagram));
			}
		}

		protected override DialogResult ShowErrorsDialogCore(bool includeIgnoreOption)
		{
			if (Network.DiagramContainsSchedulingConflicts)
			{
				// We are already showing the user a message during the pre-validation process, so no need to show them another dialog. We use a validation error to abort the save process.
				return DialogResult.None;
			}
			else
			{
				return base.ShowErrorsDialogCore(includeIgnoreOption);
			}
		}

		#endregion

		[SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
		[SuppressMessage("CargoWiseOne", "CW1056:DoNotUseGCCollect", Justification = "We dont want the silly large byte arrays left over in memory from hosting wpf in winforms")]
		static void CollectAfterDiagram()
		{
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

			// NOTE: this is un-tested, but is very important because otherwise the ElementHost control may leak byte arrays, or at least keep
			//		 them in memory for too long.
			GC.Collect(); // We dont want the silly large byte arrays left over in memory from hosting wpf in winforms
		}

		public void DiagramsGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var currentView = DiagramsGrid.ListManager.GetCurrent() as RelatedDiagramView;
			if (currentView != null)
			{
				NetworkDiagramFormFactory.ShowNetworkFormForDiagramContainingShape(currentView.Shape);
			}
		}

#if DEBUG
		public
#endif
 bool QueueGC;
	}
}
