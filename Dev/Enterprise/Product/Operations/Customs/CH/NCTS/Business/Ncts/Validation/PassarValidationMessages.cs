namespace Enterprise.Customs.CH.NCTS.Business;

public class PassarValidationMessages : CH.Business.PassarValidationMessages
{
	internal const string NS30000 = "NS30000";
	internal const string NS30093 = "NS30093";
	internal const string NS30094 = "NS30094";
	internal const string NS30106 = "NS30106";
	internal const string NS30122 = "NS30122";
	internal const string NS30128 = "NS30128";
	internal const string NS30163 = "NS30163";
	internal const string NP70176 = "NP70176";
	internal const string NZ50016 = "NZ50016";

	public static string MessageNP70061(string securityCode) => Res.GetString("02529AB2-1658-4FF6-A3EC-1CB4B7788A2C", "[NP70061] You cannot change the Security code when an MRN already exists. The Security code must remain {0}.", securityCode);

	public static string MessageNS30004 => Res.GetString("E7119FD2-0C2B-49B1-85F6-91643E39329A", "[NS30004] Additional Unloading Remark Text is required for Unloading Remark Code 4=Other.");

	public static string MessageNS30034 => Res.GetString("B7B3DCB1-ADC0-4ACC-90FB-3BE1B79B9F93", "[NS30034] Actual Consignee must be filled.");

	public static string MessageNS30035(string humanReadableName) => Res.GetString("6543280C-9975-499A-9D3F-6EE14A9388E6", "[NS30035] If TC11 Delivery Date is entered, the {0} must be filled.", humanReadableName);

	public static string MessageNP70025 => Res.GetString("0E81BC0D-FDAE-4F50-A975-8B7EFC669A76", "[NP70025] The unloading date can’t be in the future.");

	public static string MessageNP70041 => Res.GetString("EC106FF8-2186-4D3F-A500-0D8DA1CC6888", "[NP70041] At least one Customs Office of Transit must be from the same country as that of Destination Customs Office.");

	public static string MessageNP70123 => Res.GetString("FBD53D7F-D518-449A-81EE-8B2810A08C25", "[NP70123] For national transit the time limit can’t be higher than 10 days.");

	public static string MessageNP70180 => Res.GetString("71919267-9C6E-4F5A-BB83-B3C10AA4A7DC", "[NP70180] In an arrival declaration (NT007) at least one MRN or Additional Goods or Additional Transit Operation must be present.");

	public static string MessageNP70205 => Res.GetString("4FDFAA9E-62C7-47F0-A6D0-70606F7BD1B7", "[NP70205] Commodity Code must be a valid 6, 8 or 11 digits tariff code.");

	public static string MessageNP70231 => Res.GetString("0A4484E6-1D32-48D3-9512-B914AAA03F13", "[NP70231] At least one Customs Office of Transit must be declared belonging to CL010 (Country Codes Community) set of countries.");

	public static string MessageNP70237 => Res.GetString("8BB01560-46AC-4203-9087-A7EC0220BED6", "[NP70237] Please enter a difference in the unloaded value of the goods item.");

	public static string MessageNP70237_WithPackages => Res.GetString("A65E8798-5589-48E9-91ED-494D413A3BDC", "[NP70237] Please enter a difference in the unloaded value of the goods item or its packaging.");

	public static string MessageNP70254 => Res.GetString("681C7089-9F9F-43E3-B28F-1CB0A1307E27", "[NP70254] A previous document with one of the following previous document types is required on the house consignment: SNOT, SWEB, SZVE, STRE, SAUZ, STAB, SZVA, SZWA or NTRV.");

	public static string MessageNP70278 => Res.GetString("BBEA494A-6224-4201-83F3-C04B45716261", "[NP70278] The total number of consignment items including the linked declarations cannot exceed 1999.");

	public static string MessageNS30018_1 => Res.GetString("976083DC-11C5-44DC-AF2D-168085302353", "[NS30018] When already present in Declaration, Consignee must not be specified in House Consignment.");

	public static string MessageNS30018_2 => Res.GetString("0704B275-D087-4204-B583-B8E4EFD3A1D9", "[NS30018] Entering a Consignee on all existing house consignment is required.");

	public static string MessageNS30018_3 => Res.GetString("57618959-B056-4D15-AEDB-06A8804ACFE4", "[NS30018] Entering a Consignee on the affected house consignment is required.");

	public static string MessageNS30128_CH => Res.GetString("DD8373A0-476D-43EC-ACE0-2F0352C8808E", "[NS30128] Principal organization does not have a valid BP-ID, UID or DUNS registration number.");

	public static string MessageNS30128_EUGB => Res.GetString("6812EA1B-6BD9-4C0C-BE37-A9B31CDC001B", "[NS30128] Principal organization does not have a valid EORI or TCU registration number.");

	public static string MessageNS30128_NO => Res.GetString("3F10438D-F58A-427D-A462-4686411F5046", "[NS30128] Principal organization does not have a valid ORG registration number.");

	public static string MessageNS30128_TR => Res.GetString("A1B585B5-C671-4C7D-9035-D2272269329D", "[NS30128] Principal organization does not have a valid VAT registration number.");

	public static string MessageNS30113 => Res.GetString("C14C5185-7B85-45B4-B1A8-F117D9EAF211", "[NS30113] For Security Declaration, if Office of Transit Country is inside European Security zone, Estimated Number of Days is mandatory");

	public static string MessageNS30033 => Res.GetString("C84B49D3-7E1E-4D2A-9266-AECDD6459E66", "[NS30033] For Security Declaration, Estimated Number of Days is mandatory");

	public static string NZ50023(string jobNumber = "") => string.IsNullOrEmpty(jobNumber)
		? Res.GetString("1AE46BC3-53A1-4F50-89E9-F1C239B02326", "[NZ50023] This MRN is already used. A MRN can be used only once.")
		: Res.GetString("C6AE3C17-7661-49CA-AEB3-805704BD4D4E", "[NZ50023] A MRN can be used only on one NCTS Arrival Movement. This MRN is already used on NCTS Arrival Movement with Customer Reference = '{0}'.", jobNumber);

	public static string MessageNS30068 => Res.GetString("4A9565A8-E1F8-4CE3-9A31-99D3F34DDE8B", "[NS30068] You need to supply at least one House Consignment or an Export Declaration Reference.");
	public static string MessageNS30162 => Res.GetString("14783133-535D-4D14-B8A9-8ED6E6D3C2D2", "[NS30162] Country of Dispatch must be filled either on Declaration, House Consignment or Goods item Level.");
	public static string MessageNS30046 => Res.GetString("B9D01C4D-795E-4062-9F6E-950FD0F62A7C", "[NS30046] UCR Reference Number must be specified either at the Consignment item, House Consignment, or Consignment level. Alternatively, a Transport Document can be provided at the Consignment or House Consignment level.");
	public static string MessageNP70279 => Res.GetString("6B5BDB64-93D7-4C17-A8C9-3231E91C28DE", "[NP70279] This declaration has no UCR Reference Number or Transport Document. Please copy the UCR Reference Number of the linked declaration in this one if available.");
	public static string MessageNS30163NP70176(string ruleCode, int cardinality) => Res.GetString("522F18BA-F9D2-452E-BB55-404BEEDF21D1", "[{0}] The number of House Consignments, Linked Export declarations and Previous Documents of type EXPO must not exceed {1}.", ruleCode, cardinality);
}
