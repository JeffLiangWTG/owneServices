using System.Collections.Generic;

namespace Enterprise.Accounting.Business.Testing
{
	public class ArgentinaLabelTranslatorTest : CountryLabelTranslatorTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Argentina;

		protected override Dictionary<LabelsEnum, (string[] languages, object[] parametersForTranslation, string translation)[]> ExpectedLabelTranslations =>
			new Dictionary<LabelsEnum, (string[] languages, object[] parametersForTranslation, string translation)[]>
			{
				{
					LabelsEnum.RecipientConsumptionTaxRegimeHeading,
					new []
					{
						(SpanishLanguages, new object[] { "XXX" }, "CONDICIÓN FRENTE AL XXX")
					}
				},
			};
	}
}
