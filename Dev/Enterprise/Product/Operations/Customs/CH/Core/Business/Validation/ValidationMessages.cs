namespace Enterprise.Customs.CH.Business;

public static class ValidationMessages
{
	#region Shared

	public static class Plausi
	{
		internal const string CH0004 = "CH0004";
		internal const string R128 = "R128";
		internal const string R132 = "R132";
		internal const string R134 = "R134";
		internal const string R144abc = "R144abc";
		internal const string R170ab = "R170ab";
		internal const string R181 = "R181";
		internal const string R191 = "R191";
		internal const string R247 = "R247";

		public static string MessageCH0005 => Res.GetString("46C8D87B-392E-41EE-997A-22C4C72BF2DB", "When Permit Type is 11, at least Detail Keys 1 and 2 must be selected (CH0005).");
		public static string MessageR123_1 => Res.GetString("55e1e52c-dea0-43ce-be27-982643370ab7", "VAT Value, represented by Customs Value is less than Statistical Value. Please check Invoice Charges or tick VAT Value Confirmation code to confirm it (R123).");
		public static string MessageR123_2 => Res.GetString("d87f90de-ed41-45c7-950b-159e779d032b", "VAT Value, represented by Customs Value, is greater than 3 times Statistical Value. Please check Invoice Charges or tick VAT Value Confirmation code to confirm it (R123).");
		public static string MessageR124 => Res.GetString("51081E6B-9D2E-4148-9626-4E93D1C2FCE7", "If a VAT code is not applicable for the specified tariff, Confirmation must be ticked, otherwise Confirmation must not be ticked (R124).");
		public static string MessageR127_1 => Res.GetString("12E9F2DD-8E1D-48B8-B0E8-EC60E505F84E", "Gross weight must be equal or greater (max 25x) than net weight (R127).");
		public static string MessageR127_2 => Res.GetString("BF00ED90-FA24-428E-822F-131B33A17A9B", "Gross weight must be equal or greater (max 2.5x) than net weight (R127).");
		public static string MessageR132 => Res.GetString("24F77A2C-2464-4FBB-863A-C8CF255083DC", "Packaging marks are mandatory (R132).");
		public static string MessageR135 => Res.GetString("08904F6A-646C-44EF-971D-FA1BE87B18E8", "When a Permit exists, Permit Obligation must be 1 (R135).");
		public static string MessageR133a => Res.GetString("42CA8E10-C72B-49DD-B097-25EF2275AFF5", "When Duty Rate is not empty, you should tick Confirmation ({0}).", "R133a");
		public static string MessageR133b => Res.GetString("FBD2A696-0B49-49F5-9E4C-1D83FC6361A2", "The duty rate is not unique, please select a Duty Rate ({0}).", "R133b");
		public static string MessageR137 => Res.GetString("6D860479-B0E3-4926-940C-025CEF8C3210", "Details for permits are missing. For spirit tax code 280-200 you need a permit of type Obligation (type 6) from the permit authority FOCBS - AAT (code 2) (R137).");
		public static string MessageR138 => Res.GetString("AB773A16-9040-4E83-A3C9-218F7E616A66", "Details for permits are missing. For VOC: additional tax code 700-002 you need a permit of type Obligation (type 6) from the permit authority FOCBS – VOC (code 97) (R138).");
		public static string MessageR141 => Res.GetString("C820BEF0-5390-4925-8F2D-4F0F419CB6FF", "With storage code 2, 3, 4 or 5 you need a permit of type 7 (=permit for periodic tax declaration) from permit authority 96 (=FOCBS – MOT) (R141).");
		public static string MessageR142 => Res.GetString("9BABD6C3-8642-41AC-9E1B-E87E1A2FD8B3", "With storage code 1 or 2 and a customs favor hint code of 5, you need a permit of type 8 (= obligation mineral oil tax) from permit authority 96 (= FOCBS – MOT) (R142).");
		public static string MessageR144abc_1 => Res.GetString("813323EF-F87D-41E8-AE7E-732084A5D163", "NCL Obligation must be 0 when the tariff does not require a Non-Customs Law check ({0}).", R144abc);
		public static string MessageR144abc_2 => Res.GetString("6A9E5FA1-483E-420E-B579-E85EBB1E120C", "NCL Obligation must be 2 when there is an optional Non-Customs Law check for the tariff ({0}).", R144abc);
		public static string MessageR145a => Res.GetString("84667BAD-AE00-4319-BF5F-159269B11400", "At least one additional tax type from ‘600’ to ‘640’ or ‘743’ must be declared when Storage Code is ‘1’ or ‘2’ ({0}).", "R145a");
		public static string MessageR145b => Res.GetString("FDE03A33-B034-42B2-8818-0839F4A8979D", "No additional tax type from ‘600’ to ’640’, ‘710’, ‘720’, ‘730’ or from ‘740’ to ‘743’ may be declared when Storage Code is ‘3’, ‘4’ or ‘5’ ({0}).", "R145b");
		public static string MessageR146 => Res.GetString("6AEF76A2-9572-4485-903B-A53D0C265959", "Quantity must be equal to Invoice Line Gross Weight in KG (R146).");
		public static string MessageR147 => Res.GetString("557F9844-42EB-4124-AAAB-8D2CE399A314", "Quantity must be equal to Invoice Line Net Weight in KG (R147).");
		public static string MessageR148 => Res.GetString("0429B269-B482-43BA-9EA1-891B6F813E10", "Quantity must be equal to Invoice Line Additional Quantity (R148).");
		public static string MessageR149a => Res.GetString("B644D647-768E-43EF-AFC3-933F2209E1DB", "Quantity should be greater than zero (R149 a).");
		public static string MessageR149b => Res.GetString("2C855DEF-3061-4F40-AE26-7A3E9A537E01", "Alcohol Percentage should be greater than zero (R149 b).");
		public static string MessageR160 => Res.GetString("7def7e29-4846-4d50-b1cd-5f837f197dab", "Customs Net Weight in KG should be greater than or equal to Net Weight and less than or equal to Gross Weight (R160).");
		public static string MessageR162R201 => Res.GetString("3AD33EAD-165F-474E-B25D-F15EAB2932E6", @"Tariffs with a customs relief code and the tariff ‘9999.9999’ are not allowed, when the customs procedure is ‘01’ or ‘06’ (R162, R201).");
		public static string MessageR165 => Res.GetString("91BE518F-B07E-4C3F-9B63-E4FCEEC66E71", "In the case of Procedure code 11 (returned goods), other customs procedures are not allowed in the same declaration. Please check other Invoice Lines Procedures (R165).");
		public static string MessageR166c => Res.GetString("983723CC-6672-4E80-BE3E-2F6F0B7B3E78", "Preference code = PR is not possible if 'proof of origin' was given as the reason for the provisional declaration (R166 c).");
		public static string MessageR167c => Res.GetString("2af60df7-1f84-4153-99ca-c3221ba4fc2a", "The direct transportation rule may not be fulfilled for the requested preference. Please check the combination of preference, country of origin and dispatch country (otherwise the confirmation code for the dispatch country must be set) (R167 c).");
		public static string MessageR168 => Res.GetString("63d1a018-b098-4992-8abb-ba27bb0dc82f", "Importer VAT number not found, VAT number is required when VAT Code 90 or 91 is used in any Invoice Line. Please check Importer organization (R168).");
		public static string MessageR170ab => Res.GetString("8ACC85F5-BCE7-48A2-9BE8-DDFEE75B7DFF", "When a Non-Customs Law exists, NCL Obligation must be 1 ({0}).", R170ab);
		public static string MessageR173 => Res.GetString("333BE9C8-F0CA-4B3A-B34B-2EC0487A7C62", "Goods Origin must be 'CH’ when the customs procedure is ’Returning of goods’: 10,11 (R173).");
		public static string MessageR174 => Res.GetString("5E55EBEC-0F5C-411C-8823-49D1A3CDB7B0", "Rate override must be ticked and the rate must be captured (R174).");
		public static string MessageR175R179 => Res.GetString("47765748-9D96-4285-8A79-B90DE2AB62EF", @"Tariff ‘9999.9999’ is not allowed when the customs procedure is ‘05’ or ‘10’ or ‘11’ (R175, R179).");
		public static string MessageR176 => Res.GetString("038BAAA1-A72C-4D3C-886E-2892247A7765", "Only procedures ‘02’,’03’,’05’,’10’,’11’ are allowed when tariff attribute ‘Customs Favour Hint Code’ is ‘2’ (R176).");
		public static string MessageR177 => Res.GetString("389679A3-96D9-4536-A49C-C4A171C64C21", "Tariff must have a customs relief code when the customs procedure is ‘customs relief’: 5 (R177).");
		public static string MessageR182 => Res.GetString("0C430FFD-C49F-4312-9A61-B37D5B1F14D3", "A tariff with a Customs Favour Code cannot be chosen if Procedure = ‘07’ (R182).");
		public static string MessageR183a => Res.GetString("4A14363F-2797-4D02-B0F8-F36DAED20A23", "When Customs Procedure ‘08’ - Tax Free is used, Duty Rate must be 0, VAT Code must be ‘3’ and Non Commercial Goods must be ticked. The Gross Weight and Statistical Value must be greater than 0 (R183 a).");
		public static string MessageR183b => Res.GetString("F363D901-CFB9-44CB-8609-E724859F1A98", "When Customs Procedure ‘08’ - Tax Free is used, a Tariff with Statistical Code or Customs Relief Code is not allowed and Net Duty and Storage Code must not be entered. (R183 b).");
		public static string MessageR190 => Res.GetString("D5403DCA-016C-415C-898A-2DB47171BAC9", "When Process Type is 1 (Due procedure) a Permit issued by Permit Authority 98 should be entered (R190).");
		public static string MessageR193 => Res.GetString("A23B07C0-0700-4656-8610-3272C56D4E90", "The tariff code 9999.9999 000 000 may not be used with procedure code=2 (R193).");
		public static string MessageR198 => Res.GetString("17047988-381A-404A-BABC-8AE40ABCD136", "Goods in repair traffic are non-commercial goods (R198).");
		public static string MessageR208 => Res.GetString("0D15479F-1079-446E-8189-5F7D0B8DB44F", "The Process Type must be 1 and Non Commercial Goods unticked if the Procedure is 02 with Direction = 1 and VAT Code <> 3 (R208).");
		public static string MessageR219 => Res.GetString("F2179375-72EE-4B95-AFA0-3CDF6A2C6E94", "If VAT Code 91 is declared, then the Procedure Code must be 02 with Direction=1 and Refinement Type=2 (R219).");
		public static string MessageR220a => Res.GetString("86872963-72B0-4395-859B-DAD4E52DF3B6", "Rate should be greater than zero (R220 a).");
		public static string MessageR224 => Res.GetString("2805f62e-5602-4142-bf98-d5732defb6db", "Net Duty is allowed only when the tariff rate is per Kilograms Gross (R224).");
		public static string MessageR229 => Res.GetString("9C7E3EA5-26F1-41E0-B735-943A0BB8BD99", "When Process Type is 1 (=Due procedure) at least one Notify Customs Office must be entered (R229).");
		public static string MessageR249a => Res.GetString("84D32EAB-B365-4B7E-A820-1543D8838B5A", "The customs value cannot exceed 1000 CHF for the tariff codes 2402.1000, 2402.2010, 2402.2020, 2402.9000, 2403.1100, 2403.1900, 2403.9910 and 2403.9990 with statistical key = 911 (R249 a).");
		public static string MessageR249b => Res.GetString("F9142A60-A1D8-42BF-AAEF-AAC39A51546C", "The gross weight cannot exceed 10 kg for the tariff codes 2402.1000, 2402.2010, 2402.2020, 2402.9000, 2403.1100, 2403.1900, 2403.9910 and 2403.9990 with statistical key = 911 (R249 b).");
		public static string MessageR249c => Res.GetString("09EE982D-0CB8-4217-8E58-44D766CD3115", "A permit with type=4 and authority=21 is not allowed for the tariff codes 2402.1000, 2402.2010, 2402.2020, 2402.9000, 2403.1100, 2403.1900, 2403.9910 and 2403.9990 with statistical key = 911 (R249 c).");
		public static string MessageR249d => Res.GetString("AA5391DB-F3A4-42EB-B59B-11C51AEF40EA", "Rate Override must be ticked, and Overridden Rate must be set to 0 for the tariff codes 2402.1000, 2402.2010, 2402.2020, 2402.9000, 2403.1100, 2403.1900, 2403.9910 and 2403.9990 with statistical key = 911 (R249 d).");
		public static string MessageR249e => Res.GetString("2EBB18BA-D28A-4656-BADA-A35E2469C39D", "Only procedure code=’06’ is allowed for the tariff codes 2402.1000, 2402.2010, 2402.2020, 2402.9000, 2403.1100, 2403.1900, 2403.9910, 2403.9990 with statistical key = 911 (R249 e).");
		public static string MessageR256 => Res.GetString("0279F3DD-C49B-452D-99C6-DF6CB1A77F78", "The Revers Tobacco Permit (Type=’4’, Authority=’21’) is missing (R256).");
		public static string MessageR257 => Res.GetString("6780E610-B5BA-4CCF-8105-2A25DDEE7311", "Tobacco information is not allowed for tariff codes 2402.1000, 2402.2010, 2402.2020, 2402.9000, 2403.1100, 2403.1900, 2403.9910, 2403.9990 with statistical key 911 (R257).");
		public static string MessageR261_1 => Res.GetString("8C97F374-DFB9-441E-B054-9DF76F6A5B39", "If Procedure = 02 and Direction = Passive (2) and Process Type = simplified (2) and Repair=unticked, then the Billing Type must be 1 (R261).");
		public static string MessageR261_2 => Res.GetString("C0E1A703-3A6F-4CC0-AE98-F5AA62B0AB6E", "If Procedure = 02 and Direction = Passive (2) and Process Type = simplified (2) and Repair=unticked, then the Non-Commercial Goods must be unticked (R261).");
		public static string MessageR262 => Res.GetString("B2250633-DA08-49A7-A0B3-A0868EE2D7EF", "Quantity must be equal to 1% of Invoice Line Additional Quantity (R262).");
		public static string MessageR267 => Res.GetString("C2C69402-5FCE-4A1D-9271-82F76F12D90B", "If the Gross Mass is the assessment basis for the duty calculation, the Gross Mass must be greater than 0 or the gross mass confirmation must be ticked (R267).");
		public static string MessageR268a => Res.GetString("C5734463-2D2D-44FC-A057-DBCDB467942A", "An import customs declaration with statistical value and VAT value = 0 cannot be transmitted. Please check Price and Invoice Charges (R268 a).");
		public static string MessageR268b => Res.GetString("AFE838DF-4857-471A-A4D0-DF95CD858E4B", "An import customs declaration with the value = 0 in the fields Gross mass and Net mass or Additional quantity cannot be transmitted (R268 b).");
		public static string MessageR277 => Res.GetString("523B105D-CCA9-45B2-89CE-B5FAFF4B6017", "When VAT Code ‘90’ is used, a Permit of Type ‘6’ and Permit Authority ‘80’ must be entered (R277).");
		public static string MessageR285 => Res.GetString("3A29367B-0AAD-403B-AAC8-EA17BDA37564", "Non Commercial Goods must be ticked when the customs procedure is ‘07’ and the tariff ‘9999.9999’ is used (R285).");
		public static string MessageR288 => Res.GetString("e92a3700-a998-4d36-a5b3-f2a36e88686f", "Reason 4 - (Proof of origin for developing countries) only allowed for developing countries. At least one Invoice Line with a developing country as Goods Origin should be set (R288).");
		public static string MessageR292 => Res.GetString("63F99905-BC56-43A7-B556-0BB8AE19249B", "The reason for correction 7 may only be used when converting a provisional declaration into a definitive declaration (R292).");
		public static string MessageR313 => Res.GetString("073EFE11-36B3-41FD-B867-8C8C0D83BE1A", "No e-permit type (11 or 12) may be used with the specified permit authority code (R313).");
		public static string MessageR314 => Res.GetString("41720C3C-13C3-4A8C-93A8-A8B697FCE7B8", "The registered permit authority cannot process e-permits (11) (R314).");
		public static string MessageR315 => Res.GetString("60DEE84F-C7BD-47E1-91AA-2EC24D2D2FAF", "The registered permit authority cannot process general e-permits (12) (R315).");
		public static string MessageR316 => Res.GetString("0B076F86-AA39-4EBC-A65C-6EECE2B91AC4", "You must use an e-permit type for the registered permit authority (R316).");
		public static string MessageR325 => Res.GetString("9ba35954-4102-4e54-96b7-9c3dd3a7d083", @"Returned goods (except {0}): ""Non Commercial Good"" must be unticked (R325).", "Samnaun");
		public static string MessageR326 => Res.GetString("47505200-9CB7-43C8-A2DE-E6AF48A67420", "If additional tax '792' (CITES-Flora) and '292' (CITES-Fauna) are possible, only '292' (CITES-Fauna) may be specified (R326).");
		public static string MessageR327 => Res.GetString("DDD133AA-3A8F-44E2-BE44-880E1C793AA8", "If additional tax '290' (Veterinary Inspection) and '792' (CITES-Flora) are possible, only '290' (Veterinary Inspection) may be indicated (R327).");
		public static string MessageR330 => Res.GetString("6D9A5024-38D9-4CF2-947B-8187DED4B73E", "If additional tax ‘792 ‘(CITES-Flora) or ‘292’ (CITES-Fauna) is recorded, the appropriate control office (‘CITES01’, ‘CITES02’, ‘CITES03’,’ CITES04’, ‘CITES05’, or ‘CITES07’) must be indicated in the 'Special Mentions' at invoice header level (R330).");
		public static string MessageR331a => Res.GetString("BF7C4467-F23B-47C9-820C-32BCC82604C0", "The tobacco main group for cigars (2402.1000 / 999) must be ‘1’ or ‘4’ (R331).");
		public static string MessageR331b => Res.GetString("5EC25191-CF36-4600-90F9-4C3AD3B7219F", "The tobacco main group for cigarettes (2402.2010, 2402.2020 and 2402.9000 with statistical code 999) must be ‘1’, ‘2’ or ‘4’ (R331).");
		public static string MessageR331c => Res.GetString("CB528C51-E7BE-4B93-87E4-2DA0B1CD08B0", "The tobacco main group for other tobacco products (2403.1100, 2403.1900, 2403.9910 with statistical code 999) must be ‘3’ or ‘4’ (R331).");
		public static string MessageR331d => Res.GetString("040AF3E0-EBDE-4362-B3C4-88E637CAB13F", "The tobacco main group for other tobacco products (2403.9990 / 999) must be ‘1’, ‘3’ or ‘4’ (R331).");
		public static string MessageR334a => Res.GetString("BFB0E31E-ABC9-449D-A344-F7569AC9B0EA", "The quantity for additional tax 450 (=tobacco) must be equal to the Additional quantity (R334 a).");
		public static string MessageR334b => Res.GetString("8A70E891-B72C-47DE-B768-81C5FB933BDC", "The quantity for additional tax 450 (=tobacco) must be equal to the Net Weight (R334 b).");
		public static string MessageR336 => Res.GetString("2FD9468A-5287-4A35-90E2-8CAE5F025C9C", "The additional taxes ‘465’ (SOTA) and ‘470’ (Prevention) must be recorded for the tariff codes ‘2403.1900’ with statistical key ‘999’, Main Group equal to ‘3’ and Sub Group equal to ‘02’ or ‘03’ (R336).");
		public static string MessageR337 => Res.GetString("C2368362-767C-41BB-8145-ABD6FBBDDE3A", "The quantity of the tobacco tax (450) must be equal to the quantity of prevention tax (470) (R337).");
		public static string MessageR338 => Res.GetString("4C394A0B-EFCB-4C48-90E7-ABF02B437A9E", "The quantity of the tobacco tax (450) must be equal to the quantity of SOTA tax (465) (R338).");
		public static string MessageR339 => Res.GetString("5341AC52-2CD0-4704-957C-E735D314EBAC", "The quantity for additional tax 450 (=tobacco) must be equal to the Gross Weight (R339).");
		public static string MessageR348 => Res.GetString("33DBA258-FD1F-4A2D-AABA-9F6A3C59B167", "Authorized Consignee is mandatory when Clearance Location is Domicile (R348).");
		public static string MessageR353 => Res.GetString("1E6D1C74-288A-41FA-A254-E603DB26ACC4", "The procedure ‘10’ or ‘11’ cannot be combined with repair=ticked (R353).");
		public static string MessageR356 => Res.GetString("7DEA1329-10A6-4CC0-84CA-057CCD848CB9", "The tariff 9999.9999 or tariff with a Customs Favour Code cannot be chosen if Procedure = ‘07’ and Repair = ticked (R356).");
		public static string MessageR358 => Res.GetString("1A03890C-4808-4B12-AA69-F22598E36E2B", "In case of Procedure=02 and Direction=Active, the field Repair Reason must be filled in (R358).");
		public static string MessageR359 => Res.GetString("EF42B6C2-8C03-4AC0-B3CD-3A91D25B5CDD", "The Procedure=03 (Repair Traffic) can no longer be used. You must tick the field Repair instead (R359).");
		public static string MessageR361 => Res.GetString("933AD67E-8CDD-4DBC-A6D8-24EB702A22C7", "The declaration of the simplified procedure must be done with form 11.71 / 11.72 (R361).");
		public static string GetMessageR121(string nameOfAddress) => Res.GetString("1CF18655-DCF7-42DD-9955-8521CADC6563", "Country of {0} should be Switzerland (CH) or Liechtenstein (LI) (R121).", nameOfAddress);
		public static string GetMessageR158(string documentCodes) => Res.GetString("16166932-D4B0-4351-8F65-254C6DCE1FE0", "For preference tariff provide one of the following origin document types: {0} (R158).", documentCodes);
		public static string GetMessageR290_1(string documentCodes) => Res.GetString("cb393133-eafa-4eb7-9753-b6d8377c15b0", "You have not entered an origin document type {0}. A document of this type is needed as you specified a developing country as Origin Country of goods and Preferential tariff (PR) as Preference Code (R290).", documentCodes);
		public static string GetMessageR290_2(string documentCodes) => Res.GetString("a4b7786f-52ce-4d8d-bd58-51bbdba4888f", "Origin document type {0} is only allowed when Origin Country of goods is a developing country and Preference Code is Preferential tariff (PR) (R290).", documentCodes);
		public static string GetPermitObligationMustBeTwoMessage(string ruleName) => Res.GetString("6B876E80-FBAD-4820-8483-4E8DFB00CCBA", "Permit Obligation must be 2 when an optional Permit exists for the Tariff and Country Of Origin ({0}).", ruleName + "b");
		public static string GetPermitObligationMustBeZeroMessage(string ruleName) => Res.GetString("F89625D8-3C54-4B93-B8FC-D0610698CEA3", "The Permit Obligation must be 0 when no Permit exists for the Tariff and Country Of Origin or Gross Weight is below tolerance of a mandatory Permit ({0}).", ruleName + "a");
		public static string MessageNotEntered(string humanReadableName, string ruleName) => Res.GetString("64B03E60-4D85-4AD6-8287-8BA951EF2094", "You have not entered a {1} ({2}).", humanReadableName, ruleName);
		public static string MessageR181 => MessageZeroRateOverrideRequired(R181);
		public static string MessageR191 => MessageZeroRateOverrideRequired(R191);
		public static string MessageR247 => MessageZeroRateOverrideRequired(R247);
		public static string MessageZeroRateOverrideRequired(string ruleCode) => Res.GetString("EAF937A8-459A-4B68-B5D4-F391B4237925", "Rate override must be ticked and the rate must be 0 ({0}).", ruleCode);
	}

