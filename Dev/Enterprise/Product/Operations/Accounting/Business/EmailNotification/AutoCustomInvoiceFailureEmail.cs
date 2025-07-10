using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class AutoCustomInvoiceFailureEmail
	{
		public EmailSendResult Send(bool isFailure, ZGuid recipient, BusinessObjectFactory factory, List<CUSDSBPostingValidationResult> validationResults, ZString jobNumber, ZGuid jobPK, ControllerID controllerID, string jobType)
		{
			EmailSendResult result = EmailSendResult.Unsuccessful;
			if (validationResults.Count > 0)
			{
				HtmlNotificationEmailSender sender = new HtmlNotificationEmailSender();
				GlbStaff staff = factory.Load<GlbStaff>(recipient);

				if (staff == null)//it is a notification group
				{
					GlbGroup group = factory.Load<GlbGroup>(recipient);
					if (group != null)
					{
						var subject = GetSubject(isFailure, jobNumber, jobType);
						var body = GetBody(isFailure, validationResults, jobNumber, jobPK, controllerID, jobType);

						var email = sender.CreateEmail(subject, body);
						Env.OutgoingMailManager.CreateAndSave(email, recipient.ToGuid(), GroupSourceLocator.GetFromGroup(group));
					}
				}
				else
				{
					EmailDef email = sender.CreateEmail(GetSubject(isFailure, jobNumber, jobType), GetBody(isFailure, validationResults, jobNumber, jobPK, controllerID, jobType));
					email.AddRecipientForUserCommunication(staff.GS_EmailAddress);
					Env.OutgoingMailManager.CreateAndSave(email);
				}

				result = EmailSendResult.Successful;
			}
			return result;
		}

		internal protected string GetBody(bool isFailure, List<CUSDSBPostingValidationResult> validationResults, ZString jobNumber, ZGuid jobPK, ControllerID controllerID, string jobType)
		{
			ZStringBuilder result = new ZStringBuilder();

			string hyperLinkToJob = GetHyperLinkForJobNumber(isFailure, jobNumber, jobPK, controllerID, jobType);
			string errorMessage = Res.GetString("5b2cc931-9f30-4764-90a2-fa497bb51db5", "The following errors were encountered while trying to automatically process billing charges.");

			result.Append(hyperLinkToJob + (NoResString)"<br /><br />" + errorMessage + (NoResString)"<br /><br /><hr>");

			foreach (CUSDSBPostingValidationResult validationResult in validationResults)
			{
				result.Append(GetBody(validationResult));
				result.Append((NoResString)"<br /><hr><br />");
			}

			return result.ToString();
		}

		protected string GetBody(CUSDSBPostingValidationResult validationResult)
		{
			ZStringBuilder result = new ZStringBuilder();
			string processingMsg = Res.GetString("6694278b-a748-4148-988f-7826ae9c7007", "Processing result for") + " ";

			result.Append((NoResString)"<strong>" + processingMsg + validationResult.UniqueNumber + (NoResString)"</strong><br /><br />");

			foreach (string message in validationResult.Messages)
			{
				result.Append(message + "<br />");
			}

			if (validationResult.APDiscrepancyCharges.Length > 0)
			{
				result.Append(GetEmailBodyForDiscrepancyCharges(validationResult.APInvoiceDetail, validationResult.APDiscrepancyCharges, "AP"));
			}

			if (validationResult.ARDiscrepancyCharges.Length > 0)
			{
				result.Append(GetEmailBodyForDiscrepancyCharges(validationResult.ARInvoiceDetail, validationResult.ARDiscrepancyCharges, "AR"));
			}

			return result.ToString();
		}

		string GetEmailBodyForDiscrepancyCharges(ZString invoiceDetail, ChargesSummary[] charges, string invoiceType)
		{
			ZStringBuilder result = new ZStringBuilder();

			result.Append(Res.GetString("d8fea9bd-3e4c-47a9-889b-2599894844e5", "There is already an {0} Invoice posted for the job {1}. There are differences in the details for these charges, please review the differences below. You may have to reverse the existing invoice manually and re-enter with correct details.", invoiceType, invoiceDetail));
			result.Append("<br /><br />");

			HtmlTableCreator table = new HtmlTableCreator(new string[] { (NoResString)"CHARGE CODE", "DESCRIPTION", invoiceType + (NoResString)" INV. AMOUNT", (NoResString)"CUSTOMS AMOUNT", "DIFFERENCE" });

			ZDecimal totalExistingAmount = 0m;
			ZDecimal totalNewAmount = 0m;

			foreach (ChargesSummary summary in charges)
			{
				ZDecimal difference = Math.Abs(summary.ExistingExTaxAmount - summary.NewExTaxAmount);

				table.WriteRow(summary.ChargeCode, summary.Description, GetFormattedAmount(summary.ExistingExTaxAmount, difference), GetFormattedAmount(summary.NewExTaxAmount, difference), GetDifferenceAmount(difference));

				totalExistingAmount += summary.ExistingExTaxAmount;
				totalNewAmount += summary.NewExTaxAmount;

				if (!summary.AdditionalDescription.IsEmpty)
				{
					ZString[] additionalDescs = summary.AdditionalDescription.Split(System.Environment.NewLine.ToCharArray());

					foreach (string desc in additionalDescs)
					{
						if (!string.IsNullOrEmpty(desc.Trim()))
						{
							table.WriteRow("", desc, "", "", "");
						}
					}
				}
			}

			ZDecimal totalDifference = Math.Abs(totalExistingAmount - totalNewAmount);
			table.WriteRow("TOTAL", "", GetFormattedAmount(totalExistingAmount, totalDifference), GetFormattedAmount(totalNewAmount, totalDifference), GetDifferenceAmount(totalDifference));

			result.Append(table.ToHtml() + "<br />");

			return result.ToString();
		}

		string GetFormattedAmount(ZDecimal amount, ZDecimal difference)
		{
			return amount.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals) + (difference > 0m ? " *" : "");
		}

		string GetDifferenceAmount(ZDecimal difference)
		{
			return difference > 0 ? difference.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals) : "-";
		}

		string GetHyperLinkForJobNumber(bool isFailure, ZString jobNumber, ZGuid jobPK, ControllerID controllerID, string jobType)
		{
			string url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(controllerID, jobPK.ToGuid());
			return (NoResString)"<br /><strong>" + GetHeaderText(isFailure, jobType) + (NoResString)" <a href=\"" + url + (NoResString)"\">" + jobNumber + (NoResString)"</a></strong>";
		}

		internal protected string GetSubject(bool isFailure, ZString jobNumber, string jobType)
		{
			return GetHeaderText(isFailure, jobType) + jobNumber;
		}

		string GetHeaderText(bool isFailure, string jobType)
		{
			return (isFailure ? Res.GetString("2218fdf2-4ba5-4484-ab00-8007f36a368f", "Auto-Billing failure for") + " " : Res.GetString("1b448fea-6325-474b-940e-030829ca8fcf", "Auto-Billing result for") + " ") + jobType + " ";
		}

		public static string GetCustomsChargeDetails(Job job)
		{
			var stringBuilder = new ZStringBuilder();

			if (job != null)
			{
				GetChargeListHtmlHeader(stringBuilder);
				GetChargeListHtmlTableHeader(stringBuilder);

				foreach (Charge charge in job.Charges)
				{
					var creditor = job.Factory.Load<OrgHeader>(charge.JR_OH_CostAccount);
					var debtor = job.Factory.Load<OrgHeader>(charge.JR_OH_SellAccount);

					GetChargeHtmlRecord(stringBuilder, charge.ChargeCode, charge.JR_Desc, ZString.Empty, creditor != null ? creditor.OH_Code : ZString.Empty, charge.JR_CostCurrency, charge.JR_OSCostAmt
						, debtor != null ? debtor.OH_Code : ZString.Empty, charge.JR_SellCurrency, charge.JR_OSSellAmt);
				}

				GetChargeListHtmlTableFooter(stringBuilder);
			}

			return stringBuilder.ToString();
		}

		public static string GetCustomsChargeDetails(IAccIntegrationDataProvider dataProvider)
		{
			return GetCustomsChargeDetails(dataProvider.InvDataProviders);
		}

		static string GetCustomsChargeDetails(IAccInvoiceDataProvider[] dataProviders)
		{
			var stringBuilder = new ZStringBuilder();

			if (dataProviders != null)
			{
				GetChargeListHtmlHeader(stringBuilder);
				foreach (var line in dataProviders)
				{
					stringBuilder.Append(Res.GetString("19f00a86-7bb1-4d3f-ac6d-cc48b95ee954", "Line: {0}", line.UniqueNumber));
					GetChargeListHtmlTableHeader(stringBuilder);

					foreach (var customsCharges in line.CustomsCharges)
					{
						foreach (var charge in customsCharges.GetCustomsCharges(null))
						{
							var chargeCode = charge.GetChargeCode(line.Factory);
							var creditor = line.Factory.Load<OrgHeader>(charge.CreditorPK);
							var debtor = line.Factory.Load<OrgHeader>(charge.DebtorPK);

							if (creditor != null)
							{
								GetChargeHtmlRecord(stringBuilder, chargeCode, charge.Description, charge.EntryReference, creditor.OH_Code, charge.OverrideCurrency, charge.Amount
									, ZString.Empty, ZString.Empty, 0);
							}
							else if (debtor != null)
							{
								GetChargeHtmlRecord(stringBuilder, chargeCode, charge.Description, charge.EntryReference, ZString.Empty, ZString.Empty, 0
									, debtor.OH_Code, charge.OverrideCurrency, charge.Amount);
							}
						}
					}
					GetChargeListHtmlTableFooter(stringBuilder);
				}
			}

			return stringBuilder.ToString();
		}

		static void GetChargeListHtmlHeader(ZStringBuilder stringBuilder)
		{
			stringBuilder.Append(Res.GetString("bc24076f-55d6-46fb-896e-1fe8569a5087", "Charge(s):"));
			stringBuilder.Append("<br />");
		}

		static void GetChargeListHtmlTableHeader(ZStringBuilder stringBuilder)
		{
			stringBuilder.Append((NoResString)"<table>");
			stringBuilder.Append((NoResString)"<tr><th>");
			stringBuilder.Append(Res.GetString("ebaaffb7-444b-4bc5-8d83-a4e0cb94f58a", "Charge Code"));
			stringBuilder.Append((NoResString)"</th><th>");
			stringBuilder.Append(Res.GetString("f07fe04d-4919-4f7f-862f-bdba2ffe8d55", "Description"));
			stringBuilder.Append((NoResString)"</th><th>");
			stringBuilder.Append(Res.GetString("b36ccb5b-de9e-4c8b-8415-a5e743bac109", "Entry Ref."));
			stringBuilder.Append((NoResString)"</th><th>");
			stringBuilder.Append(Res.GetString("e3493c00-a5d2-4357-9f44-1e6eaad60aac", "Creditor"));
			stringBuilder.Append((NoResString)"</th><th>");
			stringBuilder.Append(Res.GetString("68c64357-4099-4fd6-9644-0a9d856eae4f", "Cost Currency"));
			stringBuilder.Append((NoResString)"</th><th>");
			stringBuilder.Append(Res.GetString("8b799c12-bc6f-4c53-909d-9e75f756f39d", "Cost Amount"));
			stringBuilder.Append((NoResString)"</th><th>");
			stringBuilder.Append(Res.GetString("b67dd812-d7b1-423c-ac14-2beb1cb4f331", "Debtor"));
			stringBuilder.Append((NoResString)"</th><th>");
			stringBuilder.Append(Res.GetString("b713b499-7f33-4bef-98d9-811c02399497", "Sell Currency"));
			stringBuilder.Append((NoResString)"</th><th>");
			stringBuilder.Append(Res.GetString("615b2886-5ccc-4a53-bd9a-a393cd5eddcf", "Sell Amount"));
			stringBuilder.Append((NoResString)"</th></tr>");
		}

		static void GetChargeListHtmlTableFooter(ZStringBuilder stringBuilder)
		{
			stringBuilder.Append((NoResString)"</table>");
		}

		static void GetChargeHtmlRecord(ZStringBuilder stringBuilder, AccChargeCode chargeCode, ZString description, ZString entityRef, ZString creditorCode, ZString costCurrency, ZDecimal costAmount,
			ZString debtorCode, ZString sellCurrency, ZDecimal sellAmount)
		{
			stringBuilder.Append((NoResString)"<tr>");
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"<td>{0}</td>", chargeCode != null ? chargeCode.AC_Code : ZString.Empty));
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"<td>{0}</td>", description));
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"<td>{0}</td>", entityRef));
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"<td>{0}</td>", creditorCode));
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"<td>{0}</td>", costCurrency));
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"<td>{0}</td>", costAmount));
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"<td>{0}</td>", debtorCode));
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"<td>{0}</td>", sellCurrency));
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"<td>{0}</td>", sellAmount));
			stringBuilder.Append((NoResString)"</tr>");
		}
	}
}
