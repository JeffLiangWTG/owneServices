using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Test.Other
{
	public class RequestDateFormHelperTest : TestCaseWithFactory
	{
		public void TestGetDateFromRequestDateForm()
		{
			var formTitle = ResString.GetMultilingualString("Test1", "Request Date Form");
			var dateCaption = NoResourceStringData.GetData("Date");

			using (var form = new ZForm())
			{
				ZFormModaliser.ShowDialogsInTest = false;
				CombineAssertions(() =>
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					var response = RequestDateFormHelper.GetDateFromRequestDateForm(formTitle, "Test Text", ZDateTime.BrettsBirthday, dateCaption);
					AssertEquals("GetDateFromRequestDateForm returns empty when cancel", ZDateTime.Empty, response);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					response = RequestDateFormHelper.GetDateFromRequestDateForm(formTitle, "Test Text", new ZDate(2022, 01, 01), dateCaption);
					AssertEquals("GetDateFromRequestDateForm returns introduced value when Ok", new ZDate(2022, 01, 01), response);
				});
			}
		}
	}
}
