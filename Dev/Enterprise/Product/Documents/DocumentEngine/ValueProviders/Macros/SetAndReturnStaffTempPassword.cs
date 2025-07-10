using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class SetAndReturnStaffTempPassword : ValueProvider
	{
		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<SetAndReturnStaffTempPassword\(\""*([^\""]+)\""*\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<SetAndReturnStaffTempPassword({staff_pk})>",
				ResString.GetMultilingualString("931f8a3f-8bff-4ab4-bcb2-fc8e3d253f3f", "Sets and returns the staff temp. password which must be reset on first login."),
				new List<(string example, object expectedResult)> { ("<SetAndReturnStaffTempPassword(8b6e5deb-5c2b-4529-8c5b-96736e728cc1)>", string.Empty) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var groups = Regex.Match(macro);
			var pk = groups.Groups[1].ToString().Trim();
			if (!Guid.TryParse(pk, out var staffPk))
			{
				ReportMacroError(report, Res.GetString("5915e9c1-cf74-49a0-9960-438b4a8aec4c", "Could not convert to Guid: {0}", pk));
				return string.Empty;
			}

			var staff = report.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffPk));

			if (staff == null)
			{
				ReportMacroError(report, Res.GetString("1bb7646a-bdf3-4083-8347-11baafd1c971", "Staff not found: {0}", pk));
				return string.Empty;
			}
			if (!staff.LocalPasswordMustBeReset)
			{
				ReportMacroError(report, Res.GetString("e9e4ae62-e478-42dc-8d19-507e403c57ff", "Staff record is found but password cannot be reset"));
				return string.Empty;
			}
			if (staff.GS_IsSystemAccount)
			{
				ReportMacroError(report, Res.GetString("6752dff5-827e-42db-92fe-427e65873b12", "Staff record is found but the password of the system account cannot be reset"));
				return string.Empty;
			}

			var password = string.Empty;
			using (var rng = RandomNumberGenerator.Create())
			{
				var tokenData = new byte[9];
				rng.GetBytes(tokenData);
				password = Convert.ToBase64String(tokenData);
			}
			staff.ChangeLocalPassword(null, password);
			staff.GS_ChangePasswordAtNextLogin = true;
			return password;
		}
	}
}
