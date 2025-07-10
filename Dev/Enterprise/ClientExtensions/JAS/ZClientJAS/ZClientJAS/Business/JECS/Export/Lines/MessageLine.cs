using System.Text;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public abstract class MessageLine
	{
		public ZString LineIdentifier
		{
			get { return LineType + JXCConstants.Version; }
		}

		public ZString LineAsString
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(LineIdentifier);
				if (FieldCount > 0)
				{
					builder.Append(JXCConstants.Delimiter);
					builder.Append(LineContent);
				}
				return builder.ToString();
			}
		}

		ZString LineContent
		{
			get
			{
				DelimitedFlatFileFormat flatFileFormat = new JXCFlatFileFormat();
				ZString line = flatFileFormat.ConvertToLine(DataRow);
				ZString lineWithoutNewLine = line.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
				return lineWithoutNewLine;
			}
		}

		JXCFlatFileDataRow DataRow
		{
			get
			{
				JXCFlatFileDataRow dataRow = new JXCFlatFileDataRow(FieldCount);
				SetFieldValues(dataRow);
				return dataRow;
			}
		}

		#region Abstract

		protected abstract ZString LineType { get; }

		protected abstract int FieldCount { get; }

		protected abstract void SetFieldValues(JXCFlatFileDataRow dataRow);

		#endregion
	}
}
