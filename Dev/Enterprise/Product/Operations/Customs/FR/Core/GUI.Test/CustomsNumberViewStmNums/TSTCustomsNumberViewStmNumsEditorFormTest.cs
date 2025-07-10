using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusAuthorizationHeaderTypeList = Enterprise.Customs.FR.Business.CusAuthorizationHeaderTypeList;

namespace Enterprise.Customs.FR.GUI.Testing
{
	[TestedType(typeof(TSTCustomsNumberViewStmNumsEditorForm))]
	public class TSTCustomsNumberViewStmNumsEditorFormTest : ZFormBasherTest
	{
		public void TestPrefix()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var prefixEdit = form.FindSingle<ZTextBox>("FountainNameTextBox");
				AssertEquals(true, prefixEdit.Visible);
				AssertEquals("Prefix", ((IExtendedControl)prefixEdit).Extensions.Get<ILabelCaptionRenderer>().Caption);
				AssertEquals(10, prefixEdit.MaxLength);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var authorisation = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisation.CPH_OH_PermitHolder = Factory.NewWithValidTestData<OrgHeader>().PK;
			authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;

			var provider = (TSTCustomsNumberViewStmNumsAuthorisationProvider)authorisation.CustomsNumberProvider;

			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = provider;
			var wrapper = new TSTCustomsNumberViewStmNumsWrapper(stmNums);
			return new TSTCustomsNumberViewStmNumsEditorForm(wrapper);
		}
	}
}
