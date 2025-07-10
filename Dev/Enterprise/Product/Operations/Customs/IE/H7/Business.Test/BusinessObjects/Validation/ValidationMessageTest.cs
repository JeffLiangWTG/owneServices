using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	public class ValidationMessageTest : TestCaseWithFactory
	{
		public void TestGetBR1106RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR1106] A Transport Document Reference must be entered either at Bill or Item level with at least one of the following Full Type codes: 'N235', 'N271', 'N703', 'N704', 'N705', 'N710', 'N714', 'N720', 'N722', 'N730', 'N740', 'N741', 'N750', 'N760', 'N785', 'N787', 'N952', 'N955'.", validationMessage.GetBR1106RuleMessage());
		}

		public void TestGetBR2010RuleMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR2010] You have not entered a Rep. Status.", validationMessage.GetBR2010RuleMessage(header.AMA_AgentTypeInfo));
		}

		public void TestGetBR2011RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR2011] At least one Previous Document is required when Additional Declaration Type is 'A'.", validationMessage.GetBR2011RuleMessage());
		}

		public void TestGetBR2032RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR2032] One of the following codes must be declared: D005, D008, N325, N380, N864, N935 when Additional Procedure is not C08.", validationMessage.GetBR2032RuleMessage());
		}

		public void TestGetBR2037RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR2037] One of the following codes must be declared: D005, D008, N325, N380, N864, N935 when Additional Procedure is not C08.", validationMessage.GetBR2037RuleMessage());
		}

		public void TestGetBR2038RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR2038] Reference Number should contain 11 digits when Type is 'N741'.", validationMessage.GetBR2038RuleMessage());
		}

		public void TestGetBR2040RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR2040] Invalid Document Type.", validationMessage.GetBR2040RuleMessage());
		}

		public void TestGetBR2048RuleMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR2048] You have not entered a Rep. Status.", validationMessage.GetBR2048RuleMessage(header.AMA_AgentTypeInfo));
		}

		public void TestGetBR2316RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR2316] Invalid Reference Number. It should be '1' when Type is '1D96'.", validationMessage.GetBR2316RuleMessage());
		}

		public void TestGetBR3006RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR3006] Identification number is mandatory if any of the following fields are empty: Exporter name, address, postcode, city, country.", validationMessage.GetBR3006RuleMessage());
		}

		public void TestGetBR3181RulePYEFormatMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR3181] Invalid PYE format. Should be maximum 9 characters long.", validationMessage.GetBR3181RulePYEFormatMessage());
		}

		public void TestGetBR3181RuleITXFormatMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR3181] Invalid ITX format. Should be maximum 9 characters long.", validationMessage.GetBR3181RuleITXFormatMessage());
		}

		public void TestGetBR3181RuleCGTFormatMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR3181] Invalid CGT format. Should be maximum 9 characters long.", validationMessage.GetBR3181RuleCGTFormatMessage());
		}

		public void TestGetBR20311RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR20311] Invalid Reference Number. It should be 'YYYYMMDD'.", validationMessage.GetBR20311RuleMessage());
		}

		public void TestGetBR20312RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR20312] Invalid combination of Document Types.", validationMessage.GetBR20312RuleMessage());
		}

		public void TestGetBR20313RuleMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR20313] You have not entered a Rep. Status.", validationMessage.GetBR20313RuleMessage(header.AMA_AgentTypeInfo));
		}

		public void TestGetBR20318RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR20318] Invalid Certificate Code.", validationMessage.GetBR20318RuleMessage());
		}

		public void TestGetBR20319RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR20319] There should be only one instance of Additional Reference Type '1D24' per Bill.", validationMessage.GetBR20319RuleOnlyOneREF1D24Message());
			AssertEquals("[BR20319] There should be only one instance of Supporting Document Type '1D24' per Bill.", validationMessage.GetBR20319RuleOnlyOneSUP1D24Message());
			AssertEquals("[BR20319] Reference must be in format 'yyyyMMddHHmm' when Additional Reference Type is '1D24'.", validationMessage.GetBR20319RuleReferenceFormatForREF1D24Message());
			AssertEquals("[BR20319] Reference must be in format 'yyyyMMddHHmm' when Supporting Document Type is '1D24'.", validationMessage.GetBR20319RuleReferenceFormatForSUP1D24Message());
			AssertEquals("[BR20319] Additional Reference Type '1D24' is required.", validationMessage.GetBR20319Rule1D24REFRequiredMessage());
			AssertEquals("[BR20319] Supporting Document Type '1D24' is required.", validationMessage.GetBR20319Rule1D24SUPRequiredMessage());
		}

		public void TestGetBR600002RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR600002] Additional Procedure' is required and has to contain either of these codes: 'C07' or 'C08'.", validationMessage.GetBR600002RuleMessage());
		}

		public void TestGetBR600005RuleValuesMustNotExceedEUR45Message()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR600005] Sum of Intrinsic Value (Items), Transport Value and Insurance Value must not exceed EUR 45 when Add. Procedure(s) is C08.", validationMessage.GetBR600005RuleValuesMustNotExceedEUR45Message());
		}

		public void TestGetBR600005RuleValuesMustNotExceedEUR150Message()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR600005] Sum of Intrinsic Value (Items) must not exceed EUR 150 when Add. Procedure(s) contains C07.", validationMessage.GetBR600005RuleValuesMustNotExceedEUR150Message());
		}

		public void TestGetBR600006RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR600006] Invalid Authorization Type.", validationMessage.GetBR600006RuleMessage());
		}

		public void TestGetBR600008RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR600008] If Additional Procedure contains the value 'F49', Additional Reference Type must be '1A06'.", validationMessage.GetBR600008RuleMessage());
		}

		public void TestGetBR600009RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR600009] If ‘Seller IOSS Number’ has been provided then ‘Additional Procedure' must contain the value 'F48'.", validationMessage.GetBR600009RuleMessage());
		}

		public void TestGetBR600014RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR600014] If Additional Reference Type is '1A06' then Additional Procedure must contain the value 'F49'.", validationMessage.GetBR600014RuleMessage());
		}

		public void TestGetC0614RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[C0614] At least one Previous Document Reference is required.", validationMessage.GetC0614RuleMessage());
		}

		public void TestGetC0617RuleImporterIdentificationNumberMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[C0617] Identification number is mandatory if any of the following fields are empty: Importer name, address, postcode, city, country.", validationMessage.GetC0617RuleImporterIdentificationNumberMessage());
		}
		public void TestGetC0617RuleExporterCountryMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[C0617] Please enter an Exporter Country/Region.", validationMessage.GetC0617RuleExporterCountryMessage());
		}

		public void TestGetC0617RuleExporterStreetMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[C0617] Please enter an Exporter Street Address.", validationMessage.GetC0617RuleExporterStreetMessage());
		}

		public void TestGetC0634RuleMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[C0634] At least one Transport Document is required for each Item or for the Bill.", validationMessage.GetC0634RuleMessage());
		}

		public void TestGetC0638RuleMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var validationMessage = new ValidationMessage();

			AssertEquals("[C0638] You have not entered a Rep. Status.", header.ValidationConfiguration.ValidationMessage.GetC0638RuleMessage(header.AMA_AgentTypeInfo));
		}

		public void TestInvalidRuleMessage()
		{
			var validationMessage = new ValidationMessage();
			AssertEquals("[BR0020] The code you have selected is not in the list.", validationMessage.InvalidValueRuleMessage.ToString());
		}
	}
}
