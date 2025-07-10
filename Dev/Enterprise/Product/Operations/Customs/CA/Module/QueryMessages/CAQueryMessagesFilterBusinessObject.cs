using CargoWise.Types;
using Enterprise.Messaging.Module;

namespace Enterprise.Customs.CA.Module
{
	class CAQueryMessagesFilterBusinessObject : EDIMessageFilterBusinessObject
	{
		protected override ZBool ShouldAddEHubIDFilters
		{
			get { return false; }
		}
	}
}
