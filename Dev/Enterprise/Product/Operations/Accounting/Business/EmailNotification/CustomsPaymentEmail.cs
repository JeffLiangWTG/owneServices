using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using CustomsPaymentCreationResult = Enterprise.Accounting.Business.JobInvoicing.CustomsPaymentCreator.CustomsPaymentCreationResult;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class CustomsPaymentCreationEmail
	{
		public EmailSendResult Send(
			ZGuid recipient, BusinessObjectFactory factory, CustomsPaymentCreationResult paymentCreationResult,
			ZString apPaymentNumber)
		{
			EmailSendResult result = EmailSendResult.Unsuccessful;

			ZString subject = GetSubject(paymentCreationResult, apPaymentNumber);
			ZString body = GetBody(paymentCreationResult);

			HtmlNotificationEmailSender sender = new HtmlNotificationEmailSender();
			GlbStaff staff = factory.Load<GlbStaff>(recipient);

			if (staff == null)//it is a notification group
			{
				var groupEmail = factory.Load<GlbGroup>(recipient);
				if (groupEmail != null)
				{
					var email = sender.CreateEmail(subject, body);
					Env.OutgoingMailManager.CreateAndSave(email, recipient.ToGuid(), GroupSourceLocator.GetFromGroup(groupEmail));
				}
				else
				{
					var email = sender.CreateEmail(subject, body);
					Env.OutgoingMailManager.CreateAndSaveToPostmasterGroup(email);
				}
			}
			else
			{
				EmailDef email = sender.CreateEmail(subject, body);
				email.AddRecipientForUserCommunication(staff.GS_EmailAddress);
				Env.OutgoingMailManager.CreateAndSave(email);
			}

			result = EmailSendResult.Successful;

			return result;
		}

		ZString GetSubject(CustomsPaymentCreationResult paymentCreationResult, ZString apPaymentNumber)
		{
			var text = paymentCreationResult.PostingResult == JobInvoicing.PaymentPostingResult.Successful ?
				Res.GetString("b3883a7c-1382-410e-b8f5-02e03b5964d2", "Auto-Payment for") + " " :
				(paymentCreationResult.PostingResult == JobInvoicing.PaymentPostingResult.Partially ?
				Res.GetString("5F916DFD-920D-47F1-904A-4E19763D059A", "Auto-Payment partially successful for") + " " :
				Res.GetString("6456789e-17a2-4e18-8ade-8524d132a266", "Auto-Payment failure for") + " ");

			return text + apPaymentNumber;
		}

		ZString GetBody(CustomsPaymentCreationResult paymentCreationResult)
		{
			var body = new ZStringBuilder();

			body.Append(paymentCreationResult.NotificationText);

			body.Append(paymentCreationResult.EmailPresentationMessage);

			if (paymentCreationResult.DataProvidersWithNotifications.Count > 0)
			{
				body.Append(GetSummaryTable(paymentCreationResult));
			}
			return body.ToStringWithDelimiterBetweenAppends((NoResString)@"</br></br>");
		}

		string GetSummaryTable(CustomsPaymentCreationResult paymentCreationResult)
		{
			string[] columnTitles = new string[]
			{
				Res.GetString("24d76d10-d8c0-4385-839d-bc7616c156a6", "Unique Number"), Res.GetString("e0eee0ef-0216-4e15-8c03-7c6e00487e0a", "Customs Amount"), Res.GetString("5b78b87f-dc2d-490a-ba9a-bd6e128f40a9", "Customs Tax Amount"), Res.GetString("19e47d96-b686-45a4-983f-28b23a194f04", "AP Inv. Amount"), Res.GetString("2a201608-50c2-4dfe-b10c-b926847e49d1", "AP Inv. Tax Amount"), Res.GetString("0fb4f60e-050b-4b12-94d1-f2bfa8ad926c", "Comment")
			};

			HtmlTableCreator tableCreator = new HtmlTableCreator(columnTitles);

			foreach (IDataProviderWithNotifications dataProviderWithNotifications in paymentCreationResult.DataProvidersWithNotifications)
			{
				ZStringBuilder errors = new ZStringBuilder();

				foreach (ZString error in dataProviderWithNotifications.Errors)
				{
					errors.Append(error);
				}

				tableCreator.WriteRow(
					dataProviderWithNotifications.DataProvider.UniqueNumber,
					dataProviderWithNotifications.CustomsAmount,
					dataProviderWithNotifications.CustomsTaxAmount,
					dataProviderWithNotifications.InvoiceAmount,
					dataProviderWithNotifications.InvoiceTaxAmount,
					errors.ToStringWithNewLineBetweenAppends());
			}

			return tableCreator.ToHtml();
		}
	}
}
