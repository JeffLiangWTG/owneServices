using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(ImportPermitMessageDocumentWrapper))]
sealed class ImportPermitMessageDocumentWrapperTest : InboundMessageDocumentWrapperTest<ImportPermitMessageDocumentWrapper, IImportClearanceNotice>
{
	public void TestHeaderProperties()
	{
		var wrapper = MessageDocumentWrapper;
		CombineAssertions(() =>
		{
			AssertEquals("H_2_1", "21", wrapper.H_2_1);
			AssertEquals("H_2_2", "22", wrapper.H_2_2);
			AssertEquals("H_3", "3", wrapper.H_3);
			AssertEquals("H_4", "4", wrapper.H_4);
			AssertEquals("H_5", "5", wrapper.H_5);
			AssertEquals("H_6", "6", wrapper.H_6);
			AssertEquals("H_7", "7", wrapper.H_7);
			AssertEquals("H_8", "8", wrapper.H_8);
			AssertEquals("H_9", "9", wrapper.H_9);
			AssertEquals("H_10", "10", wrapper.H_10);
			AssertEquals("H_11", "11", wrapper.H_11);
			AssertEquals("H_12", "2025/05/20", wrapper.H_12);
			AssertEquals("H_13", "13", wrapper.H_13);
			AssertEquals("H_14", "14", wrapper.H_14);
			AssertEquals("H_15", "2025/05/23", wrapper.H_15);
			AssertEquals("H_16", "16", wrapper.H_16);
			AssertEquals("H_17", "7", wrapper.H_17);
			AssertEquals("H_18", "8", wrapper.H_18);
			AssertEquals("H_19", "2025/05/21", wrapper.H_19);
			AssertEquals("H_20", "0", wrapper.H_20);
			AssertEquals("H_21", "21", wrapper.H_21);
			AssertEquals("H_22", "22", wrapper.H_22);
			AssertEquals("H_23", "23", wrapper.H_23);
			AssertEquals("H_24", "24", wrapper.H_24);
			AssertEquals("H_25", "25", wrapper.H_25);
			AssertEquals("H_26", "26", wrapper.H_26);
			AssertEquals("H_27", "27", wrapper.H_27);
			AssertEquals("H_28", "28", wrapper.H_28);
			AssertEquals("H_29", "29", wrapper.H_29);
			AssertEquals("H_30", "30", wrapper.H_30);
			AssertEquals("H_31", "31", wrapper.H_31);
			AssertEquals("H_32", "32", wrapper.H_32);
			AssertEquals("H_33", "33", wrapper.H_33);
			AssertEquals("H_34", "34", wrapper.H_34);
			AssertEquals("H_35", "35", wrapper.H_35);
			AssertEquals("H_36", "36", wrapper.H_36);
			AssertEquals("H_37", "37", wrapper.H_37);
			AssertEquals("H_38", "38", wrapper.H_38);
			AssertEquals("H_39", "39", wrapper.H_39);
			AssertEquals("H_40", "40", wrapper.H_40);
			AssertEquals("H_41", "41", wrapper.H_41);
			AssertEquals("H_42", "42", wrapper.H_42);
			AssertEquals("H_43", "43", wrapper.H_43);
			AssertEquals("H_44", "44", wrapper.H_44);
			AssertEquals("H_45", "45", wrapper.H_45);
			AssertEquals("H_46", "46", wrapper.H_46);
			AssertEquals("H_47_1", "471", wrapper.H_47_1);
			AssertEquals("H_47_2", "472", wrapper.H_47_2);
			AssertEquals("H_47_3", "473", wrapper.H_47_3);
			AssertEquals("H_47_4", "474", wrapper.H_47_4);
			AssertEquals("H_47_5", "475", wrapper.H_47_5);
			AssertEquals("H_48", "48", wrapper.H_48);
			AssertEquals("H_49", "49", wrapper.H_49);
			AssertEquals("H_50", "50", wrapper.H_50);
			AssertEquals("H_51", "51", wrapper.H_51);
			AssertEquals("H_52", "52", wrapper.H_52);
			AssertEquals("H_53", "53", wrapper.H_53);
			AssertEquals("H_54", "54", wrapper.H_54);
			AssertEquals("H_55", "2025/06/02", wrapper.H_55);
			AssertEquals("H_56", "56", wrapper.H_56);
			AssertEquals("H_57", "57", wrapper.H_57);
			AssertEquals("H_58", "58", wrapper.H_58);
			AssertEquals("H_59", "59", wrapper.H_59);
			AssertEquals("H_60", "60", wrapper.H_60);
			AssertEquals("H_61", "61", wrapper.H_61);
			AssertEquals("H_62", "62", wrapper.H_62);
			AssertEquals("H_63", "63", wrapper.H_63);
			AssertEquals("H_64", "64", wrapper.H_64);
			AssertEquals("H_65", "65", wrapper.H_65);
			AssertEquals("H_66", 66, wrapper.H_66);
			AssertEquals("H_67", "2025/05/25", wrapper.H_67);
			AssertEquals("H_68", "8", wrapper.H_68);
			AssertEquals("H_69", "69", wrapper.H_69);
			AssertEquals("H_70", "70", wrapper.H_70);
			AssertEquals("H_71", "71", wrapper.H_71);
			AssertEquals("H_72", "2", wrapper.H_72);
			AssertEquals("H_73", "3", wrapper.H_73);
			AssertEquals("H_74_1", "41", wrapper.H_74_1);
			AssertEquals("H_74_2", "42", wrapper.H_74_2);
			AssertEquals("H_74_3", "43", wrapper.H_74_3);
			AssertEquals("H_74_4", "44", wrapper.H_74_4);
			AssertEquals("H_74_5", "45", wrapper.H_74_5);
			AssertEquals("H_75", "75", wrapper.H_75);
			AssertEquals("H_76", "6", wrapper.H_76);
			AssertEquals("H_77", "77", wrapper.H_77);
			AssertEquals("H_78", "8", wrapper.H_78);
			AssertEquals("H_79", "79", wrapper.H_79);
			AssertEquals("H_80", "0", wrapper.H_80);
			AssertEquals("H_81", "81", wrapper.H_81);
			AssertEquals("H_82_1", "821", wrapper.H_82_1);
			AssertEquals("H_83_1", "831", wrapper.H_83_1);
			AssertEquals("H_82_2", "822", wrapper.H_82_2);
			AssertEquals("H_83_2", "832", wrapper.H_83_2);
			AssertEquals("H_82_3", "823", wrapper.H_82_3);
			AssertEquals("H_83_3", "833", wrapper.H_83_3);
			AssertEquals("H_82_4", "824", wrapper.H_82_4);
			AssertEquals("H_83_4", "834", wrapper.H_83_4);
			AssertEquals("H_82_5", "825", wrapper.H_82_5);
			AssertEquals("H_83_5", "835", wrapper.H_83_5);
			AssertEquals("H_82_6", "826", wrapper.H_82_6);
			AssertEquals("H_83_6", "836", wrapper.H_83_6);
			AssertEquals("H_82_7", "827", wrapper.H_82_7);
			AssertEquals("H_83_7", "837", wrapper.H_83_7);
			AssertEquals("H_82_8", "828", wrapper.H_82_8);
			AssertEquals("H_83_8", "838", wrapper.H_83_8);
			AssertEquals("H_82_9", "829", wrapper.H_82_9);
			AssertEquals("H_83_9", "839", wrapper.H_83_9);
			AssertEquals("H_82_10", "8210", wrapper.H_82_10);
			AssertEquals("H_83_10", "8310", wrapper.H_83_10);
			AssertEquals("H_84", "4", wrapper.H_84);
			AssertEquals("H_85", "85", wrapper.H_85);
			AssertEquals("H_86", "86", wrapper.H_86);
			AssertEquals("H_87", "7", wrapper.H_87);
			AssertEquals("H_88", "88", wrapper.H_88);
			AssertEquals("H_89", "USD", wrapper.H_89);
			AssertEquals("H_90", "90", wrapper.H_90);
			AssertEquals("H_91", "9", wrapper.H_91);
			AssertEquals("H_92", "92", wrapper.H_92);
			AssertEquals("H_93", "93", wrapper.H_93);
			AssertEquals("H_94", "4", wrapper.H_94);
			AssertEquals("H_95", "95", wrapper.H_95);
			AssertEquals("H_96", "96", wrapper.H_96);
			AssertEquals("H_97", "97", wrapper.H_97);
			AssertEquals("H_98", "98", wrapper.H_98);
			AssertEquals("H_99", "99", wrapper.H_99);
			AssertEquals("H_100", "0", wrapper.H_100);
			AssertEquals("H_101_1", "1011", wrapper.H_101_1);
			AssertEquals("H_101_2", "1012", wrapper.H_101_2);
			AssertEquals("H_101_3", "1013", wrapper.H_101_3);
			AssertEquals("H_102", "102", wrapper.H_102);
			AssertEquals("H_103", "103", wrapper.H_103);
			AssertEquals("H_104", "104", wrapper.H_104);
			AssertEquals("H_105", "05", wrapper.H_105);
			AssertEquals("H_106_1", "6", wrapper.H_106_1);
			AssertEquals("H_107_1", "71", wrapper.H_107_1);
			AssertEquals("H_108_1", "81", wrapper.H_108_1);
			AssertEquals("H_106_2", "6", wrapper.H_106_2);
			AssertEquals("H_107_2", "82", wrapper.H_107_2);
			AssertEquals("H_108_2", "82", wrapper.H_108_2);
			AssertEquals("H_106_3", "6", wrapper.H_106_3);
			AssertEquals("H_107_3", "73", wrapper.H_107_3);
			AssertEquals("H_108_3", "83", wrapper.H_108_3);
			AssertEquals("H_109_1", "9", wrapper.H_109_1);
			AssertEquals("H_109_2", "09", wrapper.H_109_2);
			AssertEquals("H_110", "10", wrapper.H_110);
			AssertEquals("H_111", "1", wrapper.H_111);
			AssertEquals("H_112", "2", wrapper.H_112);
			AssertEquals("H_113", "3", wrapper.H_113);
			AssertEquals("H_114", "4", wrapper.H_114);
			AssertEquals("H_115", "5", wrapper.H_115);
			AssertEquals("H_116_1", "1", wrapper.H_116_1);
			AssertEquals("H_117_1", "171", wrapper.H_117_1);
			AssertEquals("H_118_1", "1,181", wrapper.H_118_1);
			AssertEquals("H_119_1", 91, wrapper.H_119_1);
			AssertEquals("H_116_2", "2", wrapper.H_116_2);
			AssertEquals("H_117_2", "172", wrapper.H_117_2);
			AssertEquals("H_118_2", "1,182", wrapper.H_118_2);
			AssertEquals("H_119_2", 92, wrapper.H_119_2);
			AssertEquals("H_116_3", "3", wrapper.H_116_3);
			AssertEquals("H_117_3", "173", wrapper.H_117_3);
			AssertEquals("H_118_3", "1,183", wrapper.H_118_3);
			AssertEquals("H_119_3", 93, wrapper.H_119_3);
			AssertEquals("H_116_4", "4", wrapper.H_116_4);
			AssertEquals("H_117_4", "174", wrapper.H_117_4);
			AssertEquals("H_118_4", "1,184", wrapper.H_118_4);
			AssertEquals("H_119_4", 94, wrapper.H_119_4);
			AssertEquals("H_116_5", "5", wrapper.H_116_5);
			AssertEquals("H_117_5", "175", wrapper.H_117_5);
			AssertEquals("H_118_5", "1,185", wrapper.H_118_5);
			AssertEquals("H_119_5", 95, wrapper.H_119_5);
			AssertEquals("H_116_6", "6", wrapper.H_116_6);
			AssertEquals("H_117_6", "176", wrapper.H_117_6);
			AssertEquals("H_118_6", "1,186", wrapper.H_118_6);
			AssertEquals("H_119_6", 96, wrapper.H_119_6);
			AssertEquals("H_116_7", "7", wrapper.H_116_7);
			AssertEquals("H_117_7", "177", wrapper.H_117_7);
			AssertEquals("H_118_7", "1,187", wrapper.H_118_7);
			AssertEquals("H_119_7", 97, wrapper.H_119_7);
			AssertEquals("H_120", "120", wrapper.H_120);
			AssertEquals("H_121", "121", wrapper.H_121);
			AssertEquals("H_122_1", "221", wrapper.H_122_1);
			AssertEquals("H_122_1", "223", wrapper.H_123_1);
			AssertEquals("H_122_2", "222", wrapper.H_122_2);
			AssertEquals("H_122_2", "232", wrapper.H_123_2);
			AssertEquals("H_122_3", "223", wrapper.H_122_3);
			AssertEquals("H_122_3", "233", wrapper.H_123_3);
			AssertEquals("H_124", "4", wrapper.H_124);
			AssertEquals("H_125", "25", wrapper.H_125);
			AssertEquals("H_126", "26", wrapper.H_126);
			AssertEquals("H_127", "127", wrapper.H_127);
			AssertEquals("H_128", "8", wrapper.H_128);
			AssertEquals("H_129", "9", wrapper.H_129);
			AssertEquals("H_130", "130", wrapper.H_130);
			AssertEquals("H_131", "1", wrapper.H_131);
			AssertEquals("H_132", 132, wrapper.H_132);
			AssertEquals("H_133", 33, wrapper.H_133);
			AssertEquals("H_134", "134", wrapper.H_134);
			AssertEquals("H_135", "135", wrapper.H_135);
			AssertEquals("H_136", "136", wrapper.H_136);
			AssertEquals("H_137", "137", wrapper.H_137);
			AssertEquals("H_138", "138", wrapper.H_138);
			AssertEquals("H_139", "139", wrapper.H_139);
			AssertEquals("H_140", "140", wrapper.H_140);
			AssertEquals("H_141", "141", wrapper.H_141);
			AssertEquals("H_142", "142", wrapper.H_142);
			AssertEquals("H_143", "143", wrapper.H_143);
			AssertEquals("H_144", "144", wrapper.H_144);
			AssertEquals("H_145", "2025/07/20", wrapper.H_145);
			AssertEquals("H_146", "2025/05/30", wrapper.H_146);
			AssertEquals("H_147", "47", wrapper.H_147);
			AssertEquals("H_148", "2026/01/01", wrapper.H_148);
			AssertEquals("H_149", "2025/03/25", wrapper.H_149);
			AssertEquals("H_150", 50, wrapper.H_150);
			AssertEquals("H_151", "2025/05/03", wrapper.H_151);
			AssertEquals("H_152", "2025/05/21", wrapper.H_152);
			AssertEquals("H_153", "2025/05/22", wrapper.H_153);
			AssertEquals("H_154", "154", wrapper.H_154);
			AssertEquals("H_155", "155", wrapper.H_155);
			AssertEquals("H_156_1", "1561", wrapper.H_156_1);
			AssertEquals("H_156_2", "1562", wrapper.H_156_2);
			AssertEquals("H_156_3", "1563", wrapper.H_156_3);
			AssertEquals("H_156_4", "1564", wrapper.H_156_4);
			AssertEquals("H_156_5", "1565", wrapper.H_156_5);
			AssertEquals("H_156_6", "1566", wrapper.H_156_6);
			AssertEquals("H_156_7", "1567", wrapper.H_156_7);
			AssertEquals("H_157", "157", wrapper.H_157);
			AssertEquals("H_158_1", "1", wrapper.H_158_1);
			AssertEquals("H_159_1", "1591", wrapper.H_159_1);
			AssertEquals("H_160_1", "2025/05/01", wrapper.H_160_1);
			AssertEquals("H_158_2", "2", wrapper.H_158_2);
			AssertEquals("H_159_2", "1592", wrapper.H_159_2);
			AssertEquals("H_160_2", "2025/05/02", wrapper.H_160_2);
			AssertEquals("H_158_3", "3", wrapper.H_158_3);
			AssertEquals("H_159_3", "1593", wrapper.H_159_3);
			AssertEquals("H_160_3", "2025/05/03", wrapper.H_160_3);
			AssertEquals("H_158_4", "4", wrapper.H_158_4);
			AssertEquals("H_159_4", "1594", wrapper.H_159_4);
			AssertEquals("H_160_4", "2025/05/04", wrapper.H_160_4);
			AssertEquals("H_158_5", "5", wrapper.H_158_5);
			AssertEquals("H_159_5", "1595", wrapper.H_159_5);
			AssertEquals("H_160_5", "2025/05/05", wrapper.H_160_5);
			AssertEquals("H_158_6", "6", wrapper.H_158_6);
			AssertEquals("H_159_6", "1596", wrapper.H_159_6);
			AssertEquals("H_160_6", "2025/05/06", wrapper.H_160_6);
			AssertEquals("H_158_7", "7", wrapper.H_158_7);
			AssertEquals("H_159_7", "1597", wrapper.H_159_7);
			AssertEquals("H_160_7", "2025/05/07", wrapper.H_160_7);
			AssertEquals("H_161", "161", wrapper.H_161);
			AssertEquals("H_162", "162", wrapper.H_162);
			AssertEquals("H_163", "163", wrapper.H_163);
			AssertEquals("H_164", "*164*", wrapper.H_164);
			AssertEquals("H_165", "165", wrapper.H_165);
			AssertEquals("H_166_1", "1661", wrapper.H_166_1);
			AssertEquals("H_166_2", "1662", wrapper.H_166_2);
			AssertEquals("H_167", "167", wrapper.H_167);
			AssertEquals("H_168", 68, wrapper.H_168);
			AssertEquals("H_169", "69", wrapper.H_169);

			AssertEquals("Item count", 99, wrapper.Items.Count);
			AssertEquals("FirstPageItems count", 1, wrapper.FirstPageItems.Count);
			AssertEquals("RemainingItems count", 98, wrapper.RemainingItems.Count);
		});
	}

