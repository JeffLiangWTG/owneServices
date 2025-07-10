using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ForeignOperatorUserControlTest : TestCaseWithFactory
	{
		public void TestComponents()
		{
			using (var control = new ForeignOperatorUserControl())
			{
				CombineAssertions(() =>
				{
					AssertType<ZGroupBox>("ForeignOperatorGroupBox should be a ZGroupBox", control.ForeignOperatorGroupBox);
					AssertType<ZGuidFindBox>("OwnerGuidFindBox should be a ZGuidFindBox", control.OwnerGuidFindBox);
					AssertType<ZDropEdit>("MessageStatusDropEdit should be a ZDropEdit", control.MessageStatusDropEdit);
					AssertType<ZDropEdit>("CustomsStatusDropEdit should be a ZDropEdit", control.CustomsStatusDropEdit);
					AssertType<ZGuidFindBox>("ForeignOperatorGuidFindBox should be a ZGuidFindBox", control.ForeignOperatorGuidFindBox);
					AssertType<ZGroupBox>("ForeignOperatorDetailsGroupBox should be a ZGroupBox", control.ForeignOperatorDetailsGroupBox);
					AssertType<ZTextBox>("ForeignOperatorNameTextBox should be a ZTextBox", control.ForeignOperatorNameTextBox);
					AssertType<ZTextBox>("ForeignOperatorCountryTextBox should be a ZTextBox", control.ForeignOperatorCountryTextBox);
					AssertType<ZTextBox>("AuthorityIdentifierTextBox should be a ZTextBox", control.AuthorityIdentifierTextBox);
					AssertType<ZTextBox>("AuthorityVersionTextBox should be a ZTextBox", control.AuthorityVersionTextBox);
					AssertType<ZTextBox>("TinTextBox should be a ZTextBox", control.TinTextBox);
					AssertType<ZTextBox>("InternalCodeTextBox should be a ZTextBox", control.InternalCodeTextBox);
					AssertType<ZTextBox>("EmailTextBox should be a ZTextBox", control.EmailTextBox);
				});
			}
		}
	}
}
