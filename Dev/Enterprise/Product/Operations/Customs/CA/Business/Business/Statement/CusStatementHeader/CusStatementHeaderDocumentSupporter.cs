using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	sealed class CusStatementHeaderDocumentSupporter : DocumentSupporter
	{
		public CusStatementHeaderDocumentSupporter(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
		}

		CusStatementHeader StatementHeader
		{
			get { return (CusStatementHeader)base.BusinessObject; }
		}

		#region Overrides

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return Array.Empty<Constants.DataContext>();
		}

		public override BusinessContext BusinessContext => BusinessContext.CustomsStatementHdr;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return null;
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.CTY:
					return Core.Constants.CountryCodes.Canada;

				default:
					return base.GetFilterValue(filterName);
			}
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.CustomsDeclarationCustomiseDocument;

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			var result = base.GetContactOrganisation(menuName, contact, direction);

			if (contact == ContactType.Consignee)
			{
				result = new OrgHeaderContact(StatementHeader.Importer, null);
			}

			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var statementHeader = StatementHeader;
			if (statementHeader.IsCSARSF)
			{
				return new[] { BODocDataProvider.Get(new CSARevenueSummaryFormDocumentWrapper(statementHeader)) };
			}

			if (statementHeader.IsCARMDailyNotice)
			{
				return new[] { BODocDataProvider.Get(new CARMDailyNoticeDocumentWrapper(statementHeader)) };
			}

			if (statementHeader.IsCARMSOA)
			{
				return new[] { BODocDataProvider.Get(new CARMSOABillingPeriodDocumentWrapper(statementHeader)) };
			}

			var lastestMessage = statementHeader.Messages.Cast<EDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc)
				.FirstOrDefault(y => y.EM_MessageSubType == ARLMessageTypes.Codes.StatementOfAccount || y.EM_MessageSubType == ARLMessageTypes.Codes.DailyNotice);

			if (lastestMessage != null)
			{
				if (statementHeader.B2_IsMonthlyStatement)
				{
					return new[] { BODocDataProvider.Get(new ARLStatementOfAccountDocumentWrapper(lastestMessage)) };
				}
				return new[] { BODocDataProvider.Get(new ARLDailyNoticeDocumentWrapper(lastestMessage)) };
			}

			if (commandBeingRun != null && commandBeingRun.SU_MenuName == DailyNoticeMenuName)
			{
				return null;
			}

			return new[] { BODocDataProvider.Get(statementHeader) };
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			var lastestMessage = StatementHeader.Messages.Cast<EDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();

			if (lastestMessage == null)
			{
				if (commandBeingRun.SU_MenuName == DailyNoticeMenuName)
				{
					result = Res.GetString("fcff561f-2b77-4cf1-b4ff-97dfa7058c21", "Daily Notice message cannot be found.");
				}
				else if (commandBeingRun.SU_MenuName == CARMStatementOfAccountMenuName)
				{
					result = Res.GetString("BD0B4050-90D6-4BB6-8E9F-B79C03DC8044", "CARM Statement Of Account message cannot be found.");
				}
			}

			return result;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion

		const string DailyNoticeMenuName = "Daily Notice";
		const string CARMStatementOfAccountMenuName = "CARM Statement Of Account";
	}
}
