using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public abstract class EInvoicingTransactionErrorNotificationEmail : AccountingEmailDef
	{
		protected EInvoicingTransactionErrorNotificationEmail() : base()
		{
			ErrorMessageCollector = new ZStringBuilder();
			FromDisplayName = (NoResString)"E-Reporting Transaction Error Reporter";// No need to localise as email will be in English
			ContentType = EmailContentTypes.HTML;
		}

		readonly ZStringBuilder ErrorMessageCollector;

		public string[] GetAllErrorsThatOccuredWhileSendingEmail() => ErrorMessageCollector.ToStringWithNewLineBetweenAppends().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

		protected override GuidRegistryItem Recipient => AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup;

		protected override void DisplayEmailHasNoRecipientsError(string exceptionMessage)
		{
			AddExceptionMessageToErrorMessageCollector(exceptionMessage);
		}

		protected override void DisplayEmailNotCompleteError(string exceptionMessage)
		{
			AddExceptionMessageToErrorMessageCollector(exceptionMessage);
		}

		protected override void DisplayEmailSendFailedError(string exceptionMessage)
		{
			AddExceptionMessageToErrorMessageCollector(exceptionMessage);
		}

		void AddExceptionMessageToErrorMessageCollector(string exceptionMessage)
		{
			ErrorMessageCollector.Append(exceptionMessage);
		}

		protected string GetLinkToTransaction(ZString ledger, ZString transactionType, ZGuid transactionPK, ZString parentTableCode)
		{
			var link = ZString.Empty;
			ControllerID controllerID = null;
			switch (parentTableCode)
			{
				case AccTransactionHeaderSchema.Constants.Prefix:
					controllerID = EInvoicingControllerIdDecider.GetControllerIDRelatedToTransaction(ledger, transactionType);
					break;
				case AccComplianceDocumentHeaderSchema.Constants.Prefix:
					controllerID = EInvoicingControllerIdDecider.GetControllerIDRelatedToComplianceDocument(ledger);
					break;
			}

			if (controllerID != null)
			{
				link = ObjectFactory.Get<IShowViewFormUrlCreator>().Create(controllerID, transactionPK.ToGuid());
			}
			return link;
		}

#if DEBUG
		public ZString GetSubject_forTest()
		{
			return GetSubject();
		}

		public ZString GetBody_forTest()
		{
			return GetBody();
		}
#endif
	}
}
