using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Enterprise.Environment;

namespace Enterprise.FaxRouter.Processor
{
	public class FaxManager : FaxDataModule
	{
		public static DataTable OrderedDataSet(ArrayList aFaxManagementJobList)
		{
			DataTable result = new DataTable("FaxManagementJob");
			result.Columns.Add("ChargeCode");
			result.Columns.Add("FaxJobId");
			result.Columns.Add("ReceivedDateTime");
			result.Columns.Add("PageCount");
			result.Columns.Add("FaxRecipientId");
			result.Columns.Add("FaxSentDateTime");
			result.Columns.Add("FaxAttention");
			result.Columns.Add("FaxNumber");
			result.Columns.Add("CompanyName");
			result.Columns.Add("AckDateTime");
			result.Columns.Add("AckSuccess");

			foreach (MailDBItemDataLine aRecord in aFaxManagementJobList)
			{
				DataRow newRow = result.NewRow();
				newRow["ChargeCode"] = aRecord.ChargeCode;
				newRow["FaxJobId"] = aRecord.PrimaryKey;
				newRow["ReceivedDateTime"] = aRecord.ReceivedDateTime.ToString(LONGDATEFORMAT);
				newRow["PageCount"] = aRecord.PageCount;
				newRow["FaxRecipientId"] = aRecord.FaxRecipientId;
				newRow["FaxAttention"] = aRecord.FaxRecipientAttentionName;
				newRow["FaxNumber"] = aRecord.FaxRecipientNumber;
				newRow["CompanyName"] = aRecord.FaxRecipientCompany;

				if (aRecord.FaxRecipientSentDateTime != new DateTime(1900, 1, 1))
				{
					newRow["FaxSentDateTime"] = aRecord.ReceivedDateTime.ToString(LONGDATEFORMAT);
				}
				else
				{
					newRow["FaxSentDateTime"] = "N/A";
				}

				if (aRecord.FaxRecipientAckDateTime != new DateTime(1900, 1, 1))
				{
					newRow["AckDateTime"] = aRecord.FaxRecipientAckDateTime.ToString(LONGDATEFORMAT);
					newRow["AckSuccess"] = aRecord.FaxRecipientAckSuccess.Equals("1").ToString();
				}
				else
				{
					newRow["AckDateTime"] = "N/A";
					newRow["AckSuccess"] = "N/A";
				}
				result.Rows.Add(newRow);
			}
			return result;
		}

		static string GetUniqueFilename()
		{
			var result = "";
			var aFileType = ".tif";

			while (string.IsNullOrEmpty(result) || File.Exists(result))
			{
				var randomNumber = new Random().Next(99999999);
				result = Env.TempPath + randomNumber.ToString() + aFileType;
			}
			return result;
		}

		public static string ExtractPostedFaxFilePath(Guid faxJobId)
		{
			var filePath = GetUniqueFilename();

			var tiffFaxData = FaxDataModule.GetTiffFile(faxJobId);
			var fs = File.Create(filePath, 1024);
			fs.Write(tiffFaxData, 0, tiffFaxData.Length);
			fs.Close();

			return filePath;
		}

		static DateTime RevertCustomLongDateTimeToSystemDateTime(string aCustomLongDateTime)
		{
			DateTimeFormatInfo aDateTimeFormatter = new DateTimeFormatInfo();
			aDateTimeFormatter.LongDatePattern = LONGDATEFORMAT;
			return DateTime.ParseExact(aCustomLongDateTime, LONGDATEFORMAT, aDateTimeFormatter);
		}

		public static string GetLastReceivedDateTimeOnGrid(DataGrid aDataGrid, int aIndexColumn)
		{
			DataTable aDataTable = (DataTable)aDataGrid.DataSource;
			return RevertCustomLongDateTimeToSystemDateTime(aDataGrid[aDataTable.Rows.Count - 1, aIndexColumn].ToString()).ToString(SQLDATEFORMAT);
		}

		public static string GetFirstReceivedDateTimeOnGrid(DataGrid aDataGrid, int aIndexColumn)
		{
			return RevertCustomLongDateTimeToSystemDateTime(aDataGrid[0, aIndexColumn].ToString()).ToString(SQLDATEFORMAT);
		}
	}
}
