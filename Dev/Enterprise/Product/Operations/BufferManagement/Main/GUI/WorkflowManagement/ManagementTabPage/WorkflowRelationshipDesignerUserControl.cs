using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkflowRelationshipDesignerUserControl : ZUserControl, INetworkRefresher
	{
		readonly MultiActionButtonDialogWrapper<DeleteWorkflowOption> deleteWorkflowDialogWrapper;

		public WorkflowRelationshipDesignerUserControl(MultiActionButtonDialogWrapper<DeleteWorkflowOption> deleteWorkflowDialogWrapper)
		{
			this.deleteWorkflowDialogWrapper = deleteWorkflowDialogWrapper;
			InitializeComponent();
		}

		public void RefreshNetwork()
		{
			Network.Refresh(RefreshType.RedrawDiagram);
		}

		public new WorkflowManagementViewModel DataSource
		{
			get { return (WorkflowManagementViewModel)base.DataSource; }
		}

#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public NetworkUserControl NetworkUserControl { get; private set; }
#pragma warning restore CS0618 // Restore the warning for obsolete usage

#if DEBUG
		public
#endif
 JobNetwork Network
		{
			get
			{
				if (network == null)
				{
					InitialiseNetwork();
				}

				return network;
			}
		}

#if DEBUG
		public JobNetwork NetworkForTest => network;
#endif

		JobNetwork network;
		void InitialiseNetwork()
		{
			UnhookNetworkEvents();

			var viewModel = DataSource;

			if (viewModel != null)
			{
				var defaultShape = viewModel.JobHeader.GetDefaultDiagram();
				var controller = ObjectFactory.Get<IBMNetworkEntityController>(nameof(IBMNetworkEntityController), this, FindForm());
				network = JobNetwork.Create(defaultShape, controller, this);

				network.DeleteWorkflowDialogWrapper = deleteWorkflowDialogWrapper;
				UpdateDefaultNetwork(network);

				network.DiagramShape.DeletedByDataRefresh += DefaultShape_DeletedByDataRefresh;
				viewModel.AllProcessHeaders.CollectionCountChange -= AllProcessHeaders_CollectionCountChange;
				viewModel.AllProcessHeaders.CollectionCountChange += AllProcessHeaders_CollectionCountChange;
			}
		}

		void UnhookNetworkEvents()
		{
			if (network != null)
			{
				network.DiagramShape.DeletedByDataRefresh -= DefaultShape_DeletedByDataRefresh;
			}
		}

		void DefaultShape_DeletedByDataRefresh(object sender, EventArgs e)
		{
			InitialiseNetwork();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
			{
				var viewModel = DataSource;

				if (Controls.Count == 0 && viewModel != null)
				{
					if (IsJobHeaderValid() && IsNetworkValid())
					{
						HostNetworkUserControl();
					}
					else
					{
						ShowCoverLabel();
					}
				}
				else
				{
					if (IsJobHeaderValid())
					{
						Network.FullRefresh();
					}
				}
			}
		}

		bool IsNetworkValid()
		{
			var graph = new NetworkDependencyGraph(Network);

			return graph.IsDirectedAcyclicGraph();
		}

		bool IsJobHeaderValid()
		{
			return !(DataSource?.JobHeader?.IsDeleted) ?? false;
		}

		void HostNetworkUserControl()
		{
			if (container == null)
			{
				container = new ZElementHost
				{
					BackColor = Color.Transparent,
					Dock = DockStyle.Fill,
				};

				UnsubscribeFromNetworkUserControlEvents();
				NetworkUserControl = WinFormsHost.HostNetworkUserControlInWinForms(container, Network?.DiagramEntity, this);
				SubscribeToNetworkUserControlEvents();
				Controls.Add(container);
			}
			else
			{
				container.Visible = true;
			}
		}

		void ShowCoverLabel()
		{
			if (coverLabel == null)
			{
				coverLabel = new ZLabel
				{
					Cursor = Cursors.Hand,
					Dock = DockStyle.Fill,
					IsFontBold = true,
					Text = Res.GetString("a12af21a-f0ab-4a07-9ada-3fb5d654eb82", "The Workflow Relationship Designer cannot be shown since the network has an invalid relationship. Please correct this relationship and click this message to show the network."),
					TextAlign = ContentAlignment.MiddleCenter,
				};

				coverLabel.Click += CoverLabel_Click;
				Controls.Add(coverLabel);
			}
			else
			{
				coverLabel.Visible = true;
			}

			if (container != null)
			{
				container.Visible = false;
			}
		}

		ZLabel coverLabel;
		ZElementHost container;

#if DEBUG
		public
#endif
		void CoverLabel_Click(object sender, EventArgs e)
		{
			if (IsJobHeaderValid() && IsNetworkValid())
			{
				coverLabel.Visible = false;

				HostNetworkUserControl();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("f18a36e3-965d-4aee-8fd1-d036c1c3059a", "This network is still invalid and cannot be shown."));
			}
		}

		void SubscribeToNetworkUserControlEvents()
		{
			if (NetworkUserControl != null)
			{
				NetworkUserControl.SelectionChanged += NetworkUserControl_SelectionChanged;
			}
		}

		void UnsubscribeFromNetworkUserControlEvents()
		{
			if (NetworkUserControl != null)
			{
				NetworkUserControl.SelectionChanged -= NetworkUserControl_SelectionChanged;
			}
		}

		void NetworkUserControl_SelectionChanged(object sender, RefreshArgs e)
		{
			ProcessHeader selectedProcessHeader = null;
			if (e.Entities.FirstOrDefault() is ShapeNetworkEntity shapeNetworkEntity && shapeNetworkEntity.ProcessHeader != null)
			{
				selectedProcessHeader = shapeNetworkEntity.ProcessHeader;
			}
			SelectedWorkflowChanged?.Invoke(this, selectedProcessHeader);
		}

		internal event EventHandler<ProcessHeader> SelectedWorkflowChanged;

		void AllProcessHeaders_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded && e.BizObject is ProcessHeader)
			{
				UpdateDefaultNetwork(Network);
			}
		}

		static void UpdateDefaultNetwork(JobNetwork network)
		{
			if (network.DiagramShape.IsDefaultDiagram)
			{
				network.FullRefresh();
			}
		}

		#region INetworkRefresher

		public INetwork GetReloadedNetwork()
		{
			if (!Network.DiagramEntity.IsDeleted)
			{
				return Network;
			}
			else
			{
				if (DataSource == null)
				{
					return new DummyNetwork { DiagramEntity = new Entity() }; // The form is being closed but a refresh was added to the message queue before this happened.
				}

				var diagramShape = DataSource.Factory.New<BMNCNShapeDefaultDiagram>();
				var controller = ObjectFactory.Get<IBMNetworkEntityController>(nameof(IBMNetworkEntityController), this, FindForm());

				return JobNetwork.Create(diagramShape, controller, this);
			}
		}

		public string RefreshToken { get; private set; }

		public void Refresh(RefreshArgs args)
		{
			RefreshToken = Guid.NewGuid().ToString();

			if (IsNetworkValid())
			{
				Refreshed?.Invoke(this, args);
			}
			else
			{
				ShowCoverLabel();
			}
		}

		public event EventHandler<RefreshArgs> Refreshed;

		#endregion

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var parentZForm = ParentForm as ZForm;
			if (parentZForm != null)
			{
				parentZForm.Saved += ParentForm_Saved;
			}
		}

		void ParentForm_Saved(object sender, EventArgs e)
		{
			network?.Refresh(RefreshType.RedrawDiagram);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (components != null)
					{
						components.Dispose();
					}

					UnhookNetworkEvents();

					var parentZForm = ParentForm as ZForm;
					if (parentZForm != null)
					{
						parentZForm.Saved -= ParentForm_Saved;
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
	}
}
