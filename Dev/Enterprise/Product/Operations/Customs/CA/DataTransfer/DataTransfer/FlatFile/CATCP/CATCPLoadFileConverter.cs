using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Customs.CA.DataTransfer
{
	public class CATCPLoadFileConverter : FlatFileConverter
	{
		public CATCPLoadFileConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		public override void ImportFlatFile(IValueObject valueObject, IFlatFileFormat flatFileFormat, TextReader reader)
		{
			throw new NotSupportedException("This converter doesn't support file import");
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			var fileLines = new FlatFileDataRowCollection();
			var header = valueObject as CATCPHeaderValueObject;
			if (header != null)
			{
				var headerRow = new CATCPHeaderRow();
				headerRow[CATCPHeaderRow.Schema.RecordIdentifier] = header.RecordIdentifier;
				headerRow[CATCPHeaderRow.Schema.BusinessNumber] = header.BusinessNumber;
				fileLines.Add(headerRow);

				foreach (CATCPLine line in header.Lines)
				{
					var lineRow = new CATCPLineRow();
					lineRow[CATCPLineRow.Schema.RecordIdentifier] = line.RecordIdentifier;
					lineRow[CATCPLineRow.Schema.BusinessNumber] = line.BusinessNumber;
					lineRow[CATCPLineRow.Schema.TCPTypeCode] = line.TCPTypeCode;
					lineRow[CATCPLineRow.Schema.TCPIdentifier] = line.TCPIdentifier;
					lineRow[CATCPLineRow.Schema.AddressLine1] = line.AddressLine1;
					lineRow[CATCPLineRow.Schema.AddressLine2] = line.AddressLine2;
					lineRow[CATCPLineRow.Schema.City] = line.City;
					lineRow[CATCPLineRow.Schema.ProvinceStateCode] = line.ProvinceStateCode;
					lineRow[CATCPLineRow.Schema.CountryCode] = line.CountryCode;
					lineRow[CATCPLineRow.Schema.PostalZipCode] = line.PostalZipCode;
					lineRow[CATCPLineRow.Schema.BusinessName] = line.BusinessName;
					fileLines.Add(lineRow);
				}

				var trailerRow = new CATCPTrailerRow();
				trailerRow[CATCPTrailerRow.Schema.RecordIdentifier] = CATCPLoadFileDataAdapter.RecordIdentifier.Trailer;
				trailerRow.SetField(CATCPTrailerRow.Schema.NumberOfRecords, header.Lines.Count + 2);
				fileLines.Add(trailerRow);
			}
			return fileLines;
		}
	}
}
