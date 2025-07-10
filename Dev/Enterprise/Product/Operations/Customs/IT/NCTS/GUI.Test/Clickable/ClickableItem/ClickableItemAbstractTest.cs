using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

abstract class ClickableItemAbstractTest : TestCaseWithFactory
{
	public void TestClickableContext() => AssertNotNull(GetClickableItem().ClickableContext);

	public void TestIsFormPreSaved_Argument() => AssertArgumentExceptionThrown<ArgumentNullException>("topLevelBizObjProvider", () => GetClickableItem().IsFormPreSaved(null));

	public void TestIsFormPreSaved_FormIsSaved()
	{
		var clickableItem = GetClickableItem();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
		Assert("Form should be saved", clickableItem.IsFormPreSaved(GetTopLevelBusinessObjectProvider()));
	}

	public void TestIsFormPreSaved_FormNotSaved()
	{
		var clickableItem = GetClickableItem();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

		Assert("Form should not be saved", !clickableItem.IsFormPreSaved(GetTopLevelBusinessObjectProvider()));
		AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	protected abstract IClickableItem GetClickableItem();
	protected abstract ITopLevelBusinessObjectProvider GetTopLevelBusinessObjectProvider();
}
