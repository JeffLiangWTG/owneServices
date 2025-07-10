namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public static class KoreaSouthEInvoicingPenaltyTaxInfoConstants
	{
		public static class PenaltyType
		{
			public static string NotIssued => Res.GetString("641A2455-3D03-43A0-88BD-82E9CE2501EE", "Not Issued");
			public static string DelayedIssued => Res.GetString("A8D8796E-9BF9-4553-AC47-64652FDA8E68", "Delayed Issued");
			public static string IssuedByPaper => Res.GetString("72ABE1AC-C0E9-4F3B-AE32-CF3F97E4C1CD", "Issued by paper");
			public static string NotTransmitted => Res.GetString("0272B536-F422-48BE-ABAC-E05B72AF98BC", "Not Transmitted");
			public static string DelayedTransmitted => Res.GetString("FBBDE76B-88C8-4A07-A724-AE78DB05A180", "Delayed Transmitted");
		}

		public static class PenaltyTypeExplanation
		{
			public static string NotIssued => Res.GetString("B83E70BF-9119-485E-8DA6-CCA89A78A601", "Where a tax invoice is not issued by the deadline for filing a final return for the taxable period during which the relevant goods or services are supplied after the elapse of the time limit for issuing tax invoice.");

			public static string DelayedIssued => Res.GetString("FB2833BC-EC1D-446E-9D92-8419ED47FC75", "Where a tax invoice is issued by the deadline for filing a final return for the taxable period during which the relevant goods or services are supplied after the elapse of the time limit for issuing tax invoices.");

			public static string IssuedByPaper => Res.GetString("04541658-B540-44FB-B003-14167418B622", "Where a tax invoice is issued other than the required electronic tax invoice, during the time limit for issuing tax invoices.");

			public static string NotTransmitted => Res.GetString("6CAA788B-423D-4A07-83B1-54AF55768958", "Where the electronic tax invoice was not transmitted to NTS within the eleventh day of the month following the month in which the date of supply of goods or services falls.");

			public static string DelayedTransmitted => Res.GetString("7364FBD3-74EF-4E0B-A594-0269FEDE30A4", "Where an electronic tax invoice is transmitted to the NTS after the eleventh day of the month following the month in which the date of supply of goods or services falls.");
		}

		public static class PenaltyTax
		{
			public static string HalfOfOnePercent => Res.GetString("6858C40E-63AC-445F-AC9E-21D8C59D3687", "0.50%");
			public static string OnePercent => Res.GetString("9FAD8C08-9925-4947-BF8E-E6E5C9A07D39", "1%");
			public static string TwoPercent => Res.GetString("08415C8E-2693-409D-AFBA-D2AB268C4AF2", "2%");
			public static string NonDeductible => Res.GetString("11D3918D-407D-4879-98FE-712DB231CEBE", "Non-deductible input tax");
			public static string NotApplicable => Res.GetString("2D3EC619-9D80-4027-B32D-77F9E0B88D3F", "Not applicable");
		}
	}
}
