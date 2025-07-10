using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class USCustomsDisbursementChargeCodesValueProvider : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<USCustomsDisbursementChargeCodes>",
				ResString.GetMultilingualString("3cba0632-9556-48b1-9eb9-a85263d7ea84", "Will return a concatenated GUIDs separated by '|' that are set in Registry in 'Default Customs Disbursement Charge Code' and 'US Customs Disbursement Charge Code Override'."),
				new List<(string example, object expectedResult)> { ("<USCustomsDisbursementChargeCodes>", "c83a70e8-bd85-468e-9445-a59003cb4b71|2fb5259f-bd20-4b40-8157-d7e7e7e66086|0a5a037c-5e9a-4bd5-b250-07921907797d") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = new List<string>();
			result.Add(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value.ToString());
			foreach (EntryChargeTypeSetting entryChargeType in RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				var chargeCode = entryChargeType.AC_ChargeCode.ToString();
				if (!result.Contains(chargeCode))
				{
					result.Add(chargeCode);
				}
			}
			return new ZStringBuilder(result.ToArray()).ToStringWithDelimiterBetweenAppends("|");
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*USCustomsDisbursementChargeCodes\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
