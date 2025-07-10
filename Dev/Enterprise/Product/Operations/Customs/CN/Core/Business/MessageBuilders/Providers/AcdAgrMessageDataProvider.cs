using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CN.MessageContracts;

namespace Enterprise.Customs.CN.Business
{
	public class AcdAgrMessageDataProvider : IAcdAgrRequest
	{
		public readonly CusEntryHeader EntryHeader;

		public AcdAgrMessageDataProvider(CusEntryHeader entryHeader)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		public IAcdAgrRequestOperInfo OperationInfo => operationInfo ?? (operationInfo = new AcdAgrRequestOperInfoProvider(EntryHeader));
		IAcdAgrRequestOperInfo operationInfo;

		public IAcdAgrRequestImportInfo ImportInfo => importInfo ?? (importInfo = new AcdAgrRequestImportInfoProvider(EntryHeader));
		IAcdAgrRequestImportInfo importInfo;

		public string[] MissingMandatoryFields => missingMandatoryFields ?? (missingMandatoryFields = GetMissingMandatoryFields().ToArray());
		string[] missingMandatoryFields;

		IEnumerable<string> GetMissingMandatoryFields()
		{
			var missingFields = new List<string>();
			var importInfo = ImportInfo;

			void checkMandatoryStringFields(string fieldValue, string fieldMessage)
			{
				if (fieldValue.IsNullOrEmpty())
				{
					missingFields.Add(fieldMessage);
				}
			}

			void checkMandatoryDecimalFields(decimal fieldValue, string fieldMessage)
			{
				if (fieldValue == 0)
				{
					missingFields.Add($"{fieldMessage}");
				}
			}

			if (EntryHeader.IsImport)
			{
				checkMandatoryStringFields(importInfo.TraderCustomsCode, Res.GetString("E004860F-E94B-45A5-BA75-654F6F7A7E30", "CCD Registration Number of Import (on Declaration tab)"));
			}
			else if (EntryHeader.IsExport)
			{
				checkMandatoryStringFields(importInfo.TraderCustomsCode, Res.GetString("E004860F-E94B-45A5-BA75-654F6F7A7E31", "CCD Registration Number of Supplier (on Declaration tab)"));
			}

			checkMandatoryStringFields(importInfo.DeclarantCustomsCode, Res.GetString("E004860F-E94B-45A5-BA75-654F6F7A7E32", "CCD Registration Number of Branch (on Misc tab)"));
			checkMandatoryStringFields(importInfo.TradeMode, Res.GetString("E004860F-E94B-45A5-BA75-654F6F7A7E33", "Customs Procedure (on Entry Instruction tab)"));
			checkMandatoryStringFields(importInfo.TariffCode, Res.GetString("E004860F-E94B-45A5-BA75-654F6F7A7E34", "Tariff (on Inv. Lines tab)"));
			checkMandatoryDecimalFields(importInfo.TotalPriceOfGoods, Res.GetString("E004860F-E94B-45A5-BA75-654F6F7A7E35", "Trade Quantity (on Inv. Lines tab)"));
			checkMandatoryStringFields(importInfo.NameOfMainGoods, Res.GetString("E004860F-E94B-45A5-BA75-654F6F7A7E36", "Name Of Goods (on Inv. Lines tab)"));
			checkMandatoryStringFields(importInfo.GoodsOrigin, Res.GetString("E004860F-E94B-45A5-BA75-654F6F7A7E37", "Goods Origin (on Inv. Lines tab)"));

			return missingFields;
		}
	}
}