	public void TestShipmentType()
	{
		AssertEquals("ShipmentType", "I", MessageDocumentWrapper.ShipmentType);
	}

	public void TestTemplateCodeAndType()
	{
		AssertTemplateCodeAndType(JPOutputInformationCodeList.Codes.SAD1AG2, ImportPermitTemplateCodeList.Codes.NA, "輸入許可通知書-A");
		AssertTemplateCodeAndType(JPOutputInformationCodeList.Codes.SAD1BG2, ImportPermitTemplateCodeList.Codes.NB, "輸入許可通知書-B");
		AssertTemplateCodeAndType(JPOutputInformationCodeList.Codes.SAD1CG2, ImportPermitTemplateCodeList.Codes.NC, "輸入許可通知書-C");
		AssertTemplateCodeAndType(JPOutputInformationCodeList.Codes.SAD1BJ1, ImportPermitTemplateCodeList.Codes.BPB, "輸入許可前貨物引取承認通知書-B");
		AssertTemplateCodeAndType(JPOutputInformationCodeList.Codes.SAD1CJ1, ImportPermitTemplateCodeList.Codes.BPC, "輸入許可前貨物引取承認通知書-C");
		AssertTemplateCodeAndType(JPOutputInformationCodeList.Codes.SAD1RG1, ImportPermitTemplateCodeList.Codes.ISTA, "蔵入承認通知書（保税運送承認通知書兼用）-A");
		AssertTemplateCodeAndType(JPOutputInformationCodeList.Codes.SAD1SG1, ImportPermitTemplateCodeList.Codes.ISTB, "蔵入承認通知書（保税運送承認通知書兼用）-B");
		AssertTemplateCodeAndType(JPOutputInformationCodeList.Codes.SAD1TG1, ImportPermitTemplateCodeList.Codes.ISTC, "蔵入承認通知書（保税運送承認通知書兼用）-C");
		AssertTemplateCodeAndType(JPOutputInformationCodeList.Codes.SAD1EG2, ImportPermitTemplateCodeList.Codes.NH, "輸入（引取）許可通知書");
		AssertTemplateCodeAndType(JPOutputInformationCodeList.Codes.SAD15G1, ImportPermitTemplateCodeList.Codes.ISTH, "蔵入輸入（引取）許可通知書");
		AssertTemplateCodeAndType(JPOutputInformationCodeList.Codes.AAD1DG2, ImportPermitTemplateCodeList.Codes.NS, "輸入許可通知書（少額關稅無稅）");
		AssertTemplateCodeAndType(JPOutputInformationCodeList.Codes.AAD1DJ1, ImportPermitTemplateCodeList.Codes.BPS, "輸入許可前貨物引取承認通知書（少額關稅無稅）");
	}

