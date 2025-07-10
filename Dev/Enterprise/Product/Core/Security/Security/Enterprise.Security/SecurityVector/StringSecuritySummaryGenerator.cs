using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Security
{
	public sealed class StringSecuritySummaryGenerator : SecuritySummaryGenerator<string>
	{
		public static string Granted { get { return Res.GetString("906037e4-2ae4-49e2-87e9-beff1fb9dc21", "Granted"); } }
		public static string Denied { get { return Res.GetString("bdd841f7-7bd6-4631-a6bd-858019d41790", "Denied"); } }

		public override string GrantedValue { get { return Granted; } }

		public override string DeniedValue { get { return Denied; } }

		protected override string GenerateSummaryCore(string[] departments, string[] branches, bool[,] rights)
		{
			bool defaultAccess = rights[0, 0];
			StringBuilder sb = new StringBuilder();
			List<int> appendedDepartments = new List<int>();
			List<int> appendedBranches = new List<int>();
			for (int i = 1; i < departments.Length; i++)
			{
				if (rights[i, 0] != defaultAccess)
				{
					bool append = true;
					for (int j = 1; j < branches.Length; j++)
					{
						if (rights[i, j] != rights[i, 0])
						{
							append = false;
							break;
						}
					}
					if (append)
					{
						AppendSummary(sb, departments[i], "*");
						appendedDepartments.Add(i);
					}
				}
			}
			for (int j = 1; j < branches.Length; j++)
			{
				if (rights[0, j] != defaultAccess)
				{
					bool append = true;
					for (int i = 1; i < departments.Length; i++)
					{
						if (rights[i, j] != rights[0, j])
						{
							append = false;
							break;
						}
					}
					if (append)
					{
						AppendSummary(sb, "*", branches[j]);
						appendedBranches.Add(j);
					}
				}
			}
			for (int i = 1; i < departments.Length; i++)
			{
				if (appendedDepartments.Contains(i))
				{
					continue;
				}

				for (int j = 1; j < branches.Length; j++)
				{
					if (appendedBranches.Contains(j))
					{
						continue;
					}

					if (rights[i, j] != defaultAccess)
					{
						AppendSummary(sb, departments[i], branches[j]);
					}
				}
			}

			string result;
			if (sb.Length == 0)
			{
				result = defaultAccess ? Granted : Denied;
			}
			else if (defaultAccess)
			{
				result = Res.GetString("7abc6c29-16e7-4fbc-b5e1-c7a1824e2602", "{0}, except for logins to ({1})", Granted, sb.ToString());
			}
			else
			{
				result = Res.GetString("df4400e4-e55b-4598-bb1e-d1c3bf8353da", "{0} only for logins to ({1})", Granted, sb.ToString());
			}

			return result;
		}

		protected override string GenerateLocalAdministratorSummaryCore(IEnumerable<IGlbSecurity> securities)
		{
			if (securities != null)
			{
				var securityGroupings =
					from security in securities
					group security by (security is GlbSecurity ? ((GlbSecurity)security).ItemType : ZString.Empty) into g
					orderby g.Key
					select new { Key = g.Key, ItemCodes = g.Select(s => s is GlbSecurity ? ((GlbSecurity)s).ItemCode : ZString.Empty).Distinct().OrderBy(ic => ic) };
				StringBuilder sb = new StringBuilder();
				foreach (var securityGrouping in securityGroupings)
				{
					if (sb.Length == 0)
					{
						sb.Append(Res.GetString("58714dac-8b03-4d39-9d5c-1af9181ef222", "Granted for") + " ");
					}
					else
					{
						sb.Append(" | ");
					}

					sb.Append(securityGrouping.Key);
					sb.Append(" - ");

					bool first = true;
					foreach (var itemCode in securityGrouping.ItemCodes)
					{
						if (first)
						{
							first = false;
						}
						else
						{
							sb.Append(", ");
						}

						sb.Append(itemCode);
					}
				}

				if (sb.Length > 0)
				{
					return sb.ToString();
				}
			}

			return Denied;
		}

		void AppendSummary(StringBuilder sb, string department, string branch)
		{
			if (sb.Length > 0)
			{
				sb.Append(" | ");
			}

			sb.AppendFormat("{0}, {1}", department, branch);
		}
	}
}
