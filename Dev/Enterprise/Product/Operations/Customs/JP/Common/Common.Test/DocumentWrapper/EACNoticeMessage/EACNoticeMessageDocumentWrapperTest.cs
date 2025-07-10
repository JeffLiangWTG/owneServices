using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(EACNoticeMessageDocumentWrapper))]
	sealed class EACNoticeMessageDocumentWrapperTest : InboundMessageDocumentWrapperTest<EACNoticeMessageDocumentWrapper, IEACNoticeResponse>
	{
		public void TestHeaderProperties()
		{
			var wrapper = MessageDocumentWrapper;
			CombineAssertions(() =>
			{
				AssertEquals("H_2", "X", wrapper.H_2);
				AssertEquals("H_3", "A", wrapper.H_3);
				AssertEquals("H_4", "B", wrapper.H_4);
				AssertEquals("H_5", "C", wrapper.H_5);
				AssertEquals("H_6", "D", wrapper.H_6);
				AssertEquals("H_7", "E", wrapper.H_7);
				AssertEquals("H_8", "AAAA", wrapper.H_8);
				AssertEquals("H_9", "AAAAAAAAA1", wrapper.H_9);
				AssertEquals("H_10", "AA", wrapper.H_10);
				AssertEquals("H_11", "2024/01/01", wrapper.H_11);
				AssertEquals("H_12", "AAAAAAAAA1A", wrapper.H_12);
				AssertEquals("H_13", "AAAA1", wrapper.H_13);
				AssertEquals("H_14", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA1", wrapper.H_14);
				AssertEquals("H_15", "AAAA2", wrapper.H_15);
				AssertEquals("H_16", "AAAA3", wrapper.H_16);
				AssertEquals("H_17", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA2", wrapper.H_17);
				AssertEquals("H_18", "AAAA4", wrapper.H_18);
				AssertEquals("H_19", "AAAAAAAAA1AAAAAA1", wrapper.H_19);
				AssertEquals("H_20", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA5AAAAAAAAA6AAAAAAAAA1", wrapper.H_20);
				AssertEquals("H_21", "AAAAAAA", wrapper.H_21);
				AssertEquals("H_22", "AAAAAAAAA1AAAAA", wrapper.H_22);
				AssertEquals("H_23", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAA1", wrapper.H_23);
				AssertEquals("H_24", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAA2", wrapper.H_24);
				AssertEquals("H_25", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA5AAAAAAAAA6AAAAAAAAA2", wrapper.H_25);
				AssertEquals("H_26", "AAAAAAAAA1A", wrapper.H_26);
				AssertEquals("H_27", "AAAAAAAAA1AAAAAA2", wrapper.H_27);
				AssertEquals("H_28", "AAAAAAAAA2", wrapper.H_28);
				AssertEquals("H_29", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA5AAAAAAAAA6AAAAAAAAA3", wrapper.H_29);
				AssertEquals("H_30", "AAAAAAAAA1AA", wrapper.H_30);
				AssertEquals("H_31", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA5AAAAAAAAA6AAAAAAAAA4", wrapper.H_31);
				AssertEquals("H_32", "AAAAAAAAA", wrapper.H_32);
				AssertEquals("H_33", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAA3", wrapper.H_33);
				AssertEquals("H_34", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAA4", wrapper.H_34);
				AssertEquals("H_35", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAA5", wrapper.H_35);
				AssertEquals("H_36", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAA6", wrapper.H_36);
				AssertEquals("H_37", "AA", wrapper.H_37);
				AssertEquals("H_38", "AAAAAAAAA1A", wrapper.H_38);
				AssertEquals("H_39", "2024/01/02", wrapper.H_39);
				AssertEquals("H_40", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA5AAAAAAAAA6AAAAAAAAA7AAAAAAAAA8AAAAAAAAA9AAAAAAAAA:AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4", wrapper.H_40);
				AssertEquals("H_41", "A", wrapper.H_41);
				AssertEquals("H_42", "CC", wrapper.H_42);
				AssertEquals("H_43", "2024/01/03", wrapper.H_43);
				AssertEquals("H_44", "2024/01/04", wrapper.H_44);
				AssertEquals("H_45", "AAAAAAAAA", wrapper.H_45);
				AssertEquals("H_46", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAA7", wrapper.H_46);
				AssertEquals("H_47", "AAAAAAAAA", wrapper.H_47);
				AssertEquals("H_48", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAA8", wrapper.H_48);
				AssertEquals("H_49", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAA9", wrapper.H_49);
				AssertEquals("H_50", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAA10", wrapper.H_50);
				AssertEquals("H_51", "2024/01/05", wrapper.H_51);
				AssertEquals("H_52", "2024/01/06", wrapper.H_52);
				AssertEquals("H_53", "AAAA5", wrapper.H_53);
				AssertEquals("H_54", "AAAAAAAAA1AAAAAAAAA2", wrapper.H_54);
				AssertEquals("H_55", "AAAA6", wrapper.H_55);
				AssertEquals("H_56", "AAAAAAAAA1AAAAAAAAA2", wrapper.H_56);
				AssertEquals("H_57", "AAAA7", wrapper.H_57);
				AssertEquals("H_58", "AAAAAAAAA1AAAAAAAAA2", wrapper.H_58);
				AssertEquals("H_59", "AAAA8", wrapper.H_59);
				AssertEquals("H_60", "AAAAAAAAA1AAAAAAAAA2", wrapper.H_60);
				AssertEquals("H_61", "AAAAAAAAA1AAAAAA3", wrapper.H_61);
				AssertEquals("H_62", "AAAAAAAAA1AAAAAA4", wrapper.H_62);
				AssertEquals("H_63", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA5AAAAAAAAA6AAAAAAAAA5", wrapper.H_63);
				AssertEquals("H_64", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA5AAAAAAAAA6AAAAAAAAA6", wrapper.H_64);
				AssertEquals("H_65_1", "AAAAAAAAA1AAAAAA5", wrapper.H_65_1);
				AssertEquals("H_65_2", "AAAAAAAAA1AAAAAA6", wrapper.H_65_2);
				AssertEquals("H_65_3", "AAAAAAAAA1AAAAAA7", wrapper.H_65_3);
				AssertEquals("H_65_4", "AAAAAAAAA1AAAAAA8", wrapper.H_65_4);
				AssertEquals("H_66_1", "AAAAAAAAA1AAAAAA9", wrapper.H_66_1);
				AssertEquals("H_66_2", "AAAAAAAAA1AAAAA10", wrapper.H_66_2);
				AssertEquals("H_66_3", "AAAAAAAAA1AAAAA11", wrapper.H_66_3);
				AssertEquals("H_66_4", "AAAAAAAAA1AAAAA12", wrapper.H_66_4);
				AssertEquals("H_67", "20,240,107", wrapper.H_67);
				AssertEquals("H_68", "AA1", wrapper.H_68);
				AssertEquals("H_69", "20,240,108", wrapper.H_69);
				AssertEquals("H_70", "AA2", wrapper.H_70);
				AssertEquals("H_71", "1,111,111,111", wrapper.H_71);
				AssertEquals("H_72", "AA3", wrapper.H_72);
				AssertEquals("H_73", "1,111,111,112", wrapper.H_73);
				AssertEquals("H_74", "AA4", wrapper.H_74);
				AssertEquals("H_75", "AA5", wrapper.H_75);
				AssertEquals("H_76", "AA6", wrapper.H_76);
				AssertEquals("H_77", "111,111,111,111,111,111", wrapper.H_77);
				AssertEquals("H_78", "A", wrapper.H_78);
				AssertEquals("H_79", "AA7", wrapper.H_79);
				AssertEquals("H_80", "AA8", wrapper.H_80);
				AssertEquals("H_81", "111,111,111,111,111,112", wrapper.H_81);
				AssertEquals("H_82", "A", wrapper.H_82);
				AssertEquals("H_83", "AA9", wrapper.H_83);
				AssertEquals("H_84", "111,111,111,111,111,113", wrapper.H_84);
				AssertEquals("H_85", "A10", wrapper.H_85);
				AssertEquals("H_86", "111,111,111,111,111,114", wrapper.H_86);
				AssertEquals("H_87", "AAAAAAAAA3", wrapper.H_87);
				AssertEquals("H_88", "AA", wrapper.H_88);
				AssertEquals("H_89", "DD", wrapper.H_89);
				AssertEquals("H_90", "EE", wrapper.H_90);
				AssertEquals("H_91", "FF", wrapper.H_91);
				AssertEquals("H_92", "AAAAAAAAA1AAAAAAAAA2", wrapper.H_92);
				AssertEquals("H_93", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAA11", wrapper.H_93);
				AssertEquals("H_94", "AAAAAAAAA1AAAAAAAAA2", wrapper.H_94);
				AssertEquals("H_95", "AAAA9", wrapper.H_95);
				AssertEquals("H_96", "AAAAAAAAA1AA", wrapper.H_96);
				AssertEquals("H_97", "2024/01/09", wrapper.H_97);
				AssertEquals("H_98", "E1", wrapper.H_98);
				AssertEquals("H_99", "E2", wrapper.H_99);
				AssertEquals("H_100", "E3", wrapper.H_100);
				AssertEquals("H_101", "2024/01/10", wrapper.H_101);
			});
		}

		public void TestAdditionalProperties()
		{
			var wrapper = MessageDocumentWrapper;
			CombineAssertions(() =>
			{
				AssertEquals("TransportMode", "S", wrapper.TransportMode);
				AssertEquals("TemplateType", "輸出許可内容変更通知書", wrapper.TemplateType);
			});
		}

		protected override string GetDefaultMessageTestFile() => "EACNoticeInfomationMessage.txt";
	}
}