	void AssertTemplateCodeAndType(string outputInfomationCode, string expectedTemplateCode, string expectedTemplateType)
	{
		var documentWrapper = new ImportPermitMessageDocumentWrapper(new JPInboundMessageParseResultForTesting(outputInfomationCode), Factory);
		AssertEquals($"{outputInfomationCode} {expectedTemplateCode}", expectedTemplateCode, documentWrapper.TemplateCode);
		AssertEquals($"{outputInfomationCode} {expectedTemplateType}", expectedTemplateType, documentWrapper.TemplateType);
	}

	void AssertGroupType(string outputInfomationCode, string expectedGroupType)
	{
		var documentWrapper = new ImportPermitMessageDocumentWrapper(new JPInboundMessageParseResultForTesting(outputInfomationCode), Factory);
		AssertEquals($"{outputInfomationCode} {expectedGroupType}", expectedGroupType, documentWrapper.GroupType);
	}

	public void TestGroupType()
	{
		AssertGroupType(JPOutputInformationCodeList.Codes.SAD1AG2, Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GA);
		AssertGroupType(JPOutputInformationCodeList.Codes.SAD1RG1, Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GA);

		AssertGroupType(JPOutputInformationCodeList.Codes.SAD1BG2, Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GB);
		AssertGroupType(JPOutputInformationCodeList.Codes.SAD1SG1, Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GB);
		AssertGroupType(JPOutputInformationCodeList.Codes.SAD1BJ1, Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GB);

		AssertGroupType(JPOutputInformationCodeList.Codes.SAD1CG2, Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GC);
		AssertGroupType(JPOutputInformationCodeList.Codes.SAD1TG1, Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GC);
		AssertGroupType(JPOutputInformationCodeList.Codes.SAD1CJ1, Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GC);

		AssertGroupType(JPOutputInformationCodeList.Codes.SAD1EG2, Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GISTH);
		AssertGroupType(JPOutputInformationCodeList.Codes.SAD15G1, Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GISTH);

		AssertGroupType(JPOutputInformationCodeList.Codes.AAD1DG2, Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GBPS);
		AssertGroupType(JPOutputInformationCodeList.Codes.AAD1DJ1, Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GBPS);
	}

