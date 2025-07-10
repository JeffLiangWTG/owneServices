using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	// Please keep sorted
	public class ValidationRuleMessages
	{
		#region B1823 Rule Code & Message

		public string B1823RuleCode => B1823RuleCodeCore();
		protected virtual string B1823RuleCodeCore() => "B1823";

		public string B1823aMessage => FormatMessage(B1823RuleCode, GetB1823aMessageCore());

		protected virtual string GetB1823aMessageCore() => Res.GetString("7AA5C06E-A409-4560-941F-952E202FFCB5", "Consignee field must be empty");

		public string B1823bMessage => FormatMessage(B1823RuleCode, GetB1823bMessageCore());

		protected virtual string GetB1823bMessageCore() => Res.GetString("DBD08453-4E51-43D0-ADFC-5AE6BABA90DE", "Consignee field must be filled");

		#endregion

		#region B1848 Rule Code & Message

		public string B1848RuleCode => B1848RuleCodeCore();
		protected virtual string B1848RuleCodeCore() => "B1848";

		public string B1848Message => FormatMessage(B1848RuleCode, GetB1848MessageCore());

		protected virtual string GetB1848MessageCore() => Res.GetString("3507CDED-DC12-42AA-90CE-E35A987A38AC", "You have not entered a Country/Region Of Routing.");

		#endregion

		#region B1848_1 Rule Code & Message

		public string B1848_1RuleCode => B1848_1RuleCodeCore();
		protected virtual string B1848_1RuleCodeCore() => "B1848-1";

		public string B1848_1Message => FormatMessage(B1848_1RuleCode, GetB1848_1MessageCore());

		protected virtual string GetB1848_1MessageCore() => Res.GetString("E64D3ED9-47B5-4580-9EB7-ECEE2DB3A4D9", "You need to supply at least 2 records (Country of Dispatch and Destination) in the Country/Region of Routing.");

		#endregion

		#region B1850 Rule Code & Message

		public string B1850RuleCode => B1850RuleCodeCore();
		protected virtual string B1850RuleCodeCore() => "B1850";

		public string B1850Message => FormatMessage(B1850RuleCode, GetB1850MessageCore());

		protected virtual string GetB1850MessageCore() => Res.GetString("6DFA05F2-3B20-4D83-8566-31A24C35A092", "In transition period, which is now, 'Nationality' field must be entered");
		#endregion

		#region B1858 Rule Code & Message
		public string B1858RuleCode => B1858RuleCodeCore();
		protected virtual string B1858RuleCodeCore() => "B1858";

		public string B1858aMessage => FormatMessage(B1858RuleCode, GetB1858aMessageCore());

		protected virtual string GetB1858aMessageCore() => Res.GetString("2BF41993-17C5-4F9E-A7B6-BFA945FFAE69", "Place of Unloading cannot be entered if Security = 'NON'.");

		public string B1858bMessage => FormatMessage(B1858RuleCode, GetB1858bMessageCore());

		protected virtual string GetB1858bMessageCore() => Res.GetString("35046914-F90D-4AA6-89FE-425225936F1F", "Place of Unloading is required when Circumstance is declared as other than XXX.");

		#endregion

		#region B1858_1 Rule Code & Message

		public string B1858_1RuleCode => B1858_1RuleCodeCore();
		protected virtual string B1858_1RuleCodeCore() => "B1858-1";

		public string B1858_1Message(string humanReadableForm) => FormatMessage(B1858_1RuleCode, GetB1858_1MessageCore(humanReadableForm));

		protected virtual string GetB1858_1MessageCore(string humanReadableForm) => Res.GetString("31B9DAD1-E81E-4C71-B227-F84296831064", "Please do not enter a {0}.", humanReadableForm);

		#endregion

		#region B1858_2 Rule Code & Message

		public string B1858_2RuleCode => B1858_2RuleCodeCore();
		protected virtual string B1858_2RuleCodeCore() => "B1858-2";

		public string B1858_2aMessage => FormatMessage(B1858_2RuleCode, GetB1858_2aMessageCore());

		protected virtual string GetB1858_2aMessageCore() => Res.GetString("F8BF023F-05C8-4A32-953D-39E53C3270D6", "Place of Unloading is not transmitted to customs if Security = 'NON'.");

		public string B1858_2bMessage => FormatMessage(B1858_2RuleCode, GetB1858_2bMessageCore());

		protected virtual string GetB1858_2bMessageCore() => GetB1858bMessageCore();

		#endregion

		#region B1877_1 Rule Code & Message

		public string B1877_1RuleCode => B1877_1RuleCodeCore();
		protected virtual string B1877_1RuleCodeCore() => "B1877-1";

		public string B1877_1_1Message => FormatMessage(B1877_1RuleCode, GetB1877_1_1MessageCore());

		protected virtual string GetB1877_1_1MessageCore() =>
			Res.GetString(
				"3A1A7982-2A64-43DD-9638-484771C5F785",
				"At least one Goods item must have a Trans. Chg. MoP with different value");

		public string GetB1877_1_2Message(string attributeDescriptor) => FormatMessage(B1877_1RuleCode, GetB1877_1_2MessageCore(attributeDescriptor));

		protected virtual string GetB1877_1_2MessageCore(string attributeDescriptor) =>
			Res.GetString(
				"D77D22FD-A831-46E2-8ACD-C21BA4FD8763",
				"At least one Goods item must have a consignee with different {0}",
				attributeDescriptor);

		#endregion

		#region B1889 Rule Code & Message

		public string B1889RuleCode => B1889RuleCodeCore();
		protected virtual string B1889RuleCodeCore() => "B1889";

		#endregion

		#region B1891_1 Rule Code & Message

		public string B1891_1RuleCode => B1891_1RuleCodeCore();
		protected virtual string B1891_1RuleCodeCore() => "B1891-1";

		public string B1891_1Message => FormatMessage(B1891_1RuleCode, GetB1891_1Core());

		protected virtual string GetB1891_1Core() => Res.GetString("6D8D1568-B3A1-456B-B281-5E70F46E758A", "You have not entered a Type of Identification for Transport Departure.");

		#endregion

		#region B1892 Rule Code & Message

		public string B1892RuleCode => B1892RuleCodeCore();
		protected virtual string B1892RuleCodeCore() => "B1892";

		public string B1892Message(string fieldName) => FormatMessage(B1892RuleCode, GetB1892MessageCore(fieldName));

		protected virtual string GetB1892MessageCore(string fieldName) => Res.GetString("3CD0019F-A23C-4569-B309-1C2B3B4F9AE0", "In transition period, which is now, if no containers are used '{0}' field must be filled in", fieldName);

		#endregion

		#region B1893 Rule Code & Message

		public string B1893RuleCode => B1893RuleCodeCore();
		protected virtual string B1893RuleCodeCore() => "B1893";

		#endregion

		#region B1893_2 Rule Code & Message

		public string B1893_2RuleCode => B1893_2RuleCodeCore();
		protected virtual string B1893_2RuleCodeCore() => "B1893-2";

		#endregion

		#region B1895-1 Rule Code & Message

		public string B1895_1RuleCode => B1895_1RuleCodeCore();
		protected virtual string B1895_1RuleCodeCore() => "B1895-1";

		public string B1895_1Message => FormatMessage(B1895_1RuleCode, Get1895_1MessageCore());

		protected virtual string Get1895_1MessageCore() => Res.GetString("D0516E77-E39A-4FE7-8BD4-64512F32A2EB", "This field must be empty if Details > 'Ref.No. / UCR' field is filled.");

		#endregion

		#region B1898_1 Rule Code & Message

		public string B1898_1RuleCode => B1898_1RuleCodeCore();
		protected virtual string B1898_1RuleCodeCore() => "B1898-1";

		public string B1898_1Message => FormatMessage(B1898_1RuleCode, Get1898_1MessageCore());

		protected virtual string Get1898_1MessageCore() => Res.GetString("206248B9-5A30-421A-9C7C-177068C4C0B5", "You have not entered a Currency.");

		#endregion

		#region B1922 Rule Code

		public virtual string B1922RuleCode => "B1922";

		#endregion

		#region B2101 Rule Code & Message

		public string B2101RuleCode => B2101RuleCodeCore();
		protected virtual string B2101RuleCodeCore() => "B2101";

		public string B2101CurrencyMessage => FormatMessage(B2101RuleCode, GetPW_RX_NKCurrencyB2101MessageCore());

		protected virtual string GetPW_RX_NKCurrencyB2101MessageCore() => Res.GetString("DCA9D36C-C2BD-43C2-8F61-C12D56F26BC0", "You have not entered a Currency");

		public string B2101GrossWeightMessage => FormatMessage(B2101RuleCode, GetBY_GrossWeightB2101MessageCore());

		protected virtual string GetBY_GrossWeightB2101MessageCore() => Res.GetString("498FC82E-1F77-4D3B-901D-DFDA24542F4A", "Gross Weight must be greater than 0");

		public string B2101GrossWeightUnitMessage => FormatMessage(B2101RuleCode, GetBY_GrossWeightUnitB2101MessageCore());

		protected virtual string GetBY_GrossWeightUnitB2101MessageCore() => Res.GetString("9E8E2F80-62DE-4B76-99B9-114B2205FF82", "You have not entered a Gross Weight Unit");

		public string B2101TypeOfIdMessage => FormatMessage(B2101RuleCode, GetBM_ActiveBorderIdentificationTypeB2101MessageCore());

		protected virtual string GetBM_ActiveBorderIdentificationTypeB2101MessageCore() => Res.GetString("E37E57A2-612E-4AE6-BFEC-035DE0219EA2", "You have not entered a Type of ID");

		public string B2101TransportIdMessage => FormatMessage(B2101RuleCode, GetBM_TOLCarrierIdB2101MessageCore());

		protected virtual string GetBM_TOLCarrierIdB2101MessageCore() => Res.GetString("42B66B92-0200-4D7A-BE71-B05D5C679229", "You have not entered a Transport ID");

		public string B2101NationalityMessage => FormatMessage(B2101RuleCode, GetBM_RN_NKTOLCarrierNationalityB2101MessageCore());

		protected virtual string GetBM_RN_NKTOLCarrierNationalityB2101MessageCore() => Res.GetString("1E923D59-CB71-48D3-9ED7-EC8B75B60D8C", "You have not entered a Nationality");

		public string B2101CustomsOfficeMessage => FormatMessage(B2101RuleCode, GetBM_CustomsOfficeAtBorderB2101MessageCore());

		protected virtual string GetBM_CustomsOfficeAtBorderB2101MessageCore() => Res.GetString("184052B8-581D-4E63-9A59-C151F5682F58", "You have not entered a Customs Office");

		#endregion

		#region BR5410 Rule Code & Message

		public string BR5410RuleCode => BR5410RuleCodeCore();
		protected virtual string BR5410RuleCodeCore() => "BR5410";

		public string BR5410Message => FormatMessage(BR5410RuleCode, GetBR5410MessageCore());

		protected virtual string GetBR5410MessageCore() => Res.GetString("FE579669-62EB-4415-B08E-BED527A2C942", "Security cannot be 'ENT' or 'BTH'");

		#endregion

		#region C0001_2 Rule Code & Message

		public string C0001_2RuleCode => C0001_2RuleCodeCore();
		protected virtual string C0001_2RuleCodeCore() => "C0001-2";

		public string C0001_2aMessage => FormatMessage(C0001_2RuleCode, GetC0001_2aMessageCore());

		protected virtual string GetC0001_2aMessageCore() => Res.GetString("C3F43060-8696-4731-B195-A0188E2FEF09", "When already present in Declaration, Consignee must not be specified in House Consignment.");

		public string C0001_2bMessage => FormatMessage(C0001_2RuleCode, GetC0001_2bMessageCore());

		protected virtual string GetC0001_2bMessageCore() => Res.GetString("F9ADA98D-A1F3-49D8-B990-5B75060E0C64", "You have not entered Consignee. It is required either on Declaration or House Consignment.");

		#endregion

		#region C0001_3 Rule Code & Message

		public string C0001_3RuleCode => C0001_3RuleCodeCore();
		protected virtual string C0001_3RuleCodeCore() => "C0001-3";

		public string C0001_3aMessage => FormatMessage(C0001_3RuleCode, GetC0001_3aMessageCore());

		protected virtual string GetC0001_3aMessageCore() => GetC0001_2aMessageCore();

		public string C0001_3bMessage => FormatMessage(C0001_3RuleCode, GetC0001_3bMessageCore());

		protected virtual string GetC0001_3bMessageCore() => GetC0001_2bMessageCore();

		#endregion

		#region C0001_4 Rule Code & Message

		public string C0001_4RuleCode => C0001_4RuleCodeCore();
		protected virtual string C0001_4RuleCodeCore() => "C0001-4";

		public string C0001_4Message => FormatMessage(C0001_4RuleCode, GetC0001_4MessageCore());

		protected virtual string GetC0001_4MessageCore() => Res.GetString("9A3A3B63-53EE-4770-9E4A-5CFE9FD44808", "Consignee must be empty");

		#endregion

		#region C0001_6 Rule Code & Message

		public string C0001_6RuleCode => C0001_6RuleCodeCore();
		protected virtual string C0001_6RuleCodeCore() => "C0001-6";

		public string C0001_6Message => FormatMessage(C0001_6RuleCode, GetC0001_6MessageCore());

		protected virtual string GetC0001_6MessageCore() => Res.GetString("C9B64997-1ACF-40F1-A8DD-5575D5B53214", "This field will not be written in the message");

		#endregion

		#region C0001_7 Rule Code & Message

		public string C0001_7RuleCode => C0001_7RuleCodeCore();
		protected virtual string C0001_7RuleCodeCore() => "C0001-7";

		public string C0001_7Message => FormatMessage(C0001_7RuleCode, GetC0001_7MessageCore());

		protected virtual string GetC0001_7MessageCore() => Res.GetString("76a8de24-f86b-4120-a08a-1ea12212ce30", "You have not entered Consignee. It is required either on Declaration or House Consignment");

		#endregion

		#region C0015 Rule Code & Message

		public string C0015RuleCode => C0015RuleCodeCore();
		protected virtual string C0015RuleCodeCore() => "C0015";

		public string C0015Message => FormatMessage(C0015RuleCode, GetC0015MessageCore());

		protected virtual string GetC0015MessageCore() => Res.GetString("82546593-73BB-4375-877B-B315E97349D7", "You have not entered a Reference Number for the additional excise document.");

		#endregion

		#region C0035_1 Rule Code & Message

		public string C0035_1RuleCode => C0035_1RuleCodeCore();
		protected virtual string C0035_1RuleCodeCore() => "C0035-1";

		public string C0035_1Message => FormatMessage(C0035_1RuleCode, GetC0035_1MessageCore());

		protected virtual string GetC0035_1MessageCore() => Res.GetString("FD91A30C-812C-491C-93C1-62FE2F9D23E2", "Previous Documents are required either at Consignment or Consignment Item level when Declaration Type is T2 or T2F.");

		#endregion

		#region C0045 Rule Code & Message

		public string C0045RuleCode => C0045RuleCodeCore();
		protected virtual string C0045RuleCodeCore() => "C0045";

		public string C0045aMessage => FormatMessage(C0045RuleCode, GetC0045aMessageCore());

		protected virtual string GetC0045aMessageCore() => Res.GetString("4D24B2EC-ADE4-4B0A-9051-8CDBB5E6A0D2", "Declaration Type ‘T’ on Header level requires Declaration Type on Item level.");

		public string C0045bMessage => FormatMessage(C0045RuleCode, GetC0045bMessageCore());

		protected virtual string GetC0045bMessageCore() => Res.GetString("C9639B7C-730C-4363-8655-82819E52226E", "Declaration Types for all items must be left empty if Declaration Type at header is not 'T'.");

		#endregion

		#region C0055 Rule Code & Message

		public string C0055RuleCode => C0055RuleCodeCore();
		protected virtual string C0055RuleCodeCore() => "C0055";

		public string C0055Message => FormatMessage(C0055RuleCode, C0055MessageCore());

		protected virtual string C0055MessageCore() => Res.GetString("0ccbfdf4-8f42-4dd9-8445-910c70c2e781", "Container/Equipment Number not required when Container Mode is NCT.");

		#endregion

		#region C0060 Rule Code & Message

		public string C0060RuleCode => C0060RuleCodeCore();
		protected virtual string C0060RuleCodeCore() => "C0060";

		public string C0060Message => FormatMessage(C0060RuleCode, GetC0060MessageCore());

		protected virtual string GetC0060MessageCore() => Res.GetString("B9169E6B-E0FD-42F9-8E56-E37B5C0C66EE", "You have not entered a Number of Packages - valid for goods in shared packaging.");

		#endregion

		#region C0060-1 Rule Code & Message

		public string C0060_1RuleCode => C0060_1RuleCodeCore();
		protected virtual string C0060_1RuleCodeCore() => "C0060-1";

		public string C0060_1Message => FormatMessage(C0060_1RuleCode, GetC0060_1MessageCore());

		protected virtual string GetC0060_1MessageCore() => Res.GetString("975A4A25-F7F9-49CF-B0D5-3EDE1EB5CC69", "You have entered a non-countable Package Type.");

		#endregion

		#region C0060-2 Rule Code & Message

		public string C0060_2RuleCode => C0060_2RuleCodeCore();
		protected virtual string C0060_2RuleCodeCore() => "C0060-2";

		#endregion

		#region C0060-3 Rule Code & Message

		public string C0060_3RuleCode => C0060_3RuleCodeCore();
		protected virtual string C0060_3RuleCodeCore() => "C0060-3";

		#endregion

		#region C0065 Rule Code & Message

		public string C0065RuleCode => C0065RuleCodeCore();
		protected virtual string C0065RuleCodeCore() => "C0065";

		#endregion

		#region C0085 Rule Code & Message

		public string C0085RuleCode => C0085RuleCodeCore();
		protected virtual string C0085RuleCodeCore() => "C0085";

		public string C0085Message => FormatMessage(C0085RuleCode, GetC0085MessageCore());

		protected virtual string GetC0085MessageCore() => Res.GetString("6BFE00A1-4541-4335-9065-E119E85CF7C4", "You have not entered a Guarantee Reference Number (GRN).");

		#endregion

		#region C0085_1 Rule Code & Message

		public string C0085_1RuleCode => C0085_1RuleCodeCore();
		protected virtual string C0085_1RuleCodeCore() => "C0085-1";

		public string C0085_1Message => FormatMessage(C0085_1RuleCode, GetC0085_1MessageCore());

		protected virtual string GetC0085_1MessageCore() => GetC0085MessageCore();

		#endregion

		#region C0086_1 Rule Code & Message

		public string C0086_1RuleCode => C0086_1RuleCodeCore();
		protected virtual string C0086_1RuleCodeCore() => "C0086-1";

		public string C0086_1aMessage => FormatMessage(C0086_1RuleCode, GetC0086_1aMessageCore());

		protected virtual string GetC0086_1aMessageCore() => Res.GetString("A99933BA-796B-4782-9D80-99BD47551C32", "You have not entered a Access code (GAC).");

		public string C0086_1bMessage => FormatMessage(C0086_1RuleCode, GetC0086_1bMessageCore());

		protected virtual string GetC0086_1bMessageCore() => Res.GetString("D9C6D0B2-59A9-435B-B4D7-FFC647E53F5C", "Access code (GAC) must not be entered if Type is not 1 or 2.");

		#endregion

		#region C0101_1 Rule Code & Message

		public string C0101_1RuleCode => C0101_1RuleCodeCore();
		protected virtual string C0101_1RuleCodeCore() => (NoResString)"C0101, R0859";

		public string C0101_1RuleCodeMessage(string code, string description) => FormatMessage(C0101_1RuleCode, C0101_1RuleCodeMessageCore(code, description));
		protected virtual string C0101_1RuleCodeMessageCore(string code, string description) => Res.GetString("258FFDE8-2B95-4F9A-9A4F-FD74B7D0C7F1", "Reduced Dataset Indicator to be true requires an Authorization of Type '{0}' ({1}).", code, description);

		#endregion

		#region C0153_1 Rule Code

		public string C0153_1RuleCode => C0153_1RuleCodeCore();
		protected virtual string C0153_1RuleCodeCore() => "C0153-1";

		#endregion

		#region C0191_1 Rule Code & Message

		public string C0191_1RuleCode => C0191_1RuleCodeCore();
		protected virtual string C0191_1RuleCodeCore() => "C0191-1";

		public string C0191_1aMessage => FormatMessage(C0191_1RuleCode, GetC0191_1aMessageCore());

		protected virtual string GetC0191_1aMessageCore() => Res.GetString("BF8017CC-1942-4375-A742-5B6F0B465B30", "Place of Unloading is not transmitted to customs if Security = NON");

		public string C0191_1bMessage => FormatMessage(C0191_1RuleCode, GetC0191_1bMessageCore());

		protected virtual string GetC0191_1bMessageCore() => Res.GetString("2FF69B44-D7DC-4EE4-8ED0-9CE0E2B75732", "You have not entered a Country/Region Code or an UNLOCO for Place of Unloading");

		public string C0191_1cMessage => FormatMessage(C0191_1RuleCode, GetC0191_1cMessageCore());

		protected virtual string GetC0191_1cMessageCore() => Res.GetString("C08E3C64-653F-45A1-A198-39DC4F75F7C6", "Country/Region Code/UNLOCO is not transmitted to customs if Security = NON");

		#endregion

		#region C0191_2 Rule Code

		public string C0191_2RuleCode => C0191_2RuleCodeCore();
		protected virtual string C0191_2RuleCodeCore() => "C0191-2";

		#endregion

		#region C0298 Rule Code & Message

		public string C0298RuleCode => C0298RuleCodeCore();
		protected virtual string C0298RuleCodeCore() => "C0298";

		public string C0298Message => FormatMessage(C0298RuleCode, GetC0298MessageCore());
		protected virtual string GetC0298MessageCore() => Res.GetString("0E23C78A-DE26-4715-9EC0-9A3D1A5CB8F7", "You have not entered a Unit Qty.");

		#endregion

		#region C0337-2 Rule Code & Message

		public string C0337_2RuleCode => C0337_2RuleCodeCore();
		protected virtual string C0337_2RuleCodeCore() => "C0337-2";

		public string C0337_2Message => FormatMessage(C0337_2RuleCode, GetC0337_2MessageCore());

		protected virtual string GetC0337_2MessageCore() => Res.GetString("B88C92C5-1681-493E-AE09-040496661895", "Method of payment can be captured at consignment or house consignment level, but not at both levels.");

		#endregion

		#region C0343-1 Rule Code & Message

		public string C0343_1RuleCode => C0343_1RuleCodeCore();
		protected virtual string C0343_1RuleCodeCore() => "C0343-1";

		public string C0343_1aMessage => FormatMessage(C0343_1RuleCode, GetC0343_1aMessageCore());

		protected virtual string GetC0343_1aMessageCore() => Res.GetString("EB167306-23E2-454A-B18B-C77248C8438C", "Destination Country/Region must be filled either on Declaration or Goods Item Level.");

		public string C0343_1bMessage => FormatMessage(C0343_1RuleCode, GetC0343_1bMessageCore());

		protected virtual string GetC0343_1bMessageCore() => Res.GetString("88549FC2-2B74-4FC6-960F-5D2165A19C94", "Destination Country/Region must be filled either on Declaration, House Consignment or Goods Item Level.");

		#endregion

		#region C0343-2 Rule Code & Message

		public string C0343_2RuleCode => C0343_2RuleCodeCore();
		protected virtual string C0343_2RuleCodeCore() => "C0343-2";

		public string C0343_2Message => FormatMessage(C0343_2RuleCode, GetC0343_2MessageCore());

		protected virtual string GetC0343_2MessageCore() => GetC0343_1bMessageCore();

		#endregion

		#region C0343-3 Rule Code

		public string C0343_3RuleCode => C0343_3RuleCodeCore();
		protected virtual string C0343_3RuleCodeCore() => "C0343-3";

		#endregion

		#region C0387 Rule Code

		public string C0387RuleCode => C0387RuleCodeCore();
		protected virtual string C0387RuleCodeCore() => "C0387";

		#endregion

		#region C0403 Rule Code

		public string C0403RuleCode => C0403RuleCodeCore();
		protected virtual string C0403RuleCodeCore() => "C0403";

		#endregion

		#region C0403-1 Rule Code

		public string C0403_1RuleCode => C0403_1RuleCodeCore();
		protected virtual string C0403_1RuleCodeCore() => "C0403-1";

		#endregion

		#region C0502 Rule Code & Message

		public string C0502RuleCode => C0502RuleCodeCore();
		protected virtual string C0502RuleCodeCore() => "C0502";

		public string C0502Message => FormatMessage(C0502RuleCode, GetC0502MessageCore());

		protected virtual string GetC0502MessageCore() => Res.GetString("DFC749A4-7A1C-4F94-A5B6-F5A66F7C89CC", "You have not entered a Reference Number / UCR. A Reference Number / UCR at Goods Items level is required if there is no Reference Number / UCR at Header or House Consignments level and there is no Transport Document at Header or House Consignments level.");

		#endregion

		#region C0531 Rule Code & Message

		public string C0531RuleCode => C0531RuleCodeCore();
		protected virtual string C0531RuleCodeCore() => "C0531";

		#endregion

		#region C0586 Rule Code & Message

		public string C0586RuleCode => C0586RuleCodeCore();
		protected virtual string C0586RuleCodeCore() => "C0586";

		public string C0586Message => FormatMessage(C0586RuleCode, GetC0586MessageCore());

		protected virtual string GetC0586MessageCore() => Res.GetString("8C761664-6A09-4D14-8DE1-1D199C269AAD", "You have not entered a Country/Region Of Routing.");

		#endregion

		#region C0599 Rule Code & Message

		public string C0599RuleCode => C0599RuleCodeCore();
		protected virtual string C0599RuleCodeCore() => "C0599";

		public string C0599_1RuleCode => C0599_1RuleCodeCore();
		protected virtual string C0599_1RuleCodeCore() => "C0599-1";

		public string C0599_2RuleCode => C0599_2RuleCodeCore();
		protected virtual string C0599_2RuleCodeCore() => "C0599-2";

		#endregion

		#region C0670 Rule Code & Message

		public string C0670RuleCode => C0670RuleCodeCore();

		protected virtual string C0670RuleCodeCore() => "C0670";

		public string C0670Message => FormatMessage(C0670RuleCode, GetC0670MessageCore());

		protected virtual string GetC0670MessageCore() => Res.GetString("71551BC2-F4D4-4DD4-A7F1-9C09F140ABA9", "You have not selected at least one Container Number for this package.");

		#endregion

		#region C0806 Rule Code

		public string C0806RuleCode => C0806RuleCodeCore();

		protected virtual string C0806RuleCodeCore() => "C0806";

		#endregion

		#region C0821-1 Rule Code & Message

		public string C0821_1RuleCode => C0821_1RuleCodeCore();
		protected virtual string C0821_1RuleCodeCore() => "C0821-1";

		public string C0821_1Message => FormatMessage(C0821_1RuleCode, GetC0821_1MessageCore());

		protected virtual string GetC0821_1MessageCore() => Res.GetString("7F2F30CB-C2A3-4DE2-9306-667A603C2C04", "For the 'Departure Office' that has been used, only the first 6 characters of the 'Commodity Code' will be entered in the Message.");

		#endregion

		#region C0837-1 Rule Code & Message

		public string C0837_1RuleCode => C0837_1RuleCodeCore();
		protected virtual string C0837_1RuleCodeCore() => "C0837-1";

		public string C0837_1Message => FormatMessage(C0837_1RuleCode, GetC0837_1MessageCore());

		protected virtual string GetC0837_1MessageCore() =>
			Res.GetString(
				"808913F5-842C-4640-B319-CACD3929D0C7",
				"Net weight must be empty");

		#endregion

		#region C0839-1 Rule Code

		public string C0839_1RuleCode => C0839_1RuleCodeCore();
		protected virtual string C0839_1RuleCodeCore() => "C0839-1";

		#endregion

		#region C0909-1 Rule Code & Message

		public string C0909_1RuleCode => C0909_1RuleCodeCore();
		protected virtual string C0909_1RuleCodeCore() => "C0909-1";

		public string C0909_1aMessage => FormatMessage(C0909_1RuleCode, GetC0909_1aMessageCore());
		public string C0909_1bMessage => FormatMessage(C0909_1RuleCode, GetC0909_1bMessageCore());

		protected virtual string GetC0909_1aMessageCore() => Res.GetString("07526FE8-A572-4A1A-91B6-34E055346483", "Dispatch Country/Region must be filled either on Declaration, House Consignment or Goods Item Level.");
		protected virtual string GetC0909_1bMessageCore() => Res.GetString("0F0E8463-8791-4D57-AD1A-C71C0D230A50", "Dispatch Country/Region must be filled either on Declaration or Goods Item Level.");

		#endregion

		#region CN839 Rule Code & Message

		public string CN839RuleCode => CN839RuleCodeCore();
		protected virtual string CN839RuleCodeCore() => "CN839";

		public string GetCN839Message() => FormatMessage(CN839RuleCode, GetCN839MessageCore());
		protected virtual string GetCN839MessageCore() => Res.GetString("2FA497DA-F389-4900-B6C3-596817445278", "Date Limit must be filled");

		#endregion

		#region E1107 Rule Code

		public string E1107RuleCode => E1107RuleCodeCore();
		protected virtual string E1107RuleCodeCore() => "E1107";

		#endregion

		#region E1109 E1109-1 Rule Codes

		public string E1109RuleCode => E1109RuleCodeCore();
		protected virtual string E1109RuleCodeCore() => "E1109";
		public string E1109_1RuleCode => E1109_1RuleCodeCore();
		protected virtual string E1109_1RuleCodeCore() => "E1109-1";

		public string E1109Message(string propertyDescription) => FormatMessage(E1109RuleCode, GetE1109MessageCore(propertyDescription));

		public string E1109_1Message(string propertyDescription) => FormatMessage(E1109_1RuleCode, GetE1109MessageCore(propertyDescription));

		protected virtual string GetE1109MessageCore(string propertyDescription) => Res.GetString("3E89E384-3EBB-4B57-BE93-0C063F9B1A7F", "Entered {0} exceeding the max value supported Inside Transition Period (Total 11 digits with maximum of 3 decimals).", propertyDescription);

		#endregion

		#region E1301 Rule Code

		public string E1301RuleCode => E1301RuleCodeCore();
		protected virtual string E1301RuleCodeCore() => "E1301";

		public string E1301Message(string documentDescription) => FormatMessage(E1301RuleCode, E1301MessageCore(documentDescription));
		protected virtual string E1301MessageCore(string documentDescription) => Res.GetString("EF1646DF-15EA-4889-830C-E0936E89DBEC", "In transition period, which is now, {0} must be empty", documentDescription);

		#endregion

		#region E1401-1 Rule Code

		public string E1401_1RuleCode => E1401_1RuleCodeCore();
		protected virtual string E1401_1RuleCodeCore() => "E1401-1";

		#endregion

		#region E1406 Rule Code

		public string E1406RuleCode => E1406RuleCodeCore();
		protected virtual string E1406RuleCodeCore() => "E1406";

		public string E1406Message => FormatMessage(E1406RuleCode, GetE1406MessageCore());

		protected virtual string GetE1406MessageCore() => Res.GetString("8CD3E08C-C9C5-46E9-9583-5B79B62882B2", "In transition period, which is now, a maximum of 1 Dangerous Goods Code can be entered");

		#endregion

		#region G0001_1 Rule Code & Message

		public string G0001_1RuleCode => G0001_1RuleCodeCore();
		protected virtual string G0001_1RuleCodeCore() => "G0001-1";

		public string G0001_1Message => FormatMessage(G0001_1RuleCode, GetG0001_1MessageCore());

		protected virtual string GetG0001_1MessageCore() => Res.GetString("C1859A13-28C4-4BD3-BD42-DED58423E65A", "Consignee must be empty");

		#endregion

		#region G0321 Rule Code & Message

		public string G0321RuleCode => G0321RuleCodeCore();
		protected virtual string G0321RuleCodeCore() => "G0321";

		public string G0321Message => FormatMessage(G0321RuleCode, GetG0321MessageCore());

		protected virtual string GetG0321MessageCore() => Res.GetString("C1859A13-28C4-4BD3-BD42-DED58423E65B", "You have not entered a Reference Number - '0' will be used.");

		#endregion

		#region G0789-1 Rule Code & Message

		public string G0789_1RuleCode => G0789_1RuleCodeCore();
		protected virtual string G0789_1RuleCodeCore() => "G0789-1";

		public string G0789_1Message => FormatMessage(G0789_1RuleCode, GetG0789_1MessageCore());

		protected virtual string GetG0789_1MessageCore() => Res.GetString("DEB194FD-DEC2-46FF-836D-2878CE022192", "Customs Office at Border should be the same as Customs Office of Transit, or Customs office of Exit, or Customs Office of Destination.");

		#endregion

		#region N0002 Rule Code & Message

		public string N0002RuleCode => N0002RuleCodeCore();
		protected virtual string N0002RuleCodeCore() => "N0002";

		public string NR0002aMessage(string fieldName) => FormatMessage(N0002RuleCode, GetN0002aMessageMessageCore(fieldName));
		protected virtual string GetN0002aMessageMessageCore(string fieldName) => Res.GetString("4BB31A5E-2E85-4673-AF9A-DFE62D0ACBAA", "{0}: This field must be empty.", fieldName);

		public string NR0002bMessage(string fieldName) => FormatMessage(N0002RuleCode, GetN0002bMessageMessageCore(fieldName));
		protected virtual string GetN0002bMessageMessageCore(string fieldName) => Res.GetString("2176E437-777D-4501-9D52-6016E4266FB5", "{0}: The type of AEO authorization of the Principal cannot be determined. For AEOC type authorization, with the presence of the code 66YY in the Supporting documents, this field should not be filled.", fieldName);

		#endregion

		#region N0003 Rule Code & Message

		public string N0003RuleCode => N0003RuleCodeCore();
		protected virtual string N0003RuleCodeCore() => "N0003";

		public string GetN0003Message() => FormatMessage(N0003RuleCode, GetN0003MessageCore());
		protected virtual string GetN0003MessageCore() => Res.GetString("341975A2-32F3-4A9C-BE34-7BBA37F389FD", "Supporting Document Code 67YY has been entered, therefore Seals must not be entered.");

		#endregion

		#region NR0003 Rule Code & Message

		public string NR0003RuleCode => NR0003RuleCodeCore();
		protected virtual string NR0003RuleCodeCore() => "NR0003";

		public string NR0003Message => FormatMessage(NR0003RuleCode, GetNR0003MessageMessageCore());

		protected virtual string GetNR0003MessageMessageCore() => Res.GetString("54FEDE2B-8BEF-4683-A3BB-660F992CDD77", "Pack Quantity > 0 in Combination with an Inner Pack on the same Goods Item is not allowed.");

		#endregion

		#region NR0005 Rule Code & Message

		public string NR0005RuleCode => NR0005RuleCodeCore();
		protected virtual string NR0005RuleCodeCore() => "NR0005";

		public string NR0005Message => FormatMessage(NR0005RuleCode, GetNR0005MessageMessageCore());

		protected virtual string GetNR0005MessageMessageCore() => Res.GetString("1F854369-AB4B-490F-976B-32B3ADE25D33", "The entered access code must be linked to an active contact name of the representative.");

		#endregion

		#region NR0018 Rule Code & Message

		public string NR0018RuleCode => NR0018RuleCodeCore();
		protected virtual string NR0018RuleCodeCore() => "NR0018";

		public string NR0018Message => FormatMessage(NR0018RuleCode, GetNR0018MessageMessageCore());

		protected virtual string GetNR0018MessageMessageCore() => Res.GetString("0AF57A6C-06CE-40A6-BFBB-1171EFCD5B44", "Destination Country is outside the European Security Zone.");

		#endregion

		#region NR0020 Rule Code & Message

		public string NR0020RuleCode => NR0020RuleCodeCore();
		protected virtual string NR0020RuleCodeCore() => "NR0020";

		public string NR0020Message => FormatMessage(NR0020RuleCode, GetNR0020MessageMessageCore());

		protected virtual string GetNR0020MessageMessageCore() => Res.GetString("EC78B290-4C1A-4A43-BE36-C2C6F04AA6FF", "Gross Weight must be greater than 0 if the Goods Item has a Package with Quantity greater than 0 or the Package Type Bulk.");

		#endregion

		#region NR0026 Rule Code & Message

		public string NR0026RuleCode => NR0026RuleCodeCore();
		protected virtual string NR0026RuleCodeCore() => "NR0026";

		public string NR0026Message => FormatMessage(NR0026RuleCode, GetNR0026MessageCore());

		protected virtual string GetNR0026MessageCore() => Res.GetString("88ECA6F7-21CD-4780-90A8-4930A3B90A79", "If Unloaded Cargo conforms to declaration is ticked, then Seals State Valid must be Yes or empty.");

		#endregion

		#region NR0027 Rule Code & Message

		public string NR0027RuleCode => NR0027RuleCodeCore();
		protected virtual string NR0027RuleCodeCore() => "NR0027";

		public string NR0027Message => FormatMessage(NR0027RuleCode, GetNR0027MessageCore());

		protected virtual string GetNR0027MessageCore() => Res.GetString("BDB164CC-205E-439E-8B37-9ACC822B0E50", "Number of packages can be 0 on all the package lines of the Goods Item or on none.");

		#endregion

		#region NR0028 Rule Code & Message

		public string NR0028RuleCode => NR0028RuleCodeCore();
		protected virtual string NR0028RuleCodeCore() => "NR0028";

		public string NR0028Message => FormatMessage(NR0028RuleCode, GetNR0028MessageCore());

		protected virtual string GetNR0028MessageCore() => Res.GetString("212FB1C8-902F-48DE-8923-B4D75590D439", @"If the 'Seals State Valid' is N then the 'Unloaded cargo conforms to declaration' may not be selected.");

		#endregion

		#region NR0029 Rule Code & Message

		public string NR0029RuleCode => NR0029RuleCodeCore();
		protected virtual string NR0029RuleCodeCore() => "NR0029";

		public string GetNR0029aMessage() => FormatMessage(NR0029RuleCode, GetNR0029aMessageCore());
		protected virtual string GetNR0029aMessageCore() => Res.GetString("4DF5DA9B-D13B-4094-B3CD-5ED69F6EB167", "If Container Unloaded State is NEW, at least one Seal is required.");

		public string GetNR0029bMessage() => FormatMessage(NR0029RuleCode, GetNR0029bMessageCore());
		protected virtual string GetNR0029bMessageCore() => Res.GetString("F5FC809C-E381-4285-8CF3-78C67899151E", "If Seal Unloaded State is NEW, Seal Number cannot be empty.");

		public string GetNR0029cMessage() => FormatMessage(NR0029RuleCode, GetNR0029cMessageCore());
		protected virtual string GetNR0029cMessageCore() => Res.GetString("19FB4A7A-2DD2-4D1C-A200-38CE12D9F3D0", "If Package Unloaded State is NEW, Package Type cannot be empty.");

		public string GetNR0029dMessage() => FormatMessage(NR0029RuleCode, GetNR0029dMessageCore());
		protected virtual string GetNR0029dMessageCore() => Res.GetString("25D8AF52-5AA4-4285-AAF6-D6DBC2A7B7A8", "If Goods Item Unloaded State is NEW, Goods Description cannot be empty.");

		public string GetNR0029eMessage() => FormatMessage(NR0029RuleCode, GetNR0029eMessageCore());
		protected virtual string GetNR0029eMessageCore() => Res.GetString("C2BF404E-3825-40A7-8E95-ADFB48805F2F", "If Goods Item Unloaded State is NEW, at least one Package is required.");

		#endregion

		#region NR0031 Rule Code & Message

		public string NR0031RuleCode => NR0031RuleCodeCore();
		protected virtual string NR0031RuleCodeCore() => "NR0031";

		#endregion

		#region NR0035 Rule Code & Message

		public string NR0035RuleCode => NR0035RuleCodeCore();
		protected virtual string NR0035RuleCodeCore() => "NR0035";

		#endregion

		#region NR0036 Rule Code & Message

		public string NR0036RuleCode => NR0036RuleCodeCore();
		protected virtual string NR0036RuleCodeCore() => "NR0036";

		public string NR0036Message => FormatMessage(NR0036RuleCode, GetNR0036MessageCore);

		protected virtual string GetNR0036MessageCore => Res.GetString("B7D322B5-2399-4582-B5FD-4EF3BC1B0AE6", "You have not entered a Country/Region Code or an UNLOCO for Place of Loading.");

		#endregion

		#region NR0037 Rule Code & Message

		public string NR0037RuleCode => NR0037RuleCodeCore();
		protected virtual string NR0037RuleCodeCore() => "NR0037";

		public string NR0037Message => FormatMessage(NR0037RuleCode, GetNR0037MessageCore);

		protected virtual string GetNR0037MessageCore => Res.GetString("8C0D5B4F-DA37-4420-8D6E-95EF6792CC9C", "You have not entered a Place of Loading.");

		#endregion

		#region NR0038 Rule Code & Message

		public string NR0038RuleCode => NR0038RuleCodeCore();
		protected virtual string NR0038RuleCodeCore() => "NR0038";

		public string NR0038Message => FormatMessage(NR0038RuleCode, GetNR0038MessageCore);

		protected virtual string GetNR0038MessageCore => Res.GetString("B66B3E18-208E-4B38-96A8-2E09B7DC63CB", "You have not entered a Country/Region Code or an UNLOCO for Place of Unloading.");

		#endregion

		#region NR0039 Rule Code & Message

		public string NR0039RuleCode => NR0039RuleCodeCore();
		protected virtual string NR0039RuleCodeCore() => "NR0039";

		public string NR0039Message => FormatMessage(NR0039RuleCode, GetNR0039MessageCore);

		protected virtual string GetNR0039MessageCore => Res.GetString("FDACBB70-DA6B-46F0-8CDE-89DCB17BBC32", "You have not entered a Place of Unloading.");

		#endregion

		#region NR0041 Rule Code & Message

		public string NR0041RuleCode => NR0041RuleCodeCore();
		protected virtual string NR0041RuleCodeCore() => "NR0041";

		public string NR0041Message => FormatMessage(NR0041RuleCode, GetNR0041MessageCore);

		protected virtual string GetNR0041MessageCore => Res.GetString("554B2DA4-64F6-4DFB-A30E-EB5461E89049", "Unit of quantity 'NAR', 'NARB', 'NCL' or 'NPR' requires an Integer value.");

		#endregion

		#region NR0043 Rule Code & Message

		public string NR0043RuleCode => NR0043RuleCodeCore();
		protected virtual string NR0043RuleCodeCore() => "NR0043";

		#endregion

		#region NR0045 Rule Code & Message

		public string NR0045RuleCode => NR0045RuleCodeCore();
		protected virtual string NR0045RuleCodeCore() => "NR0045";

		#endregion

		#region NR0046 Rule Code & Message

		public string NR0046RuleCode => NR0046RuleCodeCore();
		protected virtual string NR0046RuleCodeCore() => "NR0046";

		#endregion

		#region NR0047 Rule Code & Message

		public string NR0047RuleCode => NR0047RuleCodeCore();
		protected virtual string NR0047RuleCodeCore() => "NR0047";

		#endregion

		#region NR0048 Rule Code & Message

		public string NR0048RuleCode => NR0048RuleCodeCore();
		protected virtual string NR0048RuleCodeCore() => "NR0048";

		#endregion

		#region NR0050 Rule Code & Message

		public string NR0050RuleCode => NR0050RuleCodeCore();
		protected virtual string NR0050RuleCodeCore() => "NR0050";

		public string NR0050Message => FormatMessage(NR0050RuleCode, GetNR0050MessageCore());

		protected virtual string GetNR0050MessageCore() => Res.GetString("84BD27C3-E46D-4411-8F40-1CAB42890C44", "You have not entered an Additional Identifier in Location of Goods.");

		#endregion

		#region NR0053 Rule Code & Message

		public string NR0053RuleCode => NR0053RuleCodeCore();
		protected virtual string NR0053RuleCodeCore() => "NR0053";

		public string NR0053Message => FormatMessage(NR0053RuleCode, GetNR0053MessageCore());

		protected virtual string GetNR0053MessageCore() => Res.GetString("0206D800-D215-4164-AED5-97DC510235AA", "When additional declaration type is A, the qualifier of the location must be U and its type must be C. When the additional declaration type is D, the qualifier must be V or U or blank and its type must be A or C or blank.");

		#endregion

		#region NR0055 Rule Code & Message

		public string NR0055RuleCode => NR0055RuleCodeCore();
		protected virtual string NR0055RuleCodeCore() => "NR0055";

		public string NR0055Message => FormatMessage(NR0055RuleCode, GetNR0055MessageCore());

		protected virtual string GetNR0055MessageCore() => Res.GetString("73F692AA-250E-4E1A-B0F0-14375ADC20E8", "Commodity Code must be 8 or 10 digits.");

		#endregion

		#region NR0056 Rule Code & Message

		public string NR0056RuleCode => NR0056RuleCodeCore();
		protected virtual string NR0056RuleCodeCore() => "NR0056";

		public string NR0056Message => FormatMessage(NR0056RuleCode, GetNR0056MessageCore());

		protected virtual string GetNR0056MessageCore() => Res.GetString("AECD4736-84D5-44B2-A932-A5D786825DFB", "A TIR declaration requires a type of document 'N952' in 'Previous documents' of all items.");

		#endregion

		#region NR0058 Rule Code & Message

		public string NR0058RuleCode => NR0058RuleCodeCore();
		protected virtual string NR0058RuleCodeCore() => "NR0058";

		public string NR0058Message => FormatMessage(NR0058RuleCode, GetNR0058MessageCore());

		protected virtual string GetNR0058MessageCore() => Res.GetString("C349D02D-BDEC-4B06-8B5B-608CD4AEDF18", "You have not entered Origin Country so liability amount may not be calculated properly.");

		#endregion

		#region NR0059 Rule Code & Message

		public string NR0059RuleCode => NR0059RuleCodeCore();
		protected virtual string NR0059RuleCodeCore() => "NR0059";

		public string NR0059Message => FormatMessage(NR0059RuleCode, GetNR0059MessageCore());

		protected virtual string GetNR0059MessageCore() => Res.GetString("566E0D12-97B0-4FF2-8E14-BC8906792E1B", "You have not entered Monetary Value so liability amount may not be calculated properly.");

		#endregion

		#region NR0060 Rule Code & Message

		public string NR0060RuleCode => NR0060RuleCodeCore();
		protected virtual string NR0060RuleCodeCore() => "NR0060";

		public string NR0060Message => FormatMessage(NR0060RuleCode, GetNR0060MessageCore());

		protected virtual string GetNR0060MessageCore() => Res.GetString("F813D748-F1CC-47BE-BDE3-257CD0C08DD1", "You have not entered an Additional Code.");

		#endregion

		#region NR0061 Rule Code & Message

		public string NR0061RuleCode => NR0061RuleCodeCore();
		protected virtual string NR0061RuleCodeCore() => "NR0061";

		public string NR0061Message => FormatMessage(NR0061RuleCode, GetNR0061MessageCore());

		protected virtual string GetNR0061MessageCore() => Res.GetString("124D0068-F6E6-4C87-AA95-9203838B46C2", "Gross Weight per package type should be provided if goods are managed in Temporary Storage module. Otherwise, the total Gross Weight will be apportioned among all package types");

		#endregion

		#region NR0062 Rule Code & Message

		public string NR0062RuleCode => NR0062RuleCodeCore();
		protected virtual string NR0062RuleCodeCore() => "NR0062";

		public string NR0062Message(decimal totalGoodsItems, decimal totalPackages) => FormatMessage(NR0062RuleCode, GetNR0062MessageCore(totalGoodsItems, totalPackages));

		protected virtual string GetNR0062MessageCore(decimal totalGoodsItems, decimal totalPackages) => Res.GetString("43B75682-9B92-4561-BC45-3AC42A549728", "The sum of Gross Weight in packages ({0} KG) does not match with the sum of Gross Weight ({1} KG) in Goods Items", totalPackages.ToString(NR0062_DecimalFormat), totalGoodsItems.ToString(NR0062_DecimalFormat));

		const string NR0062_DecimalFormat = "0.000000";

		#endregion

		#region NR0063 Rule Code & Message

		public string NR0063RuleCode => NR0063RuleCodeCore();
		protected virtual string NR0063RuleCodeCore() => "NR0063";

		public string NR0063Message => FormatMessage(NR0063RuleCode, GetNR0063MessageCore());

		protected virtual string GetNR0063MessageCore() => Res.GetString("FEE01371-37BC-4D4C-B68B-2EA4AFD039A6", "When there is an authorization with type ACR, the type of location must be B. If there is no authorization with type ACR, the type of location must be A.");

		#endregion

		#region NR0064 Rule Code & Message

		public string NR0064RuleCode => NR0064RuleCodeCore();

		protected virtual string NR0064RuleCodeCore() => "NR0064";

		#endregion

		#region NR0065 Rule Code & Message

		public string NR0065RuleCode => NR0065RuleCodeCore();

		protected virtual string NR0065RuleCodeCore() => "NR0065";

		#endregion

		#region NR0066 Rule Code & Message

		public string NR0066RuleCode => NR0066RuleCodeCore();

		protected virtual string NR0066RuleCodeCore() => "NR0066";

		public string NR0066Message => FormatMessage(NR0066RuleCode, GetNR0066MessageCore());

		protected virtual string GetNR0066MessageCore() => Res.GetString("6B567566-4915-43B9-A12D-3C1BE3DCAE36", "For Type N830, Goods Item Identifier cannot be zero.");

		#endregion

		#region NR0067 Rule Code & Message

		public string NR0067RuleCode => NR0067RuleCodeCore();

		protected virtual string NR0067RuleCodeCore() => "NR0067";

		public string NR0067Message => FormatMessage(NR0067RuleCode, GetNR0067MessageCore());

		protected virtual string GetNR0067MessageCore() => Res.GetString("8C751E61-AB93-4095-A708-594086FDEC3D", "You have not entered a Liability Amount.");

		#endregion

		#region NR0068 Rule Code & Message

		public virtual string NR0068RuleCode => "NR0068";

		public string NR0068Message => FormatMessage(NR0068RuleCode, GetNR0068MessageCore());

		protected virtual string GetNR0068MessageCore() => Res.GetString("91C667BE-431B-42B6-83F3-8A7BEE9581B1", "Consignor must have EORI number if additional reference code Y022 is used.");

		#endregion

		#region NR0069 Rule Code & Message

		public virtual string NR0069RuleCode => "NR0069";

		public string NR0069Message => FormatMessage(NR0069RuleCode, GetNR0069MessageCore());

		protected virtual string GetNR0069MessageCore() => Res.GetString("806CA004-7A65-4C4F-A740-2438C6469D3F", "Consignee must have EORI number if additional reference code Y023 is used.");

		#endregion

		#region NR0070 Rule Code & Message

		public string NR0070RuleCode => NR0070RuleCodeCore();

		protected virtual string NR0070RuleCodeCore() => "NR0070";

		public string NR0070Message => FormatMessage(NR0070RuleCode, GetNR0070MessageCore());

		protected virtual string GetNR0070MessageCore() => Res.GetString("48A10DD8-AABA-4FEE-B795-091F3723B51B", "Representative must have EORI number if Additional Reference Code Y025 is used.");

		#endregion

		#region NR0071 Rule Code & Message

		public string NR0071RuleCode => NR0071RuleCodeCore();

		protected virtual string NR0071RuleCodeCore() => "NR0071";

		public string NR0071Message => FormatMessage(NR0071RuleCode, GetNR0071MessageCore());

		protected virtual string GetNR0071MessageCore() => Res.GetString("223A7080-98F3-4CCB-951D-9B9D1E2EA7CD", "Principal/Holder of the Transit Procedure must have EORI number if additional reference code Y026 is used.");

		#endregion

		#region NR0072 Rule Code & Message

		public virtual string NR0072RuleCode => "NR0072";

		public string NR0072Message => FormatMessage(NR0072RuleCode, GetNR0072MessageCore());

		protected virtual string GetNR0072MessageCore() => Res.GetString("87850BC2-CEC7-44CA-AF2B-155F4CF724E1", "Carrier must have EORI number if Additional Reference Code Y028 is used.");

		#endregion

		#region NR0076 Rule Code & Message

		public string NR0076RuleCode => NR0076RuleCodeCore();

		protected virtual string NR0076RuleCodeCore() => "NR0076";

		public string NR0076Message => FormatMessage(NR0076RuleCode, GetNR0076MessageCore());

		protected virtual string GetNR0076MessageCore() => Res.GetString("4CF25757-7D41-4C9F-A088-6FFFD0838721", "Unloading date should be equal to or later than the Reception Date");

		#endregion

		#region NR0073 Rule Code & Message

		public virtual string NR0073RuleCode => "NR0073";

		public string NR0073Message => FormatMessage(NR0073RuleCode, GetNR0073MessageCore());

		protected virtual string GetNR0073MessageCore() => Res.GetString("15E2243C-0553-492B-8F0C-4C634E73D190", "Location of Goods must be entered if declaration is not a pre-lodged declaration");

		#endregion

		#region NR0074 Rule Code & Message

		public virtual string NR0074RuleCode => "NR0074";

		public string NR0074Message => FormatMessage(NR0074RuleCode, GetNR0074MessageCore());

		protected virtual string GetNR0074MessageCore() => Res.GetString("10244016-1713-4639-8FC8-2DEAD2848879", "Principal/Holder of the Transit Procedure must have EORI number if Authorization ACR or SSE is used.");

		#endregion

		#region NR0075 Rule Code & Message

		public virtual string NR0075RuleCode => "NR0075";

		public string NR0075Message => FormatMessage(NR0075RuleCode, GetNR0075MessageCore());

		protected virtual string GetNR0075MessageCore() => Res.GetString("B2C5873C-2CA2-4AB0-96F1-249FCC25AE70", "Type of location must be A if authorization ACE nor ACT is used");

		#endregion

		#region NR0078 Rule Code & Message

		public string NR0078RuleCode => NR0078RuleCodeCore();

		protected virtual string NR0078RuleCodeCore() => "NR0078";

		public string NR0078Message(decimal totalGrossWeightOfGoodsItemsInKg) => FormatMessage(NR0078RuleCode, GetNR0078MessageCore(totalGrossWeightOfGoodsItemsInKg));

		protected virtual string GetNR0078MessageCore(decimal totalGrossWeightOfGoodsItemsInKg) => Res.GetString("8DB35729-041F-4D90-BFFF-24CD1DB84323", "Total Gross Weight for House Consignment is different from the total Gross Weight of its Goods Items ({0} Kg)", totalGrossWeightOfGoodsItemsInKg);

		#endregion

		#region NR0079 Rule Code & Message

		public string NR0079RuleCode => NR0079RuleCodeCore();

		protected virtual string NR0079RuleCodeCore() => "NR0079";

		public string NR0079Message => FormatMessage(NR0079RuleCode, GetNR0079MessageCore());

		protected virtual string GetNR0079MessageCore() => Res.GetString("8D30FBEB-099C-4C09-AB41-A011C026FFE0", "Previous Documents will not be included in pre-declarations.");

		#endregion

		#region NR0080 Rule Code & Message

		public string NR0080RuleCode => NR0080RuleCodeCore();
		protected virtual string NR0080RuleCodeCore() => "NR0080";

		public string NR0080Message => FormatMessage(NR0080RuleCode, GetNR0080MessageCore);

		protected virtual string GetNR0080MessageCore => Res.GetString("EDDE02A6-C2DC-43E5-B1A2-799206C3CB0E", "Place of Loading is mandatory except for pre-declarations.");

		#endregion

		#region NR0081 Rule Code & Message

		public string NR0081RuleCode => NR0081RuleCodeCore();
		protected virtual string NR0081RuleCodeCore() => "NR0081";

		public string NR0081Message => FormatMessage(NR0081RuleCode, GetNR0081MessageCore());

		protected virtual string GetNR0081MessageCore() => Res.GetString("DAF36FB3-3ADE-4290-BF69-09E479F37C6F", "If one Transport at Departure is reported as missing, at least one additional transport must be captured with unloaded state 'NEW'.");

		#endregion

		#region NR0082 Rule Code & Message

		public string NR0082RuleCode => NR0082RuleCodeCore();
		protected virtual string NR0082RuleCodeCore() => "NR0082";

		public string NR0082Message => FormatMessage(NR0082RuleCode, GetNR0082MessageCore());
		
		protected virtual string GetNR0082MessageCore() => Res.GetString("FF909709-5A7E-4E1C-9255-94B20A8B9057", "State 'DIF' requires at least one value to be captured that is different to the declared value in the departure declaration. Unchanged columns must remain empty.");

		#endregion

		#region NR0088 Rule Code

		public string NR0088RuleCode => NR0088RuleCodeCore();
		protected virtual string NR0088RuleCodeCore() => "NR0088";

		#endregion

		#region R0006 Rule Code & Message

		public string R0006RuleCode => R0006RuleCodeCore();
		protected virtual string R0006RuleCodeCore() => "R0006";

		public string R0006aMessage => FormatMessage(R0006RuleCode, GetR0006aMessageCore());

		public string R0006bMessage => FormatMessage(R0006RuleCode, GetR0006bMessageCore());

		protected virtual string GetR0006aMessageCore() => Res.GetString("77694388-BDE1-4E30-BFB2-FE4FBF3A5531", "At least one Customs Office of Transit must be from the same country as that of Destination Customs Office.");

		protected virtual string GetR0006bMessageCore() => Res.GetString("54859A32-6B9B-41BD-A314-3BBF93D01EF6", "At least one Customs Office of Transit must be declared belonging to CL010 (Country Codes Community) set of countries.");

		#endregion

		#region R0020-1 Rule Code & Message

		public string R0020_1RuleCode => R0020_1RuleCodeCore();
		protected virtual string R0020_1RuleCodeCore() => "R0020-1";

		public string R0020_1aMessage => FormatMessage(R0020_1RuleCode, GetR0020_1aMessageCore());

		public string R0020_1bMessage => FormatMessage(R0020_1RuleCode, GetR0020_1bMessageCore());

		protected virtual string GetR0020_1aMessageCore() => Res.GetString("587EF6C1-E5B4-4626-AD98-A30C81317FD2", "Previous Document of Type in CL178 is required either at Consignment level or for this Goods Item when Declaration Type for this Goods Item is T2/T2F.");

		protected virtual string GetR0020_1bMessageCore() => Res.GetString("7C46E42D-290B-48EB-8D5A-9CFC833EA6D5", "Previous Document of Type in CL178 is required either at Consignment level or for all Goods Item when the Declaration Type is T2/T2F.");

		#endregion

		#region R0023 Rule Code & Message

		public string R0023RuleCode => R0023RuleCodeCore();
		protected virtual string R0023RuleCodeCore() => "R0023";

		public string R0023Message => FormatMessage(R0023RuleCode, GetR0023MessageCore());

		protected virtual string GetR0023MessageCore() => Res.GetString("795C3ABA-890B-4C9B-B951-D6A04DC90326", "Entered Reference Number '0' is not a valid entry.");

		#endregion

		#region R0103 Rule Code & Message

		public string R0103RuleCode => R0103RuleCodeCore();
		protected virtual string R0103RuleCodeCore() => "R0103";

		public string R0103Message => FormatMessage(R0103RuleCode, GetR0103MessageCore());

		protected virtual string GetR0103MessageCore() => Res.GetString("88AB1862-BB89-4D9F-90AA-8E1006DB1438", @"The declared ""Customs Office of Exit for Transit"" can't be same as the declared ""Customs Office of Transit"" or ""Customs Office of Destination""");

		#endregion

		#region R0219 Rule Code & Message

		public string R0219RuleCode => R0219RuleCodeCore();
		protected virtual string R0219RuleCodeCore() => "R0219";

		public string R0219Message => FormatMessage(R0219RuleCode, GetR0219MessageCore());

		protected virtual string GetR0219MessageCore() => Res.GetString("13353C7D-BB9E-42CA-A9CA-D55B560B9EAB", "Either all pack quantity should be equal to ‘0’ Or all pack quantity should be greater than ‘0’.");

		#endregion

		#region R0223 Rule Code & Message

		public string R0223RuleCode => R0223RuleCodeCore();
		protected virtual string R0223RuleCodeCore() => "R0223";

		public string R0223Message => FormatMessage(R0223RuleCode, GetR0223MessageCore());

		protected virtual string GetR0223MessageCore() => Res.GetString("682A2F81-769C-4F91-BCBF-502892AE9588", "If Gross Weight is not 0, then Net Weight must be less than or equal to Gross Weight");

		#endregion

		#region R0315 Rule Code & Message

		public string R0315RuleCode => R0315RuleCodeCore();
		protected virtual string R0315RuleCodeCore() => "R0315";

		public string R0315aMessage => FormatMessage(R0315RuleCode, GetR0315aMessageCore());

		protected virtual string GetR0315aMessageCore() => Res.GetString("44d683b1-1d22-4735-95a8-4aebbf85a890", "In the Conveyance No., the entered Flight Number should not exceed 8 character length.");

		public string R0315bMessage => FormatMessage(R0315RuleCode, GetR0315bMessageCore());

		protected virtual string GetR0315bMessageCore() => Res.GetString("de7d82f3-281b-4048-8fa8-69d7a16e398c", "In the Conveyance No., the entered Flight Number should not contain non-alphanumeric characters.");

		public string R0315cMessage => FormatMessage(R0315RuleCode, GetR0315cMessageCore());

		protected virtual string GetR0315cMessageCore() => Res.GetString("4e6d0387-6f0d-44c8-bce0-021ed3a5eca6", "In the Conveyance No., the entered Flight Number should not contain lowercase alphabets.");

		#endregion

		#region R0318 Rule Code & Message

		public string R0318RuleCode => R0318RuleCodeCore();
		protected virtual string R0318RuleCodeCore() => "R0318";

		public string R0318aMessage => FormatMessage(R0318RuleCode, GetR0318aMessageCore());

		protected virtual string GetR0318aMessageCore() => Res.GetString("9546A8C7-4BCC-4CA2-BA43-D62C06B140BD", @"Since Guarantee Type is ""4"" the GRN should be declared as alphanumeric data 24 characters long.");

		public string R0318bMessage => FormatMessage(R0318RuleCode, GetR0318bMessageCore());

		protected virtual string GetR0318bMessageCore() => Res.GetString("943467BB-163B-4750-88D4-3FD9EA7FE1AF", @"Since Guarantee Type is other than ""4"" the GRN should be declared as alphanumeric data 17 characters long.");

		#endregion

		#region R0364_2 Rule Code & Message

		public string R0364_2RuleCode => R0364_2RuleCodeCore();
		protected virtual string R0364_2RuleCodeCore() => "R0364-2";

		public string R0364_2Message(string packTypes) => FormatMessage(R0364_2RuleCode, GetR0364_2MessageCore(packTypes));

		protected virtual string GetR0364_2MessageCore(string packTypes) => Res.GetString("ED5DF65E-5D70-427D-B0E6-17D4B43D5E51", "Number of Packages can only be 0 if at least one Package in the House Consignment has been defined having: the number of Packages greater than 0; the same Marks & Number; the type other than {0}.", packTypes);

		#endregion

		#region R0364_3 Rule Code & Message

		public string R0364_3RuleCode => R0364_3RuleCodeCore();
		protected virtual string R0364_3RuleCodeCore() => "R0364-3";

		public string R0364_3Message() => FormatMessage(R0364_3RuleCode, GetR0364_3MessageCore());

		protected virtual string GetR0364_3MessageCore() => Res.GetString("94B89973-A641-4C9F-B674-BAB06F6D4331", "If package type is not bulk, number of packages can be 0 only if at least another package in the House Consignment, with the same Type and Marks & Numbers, has number of packages greater than 0");

		#endregion

		#region R0416 Rule Code & Message

		public string R0416RuleCode => R0416RuleCodeCore();
		protected virtual string R0416RuleCodeCore() => "R0416";

		public string R0416Message => FormatMessage(R0416RuleCode, GetR0416MessageCore());

		protected virtual string GetR0416MessageCore() => Res.GetString("8F7462B2-5167-4DEE-BD04-39EFD433278D", "The 17th character of the house consignment level previous document's reference number must be one of these - A or B or E.");

		#endregion

		#region R0474 Rule Code & Message

		public string R0474RuleCode => R0474RuleCodeCore();
		protected virtual string R0474RuleCodeCore() => "R0474";

		public string R0474Message => FormatMessage(R0474RuleCode, GetR0474MessageCore());

		protected virtual string GetR0474MessageCore() => Res.GetString("D3182852-DA83-443A-A16F-940D3029192F", "This field can be filled only if 'Transport ID' is present");

		#endregion

		#region R0474-1 Rule Code & Message

		public string R0474_1RuleCode => R0474_1RuleCodeCore();
		protected virtual string R0474_1RuleCodeCore() => "R0474-1";

		public string R0474_1Message => FormatMessage(R0474_1RuleCode, GetR0474_1MessageCore());

		protected virtual string GetR0474_1MessageCore() => Res.GetString("53E6F15F-EA9D-474A-89D1-56F69FEE3449", "Transport ID is mandatory if at least a Trailer ID is present.");

		#endregion

		#region R0507-1 Rule Code & Message

		public string R0507_1RuleCode => R0507_1RuleCodeCore();
		protected virtual string R0507_1RuleCodeCore() => "R0507-1";

		public string R0507_1CountryOfDispatchMessage => FormatMessage(R0507_1RuleCode, GetR0507_1CountryOfDispatchMessageCore());

		public string R0507_1CommercialReferenceNumberMessage => FormatMessage(R0507_1RuleCode, GetR0507_1CommercialReferenceNumberMessageCore());

		protected virtual string GetR0507_1CountryOfDispatchMessageCore() => Res.GetString("2827F7AF-40D6-4A84-823D-7F40E1127786", "Country of Dispatch must be different for at least one of the consignment items.");

		protected virtual string GetR0507_1CommercialReferenceNumberMessageCore() => Res.GetString("160BF8A1-1A0A-4471-B1D4-E97CACD26116", "Reference Number (UCR) must be different for at least one of the consignment items.");

		#endregion

		#region R0789 Rule Code & Message

		public string R0789RuleCode => R0789RuleCodeCore();
		protected virtual string R0789RuleCodeCore() => "R0789";

		public string R0789Message => FormatMessage(R0789RuleCode, GetR0789MessageCore());

		protected virtual string GetR0789MessageCore() => Res.GetString("3A97AC96-DF1F-4C7D-A3DF-647383F112DA", "You have not entered Transport Border Details.");

		#endregion

		#region R0789-1 Rule Code & Message

		public string R0789_1RuleCode => R0789_1RuleCodeCore();
		protected virtual string R0789_1RuleCodeCore() => "R0789-1";

		public string R0789_1Message => FormatMessage(R0789_1RuleCode, GetR0789MessageCore());

		#endregion

		#region R0840 Rule Code & Message

		public string R0840RuleCode => R0840RuleCodeCore();
		protected virtual string R0840RuleCodeCore() => "R0840";

		public string R0840Message => FormatMessage(R0840RuleCode, GetR0840MessageCore());

		protected virtual string GetR0840MessageCore() => Res.GetString("a25eea27-e8bf-4f8a-a1ce-b139832c8905", "Format of entered Identification (EORI/TCUIN) is incorrect; please enter in correct format (Nationality Code + National Identification Number i.e. a..2 + an..15)");

		#endregion

		#region R0900 Rule Code & Message

		public string R0900RuleCode => R0900RuleCodeCore();
		protected virtual string R0900RuleCodeCore() => "R0900";

		public string R0900aMessage => FormatMessage(R0900RuleCode, GetR0900aMessageCore());

		protected virtual string GetR0900aMessageCore() => Res.GetString("3BDA38AE-2637-49C8-AB08-9E52FA0B91AC", "A TIR declaration requires a guarantee of type B (TIR).");

		public string R0900bMessage => FormatMessage(R0900RuleCode, GetR0900bMessageCore());

		protected virtual string GetR0900bMessageCore() => Res.GetString("ED3D70DA-FADF-4CAC-A22D-338D558EF871", "A Guarantee of Type from CL230 list must be declared when Country of Customs Office of Departure is either from CL010 list or 'SM' or 'AD'.");

		public string R0900cMessage => FormatMessage(R0900RuleCode, GetR0900cMessageCore());

		protected virtual string GetR0900cMessageCore() => Res.GetString("81B1B47C-6FAA-4D7D-B296-C50614CE2BEE", "A Guarantee of Type from CL229 list must be declared when Country of Customs Office of Departure is not in (set CL010 i.e. Country Code Community, 'SM', 'AD').");

		#endregion

		#region R0911 Rule Code & Message

		public string R0911RuleCode => R0911RuleCodeCore();

		protected virtual string R0911RuleCodeCore() => "R0911";

		public string R0911Message => FormatMessage(R0911RuleCode, GetR0911MessageCore());

		protected virtual string GetR0911MessageCore() => Res.GetString("EDA9C1D8-1078-49ED-BF3D-F2F595EC072D", "Based on Customs Office of Departure and Destination you selected, Declaration Type should be T2 or T2F.");

		#endregion

		#region R0994 Rule Code & Message

		public string R0994RuleCode => R0994RuleCodeCore();

		protected virtual string R0994RuleCodeCore() => "R0994";

		public string R0994Message => FormatMessage(R0994RuleCode, GetR0994MessageCore());

		protected virtual string GetR0994MessageCore() => Res.GetString("44B58C46-2E25-4764-B727-F9699CCFE6F1", "Gross Weight must be equal or greater than the sum of all Gross Weights on House Consignments.");

		#endregion

		#region R0994-1 Rule Code & Message

		public string R0994_1RuleCode => R0994_1RuleCodeCore();

		protected virtual string R0994_1RuleCodeCore() => "R0994-1";

		public string R0994_1Message(string billsTotalGrossMass) => FormatMessage(R0994_1RuleCode, GetR0994_1MessageCore(billsTotalGrossMass));

		protected virtual string GetR0994_1MessageCore(string billsTotalGrossMass) => Res.GetString("401BF65B-A560-49E1-A1A3-754E38D2D2EB", "Total Gross Weight on declaration should be equal or greater than the sum of Gross Weight of all House Consignments ({0} kg)", billsTotalGrossMass);

		#endregion

		#region R3061 Rule Code & Message

		public string R3061RuleCode => "R3061";

		public string R3061Message => FormatMessage(R3061RuleCode, Res.GetString("3D8DA6F0-2C0B-4199-954C-E8A85E257E0A", "Additional Information of type 30600 can't be declared for any of the Goods Item."));

		#endregion

		#region R3062 Rule Code & Message

		public string R3062RuleCode => R3062RuleCodeCore();
		protected virtual string R3062RuleCodeCore() => "R3062";

		public string R3062Message => FormatMessage(R3062RuleCode, GetR3062MessageCore());

		protected virtual string GetR3062MessageCore() => Res.GetString("A66E0581-74C2-4B1A-B9DF-7AE41D5C1F62", "For a House Consignment, Additional Information of Type=30600 is not valid when the Country of Destination (declared at Consignment level or Goods Item level) is from CL009 (Country Codes Common Transit) list.");

		#endregion

		#region RP11 Rule Code

		public string RP11RuleCode => RP11RuleCodeCore();
		protected virtual string RP11RuleCodeCore() => "RP11";

		#endregion

		#region TR0019 Rule Code & Message

		public string TR0019RuleCode => TR0019RuleCodeCore();
		protected virtual string TR0019RuleCodeCore() => "TR0019";

		public string TR0019Message => FormatMessage(TR0019RuleCode, GetTR0019MessageCore());

		protected virtual string GetTR0019MessageCore() => Res.GetString("A0278530-211C-4481-AFCC-5B918E878D17", "Duplicate Guarantee Ref. No. is entered.");

		#endregion

		#region TR0022 Rule Code & Message

		public string TR0022RuleCode => TR0022RuleCodeCore();
		protected virtual string TR0022RuleCodeCore() => "TR0022";

		public string TR0022Message => FormatMessage(TR0022RuleCode, GetTR0022MessageCore());

		protected virtual string GetTR0022MessageCore() => Res.GetString("B3044B09-A6E0-43FF-AE69-9CAC5950A1A2", "Arrival Notification Date/Time must be in the past.");

		#endregion

		#region TR0030_1 Rule Code & Message

		public string TR0030_1RuleCode => TR0030_1RuleCodeCore();
		protected virtual string TR0030_1RuleCodeCore() => "TR0030-1";

		public string TR0030_1Message => FormatMessage(TR0030_1RuleCode, GetTR0030_1MessageCore());

		protected virtual string GetTR0030_1MessageCore() => Res.GetString("33B0929D-75B4-4991-B418-79C2247A1210", "The maximum number of 9 Previous Documents has been exceeded.");

		#endregion

		#region TR0031 Rule Code & Message

		public string TR0031RuleCode => TR0031RuleCodeCore();

		protected virtual string TR0031RuleCodeCore() => "TR0031";

		public string TR0031Message(int maxNumber) => FormatMessage(TR0031RuleCode, GetTR0031MessageCore(maxNumber));

		protected virtual string GetTR0031MessageCore(int maxNumber) => Res.GetString("F01B2E4B-789B-41DB-B514-832F473596B3", "The maximum number of {0} Additional Information Lines has been exceeded.", maxNumber);

		#endregion

		#region TR0032 Rule Code & Message

		public string TR0032RuleCode => TR0032RuleCodeCore();

		protected virtual string TR0032RuleCodeCore() => "TR0032";

		public string TR0032Message(int maxNumber) => FormatMessage(TR0032RuleCode, GetTR0032MessageCore(maxNumber));

		protected virtual string GetTR0032MessageCore(int maxNumber) => Res.GetString("E3FE0F7D-A83B-41CA-9657-9D3D327E835F", "The maximum number of {0} Additional Reference Lines has been exceeded.", maxNumber);

		#endregion

		#region TR0033 Rule Code & Message

		public string TR0033RuleCode => "TR0033";

		public string TR0033Message(int maxNumber) => FormatMessage(TR0033RuleCode, Res.GetString("6A7DBAC9-AF7C-4F07-86CE-E575B2CF3E55", "The maximum number of {0} Transport Document Lines has been exceeded.", maxNumber));

		#endregion

		#region TR0043 Rule Code & Message
		public string TR0043RuleCode => "TR0043";
		#endregion

		#region TR0044 Rule Code & Message
		public string TR0044RuleCode => "TR0044";
		#endregion

		#region TR0050 Rule Code & Message

		public string TR0050RuleCode => TR0050RuleCodeCore();
		protected virtual string TR0050RuleCodeCore() => "TR0050";

		#endregion

		#region TR0051 Rule Code & Message

		public string TR0051RuleCode => TR0051RuleCodeCore();
		protected virtual string TR0051RuleCodeCore() => "TR0051";

		public string TR0051Message => FormatMessage(TR0051RuleCode, GetTR0051MessageCore());

		protected virtual string GetTR0051MessageCore() => Res.GetString("40604FAE-EBC1-4C65-8CD4-1C9CA5799968", "When an ACR authorization is declared, it is mandatory to declare also an SSE authorization.");

		#endregion

		#region TR0061 Rule Code & Message

		public string TR0061RuleCode => TR0061RuleCodeCore();
		protected virtual string TR0061RuleCodeCore() => "TR0061";

		public string TR0061Message => FormatMessage(TR0061RuleCode, GetTR0061MessageMessageCore());

		protected virtual string GetTR0061MessageMessageCore() => Res.GetString("1380B28E-BCDF-4C3F-94E9-104230BC58AC", "Customs Office code is not in the list");

		#endregion

		#region TR0062 Rule Code & Message

		public string TR0062RuleCode => TR0062RuleCodeCore();
		protected virtual string TR0062RuleCodeCore() => "TR0062";

		public string TR0062aMessage => FormatMessage(TR0062RuleCode, GetTR0062aMessageCore());

		protected virtual string GetTR0062aMessageCore() => Res.GetString("9E8950C4-2808-4260-BF89-03F6EC29408A", "The MAWB number must consist of 11 digits.");

		public string TR0062bMessage => FormatMessage(TR0062RuleCode, GetTR0062bMessageCore());

		protected virtual string GetTR0062bMessageCore() => Res.GetString("E61C5B9D-8ED4-498D-A6FA-E2135DF0C6FE", "Wrong Airline code in MAWB number.");

		public string TR0062cMessage(int checkdigit) => FormatMessage(TR0062RuleCode, GetTR0062cMessageCore(checkdigit));

		protected virtual string GetTR0062cMessageCore(int checkdigit) => Res.GetString("775A69E3-3943-4129-97BA-42F657A5D09D", "Wrong check-digit in MAWB number. Last digit should be '{0}'.", checkdigit);

		#endregion

		#region TR0063 Rule Code & Message

		public string TR0063RuleCode => TR0063RuleCodeCore();
		protected virtual string TR0063RuleCodeCore() => "TR0063";

		public string TR0063Message => FormatMessage(TR0063RuleCode, GetTR0063MessageCore());

		protected virtual string GetTR0063MessageCore() => Res.GetString("1C984CE6-BFF2-4FB0-9A65-0A7FA0532D31", @"If ""Unloaded cargo conforms to declaration"" is set to false, at least one Goods Item or one Seal must be set to 'NEW' or 'MIS' or 'DIF'.");

		#endregion

		#region TR0065 Rule Code & Message

		public string TR0065RuleCode => TR0065RuleCodeCore();
		protected virtual string TR0065RuleCodeCore() => "TR0065";

		public string TR0065NotValidMessage => FormatMessage(TR0065RuleCode, GetTR0065NotValidMessageCore());

		protected virtual string GetTR0065NotValidMessageCore() => Res.GetString("5A6B6792-F696-4799-A73D-FE550A7ABB12", @"The GRN is not a valid format.");

		public string TR0065CheckDigitNotValidMessage => FormatMessage(TR0065RuleCode, GetTR0065CheckDigitNotValidMessageCore());

		protected virtual string GetTR0065CheckDigitNotValidMessageCore() => Res.GetString("6E8DA8E2-7E26-4086-8281-3383F8C8675B", @"The GRN check digit is not valid.");

		#endregion

		#region TR0066 Rule Code & Message

		public string TR0066RuleCode => TR0066RuleCodeCore();
		protected virtual string TR0066RuleCodeCore() => "TR0066";

		public string TR0066Message(long unitCountMaxValue) => FormatMessage(TR0066RuleCode, GetTR0066MessageMessageCore(unitCountMaxValue));

		protected virtual string GetTR0066MessageMessageCore(long unitCountMaxValue) => Res.GetString("9826C3C2-B05B-4925-88AD-8CB3B1D2F770", "Please enter a 'Number of Packages' less than or equal to {0}.", unitCountMaxValue);

		#endregion

		#region TR0067 Rule Code & Message

		public string TR0067RuleCode => TR0067RuleCodeCore();
		protected virtual string TR0067RuleCodeCore() => "TR0067";

		public string TR0067Message => FormatMessage(TR0067RuleCode, GetTR0067MessageCore());

		protected virtual string GetTR0067MessageCore() => Res.GetString("96E03D6B-D1B6-4E96-B706-53751F4F6A2D", "You need to supply at least one goods item.");

		#endregion

		#region TR0068 Rule Code & Message

		public virtual string TR0068RuleCode => "TR0068";

		public string TR0068Message => FormatMessage(TR0068RuleCode, GetTR0068MessageCore());

		protected virtual string GetTR0068MessageCore() => TR0068MessageText;

		internal static string TR0068MessageText => Res.GetString("4D1CCA8B-7B8A-4C10-B687-1EC74A4C7A32", "You have not entered a Line Price Currency.");

		#endregion

		#region TR0069 Rule Code & Message

		public string TR0069RuleCode => TR0069RuleCodeCore();
		protected virtual string TR0069RuleCodeCore() => "TR0069";

		public string TR0069Message => FormatMessage(TR0069RuleCode, GetTR0069MessageCore());

		protected virtual string GetTR0069MessageCore() => Res.GetString("9C78D1E3-7C14-449D-A83C-B0CA5B334E06", "This field must be filled.");

		#endregion

		#region TR0071 Rule Code & Message

		public string TR0071RuleCode => TR0071RuleCodeCore();
		protected virtual string TR0071RuleCodeCore() => "TR0071";

		public string TR0071Message => FormatMessage(TR0071RuleCode, GetTR0071MessageCore());

		protected virtual string GetTR0071MessageCore() => TR0071MessageText;

		internal static string TR0071MessageText => Res.GetString("E3E5D427-32F8-4FD3-AD71-9F6730F5902D", "If Authorization Nº is filled (I.e. private location), Simplified Procedure must be ticked.");

		#endregion

		#region TR0072 Rule Code & Message

		public string TR0072RuleCode => TR0072RuleCodeCore();
		protected virtual string TR0072RuleCodeCore() => "TR0072";

		#endregion

		#region TR0073 Rule Code & Message

		public string TR0073RuleCode => TR0073RuleCodeCore();
		protected virtual string TR0073RuleCodeCore() => "TR0073";

		public string TR0073Message => FormatMessage(TR0073RuleCode, GetTR0073MessageCore());

		protected virtual string GetTR0073MessageCore() => TR0073MessageText;

		internal static string TR0073MessageText => Res.GetString("A6C72E94-DF9C-46DC-AF87-C7EA422D801E", "You need to supply a movement reference number (MRN).");

		#endregion

		#region TR0074 Rule Code & Message

		public string TR0074RuleCode => TR0074RuleCodeCore();
		protected virtual string TR0074RuleCodeCore() => "TR0074";

		public string TR0074Message => FormatMessage(TR0074RuleCode, GetTR0074MessageCore());

		protected virtual string GetTR0074MessageCore() => Res.GetString("8A433D74-307F-4895-8F86-4C1B517C2F97", "The Destination Trader with its EORI number is mandatory. Please select an organization with an EORI number");

		#endregion

		#region TR0076 Rule Code & Message

		public virtual string TR0076RuleCode => "TR0076";

		public string TR0076Message => FormatMessage(TR0076RuleCode, GetTR0076MessageCore());

		protected virtual string GetTR0076MessageCore() => TR0076MessageText;

		internal static string TR0076MessageText => Res.GetString("165CF895-8CE2-4850-8A05-0419F116085F", "The Line Price for Liability Amount must be captured in all Items.");

		#endregion

		#region TR0075 Rule Code & Message

		public string TR0075RuleCode => TR0075RuleCodeCore();
		protected virtual string TR0075RuleCodeCore() => "TR0075";

		public string TR0075Message => FormatMessage(TR0075RuleCode, GetTR0075MessageCore());

		protected virtual string GetTR0075MessageCore() => TR0075MessageText;

		internal static string TR0075MessageText => Res.GetString("33D73398-9997-4B71-A584-AEBBFF65D338", "You need to supply a destination customs office for Arrival that is supported.");

		#endregion

		#region TR0077 Rule Code & Message

		public string TR0077RuleCode => TR0077RuleCodeCore();
		protected virtual string TR0077RuleCodeCore() => "TR0077";

		public string TR0077Message => FormatMessage(TR0077RuleCode, GetTR0077MessageCore());

		protected virtual string GetTR0077MessageCore() => Res.GetString("5C717684-FE63-499E-A443-3C5120B10AA0", "Gross Weight for a House Consignment can't be less than sum of Gross Weights of its underlying Goods Items.");

		#endregion

		#region TR0078 Rule Code & Message

		public string TR0078RuleCode => TR0078RuleCodeCore();
		protected virtual string TR0078RuleCodeCore() => "TR0078";

		public string TR0078Message => FormatMessage(TR0078RuleCode, GetTR0078MessageCore());

		protected virtual string GetTR0078MessageCore() => Res.GetString("215989CA-998D-4101-AC91-30ABDDF164FF", "You have not entered a Nationality");

		#endregion

		#region TR0079 Rule Code & Message

		public string TR0079RuleCode => TR0079RuleCodeCore();

		protected virtual string TR0079RuleCodeCore() => "TR0079";

		public string TR0079Message => FormatMessage(TR0079RuleCode, GetTR0079MessageCore());

		protected virtual string GetTR0079MessageCore() => Res.GetString("22F090EB-4403-4738-AF8E-C14C490566AF", "Work Phone Number must be entered in selected Contact.");

		#endregion

		#region TR0083 Rule Code & Message

		public string TR0083RuleCode => TR0083RuleCodeCore();
		protected virtual string TR0083RuleCodeCore() => "TR0083";
		#endregion

		#region TR0084 Rule Code & Message

		public string TR0084RuleCode => TR0084RuleCodeCore();
		protected virtual string TR0084RuleCodeCore() => "TR0084";

		public string TR0084Message(string fieldName) => FormatMessage(TR0084RuleCode, GetTR0084MessageCore(fieldName));

		protected virtual string GetTR0084MessageCore(string fieldName) => Res.GetString("A7429CEC-C207-48C3-B31C-BDF6615F4E95", "A unit for the {0} has been defined, perhaps from the tariff chosen, yet no quantity has been given. Supply a non-zero quantity", fieldName);

		#endregion

		#region TR0086 Rule Code & Message

		public string TR0086RuleCode => TR0086RuleCodeCore();
		protected virtual string TR0086RuleCodeCore() => "TR0086";

		public string TR0086Message => FormatMessage(TR0086RuleCode, GetTR0086MessageCore());

		protected virtual string GetTR0086MessageCore() => Res.GetString("299FC23D-AC96-4161-827D-A27539D0B7AE", "Total liability amount does not match with the sum of duties and taxes of all Goods Items");

		#endregion

		#region TR0087 Rule Code & Message

		public string TR0087RuleCode => TR0087RuleCodeCore();
		protected virtual string TR0087RuleCodeCore() => "TR0087";

		public string TR0087Message => FormatMessage(TR0087RuleCode, GetTR0087MessageCore());

		protected virtual string GetTR0087MessageCore() => MandatoryValidation.YouHaveNotEnteredMessage(NctsConstants.Traders.PrincipalCaption);

		#endregion

		#region TR0089 Rule Code & Message

		public string TR0089RuleCode => TR0089RuleCodeCore();

		protected virtual string TR0089RuleCodeCore() => "TR0089";

		public string TR0089Message(string currency) => FormatMessage(TR0089RuleCode, GetTR0089MessageCore(currency));

		protected virtual string GetTR0089MessageCore(string currency) => Res.GetString("EA15B028-E567-4B61-AD38-2F2686CD4D95", "The current Exchange Rate is missing in CW1 for Currency of Guarantee: {0}. Please maintain the current Rate in Maintain>Reference Files>Exchange Rates.", currency);

		#endregion

		#region TR0091 Rule Code & Message

		public string TR0091RuleCode => TR0091RuleCodeCore();
		protected virtual string TR0091RuleCodeCore() => "TR0091";

		public string TR0091Message => FormatMessage(TR0091RuleCode, GetTR0091MessageCore());

		protected virtual string GetTR0091MessageCore() => Res.GetString("80873C31-3299-4F6D-8D82-6146042F7EA0", "Total unloaded Gross Weight must be equal or higher than sum of Gross Weights in all House Consignments.");

		#endregion

		#region TR0092 Rule Code & Message
		public string TR0092RuleCode => TR0092RuleCodeCore();
		protected virtual string TR0092RuleCodeCore() => "TR0092";

		public string TR0092aMessage => FormatMessage(TR0092RuleCode, GetTR0092aMessageCore());

		protected virtual string GetTR0092aMessageCore() => Res.GetString("8E52DBFE-87A3-43E7-A8AA-45957024D4CA", "Date Limit cannot be before Acceptance Date");

		public string TR0092bMessage => FormatMessage(TR0092RuleCode, GetTR0092bMessageCore());

		protected virtual string GetTR0092bMessageCore() => Res.GetString("E3C051C4-B777-4133-8B80-5316E2C09D81", "Date Limit cannot be in the past");
		#endregion

		#region TR0093 Rule Code & Message

		public string TR0093RuleCode => TR0093RuleCodeCore();

		protected virtual string TR0093RuleCodeCore() => "TR0093";

		public string TR0093Message(ZDecimal usedBalance, ZString unitOfMeasure) => FormatMessage(TR0093RuleCode, GetTR0093MessageCore(usedBalance, unitOfMeasure));

		protected virtual string GetTR0093MessageCore(ZDecimal usedBalance, ZString unitOfMeasure) => Res.GetString("0eae202f-da6b-4b5c-96b9-aec9930972a6", "Balance used {0} {1} exceeds Guarantee Amount.", usedBalance, unitOfMeasure);

		#endregion

		#region TR0094 Rule Code & Message

		public string TR0094RuleCode => TR0094RuleCodeCore();
		protected virtual string TR0094RuleCodeCore() => "TR0094";

		public string TR0094Message => FormatMessage(TR0094RuleCode, GetTR0094MessageCore());

		protected virtual string GetTR0094MessageCore() => Res.GetString("60F12349-0C23-4CD2-BB9A-28D21C75F918", "House Consignment must contain at least one Goods Item.");

		#endregion

		#region TR0095 Rule Code & Message

		public string TR0095RuleCode => TR0095RuleCodeCore();
		protected virtual string TR0095RuleCodeCore() => "TR0095";

		public string TR0095Message(int containerSequence, string containerNum) => FormatMessage(TR0095RuleCode, GetTR0095MessageCore(containerSequence, containerNum));

		protected virtual string GetTR0095MessageCore(int containerSequence, string containerNum) => Res.GetString("7A789026-3AAA-4B9E-8F5E-BAE5EBC8EEAF", "The container/equipment #{0} {1} is not assigned to any Goods Item", containerSequence, containerNum);

		#endregion

		#region TR0096 Rule Code & Message

		public string TR0096RuleCode => TR0096RuleCodeCore();
		protected virtual string TR0096RuleCodeCore() => "TR0096";

		public string TR0096Message => FormatMessage(TR0096RuleCode, GetTR0096MessageCore());

		protected virtual string GetTR0096MessageCore() => Res.GetString("d1d4ee90-50d2-4a04-89eb-532ce2bab300", "Guarantee calculated does not contain for all goods items the duty amount.");

		#endregion

		#region TR0098 Rule Code & Message

		public string TR0098RuleCode => TR0098RuleCodeCore();
		protected virtual string TR0098RuleCodeCore() => "TR0098";

		#endregion

		#region TR0100 Rule Code & Message

		public virtual string TR0100RuleCode => "TR0100";

		public string TR0100Message => FormatMessage(TR0100RuleCode, GetTR0100MessageCore());

		protected virtual string GetTR0100MessageCore() => TR0100MessageText;

		internal static string TR0100MessageText => Res.GetString("5D4B4FB0-13C2-46A9-9796-1ACB849880FF", "Only values >=0 must be entered.");

		#endregion

		public static string FormatMessage(string code, string text) => $"{code.GetRuleCodeMessagePrefix()} {text}";
	}
}
