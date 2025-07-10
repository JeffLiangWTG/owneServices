using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(MultipleMrnMessageSendingDetailsUserControl))]
class MultipleMrnMessageSendingDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new MultipleMrnMessageSendingDetailsUserControl())
		{
			AssertEquals(typeof(NctsHeaderArrivalMessageSendingObjectParent), control.BindingSource.DataSourceType);
		}
	}

	public void TestDateOfUnloading()
	{
		using (var control = new MultipleMrnMessageSendingDetailsUserControl())
		{
			var dateOfUnloadingDateEdit = control.DateOfUnloadingDateEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>("Control Type", dateOfUnloadingDateEdit);
				AssertEquals("BindTo", "DateOfUnloading", dateOfUnloadingDateEdit.BindTo);
			});
		}
	}

	public void TestUnloadedCargoConformsToDeclarationCheckBox()
	{
		using (var control = new MultipleMrnMessageSendingDetailsUserControl())
		{
			var isConformedCheckBox = control.UnloadedCargoConformsToDeclarationCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Control Type", isConformedCheckBox);
				AssertEquals("BindTo", "IsConformed", isConformedCheckBox.BindTo);
			});
		}
	}
}
