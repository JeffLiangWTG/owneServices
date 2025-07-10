using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class TransactionProcessErrorEmail : AccountingEmailDef
	{
		public TransactionProcessErrorEmail(TransactionPendingAllocation errorTransaction)
		{
			Argument.NotNull(errorTransaction, nameof(errorTransaction));

			ContentType = EmailContentTypes.HTML;
			Factory = errorTransaction.Factory;
			Subject = GetSubjectCore(errorTransaction);
			Body = GetBodyCore(errorTransaction);
		}

		BusinessObjectFactory Factory { get; }

		protected override string GetBody()
		{
			return Body;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html")]
		string GetBodyCore(TransactionPendingAllocation errorTransaction)
		{
			var transactionLink = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.TransactionsPendingAllocation, errorTransaction.PK.ToGuid());
			var messageBuilder = new ZStringBuilder();
			messageBuilder.AppendLine(@"<html>");
			messageBuilder.AppendLine(@"<style>
	table {
		border-collapse:collapse 
	}
	table td {
		padding:0in 5.4pt 0in 5.4pt;	
		border:solid windowtext 1.0pt;
		height:14.5pt
	}
</style>");
			messageBuilder.AppendLine(@"<body>");
			messageBuilder.AppendLine(@"<p>The xml file associated with this transaction contains one or more warnings or errors associated with transactional data contained in the file.</p>");
			messageBuilder.AppendLine("");
			messageBuilder.AppendLine(@"<p>As a result, the transaction has not automatically posted from the Transactions Pending Allocation (TPA) screen in Cargowise using the ATP trigger.</p>");
			messageBuilder.AppendLine("");
			messageBuilder.AppendLine(@"<p>Follow the link to the transaction and go to the <strong>Notes</strong> tab to find the associated warnings/errors that will help you fix the transaction.</p>");
			messageBuilder.AppendLine("");
			messageBuilder.AppendLine($@"<p><span><a href='{transactionLink}'>{errorTransaction.AH_TransactionNum}</a></span></p>");
			messageBuilder.AppendLine("");
			messageBuilder.AppendLine(@"<p>Alternatively, if you are unable to access the link, use the following information to identify the transaction:</p>");
			AppendTable(messageBuilder, errorTransaction);
			messageBuilder.Append(@"</body></html>");
			return messageBuilder.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Builder")]
		void AppendTable(ZStringBuilder messageBuilder, TransactionPendingAllocation errorTransaction)
		{
			var orgHeader = Factory.Load<OrgHeader>(errorTransaction.AH_OH);
			messageBuilder.AppendLine("<table>");
			AppendRow(messageBuilder, "Creditor Code", orgHeader?.OH_Code);
			AppendRow(messageBuilder, "Creditor Name", orgHeader?.OH_FullName);
			AppendRow(messageBuilder, "Post Date", errorTransaction.AH_PostDate.ToShortDateString());
			AppendRow(messageBuilder, "Transaction number", errorTransaction.AH_TransactionNum);
			AppendRow(messageBuilder, "Amount (incl tax)", errorTransaction.AH_OSExTaxAmount.ToString());
			AppendRow(messageBuilder, "Description", errorTransaction.AH_Desc);
			messageBuilder.AppendLine(@"</table>");
		}

		void AppendRow(ZStringBuilder messageBuilder, string rowName, string value)
		{
			messageBuilder.AppendLine($@"<tr>
	<td>
	{rowName}:
	</td>
	<td width = 250>
	{value}
	</td>
</tr>");
		}

		protected override string GetSubject()
		{
			return Subject;
		}

		string GetSubjectCore(TransactionPendingAllocation errorTransaction)
		{
			return $@"Unable to post transaction {errorTransaction.AH_TransactionNum} due to warnings/errors identified as part of ATP trigger event";
		}

		protected override GuidRegistryItem Recipient
		{
			get
			{
				var registryItem = new ZGuid(NotificationDataRegistry.Instance.TransactionsPendingAllocationXMLPostingFailureNotificationGroup.Value);
				return !registryItem.IsValid || Factory.Load<GlbGroup>(registryItem) == null
					? NotificationDataRegistry.Instance.XMSFailureFallBackNotificationGroup
					: NotificationDataRegistry.Instance.TransactionsPendingAllocationXMLPostingFailureNotificationGroup;
			}
		}
	}
}
