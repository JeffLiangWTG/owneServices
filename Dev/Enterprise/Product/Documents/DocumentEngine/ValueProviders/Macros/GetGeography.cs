using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class GetGeography : ValueProvider
	{
		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<\s*GetGeography\s*\(\s*""\s*((?:\w+)|(?:\w[\w\s]*\w))\s*""\s*,\s*""\s*((?:\w+)|(?:\w[\w\s]*\w))\s*""\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GetGeography(\"{ShapeName}\", \"{ShapeType}\")>",
				ResString.GetMultilingualString("df355834-adb6-4a8b-9dc2-87b50f89f47b", @"This macro will return the geography defined under Location --> Geography.
The parameter {0} is the name of the shape as defined in the Geography module.
The parameter {1} is the geography type of the shape as defined in the Geography module.",
"{ShapeName}", "{ShapeType}"),
				new List<(string example, object expectedResult)> {
					((NoResString)"<GetGeography(\"SCOTLAND\", \"UKN\")>", ZGeography.CreatePolygon("0 0,1 0,1 1,0 1,0 0")) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var shapeName = match.Groups[1].Value.ToUpperInvariant();
			var shapeType = match.Groups[2].Value.ToUpperInvariant();
			var findShapeQuery = new ZQuery(GenShapeGeographySchema.SHG_Name, shapeName);
			findShapeQuery.AddToFilter(GenShapeGeographySchema.SHG_Type, shapeType);
			findShapeQuery.AddToFilter(GenShapeGeographySchema.SHG_IsActive, true);
			var shapeGeography = report.Factory.LoadTop1<IGenShapeGeography>(findShapeQuery);

			if (shapeGeography == null)
			{
				ReportMacroError(report, Res.GetString("25b09765-f6b8-4952-9b91-aa6f42968b83", "Geography shape '{0}' with type '{1}' does not exist, or it is not active.", shapeName, shapeType));
				return ZGeography.Empty;
			}
			else
			{
				return shapeGeography.SHG_Shape;
			}
		}
	}
}
