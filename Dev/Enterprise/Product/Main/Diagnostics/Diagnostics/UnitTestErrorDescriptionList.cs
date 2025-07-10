#if DEBUG

using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.DevTools.Common;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Testing
{
	[CodeAlive("used by Enterprise.Dat.Implementation.TestRunner")]
	[Serializable]
	public class UnitTestErrorDescriptionListFactory : ErrorDescriptionListFactory
	{
		public override ErrorDescriptionList New()
		{
			return new UnitTestErrorDescriptionList();
		}
	}

	[Serializable]
	public class UnitTestErrorDescriptionList : ErrorDescriptionList
	{
		readonly ITestFailureDataClient testFailureDataClient;

		public UnitTestErrorDescriptionList() : this(new TestFailureDataClient())
		{ }

		public UnitTestErrorDescriptionList(ITestFailureDataClient testFailureDataClient)
		{
			this.testFailureDataClient = testFailureDataClient;
		}

		protected override string GetFinalText(Exception e)
		{
			string result;
			var version = System.Environment.Version;
			var glbCompany = GlbCompany.CurrentCompany;
			var proxyCompany = EnvProxy.Instance.CurrentCompany;
			var glbBranch = GlbBranch.CurrentBranch;
			var proxyBranch = EnvProxy.Instance.CurrentBranch;
			var glbDept = GlbDepartment.CurrentDepartment;
			var proxyDept = EnvProxy.Instance.CurrentDepartment;
			var serverName = Db.ServerName;

			result = @"<br><br>
			         <font style=""font-size:8pt""><strong>Current Environment Information</strong></font><br><br>
			         .NET "
#if !WINZOR
					 + "Framework "
#endif
					 + $@"{version}<br>
			         Db.ServerName = {serverName}<br>
			         <br>
			         <table style=""font-size:7pt; table-layout:auto"">" +

					 TableRow("GlbCompany.CurrentCompany.GC_RN_NKCountryCode", glbCompany?.GC_RN_NKCountryCode,
						 "Env.CurrentCompany.Country.Code", proxyCompany?.Country?.Code) +

					 TableRow("GlbBranch.CurrentBranch.GB_Code", glbBranch?.GB_Code,
						 "Env.CurrentBranch.Code", proxyBranch?.Code) +

					 TableRow("GlbBranch.CurrentBranch.GB_RL_NKHomePort", glbBranch?.GB_RL_NKHomePort,
						 "Env.CurrentBranch.NKUNLOCO", proxyBranch?.NKUNLOCO) +

					 TableRow("GlbDepartment.CurrentDepartment.GE_Code", glbDept?.GE_Code,
						 "Env.CurrentDepartment.Code", proxyDept?.Code) +

					 TableRow("GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency", glbCompany?.GC_RX_NKLocalCurrency,
						 "GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency",
						 glbCompany?.Country?.RN_RX_NKLocalCurrency) +

					 "</table>";

			result += GetTestSequenceFileLink();

			return result;
		}

		protected string TableRow(string name, string value, string name2, string value2)
		{
			var error = value != null && value2 != null && value != value2;
			return
				"<tr>" +
				TableCell(" ") +
				TableCellError(error) +
				TableCell(name) +
				TableCell(" ") +
				TableCell(value ?? "") +
				TableCell("       |       ") +
				TableCellError(error) +
				TableCell(name2) +
				TableCell(" ") +
				TableCell(value2 ?? "") +
				"</tr>";
		}

		protected string TableCell(string value)
		{
			return
				"<td>" +
				Html(value) +
				"</td>";
		}

		protected string TableCellError(bool showError)
		{
			return !showError ? TableCell("") :
								"<td style=\"color:red\">" +
								"***" +
								"</td>";
		}

		string GetTestSequenceFileLink()
		{
			if (TestingState.IsRunningOnDAT)
			{
				try
				{
					return "<br/><br/><a href=\"" + testFailureDataClient.Upload(new MemoryStream(Encoding.UTF8.GetBytes(GetTestSequenceFile())), "text/plain", ".txt").Result + "\">Test Sequence File</a>";
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var pLabel = new StringBuilder(@"<br><br>
<font style=""font-size:8pt""><strong>Test Sequence Information</strong></font><br><br>
<div style=""font-size:7pt; width: 2000px; height: 400px; overflow: auto;"">");
					var testList = GetTestSequenceFile().SplitByLine();
					pLabel.Append(string.Join("", testList.Select(test => $"<p>{test}</p>")));

					return pLabel.Append("</div>").ToString();
				}
			}
			else
			{
				return string.Empty;
			}
		}

		protected virtual string GetTestSequenceFile()
		{
			return TestingState.GetTestSequenceFile();
		}
	}
}

#endif
