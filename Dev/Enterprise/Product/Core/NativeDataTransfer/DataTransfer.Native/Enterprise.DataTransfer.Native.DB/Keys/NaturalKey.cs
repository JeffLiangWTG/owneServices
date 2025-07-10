using System;
using System.Linq;

namespace Enterprise.DataTransfer.Native.DB.Keys
{
	public class NaturalKey : ForeignKey
	{
		public NaturalKey(ColumnDef columnDef)
			: base(columnDef)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No translation required.")]
		public override ColumnDef ReferenceColumnDef
		{
			get
			{
				var columnName = ReferenceTable.Prefix + "_Code";

				var column = ReferenceTable.Columns.Find(columnName);
				if (column != null)
				{
					return column;
				}

				var candidateKeys = ReferenceTable.SingleColumnCandidateConstraints.ToArray();

				if (candidateKeys.Length == 0)
				{
					throw new InvalidOperationException("ReferenceTable " + ReferenceTable.Name + " should have a Candidate Key for Natural Key " + Name + " to refer");
				}

				return candidateKeys.FirstOrDefault(k => k.Name.Contains("Code")) ?? candidateKeys.First();
			}
		}
	}
}