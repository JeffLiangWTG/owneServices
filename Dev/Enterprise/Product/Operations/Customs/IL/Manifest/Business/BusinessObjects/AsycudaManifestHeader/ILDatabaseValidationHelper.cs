using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class ILDatabaseValidationHelper : ZZDatabaseValidationHelper
	{
		public ILDatabaseValidationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override bool GetMandatoryFieldsInZZ => false;

		protected override Dictionary<string, MandatoryValidationRule> GetMandatoryFieldsCore()
		{
			var rules = base.GetMandatoryFieldsCore();
			rules.Add(Constants.ILDatabaseValidationHelper.Seal, new MandatoryValidationRule(Constants.ILDatabaseValidationHelper.SealIsRequired, () => true));

			return rules;
		}
	}
}