	public void TestSubGroupType()
	{
		AssertSubGroupType(JPOutputInformationCodeList.Codes.SAD1AG2, Constants.DocumentMessageCodes.ImportPermitMessageSubGroupTypes.GNABC);
		AssertSubGroupType(JPOutputInformationCodeList.Codes.SAD1BG2, Constants.DocumentMessageCodes.ImportPermitMessageSubGroupTypes.GNABC);
		AssertSubGroupType(JPOutputInformationCodeList.Codes.SAD1CG2, Constants.DocumentMessageCodes.ImportPermitMessageSubGroupTypes.GNABC);

		AssertSubGroupType(JPOutputInformationCodeList.Codes.SAD1BJ1, Constants.DocumentMessageCodes.ImportPermitMessageSubGroupTypes.GBPBC);
		AssertSubGroupType(JPOutputInformationCodeList.Codes.SAD1CJ1, Constants.DocumentMessageCodes.ImportPermitMessageSubGroupTypes.GBPBC);

		AssertSubGroupType(JPOutputInformationCodeList.Codes.SAD1RG1, Constants.DocumentMessageCodes.ImportPermitMessageSubGroupTypes.GISTABC);
		AssertSubGroupType(JPOutputInformationCodeList.Codes.SAD1SG1, Constants.DocumentMessageCodes.ImportPermitMessageSubGroupTypes.GISTABC);
		AssertSubGroupType(JPOutputInformationCodeList.Codes.SAD1TG1, Constants.DocumentMessageCodes.ImportPermitMessageSubGroupTypes.GISTABC);
	}

