
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class BaseDataRow : FlatFileDataRow
	{
		public BaseDataRow(ZString rawRow, int fieldCount)
			: base(fieldCount)
		{
			this.rawRow = rawRow;
			this.initialRawRow = rawRow;
			ParseRawRow();
		}

		public uint GetFieldAsUInt(int position)
		{
			uint result = 0;
			uint.TryParse(this[position], out result);
			return result;
		}

		protected virtual ZString GetValue(int position, int length)
		{
			return rawRow.SubstringSafe(position, length);
		}

		void ParseRawRow()
		{
			SetField(ShipnetConstants.Common.RecordID, GetValue(ShipnetConstants.Common.RecordIDPosition, ShipnetConstants.Common.RecordIDMaxLength));
			ParseRawRowCore();
			rawRow = ZString.Empty;
		}

		protected override ZString GetFieldCore(int position)
		{
			return base.GetFieldCore(position).Trim();
		}

		protected abstract void ParseRawRowCore();

		ZString rawRow;

		readonly ZString initialRawRow;

		public ZString InitialRawRow
		{
			get { return initialRawRow; }
		}
	}
}
