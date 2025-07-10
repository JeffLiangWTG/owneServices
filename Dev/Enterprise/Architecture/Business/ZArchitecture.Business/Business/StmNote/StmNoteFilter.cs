using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using JC = CargoWise.EntityFramework.JoinCondition;

namespace Enterprise.ZArchitecture.Business
{
	public class StmNoteQuery : ZQuery
	{
		public StmNoteQuery()
		{
			AddToFilter(JC.Or, StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, nameof(StmNoteVisibility.PUB));
			AddToFilter(JC.Or, StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, nameof(StmNoteVisibility.PRV));
			AddToFilter(JC.Or, StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, nameof(StmNoteVisibility.INT));
			AddToFilter(JC.Or, StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, nameof(StmNoteVisibility.AGV));
		}
	}

	public class StmNoteNonPublicNotesWithContextQuery : ZQuery
	{
		public StmNoteNonPublicNotesWithContextQuery(BusinessObject master, StmNoteContexts validContexts)
			: base()
		{
			AddToFilter(new ZQuery(StmNoteSchema.ST_ParentID, master.PK));
			AddToFilter(new ZQuery(StmNoteSchema.ST_Table, master.TableName));

			ZQuery nonPrivateQuery = new ZQuery(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.PUB));
			nonPrivateQuery.AddToFilter(JC.Or, StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, nameof(StmNoteVisibility.INT));
			nonPrivateQuery.AddToFilter(JC.Or, StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, nameof(StmNoteVisibility.AGV));

			AddToFilter(nonPrivateQuery, JC.And);
			AddContextFilter(validContexts);
		}

		protected void AddContextFilter(StmNoteContexts validContexts)
		{
			validContexts = AddDefaultsToValidContexts(validContexts);

			ZDBOnlyQuery contextFilter = new ZDBOnlyQuery(typeof(StmNote));

			ZString filterSqlString = GetFilterSqlStringForContextFilter(validContexts);

			contextFilter.AddFilterAndZSQLParameterCollection(filterSqlString, new ZSqlParameterCollection());

			AddToFilter(contextFilter, JC.And);
		}

		StmNoteContexts AddDefaultsToValidContexts(StmNoteContexts validContexts)
		{
			if ((StmNoteContextModule.A & validContexts.Module) != StmNoteContextModule.A)
			{
				validContexts.Module |= StmNoteContextModule.A;
			}
			if ((StmNoteContextDirection.A & validContexts.Direction) != StmNoteContextDirection.A)
			{
				validContexts.Direction |= StmNoteContextDirection.A;
			}
			if ((StmNoteContextFreightMode.A & validContexts.FreightMode) != StmNoteContextFreightMode.A)
			{
				validContexts.FreightMode |= StmNoteContextFreightMode.A;
			}

			return validContexts;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be a part of SQL expression.")]
		ZString GetFilterSqlStringForContextFilter(StmNoteContexts validContexts)
		{
			ZString filterSqlString = new ZString();
			filterSqlString = StmNoteSchema.PK.Name + " IN (SELECT " + StmNoteSchema.PK.Name + " FROM " + StmNoteSchema.Constants.SqlSchemaName + "." + StmNoteSchema.Constants.TableName + " WHERE ";
			filterSqlString += "(";

			bool excludeContextAllFromModule = false;
			if ((StmNoteContextModule.W & validContexts.Module) == StmNoteContextModule.W)
			{
				filterSqlString += StmNote.Schema.ST_NoteContextModule + " = 'A' AND ";
				filterSqlString += StmNote.Schema.ST_NoteContextDirection + " = 'A' AND ";
				filterSqlString += StmNote.Schema.ST_NoteContextFreightMode + " = 'A') OR ((";
				excludeContextAllFromModule = true;
			}

			bool contextSegmentIsNotEmpty = false;
			foreach (StmNoteContextModule context in Enum.GetValues(typeof(StmNoteContextModule)))
			{
				if ((context & validContexts.Module) == context &&
					context != StmNoteContextModule.Undefined &&
					(!excludeContextAllFromModule || (context != StmNoteContextModule.A && excludeContextAllFromModule)))
				{
					filterSqlString += StmNote.Schema.ST_NoteContextModule + " = '" + context.ToString() + "' OR ";
					contextSegmentIsNotEmpty = true;
				}
			}

			if (contextSegmentIsNotEmpty)
			{
				filterSqlString = filterSqlString.Substring(0, filterSqlString.Length - 4);
				filterSqlString += ") ";
				filterSqlString += "AND (";
			}

			contextSegmentIsNotEmpty = false;
			foreach (StmNoteContextDirection context in Enum.GetValues(typeof(StmNoteContextDirection)))
			{
				if ((context & validContexts.Direction) == context && context != StmNoteContextDirection.Undefined)
				{
					filterSqlString += StmNote.Schema.ST_NoteContextDirection + " = '" + context.ToString() + "' OR ";
					contextSegmentIsNotEmpty = true;
				}
			}

			if (contextSegmentIsNotEmpty)
			{
				filterSqlString = filterSqlString.Substring(0, filterSqlString.Length - 4);
				filterSqlString += ") ";
				filterSqlString += "AND (";
			}

			contextSegmentIsNotEmpty = false;
			foreach (StmNoteContextFreightMode context in Enum.GetValues(typeof(StmNoteContextFreightMode)))
			{
				if ((context & validContexts.FreightMode) == context && context != StmNoteContextFreightMode.Undefined)
				{
					filterSqlString += StmNote.Schema.ST_NoteContextFreightMode + " = '" + context.ToString() + "' OR ";
					contextSegmentIsNotEmpty = true;
				}
			}

			if (contextSegmentIsNotEmpty)
			{
				filterSqlString = filterSqlString.Substring(0, filterSqlString.Length - 4);
				filterSqlString += ")";
				if ((StmNoteContextModule.W & validContexts.Module) == StmNoteContextModule.W)
				{
					filterSqlString += ")";
				}
			}

			if (filterSqlString.Substring(filterSqlString.Length - 9) == " WHERE ()")
			{
				filterSqlString = filterSqlString.Substring(0, filterSqlString.Length - 9);
			}
			filterSqlString += ")";

			return filterSqlString;
		}
	}
}
