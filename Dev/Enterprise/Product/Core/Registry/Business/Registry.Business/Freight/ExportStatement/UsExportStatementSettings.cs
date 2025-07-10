using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Registry.Business
{
	internal static class UsExportStatementSettings
	{
		public static ISet<ExportStatementSetting> ReferenceSet
		{
			get
			{
				return referenceSet ?? (referenceSet = BuildReferenceSet(null));
			}
		}

		[ThreadSafe]
		static ISet<ExportStatementSetting> referenceSet;

		public static ISet<ExportStatementSetting> BuildReferenceSet(CountryExportStatementSetting countryExportStatementSetting)
		{
			var exportStatementSettings = GetReferenceExportStatementSettings()
				.Select(statement => new ExportStatementSetting(countryExportStatementSetting,
					statement.Code, statement.Statement, statement.StatementDescription,
					string.Empty, string.Empty, "UDF",
					true, true, true, true, true, true));
			return new HashSet<ExportStatementSetting>(exportStatementSettings, new ExportStatementSettingEqualityComparer());
		}

		static IEnumerable<(string Code, string Statement, string StatementDescription)> GetReferenceExportStatementSettings()
		{
			yield return ("PRF", "AES", (NoResString)"AES Proof of Filing Citation");
			yield return ("ASH", "AES", (NoResString)"AES Split Shipments");
			yield return ("PDU", "AESPOST", (NoResString)"Postdeparture Citation-USPPI");
			yield return ("PDA", "AESPOST", (NoResString)"Postdeparture Citation-Agent");
			yield return ("DWN", "AESDOWN", (NoResString)"AES Downtime Citation");
			yield return ("LOW", (NoResString)"NOEEI §30.37(a)", (NoResString)"NOEEI §30.37(a) - Low Value (<$2501)");
			yield return ("TOT", (NoResString)"NOEEI §30.37(b)", (NoResString)"NOEEI §30.37(b) - Tools of trade");
			yield return ("TMP", (NoResString)"NOEEI §30.37(r)", (NoResString)"NOEEI §30.37(r) - Return of Temporary Import Bond");
			yield return ("CAS", (NoResString)"NOEEI §30.36", (NoResString)"NOEEI §30.36");
			yield return ("ARM", (NoResString)"NOEEI §30.39", (NoResString)"NOEEI §30.39 - Shipments to US armed services");
			yield return ("BND", (NoResString)"NOEEI §30.2(d)(1)", (NoResString)"NOEEI §30.2(d)(1) - Goods under CBP bond, not in consumption");
			yield return ("TER", (NoResString)"NOEEI §30.2(d)(2)", (NoResString)"NOEEI §30.2(d)(2) - Goods between US and US territories (not PR, VI)");
			yield return ("EET", (NoResString)"NOEEI §30.2(d)(3)", (NoResString)"NOEEI §30.2(d)(3) - Exclusion for electronic transmissions and intangible transfers");
			yield return ("GFT", (NoResString)"NOEEI §30.37(h)", (NoResString)"NOEEI §30.37(h) - 15 CFR 740.12(a)&(b) Gifts and donations");
			yield return ("DIP", (NoResString)"NOEEI §30.37(i)", (NoResString)"NOEEI §30.37(i) - Diplomatic pouches");
			yield return ("REM", (NoResString)"NOEEI §30.37(j)", (NoResString)"NOEEI §30.37(j) - Human remains");
			yield return ("AVS", (NoResString)"NOEEI §30.37(o)", (NoResString)"NOEEI §30.37(o) - 15 CFR 740.15(c) Parts for US airlines");
			yield return ("TME", (NoResString)"NOEEI §30.37(q)", (NoResString)"NOEEI §30.37(q) - Temporary Exports (1 YR)");
			yield return ("GBN", (NoResString)"NOEEI §30.2(d)(4)", (NoResString)"NOEEI §30.2(d)(4) - Goods to Guantanamo Bay Naval Base");
			yield return ("IWA", (NoResString)"NOEEI §30.2(d)(5)", (NoResString)"NOEEI §30.2(d)(5) - Ultimate Dest. US or Int'l Waters for US person");
			yield return ("UMC", (NoResString)"NOEEI §30.37(c)", (NoResString)"NOEEI §30.37(c) - Shipments from US to US transiting through MX or CA");
			yield return ("MCU", (NoResString)"NOEEI §30.37(d)", (NoResString)"NOEEI §30.37(d) - Shipments from MX to CA or CA to MX transiting through US");
			yield return ("TSW", (NoResString)"NOEEI §30.37(f)", (NoResString)"NOEEI §30.37(f) - 15 CFR 772 Technology and software");
			yield return ("BKS", (NoResString)"NOEEI §30.37(g)", (NoResString)"NOEEI §30.37(g) - Literature to libraries, governments");
			yield return ("BUS", (NoResString)"NOEEI §30.37(k)", (NoResString)"NOEEI §30.37(k) - Company records");
			yield return ("PET", (NoResString)"NOEEI §30.37(l)", (NoResString)"NOEEI §30.37(l) - Pets as baggage");
			yield return ("CAR", (NoResString)"NOEEI §30.37(m)", (NoResString)"NOEEI §30.37(m) - Carriers' stores");
			yield return ("DUN", (NoResString)"NOEEI §30.37(n)", (NoResString)"NOEEI §30.37(n) - Dunnage");
			yield return ("BGG", (NoResString)"NOEEI §30.37(p)", (NoResString)"NOEEI §30.37(p) - Passenger, Crew Baggage");
			yield return ("BNK", (NoResString)"NOEEI §30.37(s)", (NoResString)"NOEEI §30.37(s) - Issued bank notes, securities, coins");
			yield return ("TDC", (NoResString)"NOEEI §30.37(t)", (NoResString)"NOEEI §30.37(t) - International transaction documents");
			yield return ("DAT", (NoResString)"NOEEI §30.37(u)", (NoResString)"NOEEI §30.37(u) - 22 CFR 123.22(b)(3)(iii) Technical data, Defense services");
			yield return ("VSL", (NoResString)"NOEEI §30.37(v)", (NoResString)"NOEEI §30.37(v) - Shipping containers");
			yield return ("APO", (NoResString)"NOEEI §30.37(w)", (NoResString)"NOEEI §30.37(w) - Shipments to Army Post Office, Diplomatic Post Office, Fleet Post Office");
			yield return ("BAG", (NoResString)"NOEEI §30.37(x)", (NoResString)"NOEEI §30.37(x) - 15 CFR 740.14 Baggage");
			yield return ("OFE", (NoResString)"NOEEI §30.40(a)", (NoResString)"NOEEI §30.40(a) - Office equipment for US gov offices");
			yield return ("HHG", (NoResString)"NOEEI §30.40(b)", (NoResString)"NOEEI §30.40(b) - Household goods for US gov employees");
			yield return ("FME", (NoResString)"NOEEI §30.40(c)", (NoResString)"NOEEI §30.40(c) - Food, medicines, supplies for US gov");
			yield return ("EY1", (NoResString)"NOEEI §30.37(y)(1)", (NoResString)"NOEEI §30.37(y)(1) – Published literature/media destined to country group E:1 and E:2");
			yield return ("EY2", (NoResString)"NOEEI §30.37(y)(2)", (NoResString)"NOEEI §30.37(y)(2) – Shipments to U.S. government destined to Country Group E:1 and E:2 under License Exception GOV");
			yield return ("EY3", (NoResString)"NOEEI §30.37(y)(3)", (NoResString)"NOEEI §30.37(y)(3) - Personal effects exported as exemption BAG to Country Group E:1 and E:2");
			yield return ("EY4", (NoResString)"NOEEI §30.37(y)(4)", (NoResString)"NOEEI §30.37(y)(4) - Gifts/donations exported to Country Group E:1 and E:2 under License Exception GFT");
			yield return ("EY5", (NoResString)"NOEEI §30.37(y)(5)", (NoResString)"NOEEI §30.37(y)(5) – Vessels/Aircraft temporary export to Country Group E:1 or E:2 under License Exception AVS");
			yield return ("EY6", (NoResString)"NOEEI §30.37(y)(6)", (NoResString)"NOEEI §30.37(y)(6) - Tools of trade temporary export (1yr) for use in Country Group E:1 or E:2 under License Exception BAG or TMP");
		}

		#region Equality Comparer

		sealed class ExportStatementSettingEqualityComparer : IEqualityComparer<ExportStatementSetting>
		{
			public bool Equals(ExportStatementSetting x, ExportStatementSetting y)
			{
				if (ReferenceEquals(x, y))
				{
					return true;
				}

				if (ReferenceEquals(x, null))
				{
					return false;
				}

				if (ReferenceEquals(y, null))
				{
					return false;
				}

				if (x.GetType() != y.GetType())
				{
					return false;
				}
				return x.Code.Equals(y.Code)
					&& x.Statement.Equals(y.Statement)
					&& x.StatementDescription.Equals(y.StatementDescription);
			}

			public int GetHashCode(ExportStatementSetting obj)
			{
				unchecked
				{
					var hashCode = obj.Code.GetHashCode();
					hashCode = (hashCode * 397) ^ obj.Statement.GetHashCode();
					hashCode = (hashCode * 397) ^ obj.StatementDescription.GetHashCode();
					return hashCode;
				}
			}
		}

		#endregion
	}
}
