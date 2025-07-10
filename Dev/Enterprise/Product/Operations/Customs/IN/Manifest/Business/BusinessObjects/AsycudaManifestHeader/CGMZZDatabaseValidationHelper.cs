using System.Collections.Generic;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.IN.Manifest.Business;

public class CGMZZDatabaseValidationHelper : ASYCUDA.Business.ZZDatabaseValidationHelper
{
	public CGMZZDatabaseValidationHelper(CGMAsycudaManifestHeader header) : base(header)
	{
	}

	protected override bool GetMandatoryFieldsInZZ => false;

	protected override Dictionary<string, MandatoryValidationRule> GetMandatoryFieldsCore()
	{
		var mandatoryFields = base.GetMandatoryFieldsCore();
		mandatoryFields[ManifestValidationRuleCodes.Consignee].NeedsToCheck = () => false;
		mandatoryFields[ManifestValidationRuleCodes.EstimatedDepartureTime].NeedsToCheck = () => false;
		return mandatoryFields;
	}
}
