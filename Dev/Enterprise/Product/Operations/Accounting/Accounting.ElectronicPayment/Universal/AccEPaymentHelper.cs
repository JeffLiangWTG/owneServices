using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicPayment.Universal
{
	internal class AccEPaymentHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		const string invalidRefreshTokenErrorMesage = "There is no active refresh token.";

		internal static ZString GetErrorDescription(ZString errorMessage, int maxlength)
		{
			ZString updatedErrorMesage;
			if (errorMessage.StartsWith(invalidRefreshTokenErrorMesage))
			{
				updatedErrorMesage = Res.GetString("3204B448-EE65-40B6-82F6-A6D1F6290E16", "Please retry your last action or re-authorize your OFX user account from your Bank Account before proceeding.");
			}
			else
			{
				updatedErrorMesage = errorMessage.Length > maxlength ? errorMessage.Substring(0, maxlength) : errorMessage;
			}
			return updatedErrorMesage;
		}

		internal static void InvalidateStaffTokenIfRefreshTokenInvalid(ZString errorMessage, ZString creatingUser, ZGuid companyPK, IXmlSessionTracker logger)
		{
			if (errorMessage.StartsWith(invalidRefreshTokenErrorMesage))
			{
				var newFactory = new BusinessObjectFactory();

				var staffTokenQuery = new ZQuery(AccEPaymentStaffTokenSchema.TK_GS_NKStaffCode, creatingUser);
				staffTokenQuery.AddToFilter(new ZQuery(AccEPaymentStaffTokenSchema.TK_GC, companyPK));
				var matchingStaffTokens = newFactory.Load<AccEPaymentStaffToken>(staffTokenQuery);
				if (matchingStaffTokens.Any())
				{
					var staffToken = matchingStaffTokens.Cast<AccEPaymentStaffToken>().FirstOrDefault(t => t.BankAccount.AB_PaymentProvider == EPaymentProviderCodes.Codes.OFX);
					if (staffToken != null)
					{
						staffToken.TK_Status = AccEPaymentStaffTokenLookups.StatusCodes.Error;
						staffToken.TK_ExpiryUtc = ZDateTime.Empty;
						try
						{
							newFactory.Save();
						}
						catch (ZCannotSaveException ex)
						{
							logger.LogBoth(Integration.LogType.Error, Res.GetString("CCA120FB-2E2A-4742-BB6A-BED256561536", "Error during save: {0}", ex.Message));
						}
					}
				}
			}
		}
	}
}
