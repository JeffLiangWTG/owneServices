using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class NudgeControls : ZUserControl
	{
		public NudgeControls(ITaskCardComponentParent parent)
		{
			InitializeComponent();
			this.parent = parent;
			this.jobCardsShown = parent.Task != null && parent.CardContent.Identifier == parent.Task.JobHeader?.PK;

			var workflow = parent.Task != null ? parent.Task.ProcessHeader : null;
			var component = workflow != null ? workflow.CurrentComponent : null;
			Visible = component != null && !component.IsBuffer;

			if (jobCardsShown)
			{
				this.BindingSource.SetBindingMember(this.NudgeAmount, "JobHeader.FH_VoteUpDownAmount");
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.ProcessTask)(null)).JobHeader.FH_VoteUpDownAmount);
			}
			else
			{
				this.BindingSource.SetBindingMember(this.NudgeAmount, "ProcessHeader.FH_VoteUpDownAmount");
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.ProcessTask)(null)).ProcessHeader.FH_VoteUpDownAmount);
			}
		}

		readonly bool jobCardsShown;
		readonly ITaskCardComponentParent parent;

		void LabelVoteUp_MouseDown(object sender, MouseEventArgs e)
		{
			parent.VoteUp(jobCardsShown);
		}

		void LabelVoteDown_MouseDown(object sender, MouseEventArgs e)
		{
			parent.VoteDown(jobCardsShown);
		}
	}
}
