using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.HoldAdderRemoverClearer;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.GB.Business
{
	public static class GbExtensionHelpers
	{
		public static void ApplyOrRemoveHoldOrClear(this CusEntryHeader entryHeader, IPortAuthorityHoldApplicationProvider holdProvider)
		{
			if (holdProvider.DirectionOfApplication == AddOrRemove.HoldAdd)
			{
				UpdateEntryHeaderToHoldAdd(entryHeader, holdProvider, true);
			}
			else if (holdProvider.DirectionOfApplication == AddOrRemove.HoldRemove)
			{
				UpdateEntryHeaderToHoldRemoved(entryHeader, holdProvider);
			}
			else if (holdProvider.DirectionOfApplication == AddOrRemove.Cleared)
			{
				UpdateEntryHeaderToCleared(entryHeader, holdProvider.Date);
			}
		}

		static void UpdateEntryHeaderToHoldRemoved(CusEntryHeader entryHeader, IPortAuthorityHoldApplicationProvider holdProvider)
		{
			var dateOfAction = holdProvider.Date;  // No date field in message, so we must use this.
			entryHeader.CH_EntryReleaseDate = dateOfAction;
			entryHeader.Logs.AddNew(Events.CustomsEntryStatus, "HoldRemoved-" + holdProvider.HoldType, dateOfAction.ToOffset());
		}

		public static void UpdateEntryHeaderToHoldAdd(CusEntryHeader entryHeader, IPortAuthorityHoldApplicationProvider holdProvider, bool logEvent)
		{
			// HMRC have cleared but another party has held
			if (logEvent)
			{
				entryHeader.Logs.AddNew(Events.CustomsEntryStatus, "Hold-" + holdProvider.HoldType, holdProvider.Date.ToOffset());
			}
			entryHeader.CH_EntryStatus = EntryStatusList.Codes.Hold;
			entryHeader.CH_Status = MessageStatusList.Codes.OK;
		}

		public static void UpdateEntryHeaderToCleared(CusEntryHeader entryHeader, ZDateTime time)
		{
			using (DisposableEnvironment.ForBranch(entryHeader.Branch.PK.ToGuid()))
			{
				if (entryHeader.CH_EntryStatus != EntryStatusList.Codes.Clear)
				{
					entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
					entryHeader.CH_Status = MessageStatusList.Codes.OK;
					entryHeader.CH_EntryReleaseDate = time;

					if (entryHeader.Declaration.JE_ApplicationCode != Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services)
					{
						SendQueriesForCleared(entryHeader);
					}
				}
			}
		}

		public static void SendQueriesForCleared(CusEntryHeader entryHeader)
		{
			var sender = new GenericImmediateEntryMessageSender(entryHeader, string.Empty, 0);
			var sendsToCustoms = new SendsMessagesToCustomsShutterUpperer(false);

			if (GBCustomsDataRegistry.Instance.SendDevQueryForNonInventoryImportsUponAcceptance.Value)
			{
				sender.Send(entryHeader.Declaration, sendsToCustoms, new Interrogate_DevDucr());
			}

			if (GBCustomsDataRegistry.Instance.SendDesQueryForNonInventoryImportsUponAcceptance.Value)
			{
				sender.Send(entryHeader.Declaration, sendsToCustoms, new Interrogate_Des());
			}
		}

		public static CusEntryHeader GetEntryHeaderFromUcnUsingMucr(this IUcnProvider ucnProvider, EDIMessage ediMessage)
		{
			var query = GetParentQueryFromUcnUsingMucr<CusEntryHeader>(ucnProvider);
			return ediMessage.Factory.LoadTop1<CusEntryHeader>(query);
		}

		public static ZQuery GetParentQueryFromUcnUsingMucr<T>(this IUcnProvider ucnProvider)
		{
			ZDBOnlySubQuery entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.EU.MasterUCR);
			if (GBCustomsDataRegistry.Instance.AllowMatchingOfInboundUcnToJobsMucrVerbatimWithoutTruncating.Value)
			{
				ZQuery twoStylesOfUcnQuery = new ZQuery();
				twoStylesOfUcnQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, ucnProvider.UcnNumberProperlyTruncated);
				twoStylesOfUcnQuery.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryNum, ucnProvider.UcnNumberVerbatim);
				entryNumberQuery.AddToFilter(twoStylesOfUcnQuery);
			}
			else
			{
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, ucnProvider.UcnNumberProperlyTruncated);
			}
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);
			var query = new ZDBOnlyQuery(typeof(T));
			query.AddSubQuery(entryNumberQuery, JoinCondition.And);

			return query;
		}

		public static CusEntryHeader[] GetCusEntryHeaderFromCusEntryNumberAndDate(ZString entryNumber, BusinessObjectFactory fact, ZDateTime chiefEntryDateNoTime, string entryTypesToExclude = "MUC")
		{
			// Try with raw entry number:
			ZDBOnlySubQuery entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);
			if (chiefEntryDateNoTime != ZDateTime.Empty && chiefEntryDateNoTime.IsValidSqlDateTime)
			{
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.EqualToDatePartOnly, chiefEntryDateNoTime);
			}
			ZQuery entryNumberRawQuery = new ZQuery(CusEntryNumSchema.CE_EntryNum, entryNumber);
			ZQuery entryNumberCleanQuery = new ZQuery(CusEntryNumSchema.CE_EntryNum, entryNumber.Replace(" ", "-"));

			ZQuery entryNumberBracketedQuery = new ZQuery();
			entryNumberBracketedQuery.AddToFilter(entryNumberCleanQuery);
			entryNumberBracketedQuery.AddToFilter(entryNumberRawQuery, JoinCondition.Or);
			entryNumberQuery.AddToFilter(entryNumberBracketedQuery);
			if (!String.IsNullOrEmpty(entryTypesToExclude))
			{
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.NotEqual, entryTypesToExclude);
			}

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			query.AddSubQuery(entryNumberQuery, JoinCondition.And);
			query.OrderBy = CusEntryHeaderSchema.CH_EntrySubmittedDate.Name + " DESC";
			return fact.Load<CusEntryHeader>(query);
		}

		public static bool IsInboundErrorToBeExcludedFromAbnormalityReporterCosItsErrorWasImpossibleToDetermineInAdvance(this EDIMessage responseMessage)
		{
			// Some solicited error messages are utterly impossible for our validation to determine, or are impracticale for us to determine.  These errors, despite being negative, should not fire our friend the abnormality reporter. 
			List<string> errorMessageToNotReport = new List<string>();
			errorMessageToNotReport.Add("Unlicensed usage of Data Interface");  // Gems thinks that the badge is unlicenced. Client has not installed licence key for the badge. 
			errorMessageToNotReport.Add("Unable to locate Inventory Record for Data Interface");  // Gems looked up the MUCR on CCSUK and found none. Unless we check against the CCSUK inventory ourselves this cannot be checked. 
			errorMessageToNotReport.Add("CHECK THE CNS FORM FOR FURTHER DETAILS"); // Gems' CNS gateway is down. 
			errorMessageToNotReport.Add("invalid transaction type"); // Gems
			errorMessageToNotReport.Add("invalid UVI"); // Destin8 doesn't like the MUCR - the docking/landing part doesn't tally with one on their books.
			foreach (string oneError in errorMessageToNotReport)
			{
				if (responseMessage.EM_MessageText.ToString().IndexOf(oneError, StringComparison.InvariantCultureIgnoreCase) > 0)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// If the response message is a response to a simple outbound message, not a full blown cusdec, we should not give a monkey's about "cleared when send with error"
		/// </summary>
		public static bool IsResponseToSimpleOutboundMessageForWhichValidationIsNotImportant(this EDIMessage responseMessage)
		{
			List<string> messageTypesThatAresimple = new List<string>();
			messageTypesThatAresimple.Add("UKCINVEACD");
			messageTypesThatAresimple.Add("UKCINVEACA");
			messageTypesThatAresimple.Add("UKCINVEACC");
			messageTypesThatAresimple.Add("UKCINVEAL");
			foreach (string oneError in messageTypesThatAresimple)
			{
				if (responseMessage.EM_MessageText.ToString().IndexOf(oneError, StringComparison.InvariantCultureIgnoreCase) > 0)
				{
					return true;
				}
			}
			return false;
		}

		public static bool AnyEntryHeadersAreMissingAmendmentRemarksAndThisIsARequestToCancel(this IEnumerable<CusEntryHeader> entries, CusdecMessageFunction declarationMessageFunction)
		{
			if (declarationMessageFunction is CusdecMessageFunction.New
				|| declarationMessageFunction is GbDes242MessageFunction.MucrAssociate
				|| declarationMessageFunction is GbDes242MessageFunction.MucrClose
				|| declarationMessageFunction is GbDes242MessageFunction.MucrDisAssociate
				)
			{
				return false;  // Not applicable for NEW messages or MUCR functions
			}
			foreach (CusEntryHeader header in entries)
			{
				if (header.CH_CustomsMessageRemarks.IsEmpty)
				{ return true; }
			}
			return false;
		}

		public static ZString MakeUniqueInterchangeNumber(this ZString original, int maxLengthIfNotInterchangeNumMaxLength = EDIInterchange.Schema.EI_InterchangeNumMaxLength)
		{
			var ticks = ZDateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture).TrimEnd('0');
			var items = new string[] { original, ticks, ZGuid.NewZGuid().ToString() };
			var result = (ZString)String.Join("_", items.Where(s => !String.IsNullOrEmpty(s)));
			return result.Left(maxLengthIfNotInterchangeNumMaxLength);
		}

		public static bool IsAnyDefenceMeasureApplicable(this TariffView tariff, string dataGrouping = CountryCodes.UnitedKingdom)
		{
			var conditionTypes = new ZString[] { "551", "552", "553", "554", "652", "654", "695", "696" };
			return tariff.Conditions.Any(c => c.ConditionClass == RefCusConditionTypes.ConditionClass.Rate && conditionTypes.Contains(c.ConditionType) && c.CusConditionType.ZX2_ZZZ_NKDataGrouping == dataGrouping);
		}
	}

	public interface IUcnProvider
	{
		ZString UcnNumberProperlyTruncated { get; }
		ZString UcnNumberVerbatim { get; }
	}

	public interface IEntryNumberProvider
	{
		ZString EntryNumber { get; }
		ZDateTime EntryDate { get; }
	}
}
