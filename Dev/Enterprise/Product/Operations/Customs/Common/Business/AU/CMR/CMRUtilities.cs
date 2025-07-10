using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRUtilities
	{
		public CMRUtilities()
		{
		}

		public bool ShouldImportMessageBeSentCMR(ZDateTime currentDate, ZDateTime firstDateOfArrival, bool hasLegacyEntryNumber, bool hasCMREntryNumber)
		{
			bool result;
			//if (HaveAnyNonCMRMessagesBeenSent(Messages)) Result = false;
			//else if (HaveAnyCMRMessagesBeenSent(Messages)) Result = true;
			if (hasLegacyEntryNumber)
			{
				result = false;
			}
			else if (hasCMREntryNumber)
			{
				result = true;
			}
#if DEBUG
			else if (Globals.IsTest && Env.Registry.AUCustomsImportsMessagingMode == Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages)
			{
				result = true;
			}
			else if (Globals.IsTest && Env.Registry.AUCustomsImportsMessagingMode == Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages)
			{
				result = false;
			}
#endif
			else if (currentDate < Core.Constants.AUCustoms.CMRImportsGoLiveDate)
			{
				result = false;
			}
			else if (currentDate >= Core.Constants.AUCustoms.CMRImportsCutOverDate)
			{
				result = true;
			}
			else if (firstDateOfArrival.IsEmpty || firstDateOfArrival < Core.Constants.AUCustoms.CMRImportsCutOverDate)
			{
				result = false;
			}
			else
			{
				result = true;
			}

			return result;
		}

		public bool AreWeRunningInCMR(ZDateTime currentDate)
		{
			bool result;
#if DEBUG
			if (Globals.IsTest && Env.Registry.AUCustomsImportsMessagingMode == Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages)
			{
				result = true;
			}
			else if (Globals.IsTest && Env.Registry.AUCustomsImportsMessagingMode == Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages)
			{
				result = false;
			}
			else
#endif
			if (currentDate < Core.Constants.AUCustoms.CMRImportsGoLiveDate)
			{
				result = false;
			}
			else if (currentDate >= Core.Constants.AUCustoms.CMRImportsCutOverDate)
			{
				result = true;
			}
			else
			{
				result = true;
			}

			return result;
		}

		public ZString ConvertOldAirCargoPaymentType(ZString oldType)
		{
			ZString result;
			switch (oldType)
			{
				case "CCX":
					result = CMRMethodsOfPayment.Codes.Collect;
					break;
				case "PPD":
					result = CMRMethodsOfPayment.Codes.PrepaidOnly;
					break;
				default:
					result = oldType;
					break;
			}
			return result;
		}

		public static bool MessageStatusChangedToAccepted(ZPropertyInfo info)
		{
			ZString infoValue = (ZString)info.Value;
			return (ZString)info.OriginalValue != infoValue &&
				(infoValue == CMRBaseStatuses.Codes.OriginalAccepted ||
				infoValue == CMRBaseStatuses.Codes.AmendmentAccepted ||
				infoValue == CMRBaseStatuses.Codes.WithdrawalAccepted);
		}

		public static bool MessageStatusChangedToRejected(ZPropertyInfo info)
		{
			ZString infoValue = (ZString)info.Value;
			return (ZString)info.OriginalValue != infoValue &&
				(infoValue == CMRBaseStatuses.Codes.OriginalRejected ||
				infoValue == CMRBaseStatuses.Codes.AmendmentRejected ||
				infoValue == CMRBaseStatuses.Codes.WithdrawalRejected);
		}

		public static bool MessageStatusChangedToWithdrawWaiting(ZPropertyInfo info)
		{
			ZString infoValue = (ZString)info.Value;
			return (ZString)info.OriginalValue != infoValue &&
				infoValue == CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
		}

		public static bool MessageStatusChangedToWaiting(ZPropertyInfo info)
		{
			ZString infoValue = (ZString)info.Value;
			return (ZString)info.OriginalValue != infoValue &&
				(infoValue == CMRBaseStatuses.Codes.AwaitingResponseToOriginal ||
				infoValue == CMRBaseStatuses.Codes.AwaitingResponseToAmendment);
		}

		#region Implementation

		public string[] GetCertificateErrors(BusinessObjectFactory factory)
		{
			var errors = CheckCertificates(factory);
			return errors.Where(x => !x.Contains((NoResString)"Warning")).ToArray();
		}

		public string[] CheckCertificates(BusinessObjectFactory factory)
		{
			var builder = new StringCollectionX();

			using (var certificateManager = ObjectFactory.New<Integration.Customs.AU.ICertificateManager>(factory))
			{
				CheckCertificate(builder, certificateManager.CustomsCertificateData, (NoResString)"Customs Encryption Certificate");
				CheckCertificate(builder, certificateManager.TrustPointCertificateData, (NoResString)"Trust Point Certificate");
				if (CheckCertificate(builder, certificateManager.CompanyCertificateData, (NoResString)"Company Private Key File"))
				{
					CheckPassword(builder, certificateManager.CompanyCertificatePassword, (NoResString)"Company Private Key File");
				}
			}

			return builder.ToArray();
		}

		bool CheckCertificate(StringCollectionX builder, byte[] certificateBytes, string certificateName)
		{
			if (certificateBytes == null || certificateBytes.Length == 0)
			{
				builder.Add($"There is no {certificateName}.");
				return false;
			}
			return true;
		}

		void CheckPassword(StringCollectionX builder, string password, string certificateName)
		{
			if (password == null || password.Length == 0)
			{
				builder.Add($"There is no password for the {certificateName}.");
			}
		}

		#endregion
	}
}
