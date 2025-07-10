using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.GUI
{
	internal class PopupEditableZGuidFindBoxTest : TestCaseWithFactory
	{
		public void TestAllowShowEditForm()
		{
			using (PopupEditableZGuidFindBox popupEditableZGuidFindBox = new PopupEditableZGuidFindBox())
			{
				AssertEquals(false, popupEditableZGuidFindBox.InternalAllowShowEditForm);
				popupEditableZGuidFindBox.InternalCode = "TEST";
				AssertEquals(true, popupEditableZGuidFindBox.InternalAllowShowEditForm);
			}
		}
	}
}
