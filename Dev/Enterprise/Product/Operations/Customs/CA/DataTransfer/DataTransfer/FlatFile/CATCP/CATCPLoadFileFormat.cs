using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DataTransfer
{
	public class CATCPLoadFileFormat : FixedWidthFlatFileFormat
	{
		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			return row is CATCPRow catcpRow ? catcpRow.ToString() : string.Empty;
		}

		public override FileExtensionType FileExtensionForExport => FileExtensionType.Txt;

		public override FileExtensionType FileExtensionForImport => throw GetNotSupportedException();
		public override FlatFileDataRow ConvertToRow(ZString rawRow) => throw GetNotSupportedException();
		public NotSupportedException GetNotSupportedException()
		{
			throw new NotSupportedException(Res.GetString("9C3D1460-B556-4963-B2AF-BAFA8F1A91C0", "Import is not supported"));
		}
	}
}
