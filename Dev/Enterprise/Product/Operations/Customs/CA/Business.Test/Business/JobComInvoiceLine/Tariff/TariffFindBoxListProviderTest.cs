namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TariffFindBoxListProviderTest : DeclarationTestHelper
	{
		public void TestCanLookupFormattedExportTariff()
		{
			var tariff2710119000 = Factory.New<CACExportTariff>();
			tariff2710119000.CE_Code = "2710119000";
			var collection = new CACExportTariffCollection(Factory);
			var provider = new TariffFindBoxListProvider(collection);

			var tariff = (CACExportTariff)provider.GetBusinessObjectFromCode(tariff2710119000.CE_FormattedCode);
			AssertEquals(tariff.CE_FormattedCode, tariff2710119000.CE_FormattedCode);

			tariff = (CACExportTariff)provider.GetBusinessObjectFromCode(tariff2710119000.CE_Code);
			AssertEquals(tariff.CE_Code, tariff2710119000.CE_Code);
		}

		public void TestCanLookupFormattedTariff()
		{
			var tariff2710119000 = Factory.New<CACExportTariff>();
			tariff2710119000.CE_Code = "2710119000";
			var collection = new CACExportTariffCollection(Factory);
			var provider = new TariffFindBoxListProvider(collection);

			var tariff = (CACExportTariff)provider.GetBusinessObjectFromCode(tariff2710119000.CE_FormattedCode);
			AssertEquals(tariff.CE_FormattedCode, tariff2710119000.CE_FormattedCode);

			tariff = (CACExportTariff)provider.GetBusinessObjectFromCode(tariff2710119000.CE_Code);
			AssertEquals(tariff.CE_Code, tariff2710119000.CE_Code);
		}

		public void TestDescriptionFromCode()
		{
			NewTariff("0101", "Live horses, asses, mules and hinnies.");
			NewTariff("01019000", "Other ");
			NewTariff("010190001", "Horses: ");
			NewTariff("0101900012", "For racing ");
			NewTariff("01011000", "Pure-bred breeding animals");
			NewTariff("0101100010", "Horses");
			NewTariff("4901", "Printed books, brochures, leaflets and similar printed matter, whether or not in single sheets. ");

			NewTariff("7326", "Other articles of iron or steel.");
			NewTariff("732690", "Other.");
			NewTariff("73269090", "Other.");
			NewTariff("7326909090", "Other.");

			NewTariff("3920", "Other plates, sheets, film, foil and strip, of plastics, non - cellular and not reinforced, laminated, supported or similarly combined with other materials.");
			NewTariff("39202000", "Of polymers of propylene");
			NewTariff("392020009", "Other:");
			NewTariff("3920200091", "Of a thickness not exceeding 0.25 mm");

			var provider = new TariffFindBoxListProvider(new CACExportTariffCollection(Factory));

			AssertEquals("DescriptionFromCode", "Horses: For racing. (Live horses, asses, mules and hinnies. Other.)", provider.DescriptionFromCode("0101900012"));
			AssertEquals("DescriptionFromCode", "Horses (Live horses, asses, mules and hinnies. Pure-bred breeding animals.)", provider.DescriptionFromCode("0101100010"));
			AssertEquals("DescriptionFromCode", "Printed books, brochures, leaflets and similar printed matter, whether or not in single sheets.", provider.DescriptionFromCode("4901"));
			AssertEquals("DescriptionFromCode", "Other. (Other articles of iron or steel. Other. Other.)", provider.DescriptionFromCode("7326909090"));
			AssertEquals("DescriptionFromCode", "Other: Of a thickness not exceeding 0.25 mm. (Other plates, sheets, film, foil and strip, of plastics, non - cellular and not reinforced, laminated, supported or similarly combined with other materials. Of polymers of propylene.)", provider.DescriptionFromCode("3920200091"));
		}

		void NewTariff(string code, string description)
		{
			var tariff = Factory.New<CACExportTariff>();
			tariff.CE_Code = code;
			tariff.CE_Description = description;
		}
	}
}
