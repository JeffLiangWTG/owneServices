using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("The Bare sub class is used")]
	[ToolboxItem(false)]
	public class ZSearchCodeBox : ZDropCodeBox
	{
		#region Bare

		[ToolboxItem(false)]
		public new class Bare : ZSearchCodeBox
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion
		protected override void OnTextChangedCore()
		{
			if (ParentDropEdit != null && ParentDropEdit.IsBound && ParentDropEdit.IsDroppedDown && string.IsNullOrEmpty(ParentDropEdit.BindToForDescription))
			{
				UpdateDropFormRefreshOnIdle();
			}
		}
		IDisposable lastDropFormRefreshWorkItem;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer constant")]
		void UpdateDropFormRefreshOnIdle()
		{
			lastDropFormRefreshWorkItem?.Dispose();
			lastDropFormRefreshWorkItem = UserIdleWorker.QueueWorkItem(this, "Updating ZSearchCodeBox description OnTextChanged",
				200, UserIdleWorkItemOptions.DisableSlowRunningWarning | UserIdleWorkItemOptions.AllowAlways, new MethodInvoker(UpdateDropFormRefresh));
		}

		void UpdateDropFormRefresh()
		{
			((ZSearchButton)ParentDropEdit.DropButton).DropDownSeacrhForm.RefreshList();
		}

#if WINZOR
		protected override bool AutoComplete => false;
#endif
		internal override void AutoCompleteText(KeyPressEventArgs e)
		{
		}
	}
}
