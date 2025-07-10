using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class GetCurrentManagerCodeByType : ValueProvider
	{
		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<(?:\s*)GetCurrentManagerCodeByType(?:\s*)\(""(?:[\s]*)(.*)(?:[\s]*)"",""(?:[\s]*)(.*)(?:[\s]*)""\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GetCurrentManagerCodeByType({staff_pk},{manager_type})>",
				ResString.GetMultilingualString("2032CF10-385B-4555-BB3F-CEC65A24F61D", "Returns the staff code of the manager with the specified type for the provided staff PK."),
				new List<(string example, object expectedResult)> { ((NoResString)"<GetCurrentManagerCodeByType(\"2032CF10-385B-4555-BB3F-CEC65A24F61D\",\"DRM\")>", string.Empty) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var groups = Regex.Match(macro);
			var pk = groups.Groups[1].ToString().Trim();
			if (!Guid.TryParse(pk, out var staffPk))
			{
				ReportMacroError(report, Res.GetString("49691908-3AEA-4338-91FC-DE13E865E338", "Could not convert to Guid : {0}", pk));
				return string.Empty;
			}

			var staff = report.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffPk));

			if (staff == null)
			{
				ReportMacroError(report, Res.GetString("2D15F799-D17E-40E6-887D-63CB945C0735", "Staff not found: {0}", pk));
				return string.Empty;
			}

			var managerType = groups.Groups[2].ToString().Trim();
			var today = ZDateTime.Now;

			var staffCode = staff.Managers
				.Where(m =>
					m.GSM_EffectiveDate <= today
					&& (m.GSM_EndDate.IsEmpty || m.GSM_EndDate > today)
					&& m.GSM_ManagerType == managerType)
				.Select(m => m.Manager.GS_Code)
				.FirstOrDefault();

			if (!staffCode.IsEmpty)
			{
				return staffCode;
			}
			return string.Empty;
		}
	}
}
