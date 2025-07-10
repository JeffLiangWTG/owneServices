using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class GetPropertyFromEntity : ValueProvider
	{
		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<(?:\s*)GetPropertyFromEntity(?:\s*)\(""(?:[\s]*)(.*)(?:[\s]*)"",""(?:[\s]*)(.*)(?:[\s]*)"",""(?:[\s]*)(.*)(?:[\s]*)""\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GetPropertyFromEntity({entityName},{pk},{property})>",
				ResString.GetMultilingualString("2032CF10-385B-4555-BB3F-CEC65A24F61E", "Returns a property from the entity only a limited set of entities are supported."),
				new List<(string example, object expectedResult)> { ("<GetPropertyFromEntity(\"GlbStaffHoliday\",\"2032CF10-385B-4555-BB3F-CEC65A24F61D\",\"GA_ApprovalStatus\")>", string.Empty) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = string.Empty;
			var groups = Regex.Match(macro);
			var typeName = groups.Groups[1].ToString().Trim();
			var type = GetTypeFromName(typeName);

			if (type == null)
			{
				ReportMacroError(report, Res.GetString("D196D3D5-1BA1-4D5C-8427-54292108E384", "Could not load type: {0}", typeName));
				return result;
			}

			var pkString = groups.Groups[2].ToString().Trim();
			if (string.IsNullOrEmpty(pkString))
			{
				return result;
			}

			if (!Guid.TryParse(pkString, out var pk))
			{
				ReportMacroError(report, Res.GetString("491CFE78-310A-4E91-B8EE-3E3D9D1FC84C", "Could not parse Guid: {0}", pkString));
				return result;
			}

			var propertyName = groups.Groups[3].ToString().Trim();
			try
			{
				var property = type.GetProperty(propertyName);
				if (property == null)
				{
					ReportMacroError(report, Res.GetString("FA1C1CCC-7A61-4E9E-BC58-0ECADB808397", "Property could not be found: {0}", propertyName));
					return result;
				}

				var entity = new BusinessObjectFactory().Load(type, pk);
				if (entity == null)
				{
					ReportMacroError(report, Res.GetString("E3C92CA0-0FDE-4BAA-BA66-251918E96B10", "Could not find {0}. PK: {1}", typeName, pkString));
					return result;
				}

				return property.GetValue(entity, null);
			}
			catch (Exception e)
			{
				ReportMacroError(report, Res.GetString("DABD6B38-2184-4BE8-ABCC-425C99754232", "Encountered error {0}", e.Message));
			}
			return result;
		}

		Type GetTypeFromName(string name)
		{
			switch (name)
			{
				case "GlbStaffHoliday":
					return typeof(GlbStaffHoliday);
				case "AccGLHeader":
					return typeof(AccGLHeader);
				case "GlbBranch":
					return typeof(GlbBranch);
				case "GlbDepartment":
					return typeof(GlbDepartment);
				default:
					return null;
			}
		}
	}
}