	void AssertSubGroupType(string outputInfomationCode, string expectedSubGroupType)
	{
		var documentWrapper = new ImportPermitMessageDocumentWrapper(new JPInboundMessageParseResultForTesting(outputInfomationCode), Factory);
		AssertEquals($"{outputInfomationCode} {expectedSubGroupType}", expectedSubGroupType, documentWrapper.SubGroupType);
	}

	public void TestImportPermitMessageCodes()
	{
		AssertEquals(420, Constants.DocumentMessageCodes.ImportPermitMessageCodes.Length);
	}

	public void TestImportPermitMessage_NA_Codes()
	{
		AssertEquals(32, Constants.DocumentMessageCodes.ImportPermitMessage_NA_Codes.Length);
	}

	public void TestImportPermitMessage_NB_Codes()
	{
		AssertEquals(32, Constants.DocumentMessageCodes.ImportPermitMessage_NB_Codes.Length);
	}

	public void TestImportPermitMessage_NC_Codes()
	{
		AssertEquals(32, Constants.DocumentMessageCodes.ImportPermitMessage_NC_Codes.Length);
	}

	public void TestImportPermitMessage_BPB_Codes()
	{
		AssertEquals(32, Constants.DocumentMessageCodes.ImportPermitMessage_BPB_Codes.Length);
	}

