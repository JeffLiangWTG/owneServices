using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.ResourceStrings.Vocabulary
{
	[Immutable]
	class EnglishVocabulary : BaseVocabulary
	{
		EnglishVocabulary()
		 : base(BuildVocabularyRules())
		{
		}

		static readonly Lazy<EnglishVocabulary> instance = new Lazy<EnglishVocabulary>(() => new EnglishVocabulary());

		public static EnglishVocabulary Instance => instance.Value;

		#region SuppressResourceStringsCheckRegion
		static ImmutableList<VocabularyRule> BuildVocabularyRules()
		{
			var rules = new List<VocabularyRule>
			{
				CreatePluralRule("$", "s"),
				CreatePluralRule("s$", "s"),
				CreatePluralRule("is$", "es"),
				CreatePluralRule("(o|z|x|ss|ch|sh|us)$", "$1es"),
				CreatePluralRule("([dti])um$", "$1a"),
				CreatePluralRule("a$", "ae"),
				CreatePluralRule("(f|fe)$", "ves"),
				CreatePluralRule("ix$", "ices"),
				CreatePluralRule("([^aeiouy])y$", "$1ies"),
				CreatePluralRule("(fish|sheep|cattle|deer|salmon|trout|troy|fix)$", "$1"),
				CreatePluralRule("(alias|bias|virus|affix)$", "$1es"),
				CreatePluralRule("(roof|silo|piano|photo|macro)$", "$1s"),
				CreatePluralRule("(l|m)ouse$", "$1ice"),
				CreatePluralRule("(quiz)$", "$1zes"),
				CreateIrregularRule("child", "children"),
				CreateIrregularRule("person", "people"),
				CreateIrregularRule("man", "men"),
				CreateIrregularRule("woman", "women"),
				CreateIrregularRule("foot", "feet"),
				CreateIrregularRule("tooth", "teeth"),
				CreateIrregularRule("goose", "geese"),
				CreateIrregularRule("brother", "brethren"),
				CreateIrregularRule("ox", "oxen", true),
				CreateIrregularRule("Bale, Compressed", "Bales, Compressed", true),
				CreateIrregularRule("Bale, Uncompressed", "Bales, Uncompressed", true),
				CreateIrregularRule("Break Bulk", "Break Bulk", true),
				CreateIrregularRule("dozen", "dozen"),
				CreateIrregularRule("bizo", "bizos"),
				CreateIrregularRule("curriculum", "curricula"),
				CreateIrregularRule("stadium", "stadiums"),
				CreateIrregularRule("Tariff", "Tariffs")
			};

			var immutableRules = ImmutableList.CreateRange(rules);
			return immutableRules;
		}

		#endregion
	}
}
