
using CargoWise.Types;

namespace Enterprise.Client.OSP.Data_Import
{
	public class IFTMIN5MBaseDataRow : OSPDataRow
	{
		public IFTMIN5MBaseDataRow(ZString dataRow)
		{
			this.DataRow = dataRow;
		}

		public IFTMIN5MBaseDataRow(IFTMIN5MBaseDataRow dataRow)
		{
			this.DataRow = dataRow.DataRow;
		}

		static class Constants
		{
			public static class SegmentCode
			{
				public const int Length = 3;
				public const int Position = 0;
			}
		}

		public ZString SegmentCode
		{
			get { return DataRow.SubstringSafe(Constants.SegmentCode.Position, Constants.SegmentCode.Length).Trim(); }
		}

		protected readonly ZString DataRow;
	}
}