	public static class JobComInvoiceLine
	{
		public static string InvalidTariff => Res.GetString("58D1AAD1-4A04-4643-82A1-42E215B2C00C", "Invalid tariff number");
		public static string AdditionalQuantityMissing => Res.GetString("0D3B477F-2702-45E8-A8A3-3C9BCE818CFC", "Additional Quantity missing or invalid (R130)");
	}

	public static class Package
	{
		public static string TotalLineQtyTooSmall => Res.GetString("0ef10ee8-26cd-4ff1-beb5-f392744ed3b6", "The total number of packs included in invoice line(s) is smaller than package quantity.");
	}

	public static class InvoiceLinePackage
	{
		public static string TotalLineQtyIsZero => Res.GetString("f88bfa3b-3264-4e1d-88b8-f4a3586aea82", "There should be at least one Invoice Line per Entry Instruction with Pack Quantity greater than 0.");
	}

	public static class SupportingDocSendingObject
	{
		public static string InvalidFileExtension(string[] allowedFileExtensions) => Res.GetString("0C728AF9-FD81-475D-B6C3-377D876A50BB", "Not allowed file type (allowed file extensions: {0}).", string.Join(" ", allowedFileExtensions));
		public static string MaxAllowedFileSizeExceeded(int maxAllowedFileSizeMB) => Res.GetString("9E0691A3-C8EE-482B-874E-9BC4957E79BE", "Maximum file size reached (Max. {0} MB).", maxAllowedFileSizeMB);
	}
	#endregion
}
