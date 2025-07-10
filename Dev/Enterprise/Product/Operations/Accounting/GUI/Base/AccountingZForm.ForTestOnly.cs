#if DEBUG

using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AccountingZForm
	{
		public ZString FReversingReason_ForTestOnly
		{
			get { return fReversingReason; }
			set { fReversingReason = value; }
		}

		public void Save_ForTestOnly(ITransactionParticipant[] factories)
		{
			Save(factories);
		}

		public MenuItem ActionsMenuItem_ForTestOnly => ActionsMenuItem;

		public bool IsReversingMode_ForTestOnly => IsReversingMode;

		public Business.Base.Transaction.IReversing ReverseTransaction_ForTestOnly => ReverseTransaction;

		public bool IsViewOrDeleteMode_ForTestOnly => IsViewOrDeleteMode;

		public ContinueWithSave ValidateAndSave_ForTestOnly()
		{
			return ValidateAndSave();
		}

		public bool LastSaveSuccessful_ForTestOnly
		{
			get { return LastSaveSuccessful; }
			set { LastSaveSuccessful = value; }
		}

		public ZString FReversingCode_ForTestOnly
		{
			get { return fReversingCode; }
			set { fReversingCode = value; }
		}

		public ContinueWithDelete ShowPreDeleteDialogs_ForTestOnly()
		{
			return ShowPreDeleteDialogs();
		}

		public void Delete_ForTestOnly()
		{
			Delete();
		}

		public bool IsINTransactionWithApprovalRequest_ForTestOnly => IsINTransactionWithApprovalRequest;

		public bool IsINTransaction_ForTestOnly => IsINTransaction;

		public void SaveToRecentItems_ForTestOnly()
		{
			SaveToRecentItems();
		}

		public AccTransactionHeader Transaction_ForTestOnly => Transaction;

		public bool GetReversingReasonAndCode_ForTestOnly()
		{
			return GetReversingReasonAndCode();
		}

		public void OnPostButtonClick_ForTestOnly(object sender, EventArgs e)
		{
			OnPostButtonClick(sender, e);
		}

		public void OnClosing_ForTestOnly(CancelEventArgs e)
		{
			OnClosing(e);
		}

		public void ValidateAll_ForTestOnly(ValidationType type)
		{
			ValidateAll(type);
		}

		public void SaveInternal_ForTestOnly()
		{
			SaveInternal();
		}

		public IButton FPostButton_ForTestOnly => fPostButton;
		public IButton FApplyButton_ForTestOnly => fApplyButton;
		public IButton FCancelButton_ForTestOnly => fCancelButton;

		public static class FormVerbs_ForTestOnly
		{
			public static string Activate => FormVerbs.Activate;
			public static string Deactivate => FormVerbs.Deactivate;
			public static string Delete => FormVerbs.Delete;
			public static string View => FormVerbs.View;
			public static string Edit => FormVerbs.Edit;
			public static string New => FormVerbs.New;
		}

		public bool ShowAuditTabForTest => ShowAuditTab;
	}
}

#endif
