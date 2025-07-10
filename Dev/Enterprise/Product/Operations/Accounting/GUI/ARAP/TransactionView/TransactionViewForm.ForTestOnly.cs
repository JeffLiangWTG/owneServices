#if DEBUG

using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.TransactionView
{
	public partial class TransactionViewForm
	{
		public ContinueWithSave ValidateAndSave_ForTestOnly()
		{
			return ValidateAndSave();
		}

		public void FCancelButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			fCancelButton_Click(sender, e);
		}

		public void OnClosing_ForTestOnly(CancelEventArgs e)
		{
			OnClosing(e);
		}

		public IButton FApplyButton_ForTestOnly => fApplyButton;

		public IButton FCancelButton_ForTestOnly => fCancelButton;

		public IButton FPostButton_ForTestOnly => fPostButton;

		public ZBool IsNewUnsavedObject_ForTestOnly => IsNewUnsavedObject;

		public ZBool IsCurrentContextMatching_ForTestOnly => IsCurrentContextMatching;
	}
}

#endif
