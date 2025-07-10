using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.ZClientCCP.Kawasaki
{
	public class KawasakiInvoiceDataFileReader : CsvInvoiceDataFileReader
	{
		public KawasakiInvoiceDataFileReader(string fileName) : base(fileName)
		{
		}

		internal List<string> InternalFileLines
		{
			get { return FileLines.Cast<string>().ToList(); }
		}

		#region Overrides

		protected override void AddRecordsForLine(ArrayList recordsArrayOfStringArrays, OCsvLine csvLine)
		{
			recordsArrayOfStringArrays.Add(CreateInvoiceLine(csvLine));
		}

		#endregion

		#region Line Conversion

		protected string[] CreateInvoiceLine(OCsvLine invoiceLine)
		{
			string[] lineRow = new string[FlatFileInvoiceDataImporter.Constants.InvoiceLineFields.RecordLength];

			lineRow[FlatFileInvoiceDataImporter.Constants.InvoiceLineFields.Type] = FlatFileInvoiceDataImporter.Constants.InvoiceRecordType.Line;
			lineRow[FlatFileInvoiceDataImporter.Constants.InvoiceLineFields.InvoiceNo] = invoiceLine.FieldValues[KawasakiInvoiceDataFileReader.Constants.InvoiceNumber];
			lineRow[FlatFileInvoiceDataImporter.Constants.InvoiceLineFields.ProductCode] = invoiceLine.FieldValues[KawasakiInvoiceDataFileReader.Constants.PartNumber];
			lineRow[FlatFileInvoiceDataImporter.Constants.InvoiceLineFields.LinePrice] = LinePrice(invoiceLine.FieldValues[KawasakiInvoiceDataFileReader.Constants.Price], invoiceLine.FieldValues[KawasakiInvoiceDataFileReader.Constants.Quantity]);
			lineRow[FlatFileInvoiceDataImporter.Constants.InvoiceLineFields.Quantity] = invoiceLine.FieldValues[KawasakiInvoiceDataFileReader.Constants.Quantity];
			lineRow[FlatFileInvoiceDataImporter.Constants.InvoiceLineFields.ProductDescription] = invoiceLine.FieldValues[KawasakiInvoiceDataFileReader.Constants.Description];

			ZString kawasakiOrigin = invoiceLine.FieldValues[KawasakiInvoiceDataFileReader.Constants.Origin];

			ZString ediEnterpriseOrigin = LookupSupplierPatternMatch("KAWMOTSYD", kawasakiOrigin);
			lineRow[FlatFileInvoiceDataImporter.Constants.InvoiceLineFields.Origin] = ediEnterpriseOrigin;

			lineRow[FlatFileInvoiceDataImporter.Constants.InvoiceLineFields.UnitofQty] = "NO";

			return lineRow;
		}

		protected string LinePrice(string price, string quantity)
		{
			ZString result = "";

			ZDecimal priceInt = 0;
			ZDecimal quantityInt = 0;

			ZDecimal.TryParse(price.Trim(), out priceInt);
			ZDecimal.TryParse(quantity.Trim(), out quantityInt);

			return ((ZDecimal)(priceInt * quantityInt)).ToString();
		}

		protected string LookupSupplierPatternMatch(string orgCode, string kawasakiCountryCode)
		{
			string result = String.Empty;
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader orgForLookup = OrgHeader.LoadFromCode(factory, orgCode);

			if (orgForLookup != null)
			{
				ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, SQLComparisonOperator.Equal, orgForLookup.PK);
				filter.AddToFilter(JoinCondition.And, OrgPatternMatchOverrideSchema.OO_ForeignCode, SQLComparisonOperator.Equal, kawasakiCountryCode);
				OrgPatternMatchOverride countryMatch = (OrgPatternMatchOverride)factory.LoadTop1(typeof(OrgPatternMatchOverride), filter);

				if (countryMatch != null)
				{
					RefCountry country = (RefCountry)factory.Load(typeof(RefCountry), countryMatch.OO_LocalGuid);
					if (country != null)
					{
						result = country.RN_Code;
					}
				}
			}
			return result;
		}

		#endregion

		#region Constants

		public static class Constants
		{
			public const int RecordLength = 11;

			public const int InvoiceNumber = 0;
			public const int PartNumber = 5;
			public const int Price = 6;
			public const int Quantity = 7;
			public const int Description = 8;
			public const int Origin = 9;
		}

		#endregion
	}
}

#region TestCase
#region TestRecords
#endregion
#region TestAddRecordsForLine
#endregion
#region TestCreateInvoiceLine
#endregion
#region TestLinePrice
#endregion
#endregion
#region Setup
#endregion
