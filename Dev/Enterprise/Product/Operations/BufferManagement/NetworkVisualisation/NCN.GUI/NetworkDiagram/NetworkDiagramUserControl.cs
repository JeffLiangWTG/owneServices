using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public partial class NetworkDiagramUserControl : ZUserControl
	{
		public NetworkDiagramUserControl()
		{
			InitializeComponent();
		}

		public new BMNCNShape DataSource
		{
			get { return (BMNCNShape)base.DataSource; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var diagramShape = (BMNCNShape)dataSource;

			if (diagramShape != null)
			{
#if !WINZOR
				var selector = XAMLHelper.CreateAndPopulateAlternativeSelector();
#else
				object selector = null;
#endif

				/*
				 * Generally getting a form from a control would be bad. 
				 * Here however, it is necessary so WPF/Async hooks can be added into Save, which is implemented on the ZForm.
				 */
				var form = FindForm() as ZForm;
				var refresher = new JobNetworkRefresher();
				var controller = new BMNetworkEntityController(refresher, form);
				Network = JobNetwork.Create(diagramShape, controller, refresher);
				refresher.AddedToNetwork(Network);

				var container = new ZElementHost
				{
					Name = "WPFElementHost",
					BackColor = Color.Transparent,
					Dock = DockStyle.Fill,
					TabStop = false, // to prevent possile NullReferenceException when setting focus on ZElementHost before NetworkUserControl is initialised in WinFormsHost.HostNetworkUserControlInWinForms (my guess, see WI00628088)
				};

				var ribbonDataProvider = RibbonIsEnabled ? new BMRibbonViewModelProvider() : null;
				NetworkUserControl = WinFormsHost.HostNetworkUserControlInWinForms(container, Network.DiagramEntity, refresher, selector, new JobNetworkNodeViewModelProvider(), ribbonDataProvider);
				Controls.Add(container);
			}
			else
			{
				Network = null;
			}
		}

#pragma warning disable CS0618
		public NetworkUserControl NetworkUserControl { get; private set; }
#pragma warning restore CS0618

		public NetworkViewModel NetworkViewModel => NetworkUserControl?.ViewModel.NetworkViewModel;

		public JobNetwork Network { get; private set; }

		public void SwitchFocusToFinder()
		{
			NetworkUserControl.SwitсhFocusToFinder();
		}

		protected virtual bool RibbonIsEnabled => true;

		#region Dispose
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect", Scope = "member", Target = "Enterprise.BufferManagement.NetworkVisualisation.GUI.NetworkDiagramUserControl.#Dispose(bool)")]  // Really need GC.Collect here
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1056:DoNotUseGCCollect", Justification = "Baseline")]
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (RibbonIsEnabled)
				{
					NetworkUserControl?.Dispose();
				}
				components?.Dispose();
			}
			base.Dispose(disposing);

			#region DEBUG
#if DEBUG
			if (Globals.IsTest)
			{
				GC.Collect();
			}
#endif
			#endregion
		}

		#endregion
	}
}
