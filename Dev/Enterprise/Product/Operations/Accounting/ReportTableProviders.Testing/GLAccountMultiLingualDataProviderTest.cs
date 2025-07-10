using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.ReportTableProviders.Testing
{
	public class GLAccountMultiLingualDataProviderTest : TestCaseWithFactory
	{
		static readonly ZInt Period1 = new ZInt(200401);
		static readonly ZInt Period2 = new ZInt(200402);

		public class MockGLAccountMultiLingualDataProvider : GLAccountMultiLingualDataProvider
		{
			public MockGLAccountMultiLingualDataProvider(object[] reportParams)
			{
				base.SetupParameterValues(reportParams);
			}

			public ZString GetLanguage()
			{ return Language; }

			public ZBool GetIncludeAccountsWithoutTransactions()
			{ return IncludeAccountsWithoutTransactions; }

			public string[] GetAllMultiLingualSQL()
			{
				int selectCount = 0;
				string[] fullSQLToRun = base.GetSQL().Split(' ');
				ArrayList sQLBreakDown = new ArrayList();
				StringBuilder sQLStatement = new StringBuilder();

				foreach (string sQL in fullSQLToRun)
				{
					if (sQL.ToUpper().IndexOf("SELECT") > -1)
					{
						if (selectCount > 0)
						{
							sQLBreakDown.Add(sQLStatement.ToString());
							sQLStatement = new StringBuilder();
						}
						selectCount++;
					}
					sQLStatement.Append((sQL.Replace((char)10, ' ')).Replace((char)9, ' ').ToUpper() + " ");
				}
				sQLBreakDown.Add(sQLStatement.ToString());

				return (string[])sQLBreakDown.ToArray(typeof(string));
			}
		}

		public void TestSetMultilingualLanguage()
		{
			ZString fTestLanguage = new ZString("AAA");
			MockGLAccountMultiLingualDataProvider mockDataProvider = new MockGLAccountMultiLingualDataProvider(new object[] { Period1, Period2, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, fTestLanguage, "Y" });
			AssertEquals("MultiLingual Language", fTestLanguage, mockDataProvider.GetLanguage());
			AssertEquals("Include Accounts Without Transactions", true, mockDataProvider.GetIncludeAccountsWithoutTransactions());
		}

		public void TestSetIncludeAccounts()
		{
			MockGLAccountMultiLingualDataProvider mockDataProvider = new MockGLAccountMultiLingualDataProvider(new object[] { Period1, Period2, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, ZString.Empty, ZString.Empty });
			AssertEquals("Exclude accounts without transactions by default", false, mockDataProvider.GetIncludeAccountsWithoutTransactions());
			mockDataProvider = new MockGLAccountMultiLingualDataProvider(new object[] { Period1, Period2, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, ZString.Empty, "Y" });
			AssertEquals("Include accounts without transactions", true, mockDataProvider.GetIncludeAccountsWithoutTransactions());
		}

		public void TestMultilingualSQL()
		{
			string includeJoinPattern = @"\s*INNER\s*JOIN\s*(?:dbo\.)?ACCGLDESCRIPTORPIVOT\s*ON\s*YJ_AG\s*=\s*AG_PK\s*";
			MockGLAccountMultiLingualDataProvider mockDataProvider = new MockGLAccountMultiLingualDataProvider(new object[] { Period1, Period2, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, ZString.Empty, ZString.Empty });
			string[] multilingualSQL = mockDataProvider.GetAllMultiLingualSQL();
			foreach (string sQLSelectBlock in multilingualSQL)
			{
				AssertEquals("All multilingual GL transaction SELECT statements require inner join to AccGLAccountDescriptor: " + System.Environment.NewLine
					+ sQLSelectBlock, true, Regex.Match(sQLSelectBlock, includeJoinPattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase).Success);
			}
		}
	}
}
