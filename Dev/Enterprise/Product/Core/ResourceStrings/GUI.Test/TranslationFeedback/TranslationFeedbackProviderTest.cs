using System;
using System.Drawing.Imaging;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.ResourceStrings.GUI.Testing
{
	sealed class TranslationFeedbackProviderTest : TranslationFeedbackTestCase
	{
		public void TestUnuspportedModule()
		{
			ENG.Put("1", new ResourceStringData("1", "One"));
			TranslationFeedbackProvider instance = new TranslationFeedbackProvider();

			using (var form = new Enterprise.Customs.AU.TestForm())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				instance.Feedback(form.label1, form.label1.Text);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "Translation of the Customs Module is not supported.");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				instance.Feedback(form.label1, form.label2.Text);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "Translation of the Customs Module is not supported.");
			}

			using (var form = new Enterprise.Freight.Forwarding.TestForm())
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				object formCreated = null;
				var formCreatedHandler = new EventHandler((sender, args) => formCreated = sender);
				ZForm.FormCreated += formCreatedHandler;
				try
				{
					instance.Feedback(form.label1, form.label1.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertType(typeof(TranslationFeedbackCreateForm), formCreated);
					((IDisposable)formCreated).Dispose();
				}
				finally
				{
					ZForm.FormCreated -= formCreatedHandler;
				}
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				instance.Feedback(form.label1, form.label2.Text);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "No matching resource strings found.");
			}
		}

		public void TestScreenshot()
		{
			ENG.Put("1", new ResourceStringData("1", "One"));
			TranslationFeedbackProvider instance = new TranslationFeedbackProvider();
			BusinessObjectFactory formFactory;

			using (var expectedImageFile = TempFile.New())
			{
				using (var form = new Enterprise.Freight.Forwarding.TestForm())
				{
					form.Show();
					ZScreenShotGrabber.CaptureWindow(form.Handle).Save(expectedImageFile.Filename, ImageFormat.Png);
					object formCreated = null;
					var formCreatedHandler = new EventHandler((sender, args) => formCreated = sender);
					ZForm.FormCreated += formCreatedHandler;
					try
					{
						instance.Feedback(form.label1, form.label1.Text);
						AssertType(typeof(TranslationFeedbackCreateForm), formCreated);
						AssertEquals(false, ((TopLevelTranslationFeedbackCollection)((TranslationFeedbackCreateForm)formCreated).BusinessEntity)[0].HasChanges);
						((TopLevelTranslationFeedbackCollection)((TranslationFeedbackCreateForm)formCreated).BusinessEntity)[0].XT_SuggestedTranslation = "XXX";
						formFactory = ((TopLevelTranslationFeedbackCollection)((TranslationFeedbackCreateForm)formCreated).BusinessEntity)[0].Factory;
						((TranslationFeedbackCreateForm)formCreated).FireSaveButton();
						((IDisposable)formCreated).Dispose();
					}
					finally
					{
						ZForm.FormCreated -= formCreatedHandler;
					}
				}

				var feedbacks = new BusinessObjectFactory().Load<StmTranslationFeedback>(new ZQuery());
				AssertEquals(1, feedbacks.Length);
				AssertFileSameAsBytes(expectedImageFile.Filename, feedbacks[0].XT_Screenshot);
				AssertEquals(1, formFactory.GetDatabaseCount(typeof(EDIInterchange)));
				var interchange = formFactory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
				var serializer = ZXmlSerializer.New(typeof(TranslationFeedbackEntry));
				using (var xmlReader = SystemMessage.GetXmlReader(SystemMessage.DebugOnlyCreateDownloadedMessage(interchange)))
				{
					var entry = TranslationFeedbackMessagesSerializer.DeserializeTranslationFeedbackEntry(xmlReader);
					AssertFileSameAsBytes(expectedImageFile.Filename, entry.Screenshot);
				}
			}
		}

		public void TestNullControl()
		{
			AddMockTranslation("1", "One", "一");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			object formCreated = null;
			var formCreatedHandler = new EventHandler((sender, args) => formCreated = sender);
			ZForm.FormCreated += formCreatedHandler;
			try
			{
				new TranslationFeedbackProvider().Feedback(Core.SharedConstants.Languages.ChineseTraditional, "一", new string[] { "1", "2", "3" });
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertType(typeof(TranslationFeedbackCreateForm), formCreated);
				((IDisposable)formCreated).Dispose();
			}
			finally
			{
				ZForm.FormCreated -= formCreatedHandler;
			}
		}

		public void TestTranslationDuplicateForms()
		{
			ENG.Put("1", new ResourceStringData("1", "One"));
			ENG.Put("2", new ResourceStringData("2", "Two"));
			var instance = new TranslationFeedbackProvider();

			using (var form = new Enterprise.Freight.Forwarding.TestForm())
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				object formCreated = null;
				var formCreatedHandler = new EventHandler((sender, args) => formCreated = sender);
				ZForm.FormCreated += formCreatedHandler;

				try
				{
					AssertEquals(0, CountOpenedForms(typeof(TranslationFeedbackCreateForm)));
					instance.Feedback(form.label1, form.label1.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertType(typeof(TranslationFeedbackCreateForm), formCreated);

					AssertEquals(1, CountOpenedForms(typeof(TranslationFeedbackCreateForm)));
					var formLabel1 = (TranslationFeedbackCreateForm)formCreated;

					instance.Feedback(form.label2, form.label2.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertType(typeof(TranslationFeedbackCreateForm), formCreated);

					AssertEquals(2, CountOpenedForms(typeof(TranslationFeedbackCreateForm)));
					var formLabel2 = (TranslationFeedbackCreateForm)formCreated;

					Assert(!ReferenceEquals(formLabel1, formLabel2));

					instance.Feedback(form.label1, form.label1.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertType(typeof(TranslationFeedbackCreateForm), formCreated);
					AssertEquals(2, CountOpenedForms(typeof(TranslationFeedbackCreateForm)));

					instance.Feedback(form.label2, form.label2.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertType(typeof(TranslationFeedbackCreateForm), formCreated);
					AssertEquals(2, CountOpenedForms(typeof(TranslationFeedbackCreateForm)));

					formLabel1.Dispose();
					formLabel2.Dispose();
				}
				finally
				{
					ZForm.FormCreated -= formCreatedHandler;
				}
			}
		}

		public void TestShowResourceStringForm()
		{
			ENG.Put("1", new ResourceStringData("1", "One"));
			ENG.Put("2", new ResourceStringData("2", "Two"));
			var instance = new TranslationFeedbackProvider();

			using (var form = new Enterprise.Freight.Forwarding.TestForm())
			{
				form.Show();
				object formCreated = null;
				var formCreatedHandler = new EventHandler((sender, args) =>
				{
					if (sender is HelpDataStringForm)
					{
						formCreated = sender;
					}
				});
				ZForm.FormCreated += formCreatedHandler;
				try
				{
					var oneMatch = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Res.CurrentLanguage, "One")[0];
					var twoMatch = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Res.CurrentLanguage, "Two")[0];
					AssertEquals(0, CountOpenedForms(typeof(HelpDataStringForm)));

					instance.ShowResourceStringForm(oneMatch);
					AssertType(typeof(HelpDataStringForm), formCreated);
					AssertEquals(1, CountOpenedForms(typeof(HelpDataStringForm)));
					var formLabel1 = (HelpDataStringForm)formCreated;

					instance.ShowResourceStringForm(twoMatch);
					AssertType($"The name of unexpected form shown is '{((ZForm)formCreated)?.Name}'", typeof(HelpDataStringForm), formCreated);
					AssertEquals(2, CountOpenedForms(typeof(HelpDataStringForm)));
					var formLabel2 = (HelpDataStringForm)formCreated;

					Assert(!ReferenceEquals(formLabel1, formLabel2));

					instance.ShowResourceStringForm(oneMatch);
					AssertEquals(2, CountOpenedForms(typeof(HelpDataStringForm)));
					instance.ShowResourceStringForm(twoMatch);
					AssertEquals(2, CountOpenedForms(typeof(HelpDataStringForm)));

					formLabel1.Dispose();
					formLabel2.Dispose();
				}
				finally
				{
					ZForm.FormCreated -= formCreatedHandler;
				}
			}
		}

		int CountOpenedForms(Type formType)
		{
			return CargoWise.Windows.UI.ZApplication.GetOpenForms().Count(openForm => openForm.GetType() == formType);
		}
	}
}
