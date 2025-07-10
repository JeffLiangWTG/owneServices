using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(InspectionNoticeMessageDocumentWrapper))]
	sealed class InspectionNoticeMessageDocumentWrapperTest : InboundMessageDocumentWrapperTest<InspectionNoticeMessageDocumentWrapper, IInspectionInformation>
	{
		public void TestHeaderProperties()
		{
			var wrapper = MessageDocumentWrapper;
			CombineAssertions(() =>
			{
				AssertEquals("H_2", "2", wrapper.H_2);
				AssertEquals("H_3", "23334", wrapper.H_3);
				AssertEquals("H_4", "4", wrapper.H_4);
				AssertEquals("H_5", "5", wrapper.H_5);
				AssertEquals("H_6", "6", wrapper.H_6);
				AssertEquals("H_7", "7", wrapper.H_7);
				AssertEquals("H_8", "8", wrapper.H_8);
				AssertEquals("H_9", "9", wrapper.H_9);
				AssertEquals("H_10", "10", wrapper.H_10);
				AssertEquals("H_11", "11", wrapper.H_11);
				AssertEquals("H_12", "12", wrapper.H_12);
				AssertEquals("H_13", "13", wrapper.H_13);
				AssertEquals("H_14", "14", wrapper.H_14);
				AssertEquals("H_15", "15", wrapper.H_15);
				AssertEquals("H_16", "16", wrapper.H_16);
				AssertEquals("H_17", "19,910.17", wrapper.H_17);
				AssertEquals("H_18", "18", wrapper.H_18);
				AssertEquals("H_19", "19,999.19", wrapper.H_19);
				AssertEquals("H_20", "20", wrapper.H_20);
				AssertEquals("H_21", "99,999.21", wrapper.H_21);
				AssertEquals("H_22", "22", wrapper.H_22);
				AssertEquals("H_23", "23", wrapper.H_23);
				AssertEquals("H_24", "24", wrapper.H_24);
				AssertEquals("H_25", "25", wrapper.H_25);
				AssertEquals("H_26", "26", wrapper.H_26);
				AssertEquals("H_27", "7", wrapper.H_27);
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
				AssertEquals("H_38_1", "C1", wrapper.H_38_1);
				AssertEquals("H_38_2", "C2", wrapper.H_38_2);
				AssertEquals("H_38_3", "C3", wrapper.H_38_3);
				AssertEquals("H_38_4", "C4", wrapper.H_38_4);
				AssertEquals("H_38_5", "C5", wrapper.H_38_5);
				AssertEquals("H_39", "9", wrapper.H_39);
				AssertEquals("H_40", "40", wrapper.H_40);
				AssertEquals("H_41", "41", wrapper.H_41);
				AssertEquals("H_42", "42", wrapper.H_42);
				AssertEquals("H_43", "2025/05/07", wrapper.H_43);
				AssertEquals("H_44", "44", wrapper.H_44);
				AssertEquals("H_45", "45", wrapper.H_45);
				AssertEquals("H_46", "46", wrapper.H_46);
				AssertEquals("H_47", "7", wrapper.H_47);
				AssertEquals("H_48", "48", wrapper.H_48);
				AssertEquals("H_49", "49", wrapper.H_49);
				AssertEquals("H_50", "0", wrapper.H_50);
				AssertEquals("H_51", "51", wrapper.H_51);
				AssertEquals("H_52", "2", wrapper.H_52);
				AssertEquals("H_53", "53", wrapper.H_53);
				AssertEquals("H_54", "4", wrapper.H_54);
				AssertEquals("H_55", "55", wrapper.H_55);
				AssertEquals("H_56", "56", wrapper.H_56);
				AssertEquals("H_57", "57", wrapper.H_57);
				AssertEquals("H_58", "58", wrapper.H_58);
				AssertEquals("H_59", "59", wrapper.H_59);
				AssertEquals("H_60", "60", wrapper.H_60);
				AssertEquals("H_61", "1", wrapper.H_61);
				AssertEquals("H_62", "62", wrapper.H_62);
				AssertEquals("H_63", "3", wrapper.H_63);
				AssertEquals("H_64", "4", wrapper.H_64);
				AssertEquals("H_65", "2025/05/06", wrapper.H_65);
				AssertEquals("H_66", "05:16:00", wrapper.H_66);
			});
		}

		public void TestAdditionalProperties()
		{
			var wrapper = MessageDocumentWrapper;
			CombineAssertions(() =>
			{
				AssertEquals("TransportMode", "A", wrapper.TransportMode);
				AssertEquals("TemplateType", "検査指定票（特定輸出申告）", wrapper.TemplateType);
				AssertEquals("ShipmentType", "E", wrapper.ShipmentType);
			});
		}

		protected override string GetDefaultMessageTestFile() => "InspectionInfomationMessage.txt";
	}
}
