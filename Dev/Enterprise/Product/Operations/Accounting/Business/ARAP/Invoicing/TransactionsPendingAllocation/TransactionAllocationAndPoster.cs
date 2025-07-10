using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class TransactionAllocationAndPoster : IProcessor
	{
		public TransactionAllocationAndPoster(IWorkflowProvider provider)
		{
			invoicePendingAllocation = provider as TransactionPendingAllocation;
		}

		public void Process(INotifications notifications1, CancellationToken token = default)
		{
			if (invoicePendingAllocation == null)
			{
				return;
			}

			var notifications = new NotificationCollection();
			var convertionResult = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation);

			if (convertionResult.Invoice == null)
			{
				notifications.AddError(convertionResult.ErrorMessage);
			}
			else
			{
				var invoice = convertionResult.Invoice;

				notification = notifications1;
				invoice.OnNegativeCompliancesFailedToCreate += OnNegativeCompliancesFailedToCreate;
				var tempUser = invoice.Factory.Load<GlbStaff>(AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.Value);

				if (tempUser == null)
				{
					invoice.AddRowError(Res.GetString("F79B5F7B-A9FC-4903-A111-CD2009556FF3", "Please ensure you have set a user for security set at '{0}' before attempting to post.", AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.Location()));
				}
				else if (!tempUser.GS_IsActive)
				{
					invoice.AddRowError(Res.GetString("802c6933-7a69-497f-ae1f-7bbfdcf2d138", "Please ensure you have set an active user in '{0}' before attempting to post.", AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.Location()));
				}
				else if (tempUser.HomeBranch == null || tempUser.HomeDepartment == null)
				{
					if (tempUser.HomeBranch == null && tempUser.HomeDepartment == null)
					{
						invoice.AddRowError(Res.GetString("3422AB33-5FE9-4196-BB0B-B7C10DD0BFBE", "Please ensure that both Home Branch and Home Department are entered for the user set at '{0}' before attempting to post.", AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.Location()));
					}

					else if (tempUser.HomeBranch == null)
					{
						invoice.AddRowError(Res.GetString("55CD2EB1-CAD5-4388-B9B0-81C2E3693E56", "Please ensure that a Home Branch is entered for the user set at '{0}' before attempting to post.", AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.Location()));
					}

					else if (tempUser.HomeDepartment == null)
					{
						invoice.AddRowError(Res.GetString("5E6B2349-A60D-4E12-BB92-75194F7CF591", "Please ensure that a Home Department is entered for the user set at '{0}' before attempting to post.", AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.Location()));
					}
				}
				else if (Env.CurrentUserPK != tempUser.PK || Env.CurrentBranchPK != tempUser.HomeBranch.PK || Env.CurrentDepartmentPK != tempUser.HomeDepartment.PK)
				{
					invoice.AddRowError(Res.GetString("858a241f-da84-4b32-9462-6f165e741e2b", "The user context was changed after starting processing the ATP trigger. It may due to the value change of registry '{0}'.", AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.Location()));
				}
				else
				{
					invoice.RunPreSaveValidation();
					token.ThrowIfCancellationRequested();

					if (!invoice.HasErrors)
					{
						if (new InvoicingPreSaveHelper().PreSaveActions(invoice, new TransactionPendingAllocationPostingTriggerGUIProvider(invoice), false).CanProceed)
						{
							invoicePendingAllocation.Factory.ChildFactories.Add(invoice.Factory);
						}
					}
				}

				foreach (string error in invoice.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList())
				{
					notifications.AddError(error);
				}
			}

			if (notifications.HasErrors())
			{
				new TransactionProcessErrorEmail(invoicePendingAllocation).Send();
			}

			if (notifications.Any())
			{
				var stmNote = invoicePendingAllocation.CreateSystemNote(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote);
				stmNote.ST_NoteText = string.Join(
					System.Environment.NewLine,
					notifications.Select(notification => $"[{ZDateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}]{notification.Message}")
				);
			}

			notifications1.AddRange(notifications);
		}

		void OnNegativeCompliancesFailedToCreate(object sender, EventArgs e)
		{
			var invoice = (InvoicingBase)sender;
			var warningMsg = AccountingConstants.GetComplianceDocumentNegativeMessageWithInfo(invoice.AH_Ledger, invoice.AH_TransactionType, invoice.AH_TransactionNum);
			notification.AddWarning(warningMsg);
			var stmNote = invoicePendingAllocation.CreateSystemNote(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote);
			stmNote.ST_NoteText = $"[{ZDateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}]{warningMsg}";
		}

		readonly TransactionPendingAllocation invoicePendingAllocation;
		INotifications notification;
	}
}
