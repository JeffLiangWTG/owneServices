using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing.LPCO
{
	sealed class LPCOUserControlTest : TestCaseWithFactory
	{
		public void TestComponents()
		{
			using (var control = new LPCOUserControl())
			{
				var lPCORetroactiveDateZDateEdit = control.LPCORetroactiveDateZDateEdit;

				AssertType<ZDateTimeOffsetEdit>("lPCORetroactiveDateZDateEdit should be a ZDateTimeOffsetEdit", lPCORetroactiveDateZDateEdit);
				Assert("LPCORetroactiveDateZDateEdit should be visible", lPCORetroactiveDateZDateEdit.Visible);

				var lPCONumberTextBox = control.LPCONumberTextBox;

				AssertType<ZTextBox>("LPCONumberTextBox should be a ZTextBox", lPCONumberTextBox);
				Assert("LPCONumberTextBox should be visible", lPCONumberTextBox.Visible);

				var lPCOJobNumberTextBox = control.LPCOJobNumberTextBox;

				AssertType<ZTextBox>("LPCOJobNumberTextBox should be a ZTextBox", lPCOJobNumberTextBox);
				Assert("LPCOJobNumberTextBox should be visible", lPCOJobNumberTextBox.Visible);

				var lPCOHolderFindBox = control.LPCOHolderFindBox;

				AssertType<ZGuidFindBox>("LPCOHolderFindBox should be a ZGuidFindBox", lPCOHolderFindBox);
				Assert("LPCOHolderFindBox should be visible", lPCOHolderFindBox.Visible);

				var lPCOStartDateZDateEdit = control.LPCOStartDateZDateEdit;

				AssertType<ZDateEdit>("LPCOStartDateZDateEdit should be a ZDate", lPCOStartDateZDateEdit);
				Assert("LPCOStartDateZDateEdit should be visible", lPCOStartDateZDateEdit.Visible);

				var lPCOEndDateZDateEdit = control.LPCOEndDateZDateEdit;

				AssertType<ZDateEdit>("LPCOEndDateZDateEdit should be a ZDate", lPCOEndDateZDateEdit);
				Assert("LPCOEndDateZDateEdit should be visible", lPCOEndDateZDateEdit.Visible);
			}
		}
	}
}
