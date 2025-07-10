using System.Windows.Forms;
using CargoWise.Application;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using Res = Enterprise.DocumentVisualizer.GUI.Res;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class TranslationFeedbackViewTest : TestCase
	{
		public void TestOpenFeedbackForm_SupportedMacro()
		{
			const string unquotedmacro = "Importer";

			var provider = new Mock<ITranslationFeedbackProvider>();

			using (var control = new Control())
			using (var mockCache = Res.GetLanguageInstance(Res.CurrentLanguage).UseMockData())
			using (ObjectFactory.Substitute(provider.Object))
			{
				var hasFeedbackBeenShown = false;

				provider
					.Setup(p => p.Feedback(It.IsAny<Control>(), It.IsAny<ResourceStringData>()))
					.Callback<Control, ResourceStringData>((c, r) => { hasFeedbackBeenShown = true; });

				var resStringKey = unquotedmacro.GetResStringKey();

				mockCache.Put(resStringKey,
					new ResourceStringData(resStringKey, "Importador"));

				TranslationFeedbackView.OpenFeedbackForm(control, unquotedmacro);

				Assert("expected to show feedback", hasFeedbackBeenShown);
			}
		}

		public void TestOpenFeedbackForm_UnsupportedMacro()
		{
			const string unquotedmacro = "<Macro>";

			var provider = new Mock<ITranslationFeedbackProvider>();

			using (var control = new Control())
			using (var mockCache = Res.GetLanguageInstance(Res.CurrentLanguage).UseMockData())
			using (ObjectFactory.Substitute(provider.Object))
			{
				var hasFeedbackBeenShown = false;

				provider
					.Setup(p => p.Feedback(It.IsAny<Control>(), It.IsAny<ResourceStringData>()))
					.Callback<Control, ResourceStringData>((c, r) => { hasFeedbackBeenShown = true; });

				var resStringKey = unquotedmacro.GetResStringKey();

				mockCache.Put(resStringKey,
					new ResourceStringData(resStringKey, "Importador"));

				TranslationFeedbackView.OpenFeedbackForm(control, unquotedmacro);

				AssertEquals("expected to show message", "The following macro cannot be translated: <Macro>", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("expected not to show feedback", !hasFeedbackBeenShown);
			}
		}
	}
}
