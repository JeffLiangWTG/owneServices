using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.ServiceTasks
{
	public class CcsukMaintenanceServiceTaskRunner_Archiver
	{
		public CcsukMaintenanceServiceTaskRunner_Archiver(Integration.ILogger serviceLogger)
		{
			this.serviceLogger = serviceLogger;
		}

		public void ArchiveOldRecordsAndSendReports(CancellationToken token)
		{
			var factory = new BusinessObjectFactory();
			var gbCompanies = LoadGBCompanies(factory);

			foreach (var company in gbCompanies)
			{
				token.ThrowIfCancellationRequested();

				var aBranchPK = company.Branches.Where(x => x.GB_IsActive).Select(x => x.PK).FirstOrDefault();
				if (!aBranchPK.IsValid)
				{
					serviceLogger.Log(Integration.LogType.Warning, $"Cannot execute, there are no active UK branches under company {company.GC_Code}");
				}
				else
				{
					using (DisposableEnvironment.ForBranch(aBranchPK.ToGuid()))
					{
						reportOfWhatIsarchived = new Dictionary<ICcsukCusAwb, ReasonForArchiving>();
						warningReportOfWhatIsNotArchived = new Dictionary<ICcsukCusAwb, ReasonForArchiving>();

						var companyPk = company.PK;
						var unarchivedBasics = factory.Load<CusMAWB>(GetAllUnarchivedMastersQuery(companyPk));
						var unarchivedHouses = factory.Load<CusHAWB>(GetAllUnarchivedHawbsQuery(companyPk));
						var unarchivedSplitHouses = factory.Load<SplitHouse>(GetAllUnarchivedSplitHousesQuery(companyPk));
						var unarchivedSplitBasics = factory.Load<SplitBasic>(GetAllUnarchivedSplitBasicQuery(companyPk));

						var allAwbs = new List<ICcsukCusAwb>();
						allAwbs.AddRange(unarchivedHouses);
						allAwbs.AddRange(unarchivedSplitHouses);
						allAwbs.AddRange(unarchivedSplitBasics);
						allAwbs.AddRange(unarchivedBasics);

						UpdateSaveAndSendReport(allAwbs, company, token);
					}
				}
			}
		}

		GlbCompany[] LoadGBCompanies(BusinessObjectFactory factory)
		{
			var query = new ZQuery();
			_ = query.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);

			return factory.Load<GlbCompany>(query);
		}

		void UpdateSaveAndSendReport(IEnumerable<ICcsukCusAwb> unarchivedAwbs, GlbCompany company, CancellationToken token)
		{
			int index = 1;

			var factory = company.Factory;

			foreach (var awb in unarchivedAwbs)
			{
				token.ThrowIfCancellationRequested();
				if (!awb.HasSplits)
				{
					var reasonForAnyArchiving = GetReasonForShouldArchiveNow(awb);
					switch (reasonForAnyArchiving)
					{
						case ReasonForArchiving.Status1DateAndCacDatesBothOlderThanOneWeek:
						case ReasonForArchiving.NprFewerThanNpxAndFinalCustomsActionDateOlderThan180Days:
						case ReasonForArchiving.Status1DateOlderThanAWeekAndEuropeanSDC:
						case ReasonForArchiving.PreArrivalOlderThanFourDays:
						case ReasonForArchiving.ArchiveBecauseOfCustomEventExclusion:
							awb.ArchiveOnCcsuk(reasonForAnyArchiving);
							reportOfWhatIsarchived.Add(awb, reasonForAnyArchiving);
							index++;
							break;

						case ReasonForArchiving.DoNotArchiveBecauseNprGreaterThanNpx:
						case ReasonForArchiving.DoNotArchiveBecauseStatus2NotSetForEtsf:
							warningReportOfWhatIsNotArchived.Add(awb, reasonForAnyArchiving);
							break;

						case ReasonForArchiving.None:  // don't need to finalise yet
							break;

						default:
							throw new NotImplementedException("You have added a new reason for archiving but not added a case for it");
					}
				}

				if (index % 50 == 0)
				{
					factory.Save();
				}
			}

			SendReports(company);

			factory.Save();
		}

		static ReasonForArchiving GetReasonForShouldArchiveNow(ICcsukCusAwb unarchivedAwb)
		{
			var reason = ReasonForArchiving.None;
			var isEuropean = CcsukUtilities.IsEuropeanShipmentDescriptionCode(unarchivedAwb);
			if (
					!isEuropean
					&&
					unarchivedAwb.IsCompleteOnCcsuk
					&&
					DateIsOlderThanNDays(unarchivedAwb.CustomsActionDate, 7)
					&&
					DateIsOlderThanNDays(unarchivedAwb.Status1Date, 7)
				)
			{
				reason = ReasonForArchiving.Status1DateAndCacDatesBothOlderThanOneWeek;
			}
			else if (isEuropean && DateIsOlderThanNDays(unarchivedAwb.Status1Date, 7))
			{
				reason = ReasonForArchiving.Status1DateOlderThanAWeekAndEuropeanSDC;
			}
			else if (unarchivedAwb.NumberOfPiecesReceived < unarchivedAwb.NumberOfPiecesExpected && CusAwbIsReadOnlyHelper.FinalisedCustomsStatusCodes.Contains(unarchivedAwb.CustomsActionCode))
			{
				// If CAC awarded more than 6 months ago and CAC is a final one, but still not all pieces received,
				if (DateIsOlderThanNDays(unarchivedAwb.CustomsActionDate, 180))
				{
					reason = ReasonForArchiving.NprFewerThanNpxAndFinalCustomsActionDateOlderThan180Days;
				}
			}
			else if (unarchivedAwb.NumberOfPiecesReceived > unarchivedAwb.NumberOfPiecesExpected)
			{
				var registryItemValues = GBCustomsDataRegistry.Instance.NPRExclusionEventCodes.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
				reason = registryItemValues.Any() && AwbOrParentHasAnyEvent(unarchivedAwb, registryItemValues)
					? ReasonForArchiving.ArchiveBecauseOfCustomEventExclusion : ReasonForArchiving.DoNotArchiveBecauseNprGreaterThanNpx;
			}
			else if (unarchivedAwb.IsPrearrival && IsOnNetwork(unarchivedAwb) && DateIsOlderThanNDays(unarchivedAwb.LocalCreationDate, 4))
			{
				reason = ReasonForArchiving.PreArrivalOlderThanFourDays;
			}

			if (reason != ReasonForArchiving.DoNotArchiveBecauseNprGreaterThanNpx && !unarchivedAwb.Status2Granted && IsETSFJob(unarchivedAwb))
			{
				reason = ReasonForArchiving.DoNotArchiveBecauseStatus2NotSetForEtsf;
			}
			return reason;
		}

		static bool AwbOrParentHasAnyEvent(ICcsukCusAwb unarchivedAwb, ExcessNPRExclusionEventCodeSettingCollection registryItemValues)
		{
			var awb = unarchivedAwb as EnterpriseBusinessObject;
			var shipmentOrConsol = unarchivedAwb.ForwardingParent;
			var predicate = new Func<EnterpriseBusinessObject, ExcessNPRExclusionEventCodeSettingCollection, bool>(MatchEventCode);
			return (awb != null && predicate(awb, registryItemValues)) || predicate(shipmentOrConsol, registryItemValues);
		}

		static bool MatchEventCode(EnterpriseBusinessObject bizObj, ExcessNPRExclusionEventCodeSettingCollection registryItemValues)
		{
			var logs = bizObj?.Logs?
				.Find(x => registryItemValues.OfType<ExcessNPRExclusionEventCodeSetting>()
				.Any(y => y.EventCode == x.SL_SE_NKEvent));
			return logs != null && logs.Any();
		}

		static bool IsETSFJob(ICcsukCusAwb unarchivedAwb)
		{
			// Agent-only records should not appear on the report because agents can't change S2, and sheds are often unwilling to set it correctly.
			// Only inlcude S2=N records on the report if the records are ETSF record, i.e. the user can change S2.
			return LicenceAndPimaHelper.IsFullShed(unarchivedAwb);
		}

		static bool IsOnNetwork(ICcsukCusAwb unarchivedAwb)
		{
			return unarchivedAwb.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.OnCommDb
						|| unarchivedAwb.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
		}

		static bool DateIsOlderThanNDays(ZDateTime dateToTest, int maxAgeInDays)
		{
			return !dateToTest.IsEmpty && dateToTest.AddDays(maxAgeInDays) <= ZDateTime.Now;
		}

		#region Queries

		static ZQuery GetAllUnarchivedMastersQuery(ZGuid companyPk)
		{
			var result = new ZDBOnlyQuery(typeof(CusMAWB));
			result.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			result.AddSubQuery(GetBranchQuery(companyPk), JoinCondition.And);

			var subForMaster = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.CS_CM, true);
			subForMaster.IsNoLock = true;
			subForMaster.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, false);
			result.AddSubQuery(subForMaster, JoinCondition.And);

			var subForNotArchived = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.CS_CM, false);
			subForNotArchived.IsNoLock = true;
			subForNotArchived.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, true);
			subForNotArchived.AddToFilter(CusHAWBSchema.CS_MsgStatus, SQLComparisonOperator.NotEqual, PresenceOnNetworkList.Codes.ArchivedOnCcsuk);
			result.AddSubQuery(subForNotArchived, JoinCondition.And);

			result.IsNoLock = true;
			return result;
		}

		static ZQuery GetAllUnarchivedHawbsQuery(ZGuid companyPk)
		{
			var query = new ZDBOnlyQuery(typeof(CusHAWB));
			query.AddToFilter(CusHAWBSchema.CS_MsgStatus, SQLComparisonOperator.NotEqual, PresenceOnNetworkList.Codes.ArchivedOnCcsuk);
			query.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, false);
			query.IsNoLock = true;

			var sub = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
			sub.IsNoLock = true;
			sub.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			sub.AddSubQuery(GetBranchQuery(companyPk), JoinCondition.And);

			query.AddSubQuery(sub, JoinCondition.And);
			return query;
		}

		static ZQuery GetAllUnarchivedSplitHousesQuery(ZGuid companyPk)
		{
			var query = new ZDBOnlyQuery(typeof(SplitHouse));
			query.IsNoLock = true;
			query.AddToFilter(CusPartShipSchema.CG_Status, SQLComparisonOperator.NotEqual, PresenceOnNetworkList.Codes.ArchivedOnCcsuk);

			var hawbSub = new ZDBOnlySubQuery(typeof(CusHAWB), CusPartShipSchema.CG_CS);
			hawbSub.IsNoLock = true;
			hawbSub.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, false);

			var mawbHawbSub = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
			mawbHawbSub.IsNoLock = true;
			mawbHawbSub.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			mawbHawbSub.AddSubQuery(GetBranchQuery(companyPk), JoinCondition.And);

			hawbSub.AddSubQuery(mawbHawbSub, JoinCondition.And);
			query.AddSubQuery(hawbSub, JoinCondition.And);
			return query;
		}

		static ZQuery GetAllUnarchivedSplitBasicQuery(ZGuid companyPk)
		{
			var query = new ZDBOnlyQuery(typeof(SplitHouse));
			query.IsNoLock = true;
			query.AddToFilter(CusPartShipSchema.CG_Status, SQLComparisonOperator.NotEqual, PresenceOnNetworkList.Codes.ArchivedOnCcsuk);

			var mawbSub = new ZDBOnlySubQuery(typeof(CusMAWB), CusPartShipSchema.CG_CM_LinkToPartMaster);
			mawbSub.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			mawbSub.AddSubQuery(GetBranchQuery(companyPk), JoinCondition.And);

			mawbSub.IsNoLock = true;
			query.AddSubQuery(mawbSub, JoinCondition.And);
			return query;
		}

		static ZDBOnlySubQuery GetBranchQuery(ZGuid companyPk)
		{
			var query = new ZDBOnlySubQuery(typeof(GlbBranch), CusMAWBSchema.CM_GB);
			query.AddToFilter(GlbBranchSchema.GB_GC, companyPk);

			return query;
		}

		#endregion

		void SendReports(GlbCompany company)
		{
			var stringbuilder = new ZStringBuilder();
			var sortedList = warningReportOfWhatIsNotArchived.Keys.ToList();
			if (sortedList.Count > 0)
			{
				stringbuilder.Append("<h3>AWB Warning Report</h3>");
				stringbuilder.Append(string.Format(CultureInfo.InvariantCulture, "<p>Company: {0}</p>", company.CompanyName));
				stringbuilder.Append("<p>Generated " + ZDateTime.Now.ToLongTimeString() + "</p>");
				stringbuilder.Append("<p>The following AWBs have not been archived.  Your attention is required.</p>");

				var htmlTableWarnings = new HtmlTableCreator(new[] { "AWB Number With Shed", "Reason for Warning", "CAC", "CAC Date", "NPX", "NPR" });
				htmlTableWarnings.EnableHTMLEncoding = false;
				SortByAwbNumber(sortedList);

				foreach (var awb in sortedList)
				{
					var tuple = GetLinkAndExplanation(awb, warningReportOfWhatIsNotArchived[awb]);
					htmlTableWarnings.WriteRow(tuple.Item1, tuple.Item2, awb.CustomsActionCode, awb.CustomsActionDate, awb.NumberOfPiecesExpected, awb.NumberOfPiecesReceived);
				}

				stringbuilder.Append(htmlTableWarnings.ToHtml());

				stringbuilder.Append("<hr/>");
			}

			sortedList = reportOfWhatIsarchived.Keys.ToList();
			if (sortedList.Count > 0)
			{
				stringbuilder.Append("<h3>Archived AWB Report</h3>");
				stringbuilder.Append(string.Format(CultureInfo.InvariantCulture, "<p>Company: {0}</p>", company.CompanyName));
				stringbuilder.Append("<p>The following AWBs have been archived.</p>");

				var htmlTableGood = new HtmlTableCreator(new string[] { "AWB Number", "Reason for Archive", "CAC", "CAC Date", "Status1 Date", "NPX", "NPR" });
				htmlTableGood.EnableHTMLEncoding = false;
				SortByAwbNumber(sortedList);

				foreach (var awb in sortedList)
				{
					var tuple = GetLinkAndExplanation(awb, reportOfWhatIsarchived[awb]);
					htmlTableGood.WriteRow(tuple.Item1, tuple.Item2, awb.CustomsActionCode, awb.CustomsActionDate, awb.Status1Date, awb.NumberOfPiecesExpected, awb.NumberOfPiecesReceived);
				}

				stringbuilder.Append(htmlTableGood.ToHtml());
			}

			if (!stringbuilder.IsEmpty || GBCustomsDataRegistry.Instance.CcsukAlwaysSendAwbReport.Value)
			{
				if (stringbuilder.IsEmpty)
				{
					var pathToRelevantRegistryNode = "Maintain -> System -> Registry -> Customs -> Country or Region Specific -> United Kingdom -> Service Providers -> CCS-UK -> Always send AWB Warning Report?";
					stringbuilder.Append($"<h3>AWB Warning/Archive Report</h3> <p> Nothing was archived nor warned on this occasion. Report date: {ZDateTime.Now.ToLongTimeString()}</p> <small>To disable empty reports, change the registry setting: {pathToRelevantRegistryNode}</small>");
				}
				new CcsukEmailSender(company.Factory, CcsukEmailSender.ToWhom.CustomsGroupOnly, null)
					.SendEmailWithoutIndividualJobLink("CCSUK finalisation report",
						stringbuilder.ToStringWithNewLineBetweenAppends(),
						GBCustomsDataRegistry.Instance.NotificationCcsukArchive,
						string.Empty,
						company.PK.ToGuid(),
						Guid.Empty,
						Guid.Empty);
			}
		}

		static void SortByAwbNumber(List<ICcsukCusAwb> list)
		{
			list.Sort(delegate(ICcsukCusAwb awb1, ICcsukCusAwb awb2)
			   {
				   return awb1.ReferenceNumber.CompareTo(awb2.ReferenceNumber);
			   });
		}

		static Tuple<ZString, ZString> GetLinkAndExplanation(ICcsukCusAwb awb, ReasonForArchiving why)
		{
			var link = string.Format(CultureInfo.InvariantCulture, "<a href='{0}'>{1}</a></p>", CcsukEmailSender.GetLinkForJob((BusinessObject)awb), awb.ReferenceNumberWithShed);
			var reasonExplanation = CcsukUtilities.GetEnumDescription(why);
			return new Tuple<ZString, ZString>(link, reasonExplanation);
		}

		Dictionary<ICcsukCusAwb, ReasonForArchiving> reportOfWhatIsarchived = new Dictionary<ICcsukCusAwb, ReasonForArchiving>();
		Dictionary<ICcsukCusAwb, ReasonForArchiving> warningReportOfWhatIsNotArchived = new Dictionary<ICcsukCusAwb, ReasonForArchiving>();
		readonly Integration.ILogger serviceLogger;
	}
}
