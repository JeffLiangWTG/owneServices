using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = MailManager.Module.Res;
using ResString = MailManager.Module.ResString;

namespace Enterprise.MailManager.Module
{
	public class MailItemFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Constants
		{
			public const string Queued = "QUE";
		}

		public MailItemFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddDateFilters(filters);
			AddModeFilters(filters);
			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Subject", MailDBItemsSchema.MI_Subject).MultilingualDescription = ResString.GetMultilingualString("MailManager|MailItemFilter|Subject", "Subject");
			filters.AddTextFilter("Mail Body", MailDBItemsSchema.MI_Body).MultilingualDescription = ResString.GetMultilingualString("MailManager|MailItemFilter|MailBody", "Mail Body");
			filters.AddTextFilter("Sender", MailDBItemsSchema.MI_From).MultilingualDescription = ResString.GetMultilingualString("MailManager|MailItemFilter|Sender", "Sender");
			var recipientsFilter = filters.AddTextFilter("Recipients", RecipientsQuery);
			recipientsFilter.MultilingualDescription = ResString.GetMultilingualString("MailManager|MailItemFilter|Recipients", "Recipients");
			recipientsFilter.SubGroup = new MailRecipientSubGroup();
		}

		#endregion

		#region Date

		void AddDateFilters(ModuleFilterCollection filters)
		{
			ModuleDateFilter dateFilter = filters.AddDateFilter("Send", MailDBItemsSchema.MI_SendDateTime, true);
			dateFilter.HideFutureDates = true;
			dateFilter.MultilingualDescription = ResString.GetMultilingualString("MailManager|MailItemFilter|Send", "Send");

			dateFilter = filters.AddDateFilter("Received", MailDBItemsSchema.MI_ReceivedDateTime, true);
			dateFilter.HideFutureDates = true;
			dateFilter.MultilingualDescription = ResString.GetMultilingualString("MailManager|MailItemFilter|Received", "Received");
		}

		#endregion

		#region Mode

		void AddModeFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter directionFilter = filters.AddTextFilter("Direction", MailDBItemsSchema.MI_Direction, MI_DirectionList);
			directionFilter.Category = FilterCategories.ModesAndTypes;
			directionFilter.MultilingualDescription = ResString.GetMultilingualString("MailManager|MailItemFilter|Direction", "Direction");

			ModuleTextFilter statusFilter = filters.AddTextFilter("Status", GetStatusQuery, MI_StatusList);
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("MailManager|MailItemFilter|Status", "Status");
		}

		#endregion

		#endregion

		#region Lists

		public CodeDescriptionPairList MI_DirectionList
		{
			get
			{
				if (fMI_DirectionList == null)
				{
					fMI_DirectionList = new CodeDescriptionPairList();
					fMI_DirectionList.AddPair(MailDirection.Transmit, Res.GetString("MailManager|MailItemFilter|Direction|Transmit", "Transmit"));
					fMI_DirectionList.AddPair(MailDirection.Receive, Res.GetString("MailManager|MailItemFilter|Direction|Receive", "Receive"));
				}

				return fMI_DirectionList;
			}
		}

		CodeDescriptionPairList fMI_DirectionList;

		public CodeDescriptionPairList MI_StatusList
		{
			get
			{
				if (fMI_StatusList == null)
				{
					fMI_StatusList = GetStatusListCore();
				}

				return fMI_StatusList;
			}
		}

		protected virtual CodeDescriptionPairList GetStatusListCore()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(MailStatus.Failed, Res.GetString("MailManager|MailItemFilter|Status|Failed", "Failed"));
			result.AddPair(MailStatus.Processed, Res.GetString("MailManager|MailItemFilter|Status|Processed", "Processed"));
			result.AddPair(MailStatus.Unprocessed, Res.GetString("MailManager|MailItemFilter|Status|Unprocessed", "Unprocessed"));
			result.AddPair(MailStatus.Queued, Res.GetString("MailManager|MailItemFilter|Status|Queued", "Queued"));
			result.AddPair(MailStatus.QueuedWithAck, Res.GetString("MailManager|MailItemFilter|Status|QueuedWithAck", "Queued With Acknowledgement"));
			result.AddPair(MailStatus.Sent, Res.GetString("MailManager|MailItemFilter|Status|Sent", "Sent"));
			return result;
		}

		CodeDescriptionPairList fMI_StatusList;

		#endregion

		#region Query

		ZQuery RecipientsQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(MailRecipient));
			query.AddToFilter(JoinCondition.And, MailDBRecipientsSchema.MR_RecipientMailAddress, comparisonOperator, value);
			return query;
		}

		protected class MailRecipientSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(MailItem));

				var mailRecipientSubQuery = new ZDBOnlySubQuery(typeof(MailRecipient), MailDBRecipientsSchema.MR_MI);
				mailRecipientSubQuery.AddToFilter(filter);
				result.AddSubQuery(mailRecipientSubQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#region Status

		ZQuery GetStatusQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(MailDBItemsSchema.MI_Status, value);
			return result;
		}

		#endregion
	}
}
