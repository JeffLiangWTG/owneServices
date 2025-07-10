using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing;

[TestedType(typeof(Phase5ArrivalDeclarationDetailsLayout))]
sealed class Phase5ArrivalDeclarationDetailsLayoutTest : LayoutsAbstractTest
{
	[RequiresSTA]
	public void TestAcceptanceDateEdit()
	{
		CombineAssertions(() =>
		{
			using var form = new ZForm(Header);
			using var control = new Phase5ArrivalNotificationTabUserControl();
			form.Controls.Add(control);
			form.Show();

			var acceptanceDateEdit = control.FindSingle<ZDateEdit>(x => x.Name == EU.NCTS.GUI.ArrivalDeclarationDetailsControlBag.Instance.AcceptanceDateEdit.ControlName);
			AssertEquals("BindTo", nameof(NctsHeader.AcceptanceDate), acceptanceDateEdit.BindTo);
		});
	}

	[RequiresSTA]
	public void TestReleaseDateEdit()
	{
		using var form = new ZForm(Header);
		using var control = new Phase5ArrivalNotificationTabUserControl();
		form.Controls.Add(control);
		form.Show();

		var releaseDateEdit = control.FindSingle<ZDateEdit>(x => x.Name == EU.NCTS.GUI.ArrivalDeclarationDetailsControlBag.Instance.ReleaseDateEdit.ControlName);
		AssertEquals("BindTo", nameof(NctsHeader.ReleaseDate), releaseDateEdit.BindTo);
	}

	[RequiresSTA]
	public void TestReleaseDateEditCaption()
	{
		Header.ArrivalMovementHeader.BM_CustomsStatus = "CL1";
		Header.ArrivalMovementHeader.BM_Phase = "007";
		using var form = new Phase5ArrivalMovementForm(Header);
		form.Show();

		NavigateToUnloadingAndBackToArrivalNotifications();
		AssertReleaseDateEditCaptionIsNotEmpty();

		void NavigateToUnloadingAndBackToArrivalNotifications()
		{
			var unloadingRemarks = form.FindSingle<ZTabPage>(x => x.Name == "UnloadingRemarksTabPage");
			_ = unloadingRemarks.Focus();

			var unloadingDifferences = form.FindSingle<ZGroupBox>(x => x.Name == "UnloadingDifferencesGroupBox");
			_ = unloadingDifferences.Focus();

			var mainTab = form.FindSingle<ZTabPage>(x => x.Name == "MainTabPage");
			_ = mainTab.Focus();
		}

		void AssertReleaseDateEditCaptionIsNotEmpty()
		{
			var releaseDateEdit = form.FindSingle<ZDateEdit>(x => x.Name == EU.NCTS.GUI.ArrivalDeclarationDetailsControlBag.Instance.ReleaseDateEdit.ControlName);
			var captions = ZLabelCaptionCache.Instance.GetCaptions(releaseDateEdit, true);
			Assert("ReleaseDateEdit Caption is not empty", captions != null && captions.Any(x => !string.IsNullOrEmpty(x)));
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.ArrivalDeclarationDetailsLayoutBuilder<NctsHeader>();

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.ArrivalDeclarationDetailsControlBag.Instance.StatusDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.ArrivalDeclarationDetailsControlBag.Instance.PhaseDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.ArrivalDeclarationDetailsControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Auto);
			yield return (ArrivalDeclarationDetailsControlBag.Instance.CircuitTextBox, ControlWidthClass.Auto);
			yield return (ArrivalDeclarationDetailsControlBag.Instance.ArrivalSummaryDeclarationUserControl, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.ArrivalDeclarationDetailsControlBag.Instance.SeparatorLabel, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.ArrivalDeclarationDetailsControlBag.Instance.AcceptanceDateEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.ArrivalDeclarationDetailsControlBag.Instance.ReleaseDateEdit, ControlWidthClass.Auto);
		}
	}

	NctsHeader Header => header ??= CreateNewHeader();
	NctsHeader header;

	NctsHeader CreateNewHeader()
	{
		var header = Factory.NewWithValidTestData<NctsHeader>();
		header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		return header;
	}
}
