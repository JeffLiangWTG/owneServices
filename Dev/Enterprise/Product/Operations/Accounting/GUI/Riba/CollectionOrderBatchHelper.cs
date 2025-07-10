using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.Riba
{
	public static class CollectionOrderBatchHelper
	{
		public static string SelectAtLeastOneOrderFirstErrorMessage => Res.GetString("54a2a51f-e346-4652-bb6a-624de5ceec1c", "Please select at least one order first.");
		public static string SelectOrderFirstErrorMessage => Res.GetString("34336b9b-ccfd-4ac7-b08f-0940ddae8253", "Please select one order first.");
		static string CannotAddTranToRejectedOrderErrorMessage => Res.GetString("962b9f45-750d-43b2-8339-98b96a89afc2", "You are not allowed to add new transactions to a rejected order.");
		static string OrderAlreadyMatchedWithReceiptErrorMessage => Res.GetString("6bd4d822-b29e-43f6-8179-b248f6199780", "This order has already been fully paid with a receipt.");
		static string OrdersAlreadyMatchedWithReceiptErrorMessage => Res.GetString("ba12939f-9299-44a5-b87f-a31f34eef1ff", "Some order have already been fully paid with a receipt.");
		static string OrderAlreadyRejectedErrorMessage => Res.GetString("f0ecb316-839a-41e4-ae30-18f70e5efe15", "This order has already been rejected.");
		static string OrdersAlreadyRejectedErrorMessage => Res.GetString("8d5d2514-6de0-4c9b-a5f7-67a9b1252f58", "Some orders have already been rejected.");
		static string OrderRejectedReasonMessage => Res.GetString("0c6e738f-d032-4e9d-af10-c51da4be7ece", "Please enter the reason for Rejecting this order");

		public static void AddTransactionsToOrder(AccCollectionOrder order, SecurityCheckpoint securityCheckPoint, bool includeInBatch = true)
		{
			Argument.NotNull(order, nameof(order));
			Argument.NotNull(securityCheckPoint, nameof(securityCheckPoint));

			if (!securityCheckPoint.IsAllowed)
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
			}
			else if (order.IsCancelled)
			{
				Globals.Message.ShowInformation(CannotAddTranToRejectedOrderErrorMessage);
			}
			else if (order.IsMatchedWithReceipt)
			{
				Globals.Message.ShowInformation(OrderAlreadyMatchedWithReceiptErrorMessage);
			}
			else if (order.ACO_DepositedDate.IsValid)
			{
				Globals.Message.ShowInformation(AccountingConstants.OrderAlreadyCompletedErrorMessage);
			}
			else
			{
				using (var addTransactionsForm = new AddTransactionsToOrderForm(new OrderTransactionsFilterHolder(order)))
				{
					order.SetIncludeInBatchWithoutRecalculateBatchAmount(includeInBatch);
					ZFormModaliser.ShowDialogAndDispose(addTransactionsForm);
				}
			}
		}

		public static void RejectOrder(AccCollectionOrder order, SecurityCheckpoint securityCheckPoint, bool doSave)
		{
			Argument.NotNull(order, nameof(order));
			Argument.NotNull(securityCheckPoint, nameof(securityCheckPoint));

			if (!securityCheckPoint.IsAllowed)
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
			}
			else if (order.IsCancelled)
			{
				Globals.Message.ShowInformation(OrderAlreadyRejectedErrorMessage);
			}
			else if (order.IsMatchedWithReceipt)
			{
				Globals.Message.ShowInformation(OrderAlreadyMatchedWithReceiptErrorMessage);
			}
			else
			{
				DialogResult dialogResult = DialogResult.Yes;
				if (order.ACO_DepositedDate.IsValid)
				{
					dialogResult = Globals.Message.Show(Res.GetString("d2a1e3bd-004f-4f86-b9a6-ef39c91b4749", "The collection order has been completed with a deposited date saved. Are you sure you want to reject this order?"), Res.GetString("50d7be46-f790-46f3-a96f-a1093ca0248c", "Question"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
				}

				if (dialogResult == DialogResult.Yes)
				{
					RejectOrderReasonHolder rejectHolder = new RejectOrderReasonHolder();
					using (var orderRejectReasonForm = new OrderRejectReasonForm(rejectHolder, OrderRejectedReasonMessage, Res.GetString("27368da7-1020-46b5-8d9b-12cbd1cf4392", "Reject Reason")))
					{
						ZFormModaliser.ShowDialogAndDispose(orderRejectReasonForm);
					}
					if (!string.IsNullOrEmpty(rejectHolder.Reason))
					{
						order.Reject(Res.GetString("91fb3bb3-c3be-46a7-a786-7dd3c6e69091", "- {0} - {1} Entered By {2}", rejectHolder.Code, rejectHolder.Reason, GlbStaff.CurrentUser.GS_LoginName));
						if (doSave)
						{
							order.Factory.Save();
						}
					}
				}
			}
		}

		static DialogResult GetBackPostDate(BackDatePostHolder backDatePostHolder)
		{
			Argument.NotNull(backDatePostHolder, nameof(backDatePostHolder));

			var result = DialogResult.OK; //Cancel;
			var securityCheckPoint = Env.Security.CollectionBatchAllowBackPosting;

			if (!securityCheckPoint.IsAllowed)
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
			}
			else if (AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value)
			{
				using (var backDateForm = new BackDatePostForm(backDatePostHolder))
				{
					result = ZFormModaliser.ShowDialogAndDispose(backDateForm);
				}
			}

			return result;
		}

		public static void CreateReceipts(IEnumerable<AccCollectionOrder> orders, SecurityCheckpoint securityCheckPoint)
		{
			Argument.NotNull(orders, nameof(orders));
			Argument.NotNull(securityCheckPoint, nameof(securityCheckPoint));

			if (!securityCheckPoint.IsAllowed)
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
			}
			else if (!orders.Any())
			{
				Globals.Message.ShowError(SelectAtLeastOneOrderFirstErrorMessage);
			}
			else if (orders.Any(x => x.IsCancelled))
			{
				Globals.Message.ShowError(OrdersAlreadyRejectedErrorMessage);
			}
			else if (orders.Any(x => x.IsMatchedWithReceipt))
			{
				Globals.Message.ShowInformation(OrdersAlreadyMatchedWithReceiptErrorMessage);
			}
			else
			{
				var backDatePostHolder = new BackDatePostHolder();
				var result = CollectionOrderBatchHelper.GetBackPostDate(backDatePostHolder);

				if (result == DialogResult.OK)
				{
					var factory = new BusinessObjectFactory();
					foreach (var order in orders)
					{
						try
						{
							order.CreateReceiptsAndDepositBatch(factory, backDatePostHolder.BackPostDate, backDatePostHolder.BackInvoiceDate, skipCreateDepositBatch: false);
						}
						catch (CollectionBatchProcessException ex)
						{
							Globals.Message.ShowError(ex.UserFriendlyMessage);
							return;
						}
					}
					Globals.Message.ShowInformation(Res.GetString("d1739716-c6dc-4678-b876-df79e4aa9df5", "Receipts and Deposit Batch are created successfully."));
				}
			}
		}

		public static void CreateReceiptsAndOneDepositBatch(AccCollectionBatch batch, SecurityCheckpoint securityCheckPoint)
		{
			Argument.NotNull(batch, nameof(batch));
			Argument.NotNull(securityCheckPoint, nameof(securityCheckPoint));

			if (!securityCheckPoint.IsAllowed)
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
			}
			else
			{
				var backDatePostHolder = new BackDatePostHolder();
				var result = GetBackPostDate(backDatePostHolder);

				if (result == DialogResult.OK)
				{
					ZString errorMessage;
					if (!batch.CreateReceiptsAndDepositBatch(out errorMessage, backDatePostHolder.BackPostDate, backDatePostHolder.BackInvoiceDate, createDepositBatchPerOrder: false))
					{
						Globals.Message.ShowError(errorMessage);
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("fe166f30-faa4-4146-9bec-9bf581f6a12b", "Receipts and Deposit Batch are created successfully."));
					}
				}
			}
		}

		public static void CreateReceiptsAndIndividualDepositBatch(AccCollectionBatch batch, SecurityCheckpoint securityCheckPoint)
		{
			Argument.NotNull(batch, nameof(batch));
			Argument.NotNull(securityCheckPoint, nameof(securityCheckPoint));

			if (!securityCheckPoint.IsAllowed)
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
			}
			else
			{
				var backDatePostHolder = new BackDatePostHolder();
				var result = GetBackPostDate(backDatePostHolder);

				if (result == DialogResult.OK)
				{
					ZString errorMessage;
					if (!batch.CreateReceiptsAndDepositBatch(out errorMessage, backDatePostHolder.BackPostDate, backDatePostHolder.BackInvoiceDate, createDepositBatchPerOrder: true))
					{
						Globals.Message.ShowError(errorMessage);
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("c7a52db4-e6ce-41bc-9f95-2d5aaedb5cd4", "Receipts and Deposit Batches are created successfully."));
					}
				}
			}
		}

		public static void PrintCollectionOrders(AccCollectionOrder[] accCollectionOrders, SecurityCheckpoint securityCheckPoint)
		{
			if (!securityCheckPoint.IsAllowed)
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
			}
			else
			{
				using (PrintTask printTask = GetPrintTask(accCollectionOrders))
				{
					printTask?.Run(Env.Security.None);
				}
			}
		}

		static PrintTask GetPrintTask(AccCollectionOrder[] selectedOrders)
		{
			PrintTask task = null;

			var packs = new List<DocumentPack>();
			var stmMenuItem = GetOrderDocumentCommand(selectedOrders[0]);

			selectedOrders.ForEach((x) =>
			{
				var pack = new DocumentPack(stmMenuItem, x, null, null);
				packs.Add(pack);
			});

			if (packs.Count > 0)
			{
				task = new PrintTask();
				task.AddRange(packs);
			}

			return task;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter related")]
		static DocumentCommand GetOrderDocumentCommand(AccCollectionOrder accCollectionOrder)
		{
			var commandFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Collection Advice");
			commandFilter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.CollectionOrder));
			commandFilter.AddToFilter(StmMenuItemSchema.SU_IsPublished, Core.Constants.BooleanTrueChar);
			var documentCommands = new DocumentCommandCollection(accCollectionOrder);
			documentCommands.Load();
			BusinessObject[] commands = documentCommands.Find(commandFilter);
			if (!commands.Any())
			{
				throw new ReportException("Document Command was not found. Document not printed.");
			}
			return (DocumentCommand)commands[0];
		}
	}
}
