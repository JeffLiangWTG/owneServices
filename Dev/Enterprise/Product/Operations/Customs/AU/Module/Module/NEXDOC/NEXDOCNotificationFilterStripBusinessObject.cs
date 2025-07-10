using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module.NEXDOC
{
	public class NEXDOCNotificationFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			filters.AddNumberFilter("Rex Number", QuarantineNexDocNotificationSchema.QN_RexNumber).WithMaxLengthOf<ModuleNumberFilter>(QuarantineNexDocNotificationSchema.QN_RexNumber);

			ModuleTextFilter typeFilter = filters.AddTextFilter("Exporter Reference", QuarantineNexDocNotificationSchema.QN_ExporterReference);
			typeFilter.MaxLength = QuarantineNexDocNotificationSchema.QN_ExporterReference.MaxLength;

			var notificationTypeFilter = filters.AddTextFilter("Notification Type", QuarantineNexDocNotificationSchema.QN_NotificationType, NotificationType);
			notificationTypeFilter.Category = FilterCategories.ModesAndTypes;
			notificationTypeFilter.MaxLength = QuarantineNexDocNotificationSchema.QN_NotificationType.MaxLength;

			var ackFilter = filters.AddTextFilter("Acknowledge Status", QuarantineNexDocNotificationSchema.QN_AcknowledgeStatus, AcknowledgeStatus).WithMaxLengthOf<ModuleTextFilter>(QuarantineNexDocNotificationSchema.QN_AcknowledgeStatus);
			ackFilter.Visibility = FilterVisibility.AlwaysVisible;
			ackFilter.DefaultProperty = NEXDOCAcknowledgeStatus.Codes.NotActioned;

			filters.AddTextFilter("Message Status", QuarantineNexDocNotificationSchema.QN_MessageStatus, MessageStatus).WithMaxLengthOf<ModuleTextFilter>(QuarantineNexDocNotificationSchema.QN_MessageStatus);

			filters.AddDateFilter("Received Date", QuarantineNexDocNotificationSchema.QN_SystemCreateTimeUtc, true).Category = FilterCategories.Dates;
			return filters;
		}

		internal CodeDescriptionPairList NotificationType => Factory.GetCachedValue<NEXDOCNotificationType>();
		internal CodeDescriptionPairList MessageStatus => Factory.GetCachedValue<NEXDOCMessageStatus>();
		internal CodeDescriptionPairList AcknowledgeStatus => Factory.GetCachedValue<NEXDOCAcknowledgeStatus>();
	}
}
