using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.DocumentEngine
{
	public class DocumentProtector
	{
		public DocumentProtector(Report report)
		{
			Report = report;
		}

		public void PasswordProtectForModifying()
		{
			if (Report.IsPasswordProtectedForModifying)
			{
				var password = GetExcelPasswordForModifying();
				if (!string.IsNullOrEmpty(password))
				{
					// Report.DocumentPasswordInformationEventCode = (ZString)Events.DocumentPasswordInformation.Code;
					Report.XlInterface.ProtectSheets(password);
				}
			}
		}

		public void PasswordProtectForOpening(StmPrintJob job, string filename)
		{
			if (Report.IsPasswordProtectedForOpening)
			{
				var password = GetExcelPasswordForOpening();
				if (!string.IsNullOrEmpty(password))
				{
					// Report.DocumentPasswordInformationEventCode = (ZString)Events.DocumentPasswordInformation.Code;
					job.SP_ExcelEncryptedPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(password);
				}
			}

			ExcelProtectedInfoForAddingEvent = GetExcelProtectedInfoForAddingEvent(job, filename);
			protectedForModifying = false;
			protectedForOpening = false;
		}

		public ZString ExcelProtectedInfoForAddingEvent { get; private set; }
		bool protectedForModifying;
		bool protectedForOpening;

		public static void EncryptExcelFileOpeningAccess(ZString path, ZString passwordForOpening, bool shouldDecryptPassword = true)
		{
			if (shouldDecryptPassword)
			{
				passwordForOpening = TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(passwordForOpening);
			}
			using (var excel = new ExcelInterface())
			{
				excel.LoadExcelFile(path);
				excel.Xls.Protection.OpenPassword = passwordForOpening;
				excel.SaveToFile(path);
			}
		}

		public ZString GetExcelProtectedInfoForAddingEvent(StmPrintJob printJob, ZString attachedFileName)
		{
			if (!SystemDataRegistry.Instance.AllocatePasswordProtectedExcelSpreadsheets.Value || !printJob.IsExcelAttachment || !printJob.IsEmailJob && !printJob.IsDocumentDeliverySuccessfulJob)
			{
				return ZString.Empty;
			}

			var protectedInfo = string.Empty;
			if (protectedForModifying && protectedForOpening)
			{
				protectedInfo = (NoResString)"both open and modify";
			}
			else if (protectedForModifying)
			{
				protectedInfo = (NoResString)"modify";
			}
			else if (protectedForOpening)
			{
				protectedInfo = (NoResString)"open";
			}
			else
			{
				return ZString.Empty;
			}

			var result = new ZStringBuilder();
			if (printJob != null)
			{
				result.Append(printJob.SP_DocumentType + "|");
			}

			if (!string.IsNullOrEmpty(attachedFileName))
			{
				result.Append(attachedFileName + "|");
			}

			if (!string.IsNullOrEmpty(protectedInfo))
			{
				result.Append($"Allocated with the {UserDescription} {protectedInfo} password.");
			}

			if (printJob != null)
			{
				result.Append("|" + printJob.PK);
			}

			return result.ToString();
		}

		ZString GetExcelPasswordForModifying()
		{
			var user = User;
			if (user != null && !string.IsNullOrEmpty(user.ExcelPasswordForModifying))
			{
				protectedForModifying = true;
				return user.ExcelPasswordForModifying;
			}

			return Env.Registry.ExcelPasswordForModifying;
		}

		ZString GetExcelPasswordForOpening()
		{
			var user = User;
			if (user != null && !string.IsNullOrEmpty(user.ExcelPasswordForOpening))
			{
				protectedForOpening = true;
				return user.ExcelPasswordForOpening;
			}

			return Env.Registry.ExcelPasswordForOpening;
		}

		IExcelPasswordRetrieverForDocumentDelivery User
		{
			get
			{
				if (Contact?.Staff != null)
				{
					return Contact.Staff;
				}
				else if (Contact?.Contact != null)
				{
					return Contact.Contact;
				}
				return null;
			}
		}

		ZString UserDescription
		{
			get
			{
				if (Contact?.Staff != null)
				{
					return $"Staff {Contact.Staff.GS_FullName}";
				}
				else if (Contact?.Contact != null)
				{
					return $"Contact {Contact.Contact.OC_ContactName}";
				}
				return ZString.Empty;
			}
		}

		DocDeliveryContact Contact
		{
			get
			{
				return Report.DeliveryContact;
			}
		}

		readonly Report Report;
	}
}
