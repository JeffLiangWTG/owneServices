using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(MessagesUserControl))]
	sealed class MessagesUserControlTest : TestCaseWithFactory
	{
		public void TestHasWebBrowser()
		{
			using (var control = new MessagesUserControl())
			{
				var child = control.Controls.Find("MessageInterpretationWebBrowser", true);
				AssertNotNull(child.FirstOrDefault());
			}
		}

		public void TestMessagesGridColumnsDisplay()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			using (var form = new ZForm(manifest))
			using (var control = new MessagesUserControl())
			{
				var messagesGrid = control.Controls.Find("messagesGrid", true).First() as ZGrid;
				Assert("EM_ReceiveTransmit", messagesGrid.ColumnStyles.ToArray().Any(column => (column as ZTextBoxColumnStyleInfo).ColumnName == "EM_ReceiveTransmit"));
				Assert("EM_MessageType", messagesGrid.ColumnStyles.ToArray().Any(column => (column as ZTextBoxColumnStyleInfo).ColumnName == "EM_MessageType"));
				Assert("EM_ApplicationReference", messagesGrid.ColumnStyles.ToArray().Any(column => (column as ZTextBoxColumnStyleInfo).ColumnName == "EM_ApplicationReference"));
				Assert("EM_MessageSubType", messagesGrid.ColumnStyles.ToArray().Any(column => (column as ZTextBoxColumnStyleInfo).ColumnName == "EM_MessageSubType"));
				Assert("EM_ApplicationReference default to off", !messagesGrid.GetColumnStyle("EM_ApplicationReference").IsVisible);
				Assert("EM_MessageSubType default to off", !messagesGrid.GetColumnStyle("EM_MessageSubType").IsVisible);
				Assert("EM_CreateUserFullName default to off", !messagesGrid.GetColumnStyle("EM_CreateUserFullName").IsVisible);
			}
		}

		public void TestMessageTextTextBox()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			using (var form = new ZForm(manifest))
			using (var control = new MessagesUserControl())
			{
				var textBox = control.FindSingle<ZTextBox>("MessageTextTextBox");
				CombineAssertions(() =>
				{
					AssertEquals("HideSelection", false, textBox.HideSelection);
					AssertEquals("EnableFindDialog", true, textBox.EnableFindDialog);
				});
			}
		}

		public void TestMessagesGridExtraColumnInfos()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			using (var form = new ZForm(manifest))
			using (var control = new MessagesUserControlForTest())
			{
				var messagesGrid = control.Controls.Find("messagesGrid", true).First() as ZGrid;
				var provider = new ApplicationGUIProviderForTesting(manifest);
				control.OnProviderIdentifierChangedCore(provider);
				AssertNotNull(messagesGrid.GetColumnStyle("TestColumn"));
				control.OnProviderIdentifierChangedCore(null);
				AssertNotNull(messagesGrid.GetColumnStyle("TestColumn"));
			}
		}
	}

	class MessagesUserControlForTest : MessagesUserControl
	{
		public void OnProviderIdentifierChangedCore(ApplicationGUIProvider provider)
		{
			base.OnProviderIdentifierChanged(provider);
		}
	}
}
