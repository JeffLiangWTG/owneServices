using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class DefaultDebtorList : CodeDescriptionPairList
	{
		public DefaultDebtorList()
			: base()
		{
			AddPair(Codes.PrepaidBillToParty, Descriptions.PrepaidBillToParty);
			AddPair(Codes.CollectBillToParty, Descriptions.CollectBillToParty);
			AddPair(Codes.ControlCustomerFallToPrepaid, Descriptions.ControlCustomerFallToPrepaid);
			AddPair(Codes.ControlCustomerFallToCollect, Descriptions.ControlCustomerFallToCollect);
		}

		public static class Codes
		{
			public const string PrepaidBillToParty = "PBP";
			public const string CollectBillToParty = "CBP";
			public const string ControlCustomerFallToPrepaid = "CCP";
			public const string ControlCustomerFallToCollect = "CCC";
		}

		public static class Descriptions
		{
			public static MultilingualString PrepaidBillToParty = ResString.GetMultilingualString("E409D355-BF6D-44B1-9BF0-19C0144CD2D5", "Prepaid Bill-To Party");
			public static MultilingualString CollectBillToParty = ResString.GetMultilingualString("7C231759-5380-480C-B50A-18B3902283A8", "Collect Bill-To Party");
			public static MultilingualString ControlCustomerFallToPrepaid = ResString.GetMultilingualString("6C531FCD-ECEB-4DA0-BA2E-DC08659F8487", "Job's Controlling Customer falling back to Prepaid Bill-To Party");
			public static MultilingualString ControlCustomerFallToCollect = ResString.GetMultilingualString("A024D514-91B5-486B-B144-B301EB08FE41", "Job's Controlling Customer falling back to Collect Bill-To Party");
		}
	}
}
