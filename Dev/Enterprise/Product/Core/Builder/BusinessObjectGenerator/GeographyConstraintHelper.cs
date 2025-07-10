using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;

namespace Enterprise.BusinessObjectGenerator
{
	public class GeographyConstraintHelper
	{
		public IEnumerable<string> GetAllowedSpatialTypes(string tableName, string columnName)
		{
			tableName = SanitizeName(tableName);
			columnName = SanitizeName(columnName);
			var allTypes = SpatialType.All;
			var sql = $@"
select 
    con.definition
from sys.check_constraints con
    inner join sys.objects t
        on con.parent_object_id = t.object_id
    inner join sys.all_columns col
        on con.parent_column_id = col.column_id
        and con.parent_object_id = col.object_id
    inner join sys.types tp
        on col.system_type_id = tp.system_type_id
	where t.name = '{tableName}' and col.name = '{columnName}' and con.is_disabled = 0 and tp.name = 'geography'
";

			var constraint = Db.Connection.ExecuteScalar(sql) as string;

			if (string.IsNullOrEmpty(constraint))
			{
				return allTypes;
			}

			constraint = constraint.Replace("[" + columnName + "]", "@test_geography");
			sql = @"
declare @test_geography geography = convert(geography, '{0}').MakeValid();
select case when {1} then 1 else 0 end;
" + DbCommand.ExecuteAsReaderFlagComments;

			var result = from type in allTypes
						 where (int)Db.Connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, sql, GetTestGeographyValue(type), constraint)) == 1
						 select type;

			return result.ToList();
		}

		string SanitizeName(string input)
		{
			return new Regex(@"[^\w]").Replace(input, "");
		}

		string GetTestGeographyValue(string spatialType)
		{
			switch (spatialType)
			{
				case SpatialType.Point:
					return "POINT (1 1)";
				case SpatialType.LineString:
					return "LINESTRING (1 1, 2 2)";
				case SpatialType.CircularString:
					return "CIRCULARSTRING(1 1, 2 0, 2 0, 1 1, 0 1)";
				case SpatialType.CompoundCurve:
					return "COMPOUNDCURVE(CIRCULARSTRING(1 0, 0 1, -1 0), (-1 0, 2 0))";
				case SpatialType.Polygon:
					return "POLYGON ((0 0, 1 0, 1 1, 0 1, 0 0))";
				case SpatialType.CurvePolygon:
					return "CURVEPOLYGON(CIRCULARSTRING(1 3, 3 5, 4 7, 7 3, 1 3))";
				case SpatialType.MultiPoint:
					return "MULTIPOINT ((0 0), (1 1))";
				case SpatialType.MultiLineString:
					return "MULTILINESTRING((1 1, 3 5), (-5 3, -8 -2))";
				case SpatialType.MultiPolygon:
					return "MULTIPOLYGON(((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 1, 1 1)), ((9 9, 9 10, 10 9, 9 9)))";
				case SpatialType.GeometryCollection:
					return "GEOMETRYCOLLECTION(LINESTRING(1 1, 3 5),POLYGON((-1 -1, 1 -5, -5 5, -5 -1, -1 -1)))";
				default:
					return GetTestGeographyValue(SpatialType.Point);
			}
		}

		public static class SpatialType
		{
			public const string Point = "Point";
			public const string LineString = "LineString";
			public const string CircularString = "CircularString";
			public const string CompoundCurve = "CompoundCurve";
			public const string Polygon = "Polygon";
			public const string CurvePolygon = "CurvePolygon";
			public const string MultiPoint = "MultiPoint";
			public const string MultiLineString = "MultiLineString";
			public const string MultiPolygon = "MultiPolygon";
			public const string GeometryCollection = "GeometryCollection";

			public static IEnumerable<string> All
			{
				get
				{
					yield return Point;
					yield return LineString;
					yield return CircularString;
					yield return CompoundCurve;
					yield return Polygon;
					yield return CurvePolygon;
					yield return MultiPoint;
					yield return MultiLineString;
					yield return MultiPolygon;
					yield return GeometryCollection;
				}
			}
		}
	}
}
