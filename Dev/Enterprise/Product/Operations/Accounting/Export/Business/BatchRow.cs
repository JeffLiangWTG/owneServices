using System;

namespace Enterprise.Accounting.Export.Business
{
	public class BatchRow
	{
		public BatchRow()
		{
		}

		public BatchRow(Guid parentId, string parentTableCode, string rowType, int sequence, Guid ah_pk)
		{
			ParentID = parentId;
			ParentTableCode = parentTableCode;
			RowType = rowType;
			Sequence = sequence;
			AH_PK = ah_pk;
		}

		public Guid ParentID { get; set; }
		public string ParentTableCode { get; set; }
		public string RowType { get; set; }
		public int Sequence { get; set; }
		public Guid AH_PK { get; set; }
	}
}
