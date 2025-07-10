using Enterprise.Customs.GB.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	class CDSSendsMessagesToCustomsGUITest : TestCase
	{
		public void TestShowEnhancedValidation()
		{
			var wrapperMock = new Mock<IEnhancedValidationEntryWrapper>();
			var sendsMessagesToCustoms = new CDSSendsMessagesToCustomsGUI();

			ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;
			var result = sendsMessagesToCustoms.ShowEnhancedValidation(wrapperMock.Object);

			CombineAssertions(() =>
			{
				AssertType<MessageSending.EnhancedValidationForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("result from OK", expected: true, result);
			});

			ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.Cancel;
			result = sendsMessagesToCustoms.ShowEnhancedValidation(wrapperMock.Object);

			AssertEquals("result from Cancel", expected: false, result);
		}
	}
}
