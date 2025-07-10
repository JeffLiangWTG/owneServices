using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.ClientSharedComponents
{
	public abstract class FixedWidthFlatFileDataRow : SharedFlatFileDataRow
	{
		public FixedWidthFlatFileDataRow() : base() { }
		public FixedWidthFlatFileDataRow(ZString lineData) : base(lineData) { }

		public FixedWidthFlatFileDataRow(int fieldCount) : base(fieldCount) { }

		public FixedWidthFlatFileDataRow(int fieldCount, ZString lineData)
			: base(fieldCount)
		{
			SetFieldProperties(lineData);
		}

		public override string ToString()
		{
			if (InternalPropertyFieldList.Count == 0)
			{
				var builder = new FixedWidthLineBuilder(WhiteSpace, Length);
				int position = 0;
				for (int i = 0; i < FieldCount; i++)
				{
					builder.AddDataToLine(DataRow[i] ?? "",
						position,
						FieldProperties[i].Length,
						PadRight(FieldProperties[i]));
					position += FieldProperties[i].Length;
				}
				return builder.ToString();
			}
			else
			{
				PopulateFields();
				// Please note: Set alignment and padding character at the FieldAttrib level: eg: [FieldAttribute(10, 35, AlignTypes.Right, ' ')]
				var builder = new FixedWidthLineBuilder(this.WhiteSpace, this.Length);
				int position = 0;
				InternalPropertyFieldList.ForEach(f =>
				{
					builder.AddDataToLine(DataRow[f.Attribute.Position] ?? String.Empty, position, f.Attribute.Length);
					position += f.Attribute.Length;
				});
				return builder.ToString();
			}
		}

		protected virtual bool PadRight(FlatFileFieldProperty flatFileProperty)
		{
			return false;
		}

		void SetFieldProperties(ZString lineData)
		{
			int position = 0;
			for (int idx = 0; idx < this.FieldCount; idx++)
			{
				DataRow[idx] = lineData.SubstringSafe(position, FieldProperties[idx].Length).Trim();
				position += this.FieldProperties[idx].Length;
			}
		}

		public override bool IsFixedWidth
		{
			get { return true; }
		}

		public override string DataDateFormat
		{
			get { return dataDateFormat; }
		}
		const string dataDateFormat = "ddMMyyyy";
	}
}
