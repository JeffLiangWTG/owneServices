using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class ValidationMessage : EU.H7.Business.ValidationMessage
	{
		protected override string GetInvalidValueRuleCodeCore() => "BR0020";

		#region BR1106

		public string BR1106RuleCode => "BR1106";

		public string GetBR1106RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR1106RuleCode, Res.GetString("9EC29E8E-D18A-4FBF-A188-9342E28C7643", "A Transport Document Reference must be entered either at Bill or Item level with at least one of the following Full Type codes: 'N235', 'N271', 'N703', 'N704', 'N705', 'N710', 'N714', 'N720', 'N722', 'N730', 'N740', 'N741', 'N750', 'N760', 'N785', 'N787', 'N952', 'N955'."));

		#endregion

		#region BR2010

		public string BR2010RuleCode => "BR2010";

		public string GetBR2010RuleMessage(ZPropertyInfo info) => FormatValidationMessageWithRuleCodePrefix(BR2010RuleCode, GetMandatoryValueRuleMessage(info));

		public string GetBR2010RuleMessage(string message) => FormatValidationMessageWithRuleCodePrefix(BR2010RuleCode, message);

		#endregion

		#region BR2011

		public string BR2011RuleCode => "BR2011";

		protected virtual string GetaBR2011MessageCore() => Res.GetString("AB6C6B43-54FA-4852-B5B7-4DFD40FC76F8", "At least one Previous Document is required when Additional Declaration Type is 'A'.");

		public string GetBR2011RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR2011RuleCode, GetaBR2011MessageCore());

		#endregion

		#region BR2032

		public string BR2032RuleCode => "BR2032";

		protected virtual string GetaBR2032MessageCore() => Res.GetString("4BE117AD-6B7E-4EA2-823E-94DD8376B5E4", "One of the following codes must be declared: D005, D008, N325, N380, N864, N935 when Additional Procedure is not C08.");

		public string GetBR2032RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR2032RuleCode, GetaBR2032MessageCore());

		#endregion

		#region BR2037

		public string BR2037RuleCode => "BR2037";
		protected virtual string GetaBR2037MessageCore() => Res.GetString("D6656521-FD73-4E69-BDA3-F341EEB2A127", "One of the following codes must be declared: D005, D008, N325, N380, N864, N935 when Additional Procedure is not C08.");
		public string GetBR2037RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR2037RuleCode, GetaBR2037MessageCore());

		#endregion

		#region BR2038

		public string BR2038RuleCode => "BR2038";
		protected virtual string GetaBR2038MessageCore() => Res.GetString("E84EEB96-4C0A-47A8-8DD5-B9F1C6213B52", "Reference Number should contain 11 digits when Type is 'N741'.");
		public string GetBR2038RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR2038RuleCode, GetaBR2038MessageCore());

		#endregion

		#region BR2040

		public string BR2040RuleCode => "BR2040";
		protected virtual string GetaBR2040MessageCore() => Res.GetString("99CB32A5-A8A2-4937-8CE0-167A96BB140E", "Invalid Document Type.");
		public string GetBR2040RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR2040RuleCode, GetaBR2040MessageCore());

		#endregion

		#region BR2048

		public string BR2048RuleCode => "BR2048";
		public string GetBR2048RuleMessage(ZPropertyInfo info) => FormatValidationMessageWithRuleCodePrefix(BR2048RuleCode, GetMandatoryValueRuleMessage(info));
		public string GetBR2048RuleMessage(string message) => FormatValidationMessageWithRuleCodePrefix(BR2048RuleCode, message);

		#endregion

		#region BR2316

		public string BR2316RuleCode => "BR2316";
		protected virtual string GetaBR2316MessageCore() => Res.GetString("DBA05F27-FF1C-439C-9B20-B2952CC1C1BC", "Invalid Reference Number. It should be '1' when Type is '1D96'.");
		public string GetBR2316RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR2316RuleCode, GetaBR2316MessageCore());

		#endregion

		#region BR3006

		public string BR3006RuleCode = "BR3006";

		public string GetBR3006RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR3006RuleCode, Res.GetString("c785ba8f-7569-4e0b-b08a-deeb5cf9b53b", "Identification number is mandatory if any of the following fields are empty: Exporter name, address, postcode, city, country."));

		#endregion

		#region BR3181

		public string GetBR3181RulePYEFormatMessage() => FormatValidationMessageWithRuleCodePrefix(BR3181RuleCode, Res.GetString("6e57fc8a-0e4d-4dc2-9162-e8e0f3959cf7", "Invalid PYE format. Should be maximum 9 characters long."));

		public string GetBR3181RuleITXFormatMessage() => FormatValidationMessageWithRuleCodePrefix(BR3181RuleCode, Res.GetString("5c6df53c-ec23-43f2-b48f-3d5345c07e7f", "Invalid ITX format. Should be maximum 9 characters long."));

		public string GetBR3181RuleCGTFormatMessage() => FormatValidationMessageWithRuleCodePrefix(BR3181RuleCode, Res.GetString("6e94f445-4cd9-4104-b1da-af7e58741992", "Invalid CGT format. Should be maximum 9 characters long."));

		#endregion

		#region BR20311

		public string BR20311RuleCode => "BR20311";
		protected virtual string GetaBR20311MessageCore() => Res.GetString("DCE14673-CF6E-4076-BA22-7243DC61A498", "Invalid Reference Number. It should be 'YYYYMMDD'.");
		public string GetBR20311RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR20311RuleCode, GetaBR20311MessageCore());

		#endregion

		#region BR20312

		public string BR20312RuleCode => "BR20312";
		protected virtual string GetaBR20312MessageCore() => Res.GetString("98427256-39E3-4BA1-9DC8-80E075C4D4A8", "Invalid combination of Document Types.");
		public string GetBR20312RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR20312RuleCode, GetaBR20312MessageCore());

		#endregion

		#region BR20313

		public string BR20313RuleCode => "BR20313";

		public string GetBR20313RuleMessage(ZPropertyInfo info) => FormatValidationMessageWithRuleCodePrefix(BR20313RuleCode, MandatoryValidation.YouHaveNotEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(info)));

		#endregion

		#region BR20318

		public string BR20318RuleCode => "BR20318";
		protected virtual string GetaBR20318MessageCore() => Res.GetString("EAB5116B-137A-4E30-B9B4-A2DC3C6DDE3F", "Invalid Certificate Code.");
		public string GetBR20318RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR20318RuleCode, GetaBR20318MessageCore());

		#endregion

		#region BR20319

		public string BR20319RuleCode => "BR20319";

		public string GetBR20319RuleOnlyOneREF1D24Message() => FormatValidationMessageWithRuleCodePrefix(BR20319RuleCode, Res.GetString("4CAC7D4D-B314-4FF3-B8E3-E51C21AA8F74", "There should be only one instance of Additional Reference Type '1D24' per Bill."));

		public string GetBR20319RuleOnlyOneSUP1D24Message() => FormatValidationMessageWithRuleCodePrefix(BR20319RuleCode, Res.GetString("394E0DD6-EC9F-4913-968F-B34F4CD61B35", "There should be only one instance of Supporting Document Type '1D24' per Bill."));

		public string GetBR20319RuleReferenceFormatForREF1D24Message() => FormatValidationMessageWithRuleCodePrefix(BR20319RuleCode, Res.GetString("510B2A4B-1CD5-4EF2-A374-F63B5055AEFA", "Reference must be in format 'yyyyMMddHHmm' when Additional Reference Type is '1D24'."));

		public string GetBR20319RuleReferenceFormatForSUP1D24Message() => FormatValidationMessageWithRuleCodePrefix(BR20319RuleCode, Res.GetString("FFC7802A-CB46-49B3-891B-68F663B37ED0", "Reference must be in format 'yyyyMMddHHmm' when Supporting Document Type is '1D24'."));

		public string GetBR20319Rule1D24REFRequiredMessage() => FormatValidationMessageWithRuleCodePrefix(BR20319RuleCode, Res.GetString("B61A7F1C-1EC5-49F9-9019-19CE2EAF440C", "Additional Reference Type '1D24' is required."));

		public string GetBR20319Rule1D24SUPRequiredMessage() => FormatValidationMessageWithRuleCodePrefix(BR20319RuleCode, Res.GetString("B7D9CEDA-2AAC-40EA-A495-742196241DBD", "Supporting Document Type '1D24' is required."));

		#endregion

		#region BR600002

		public string BR600002RuleCode => "BR600002";

		public string GetBR600002RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR600002RuleCode, Res.GetString("8d9ce722-6848-4540-8ee6-8f61d455e57a", "Additional Procedure' is required and has to contain either of these codes: 'C07' or 'C08'."));

		#endregion

		#region BR600008

		public string BR600008RuleCode => "BR600008";

		public string GetBR600008RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR600008RuleCode, Res.GetString("a9a48709-ec3a-440e-b342-561b4014e3b8", "If Additional Procedure contains the value 'F49', Additional Reference Type must be '1A06'."));

		#endregion

		#region BR600005

		public string BR600005RuleCode => "BR600005";

		public string GetBR600005RuleValuesMustNotExceedEUR45Message() => FormatValidationMessageWithRuleCodePrefix(BR600005RuleCode, Res.GetString("0d69ca6b-7043-4aab-9a62-7ec7c81ed3a8", "Sum of Intrinsic Value (Items), Transport Value and Insurance Value must not exceed EUR 45 when Add. Procedure(s) is C08."));

		public string GetBR600005RuleValuesMustNotExceedEUR150Message() => FormatValidationMessageWithRuleCodePrefix(BR600005RuleCode, Res.GetString("48f374c2-d3c5-4263-a789-02e18286d1b4", "Sum of Intrinsic Value (Items) must not exceed EUR 150 when Add. Procedure(s) contains C07."));

		#endregion

		#region BR600006

		public string BR600006RuleCode => "BR600006";
		protected virtual string GetaBR600006MessageCore() => Res.GetString("A38DFB43-258F-4AD4-80B2-B14B64350622", "Invalid Authorization Type.");
		public string GetBR600006RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR600006RuleCode, GetaBR600006MessageCore());

		#endregion

		#region BR600009

		public string BR600009RuleCode => "BR600009";

		public string GetBR600009RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR600009RuleCode, Res.GetString("0ced54e5-60de-4f77-90ae-8b057cf802d9", "If ‘Seller IOSS Number’ has been provided then ‘Additional Procedure' must contain the value 'F48'."));

		#endregion

		#region BR600014

		public string BR600014RuleCode => "BR600014";

		public string GetBR600014RuleMessage() => FormatValidationMessageWithRuleCodePrefix(BR600014RuleCode, Res.GetString("a33d51ec-0ba2-4726-bb11-e3d1283911dc", "If Additional Reference Type is '1A06' then Additional Procedure must contain the value 'F49'."));

		#endregion

		#region C0614

		public string C0614RuleCode = "C0614";

		public string GetC0614RuleMessage() => FormatValidationMessageWithRuleCodePrefix(C0614RuleCode, Res.GetString("92874EF2-2FB4-4867-B762-F5BFBF30D49B", "At least one Previous Document Reference is required."));

		#endregion

		#region C0617

		public string C0617RuleCode = "C0617";

		public string GetC0617RuleImporterIdentificationNumberMessage() => FormatValidationMessageWithRuleCodePrefix(C0617RuleCode, Res.GetString("66867181-3015-41d3-964c-2fc3938be7d6", "Identification number is mandatory if any of the following fields are empty: Importer name, address, postcode, city, country."));

		public string GetC0617RuleExporterCountryMessage() => FormatValidationMessageWithRuleCodePrefix(C0617RuleCode, Res.GetString("c422d70a-ffb2-411d-95fb-f423f8b6b795", "Please enter an Exporter Country/Region."));

		public string GetC0617RuleExporterStreetMessage() => FormatValidationMessageWithRuleCodePrefix(C0617RuleCode, Res.GetString("6b93bf61-4217-40f2-ad7a-4a52d0e18952", "Please enter an Exporter Street Address."));

		#endregion

		#region C0634

		public string C0634RuleCode => "C0634";
		protected virtual string GetaC0634MessageCore() => Res.GetString("9515CE84-A8B3-4241-A0C6-C32C431091D8", "At least one Transport Document is required for each Item or for the Bill.");
		public string GetC0634RuleMessage() => FormatValidationMessageWithRuleCodePrefix(C0634RuleCode, GetaC0634MessageCore());

		#endregion

		#region C0638

		public string C0638RuleCode => "C0638";

		public string GetC0638RuleMessage(ZPropertyInfo info) => FormatValidationMessageWithRuleCodePrefix(C0638RuleCode, MandatoryValidation.YouHaveNotEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(info)));

		#endregion

		#region CD0185

		public string CD0185RuleCode => "CD0185";
		protected virtual string GetaCD0185MessageCore() => Res.GetString("E5437DBB-CF8E-4CA0-A0F9-9459B3255E30", "Tariff cannot contain codes '3303.00.10 00' or '3303.00.90 00'.");
		public string GetCD0185RuleMessage() => FormatValidationMessageWithRuleCodePrefix(CD0185RuleCode, GetaCD0185MessageCore());

		#endregion

	}
}
