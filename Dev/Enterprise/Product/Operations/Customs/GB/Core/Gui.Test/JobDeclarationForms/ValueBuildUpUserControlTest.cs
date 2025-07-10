using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	public class ValueBuildUpUserControlTest : TestCaseWithFactory
	{
		public void TestFARPCodeFindBox()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, "EU IATA");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, "KD1", "KD1 DESC", ZDate.Today.AddYears(-1), ZDate.Today.AddYears(1));

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (var form = new ZForm())
			using (var control = new ValueBuildUpUserControl())
			{
				var dec = Factory.NewWithValidTestData<JobDeclaration>();
				dec.JE_ApplicationCode = "CHF";
				form.SetDataBinding(dec, "");
				form.Controls.Add(control);
				form.Show();

				var findBox = (ZCodeFindBox)control.Controls.Find("FARPCodeFindBox", true).First();

				findBox.PopupButton.PerformClick();
				AssertEquals(Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList, findBox.ModuleID);
			}
		}
	}
}
