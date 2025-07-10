using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.NZP.CMS.Testing
{
	public class CMSFlatFileDataRowTest : TestCase
	{
		public void TestProperties()
		{
			CMSFlatFileDataRow row = CreateRow();
			AssertEquals("Record Type", "R", row.RecordType);
			AssertEquals("Account Code", "   Account", row.AccountCode);
			AssertEquals("Warehouse Code", "Ware", row.WarehouseCode);
			AssertEquals("Transaction Code", "Tran", row.TransactionCode);
			AssertEquals("Product Code", "ProductCode", row.ProductCode);
			AssertEquals("Transaction Quantity", Constants.Quantity, row.TransactionQuantity);
			AssertEquals("File Generated Date", "20-Oct-2005", row.FileGeneratedDate.ToString(Constants.DateFormat));
			AssertEquals("Delivery Code", "Del", row.DeliveryCode);
			AssertEquals("Exempt Number", "ExemptNu", row.ExemptNumber);
			AssertEquals("Tax Total", 974543.5438m, row.TaxTotal);
			AssertEquals("Line Total", 43974.5365m, row.LineTotal);
			AssertEquals("Pricing Indicator", "P", row.PricingIndicator);
			AssertEquals("Final Price", 84732.2300m, row.FinalPrice);
			AssertEquals("Comment", "Comment", row.Comment);
			AssertEquals("Contract Number", "CONT", row.CONT);
			AssertEquals("Reject Code", "Reje", row.RejectCode);
			AssertEquals("Batch Number", "BatchNum", row.BatchNumber);
			AssertEquals("ExternalID", "ExternalID", row.ExternalID);
			AssertEquals("Intermediary", "Intermed", row.Intermediary);
			AssertEquals("Reference", "Referenc", row.Reference);
			AssertEquals("Line Item", "Li", row.LineItem);
			AssertEquals("Batch Date", "28-Oct-2005", row.BatchDate.ToString(Constants.DateFormat));
			AssertEquals("Transaction Date", "01-Nov-2005", row.TransactionDate.ToString(Constants.DateFormat));
			AssertEquals("Item", "Item", row.Item);
			AssertEquals("Weight", 234.230m, row.Weight);
			AssertEquals("Volume", 78983.83m, row.Volume);
			AssertEquals("UserReference1", "UserRefere", row.UserReference1);
			AssertEquals("UserReference2", "UserReference2", row.UserReference2);
			AssertEquals("Activity1", "Activity1", row.Activity1);
			AssertEquals("Activity2", "Activity2", row.Activity2);
			AssertEquals("Reference Number", "ReferenceNum", row.ReferenceNumber);
			AssertEquals("Extension Reference", "ExtensionRef", row.ExtensionReference);
			AssertEquals("Normal Price", "NormalPrice", row.NormalPrice);
			AssertEquals("Code", "Code", row.Code);
			AssertEquals("Value", "Value", row.Value);
			AssertEquals("Version Number", "VersionNum", row.VersionNumber);
		}

#region CreateRow
		CMSFlatFileDataRow CreateRow()
		{
			CMSFlatFileDataRow row = new CMSFlatFileDataRow();
			AssertEquals("Number of fields should be " + Constants.NumberOfFields, Constants.NumberOfFields, row.FieldCount);
			row.RecordType = "RecordType";
			row.AccountCode = "Account";
			row.WarehouseCode = "WarehouseCode";
			row.ProductCode = "ProductCode";
			row.TransactionCode = "TransactionCode";
			row.TransactionQuantity = Constants.Quantity;
			row.FileGeneratedDate = new ZDateTime(2005, 10, 20, 15, 14, 23);
			row.DeliveryCode = "DeliveryCode";
			row.ExemptNumber = "ExemptNumber";
			row.TaxTotal = 974543.54378m;
			row.LineTotal = 43974.536469m;
			row.PricingIndicator = "PricingIndicator";
			row.FinalPrice = 84732.23m;
			row.Comment = "Comment";
			row.CONT = "CONT";
			row.RejectCode = "RejectCode";
			row.BatchNumber = "BatchNumber";
			row.ExternalID = "ExternalID";
			row.Intermediary = "Intermediary";
			row.Reference = "Reference";
			row.LineItem = "LineItem";
			row.BatchDate = new ZDateTime(2005, 10, 28, 11, 50, 21);
			row.TransactionDate = new ZDateTime(2005, 11, 1, 21, 23, 56);
			row.Item = "Item";
			row.Weight = 234.23m;
			row.Volume = 78983.825m;
			row.UserReference1 = "UserReference1";
			row.UserReference2 = "UserReference2";
			row.Activity1 = "Activity1";
			row.Activity2 = "Activity2";
			row.ReferenceNumber = "ReferenceNumber";
			row.ExtensionReference = "ExtensionReference";
			row.NormalPrice = "NormalPrice";
			row.Code = "Code";
			row.Value = "Value";
			row.VersionNumber = "VersionNumber";
			return row;
		}
#endregion
	}
}
