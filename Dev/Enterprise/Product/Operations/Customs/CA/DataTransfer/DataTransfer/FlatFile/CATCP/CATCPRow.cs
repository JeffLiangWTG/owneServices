using System.Linq;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.CA.DataTransfer
{
	public abstract class CATCPRow : FlatFileDataRow
	{
		protected CATCPRow(int[] fieldsLengthSchema)
			: base(fieldsLengthSchema.Length)
		{
			this.fieldsLengthSchema = fieldsLengthSchema;
			this.fillerLength = DataMaxLength - fieldsLengthSchema.Sum(x => x);
		}
		readonly int[] fieldsLengthSchema;
		readonly int fillerLength;
		internal const int DataMaxLength = 450;

		public override string ToString()
		{
			var result = new ZStringBuilder();
			var index = 0;
			foreach (var fieldLength in fieldsLengthSchema)
			{
				result.Append(DataRow[index++].PadRight(fieldLength));
			}
			result.Append("".PadRight(fillerLength));
			return result.ToString();
		}

		protected override void SetFieldCore(int position, ZString value)
		{
			base.SetFieldCore(position, value.Left(GetFieldLength(position)));
		}

		internal int GetFieldLength(int position) => fieldsLengthSchema[position];
	}
}
