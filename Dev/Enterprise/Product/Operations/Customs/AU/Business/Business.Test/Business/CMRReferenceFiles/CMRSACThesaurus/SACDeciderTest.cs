using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SACDeciderTest : TestCaseWithFactory
	{
		public void TestStopPhrasesFoundInGoodsDescription()
		{
			AssertNoStopWordsFoundInGoodsDescription(new SACDecider(Factory, 250, "Pillow"));
			AssertNoStopWordsFoundInGoodsDescription(new SACDecider(Factory, 250, "Carpet"));
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, TraditionalMedicine), TraditionalMedicine);
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, "Traditional blah medicine pillow traditional medicine"), TraditionalMedicine, Medicine);
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, "pillow \t-tRaditional mediCine+\t"), TraditionalMedicine, Medicine);
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, "pillow \t-tRaditional mediCine+\t"), TraditionalMedicine, Medicine);
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, "Traditional,blah,medicine,pill,traditional medicine"), TraditionalMedicine, Medicine, "Pill");
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, "pillow traditional-medicine"), TraditionalMedicine, Medicine);
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, "pillow **traditional**medicine**"), TraditionalMedicine, Medicine);
		}

		public void TestStopPhrasesFoundInGoodsDescription_WithPlurals()
		{
			AssertNoStopWordsFoundInGoodsDescription(new SACDecider(Factory, 250, "Pillow"));
			AssertNoStopWordsFoundInGoodsDescription(new SACDecider(Factory, 250, "Pillows"));
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, Gun), Gun);
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, "Guns"), Gun);
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, "Gas"), "Gas");
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, "Gases"), "Gas");
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, "Pyrotechnic device"), "Pyrotechnic device");
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, "Pyrotechnic devices"), "Pyrotechnic device");
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, "Brandy"), "Brandy");
			AssertStopPhrasesFoundInGoodsDescription(new SACDecider(Factory, 250, "Brandies"), "Brandy");
		}

		public void TestThesaurusWordsFoundAsSingleString()
		{
			SACDecider sACDecider = new SACDecider(Factory, 250, "Pillow");
			AssertEquals("Words are empty", "", sACDecider.ThesaurusWordsFoundAsSingleString());

			sACDecider = new SACDecider(Factory, 250, "Carpet");
			AssertEquals("Words are empty", "", sACDecider.ThesaurusWordsFoundAsSingleString());

			sACDecider = new SACDecider(Factory, 250, "Pharmaceutical");
			AssertEquals("Stop phrase found", "Pharmaceutical", sACDecider.ThesaurusWordsFoundAsSingleString());

			sACDecider = new SACDecider(Factory, 250, "Traditional blah medicine pillow traditional medicine");
			Assert("Medicine stop phrases found", sACDecider.ThesaurusWordsFoundAsSingleString().Contains("Medicine"));
			Assert("Traditional medicine stop phrases found", sACDecider.ThesaurusWordsFoundAsSingleString().Contains("Traditional medicine"));

			sACDecider = new SACDecider(Factory, 250, "pills");
			AssertEquals("Stop phrase found", "Pill", sACDecider.ThesaurusWordsFoundAsSingleString());

			sACDecider = new SACDecider(Factory, 250, "Pharmaceutical ampule injection gun refill for non-surgical use and pill replacment");
			Assert("Gun stop phrases found", sACDecider.ThesaurusWordsFoundAsSingleString().Contains("Gun"));
			Assert("Pill stop phrases found", sACDecider.ThesaurusWordsFoundAsSingleString().Contains("Pill"));
			Assert("Pharmaceutical stop phrases found", sACDecider.ThesaurusWordsFoundAsSingleString().Contains("Pharmaceutical"));
		}

		public void TestIsValidForSAC_ValidWithValueAndDescription()
		{
			var sACDecider = new SACDecider(Factory, 249, "Description");
			AssertEquals("Value and Description OK", true, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", false, sACDecider.IsValueOverTheScreenFreeValue);
			AssertNoStopWordsFoundInGoodsDescription(sACDecider);

			sACDecider = new SACDecider(Factory, 250, "Description");
			AssertEquals("Value and Description OK", true, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", false, sACDecider.IsValueOverTheScreenFreeValue);
			AssertNoStopWordsFoundInGoodsDescription(sACDecider);
		}

		public void TestIsValidForSAC_NotValidWithRightValueAndWrongDescription()
		{
			var sACDecider = new SACDecider(Factory, 250, "Traditional medicine");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", false, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, TraditionalMedicine, Medicine);

			sACDecider = new SACDecider(Factory, 250, "Description with Plutonium in the middle of it");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", false, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, "Plutonium");

			sACDecider = new SACDecider(Factory, 250, "Description with Household goods in the middle of it");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", false, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, "Household goods");

			sACDecider = new SACDecider(Factory, 250, "Micro organism at the start");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", false, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, "Micro organism");

			sACDecider = new SACDecider(Factory, 250, "Word at the end is Cigar");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", false, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, "Cigar");

			sACDecider = new SACDecider(Factory, 250, "Word at the end is CIGAR");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", false, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, "Cigar");

			sACDecider = new SACDecider(Factory, 250, "plutonium Word at the end is CIGAR MiCro orGanIsm");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", false, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, "Cigar", "Micro organism", "Plutonium");

			using (AUCustomsDataRegistry.Instance.ManifestSACOverride.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Assert("Right Value and registry says Thesaurus to be ignored", new SACDecider(Factory, 250, "plutonium Word at the end is CIGAR MiCro orGanIsm").IsValidForSAC);
			}
		}

		public void TestIsValidForSAC_NotValidWithWrongValueAndRightDescription()
		{
			var sACDecider = new SACDecider(Factory, 650, "Description");
			AssertEquals("WrongValueAndRightDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", true, sACDecider.IsValueOverTheScreenFreeValue);
			AssertNoStopWordsFoundInGoodsDescription(sACDecider);

			sACDecider = new SACDecider(Factory, 1200, "Description");
			AssertEquals("WrongValueAndRightDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", true, sACDecider.IsValueOverTheScreenFreeValue);
			AssertNoStopWordsFoundInGoodsDescription(sACDecider);

			sACDecider = new SACDecider(Factory, 251, "Description");
			AssertEquals("WrongValueAndRightDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", true, sACDecider.IsValueOverTheScreenFreeValue);
			AssertNoStopWordsFoundInGoodsDescription(sACDecider);

			sACDecider = new SACDecider(Factory, 1000, "Description");
			AssertEquals("WrongValueAndRightDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", true, sACDecider.IsValueOverTheScreenFreeValue);
			AssertNoStopWordsFoundInGoodsDescription(sACDecider);

			sACDecider = new SACDecider(Factory, 3000, "Description");
			AssertEquals("WrongValueAndRightDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", true, sACDecider.IsValueOverTheScreenFreeValue);
			AssertNoStopWordsFoundInGoodsDescription(sACDecider);
		}

		public void TestIsValidForSAC_NotValidWithWrongValueAndWrongDescription()
		{
			var sACDecider = new SACDecider(Factory, 650, "Traditional medicine");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", true, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, TraditionalMedicine, Medicine);

			sACDecider = new SACDecider(Factory, 1200, "Description with Plutonium in the middle of it");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", true, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, "Plutonium");

			sACDecider = new SACDecider(Factory, 1500, "Description with Household goods in the middle of it");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", true, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, "Household goods");

			sACDecider = new SACDecider(Factory, 1000, "Micro organism at the start");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", true, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, "Micro organism");

			sACDecider = new SACDecider(Factory, 251, "Word at the end is Cigar");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", true, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, "Cigar");

			sACDecider = new SACDecider(Factory, 15000, "Word at the end is CIGAR");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", true, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, "Cigar");

			sACDecider = new SACDecider(Factory, 8000, "plutonium Word at the end is CIGAR MiCro orGanIsm");
			AssertEquals("RightValueAndWrongDescription", false, sACDecider.IsValidForSAC);
			AssertEquals("ValueOverTheScreenFreeValue", true, sACDecider.IsValueOverTheScreenFreeValue);
			AssertStopPhrasesFoundInGoodsDescription(sACDecider, "Cigar", "Micro organism", "Plutonium");

			using (AUCustomsDataRegistry.Instance.ManifestSACOverride.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Assert("Right Value and registry says Thesaurus to be ignored", new SACDecider(Factory, 200, "plutonium Word at the end is CIGAR MiCro orGanIsm").IsValidForSAC);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TaxOrFeeTestHelper.SetUp();
			TaxOrFeeTestHelper.SetDeminimus(Factory, 250m);
			CMRReferenceFilesTestHelper.InsertThesaurusWithDefaultData(Factory);
		}

		void AssertStopPhrasesFoundInGoodsDescription(SACDecider sacDecider, params string[] expectedFoundPhrases)
		{
			var actualFoundPhrases = sacDecider.StopPhrasesFoundInGoodsDescription;
			CombineAssertions(() =>
			{
				foreach (ZString expectedPhrase in expectedFoundPhrases)
				{
					AssertCollectionContains(expectedPhrase, expectedPhrase, actualFoundPhrases);
				}
			});
		}

		void AssertNoStopWordsFoundInGoodsDescription(SACDecider sacDecider)
		{
			Assert(!sacDecider.StopPhrasesFoundInGoodsDescription.Any());
		}

		const string TraditionalMedicine = "Traditional medicine";
		const string Medicine = "Medicine";
		const string Gun = "Gun";
	}
}
