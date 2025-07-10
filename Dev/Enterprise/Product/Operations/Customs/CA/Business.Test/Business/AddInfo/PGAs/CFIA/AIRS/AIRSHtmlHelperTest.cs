using System;
using System.IO;
using System.Net;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	public sealed class AIRSHtmlHelperTest : TestCaseWithFactory
	{
		public void TestGetHtmlFormattedText()
		{
			var expectedHtml = @"<html>
<head>
	<style type='text/css'>
		th {
			font-family: Verdana, Arial, Helvetica, sans-serif;
			font-size: 12px;
			border: 1px solid gray;
			border-collapse: collapse;
			background-color: lightgray;
		}
		td {
			font-family: Verdana, Arial, Helvetica, sans-serif;
			font-size: 11px;
			border: 1px solid gray;
			border-collapse: collapse;
			background-color: white;
		}
	</style>
</head>
<body>
	<table border='0' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF' width='100%' height='100%'>
		<tr><td>Test Html Content</td></td>
	</table>
</body>
</html>";
			AssertEquals(expectedHtml, AIRSHtmlHelper.GetHtmlFormattedText(@"<tr><td>Test Html Content</td></td>"));
		}

		public void TestWriteMessageErrorHtml()
		{
			AssertEquals("<html>Test Error Message</Html>", AIRSHtmlHelper.WriteMessageErrorHtml("Test Error Message"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetPostData()
		{
			var bo = new AIRSWebpageNavigator(Factory, "803010");
			var postDataBuilder = new ZStringBuilder();
			postDataBuilder.Append("__VIEWSTATE=DUMMYVIEWSTATE&");
			postDataBuilder.Append("__VIEWSTATEGENERATOR=20337DB4&");
			postDataBuilder.Append("__VIEWSTATEENCRYPTED=&");
			postDataBuilder.Append("__EVENTVALIDATION=EVENTVALIDATION&");
			postDataBuilder.Append("ctl00%24ContentMain%24ctlSearch%24chkHSDesc=on&");
			postDataBuilder.Append("ctl00%24ContentMain%24ctlSearch%24chkHSCode=on&");
			postDataBuilder.Append("ctl00%24ContentMain%24ctlSearch%24chkAltDesc=on&");
			postDataBuilder.Append("ctl00%24ContentMain%24ctlSearch%24chkKeyWorks=on&");
			postDataBuilder.Append("ctl00%24ContentMain%24ctlSearch%24chkLatin=on&");
			postDataBuilder.Append("ctl00%24ContentMain%24ctlSearch%24txtFind=803010&");
			postDataBuilder.Append("ctl00%24ContentMain%24ctlSearch%24btnSearch=Search&");
			postDataBuilder.Append("ctl00%24ContentMain%24ctlCommSelect%24hidCurrentStep=1");
			AssertEquals(Encoding.UTF8.GetBytes(postDataBuilder.ToString()), AIRSHtmlHelper.GetPostData(bo.WebPageConfiguration, "803010", GetInitialFileRequest));
		}

		static ZString GetInitialFileRequest(ZString url)
		{
#pragma warning disable SYSLIB0014 // WebRequest.Create(Uri) is obsolete: WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.
			var request = WebRequest.Create(new Uri(url)) as FileWebRequest;
#pragma warning restore SYSLIB0014
			request.Method = "GET";

			var firstResponse = string.Empty;
			using (var response = request.GetResponse())
			{
				using (var responseStream = response.GetResponseStream())
				{
					using (var reader = new StreamReader(responseStream, Encoding.UTF8))
					{
						firstResponse = reader.ReadToEnd();
					}
				}
			}
			return firstResponse;
		}

		public void TestBuildHtmlFromDataSet()
		{
			var materializedLPCOs = new CodeDescriptionPairList();
			var deMaterializedLPCOs = new CodeDescriptionPairList();
			var aIRSRegistrations = new CodeDescriptionPairList();

			var result = AIRSHtmlHelper.BuildHtmlFromDataSet(Factory, materializedLPCOs, deMaterializedLPCOs, aIRSRegistrations, true);
			AssertEquals(AIRSHtmlHelper.WriteMessageErrorHtml(AIRSHtmlHelper.Constant.NoDataSetMessage), result);

			materializedLPCOs.AddPair("1", "1 Desc");
			materializedLPCOs.AddPair("2", "2 Desc");

			deMaterializedLPCOs.AddPair("3", "3 Desc");
			deMaterializedLPCOs.AddPair("4", "4 Desc");

			aIRSRegistrations.AddPair("5", "5 Desc");

			var parentHtml = @"<html>
<head>
	<style type='text/css'>
		th {
			font-family: Verdana, Arial, Helvetica, sans-serif;
			font-size: 12px;
			border: 1px solid gray;
			border-collapse: collapse;
			background-color: lightgray;
		}
		td {
			font-family: Verdana, Arial, Helvetica, sans-serif;
			font-size: 11px;
			border: 1px solid gray;
			border-collapse: collapse;
			background-color: white;
		}
	</style>
</head>
<body>
	<table border='0' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF' width='100%' height='100%'>
		<tr>
	<th>CODE</th><th>REGISTRATION</th>
</tr><tr>
	<th colspan=""2"">Materialized LPCO (Image Required)</th>
</tr><tr>
	<td>1</td><td>1 Desc</td>
</tr><tr>
	<td>2</td><td>2 Desc</td>
</tr><tr>
	<th colspan=""2"">Dematerialized LPCO (Image Not Required)</th>
</tr><tr>
	<td>3</td><td>3 Desc</td>
</tr><tr>
	<td>4</td><td>4 Desc</td>
</tr><tr>
	<th colspan=""2"">AIRS Registration</th>
</tr><tr>
	<td>5</td><td>5 Desc</td>
</tr>
	</table>
</body>
</html>
";

			result = AIRSHtmlHelper.BuildHtmlFromDataSet(Factory, materializedLPCOs, deMaterializedLPCOs, aIRSRegistrations, false);
			AssertMultilineASCIIEquals("Parent Web Content", parentHtml, result);

			var childHtm = @"<html>
<head>
	<style type='text/css'>
		th {
			font-family: Verdana, Arial, Helvetica, sans-serif;
			font-size: 12px;
			border: 1px solid gray;
			border-collapse: collapse;
			background-color: lightgray;
		}
		td {
			font-family: Verdana, Arial, Helvetica, sans-serif;
			font-size: 11px;
			border: 1px solid gray;
			border-collapse: collapse;
			background-color: white;
		}
	</style>
</head>
<body>
	<table border='0' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF' width='100%' height='100%'>
		<tr>
	<th colspan=""2"">OR</th>
</tr><tr>
	<th>CODE</th><th>REGISTRATION</th>
</tr><tr>
	<th colspan=""2"">Materialized LPCO (Image Required)</th>
</tr><tr>
	<td>1</td><td>1 Desc</td>
</tr><tr>
	<td>2</td><td>2 Desc</td>
</tr><tr>
	<th colspan=""2"">Dematerialized LPCO (Image Not Required)</th>
</tr><tr>
	<td>3</td><td>3 Desc</td>
</tr><tr>
	<td>4</td><td>4 Desc</td>
</tr><tr>
	<th colspan=""2"">AIRS Registration</th>
</tr><tr>
	<td>5</td><td>5 Desc</td>
</tr>
	</table>
</body>
</html>
";
			result = AIRSHtmlHelper.BuildHtmlFromDataSet(Factory, materializedLPCOs, deMaterializedLPCOs, aIRSRegistrations, true);
			AssertMultilineASCIIEquals("Child Web Content", childHtm, result);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Baseline")]
		protected override void SetUp()
		{
			base.SetUp();
			var dummyUrl = TestPath + AIRSTestWebPageDummy;
			SetupRefSysConfigType(Factory, dummyUrl, dummyUrl);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		public static string TestPath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\Business\AddInfo\PGAs\CFIA\AIRS\TestFiles\";
		public static string AIRSTestWebPageEng = "AIRSTestWebPageEng.html";
		public static string AIRSTestWebPageFra = "AIRSTestWebPageFra.html";
		public static string AIRSTestWebPageDummy = "AIRSTestWebPageDummy.html";

		public static void SetupRefSysConfigType(BusinessObjectFactory factory, ZString urlFra, ZString urleng)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateRefSysConfigType("CAAIRSUriF", "CAAIRSUriF Desc", "CAAIRSUriF Long Desc");
			helper.CreateRefSysConfigType("CAAIRSUriE", "CAAIRSUriE Desc", "CAAIRSUriE Long Desc");
			helper.CreateRefSysConfigType("CAAIRSEUID", "CAAIRSEUID Desc", "CAAIRSEUID Long Desc");
			helper.CreateRefSysConfigType("CAAIRSETID", "CAAIRSETID Desc", "CAAIRSETID Long Desc");
			helper.CreateRefSysConfigType("CAAIRSMSID", "CAAIRSMSID Desc", "CAAIRSMSID Long Desc");
			helper.CreateRefSysConfigType("CAAIRSITID", "CAAIRSITID Desc", "CAAIRSITID Long Desc");
			helper.CreateRefSysConfigType("CAAIRSPDTA", "CAAIRSPDTA Desc", "CAAIRSPDTA Long Desc");
			helper.CreateRefSysConfigType("CAAIRSIWTF", "CAAIRSIWTF Desc", "CAAIRSIWTF Long Desc");
			helper.CreateRefSysConfigType("CAAIRSIWTE", "CAAIRSIWTE Desc", "CAAIRSIWTE Long Desc");
			helper.CreateRefSysConfigType("CAAIRSMGTF", "CAAIRSMGTF Desc", "CAAIRSMGTF Long Desc");
			helper.CreateRefSysConfigType("CAAIRSMGTE", "CAAIRSMGTE Desc", "CAAIRSMGTE Long Desc");
			helper.CreateRefSysConfigType("CAAIRSREGE", "CAAIRSREGE Desc", "CAAIRSREGE Long Desc");
			helper.CreateRefSysConfigType("CAAIRSREGF", "CAAIRSREGF Desc", "CAAIRSREGF Long Desc");
			helper.CreateRefSysConfigType("CAAIRSDMTF", "CAAIRSDMTF Desc", "CAAIRSDMTF Long Desc");
			helper.CreateRefSysConfigType("CAAIRSDMTE", "CAAIRSDMTE Desc", "CAAIRSDMTE Long Desc");
			helper.CreateRefSysConfigType("CAAIRSCDTF", "CAAIRSCDTF Desc", "CAAIRSCDTF Long Desc");
			helper.CreateRefSysConfigType("CAAIRSCDTE", "CAAIRSCDTE Desc", "CAAIRSCDTE Long Desc");
			helper.CreateRefSysConfigType("CAAIRSRTTF", "CAAIRSRTTF Desc", "CAAIRSRTTF Long Desc");
			helper.CreateRefSysConfigType("CAAIRSRTTE", "CAAIRSRTTE Desc", "CAAIRSRTTE Long Desc");
			helper.CreateRefSysConfigType("CAAIRSORTF", "CAAIRSORTF Desc", "CAAIRSORTF Long Desc");
			helper.CreateRefSysConfigType("CAAIRSORTE", "CAAIRSORTE Desc", "CAAIRSORTE Long Desc");
			helper.CreateRefSysConfigType("CAAIRSLAKY", "CAAIRSLAKY Desc", "CAAIRSLAKY Long Desc");
			helper.CreateRefSysConfigType("CAAIRSLACD", "CAAIRSLACD Desc", "CAAIRSLACD Long Desc");
			helper.CreateRefSysConfigType("CAAIRSLADS", "CAAIRSLADS Desc", "CAAIRSLADS Long Desc");
			helper.CreateRefSysConfig("CAAIRSUriF", urlFra, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSUriE", urleng, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSEUID", "ctl00_ContentMain_lblEndUseText", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSETID", "ctl00_ContentMain_lblOGDExtensionIDText", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSMSID", "ctl00_ContentMain_lblMiscText", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSITID", "ctl00_ContentMain_pnlIIDTable", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSPDTA", "ctl00%24ContentMain%24ctlSearch%24chkHSDesc=on&ctl00%24ContentMain%24ctlSearch%24chkHSCode=on&ctl00%24ContentMain%24ctlSearch%24chkAltDesc=on&ctl00%24ContentMain%24ctlSearch%24chkKeyWorks=on&ctl00%24ContentMain%24ctlSearch%24chkLatin=on&ctl00%24ContentMain%24ctlSearch%24txtFind=TariffToSearch&ctl00%24ContentMain%24ctlSearch%24btnSearch=Search&ctl00%24ContentMain%24ctlCommSelect%24hidCurrentStep=1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSIWTF", "Système automatisé de référence à l'importation: Déclaration intégrée des importations", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSIWTE", "Automated Import Reference System: Integrated Import Declaration", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSMGTF", "LPCA matérialisés (Image requise)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSMGTE", "Materialized LPCO (Image Required)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSREGF", "Enregistrement SARI", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSREGE", "AIRS Registration", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSDMTF", "LPCA dématérialisés (Image non requise)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSDMTE", "Dematerialized LPCO (Image Not Required)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSCDTF", "CODE", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSCDTE", "CODE", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSRTTF", "ENREGISTREMENT", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSRTTE", "REGISTRATION", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSORTF", "OU", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSORTE", "OR", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSLAKY", "headers", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSLACD", "col_1_code", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateRefSysConfig("CAAIRSLADS", "col_2_reg", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			factory.Save();
		}
	}
}
