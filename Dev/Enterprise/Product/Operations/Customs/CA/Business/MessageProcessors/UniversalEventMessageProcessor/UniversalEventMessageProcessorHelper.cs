using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public static class UniversalEventMessageProcessorHelper
	{
		public static ZString GetContextValueByType(List<Context> contextCollection, ZString type)
		{
			var result = ZString.Empty;
			if (contextCollection != null)
			{
				var context = contextCollection.FirstOrDefault(o => o.Type != null && o.Type.Type.GetValueOrDefault() == type);
				if (context != null)
				{
					result = context.Value.GetValueOrDefault();
				}
			}
			return result;
		}

		public static ZString GetTargetKeyByType(IEnumerable<IDataTargetDataObject> targetCollection, ZString type)
		{
			var result = ZString.Empty;
			if (targetCollection != null)
			{
				var target = targetCollection.FirstOrDefault(o => (o?.Type.GetValueOrDefault() ?? ZString.Empty) == type);
				if (target != null)
				{
					result = target.Key.GetValueOrDefault();
				}
			}
			return result;
		}

		public static IEnumerable<ZString> GetContextValuesByType(List<Context> contextCollection, ZString type)
		{
			return contextCollection?.Where(o => (o.Type?.Type.GetValueOrDefault() ?? ZString.Empty) == type)
						?.Select(o => o.Value.GetValueOrDefault());
		}

		public static ZString GetUniqueIDValueFromDataTarget(UniversalEvent eventDataObject)
		{
			var result = ZString.Empty;
			if (eventDataObject.DataContext != null && eventDataObject.DataContext.DataTargetCollection != null && eventDataObject.DataContext.DataTargetCollection.Any())
			{
				var uniqueID = eventDataObject.DataContext.DataTargetCollection.FirstOrDefault();
				result = uniqueID != null ? uniqueID.Key.GetValueOrDefault() : ZString.Empty;
			}
			return result;
		}

		internal static BusinessObjectCollectionWrapper<D4MessageInterpretationGenerator.RelatedDocument> GetRelatedDocument(UniversalEvent eventDataObject)
		{
			var result = new List<D4MessageInterpretationGenerator.RelatedDocument>();
			var contextCollection = eventDataObject.ContextCollection;
			if (contextCollection != null)
			{
				foreach (var context in contextCollection)
				{
					var contextType = context.Type;
					if (contextType != null && contextType.Type.GetValueOrDefault() == UniversalEventMessageProcessorConstants.ContextType.RelatedDocument.Name)
					{
						var subContextCollection = context.SubContextCollection;
						if (subContextCollection != null)
						{
							var documentNumber = UniversalEventMessageProcessorHelper.GetContextValueByType(subContextCollection, UniversalEventMessageProcessorConstants.ContextType.RelatedDocument.SubContextType.DocumentNumber);
							var documentType = UniversalEventMessageProcessorHelper.GetContextValueByType(subContextCollection, UniversalEventMessageProcessorConstants.ContextType.RelatedDocument.SubContextType.DocumentType);
							if (!documentNumber.IsEmpty)
							{
								var document = new D4MessageInterpretationGenerator.RelatedDocument
								{
									DocumentType = documentType,
									DocumentNumber = documentNumber
								};
								result.Add(document);
							}
						}
					}
				}
			}
			return new BusinessObjectCollectionWrapper<D4MessageInterpretationGenerator.RelatedDocument>(result);
		}

		public static ZDateTimeOffset GetEventTime(this UniversalEvent universalEvent)
		{
			return universalEvent?.EventTime ?? ZDateTimeOffset.Empty;
		}

		public static ZString GetContextValueByType(this UniversalEvent universalEvent, ZString contentType)
		{
			return universalEvent == null ? ZString.Empty : GetContextValueByType(universalEvent.ContextCollection, contentType);
		}

		public static ZString GetTargetKeyByType(this UniversalEvent universalEvent, ZString contentType)
		{
			var context = universalEvent?.DataContext;
			return context == null ? ZString.Empty : GetTargetKeyByType(context.DataTargetCollection, contentType);
		}
	}
}
