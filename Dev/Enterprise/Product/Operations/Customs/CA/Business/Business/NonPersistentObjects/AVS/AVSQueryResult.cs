using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Business
{
	public class AVSQueryResult : NonPersistentBusinessObject
	{
		public AVSQueryResult(StmNote stmNote) : base(stmNote.Factory)
		{
			var notes = stmNote.ST_NoteText.Split(new char[] { '|' }, 2);
			if (notes.Length == 1)
			{
				fQueryResult = notes[0];
			}
			else if (notes.Length > 1)
			{
				if (!ZDateTime.TryParseISO8601Date(notes[0], out fQueryTime))
				{
					fQueryTime = ZDateTime.Empty;
				}
				fQueryResult = notes[1];
			}
		}

		#region Schema

		public static class Schema
		{
			public const string QueryTime = "QueryTime";
			public const string QueryResult = "QueryResult";
		}

		#endregion

		#region QueryTime

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.CA.Business.AVSQueryResult|QueryTime", Caption = "Query Time")]
		public virtual ZDateTime QueryTime
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return fQueryTime;
			}
			set
			{
				SetNonPersistentPropertyValue(QueryTimeInfo, ref fQueryTime, value);
			}
		}

		public virtual ZPropertyInfo QueryTimeInfo
		{
			get
			{
				return this.GetZPropertyInfo(Schema.QueryTime);
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		ZDateTime fQueryTime;

		#endregion

		#region QueryResult

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.CA.Business.AVSQueryResult|QueryResult", Caption = "Query Result")]
		public virtual ZString QueryResult
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return fQueryResult;
			}
			set
			{
				SetNonPersistentPropertyValue(QueryResultInfo, ref fQueryResult, value);
			}
		}

		public virtual ZPropertyInfo QueryResultInfo
		{
			get
			{
				return this.GetZPropertyInfo(Schema.QueryResult);
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		ZString fQueryResult;

		#endregion
	}
}
