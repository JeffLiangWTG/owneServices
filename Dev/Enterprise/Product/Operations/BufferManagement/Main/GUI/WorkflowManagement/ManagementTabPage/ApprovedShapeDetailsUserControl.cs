using System;
using CargoWise.Application;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration.Network;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ApprovedShapeDetailsUserControl : ZUserControl
	{
		public ApprovedShapeDetailsUserControl()
		{
			InitializeComponent();
		}

		ProcessHeader SelectedWorkflow
		{
			get
			{
				var workflow = BindingSource.Current as ProcessHeader;

				return workflow == null || workflow.IsDeleted ? null : workflow;
			}
		}

		void OpenDiagramButton_Click(object sender, EventArgs e)
		{
			var selectedWorkflow = SelectedWorkflow;
			if (selectedWorkflow != null && selectedWorkflow.ApprovedShape != null)
			{
				ObjectFactory.Get<INetworkDiagramFormFactory>().ShowNetworkFormForDiagramContainingShape(selectedWorkflow.ApprovedShape);
			}
		}
	}
}
