using System.Collections;
using System.Collections.Specialized;
using System.IO;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer;
using InvoiceHeaderConstants = Enterprise.Customs.DataTransfer.FlatFileInvoiceDataImporter.Constants.InvoiceHeaderFields;
using InvoiceLineConstants = Enterprise.Customs.DataTransfer.FlatFileInvoiceDataImporter.Constants.InvoiceLineFields;
using RecordType = Enterprise.Customs.DataTransfer.FlatFileInvoiceDataImporter.Constants.InvoiceRecordType;

namespace Enterprise.Client.ZClientPOW.Suzuki
{
	public class SuzukiInvoiceDataFileReader : FileDataReader
	{
		public SuzukiInvoiceDataFileReader(string uri) : base(uri)
		{
			this.Uri = uri;
		}

		public override string[][] Records
		{
			get
			{
				if (fRecords == null)
				{
					ArrayList recordsArray = new ArrayList();
					foreach (string lineFromFile in FileLines)
					{
						ProcessRecord(lineFromFile, recordsArray);
					}

					fRecords = new string[recordsArray.Count][];
					for (int i = 0; i < recordsArray.Count; i++)
					{
						fRecords[i] = (string[])recordsArray[i];
					}
				}
				return fRecords;
			}
		}
		string[][] fRecords;

		#region Processing

		protected void ProcessRecord(string lineFromFile, ArrayList recordsArray)
		{
			if (recordsArray.Count == 0)
			{
				string[] header = new string[InvoiceHeaderConstants.RecordLength];
				header[InvoiceHeaderConstants.Type] = RecordType.Head;
				header[InvoiceHeaderConstants.InvoiceCurrency] = Enterprise.Core.Constants.CurrencyCodes.Australia;
				recordsArray.Add(header);
			}
			recordsArray.Add(ConvertDataLineToEDICsvLine(ExtractInvoiceLineFromDataLine(lineFromFile)));
		}

		#endregion

		#region DataTransformation

		protected string[] ExtractInvoiceLineFromDataLine(ZString dataLine)
		{
			string[] result = new string[3];

			if (dataLine.Length >= 118)
			{
				result[SuzukiConstants.PartNumber] = dataLine.SubstringSafe(26, 11).Trim();
				result[SuzukiConstants.Quantity] = dataLine.SubstringSafe(79, 7).Trim();
				result[SuzukiConstants.TotalPrice] = dataLine.SubstringSafe(86, 33).Trim();
			}

			return result;
		}

		protected string[] ConvertDataLineToEDICsvLine(string[] dataLine)
		{
			string[] result = new string[InvoiceLineConstants.RecordLength];

			result[InvoiceLineConstants.Type] = RecordType.Line;
			result[InvoiceLineConstants.ProductCode] = dataLine[SuzukiConstants.PartNumber];
			result[InvoiceLineConstants.Quantity] = dataLine[SuzukiConstants.Quantity];
			result[InvoiceLineConstants.LinePrice] = dataLine[SuzukiConstants.TotalPrice];

			return result;
		}

		protected static class SuzukiConstants
		{
			public const int PartNumber = 0;
			public const int Quantity = 1;
			public const int TotalPrice = 2;
		}

		#endregion

		#region Implementation

		protected string Uri;

		protected StringCollection FileLines
		{
			get
			{
				if (fFileLines == null)
				{
					fFileLines = new StringCollection();
					using (StreamReader reader = new StreamReader(Uri))
					{
						string line;
						while ((line = reader.ReadLine()) != null)
						{
							fFileLines.Add(line);
						}
					}
				}
				return fFileLines;
			}
		}
		StringCollection fFileLines;

		#endregion
	}
}

#region Data Conversion and Processing
#endregion
#region Setup
#endregion
