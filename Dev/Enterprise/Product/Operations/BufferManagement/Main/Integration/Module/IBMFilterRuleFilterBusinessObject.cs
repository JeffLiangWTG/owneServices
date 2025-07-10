using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMFilterRuleFilterBusinessObject : IRelatedModuleFilterBusinessObject
	{
		string FilterControlIdentifier { get; set; }

		IEnumerable<ZString> ActiveFilterIdentifiers { get; }

		IEnumerable<ZString> ActiveFilterIdentifiersIncludingUserDefined { get; }
	}
}
