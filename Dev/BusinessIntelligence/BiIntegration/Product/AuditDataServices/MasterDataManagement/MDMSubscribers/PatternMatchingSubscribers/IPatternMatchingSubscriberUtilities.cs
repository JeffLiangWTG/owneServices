using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public interface IPatternMatchingSubscriberUtilities
	{
		bool UpdatePatternMatchingCountryCode(BusinessObjectFactory factory, SchemaGuidColumn parentIdColumn, SchemaGuidColumn masterIDColumn, ZGuid parentId, BusinessObject master, ZString newCountryCode);

		SchemaGuidColumn PatternMatchingAddressMasterIdColumn { get; }

		SchemaGuidColumn PatternMatchingDomainMasterIdColumn { get; }

		SchemaGuidColumn PatternMatchingEmailMasterIdColumn { get; }

		SchemaGuidColumn PatternMatchingNameMasterIdColumn { get; }

		SchemaGuidColumn PatternMatchingPhoneMasterIdColumn { get; }

		SchemaGuidColumn PatternMatchingRegCodeMasterIdColumn { get; }
	}
}
