
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.FSH.TsManifest
{
	public class FortuneShippingDataRow : FlatFileDataRow
	{
		public FortuneShippingDataRow(ZString[] fields, int fieldCount) : base(fields.Length > fieldCount ? fields.Length : fieldCount)
		{
			for (int i = 0; i < fields.Length; i++)
			{
				SetField(i, fields[i]);
			}
		}

		public ZInt RowType
		{
			get { return base.GetFieldAsZInt(0); }
		}
	}
}
