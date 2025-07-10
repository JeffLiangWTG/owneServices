using System.Collections.Generic;
using CargoWise.Integration;

namespace Enterprise.Integration.Compliance
{
	public interface IComplianceSubTypeList : IEnumerable<IComplianceSubType>
	{
		ICodeDescriptionPairList GetDescriptionPairList();

		ICodeDescriptionPairList GetLocalDescriptionPairList();
	}
}
