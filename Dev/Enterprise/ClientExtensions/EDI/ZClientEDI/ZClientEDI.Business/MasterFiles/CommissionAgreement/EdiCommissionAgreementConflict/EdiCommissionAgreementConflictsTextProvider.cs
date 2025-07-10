using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementConflictsTextProvider : CommissionAgreementConflictsTextProvider
	{
		#region New

		protected EdiCommissionAgreementConflictsTextProvider()
		{
		}

		#endregion

		#region ToDisplayList

		public override string ToDisplayList(IEnumerable<ICommissionAgreementConflict> conflicts, int indentLevel, bool html)
		{
			var result = new ZStringBuilder();

			var databaseConflicts = new List<CommissionAgreementItemAndDatabaseConflict>();
			var companyConflicts = new List<CommissionAgreementItemAndCompanyConflict>();
			var companyAutoAddCountryConflicts = new List<CommissionAgreementItemAndCompanyAutoAddCountryConflict>();
			var companyAutoAddDatabaseConflicts = new List<CommissionAgreementItemAndCompanyAutoAddDatabaseConflict>();

			foreach (var conflict in conflicts.OfType<ICommissionAgreementItemConflict>())
			{
				var conflictType = conflict.GetType();
				if (conflictType == typeof(CommissionAgreementItemAndDatabaseConflict))
				{
					databaseConflicts.Add((CommissionAgreementItemAndDatabaseConflict)conflict);
				}
				else if (conflictType == typeof(CommissionAgreementItemAndCompanyConflict))
				{
					companyConflicts.Add((CommissionAgreementItemAndCompanyConflict)conflict);
				}
				else if (conflictType == typeof(CommissionAgreementItemAndCompanyAutoAddCountryConflict))
				{
					companyAutoAddCountryConflicts.Add((CommissionAgreementItemAndCompanyAutoAddCountryConflict)conflict);
				}
				else if (conflictType == typeof(CommissionAgreementItemAndCompanyAutoAddDatabaseConflict))
				{
					companyAutoAddDatabaseConflicts.Add((CommissionAgreementItemAndCompanyAutoAddDatabaseConflict)conflict);
				}
			}

			AppendCompanyConflictsList(result, companyConflicts, indentLevel, html);
			AppendCompanyAutoAddDatabaseConflictsList(result, companyAutoAddDatabaseConflicts, indentLevel, html);
			AppendCompanyAutoAddCountryConflictsList(result, companyAutoAddCountryConflicts, indentLevel, html);

			AppendDatabaseConflictsList(result, databaseConflicts, indentLevel, html);

			return result.ToString();
		}

		static void AppendCompanyConflictsList(ZStringBuilder result, IEnumerable<CommissionAgreementItemAndCompanyConflict> companyConflicts, int indentLevel, bool html)
		{
			var groupedCompanyConflicts = companyConflicts.GroupBy(x => x.ClientCompany);
			foreach (var companyConflictGrouping in groupedCompanyConflicts.OrderBy(x => x.Key == null ? ZString.Empty : x.Key.LCC_Name))
			{
				if (!html && !result.IsEmpty) { result.AppendLine(); }

				var company = companyConflictGrouping.Key;
				var header = company == null ? "All Company Usages" : string.Format(CultureInfo.CurrentCulture, "{0} ({1}) Company Usages", company.LCC_Name, company.LCC_Code);
				result.Append(GetNestedList(header, companyConflictGrouping, indentLevel, html));
			}
		}

		static void AppendCompanyAutoAddCountryConflictsList(ZStringBuilder result, IEnumerable<CommissionAgreementItemAndCompanyAutoAddCountryConflict> companyAutoAddCountryConflicts, int indentLevel, bool html)
		{
			var groupedCompanyAutoAddCountryConflicts = companyAutoAddCountryConflicts.GroupBy(x => new { x.LicenceDatabase, x.CountryCode });
			foreach (var companyAutoAddCountryConflictGrouping in groupedCompanyAutoAddCountryConflicts.OrderBy(x => x.Key.LicenceDatabase == null ? ZString.Empty : x.Key.LicenceDatabase.LD_ServerCode).ThenBy(x => x.Key.CountryCode))
			{
				if (!html && !result.IsEmpty) { result.AppendLine(); }

				var autoAddCountryConflict = companyAutoAddCountryConflictGrouping.Key;
				var header = autoAddCountryConflict.LicenceDatabase == null ?
					string.Format(CultureInfo.CurrentCulture, "Usages of newly created {0} companies", autoAddCountryConflict.CountryCode) :
					string.Format(CultureInfo.CurrentCulture, "Usages of newly created {0} companies for {1} Database", autoAddCountryConflict.CountryCode, autoAddCountryConflict.LicenceDatabase.LD_ServerCode);

				result.Append(GetNestedList(header, companyAutoAddCountryConflictGrouping, indentLevel, html));
			}
		}

		static void AppendCompanyAutoAddDatabaseConflictsList(ZStringBuilder result, IEnumerable<CommissionAgreementItemAndCompanyAutoAddDatabaseConflict> companyAutoAddDatabaseConflicts, int indentLevel, bool html)
		{
			var groupedCompanyAutoAddDatabaseConflicts = companyAutoAddDatabaseConflicts.GroupBy(x => x.LicenceDatabase);
			foreach (var companyAutoAddDatabaseConflictGrouping in groupedCompanyAutoAddDatabaseConflicts.OrderBy(x => x.Key == null ? ZString.Empty : x.Key.LD_ServerCode))
			{
				if (!html && !result.IsEmpty) { result.AppendLine(); }

				var database = companyAutoAddDatabaseConflictGrouping.Key;
				var header = database == null ?
					"Usages of all newly created companies" :
					string.Format(CultureInfo.CurrentCulture, "Usages of newly created companies for {0} Database", database.LD_ServerCode);

				result.Append(GetNestedList(header, companyAutoAddDatabaseConflictGrouping, indentLevel, html));
			}
		}

		static void AppendDatabaseConflictsList(ZStringBuilder result, IEnumerable<CommissionAgreementItemAndDatabaseConflict> databaseConflicts, int indentLevel, bool html)
		{
			var groupedDatabaseConflicts = databaseConflicts.GroupBy(x => x.LicenceDatabase);
			foreach (var databaseConflictGrouping in groupedDatabaseConflicts.OrderBy(x => x.Key == null ? ZString.Empty : x.Key.LD_ServerCode))
			{
				if (!html && !result.IsEmpty) { result.AppendLine(); }

				var database = databaseConflictGrouping.Key;
				var header = database == null ? "All Database Usages" : string.Format(CultureInfo.CurrentCulture, "{0} Database Usages", database.LD_ServerCode);
				result.Append(GetNestedList(header, databaseConflictGrouping, indentLevel, html));
			}
		}

		static string GetNestedList(string header, IEnumerable<ICommissionAgreementItemConflict> conflicts, int indentLevel, bool html)
		{
			var result = new ZStringBuilder();

			if (html)
			{
				result.Append("<li>[" + WebUtility.HtmlEncode(header) + "]<ul>");
				result.Append(GetLeafList(conflicts, indentLevel + 1, true));
				result.Append("</ul></li>");
			}
			else
			{
				var prefix = new string(' ', indentLevel * 2);
				result.AppendLine(prefix + "[" + header + "]");
				result.Append(GetLeafList(conflicts, indentLevel + 1, false));
			}

			return result.ToString();
		}

		public override string ToConflictWarningMessage(IEnumerable<ICommissionAgreementConflict> conflicts)
		{
			var result = base.ToConflictWarningMessage(conflicts);
			result += "To allow more specific Product / Service / Module subsets, click \"Add Filters (View Filters)\" and ensure the database and company filters have the same settings as above.";
			return result;
		}

		#endregion
	}
}