	public void TestImportPermitMessage_BPC_Codes()
	{
		AssertEquals(32, Constants.DocumentMessageCodes.ImportPermitMessage_BPC_Codes.Length);
	}

	public void TestImportPermitMessage_ISTA_Codes()
	{
		AssertEquals(80, Constants.DocumentMessageCodes.ImportPermitMessage_ISTA_Codes.Length);
	}

	public void TestImportPermitMessage_ISTB_Codes()
	{
		AssertEquals(80, Constants.DocumentMessageCodes.ImportPermitMessage_ISTB_Codes.Length);
	}

	public void TestImportPermitMessage_ISTC_Codes()
	{
		AssertEquals(80, Constants.DocumentMessageCodes.ImportPermitMessage_ISTC_Codes.Length);
	}

	public void TestImportPermitMessage_NH_Codes()
	{
		AssertEquals(8, Constants.DocumentMessageCodes.ImportPermitMessage_NH_Codes.Length);
	}

	public void TestImportPermitMessage_ISTH_Codes()
	{
		AssertEquals(4, Constants.DocumentMessageCodes.ImportPermitMessage_ISTH_Codes.Length);
	}

	public void TestImportPermitMessage_NS_Codes()
	{
		AssertEquals(4, Constants.DocumentMessageCodes.ImportPermitMessage_NS_Codes.Length);
	}

	public void TestImportPermitMessage_BPS_Codes()
	{
		AssertEquals(4, Constants.DocumentMessageCodes.ImportPermitMessage_BPS_Codes.Length);
	}

	protected override string GetDefaultMessageTestFile() => "ImportClearancePermitTestMessage.txt";

	class JPInboundMessageParseResultForTesting : IJPInboundMessageParseResult
	{
		public JPInboundMessageParseResultForTesting(string outputInfomationCode)
		{
			ResponseHeader = GetResponseHeader(outputInfomationCode);
		}

		public IJPInboundMessageHeader ResponseHeader { get; internal set; }

		IJPInboundMessageHeader GetResponseHeader(string outputInfomationCode)
		{
			var mockJPInboundMessageHeader = new Mock<IJPInboundMessageHeader>();
			mockJPInboundMessageHeader.Setup(x => x.OutputInformationCode).Returns(outputInfomationCode);
			return mockJPInboundMessageHeader.Object;
		}

		public bool IsSuccess => true;

		public bool HasWarnings => false;

		public bool HasResultCode => true;

		public Error[] Errors => [];

		public string ResultCode => string.Empty;

		public IJPInboundMessageDataProvider MessageProvider => default;
	}
}
