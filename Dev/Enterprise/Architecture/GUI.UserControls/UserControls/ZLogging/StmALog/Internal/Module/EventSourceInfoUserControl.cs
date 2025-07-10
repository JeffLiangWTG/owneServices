using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public partial class EventSourceInfoUserControl : ZUserControl
	{
		public EventSourceInfoUserControl()
		{
			InitializeComponent();
			Visible = false;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var collection = dataSource as IBusinessObjectCollection;
			if (collection == null || collection.TypeOfElements == BindingSource.DataSourceType)
			{
				base.SetDataBinding(collection, dataMember);
			}
		}

		protected override void OnCurrentDataItemChanged(System.EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var currentLog = this.BindingSource.Current as StmALog;
			var parentSplitContainer = Parent?.Parent as KSplitContainer;
			var currentLogIsNotNull = currentLog != null;
			var setVisible = (currentLogIsNotNull && currentLog.SourceInfoItems.Count != 0);
			var otherSetVisible = currentLogIsNotNull && currentLog.RelatedEDIMessage != null && currentLog.RelatedEDIMessage.Message != null;
			if (parentSplitContainer != null)
			{
				parentSplitContainer.Panel2Collapsed = !(setVisible || otherSetVisible);
			}
			Visible = setVisible || otherSetVisible;
			ediMessageInfoGroupBox.Visible = otherSetVisible;
		}

#if DEBUG
		internal
#endif
		void ShowMessageButton_Click(object sender, System.EventArgs e)
		{
			var currentLog = this.BindingSource.Current as StmALog;
			if (currentLog != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.Messaging.EDIMessage);
				if (currentLog.RelatedEDIMessage != null)
				{
					var form = controller.ShowViewForm((BusinessObject)currentLog.RelatedEDIMessage.Message);
					if (form != null) // form is null when current user doesn't have permissions to view EDIMessage
					{
						form.Show();
						return;
					}
				}
				else
				{
					Globals.Message.Show(Enterprise.ZArchitecture.GUI.UserControls.Res.GetString("90e06bb7-55e9-44c7-99eb-2d82a2cdc956", @"No Related EDI Messages available for the form."));
					return;
				}
			}

			Env.Security.EDIMessage.ShowError();
		}
	}
}
