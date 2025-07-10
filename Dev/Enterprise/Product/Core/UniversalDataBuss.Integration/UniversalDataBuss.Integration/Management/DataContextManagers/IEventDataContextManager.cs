using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IEventDataContextManager : IDataContextManager
	{
		BusinessObject[] GetLogParentsForEvent(IXmlEventValueObject xmlEvent, BusinessObjectFactory factory, IXmlImportLogger logger);

		IKeysResult GetLogKeysForEvent(IXmlEventValueObject xmlEvent, BusinessObjectFactory factory, IXmlImportLogger logger);

		bool ManagesEvents { get; }

		IEventDataObjectWriter GetEventDataObjectWriter(IDataWritingManager writeManager);

		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> EventContextValues { get; }

		IEnumerable<IUniversalJobLink> GetEventDataTarget(RecipientRoleType recipientRole, IOrgHeader recipientOrganisation);

		IEnumerable<KeyValuePair<IZType, IZType>> AdditionalFieldsToUpdateValues { get; }

		void OnUniversalEventAdded(IXmlSessionTracker logger, IXmlEventValueObject xmlEvent);

		bool CanUpdateLogParentFromEvent(BusinessObject logParent, IXmlEventValueObject xmlEvent, out ZString failureReason);
	}
}
