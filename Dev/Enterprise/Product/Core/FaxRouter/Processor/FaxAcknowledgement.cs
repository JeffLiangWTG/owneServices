using System;
using System.Collections;
using System.Text;
#if NETFRAMEWORK
#pragma warning disable CW1086 // Do not use System.Web.Mail
using System.Web.Mail;
#pragma warning restore CW1086
#else
using System.Net.Mail;
#endif
using Enterprise.FaxRouter.MailSecurity;
using Enterprise.FaxRouter.TypeDefinitions;

namespace Enterprise.FaxRouter.Processor
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1086:DoNotUseSystemWebMail", Justification = "Baseline")]
	public class FaxAcknowledgement : FaxDataModule
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public string ProcessFaxJobConfirmation(MailDBItemDataLine aMailDBItem)
		{
			var sendEnterpriseAck = false;
			var chargeCode = ParseChargeCode(aMailDBItem);
			string returnStatus;

			if (!string.IsNullOrEmpty(chargeCode))
			{
				if (IsValidFaxRecipientJob(chargeCode))
				{
					var aMailDBItemRecord = GetFaxRecord(chargeCode);

					var sendFaxAck = aMailDBItemRecord.SysId != null && aMailDBItemRecord.SysId.Trim().Equals("UPSSYD")
						&& !string.IsNullOrWhiteSpace(aMailDBItemRecord.SysFaxJobId);

					var status = ParseStatus(aMailDBItem);

					if (sendFaxAck)
					{
						if (status == DeliveryNotificationType.FAILED)
						{
							SendFaxDeliveryNotification(aMailDBItemRecord, "Failed", DeliveryNotificationType.FAILED, EDI_SEND_FAX_ACK_EMAIL_ON_FAILURE);
						}
						else
						{
							SendFaxDeliveryNotification(aMailDBItemRecord, DeliveryNotificationType.SUCCESS, EDI_SEND_FAX_ACK_EMAIL_ON_SUCCESS);
						}
					}

					sendEnterpriseAck = aMailDBItemRecord.SysId == "ediEnterprise" && aMailDBItemRecord.SysFaxJobId != null;
					if (sendEnterpriseAck)
					{
						SendEnterpriseFaxDeliveryNotification(aMailDBItemRecord.SysFaxJobId, status, aMailDBItemRecord.From);
					}

					var aFaxAcknowledgementDataLine = new FaxAcknowledgementDataLine();
					aFaxAcknowledgementDataLine.AckId = Guid.NewGuid().ToString();
					aFaxAcknowledgementDataLine.AckBody = aMailDBItem.Body;
					aFaxAcknowledgementDataLine.AckChargeCode = chargeCode;
					aFaxAcknowledgementDataLine.FaxReciepientId = chargeCode;
					InsertFaxAcknowledgement(aFaxAcknowledgementDataLine);
					UpdateFaxRecipientWithAck(aFaxAcknowledgementDataLine.FaxReciepientId, (int)status);
				}
				else
				{
					returnStatus = TypeTranslator.AcknowledgementParsedTypeToString(AcknowledgementParsedType.CCD_NOT_IN_EDIFAXDB, chargeCode);
					EventLogging.EventLog.AddErrorEntry("FaxAck", returnStatus);
				}
			}
			else
			{
				returnStatus = TypeTranslator.AcknowledgementParsedTypeToString(AcknowledgementParsedType.CCD_NOT_IN_EMAIL, "");
				EventLogging.EventLog.AddErrorEntry("FaxAck", returnStatus);
			}

			returnStatus = TypeTranslator.AcknowledgementParsedTypeToString(AcknowledgementParsedType.SUCCESS, chargeCode);
			if (sendEnterpriseAck)
			{
				returnStatus += " [ediEnterprise ACK]";
			}

			return returnStatus;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		string ParseChargeCode(MailDBItemDataLine aMailDBItem)
		{
			var chargeCode = string.Empty;

			if (ACK_FORMAT.ToLower() == WWFax.ToLower())
			{
				var chargeCodeLocation = aMailDBItem.Body.IndexOf("Charge Code:");
				if (chargeCodeLocation > 0)
				{
					chargeCodeLocation += "Charge Code:".Length;
					chargeCode = aMailDBItem.Body.Substring(chargeCodeLocation, Guid.Empty.ToString().Length + 1).Trim();
				}
			}
			else if (ACK_FORMAT.ToLower() == TNZ.ToLower())
			{
				var subject = aMailDBItem.Subject;
				return subject.Substring(subject.LastIndexOf(". ") + 2, 36 /* length of GUID */);
			}
			else
			{
				throw new ApplicationException($"Configuration error: ACK_FORMAT should be {WWFax} or {TNZ}");
			}

			return chargeCode;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		DeliveryNotificationType ParseStatus(MailDBItemDataLine aMailDBItem)
		{
			var result = DeliveryNotificationType.UNKNOWN;

			if (ACK_FORMAT.ToLower() == WWFax.ToLower())
			{
				var faxesSentStartLocation = aMailDBItem.Body.IndexOf("Faxes Sent") + "Faxes Sent".Length;
				var faxesSentEndLocation = aMailDBItem.Body.IndexOf(".", faxesSentStartLocation);
				var faxAckSuccessStatus = int.Parse(aMailDBItem.Body.Substring(faxesSentStartLocation, faxesSentEndLocation - faxesSentStartLocation));
				if (faxAckSuccessStatus == 0)
				{
					return DeliveryNotificationType.FAILED;
				}
				else
				{
					return DeliveryNotificationType.SUCCESS;
				}
			}
			else if (ACK_FORMAT.ToLower() == TNZ.ToLower())
			{
				if (aMailDBItem.Subject.IndexOf("SUCCESS") >= 0)
				{
					result = DeliveryNotificationType.SUCCESS;
				}
				else if (aMailDBItem.Subject.IndexOf("FAIL") >= 0)
				{
					result = DeliveryNotificationType.FAILED;
				}
			}
			else
			{
				throw new ApplicationException($"Configuration error: ACK_FORMAT should be {WWFax} or {TNZ}");
			}

			return result;
		}

		public static void SendEmailNotificationForOverDueFaxAcknowledgements(ArrayList overDueAcknowledgements, bool sendEmailToEDIAdmin)
		{
			if (!sendEmailToEDIAdmin)
			{
				return;
			}

			var aMessageBody = new StringBuilder();
			var rowIndex = 0;

			foreach (EDIFaxDBRecipientDataLine aRecord in overDueAcknowledgements)
			{
				_ = aMessageBody.Append(FormatOverDueFaxMessageLine(aRecord, rowIndex++));
				_ = aMessageBody.Append("\n");
				OverDueAckFaxWarningSent(aRecord.FaxRecipientId);
			}

#if NETFRAMEWORK
			var message = new MailMessage
			{
				From = FAX_GATEWAY_EMAIL,
				Subject = $"EDI Fax Gateway - {rowIndex} Overdue Acknowledgements",
				BodyFormat = MailFormat.Text,
				Body = aMessageBody.ToString()
			};

			SmtpMail.SmtpServer = EDI_ACK_SMTP_SERVER;

			if (sendEmailToEDIAdmin)
			{
				message.To = FAX_ADMINISTRATOR_EMAIL;
				SmtpMail.Send(message);
			}
#else
			using (var client = new SmtpClient())
			{
				var message = new MailMessage
				{
					From = new MailAddress(FAX_GATEWAY_EMAIL),
					Subject = $"EDI Fax Gateway - {rowIndex} Overdue Acknowledgements",
					Body = aMessageBody.ToString()
				};

				if (sendEmailToEDIAdmin)
				{
					message.To.Add(FAX_ADMINISTRATOR_EMAIL);
					client.Send(message);
				}
			}
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const strng")]
		static string FormatOverDueFaxMessageLine(EDIFaxDBRecipientDataLine aRecord, int aRowIndex)
		{
			var result = new StringBuilder();

			const int CHARGE_CODE_FIELD_WIDTH = 18;
			const int FAX_RECIPIENT_ID_FIELD_WIDTH = 40;
			const int SENT_DATETIME_FIELD_WIDTH = 25;
			const int ATTENTION_NAME_FIELD_WIDTH = 20;
			const int FAX_NUMBER_FIELD_WIDTH = 12;
			const int COMPANY_FIELD_WIDTH = 20;
			const char PAD_CHAR = ' ';

			if (aRowIndex == 0)
			{
				_ = result.Append("Tracking No.".PadLeft(CHARGE_CODE_FIELD_WIDTH, PAD_CHAR));
				_ = result.Append("EDI FaxRecipientId".PadLeft(FAX_RECIPIENT_ID_FIELD_WIDTH, PAD_CHAR));
				_ = result.Append("Sent Date".PadLeft(SENT_DATETIME_FIELD_WIDTH, PAD_CHAR));
				_ = result.Append("Attention".PadLeft(ATTENTION_NAME_FIELD_WIDTH, PAD_CHAR));
				_ = result.Append("Fax No.".PadLeft(FAX_NUMBER_FIELD_WIDTH, PAD_CHAR));
				_ = result.Append("Company".PadLeft(COMPANY_FIELD_WIDTH, PAD_CHAR));
				_ = result.Append("\n");
			}

			_ = result.Append(aRecord.ChargeCode.PadLeft(CHARGE_CODE_FIELD_WIDTH, PAD_CHAR));
			_ = result.Append(aRecord.FaxRecipientId.ToString().PadLeft(FAX_RECIPIENT_ID_FIELD_WIDTH, PAD_CHAR));
			_ = result.Append(aRecord.SentDateTime.ToString(LONGDATEFORMAT).PadLeft(SENT_DATETIME_FIELD_WIDTH, PAD_CHAR));
			_ = result.Append(aRecord.AttentionName.Trim().PadLeft(ATTENTION_NAME_FIELD_WIDTH, PAD_CHAR));
			_ = result.Append(aRecord.FaxNumber.Trim().PadLeft(FAX_NUMBER_FIELD_WIDTH, PAD_CHAR));
			_ = result.Append(aRecord.Company.Trim().PadLeft(COMPANY_FIELD_WIDTH, PAD_CHAR));

			return result.ToString();
		}

		public static void SendFaxDeliveryNotification(MailDBItemDataLine aRecord, DeliveryNotificationType aDeliveryNotificationType, string aAckEmailAddress)
		{
			SendFaxDeliveryNotification(aRecord, "", aDeliveryNotificationType, aAckEmailAddress);
		}

		public static void SendFaxDeliveryNotification(MailDBItemDataLine aRecord, string aDeliveryMessage, DeliveryNotificationType aDeliveryNotificationType, string aAckEmailAddress)
		{
			if (FAX_ACK_IS_HUMAN)
			{
				SendHumanFaxDeliveryNotification(aRecord, aDeliveryMessage, aDeliveryNotificationType, aAckEmailAddress);
			}
			else
			{
				SendSystemFaxDeliveryNotification(aRecord, aDeliveryMessage, aDeliveryNotificationType, aAckEmailAddress);
			}
		}

		public static void SendFaxDeliveryNotification(MailDBItemDataLine aRecord, DeliveryNotificationType aDeliveryNotificationType, bool sendEmailToEDIAdmin)
		{
			SendFaxDeliveryNotification(aRecord, "", aDeliveryNotificationType, sendEmailToEDIAdmin);
		}

		public static void SendFaxDeliveryNotification(MailDBItemDataLine aRecord, string aDeliveryMessage, DeliveryNotificationType aDeliveryNotificationType, bool sendEmailToEDIAdmin)
		{
			if (FAX_ACK_IS_HUMAN)
			{
				SendHumanFaxDeliveryNotification(aRecord, aDeliveryMessage, aDeliveryNotificationType, sendEmailToEDIAdmin);
			}
			else
			{
				SendSystemFaxDeliveryNotification(aRecord, aDeliveryMessage, aDeliveryNotificationType, sendEmailToEDIAdmin);
			}
		}

		static void MessageBodyAppend(StringBuilder stringBuilder, string prefix, string property)
		{
			_ = stringBuilder.Append($"{prefix}={property}{System.Environment.NewLine}");
		}

		public static void SendSystemFaxDeliveryNotification(MailDBItemDataLine aRecord, string aDeliveryMessage, DeliveryNotificationType aDeliveryNotificationType, bool sendEmailToEDIAdmin)
		{
			if (!sendEmailToEDIAdmin)
			{
				return;
			}

			ICryptographicProvider aCryptProvider = new CryptProvider();
			var currentDateTime = GetCurrentDateTime().ToString(aCryptProvider.GetDateTimeFormat());

			var aMessageBody = new StringBuilder();

			MessageBodyAppend(aMessageBody, "HOUSEBILL", aRecord.ChargeCode);
			MessageBodyAppend(aMessageBody, "FAXNUMBER", aRecord.FaxRecipientNumber);
			MessageBodyAppend(aMessageBody, "FAXATTENTION", aRecord.FaxRecipientAttentionName);
			MessageBodyAppend(aMessageBody, "FAXATTENTIONCOMPANY", aRecord.FaxRecipientCompany);
			MessageBodyAppend(aMessageBody, "SYSFAXJOBID", aRecord.SysFaxJobId);
			MessageBodyAppend(aMessageBody, "SYSID", FAX_GATEWAY_SYSID);
			MessageBodyAppend(aMessageBody, "SENTDATETIME", currentDateTime);
			MessageBodyAppend(aMessageBody, "ACKKEY", aCryptProvider.GenerateKey(GetTiffFile(aRecord.PrimaryKey), currentDateTime, CryptKeyType.ACK));
			MessageBodyAppend(aMessageBody, "ACKCODE", ((int)aDeliveryNotificationType).ToString());
			_ = aMessageBody.Append($"ACKMESSAGE={aDeliveryMessage.Trim().ToUpper()}");

			for (var i = 0; i < 4; i++)
			{
				_ = aMessageBody.Append(System.Environment.NewLine);
			}

#if NETFRAMEWORK
			var message = new MailMessage
			{
				From = FAX_GATEWAY_EMAIL,
				Subject = $"EDI Fax Gateway - Delivery {TypeTranslator.DeliveryNotificationTypeToString(aDeliveryNotificationType)} ({aRecord.ChargeCode})",
				BodyFormat = MailFormat.Text,
				Body = aMessageBody.ToString()
			};

			SmtpMail.SmtpServer = EDI_ACK_SMTP_SERVER;

			if (sendEmailToEDIAdmin)
			{
				message.To = FAX_ADMINISTRATOR_EMAIL;
				SmtpMail.Send(message);
			}
#else
			using (var client = new SmtpClient())
			{
				var message = new MailMessage
				{
					From = new MailAddress(FAX_GATEWAY_EMAIL),
					Subject = $"EDI Fax Gateway - Delivery {TypeTranslator.DeliveryNotificationTypeToString(aDeliveryNotificationType)} ({aRecord.ChargeCode})",
					Body = aMessageBody.ToString()
				};

				if (sendEmailToEDIAdmin)
				{
					message.To.Add(FAX_ADMINISTRATOR_EMAIL);
					client.Send(message);
				}
			}
#endif
		}

		public static void SendSystemFaxDeliveryNotification(MailDBItemDataLine aRecord, string aDeliveryMessage, DeliveryNotificationType aDeliveryNotificationType, string aAckEmailAddress)
		{
			ICryptographicProvider aCryptProvider = new CryptProvider();
			var currentDateTime = GetCurrentDateTime().ToString(aCryptProvider.GetDateTimeFormat());

			var aMessageBody = new StringBuilder();

			MessageBodyAppend(aMessageBody, "HOUSEBILL", aRecord.ChargeCode);
			MessageBodyAppend(aMessageBody, "FAXNUMBER", aRecord.FaxRecipientNumber);
			MessageBodyAppend(aMessageBody, "FAXATTENTION", aRecord.FaxRecipientAttentionName);
			MessageBodyAppend(aMessageBody, "FAXATTENTIONCOMPANY", aRecord.FaxRecipientCompany);
			MessageBodyAppend(aMessageBody, "SYSFAXJOBID", aRecord.SysFaxJobId);
			MessageBodyAppend(aMessageBody, "SYSID", FAX_GATEWAY_SYSID);
			MessageBodyAppend(aMessageBody, "SENTDATETIME", currentDateTime);
			MessageBodyAppend(aMessageBody, "ACKKEY", aCryptProvider.GenerateKey(GetTiffFile(aRecord.PrimaryKey), currentDateTime, CryptKeyType.ACK));
			MessageBodyAppend(aMessageBody, "ACKCODE", ((int)aDeliveryNotificationType).ToString());
			_ = aMessageBody.Append($"ACKMESSAGE={aDeliveryMessage.Trim().ToUpper()}");

			for (var i = 0; i < 4; i++)
			{
				_ = aMessageBody.Append(System.Environment.NewLine);
			}

#if NETFRAMEWORK
			var message = new MailMessage
			{
				From = FAX_GATEWAY_EMAIL,
				Subject = $"EDI Fax Gateway - Delivery {TypeTranslator.DeliveryNotificationTypeToString(aDeliveryNotificationType)} ({aRecord.ChargeCode})",
				BodyFormat = MailFormat.Text,
				Body = aMessageBody.ToString()
			};

			SmtpMail.SmtpServer = EDI_ACK_SMTP_SERVER;

			message.To = aAckEmailAddress;
			SmtpMail.Send(message);
#else
			using (var client = new SmtpClient(EDI_ACK_SMTP_SERVER))
			{
				var message = new MailMessage
				{
					From = new MailAddress(FAX_GATEWAY_EMAIL),
					Subject = $"EDI Fax Gateway - Delivery {TypeTranslator.DeliveryNotificationTypeToString(aDeliveryNotificationType)} ({aRecord.ChargeCode})",
					Body = aMessageBody.ToString()
				};

				message.To.Add(aAckEmailAddress);
				client.Send(message);
			}
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public static void SendHumanFaxDeliveryNotification(MailDBItemDataLine aRecord, string aDeliveryMessage, DeliveryNotificationType aDeliveryNotificationType, bool sendEmailToEDIAdmin)
		{
			if (!sendEmailToEDIAdmin)
			{
				return;
			}

			var aMessageBody = new StringBuilder();
			MessageBodyAppend(aMessageBody, " HouseBill: ", aRecord.ChargeCode);
			MessageBodyAppend(aMessageBody, " Attention: ", aRecord.FaxRecipientAttentionName);
			MessageBodyAppend(aMessageBody, " Company: ", aRecord.FaxRecipientCompany);
			MessageBodyAppend(aMessageBody, " Fax No: ", aRecord.FaxRecipientNumber);
			MessageBodyAppend(aMessageBody, " Sent Date: ", aRecord.FaxRecipientSentDateTime.ToString(LONGDATEFORMAT));
			MessageBodyAppend(aMessageBody, " Acknowledgement Date: ", aRecord.FaxRecipientAckDateTime.ToString(LONGDATEFORMAT));
			_ = aMessageBody.Append($" Acknowledgement Type: {TypeTranslator.DeliveryNotificationTypeToString(aDeliveryNotificationType)}");

			if (aDeliveryNotificationType.Equals(DeliveryNotificationType.FAILED))
			{
				_ = aMessageBody.Append($" {aDeliveryMessage.Trim().ToUpper()}");
			}
			_ = aMessageBody.Append($"{System.Environment.NewLine} EDI Fax Job Ref.: {aRecord.FaxRecipientId}");

#if NETFRAMEWORK
			var message = new MailMessage
			{
				From = FAX_GATEWAY_EMAIL,
				Subject = $"EDI Fax Gateway - Delivery {TypeTranslator.DeliveryNotificationTypeToString(aDeliveryNotificationType)} ({aRecord.ChargeCode})",
				BodyFormat = MailFormat.Text,
				Body = aMessageBody.ToString()
			};

			SmtpMail.SmtpServer = EDI_ACK_SMTP_SERVER;

			if (sendEmailToEDIAdmin)
			{
				message.To = FAX_ADMINISTRATOR_EMAIL;
				SmtpMail.Send(message);
			}
#else
			using (var client = new SmtpClient(EDI_ACK_SMTP_SERVER))
			{
				var message = new MailMessage
				{
					From = new MailAddress(FAX_GATEWAY_EMAIL),
					Subject = $"EDI Fax Gateway - Delivery {TypeTranslator.DeliveryNotificationTypeToString(aDeliveryNotificationType)} ({aRecord.ChargeCode})",
					Body = aMessageBody.ToString()
				};

				if (sendEmailToEDIAdmin)
				{
					message.To.Add(FAX_ADMINISTRATOR_EMAIL);
					client.Send(message);
				}
			}
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public static void SendHumanFaxDeliveryNotification(MailDBItemDataLine aRecord, string aDeliveryMessage, DeliveryNotificationType aDeliveryNotificationType, string aAckEmailAddress)
		{
			var aMessageBody = new StringBuilder();
			MessageBodyAppend(aMessageBody, " HouseBill: ", aRecord.ChargeCode);
			MessageBodyAppend(aMessageBody, " Attention: ", aRecord.FaxRecipientAttentionName);
			MessageBodyAppend(aMessageBody, " Company: ", aRecord.FaxRecipientCompany);
			MessageBodyAppend(aMessageBody, " Fax No: ", aRecord.FaxRecipientNumber);
			MessageBodyAppend(aMessageBody, " Sent Date: ", aRecord.FaxRecipientSentDateTime.ToString(LONGDATEFORMAT));
			MessageBodyAppend(aMessageBody, " Acknowledgement Date: ", aRecord.FaxRecipientAckDateTime.ToString(LONGDATEFORMAT));
			_ = aMessageBody.Append($" Acknowledgement Type: {TypeTranslator.DeliveryNotificationTypeToString(aDeliveryNotificationType)}");

			if (aDeliveryNotificationType.Equals(DeliveryNotificationType.FAILED))
			{
				_ = aMessageBody.Append($" {aDeliveryMessage.Trim().ToUpper()}");
			}
			_ = aMessageBody.Append($"{System.Environment.NewLine} EDI Fax Job Ref.: {aRecord.FaxRecipientId}");

#if NETFRAMEWORK
			var message = new MailMessage
			{
				From = FAX_GATEWAY_EMAIL,
				Subject = $"EDI Fax Gateway - Delivery {TypeTranslator.DeliveryNotificationTypeToString(aDeliveryNotificationType)} ({aRecord.ChargeCode})",
				BodyFormat = MailFormat.Text,
				Body = aMessageBody.ToString()
			};

			SmtpMail.SmtpServer = EDI_ACK_SMTP_SERVER;

			message.To = aAckEmailAddress;
			SmtpMail.Send(message);
#else
			using (var client = new SmtpClient(EDI_ACK_SMTP_SERVER))
			{
				var message = new MailMessage
				{
					From = new MailAddress(FAX_GATEWAY_EMAIL),
					Subject = $"EDI Fax Gateway - Delivery {TypeTranslator.DeliveryNotificationTypeToString(aDeliveryNotificationType)} ({aRecord.ChargeCode})",
					Body = aMessageBody.ToString()
				};

				message.To.Add(aAckEmailAddress);
				client.Send(message);
			}
#endif
		}

		public virtual void SendEnterpriseFaxDeliveryNotification(string sysFaxJobId, DeliveryNotificationType status, string email)
		{
#if NETFRAMEWORK
			var message = new MailMessage();
			message.To = email;
			message.From = NOREPLY_EMAIL;
			message.Subject = $"{{EDIFAX}} {sysFaxJobId} {TypeTranslator.DeliveryNotificationTypeToString(status)}";
			SmtpMail.SmtpServer = EDI_ACK_SMTP_SERVER;

			SmtpMail.Send(message);
#else
			using (var client = new SmtpClient(EDI_ACK_SMTP_SERVER))
			{
				var message = new MailMessage();
				message.To.Add(email);
				message.From = new MailAddress(NOREPLY_EMAIL);
				message.Subject = $"{{EDIFAX}} {sysFaxJobId} {TypeTranslator.DeliveryNotificationTypeToString(status)}";
				client.Send(message);
			}
#endif
		}
	}
}
