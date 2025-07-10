using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class CusEntryInstructionDigitCheckHelperTest : TestCaseWithFactory
	{
		public void TestStyle1stDigitIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
				AssertEquals("0XXXXX", true, entryInstruction.Style1stDigitIs0());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._200000;
				AssertEquals("Not 0XXXXX", false, entryInstruction.Style1stDigitIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style1stDigitIs0(null));
			});
		}

		public void TestStyle1stDigitIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120000;
				AssertEquals("1XXXXX", true, entryInstruction.Style1stDigitIs1());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
				AssertEquals("Not 1XXXXX", false, entryInstruction.Style1stDigitIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style1stDigitIs1(null));
			});
		}

		public void TestStyle1stDigitIs2()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._200100;
				AssertEquals("2XXXXX", true, entryInstruction.Style1stDigitIs2());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
				AssertEquals("Not 2XXXXX", false, entryInstruction.Style1stDigitIs2());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style1stDigitIs2(null));
			});
		}

		public void TestStyle1stDigitIs1And2ndIs2()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120000;
				AssertEquals("12XXXX", true, entryInstruction.Style1stDigitIs1And2ndIs2());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
				AssertEquals("Not 12XXXX", false, entryInstruction.Style1stDigitIs1And2ndIs2());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style1stDigitIs1And2ndIs2(null));
			});
		}

		public void TestStyle2ndDigitIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._201310;
				AssertEquals("X0XXXX", true, entryInstruction.Style2ndDigitIs0());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110100;
				AssertEquals("Not X0XXXX", false, entryInstruction.Style2ndDigitIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style2ndDigitIs0(null));
			});
		}

		public void TestStyle2ndDigitIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111310;
				AssertEquals("X1XXXX", true, entryInstruction.Style2ndDigitIs1());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120000;
				AssertEquals("Not X1XXXX", false, entryInstruction.Style2ndDigitIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style2ndDigitIs1(null));
			});
		}

		public void TestStyle2ndDigitIs2()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120110;
				AssertEquals("X2XXXX", true, entryInstruction.Style2ndDigitIs2());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110110;
				AssertEquals("Not X2XXXX", false, entryInstruction.Style2ndDigitIs2());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style2ndDigitIs2(null));
			});
		}

		public void TestStyle3rdDigitIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._201300;
				AssertEquals("XX1XXX", true, entryInstruction.Style3rdDigitIs1());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
				AssertEquals("Not XX1XXX", false, entryInstruction.Style3rdDigitIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style3rdDigitIs1(null));
			});
		}

		public void TestStyle4thDigitIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
				AssertEquals("XXX1XX", true, entryInstruction.Style4thDigitIs1());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._201300;
				AssertEquals("Not XXX1XX", false, entryInstruction.Style4thDigitIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style4thDigitIs1(null));
			});
		}

		public void TestStyle4thDigitIs2()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000200;
				AssertEquals("XXX2XX", true, entryInstruction.Style4thDigitIs2());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;
				AssertEquals("Not XXX2XX", false, entryInstruction.Style4thDigitIs2());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style4thDigitIs2(null));
			});
		}

		public void TestStyle4thDigitIs3()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._001300;
				AssertEquals("XXX3XX", true, entryInstruction.Style4thDigitIs3());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000200;
				AssertEquals("Not XXX3XX", false, entryInstruction.Style4thDigitIs3());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style4thDigitIs3(null));
			});
		}

		public void TestStyle4thDigitIs4()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000400;
				AssertEquals("XXX4XX", true, entryInstruction.Style4thDigitIs4());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;
				AssertEquals("Not XXX4XX", false, entryInstruction.Style4thDigitIs4());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style4thDigitIs4(null));
			});
		}

		public void TestStyle4thDigitIs9()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000902;
				AssertEquals("XXX9XX", true, entryInstruction.Style4thDigitIs9());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000400;
				AssertEquals("Not XXX9XX", false, entryInstruction.Style4thDigitIs9());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style4thDigitIs9(null));
			});
		}

		public void TestStyle5thDigitIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110200;
				AssertEquals("XXXX0X", true, entryInstruction.Style5thDigitIs0());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000210;
				AssertEquals("Not XXXX0X", false, entryInstruction.Style5thDigitIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style5thDigitIs0(null));
			});
		}

		public void TestStyle5thDigitIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000210;
				AssertEquals("XXXX1X", true, entryInstruction.Style5thDigitIs1());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110200;
				AssertEquals("Not XXXX1X", false, entryInstruction.Style5thDigitIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style5thDigitIs1(null));
			});
		}

		public void TestStyleFirstThreeDigitsAre1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
				AssertEquals("111XXX", true, entryInstruction.StyleFirstThreeDigitsAre1());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110200;
				AssertEquals("Not XXXX1X", false, entryInstruction.StyleFirstThreeDigitsAre1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.StyleFirstThreeDigitsAre1(null));
			});
		}

		public void TestStyle2ndDigitIs0And3rdDigitIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._001300;
				AssertEquals("X01XXX", true, entryInstruction.Style2ndDigitIs0And3rdDigitIs1());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110200;
				AssertEquals("Not X01XXX", false, entryInstruction.Style2ndDigitIs0And3rdDigitIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Style2ndDigitIs0And3rdDigitIs1(null));
			});
		}

		public void TestIsSDEExport()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._001310;
				AssertEquals("X01XXX", true, entryInstruction.IsSDEExport());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110100;
				AssertEquals("Not X01XXX", false, entryInstruction.IsSDEExport());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.IsSDEExport(null));
			});
		}

		public void TestIsSDEOutwardProcessing()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
				AssertEquals("111XXX", true, entryInstruction.IsSDEOutwardProcessing());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
				AssertEquals("Not 111XXX", false, entryInstruction.IsSDEOutwardProcessing());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.IsSDEOutwardProcessing(null));
			});
		}

		public void TestIsSDEExportOrSDEOutwardProcessing()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._001300;
				AssertEquals("IsSDE", true, CusEntryInstructionDigitCheckHelper.IsSDEExportOrSDEOutwardProcessing(entryInstruction));
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
				AssertEquals("IsSDEOutwardProcessing", true, CusEntryInstructionDigitCheckHelper.IsSDEExportOrSDEOutwardProcessing(entryInstruction));
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000200;
				AssertEquals("Unmatched", false, CusEntryInstructionDigitCheckHelper.IsSDEExportOrSDEOutwardProcessing(entryInstruction));
			});
		}

		public void TestIsCCLExport()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000410;
				AssertEquals("Matched", true, CusEntryInstructionDigitCheckHelper.IsCCLExport(entryInstruction));
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000110;
				AssertEquals("Unmatched", false, CusEntryInstructionDigitCheckHelper.IsCCLExport(entryInstruction));
			});
		}

		public void TestIsOPOOutwardProcessing()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
				AssertEquals("11XXXX", true, entryInstruction.IsOPOOutwardProcessing());

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
				AssertEquals("Not 11XXXX", false, entryInstruction.IsOPOOutwardProcessing());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.IsOPOOutwardProcessing(null));
			});
		}

		public void TestSubStyle1stDigitIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
				AssertEquals("0X", true, entryInstruction.SubStyle1stDigitIs0());

				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
				AssertEquals("Not 0X", false, entryInstruction.SubStyle1stDigitIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.SubStyle1stDigitIs0(null));
			});
		}

		public void TestSubStyle1stDigitIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
				AssertEquals("1X", true, entryInstruction.SubStyle1stDigitIs1());

				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
				AssertEquals("Not 1X", false, entryInstruction.SubStyle1stDigitIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.SubStyle1stDigitIs1(null));
			});
		}

		public void TestSubStyle1stDigitIs2()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
				AssertEquals("2X", true, entryInstruction.SubStyle1stDigitIs2());

				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
				AssertEquals("Not 2X", false, entryInstruction.SubStyle1stDigitIs2());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.SubStyle1stDigitIs2(null));
			});
		}

		public void TestConstellation1stDigitIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0100;
				AssertEquals("0XXX", true, entryInstruction.Constellation1stDigitIs0());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._1000;
				AssertEquals("Not 0XXX", false, entryInstruction.Constellation1stDigitIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation1stDigitIs0(null));
			});
		}

		public void TestConstellation1stDigitIs0And2ndIs1And3rdIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0101;
				AssertEquals("010X", true, entryInstruction.Constellation1stDigitIs0And2ndIs1And3rdIs0());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0110;
				AssertEquals("Not 010X", false, entryInstruction.Constellation1stDigitIs0And2ndIs1And3rdIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation1stDigitIs0And2ndIs1And3rdIs0(null));
			});
		}

		public void TestConstellation1stDigitIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._1000;
				AssertEquals("1XXX", true, entryInstruction.Constellation1stDigitIs1());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0100;
				AssertEquals("Not 1XXX", false, entryInstruction.Constellation1stDigitIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation1stDigitIs1(null));
			});
		}

		public void TestConstellation1stDigitIs1And2ndIs1And3rdIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._1100;
				AssertEquals("110X", true, entryInstruction.Constellation1stDigitIs1And2ndIs1And3rdIs0());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0100;
				AssertEquals("Not 110X", false, entryInstruction.Constellation1stDigitIs1And2ndIs1And3rdIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation1stDigitIs1And2ndIs1And3rdIs0(null));
			});
		}

		public void TestConstellation2ndDigitIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._1010;
				AssertEquals("X0XX", true, entryInstruction.Constellation2ndDigitIs0());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0110;
				AssertEquals("Not X0XX", false, entryInstruction.Constellation2ndDigitIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation2ndDigitIs0(null));
			});
		}

		public void TestConstellation2ndDigitIs0And3rdIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;
				AssertEquals("X00X", true, entryInstruction.Constellation2ndDigitIs0And3rdIs0());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				AssertEquals("Not X00X", false, entryInstruction.Constellation2ndDigitIs0And3rdIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation2ndDigitIs0And3rdIs0(null));
			});
		}

		public void TestConstellation2ndDigitIs0And3rdIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				AssertEquals("X01X", true, entryInstruction.Constellation2ndDigitIs0And3rdIs1());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;
				AssertEquals("Not X01X", false, entryInstruction.Constellation2ndDigitIs0And3rdIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation2ndDigitIs0And3rdIs1(null));
			});
		}

		public void TestConstellation2ndDigitIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0100;
				AssertEquals("X1XX", true, entryInstruction.Constellation2ndDigitIs1());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._1000;
				AssertEquals("Not X1XX", false, entryInstruction.Constellation2ndDigitIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation2ndDigitIs1(null));
			});
		}

		public void TestConstellation2ndDigitIs1And3rdIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0100;
				AssertEquals("X10X", true, entryInstruction.Constellation2ndDigitIs1And3rdIs0());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0111;
				AssertEquals("Not X10X", false, entryInstruction.Constellation2ndDigitIs1And3rdIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation2ndDigitIs1And3rdIs0(null));
			});
		}

		public void TestConstellation2ndDigitIs0And4thDigitIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;
				AssertEquals("X0X1", true, entryInstruction.Constellation2ndDigitIs0And4thDigitIs1());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0111;
				AssertEquals("Not X0X1", false, entryInstruction.Constellation2ndDigitIs0And4thDigitIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation2ndDigitIs0And4thDigitIs1(null));
			});
		}

		public void TestConstellation2ndDigitIs1And4thDigitIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0101;
				AssertEquals("X1X1", true, entryInstruction.Constellation2ndDigitIs1And4thDigitIs1());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;
				AssertEquals("Not X1X1", false, entryInstruction.Constellation2ndDigitIs1And4thDigitIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation2ndDigitIs1And4thDigitIs1(null));
			});
		}

		public void TestConstellation2ndDigitIs1And3rdIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0111;
				AssertEquals("X11X", true, entryInstruction.Constellation2ndDigitIs1And3rdIs1());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0100;
				AssertEquals("Not X11X", false, entryInstruction.Constellation2ndDigitIs1And3rdIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation2ndDigitIs1And3rdIs1(null));
			});
		}

		public void TestConstellation3rdDigitIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0100;
				AssertEquals("XX0X", true, entryInstruction.Constellation3rdDigitIs0());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0011;
				AssertEquals("Not XX0X", false, entryInstruction.Constellation3rdDigitIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation3rdDigitIs0(null));
			});
		}

		public void TestConstellation3rdDigitIs0And4thIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0100;
				AssertEquals("XX00", true, entryInstruction.Constellation3rdDigitIs0And4thIs0());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._1010;
				AssertEquals("Not XX00", false, entryInstruction.Constellation3rdDigitIs0And4thIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation3rdDigitIs0And4thIs0(null));
			});
		}

		public void TestConstellation3rdDigitIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0011;
				AssertEquals("XX1X", true, entryInstruction.Constellation3rdDigitIs1());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;
				AssertEquals("Not XX1X", false, entryInstruction.Constellation3rdDigitIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation3rdDigitIs1(null));
			});
		}

		public void TestConstellation3rdDigitIs1And4thIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				AssertEquals("XX10", true, entryInstruction.Constellation3rdDigitIs1And4thIs0());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0101;
				AssertEquals("Not XX10", false, entryInstruction.Constellation3rdDigitIs1And4thIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation3rdDigitIs1And4thIs0(null));
			});
		}

		public void TestConstellation4thDigitIs0()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				AssertEquals("XXX0", true, entryInstruction.Constellation4thDigitIs0());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0101;
				AssertEquals("Not XXX0", false, entryInstruction.Constellation4thDigitIs0());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation4thDigitIs0(null));
			});
		}

		public void TestConstellation4thDigitIs1()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0101;
				AssertEquals("XXX1", true, entryInstruction.Constellation4thDigitIs1());

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				AssertEquals("Not XXX1", false, entryInstruction.Constellation4thDigitIs1());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation4thDigitIs1(null));
			});
		}

		public void TestConstellation4thDigitIs3()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = "0003";
				AssertEquals("XXX3", true, entryInstruction.Constellation4thDigitIs3());

				entryInstruction.ZG_PartyConstellation = "1122";
				AssertEquals("Not XXX3", false, entryInstruction.Constellation4thDigitIs3());

				AssertEquals("entryInstruction is null", false, CusEntryInstructionDigitCheckHelper.Constellation4thDigitIs3(null));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
		}
		CusEntryInstruction entryInstruction;
	}
}
