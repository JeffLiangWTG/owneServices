using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class IntegratedImportDeclarationEventParentFinder : EventParentFinder
	{
		internal IntegratedImportDeclarationEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject)
		{
			var result = new List<BusinessObject>();
			var dataContext = eventDataObject?.DataContext;
			if (dataContext?.RecipientRoleCollection?.FirstOrDefault()?.Code == RecipientRoleType.CD4)
			{
				var uniqueIDValue = eventDataObject.GetTargetKeyByType(nameof(DataContextType.CAIntegratedImportDeclaration));
				if (!uniqueIDValue.IsEmpty)
				{
					var applicationReference = DuplicateUniversalEventChecker.GetApplicationReference(eventDataObject);
					if (DuplicateUniversalEventChecker.CheckDuplicateMessages(applicationReference, factory, new[] { UniversalEventMessageTypes.Codes.IIDResponses, UniversalEventMessageTypes.Codes.D4Notices }))
					{
						throw new DuplicateMessageException(applicationReference);
					}
					else
					{
						var cusEntryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
						cusEntryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, uniqueIDValue);
						cusEntryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, MessageTypeList.Codes.EDIRelease);
						result.AddRange(factory.Load<CusEntryHeader>(cusEntryHeaderQuery));
					}
				}
			}
			return result.ToArray();
		}
	}
}
