using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class GetStaffValueForPerson : ValueProvider
	{
		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<(?:\s*)GetStaffValueForPerson(?:\s*)\((?:[\s]*)([^\s]+)(?:[\s]*)(,(.+))?(?:[\s]*)\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GetStaffValueForPerson({applicantPk}[, {fieldname}])>",
				ResString.GetMultilingualString("1C4BC8FA-8D10-4908-AE49-BB30D7C1C829", "Returns the staff PK of the person related to the on-boarding applicant."),
				new List<(string example, object expectedResult)> { ("<GetStaffValueForPerson(2032CF10-385B-4555-BB3F-CEC65A24F61D, GS_LoginName)>", string.Empty) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var groups = Regex.Match(macro);
			var pk = groups.Groups[1].ToString().Trim();
			if (!Guid.TryParse(pk, out var personPK))
			{
				ReportMacroError(report, Res.GetString("49691908-3AEA-4338-91FC-DE13E865E338", "Could not convert to Guid : {0}", pk));
				return string.Empty;
			}

			var staff = report.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_PER, personPK));
			if (staff == null)
			{
				ReportMacroError(report, Res.GetString("B74D1D13-81CC-444A-AA43-33F44B277EFB", "Staff not found."));
				return string.Empty;
			}

			if (!string.IsNullOrEmpty(groups.Groups[3].ToString()))
			{
				var fieldname = groups.Groups[3].ToString().Trim().Replace("'", String.Empty).Replace("\"", string.Empty);
				var property = typeof(GlbStaff).GetProperty(fieldname, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.IgnoreCase);
				if (property == null)
				{
					ReportMacroError(report, Res.GetString("88C2DD7C-D638-4619-818A-C4995536D1AA", "Field not found."));
					return string.Empty;
				}
				return property?.GetValue(staff)?.ToString() ?? String.Empty;
			}
			return staff.PK.ToString();
		}
	}
}
