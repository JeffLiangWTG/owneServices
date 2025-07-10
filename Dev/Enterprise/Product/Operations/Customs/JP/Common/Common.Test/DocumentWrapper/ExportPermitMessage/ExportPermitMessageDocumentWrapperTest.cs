using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(ExportPermitMessageDocumentWrapper))]
sealed class ExportPermitMessageDocumentWrapperTest : InboundMessageDocumentWrapperTest<ExportPermitMessageDocumentWrapper, IExportClearancePermit>
{
	public void TestHeaderProperties()
	{
		var wrapper = MessageDocumentWrapper;

		CombineAssertions(() =>
		{
			AssertEquals("H_2", "2222", wrapper.H_2);
			AssertEquals("H_3", "3", wrapper.H_3);
			AssertEquals("H_4", "4", wrapper.H_4);
			AssertEquals("H_5", "5", wrapper.H_5);
			AssertEquals("H_6", "6", wrapper.H_6);
			AssertEquals("H_7", "7", wrapper.H_7);
			AssertEquals("H_9", "9999", wrapper.H_9);
			AssertEquals("H_10", "0000000000", wrapper.H_10);
			AssertEquals("H_11", "11", wrapper.H_11);
			AssertEquals("H_12", "2025/01/01", wrapper.H_12);
			AssertEquals("H_13", "33333333333", wrapper.H_13);
			AssertEquals("H_14", "4", wrapper.H_14);
			AssertEquals("H_17", "7", wrapper.H_17);
			AssertEquals("H_18", "88888888888888888", wrapper.H_18);
			AssertEquals("H_19", "9999999999999999999999999999999999999999999999999999999999999999999999", wrapper.H_19);
			AssertEquals("H_20", "0000000", wrapper.H_20);
			AssertEquals("H_21", "111111111111111", wrapper.H_21);
			AssertEquals("H_22", "22222222222222222222222222222222222", wrapper.H_22);
			AssertEquals("H_23", "33333333333333333333333333333333333", wrapper.H_23);
			AssertEquals("H_24", "4444444444444444444444444444444444444444444444444444444444444444444444", wrapper.H_24);
			AssertEquals("H_25", "55555555555", wrapper.H_25);
			AssertEquals("H_26", "66666666666666666", wrapper.H_26);
			AssertEquals("H_27", "7777777777", wrapper.H_27);
			AssertEquals("H_28", "8888888888888888888888888888888888888888888888888888888888888888888888", wrapper.H_28);
			AssertEquals("H_29", "999999999999", wrapper.H_29);
			AssertEquals("H_30", "0000000000000000000000000000000000000000000000000000000000000000000000", wrapper.H_30);
			AssertEquals("H_31", "111111111", wrapper.H_31);
			AssertEquals("H_32", "22222222222222222222222222222222222", wrapper.H_32);
			AssertEquals("H_33", "33333333333333333333333333333333333", wrapper.H_33);
			AssertEquals("H_34", "44444444444444444444444444444444444", wrapper.H_34);
			AssertEquals("H_35", "55555555555555555555555555555555555", wrapper.H_35);
			AssertEquals("H_36", "66", wrapper.H_36);
			AssertEquals("H_37", "77777", wrapper.H_37);
			AssertEquals("H_38", "88888888888888888888888888888888888888888888888888", wrapper.H_38);
			AssertEquals("H_39", "99999", wrapper.H_39);
			AssertEquals("H_40", "00000", wrapper.H_40);
			AssertEquals("H_41", "11111111111111111111111111111111111", wrapper.H_41);
			AssertEquals("H_42", "22,222,222", wrapper.H_42);
			AssertEquals("H_43", "333", wrapper.H_43);
			AssertEquals("H_44", "44444444444444444444444444444444444", wrapper.H_44);
			AssertEquals("H_45", "5,555,555,555", wrapper.H_45);
			AssertEquals("H_46", "666", wrapper.H_46);
			AssertEquals("H_47", "77777", wrapper.H_47);
			AssertEquals("H_48", "88888888888888888888", wrapper.H_48);
			AssertEquals("H_49", "9999999999", wrapper.H_49);
			AssertEquals("H_50", "00", wrapper.H_50);
			AssertEquals("H_51", "11111", wrapper.H_51);
			AssertEquals("H_52", "22222222222222222222", wrapper.H_52);
			AssertEquals("H_53", "3", wrapper.H_53);
			AssertEquals("H_54", "44444", wrapper.H_54);
			AssertEquals("H_55", "55555555555555555555", wrapper.H_55);
			AssertEquals("H_56", "666", wrapper.H_56);
			AssertEquals("H_57", "77777", wrapper.H_57);
			AssertEquals("H_58", "888888888", wrapper.H_58);
			AssertEquals("H_59", "99999999999999999999999999999999999", wrapper.H_59);
			AssertEquals("H_60", "2025/01/02", wrapper.H_60);
			AssertEquals("H_61", "1", wrapper.H_61);
			AssertEquals("H_62", "2", wrapper.H_62);
			AssertEquals("H_63", "3", wrapper.H_63);
			AssertEquals("H_64", "44444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444", wrapper.H_64);
			AssertEquals("H_65", "55", wrapper.H_65);
			AssertEquals("H_66_1", "6661", wrapper.H_66_1);
			AssertEquals("H_66_2", "6662", wrapper.H_66_2);
			AssertEquals("H_66_3", "6663", wrapper.H_66_3);
			AssertEquals("H_66_4", "6664", wrapper.H_66_4);
			AssertEquals("H_66_5", "6665", wrapper.H_66_5);
			AssertEquals("H_66_6", "6666", wrapper.H_66_6);
			AssertEquals("H_66_7", "6667", wrapper.H_66_7);
			AssertEquals("H_66_8", "6668", wrapper.H_66_8);
			AssertEquals("H_66_9", "6669", wrapper.H_66_9);
			AssertEquals("H_66_10", "6610", wrapper.H_66_10);
			AssertEquals("H_66_11", "6611", wrapper.H_66_11);
			AssertEquals("H_66_12", "6612", wrapper.H_66_12);
			AssertEquals("H_66_13", "6613", wrapper.H_66_13);
			AssertEquals("H_66_14", "6614", wrapper.H_66_14);
			AssertEquals("H_66_15", "6615", wrapper.H_66_15);
			AssertEquals("H_67_1", "77777777777777777771", wrapper.H_67_1);
			AssertEquals("H_67_2", "77777777777777777772", wrapper.H_67_2);
			AssertEquals("H_67_3", "77777777777777777773", wrapper.H_67_3);
			AssertEquals("H_67_4", "77777777777777777774", wrapper.H_67_4);
			AssertEquals("H_67_5", "77777777777777777775", wrapper.H_67_5);
			AssertEquals("H_67_6", "77777777777777777776", wrapper.H_67_6);
			AssertEquals("H_67_7", "77777777777777777777", wrapper.H_67_7);
			AssertEquals("H_67_8", "77777777777777777778", wrapper.H_67_8);
			AssertEquals("H_67_9", "77777777777777777779", wrapper.H_67_9);
			AssertEquals("H_67_10", "77777777777777777710", wrapper.H_67_10);
			AssertEquals("H_67_11", "77777777777777777711", wrapper.H_67_11);
			AssertEquals("H_67_12", "77777777777777777712", wrapper.H_67_12);
			AssertEquals("H_67_13", "77777777777777777713", wrapper.H_67_13);
			AssertEquals("H_67_14", "77777777777777777714", wrapper.H_67_14);
			AssertEquals("H_67_15", "77777777777777777715", wrapper.H_67_15);
			AssertEquals("H_68", "8", wrapper.H_68);
			AssertEquals("H_69", "99999999999999999999999999999999999", wrapper.H_69);
			AssertEquals("H_70", "0000000000", wrapper.H_70);
			AssertEquals("H_71", "111", wrapper.H_71);
			AssertEquals("H_72", "222", wrapper.H_72);
			AssertEquals("H_73", "333,333,333,333,333,333", wrapper.H_73);
			AssertEquals("H_74", "4", wrapper.H_74);
			AssertEquals("H_75", "555", wrapper.H_75);
			AssertEquals("H_76", "666,666,666,666,666,666", wrapper.H_76);
			AssertEquals("H_77_1", "771", wrapper.H_77_1);
			AssertEquals("H_78_1", "888888881", wrapper.H_78_1);
			AssertEquals("H_77_2", "772", wrapper.H_77_2);
			AssertEquals("H_78_2", "888888882", wrapper.H_78_2);
			AssertEquals("H_79", "999,999,999,999,999,999", wrapper.H_79);
			AssertEquals("H_80", "0", wrapper.H_80);
			AssertEquals("H_81", "11", wrapper.H_81);
			AssertEquals("H_82", "22", wrapper.H_82);
			AssertEquals("H_83_1", "33333333333333331", wrapper.H_83_1);
			AssertEquals("H_83_2", "33333333333333332", wrapper.H_83_2);
			AssertEquals("H_83_3", "33333333333333333", wrapper.H_83_3);
			AssertEquals("H_83_4", "33333333333333334", wrapper.H_83_4);
			AssertEquals("H_83_5", "33333333333333335", wrapper.H_83_5);
			AssertEquals("H_84", "4444444444444444444444444444444444444444444444444444444444444444444444", wrapper.H_84);
			AssertEquals("H_85", "555555555555555", wrapper.H_85);
			AssertEquals("H_86", "66666666666666666666666666666666666", wrapper.H_86);
			AssertEquals("H_87", "77777777777777777777777777777777777", wrapper.H_87);
			AssertEquals("H_88", "8888888888888888888888888888888888888888888888888888888888888888888888", wrapper.H_88);
			AssertEquals("H_90", "100", wrapper.H_90);
			AssertEquals("H_91", "11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111", wrapper.H_91);
			AssertEquals("H_92", "2222222222222222222222222222222222222222222222222222222222222222222222", wrapper.H_92);
			AssertEquals("H_93", "3333333333333333333333333333333333333333333333333333333333333333333333", wrapper.H_93);
			AssertEquals("H_94", "44444444444444444444", wrapper.H_94);
			AssertEquals("H_95", "55555555555555555555555555555555555", wrapper.H_95);
			AssertEquals("H_96", "66666666666666666666", wrapper.H_96);
			AssertEquals("H_97", "77777", wrapper.H_97);
			AssertEquals("H_98", "888888888888", wrapper.H_98);
			AssertEquals("H_100", "0000000000000000000000000000000000000000000000000000000000000000000000", wrapper.H_100);
			AssertEquals("H_101", "2025/01/03", wrapper.H_101);
			AssertEquals("H_102", "222222222222222222222222222222222222", wrapper.H_102);
			AssertEquals("H_103", "2025/01/04", wrapper.H_103);
			AssertEquals("H_104", "2025/01/05", wrapper.H_104);
		});
	}

	public void TestItems()
	{
		AssertEquals(1, MessageDocumentWrapper.Items.Count);
	}

	protected override string GetDefaultMessageTestFile() => "ExportClearancePermitTestMessage.txt";
}
