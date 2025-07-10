using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class DeclarationDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(NctsDepartureMovementHeader), control.BindingSource.DataSourceType);
	}

	public void TestReleaseCodeTextBox()
	{
		var releaseCodeTextBox = control.ReleaseCodeTextBox;
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>(releaseCodeTextBox);
			AssertEquals("BindTo", $"{nameof(NctsDepartureMovementHeader.Header)}.{nameof(NctsHeader.ReleaseCode)}", releaseCodeTextBox.BindTo);
		});
	}

	public void TestReleaseDateEdit()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Binding", $"{nameof(NctsDepartureMovementHeader.Header)}.{nameof(NctsHeader.CustomsReleaseIssueDate)}", control.ReleaseDateEdit.GetBindingMember());
			AssertType<ZDateEdit>("Type", control.ReleaseDateEdit);
		});
	}

	public void TestWriteOffDateEdit()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Binding", $"{nameof(NctsDepartureMovementHeader.Header)}.{nameof(NctsHeader.CustomsWriteOffDate)}", control.WriteOffDateEdit.GetBindingMember());
			AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Short, control.WriteOffDateEdit.DateTimeFormat);
			AssertType<ZDateEdit>("Type", control.WriteOffDateEdit);
		});
	}

	public void TestControlChannelDropEdit()
	{
		var controlChannelDropEdit = control.ControlChannelDropEdit;

		CombineAssertions(() =>
		{
			controlChannelDropEdit.AssertThisControl(c => c
				.WithReadOnly()
				.WithBindTo(nameof(NctsDepartureMovementHeader.BM_ControlChannel))
				.WithCaption("Control Channel"));

			AssertType<ZDropEdit>("Type", controlChannelDropEdit);
		});
	}

	protected override void SetUp()
	{
		control = new DeclarationDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	DeclarationDetailsUserControl control;
}
