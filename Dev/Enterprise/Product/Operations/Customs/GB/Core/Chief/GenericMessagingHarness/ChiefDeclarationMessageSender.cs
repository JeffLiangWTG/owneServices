using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.GB.Chief
{
	// NB for full history of this file, which was split off from C:\dev\Enterprise\Product\Operations\Customs\GB\Core\Business\GbDeclarationMessageSender.cs, see that file's history

	public abstract class ChiefDeclarationMessageSender : GbDeclarationMessageSender
	{
		protected virtual bool IsNewOrAmended(CusdecMessageFunction functionNewDeletedAmended) => functionNewDeletedAmended is CusdecMessageFunction.New || functionNewDeletedAmended is CusdecMessageFunction.Amended;

		protected virtual bool IsDeletedOrAmended(CusdecMessageFunction functionNewDeletedAmended) => functionNewDeletedAmended is CusdecMessageFunction.Deleted || functionNewDeletedAmended is CusdecMessageFunction.Amended;

		protected virtual bool ValidateForMessageType(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction functionNewDeletedAmended, MessageSendingNotificationCollection existingNotifications)
		{
			bool canSend = false;

			if (IsNewOrAmended(functionNewDeletedAmended))
			{
				// New or amended - validate fully
				canSend = ValidateFullyForNewOrAmended(declaration, sendMessagesToCustoms, true, existingNotifications);
			}
			else if (IsDeletedOrAmended(functionNewDeletedAmended))
			{
				// Delete or amend needs remarks
				canSend = ValidateForAmendmentOrDeletion(declaration, sendMessagesToCustoms, functionNewDeletedAmended);
			}
			else if (functionNewDeletedAmended is GbDes242MessageFunction)
			{
				canSend = ValidateMucrFunctions(declaration, sendMessagesToCustoms, functionNewDeletedAmended);
			}
			else if (functionNewDeletedAmended is QueryMessageFunction)
			{
				canSend = ValidateInterrogationFunctions(declaration, sendMessagesToCustoms, functionNewDeletedAmended);
			}

			return canSend;
		}

		protected override bool ValidateAndShowUserAnyWarningsOrErrors(IBusiness bizO, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction functionNewDeletedAmended)
		{
			var declaration = bizO as JobDeclaration;
			bool canSend = false;

			BusinessObject boToValidate;
			if (declaration.Shipment != null)
			{
				boToValidate = declaration.Shipment;
			}
			else
			{
				boToValidate = declaration;
			}

			var notifications = ExecuteValidatingUsingMessageSendingValidationAndReturnNotifications(boToValidate);
			if (notifications != null && notifications.ContainsError())
			{
				sendMessagesToCustoms.WarnUserAboutSomething(notifications.NotificationsAsString(), "Error");
				return canSend;
			}

			canSend = CheckTheFunctionCodeIsValidOrNotForSending(declaration.ApplicationExtender is ChiefApplicationExtender, sendMessagesToCustoms, functionNewDeletedAmended, declaration);

			if (canSend)
			{
				canSend = ValidateForMessageType(declaration, sendMessagesToCustoms, functionNewDeletedAmended, notifications);
			}

			if (canSend)
			{
				canSend = CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(declaration, sendMessagesToCustoms);
			}
			if (canSend)
			{
				canSend = CheckCcsukAirWaybillIsAcceptable(declaration, sendMessagesToCustoms);
			}
			if (canSend)
			{
				canSend = CheckCcsukFallbackIsInOperation(declaration, sendMessagesToCustoms);
			}
			return canSend;
		}

		public static MessageSendingNotificationCollection ExecuteValidatingUsingMessageSendingValidationAndReturnNotifications(BusinessObject boToValidate)
		{
			return MessageSendingValidation.New(boToValidate, null).CheckBusinessObjectLevelValidation();
		}

		bool IsExportInventoryMessages(CusdecMessageFunction how)
		{
			return how is GbDes242MessageFunction.MucrAssociate
				|| how is GbDes242MessageFunction.MucrDisAssociate
				|| how is GbDes242MessageFunction.MucrClose
				|| how is GbInventoryManagementMessageFunction.ArrivalActual
				|| how is GbInventoryManagementMessageFunction.ArrivalAnticipated
				|| how is GbInventoryManagementMessageFunction.Departure
				|| how is GbDes242MessageFunction.QueryMasterDEC;
		}

		protected bool CheckTheFunctionCodeIsValidOrNotForSending(bool isChiefMessage, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction how, JobDeclaration declaration = null)
		{
			if (isChiefMessage)
			{
				var isExportInventoryMessages = IsExportInventoryMessages(how);

				if (isExportInventoryMessages)
				{
					var isDualRunValid = Universal.ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.CHIEF_CDS_EXPORT_DUAL_RUN, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today);
					if (isDualRunValid)
					{
						if (!sendMessagesToCustoms.ShowUserConfirmation(exportDualRunWarning, "Ignore or not", "Please type the following to continue: ", "yes"))
						{
							return false;
						}
					}
				}

				var sunSetCodeList = GetSunset(declaration);
				var isSunset = (sunSetCodeList?.ZZD_StartDate ?? ZDateTime.MaxSmallDateTime) < ZDateTime.Today;
				var importOrExports = declaration?.IsImport() ?? ZBool.False ? "imports" : "exports";

				if (isSunset)
				{
					if (!sendMessagesToCustoms.ShowUserConfirmation(string.Format(sunsetWarning, importOrExports), "Ignore or not", "Please type the following to continue: ", string.Format(sunsetWarningOverride, importOrExports)))
					{
						return false;
					}
				}
				else if (sunSetCodeList != null)
				{
					var warningFactor = ZDecimal.ParseSafe(sunSetCodeList.GetAttribute(AttributeNames.Codes.WarningFactor), ZDecimal.Zero);

					if (warningFactor > ZDecimal.Zero)
					{
						var noOfWeeksTilSunset = Math.Floor((sunSetCodeList.ZZD_StartDate - ZDateTime.Now).Days / 7.0m) + 1;
						var nagGraceNumber = Math.Floor(noOfWeeksTilSunset * warningFactor);

						if (nagGraceNumber == 0)
						{
							return NagUserRegardingChiefSunset(sendMessagesToCustoms, importOrExports, sunSetCodeList.ZZD_StartDate);
						}
						else
						{
							var messageCountThisSession = (declaration?.IsImport ?? ZBool.False) ? JobDeclarationMessageManagerFrontEnd.CountOfImportDeclarationsThisSession
																: JobDeclarationMessageManagerFrontEnd.CountOfExportDeclarationsThisSession;

							if (messageCountThisSession % nagGraceNumber == 0)
							{
								return NagUserRegardingChiefSunset(sendMessagesToCustoms, importOrExports, sunSetCodeList.ZZD_StartDate);
							}
						}
					}
				}
			}

			return true;
		}

		bool NagUserRegardingChiefSunset(ISendsMessagesToCustoms sender, ZString importOrExports, ZDateTime date) => sender.YesNoQuery(string.Format(sunsetNagWarning, importOrExports, date.ToString("dd/MM/yyyy")), "Send or not");

		ZZRefCusCodeListCombined GetSunset(JobDeclaration declaration)
		{
			var factory = new BusinessObjectFactory();
			var code = (declaration?.IsExport ?? ZBool.False) ? FunctionalityTypes.CHIEF_SUNSET_EXP : (declaration?.IsImport ?? ZBool.False) ? FunctionalityTypes.CHIEF_SUNSET_IMP : string.Empty;

			var refCusCodeList = ZZRefCusCodeListCombined.Loader.Load(factory,
														Core.Constants.CountryCodes.UnitedKingdom,
														Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS,
														ZDateTime.MaxSmallDateTime.AddDays(-1), ZDateTime.MinSmallDateTimeValue).FirstOrDefault(x => x.ZZD_Code == code) ?? ZZRefCusCodeListCombined.Loader.Load(factory,
														Core.Constants.CountryCodes.UnitedKingdom,
														Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC,
														ZDateTime.MaxSmallDateTime.AddDays(-1), ZDateTime.MinSmallDateTimeValue).FirstOrDefault(x => x.ZZD_Code == code);

			return refCusCodeList;
		}

		const string exportDualRunWarning = "Dual running for CDS/CHIEF export inventory is in place.  You are advised to send this message only to CDS and not to CHIEF.  CDS will send a copy to CHIEF to ensure both systems are synchronised.\r\nDo you want to ignore this advice and send to CHIEF anyway?";
		const string sunsetWarning = "CHIEF for {0} is now obsolete.  You are advised to send this message only to CDS and not to CHIEF.\r\nDo you want to ignore this advice and send to CHIEF anyway?";
		const string sunsetWarningOverride = "CHIEF for {0} is obsolete";

		const string sunsetNagWarning = "CHIEF for {0} is due to be retired on {1}.\r\nPlease migrate to CDS as soon as possible.  Would you like to send to CHIEF now?";

		protected bool CheckCcsukFallbackIsInOperation(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			if (declaration.ZG_Gateway == GatewayList.Codes.CCSUKviaNTMsgGW
					&&
					((declaration.IsExport && GBCustomsDataRegistry.Instance.ChiefFallbackExports.Value)
						||
						(declaration.IsImport && GBCustomsDataRegistry.Instance.ChiefFallbackImports.Value)
					)
				)
			{
				return sendMessagesToCustoms.YesNoQuery("CCS-UK electronic fallback is in effect. You may receive no reply from CHIEF until revocation and the job may remain in 'awaiting response' status until then.\r\nIt is also advised to ensure that a header-level Additional Information statement (e.g. FBK01/2/3) has been added where relevant.\r\nProceed?", "Fallback");
			}
			return true;
		}

		protected bool CheckCcsukAirWaybillIsAcceptable(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			var isValid = true;
			if (declaration.IsInventoryControlledAirImport
				&& declaration.ZG_Gateway == GatewayList.Codes.CCSUKviaNTMsgGW
				&& declaration.IsInventoryControlledAirImport)
			{
				var matches = MasterUCRHelper.PreparePatternMatch(declaration.JE_MasterUCR);
				if (matches != null && matches.Count > 0)
				{
					var iCcsukCusAwbBase = MasterUCRHelper.FindMawbFromPatternMatch(matches, declaration.Factory);
					if (iCcsukCusAwbBase != null)
					{
						var optionalHawbFromMucr = MasterUCRHelper.GetMatchedCode(matches, "HAWB");
						if (!optionalHawbFromMucr.IsEmpty)
						{
							iCcsukCusAwbBase = MasterUCRHelper.FindHawbFromPattern(optionalHawbFromMucr, iCcsukCusAwbBase.PK, declaration.Factory);
						}
					}

					if (iCcsukCusAwbBase != null)
					{
						var warningsStringBuilder = new ZStringBuilder();
						if (iCcsukCusAwbBase.DescriptionOfGoods.Contains("CONSOL", StringComparison.OrdinalIgnoreCase))
						{
							isValid = false;
							// Requirement demanded by Navinder Johal (LocalCOMP CCG CITEX) (navinder.johal@hmrc.gsi.gov.uk), 29/11/2011.
							warningsStringBuilder.Append("This declaration has a CCSUK house or basic bill, but the bill's description of goods is not acceptable. 'Consol' is not allowed. Edit the bill's description before sending to CHIEF. This is a requirement from HMRC.");
						}
						if (iCcsukCusAwbBase.ShipmentDescriptionCode == "M" && iCcsukCusAwbBase.Status1Date.IsEmpty && !iCcsukCusAwbBase.HasSplits)
						{
							isValid = false;
							warningsStringBuilder.Append("This declaration has a CCSUK house or basic bill, but the bill's Shipment Description Code is M and Status 1 is unset and no splits exist. An entry is not permitted.");
						}

						if (!isValid)
						{
							sendMessagesToCustoms.WarnUserAboutSomething(warningsStringBuilder.ToStringWithNewLineBetweenAppends(), "Error");
						}
					}
				}
			}
			return isValid;
		}

		bool ValidateInventoryFunctions(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction how)
		{
			if (declaration.JE_LocationOfGoods.IsEmpty)
			{
				sendMessagesToCustoms.WarnUserAboutSomething("Location of goods is required for arrival/departure message", "Error");
				return false;
			}
			GbInventoryManagementMessageFunction howAsInventory = how as GbInventoryManagementMessageFunction;
			if (howAsInventory.Level == GbInventoryManagementMessageFunction.MasterOrDeclaration.Master && declaration.JE_MasterUCR.IsEmpty)
			{
				sendMessagesToCustoms.WarnUserAboutSomething("Master UCR is required for a master-level message", "Error");
				return false;
			}
			return true;
		}

		public static bool ValidateFullyForNewOrAmended(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms, bool doCreditCheck, MessageSendingNotificationCollection existingNotifications)
		{
			bool canSend = false;

			if (!existingNotifications.ContainsWarning() || sendMessagesToCustoms.AskUserToContinueWithAction(existingNotifications.NotificationsAsString(), "Warning", declaration))
			{
				canSend = true;
			}

			if (canSend && doCreditCheck)
			{
				canSend = new GbMessageManagerCreditCheckWithSecurityHelper(declaration, sendMessagesToCustoms).WarnAboutCreditChecks();
			}

			return canSend;
		}

		protected bool ValidateForAmendmentOrDeletion(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction functionNewDeletedAmended)
		{
			bool canSend = true;
			if (declaration.CustomsEntryHeaders
				.Cast<Business.Declaration.CusEntryHeader>()
				.Where(x => !x.DoNotSendMessageForThisEntryBecauseSendingForIndividualEntries)
				.AnyEntryHeadersAreMissingAmendmentRemarksAndThisIsARequestToCancel(functionNewDeletedAmended))
			{
				sendMessagesToCustoms.WarnUserAboutSomething(missingReasonWarning
				, "A reason for amendment/cancellation is needed");
				canSend = false;
			}
			return canSend;
		}

		bool ValidateInterrogationFunctions(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction functionNewDeletedAmended)
		{
			QueryMessageFunction qmf = functionNewDeletedAmended as QueryMessageFunction;
			if (qmf == null)
			{
				return true;
			}

			if (qmf.SubFunction == QueryMessageFunction.LevelsOfMessage.MasterLevel)
			{
				if (declaration.JE_MasterUCR.IsEmpty)
				{
					sendMessagesToCustoms.NotifyUserOfAnInvalidOperation("Message requires a MUCR.  Please supply one, or choose the DUCR version.");
					return false;
				}
				return true;
			}
			else
			{
				if (declaration.JE_UCR.IsEmpty)
				{
					sendMessagesToCustoms.NotifyUserOfAnInvalidOperation("Message requires a DUCR.  Please save this job first.");
					return false;
				}
				return true;
			}
		}

		bool ValidateMucrFunctions(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction functionNewDeletedAmended)
		{
			// MUCR and interrogation functions...
			bool canSend = true;
			if (functionNewDeletedAmended is GbInventoryManagementMessageFunction)
			{
				return ValidateInventoryFunctions(declaration, sendMessagesToCustoms, functionNewDeletedAmended);
			}
			else if (functionNewDeletedAmended is GbDes242MessageFunction.MucrAssociate || functionNewDeletedAmended is GbDes242MessageFunction.MucrClose)
			{
				if (declaration.JE_MasterUCR.IsEmpty)
				{
					canSend = false;
					sendMessagesToCustoms.NotifyUserOfAnInvalidOperation("Closing or associating requires a MUCR. Please enter a MUCR in the box, or for badges with automatic calculation enabled supply the requisite components (e.g. Mawb and Hawb)");
				}
			}
			return canSend;
		}

		public const string missingReasonWarning =
@"Please supply a reason for the cancellation for each entry header. Without reason(s), you will see the following errors:
	E2553	COMMIT IS NOT POSSIBLE WHILST REASON FOR ACTION MISSING
	E1521	AT LEAST ONE LINE MUST BE ENTERED
You can supply reasons for amendment/cancellation on the Entries tab in the top right-hand corner.";
	}
}
