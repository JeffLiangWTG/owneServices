using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class AdditionalArrivalNotificationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var userControl = new AdditionalArrivalNotificationDetailsUserControl())
			{
				AssertControl(userControl.SimplifiedProcedureCheckBox, "ArrivalMovementHeader.IsSimplifiedNctsProcedure");
				AssertControl(userControl.AutomaticCompletionCheckBox, "ESNctsHeader.CEN_AutomaticCompletion");
				AssertControl(userControl.AutomaticTranshipmentCheckBox, "ESNctsHeader.CEN_AutomaticTranshipment");
				AssertControl(userControl.TIRArrivalCheckBox, "ESNctsHeader.CEN_TIRArrival");
				AssertControl(userControl.TIRPartialUnloadingCheckBox, "ESNctsHeader.CEN_TIRPartialUnloading");
				AssertControl(userControl.TIRCarnetPageIntEdit, "ESNctsHeader.CEN_TIRCarnetPage");

				void AssertControl<T>(T control, string bindTo) where T : Control, IBindTo
				{
					AssertEquals($"{control.Name}.BindTo", bindTo, control.BindTo);
				}
			}
		}

		public void TestTIRArrivalDetailsVisibility()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			using (var form = new Phase5ArrivalMovementForm(nctsHeader))
			{
				form.Show();
				var userControl = form.FindSingle<AdditionalArrivalNotificationDetailsUserControl>();

				CombineAssertions(() =>
				{
					nctsHeader.ESNctsHeader.CEN_TIRArrival = ZBool.False;
					AssertEquals($"{userControl.TIRPartialUnloadingCheckBox.Name}.Visible: CEN_TIRArrival={nctsHeader.ESNctsHeader.CEN_TIRArrival}", false, userControl.TIRPartialUnloadingCheckBox.Visible);
					AssertEquals($"{userControl.TIRCarnetPageIntEdit.Name}.Visible: CEN_TIRArrival={nctsHeader.ESNctsHeader.CEN_TIRArrival}", false, userControl.TIRCarnetPageIntEdit.Visible);

					nctsHeader.ESNctsHeader.CEN_TIRArrival = ZBool.True;
					AssertEquals($"{userControl.TIRPartialUnloadingCheckBox.Name}.Visible: CEN_TIRArrival={nctsHeader.ESNctsHeader.CEN_TIRArrival}", true, userControl.TIRPartialUnloadingCheckBox.Visible);
					AssertEquals($"{userControl.TIRCarnetPageIntEdit.Name}.Visible: CEN_TIRArrival={nctsHeader.ESNctsHeader.CEN_TIRArrival}", true, userControl.TIRCarnetPageIntEdit.Visible);
				});
			}
		}
	}
}
