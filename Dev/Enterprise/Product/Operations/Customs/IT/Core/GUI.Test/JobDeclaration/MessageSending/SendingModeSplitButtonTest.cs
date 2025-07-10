using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class SendingModeSplitButtonTest : ZButtonTest
{
	[ExpectNoExceptions]
	public void TestEventHandlersForMultipleActionsButtonStyle()
	{
		using (var form = new ZForm())
		{
			var iCanBeHitSendingModeSplitButtonMock = new Mock<ICanBeHitSendingModeSplitButton>();
			var splitButton = GetSendingModeSplitButton(iCanBeHitSendingModeSplitButtonMock.Object);
			form.Controls.Add(splitButton);
			form.Show();

			var toolStripItems = splitButton.ContextMenuStrip.Items.Cast<ToolStripItem>().ToArray();
			splitButton.SetButtonStyle(SendingModeSplitButton.ButtonStyle.MultipleActions);
			CombineAssertions(() =>
			{
				iCanBeHitSendingModeSplitButtonMock.Setup(m => m.HitAutomatic());
				splitButton.PerformClick();
				iCanBeHitSendingModeSplitButtonMock.VerifyAll();
				iCanBeHitSendingModeSplitButtonMock.Reset();

				iCanBeHitSendingModeSplitButtonMock.Setup(m => m.HitManual());
				toolStripItems[0].PerformClick();
				iCanBeHitSendingModeSplitButtonMock.VerifyAll();
				iCanBeHitSendingModeSplitButtonMock.Reset();

				iCanBeHitSendingModeSplitButtonMock.Setup(m => m.HitFallback());
				toolStripItems[1].PerformClick();
				iCanBeHitSendingModeSplitButtonMock.VerifyAll();
				iCanBeHitSendingModeSplitButtonMock.Reset();
			});
			iCanBeHitSendingModeSplitButtonMock.VerifyAll();
		}
	}

	[ExpectNoExceptions]
	public void TestEventHandlersForFallbackActionButtonStyle()
	{
		using (var form = new ZForm())
		{
			var iCanBeHitSendingModeSplitButtonMock = new Mock<ICanBeHitSendingModeSplitButton>();
			var splitButton = GetSendingModeSplitButton(iCanBeHitSendingModeSplitButtonMock.Object);
			form.Controls.Add(splitButton);
			form.Show();

			splitButton.SetButtonStyle(SendingModeSplitButton.ButtonStyle.FallbackAction);

			iCanBeHitSendingModeSplitButtonMock.Setup(m => m.HitFallback());
			splitButton.PerformClick();
			iCanBeHitSendingModeSplitButtonMock.VerifyAll();
		}
	}

	[ExpectNoExceptions]
	public void TestEventHandlersForAutomaticActionButtonStyle()
	{
		using (var form = new ZForm())
		{
			var iCanBeHitSendingModeSplitButtonMock = new Mock<ICanBeHitSendingModeSplitButton>();
			var splitButton = GetSendingModeSplitButton(iCanBeHitSendingModeSplitButtonMock.Object);
			form.Controls.Add(splitButton);
			form.Show();

			splitButton.SetButtonStyle(SendingModeSplitButton.ButtonStyle.AutomaticAction);

			iCanBeHitSendingModeSplitButtonMock.Setup(m => m.HitAutomatic());
			splitButton.PerformClick();
			iCanBeHitSendingModeSplitButtonMock.VerifyAll();
		}
	}

	public void TestSetMultipleActionsButtonStyle()
	{
		AssertButtonTextBasedOnStyle(SendingModeSplitButton.ButtonStyle.MultipleActions, "&Send");
	}

	public void TestSetFallbackActionButtonStyle()
	{
		AssertButtonTextBasedOnStyle(SendingModeSplitButton.ButtonStyle.FallbackAction, "&Fallback Procedure");
	}

	public void TestSetAutomaticActionButtonStyle()
	{
		AssertButtonTextBasedOnStyle(SendingModeSplitButton.ButtonStyle.AutomaticAction, "&Send");
	}

	SendingModeSplitButton GetSendingModeSplitButton(ICanBeHitSendingModeSplitButton iCanBeHitSendingModeSplitButton)
	{
		var splitButton = new SendingModeSplitButton();
		splitButton.AutomaticSendSelected += (s, e) => iCanBeHitSendingModeSplitButton.HitAutomatic();
		splitButton.ManualSendSelected += (s, e) => iCanBeHitSendingModeSplitButton.HitManual();
		splitButton.FallbackProcedureSelected += (s, e) => iCanBeHitSendingModeSplitButton.HitFallback();
		return splitButton;
	}

	void AssertButtonTextBasedOnStyle(SendingModeSplitButton.ButtonStyle buttonStyle, string expectedText)
	{
		using (var form = new ZForm())
		{
			var splitButton = new SendingModeSplitButton();
			form.Controls.Add(splitButton);
			form.Show();

			splitButton.SetButtonStyle(buttonStyle);
			AssertEquals("Text", expectedText, splitButton.Text);
		}
	}
}

public interface ICanBeHitSendingModeSplitButton
{
	void HitAutomatic();

	void HitManual();

	void HitFallback();
}
