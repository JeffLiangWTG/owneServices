using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class ClearanceInfo : NonPersistentBusinessObject
	{
		ClearanceInfo(CusEntryNumber entryNumber)
			: base(entryNumber.Factory)
		{
			this.entryNumber = entryNumber;
			Argument.NotNull(entryNumber, nameof(entryNumber));
		}
		public readonly CusEntryNumber entryNumber;

		public static ClearanceInfo LoadNew(CusEntryNumber cusNCTSHeader)
		{
			Argument.NotNull(cusNCTSHeader, nameof(cusNCTSHeader));
			var updateClearanceInfo = new ClearanceInfo(cusNCTSHeader);
			updateClearanceInfo.ClearanceNumber = cusNCTSHeader.CE_EntryNum;
			updateClearanceInfo.ClearanceDate = cusNCTSHeader.CE_IssueDate;
			updateClearanceInfo.ArrivalLimitDate = cusNCTSHeader.CE_ExpiryDate;
			return updateClearanceInfo;
		}

		public ZString ClearanceNumber { get; set; }
		public ZDateTime ClearanceDate { get; set; }
		public ZDateTime ArrivalLimitDate { get; set; }

		public static bool CSVCodeIsValidOrEmpty(ZString csvCode) => Regex.IsMatch(csvCode, @"^[a-zA-Z0-9]+$") || csvCode.IsEmpty;
	}
}
