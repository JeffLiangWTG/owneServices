namespace Enterprise.Customs.KR.Messaging
{
	partial class StatementHeaderTypeList
	{
		public static string GetFormName(string code)
		{
			switch (code)
			{
				case Codes.CustomsDisbursementBill:
					return Res.GetString("40947409-6F0E-4DBC-B4B2-510DB2720FFB", "Customs Individual Disbursement Bills");
				case Codes.Invoice:
					return Res.GetString("5CD647B6-5221-47B0-A4EF-6826447E42FB", "Monthly Bill of Customs Disbursement Charges");
				case Codes.MonthlyReceipt:
					return Res.GetString("1AC6B5F5-B41C-4A28-8CE0-186E5F430300", "Monthly Invoice of VAT");
				case Codes.IndividualCollectionReceipt:
					return Res.GetString("366BA331-DDE7-45D8-A011-919CCB5702D5", "Monthly Invoice of VAT (Collection of Individual Cases)");
				case Codes.Normal:
					return Res.GetString("F539610C-D3DD-4D2D-8E6C-AC41ACA5A6FD", "Bill of Misc Customs Disbursement Charges");
				case Codes.NormalReport:
					return Res.GetString("A3E9F4A5-87E6-442B-85B6-990731B54991", "Invoice of Misc Customs Disbursement Charges");
				default:
					return string.Empty;
			}
		}

		public static string[] EntryDisbursementBillTypes => new string[] { Codes.CustomsDisbursementBill, Codes.Invoice };
	}
}
