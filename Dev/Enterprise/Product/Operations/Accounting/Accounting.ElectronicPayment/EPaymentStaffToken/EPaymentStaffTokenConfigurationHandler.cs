using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.XmlCredential;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicPayment.EPaymentStaffToken
{
	public class EPaymentStaffTokenConfigurationHandler : XmlCredentialConfigurationHandler
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Field Name")]
		const string ProviderFieldName = "Provider";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Field Name")]
		const string ScopeFieldName = "Scope";
		const string RefreshExpiryFieldName = "RefreshExpiry";
		const string BankAccountFieldName = "BankAccount";
		const string ErrorDescriptionFieldName = "ErrorDescription";

		protected EPaymentStaffTokenConfigurationHandler(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool ProcessBranchOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group branchGroup, ItemData branchItems, Group group, ItemData itemData)
		{
			return true;
		}

		protected override bool ProcessCompanyOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group otherGroup, ItemData otherItems)
		{
			return true;
		}

		protected override bool ProcessGroupOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group groupGroup, ItemData groupItems, Group otherGroup, ItemData otherItems)
		{
			return true;
		}

		protected override bool ProcessStaffOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group groupGroup, ItemData groupItems, Group staffGroup, ItemData staffItems, Group otherGroup, ItemData otherItems)
		{
			return true;
		}

		protected override bool ProcessStaffLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group groupGroup, ItemData groupItems, Group staffGroup, ItemData staffItems)
		{
			var result = true;
			var cw1CompanyCode = companyGroup.Reference;
			var cw1StaffCode = staffGroup.Reference;
			var provider = staffItems.Items.FirstOrDefault(x => x.Name.Equals(ProviderFieldName))?.Value;
			var scope = staffItems.Items.FirstOrDefault(x => x.Name.Equals(ScopeFieldName))?.Value;
			var refreshTokenExpiry = staffItems.Items.FirstOrDefault(x => x.Name.Equals(RefreshExpiryFieldName))?.Value;
			var bankAccount = staffItems.Items.FirstOrDefault(x => x.Name.Equals(BankAccountFieldName))?.Value;
			var ofxAccountName = staffItems.Credentials.FirstOrDefault()?.UserName;
			var errorDescription = staffItems.Items.FirstOrDefault(x => x.Name.Equals(ErrorDescriptionFieldName))?.Value;
			if (cw1CompanyCode.IsEmpty)
			{
				logger.LogError(Res.GetString("DCFC71D4-9077-42DA-989C-F64774EA96A6", "Company code is missing."));
				result = false;
			}
			else if (cw1StaffCode.IsEmpty)
			{
				logger.LogError(Res.GetString("913B02BC-A5E1-486A-ADC2-535AC0CF7386", "Staff code is missing."));
				result = false;
			}
			else if (!scope.HasValue || scope.Value.IsEmpty)
			{
				logger.LogError(Res.GetString("BDAFC291-4AD4-44DC-8D60-1F7E71FB2972", "Token scope is missing."));
				result = false;
			}
			else if (!ofxAccountName.HasValue || ofxAccountName.Value.IsEmpty)
			{
				logger.LogError(Res.GetString("499E2833-644C-4502-B570-451B7849EE2D", "Payment provider account name is missing."));
				result = false;
			}
			else if (!refreshTokenExpiry.HasValue || refreshTokenExpiry.Value.IsEmpty)
			{
				logger.LogError(Res.GetString("638E1672-91C9-4535-BFD5-7E6F1A27DDFB", "Refresh token expiry information is missing."));
				result = false;
			}
			else if (!bankAccount.HasValue || bankAccount.Value.IsEmpty)
			{
				logger.LogError(Res.GetString("8FB9E2D1-0DCC-4168-99F1-027DB38FD6E2", "Bank account information is missing."));
				result = false;
			}
			else
			{
				var newFactory = new BusinessObjectFactory();
				var tokenCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, cw1CompanyCode));
				var bankAcct = Factory.LoadTop1<AccBankAccount>(new ZQuery(AccBankAccountSchema.AB_Code, bankAccount));
				if (tokenCompany == null)
				{
					logger.LogError(Res.GetString("4067C9CE-08AE-4DBD-80F6-141E7B20F856", "Failed to find company '{0}'.", cw1CompanyCode));
					result = false;
				}
				else if (bankAcct == null)
				{
					logger.LogError(Res.GetString("60712174-C093-4E54-AEB8-A45879F51AFC", "Failed to find bank account '{0}'.", bankAccount));
					result = false;
				}
				else
				{
					var query = new ZQuery(AccEPaymentStaffTokenSchema.TK_GS_NKStaffCode, cw1StaffCode);
					query.AddToFilter(AccEPaymentStaffTokenSchema.TK_AB, bankAcct.PK);
					query.AddToFilter(AccEPaymentStaffTokenSchema.TK_Scope, scope);
					query.AddToFilter(AccEPaymentStaffTokenSchema.TK_GC, tokenCompany.PK);
					var matchedRecordInDB = newFactory.Load<AccEPaymentStaffToken>(query);
					if (matchedRecordInDB.Length == 1)
					{
						var tokenExpiryDateTime = new ZDateTime(refreshTokenExpiry.Value);
						var staffToken = matchedRecordInDB[0];
						if (staffToken.TK_RequestedUtc > tokenExpiryDateTime)
						{
							logger.LogError(Res.GetString("B13D9A72-E7EA-4E75-AB14-A78480F4F233", "Token's expiry is earlier than the requested date/time."));
							result = false;
						}
						else if (!staffToken.TK_AccountName.IsEmpty && staffToken.TK_AccountName != ofxAccountName.Value)
						{
							// detect error that the bank account was authorized with a different ofx user before
							logger.LogError(Res.GetString("55D9AB9E-D21C-494B-856E-12E6ADEA3080", "Provider authorization was done with a different user account before."));
							result = false;
						}
						else
						{
							var errorDetail = ZString.Empty;
							if (errorDescription.HasValue)
							{
								errorDetail = errorDescription.Value;
								staffToken.TK_Status = AccEPaymentStaffTokenLookups.StatusCodes.Error;
								staffToken.TK_ExpiryUtc = ZDateTime.Empty;
							}
							else
							{
								staffToken.TK_AccountName = ofxAccountName.Value;
								staffToken.TK_Status = AccEPaymentStaffTokenLookups.StatusCodes.Authorised;
								staffToken.TK_ExpiryUtc = tokenExpiryDateTime;
							}
							staffToken.TK_ErrorDescription = errorDetail;
							try
							{
								newFactory.Save();
								logger.Log(Res.GetString("DC84F65D-035A-44F6-AAE8-D614339C31F9", "Payment staff token saved successfully"));
							}
							catch (ZCannotSaveException ex)
							{
								logger.LogError(Res.GetString("CCA120FB-2E2A-4742-BB6A-BED256561536", "Error during save: {0}", ex.Message));
								result = false;
							}
						}
					}
					else
					{
						//report error - either nothing found or multiple records found
						logger.LogError(Res.GetString("9C008A18-B06C-4631-9AE6-C282D5E882FD", "Found {0} matched staff token record(s), however it must only be 1.", matchedRecordInDB.Length));
					}
				}
			}
			return result;
		}

		protected override bool ProcessSystemOtherLevel(Configuration configuration, Group systemGroup, ItemData items, Group otherGroup, ItemData otherItems)
		{
			return true;
		}
	}
}
