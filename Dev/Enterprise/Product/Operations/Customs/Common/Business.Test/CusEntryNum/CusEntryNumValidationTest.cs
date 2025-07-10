using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	sealed class CusEntryNumValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCE_EntryNum()
		{
			var num = Factory.New<CusEntryNumber>();

			num.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			AssertHasWarningContaining(num.CE_EntryNumInfo, MandatoryValidation.YouHaveNotEntered);
			num.CE_EntryNum = "123";
			AssertNoWarningContaining(num.CE_EntryNumInfo, MandatoryValidation.YouHaveNotEntered);

			num.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			num.CE_EntryNum = "123456789";
			AssertNoWarning(num.CE_EntryNumInfo, CusEntryNumValidation.ITNumberLengthExceeded);

			num.CE_EntryNum = "1234567890123456";
			AssertHasWarning(num.CE_EntryNumInfo, CusEntryNumValidation.ITNumberLengthExceeded);

			num.CE_EntryNum = "1";
			num.CE_EntryType = UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.UAEInstalmentNumber;
			AssertNoError(num.CE_EntryNumInfo, "UAE Installment Number must be a whole number.");
			AssertNoError(num.CE_EntryNumInfo, "UAE Installment Number must be greater than or equal to 1.");
			AssertNoError(num.CE_EntryNumInfo, "You have not entered a UAE Installment Number.");
			num.CE_EntryNum = "x";
			AssertHasError(num.CE_EntryNumInfo, "UAE Installment Number must be a whole number.");
			num.CE_EntryNum = "0";
			AssertHasError(num.CE_EntryNumInfo, "UAE Installment Number must be greater than or equal to 1.");
			num.CE_EntryNum = "00000";
			AssertHasError(num.CE_EntryNumInfo, "UAE Installment Number must be greater than or equal to 1.");
			num.CE_EntryNum = "";
			AssertHasError(num.CE_EntryNumInfo, "You have not entered a UAE Installment Number.");

			num.CE_EntryNum = "-123";
			AssertHasError(num.CE_EntryNumInfo, "UAE Installment Number must be a whole number.");

			num.CE_RN_NKCountryCode = Constants.CountryCodes.Egypt;
			num.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			AssertHasError(num.CE_EntryNumInfo, "ACI Number should contain 19 digits when country/region of issue is EG.");
			num.CE_EntryNum = "1234567890123456789";
			AssertNoError(num.CE_EntryNumInfo, "ACI Number should contain 19 digits when country/region of issue is EG.");
			num.CE_ParentTable = "JobShipment";

			var num1 = Factory.New<CusEntryNumber>();
			num1.CE_ParentID = ZGuid.NewZGuid();
			num1.CE_ParentTable = JobShipmentSchema.Constants.TableName;
			num1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num1.CE_RN_NKCountryCode = Constants.CountryCodes.Canada;
			num1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			num1.CE_EntryNum = "1234 567890";
			Factory.Save();
			var num2 = Factory.New<CusEntryNumber>();
			num2.CE_ParentID = ZGuid.NewZGuid();
			num2.CE_ParentTable = JobShipmentSchema.Constants.TableName;
			num2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num2.CE_RN_NKCountryCode = Constants.CountryCodes.Canada;
			num2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			num2.CE_EntryNum = "1234567890";
			AssertHasWarning(num2.CE_EntryNumInfo, CusEntryNumValidation.AlreadyContainsCCN);

			num2.CE_EntryNum = "12345678 90";
			AssertNoWarning(num2.CE_EntryNumInfo, CusEntryNumValidation.AlreadyContainsCCN);

			var numTestMrn = GenerateCusEntryNumber();
			numTestMrn.CE_EntryIsSystemGenerated = false;
			numTestMrn.CE_EntryNum = "10LT10100016194298";
			AssertNoWarning(numTestMrn.CE_EntryNumInfo, "Country of issue is not recognized, cannot validate this MRN");
			numTestMrn.CE_EntryNum = "10LT";
			AssertNoWarning(numTestMrn.CE_EntryNumInfo, "Country of issue is not recognized, cannot validate this MRN");
			numTestMrn.CE_EntryNum = "10SG10100016194298";
			AssertHasWarning(numTestMrn.CE_EntryNumInfo, "Country of issue is not recognized, cannot validate this MRN");
			numTestMrn.CE_EntryNum = "10";
			AssertHasWarning(numTestMrn.CE_EntryNumInfo, "Country of issue is not recognized, cannot validate this MRN");

			TestExtendedMrnValidation(numTestMrn);
		}

		CusEntryNumber GenerateCusEntryNumber()
		{
			var numTestMrn = Factory.New<CusEntryNumber>();
			numTestMrn.CE_ParentID = ZGuid.NewZGuid();
			numTestMrn.CE_ParentTable = JobShipmentSchema.Constants.TableName;
			numTestMrn.CE_RN_NKCountryCode = Constants.CountryCodes.France;
			numTestMrn.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			return numTestMrn;
		}

		public void TestCheckCE_EntryNum_ROT()
		{
			var num = Factory.New<CusEntryNumber>();

			num.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num.CE_EntryType = UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.RotationNumber;
			num.CE_EntryNum = "1234567";
			AssertHasError(num.CE_EntryNumInfo, "ROT Number should only contain 6 letters and digits.");
			num.CE_EntryNum = "123456";
			AssertNoError(num.CE_EntryNumInfo, "ROT Number should only contain 6 letters and digits.");
			num.CE_EntryNum = "~12345";
			AssertHasError(num.CE_EntryNumInfo, "ROT Number should only contain 6 letters and digits.");
			num.CE_EntryNum = "ROT123";
			AssertNoError(num.CE_EntryNumInfo, "ROT Number should only contain 6 letters and digits.");
			num.CE_EntryNum = "ROT1";
			AssertHasError(num.CE_EntryNumInfo, "ROT Number should only contain 6 letters and digits.");
		}

		#region TEST MRN
		void TestExtendedMrnValidation(CusEntryNumber numTestMrn)
		{
			#region AD
			RunMrnTestAssertTransit("10AD00000117716879", "10AD0000011771687A");
			#endregion
			#region AT
			RunMrnTestAssertTransit("10AT100200TN8VECI2", "10AT100200TB8VECI2");
			RunMrnTestAssertTransit("10AT100000TV8VF470", "10AT100000TG8VF470");
			RunMrnTestAssertTransit("10AT100200CT8VDY30", "10AT100200TT8VDY30");
			RunMrnTestAssertExport("10AT100000EN8VDM28", "10AT100000GN8VDM28");
			RunMrnTestAssertExport("10AT100000EV8VDM28", "10AT100000E48VDM28");
			RunMrnTestAssertExport("10AT100000EA8VDM28", "10AT100000E58VDM28");
			RunMrnTestAssertExport("10AT100000ES8VDM28", "10AT100000E38VDM28");
			RunMrnTestAssertImport("10AT100000IS8VDM28", "10AT100000IR8VDM28");
			#endregion
			#region BE
			RunMrnTestAssertTransit("10BE10100016194298", "10BE10100016194G98");
			RunMrnTestAssertExport("10BEE0000000024958", "10BEF0000000024958");
			RunMrnTestAssertImport("10BEN0000000024958", "10BER0000000024958");
			#endregion
			#region BG
			RunMrnTestAssertTransit("10BG00100000145971", "10BG0010000014G971");
			RunMrnTestAssertExport("10BG002000A0006920", "10BG002000Z0006920");
			RunMrnTestAssertExport("10BG002000C0006920", "10BG002000Z0006920");
			RunMrnTestAssertExport("10BG002000D0006920", "10BG002000Z0006920");
			RunMrnTestAssertImport("10BGI0510000001248", "10BGT0510000001248");
			#endregion
			#region CH
			RunMrnTestAssertTransit("10CH00000007403506", "10CH000000074R3506");
			#endregion
			#region CY
			RunMrnTestAssertTransit("10CY00044010009F52", "10CY00044060009F52");
			RunMrnTestAssertExport("10CY066300260LZRY0", "10CY066300960LZRY0");
			RunMrnTestAssertImport("10CY00051030004260", "10CY00051080004260");
			#endregion
			#region CZ
			RunMrnTestAssertTransit("10CZ01720098G2QCS4", "10CZ01720078G2QCS4");
			RunMrnTestAssertExport("10CZ01630028G4CZ20", "10CZ01720078G2QCS4");
			RunMrnTestAssertImport("10CZ17610013810010", "10CZ01720078G2QCS4");
			#endregion
			#region DE
			RunMrnTestAssertTransit("10DE210119796487M8", "10DE210119796487P8");
			RunMrnTestAssertTransit("10DE210119796487T8", "10DE210119796487P8");
			RunMrnTestAssertTransit("10DE210119796487J8", "10DE210119796487P8");
			RunMrnTestAssertTransit("10DE210119796487K8", "10DE210119796487P8");
			RunMrnTestAssertTransit("10DE210119796487L8", "10DE210119796487P8");
			RunMrnTestAssertTransit("10DE210119796487M8", "10DE210119796487P8");
			RunMrnTestAssertExport("10DE210116869564E5", "10DE210116869564P5");
			RunMrnTestAssertExport("10DE210116869564X7", "10DE210116869564P7");
			RunMrnTestAssertExport("10DE210116869564A7", "10DE210116869564P7");
			RunMrnTestAssertExport("10DE210116869564B7", "10DE210116869564P7");
			RunMrnTestAssertImport("10DE213000000001I9", "10DE21300000000189");
			RunMrnTestAssertImport("10DE213000000001Z4", "10DE21300000000174");
			RunMrnTestAssertImport("10DE213000000001R4", "10DE21300000000174");
			#endregion
			#region DK
			RunMrnTestAssertTransit("10DK0004601366B740", "10DK0004608366B740");
			RunMrnTestAssertExport("10DK0031002366C257", "10DK0004608366B740");
			RunMrnTestAssertImport("10DK00470630002058", "10DK0004608366B740");
			#endregion
			#region EE
			RunMrnTestAssertTransit("10EE5130EET0340433", "10EE5130EEB0340433");
			RunMrnTestAssertTransit("10EE5600EEN0336554", "10EE5600EEB0336554");
			RunMrnTestAssertExport("10EE1210EE70557590", "10EE1210EEZ0557590");
			RunMrnTestAssertImport("10EE5130EEC0340433", "10EE5130EEB0340433");
			RunMrnTestAssertImport("10EE5600EEN0336554", "10EE5130EEB0340433");
			RunMrnTestAssertImport("10EE5600EET0336555", "10EE5130EEB0340433");
			#endregion
			#region GR
			RunMrnTestAssertTransit("10GRTR083200003214", "10GR00083200003214");
			RunMrnTestAssertTransit("10GRRT083200003214", "10GR00083200003214");
			RunMrnTestAssertExport("10GREX083200003214", "10GR00083200003214");
			RunMrnTestAssertImport("10GREN110200003116", "10GR00083200003214");
			#endregion
			#region ES
			RunMrnTestAssertTransit("10ES00014150034010", "10ES00014180034010");
			RunMrnTestAssertExport("10ES00010110047570", "10ES00010180047570");
			RunMrnTestAssertExport("10ES00010120047570", "10ES00010180047570");
			RunMrnTestAssertImport("10ES00170170000215", "10ES00170100000215");
			RunMrnTestAssertImport("10ES001701B0000215", "10ES00170100000215");
			RunMrnTestAssertImport("10ES001701C0000215", "10ES00170100000215");
			RunMrnTestAssertImport("10ES001701D0000215", "10ES00170100000215");
			RunMrnTestAssertImport("10ES001701E0000215", "10ES00170100000215");
			RunMrnTestAssertImport("10ES001701F0000215", "10ES00170100000215");
			#endregion
			#region FI
			RunMrnTestAssertTransit("10FI000000070863T7", "10FI000000070863G7");
			RunMrnTestAssertExport("10FI000000130153E4", "10FI000000070863G7");
			RunMrnTestAssertImport("10FI00000000I23419", "10FI000000070863G7");
			RunMrnTestAssertImport("10FI000000001234I9", "10FI000000070863G7");
			#endregion
			#region FR
			RunMrnTestAssertTransit("10FR01000695484605", "10FR01000695484A05");
			RunMrnTestAssertExport("10FRC0027013316110", "10FRG1000695484605");
			RunMrnTestAssertExport("10FRD0617B06089831", "10FRG1000695484605");
			RunMrnTestAssertImport("10FRS0001234567891", "10FRG1000695484605");
			#endregion
			#region HU
			RunMrnTestAssertTransit("10HU101000109AFE24", "10HU101000509AFE24");
			RunMrnTestAssertExport("10HU10100020976452", "10HU10100050976452");
			RunMrnTestAssertImport("10HU101000I0976452", "10HU10100050976452");
			#endregion
			#region IE
			RunMrnTestAssertImport("10IE00000000612286", "");
			RunMrnTestAssertTransit("10IEDUB40010094A03", "10IEDUB40050094A03");
			RunMrnTestAssertExport("10IEDU4EU014685051", "10IEDU400054685051");
			RunMrnTestAssertExport("09IEDU4CO011239413", "09IEDU400055239413");
			RunMrnTestAssertExport("10IEDU1EX006985811", "10IEDU100006985811");
			#endregion
			#region IT
			RunMrnTestAssertTransit("10ITQ0B8T0018583T1", "12ITQU5EN0145171G1");
			RunMrnTestAssertExport("10ITQ0B1T0153253E0", "12ITQU5EN0145171G1");
			RunMrnTestAssertImport("09ITQYG1T0077667I3", "12ITQU5EN0145171G1");
			RunMrnTestAssertImport("12ITQU5EN0145171N1", "12ITQU5EN0145171G1");
			#endregion
			#region LT
			RunMrnTestAssertTransit("10LTKA100011789E50", "10LTKA100051789E50");
			RunMrnTestAssertExport("10LTKR1000IS0199F0", "10LTKR1000GS0199F0");
			RunMrnTestAssertExport("10LTKR1000EK0199F0", "10LTKR1000GS0199F0");
			RunMrnTestAssertImport("10LTKR1000IV0199F0", "10LTKR1000GS0199F0");
			#endregion
			#region LU
			RunMrnTestAssertTransit("10LU704000103AF901", "10LU704000503AF901");
			RunMrnTestAssertExport("10LU704000203AF820", "10LU704000503AF820");
			RunMrnTestAssertImport("YYLUENSA123456789X", "YYLUEGSA123456789X");
			#endregion
			#region LV
			RunMrnTestAssertTransit("10LV00020610B97B84", "10LV00020650B97B84");
			RunMrnTestAssertExport("10LV00020620249122", "10LV00020650B97B84");
			RunMrnTestAssertImport("10LV00081630000953", "10LV00020650B97B84");
			#endregion
			#region MT
			RunMrnTestAssertTransit("10MT0001181004F449", "10MT000118500597B8");
			RunMrnTestAssertExport("10MT000118200597B8", "10MT000118500597B8");
			RunMrnTestAssertImport("YYMTDDMM3XXXXXXXXZ", "YYMTDDMM5XXXXXXXXZ");
			#endregion
			#region NL
			RunMrnTestAssertTransit("10NL5631291B1A3361", "10NL5631295B1A3361");
			RunMrnTestAssertExport("10NL5631772B392BE6", "10NL5631295B1A3361");
			RunMrnTestAssertImport("10NL9871774B392BE6", "10NL5631295B1A3361");
			#endregion
			#region NO
			RunMrnTestAssertTransit("10NO01011A10583AE9", "10NO01011A50583AE9");
			#endregion
			#region PL
			RunMrnTestAssertTransit("10PL301010N16885B4", "10PL301010516885B4");
			RunMrnTestAssertTransit("10PL301010T16885B4", "10PL301010516885B4");
			RunMrnTestAssertExport("10PL322050E0019720", "10PL322050K0019720");
			RunMrnTestAssertExport("10PL322050S0019720", "10PL322050K0019720");
			RunMrnTestAssertExport("10PL322050W0019720", "10PL322050K0019720");
			RunMrnTestAssertImport("10PL404030I0004381", "10PL404030L0004381");
			RunMrnTestAssertImport("10PL404030M0004381", "10PL404030L0004381");
			RunMrnTestAssertImport("10PL404030D0004381", "10PL404030L0004381");
			#endregion
			#region PT
			RunMrnTestAssertTransit("10PT0001151011BA25", "10PT0001155011BA25");
			RunMrnTestAssertExport("10PT00001521172334", "10PT0001155011BA25");
			RunMrnTestAssertImport("10PT00001541172334", "10PT0001155011BA25");
			#endregion
			#region RO
			RunMrnTestAssertTransit("10ROBU14000002644116", "10ROBU14000002B446");
			RunMrnTestAssertExport("10ROBU1030E0016635", "10ROBU1030D0016635");
			RunMrnTestAssertImport("10ROTM5510I0000016", "10ROTM5510D0000016");
			#endregion
			#region SE
			RunMrnTestAssertTransit("10SE00005010C124F5", "10SE00005050C124F5");
			RunMrnTestAssertExport("10SEE3MHI94AQQ49T8", "10SER3MHI94AQQ49T8");
			RunMrnTestAssertExport("10SESUDHI94AQQ49T8", "10SESEDHI94AQQ49T8");
			RunMrnTestAssertImport("10SESIDHI94AQQ49T8", "10SESODHI94AQQ49T8");
			#endregion
			#region SI
			RunMrnTestAssertTransit("10SI00102615009346", "10SI00102685009346");
			RunMrnTestAssertExport("10SI00102625027607", "10SI00102685009346");
			RunMrnTestAssertImport("10SI00102635027607", "10SI00102685009346");
			#endregion
			#region SK
			RunMrnTestAssertTransit("10SK5161TR00000391", "10SK5161AR00000391");
			RunMrnTestAssertExport("10SK5161EX00069002", "10SK5161GK00069002");
			RunMrnTestAssertExport("10SK5161PV00069002", "10SK5161GK00069002");
			RunMrnTestAssertImport("10SK5161IM00069002", "10SK5161GK00069002");
			RunMrnTestAssertImport("10SK5161PD00069002", "10SK5161GK00069002");
			#endregion
			#region SM
			RunMrnTestAssertTransit("10SMQ02010004408T1", "10SMQ02010004408G1");
			#endregion
			#region GB
			RunMrnTestAssertTransit("10GB00002910B75BE5", "10GB00002950B75BE5");
			RunMrnTestAssertExport("10GB03X52858027017", "10GB03G52858027017");
			RunMrnTestAssertImport("10GB03I52858027017", "10GB03G52858027017");
			#endregion
		}

		[ExpectNoExceptions]
		void RunMrnTestAssertTransit(string goodValue, string badValue)
		{
			string countryCode = goodValue.Substring(2, 2);
			if (!countryCode.Equals("IE"))
			{
				ProcessAssertMrn(goodValue, badValue, MrnTypes.Transit);
			}
			else
			{
				ProcessAssertMrnAlwaysValidUnknown(goodValue, badValue, MrnTypes.Transit);
			}
		}
		[ExpectNoExceptions]
		void RunMrnTestAssertExport(string goodValue, string badValue)
		{
			string countryCode = goodValue.Substring(2, 2);
			if (!countryCode.Equals("IE"))
			{
				ProcessAssertMrn(goodValue, badValue, MrnTypes.Export);
			}
			else
			{
				ProcessAssertMrnAlwaysValidUnknown(goodValue, badValue, MrnTypes.Export);
			}
		}

		[ExpectNoExceptions]
		void RunMrnTestAssertImport(string goodValue, string badValue)
		{
			string countryCode = goodValue.Substring(2, 2);
			if (!countryCode.Equals("IE"))
			{
				ProcessAssertMrn(goodValue, badValue, MrnTypes.Import);
			}
			else
			{
				ProcessAssertMrnAlwaysValidUnknown(goodValue, badValue, MrnTypes.Import);
			}
		}

		[ExpectNoExceptions]
		void ProcessAssertMrn(string goodValue, string badValue, MrnTypes mrnType)
		{
			var numTestMrn = GenerateCusEntryNumber();
			string countryCode = goodValue.Substring(2, 2);

			numTestMrn.CE_EntryIsSystemGenerated = true;
			numTestMrn.CE_EntryNum = badValue;
			AssertNoWarnings(numTestMrn.CE_EntryNumInfo);

			numTestMrn.CE_EntryIsSystemGenerated = false;

			numTestMrn.CE_EntryNum = goodValue;
			AssertNoWarning(numTestMrn.CE_EntryNumInfo, "This is not a valid MRN for " + countryCode);

			NUnit.Framework.Assert.That(RunMrnTest(goodValue, mrnType, numTestMrn), NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));

			numTestMrn.CE_EntryNum = badValue;
			AssertHasWarning(numTestMrn.CE_EntryNumInfo, "This is not a valid MRN for " + countryCode);

			NUnit.Framework.Assert.That(RunMrnTest(badValue, mrnType, numTestMrn), NUnit.Framework.Is.EqualTo("This is not a valid " + mrnType + " MRN for country " + countryCode));
		}

		[ExpectNoExceptions]
		void ProcessAssertMrnAlwaysValidUnknown(string goodValue, string badValue, MrnTypes mrnType)
		{
			if (!string.IsNullOrEmpty(badValue))
			{
				var numTestMrn = GenerateCusEntryNumber();
				string countryCode = goodValue.Substring(2, 2);

				numTestMrn.CE_EntryIsSystemGenerated = false;
				NUnit.Framework.Assert.That(RunMrnTest(goodValue, mrnType, numTestMrn), NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));

				numTestMrn.CE_EntryNum = badValue;
				NUnit.Framework.Assert.That(RunMrnTest(badValue, mrnType, numTestMrn), NUnit.Framework.Is.EqualTo("This is not a valid " + mrnType + " MRN for country " + countryCode));
			}
		}

		string RunMrnTest(string testValue, MrnTypes type, CusEntryNumber numTestMrn)
		{
			return MovementReferenceNumberValidator.ApplyAdditionalValidationOnMrn(testValue, type);
		}
		#endregion

		public void TestCE_EntryType_ListValidation()
		{
			var num = Factory.New<CusEntryNumber>();

			num.CE_EntryType = "XYZ";
			AssertNoErrors(num.CE_EntryTypeInfo);

			num.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num.CE_EntryType = "ABC";
			AssertHasErrorContaining(num.CE_EntryTypeInfo, ListValidation.InvalidCodeMessageError);

			num.CE_EntryType = num.Lookups.AdditionalReferenceNumberTypes[0].Code;
			AssertNoErrors(num.CE_EntryTypeInfo);

			num.CE_EntryType = "";
			AssertHasError(num.CE_EntryTypeInfo, "Please enter a Number Type.");
		}

		public void TestCE_EntryType_UniqueValidation_AllowMiltiple()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Germany);

			var parentID1 = ZGuid.NewZGuid();
			var num1 = Factory.New<CusEntryNumber>();
			num1.CE_ParentID = parentID1;
			num1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			var num2 = Factory.New<CusEntryNumber>();
			num2.CE_ParentID = parentID1;
			num2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			num1.CE_EntryType = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num2.CE_EntryType = CusEntryNumber.Categories.AdditionalReferenceNumber;
			AssertHasErrors(num1.CE_EntryTypeInfo);
			AssertHasErrors(num2.CE_EntryTypeInfo);

			num1.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
			num2.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
			AssertNoErrors(num1.CE_EntryTypeInfo);
			AssertNoErrors(num2.CE_EntryTypeInfo);
		}

		public void TestCE_EntryType_UniqueValidation()
		{
			var parentID1 = ZGuid.NewZGuid();
			var parentID2 = ZGuid.NewZGuid();

			var num1 = Factory.New<CusEntryNumber>();
			num1.CE_ParentID = parentID1;
			var num2 = Factory.New<CusEntryNumber>();
			num2.CE_ParentID = parentID1;
			var num3 = Factory.New<CusEntryNumber>();
			num3.CE_ParentID = parentID2;

			num1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num3.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			num1.CE_EntryType = num1.Lookups.AdditionalReferenceNumberTypes[0].Code;
			num2.CE_EntryType = num2.Lookups.AdditionalReferenceNumberTypes[1].Code;
			num2.CE_EntryType = num3.Lookups.AdditionalReferenceNumberTypes[2].Code;

			AssertNoErrors(num1.CE_EntryTypeInfo);
			AssertNoErrors(num2.CE_EntryTypeInfo);
			AssertNoErrors(num3.CE_EntryTypeInfo);

			num2.CE_EntryType = num1.CE_EntryType;
			AssertHasErrors(num1.CE_EntryTypeInfo);
			AssertHasErrors(num2.CE_EntryTypeInfo);
			AssertNoErrors(num3.CE_EntryTypeInfo);

			num1.CE_EntryType = num1.Lookups.AdditionalReferenceNumberTypes[1].Code;
			AssertNoErrors(num1.CE_EntryTypeInfo);
			AssertNoErrors(num2.CE_EntryTypeInfo);
			AssertNoErrors(num3.CE_EntryTypeInfo);

			num3.CE_EntryType = num1.CE_EntryType;
			AssertNoErrors(num1.CE_EntryTypeInfo);
			AssertNoErrors(num2.CE_EntryTypeInfo);
			AssertNoErrors(num3.CE_EntryTypeInfo);

			num2.CE_Category = "XXX";
			num2.CE_EntryType = num1.CE_EntryType;
			AssertNoErrors(num1.CE_EntryTypeInfo);
			AssertNoErrors(num2.CE_EntryTypeInfo);
			AssertNoErrors(num3.CE_EntryTypeInfo);

			var num4 = Factory.New<CusEntryNumber>();
			num4.CE_ParentID = parentID1;
			num4.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num4.CE_RN_NKCountryCode = Constants.CountryCodes.Iceland;
			num4.CE_EntryType = num1.CE_EntryType;
			AssertNoErrors(num1.CE_EntryTypeInfo);
			AssertNoErrors(num2.CE_EntryTypeInfo);
			AssertNoErrors(num3.CE_EntryTypeInfo);
			AssertNoErrors(num4.CE_EntryTypeInfo);

			num1.CE_EntryType = num1.Lookups.AdditionalReferenceNumberTypes[1].Code;
			num1.CE_EntryType = num4.CE_EntryType;
			AssertNoErrors(num1.CE_EntryTypeInfo);
			AssertNoErrors(num2.CE_EntryTypeInfo);
			AssertNoErrors(num3.CE_EntryTypeInfo);
			AssertNoErrors(num4.CE_EntryTypeInfo);
		}

		[ExpectNoExceptions]
		public void TestCE_EntryType_UniqueValidation_IsUnique()
		{
			var parentId1 = ZGuid.NewZGuid();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var customsReferenceNumberTypeCollection = new CustomsReferenceNumberTypeCollection();
				var customsReferenceNumberType1 = customsReferenceNumberTypeCollection.Add("AAA", (NoResString)"AAA code");
				customsReferenceNumberType1.IsUnique = true;
				var customsReferenceNumberType2 = customsReferenceNumberTypeCollection.Add("BBB", (NoResString)"BBB code");
				customsReferenceNumberType2.IsUnique = false;

				FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customsReferenceNumberTypeCollection);

				NUnit.Framework.Assert.That(CusEntryNumLookups.GetAdditionalReferenceNumberTypes(Constants.CountryCodes.UnitedStates).CodesAsString, NUnit.Framework.Is.EquivalentTo("IT, RRN, AAA, BBB"), "prerequisite");

				var cusEntryNumber1 = CreateAdditionalReferenceNumber(parentId1, "IT");
				AssertNoError(cusEntryNumber1.CE_EntryTypeInfo, "The Number Type has been duplicated and must be unique.");

				var cusEntryNumber2 = CreateAdditionalReferenceNumber(parentId1, "IT");
				AssertHasError(cusEntryNumber1.CE_EntryTypeInfo, "The Number Type has been duplicated and must be unique.");
				AssertHasError(cusEntryNumber2.CE_EntryTypeInfo, "The Number Type has been duplicated and must be unique.");

				var cusEntryNumber3 = CreateAdditionalReferenceNumber(parentId1, "AAA");
				AssertNoError(cusEntryNumber3.CE_EntryTypeInfo, "The Number Type has been duplicated and must be unique.");

				var cusEntryNumber4 = CreateAdditionalReferenceNumber(parentId1, "AAA");
				AssertHasError(cusEntryNumber3.CE_EntryTypeInfo, "The Number Type has been duplicated and must be unique.");
				AssertHasError(cusEntryNumber4.CE_EntryTypeInfo, "The Number Type has been duplicated and must be unique.");

				var cusEntryNumber5 = CreateAdditionalReferenceNumber(parentId1, "BBB");
				AssertNoError(cusEntryNumber5.CE_EntryTypeInfo, "The Number Type has been duplicated and must be unique.");

				var cusEntryNumber6 = CreateAdditionalReferenceNumber(parentId1, "BBB");
				AssertNoError(cusEntryNumber5.CE_EntryTypeInfo, "The Number Type has been duplicated and must be unique.");
				AssertNoError(cusEntryNumber6.CE_EntryTypeInfo, "The Number Type has been duplicated and must be unique.");
			}
		}

		public void TestCE_EntryNum_UniqueValidation()
		{
			var customsReferenceNumberTypeCollection = new CustomsReferenceNumberTypeCollection();
			var customsReferenceNumberType1 = customsReferenceNumberTypeCollection.Add("AAA", (NoResString)"AAA code");
			customsReferenceNumberType1.IsUnique = true;
			var customsReferenceNumberType2 = customsReferenceNumberTypeCollection.Add("BBB", (NoResString)"BBB code");
			customsReferenceNumberType2.IsUnique = false;

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customsReferenceNumberTypeCollection);

			AssertCE_EntryNum_UniqueValidation("AAA");
			AssertCE_EntryNum_UniqueValidation("BBB");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				AssertCE_EntryNum_UniqueValidation(UnitedStatesAdditionalReferenceNumberTypes.Codes.IT);
			}
		}

		[ExpectNoExceptions]
		public void TestCE_EntryNum_AutomationControlledValidation()
		{
			var customsReferenceNumberTypeCollection = new CustomsReferenceNumberTypeCollection();
			var customsReferenceNumberType1 = customsReferenceNumberTypeCollection.Add("AAA", (NoResString)"AAA code");
			customsReferenceNumberType1.IsAutomation = true;
			var customsReferenceNumberType2 = customsReferenceNumberTypeCollection.Add("BBB", (NoResString)"BBB code");
			customsReferenceNumberType2.IsAutomation = false;

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customsReferenceNumberTypeCollection);

			var parent = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Forwarding.IForwardingShipment)));

			var cusEntryNumber1 = CreateAdditionalReferenceNumber(parent.PK, "AAA");
			var cusEntryNumber2 = CreateAdditionalReferenceNumber(parent.PK, "BBB");
			cusEntryNumber1.CE_EntryNum = "11111";
			cusEntryNumber2.CE_EntryNum = "22222";

			cusEntryNumber1.Validation.ValidateCE_EntryType();
			cusEntryNumber2.Validation.ValidateCE_EntryType();

			AssertHasErrorContaining(cusEntryNumber1.CE_EntryTypeInfo, "can only be added by Universal XML");
			AssertNoErrorContaining(cusEntryNumber2.CE_EntryTypeInfo, "can only be added by Universal XML");

			var dataImporting = parent as ISupportDataImporting;
			dataImporting.IsImportingData = true;
			cusEntryNumber1.Validation.ValidateCE_EntryType();
			AssertNoErrorContaining(cusEntryNumber1.CE_EntryNumInfo, "can only be added by Universal XML");
			NUnit.Framework.Assert.That(cusEntryNumber1.ReadOnly, NUnit.Framework.Is.EqualTo(false));

			dataImporting.IsImportingData = false;
			Factory.Save();
			NUnit.Framework.Assert.That(cusEntryNumber1.ReadOnly, NUnit.Framework.Is.EqualTo(true));
		}

		public void TestCustomsAuthorisationReference()
		{
			var fakeJob = Factory.New<FakeJob>();
			fakeJob.JobDirection = Directions.Export;
			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.Parent = fakeJob;
			entryNum.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			entryNum.CE_RN_NKCountryCode = "GB";
			entryNum.CE_EntryType = "CAR";
			AssertNoMessageErrorContaining(entryNum.CE_EntryTypeInfo, "CAR is only for United Kingdom");
			entryNum.CE_EntryNum = "POOP";
			AssertHasMessageErrorContaining(entryNum.CE_EntryNumInfo, "match required pattern");
			AssertNoMessageErrorContaining(entryNum.CE_EntryNumInfo, "CAR must start with");
			entryNum.CE_EntryNum = "";
			AssertHasMessageErrorContaining(entryNum.CE_EntryNumInfo, "match required pattern");
			AssertNoMessageErrorContaining(entryNum.CE_EntryNumInfo, "CAR must start with");
			entryNum.CE_EntryNum = "EFB01-XYZ-D1234";
			AssertNoMessageErrorContaining(entryNum.CE_EntryNumInfo, "match required pattern");
			AssertNoMessageErrorContaining(entryNum.CE_EntryNumInfo, "CAR must start with 'E'");
			entryNum.CE_EntryNum = "IFB01-XYZ-D1234";
			AssertNoMessageErrorContaining(entryNum.CE_EntryNumInfo, "match required pattern");
			AssertHasMessageErrorContaining(entryNum.CE_EntryNumInfo, "CAR must start with 'E'");
			AssertNoMessageErrorContaining(entryNum.CE_EntryNumInfo, "CAR must start with 'I'");
			AssertHasMessageErrorContaining(entryNum.CE_EntryNumInfo, "CAR must start with");
			fakeJob.JobDirection = Directions.Import;
			entryNum.CE_EntryNum = "EFB01-XYZ-D1234";
			AssertNoMessageErrorContaining(entryNum.CE_EntryNumInfo, "match required pattern");
			AssertNoMessageErrorContaining(entryNum.CE_EntryNumInfo, "CAR must start with 'E'");
			AssertHasMessageErrorContaining(entryNum.CE_EntryNumInfo, "CAR must start with 'I'");
			AssertHasMessageErrorContaining(entryNum.CE_EntryNumInfo, "CAR must start with");
			entryNum.CE_EntryNum = "IFB01-XYZ-D1234";
			AssertNoMessageErrorContaining(entryNum.CE_EntryNumInfo, "match required pattern");
			AssertNoMessageErrorContaining(entryNum.CE_EntryNumInfo, "CAR must start with 'E'");
			AssertNoMessageErrorContaining(entryNum.CE_EntryNumInfo, "CAR must start with 'I'");
			AssertNoMessageErrorContaining(entryNum.CE_EntryNumInfo, "CAR must start with");
			entryNum.CE_EntryType = "";
			entryNum.CE_RN_NKCountryCode = "AU";
			entryNum.CE_EntryType = "CAR";
			AssertHasMessageErrorContaining(entryNum.CE_EntryTypeInfo, "CAR is only for United Kingdom");
			entryNum.CE_RN_NKCountryCode = "CA";
			entryNum.CE_EntryType = "CCN";
			AssertNoMessageErrorContaining(entryNum.CE_EntryTypeInfo, "CAR is only for United Kingdom");
		}

		public void TestCheckCE_RN_NKCountryCode()
		{
			var num1 = Factory.New<CusEntryNumber>();
			var num2 = Factory.New<CusEntryNumber>();
			var num3 = Factory.New<CusEntryNumber>();

			num1.CE_Category = CusEntryNumber.Categories.InspectionStatus;
			num1.CE_RN_NKCountryCode = Constants.CountryCodes.EuropeanUnion;

			num2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num2.CE_RN_NKCountryCode = Constants.CountryCodes.EuropeanUnion;

			num3.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			num3.CE_RN_NKCountryCode = Constants.CountryCodes.EuropeanUnion;

			AssertNoErrors("EU is valid for INS type CusEntryNumbers", num1.CE_RN_NKCountryCodeInfo);
			AssertHasErrorContaining(num2.CE_RN_NKCountryCodeInfo, ListValidation.InvalidCodeError);
			AssertNoErrors("EU is valid for CUS type CusEntryNumbers", num3.CE_RN_NKCountryCodeInfo);

			num1.CE_RN_NKCountryCode = "XX";
			AssertHasErrorContaining(num1.CE_RN_NKCountryCodeInfo, ListValidation.InvalidCodeError);
		}

		class FakeJob : DummyBusinessObject, IImportExport
		{
			public FakeJob(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public Directions JobDirection { get; set; }
		}

		void AssertCE_EntryNum_UniqueValidation(ZString entryType)
		{
			var parentId = ZGuid.NewZGuid();

			var cusEntryNumber1 = CreateAdditionalReferenceNumber(parentId, entryType);
			var cusEntryNumber2 = CreateAdditionalReferenceNumber(parentId, entryType);

			cusEntryNumber1.Validation.ValidateCE_EntryNum();
			cusEntryNumber2.Validation.ValidateCE_EntryNum();

			AssertHasError(cusEntryNumber1.CE_EntryNumInfo, "There is another entry with the same type and number.");
			AssertHasError(cusEntryNumber2.CE_EntryNumInfo, "There is another entry with the same type and number.");

			cusEntryNumber1.CE_EntryNum = "11111";
			cusEntryNumber2.Validation.ValidateCE_EntryNum();
			AssertNoError(cusEntryNumber1.CE_EntryNumInfo, "There is another entry with the same type and number.");
			AssertNoError(cusEntryNumber2.CE_EntryNumInfo, "There is another entry with the same type and number.");

			cusEntryNumber2.CE_EntryNum = "11111";
			cusEntryNumber1.Validation.ValidateCE_EntryNum();
			AssertHasError(cusEntryNumber1.CE_EntryNumInfo, "There is another entry with the same type and number.");
			AssertHasError(cusEntryNumber2.CE_EntryNumInfo, "There is another entry with the same type and number.");
		}

		CusEntryNumber CreateAdditionalReferenceNumber(ZGuid parentId, ZString entryType)
		{
			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_ParentID = parentId;
			cusEntryNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			cusEntryNumber.CE_EntryType = entryType;
			return cusEntryNumber;
		}
	}
}
