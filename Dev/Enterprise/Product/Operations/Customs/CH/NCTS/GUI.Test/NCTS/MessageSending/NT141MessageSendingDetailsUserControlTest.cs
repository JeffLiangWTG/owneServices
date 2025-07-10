using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(NT141MessageSendingDetailsUserControl))]
sealed class NT141MessageSendingDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new NT141MessageSendingDetailsUserControl())
		{
			AssertEquals(typeof(NctsHeaderDepartureMessageSendingObjectParent), control.BindingSource.DataSourceType);
		}
	}

	public void TestDoubleEntryMRN()
	{
		using (var control = new NT141MessageSendingDetailsUserControl())
		{
			var doubleMRNTextBox = control.DoubleEntryMRNTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Control Type", doubleMRNTextBox);
				AssertEquals("BindTo", "SendingObjectsCollection.DoubleEntryMRN", doubleMRNTextBox.BindTo);
			});
		}
	}

	public void TestActualDestinationCustomsOffice()
	{
		using (var control = new NT141MessageSendingDetailsUserControl())
		{
			var actualDestinationCustomsOfficeFindBox = control.ActualDestinationCustomsOfficeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Control Type", actualDestinationCustomsOfficeFindBox);
				AssertEquals("BindTo", "SendingObjectsCollection.ActualDestinationCustomsOffice", actualDestinationCustomsOfficeFindBox.BindTo);
			});
		}
	}

	public void TestTC11DeliveryDate()
	{
		using (var control = new NT141MessageSendingDetailsUserControl())
		{
			var tcI11DateEdit = control.TC11DeliveryDateEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>("Control Type", tcI11DateEdit);
				AssertEquals("BindTo", "SendingObjectsCollection.TC11DeliveryDate", tcI11DateEdit.BindTo);
			});
		}
	}

	public void TestActualConsignee()
	{
		using (var control = new NT141MessageSendingDetailsUserControl())
		{
			var actualConsigneeAddressControl = control.ActualConsigneeAddressControl;
			CombineAssertions(() =>
			{
				AssertType<ZDocAddressControl>("Control Type", actualConsigneeAddressControl);
				AssertEquals("BindTo", "SendingObjectsCollection.ActualConsignee", actualConsigneeAddressControl.BindTo);
			});
		}
	}
}
