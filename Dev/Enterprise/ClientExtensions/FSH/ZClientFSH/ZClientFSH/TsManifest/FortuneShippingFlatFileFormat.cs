using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.FSH.TsManifest
{
	public class FortuneShippingFlatFileFormat : FlatFileFormat
	{
		public FortuneShippingFlatFileFormat()
		{
		}

		#region Import

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			ZString[] splitRow = rawRow.SplitIgnoringEscapedDelimiter(':', '?');
			splitRow = StripFromZStringArray(splitRow, "?");

			ZInt fieldCount = 0;
			ZInt.TryParse(RecordFields.GetDescriptionFromCode(splitRow[0]), out fieldCount);
			return new FortuneShippingDataRow(splitRow, fieldCount);
		}

		ZString[] StripFromZStringArray(ZString[] strings, ZString stringToStrip)
		{
			ZString[] result = new ZString[strings.Length];

			for (int i = 0; i < strings.Length; i++)
			{
				result[i] = strings[i].Replace(stringToStrip, "");
			}

			return result;
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.Txt; }
		}

		#endregion

		#region Export

		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			throw new NotImplementedException("This format does not support exporting.");
		}

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.Txt; }
		}

		#endregion

		#region Implementation

		FSHRecordFields RecordFields
		{
			get
			{
				if (fRecordFields == null)
				{
					fRecordFields = new FSHRecordFields();
				}
				return fRecordFields;
			}
		}
		FSHRecordFields fRecordFields;

#endregion

					}
}
