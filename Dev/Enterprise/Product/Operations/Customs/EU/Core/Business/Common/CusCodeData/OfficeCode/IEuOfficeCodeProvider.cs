using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public interface IEuOfficeCodeProvider
	{
		ZString CountryCode { get; }
		ZBool IsImport { get; }
		ZBool IsExport { get; }
		bool IsNCTS { get; }
		bool IsEMCS { get; }
		IEnumerable<EuOfficeCode> CustomsOffices { get; }
		CustomsOfficeRequirementHelper CustomsOfficeRequirementHelper { get; }
	}
}
