using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Test
{
	[TestedType(typeof(AIRSLPCOSelection))]
	sealed class AIRSLPCOSelectionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBuildAL_LPCOAndRegistrationForShowInfo()
		{
			var lpcoSelection = new AIRSLPCOSelection(Factory, true);

			lpcoSelection.BuildWebContent();

			AssertEquals(AIRSHtmlHelper.WriteMessageErrorHtml(AIRSHtmlHelper.Constant.NoDataSetMessage), lpcoSelection.AL_LPCOAndRegistrationForShow);

			lpcoSelection.MaterializedLPCOs.AddPair("1", "1 Desc");
			lpcoSelection.MaterializedLPCOs.AddPair("2", "2 Desc");

			lpcoSelection.DeMaterializedLPCOs.AddPair("3", "3 Desc");
			lpcoSelection.DeMaterializedLPCOs.AddPair("4", "4 Desc");

			lpcoSelection.AIRSRegistrations.AddPair("5", "5 Desc");

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
		<tr>
	<th colspan=""2"">OR</th>
</tr><tr>
	<th></th><th></th>
</tr><tr>
	<th colspan=""2""></th>
</tr><tr>
	<td>1</td><td>1 Desc</td>
</tr><tr>
	<td>2</td><td>2 Desc</td>
</tr><tr>
	<th colspan=""2""></th>
</tr><tr>
	<td>3</td><td>3 Desc</td>
</tr><tr>
	<td>4</td><td>4 Desc</td>
</tr><tr>
	<th colspan=""2""></th>
</tr><tr>
	<td>5</td><td>5 Desc</td>
</tr>
	</table>
</body>
</html>
";

			lpcoSelection.BuildWebContent();
			AssertMultilineASCIIEquals(expectedHtml, lpcoSelection.AL_LPCOAndRegistrationForShow);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AIRSLPCOSelection(Factory, true);
		}
	}
}
