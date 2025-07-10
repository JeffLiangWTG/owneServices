

using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.Wow
{
	public static class WowConstants
	{
		public const string EdiTrackMessageTypeName = "MANAGING IMPORTS";
		public const string EdiTrackAllOrdersMessage = "Supermarkets";
		public const string DataImportNotificationGroupCode = "ONG";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public const string ErrorNotificationEmailSubject = "CargoWise One Order Notifications";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public const string ProductBuyerTakeOnErrorNotificationEmailSubject = "CargoWise One Order Notifications for Product Buyer Take On";
		public const string StandardServiceLevelCode = "STD";
		public const string OrderIncoTermNoteDescription = "Incoterms";

		public const string EdiTrackEmailDateFormat = "yyyyMMddhhmmss";

		public static class OrderNumberFilterTypes
		{
			public const string BuyerName = "Buyer Name";
		}

		public static class OrderStatuses
		{
			public const string ContainersAttached = "CNT";
		}

		public static class PaymentTypes
		{
			public const string TelegraphicTransfer = "T/T";
			public const string LetterCredit = "L/C";
			public const string DocPrepayment = "D/P";
		}

		public static CodeDescriptionPairList GetPaymentTypeList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			result.AddPair(PaymentTypes.TelegraphicTransfer, "Telegraphic Transfer");
			result.AddPair(PaymentTypes.LetterCredit, "Letter Of Credit");
			result.AddPair(PaymentTypes.DocPrepayment, "Document Prepayment");
			return result;
		}

		#region CustomsEntryStatusCaseBodyStatement

		public static string CustomsEntryStatusCaseBodyStatement
		{
			get
			{
				string result = System.Environment.NewLine;

				CodeDescriptionPairList[] lists = new CodeDescriptionPairList[]
					{
						new CMRImportEntryAdviceList(),
						new CMRImportMessageStatusList(),
						new LegacyCustomsEntryStatusList(),
						new EdificeCustomsEntryStatusList(),
						new ExportCustomsEntryStatusList()
					};

				foreach (CodeDescriptionPairList list in lists)
				{
					foreach (CodeDescriptionPair status in list)
					{
						if (result.IndexOf("'" + status.Code + "'") < 0)
						{
							result += "WHEN '" + CargoWise.Data.DataUtils.EscapeSingleQuotes(status.Code) + "' THEN '" + CargoWise.Data.DataUtils.EscapeSingleQuotes(status.Description) + "'" + System.Environment.NewLine;
						}
					}
				}

				result += "ELSE 'Unknown'" + System.Environment.NewLine;

				return result;
			}
		}

		#endregion

	}
}
