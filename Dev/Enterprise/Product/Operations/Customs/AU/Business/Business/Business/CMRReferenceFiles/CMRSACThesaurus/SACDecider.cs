using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SACDecider
	{
		public SACDecider(BusinessObjectFactory factory, ZDecimal valueInLocalCurrencyToCheck, ZString descriptionToCheck)
		{
			this.factory = factory;
			this.valueInLocalCurrencyToCheck = valueInLocalCurrencyToCheck;
			this.descriptionToCheck = descriptionToCheck;
		}

		public virtual ZBool IsValidForSAC
		{
			get
			{
				if (AUCustomsDataRegistry.Instance.ManifestSACOverride.Value)
				{
					return !IsValueOverTheScreenFreeValue;
				}
				else
				{
					return !IsValueOverTheScreenFreeValue && !StopPhrasesFoundInGoodsDescription.Any();
				}
			}
		}

		public ZBool IsValueOverTheScreenFreeValue
		{
			get
			{
				var deminimus = UniversalReferenceHelper.GetDeminimus(factory);
				return valueInLocalCurrencyToCheck > deminimus;
			}
		}

		public IEnumerable<ZString> StopPhrasesFoundInGoodsDescription => StopPhraseMatcher.FindMatchingStopPhrasesPluralized(descriptionToCheck, SACStopPhrases);

		public ZString ThesaurusWordsFoundAsSingleString()
		{
			ZString result = ZString.Empty;
			foreach (string foundWord in StopPhrasesFoundInGoodsDescription)
			{
				result += result.IsEmpty ? foundWord : ", " + foundWord;
			}

			return result;
		}

		#region Implementation

		readonly BusinessObjectFactory factory;
		readonly ZDecimal valueInLocalCurrencyToCheck;
		readonly ZString descriptionToCheck;

		string[] SACStopPhrases
		{
			get
			{
				return factory.GetCachedValue("SACDecider|SACStopPhrases", () =>
				{
					var thesaurus = CMRReferenceDataHelper.LoadCMRSACThesaurus(factory);
					return thesaurus.Select(x => x.ToString()).ToArray();
				});
			}
		}
		#endregion
	}
}
