using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebThemeSelectorChildControl))]
	sealed class WebThemeSelectorChildControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.ReadOnly;
		}

		public void TestNormalUsageIsOkay()
		{
			var name1 = "image1.jpg";
			var name2 = "image2.jpg";
			var data1 = new byte[] { 1, 2, 3, 4 };
			var data2 = new byte[] { 5, 6, 7, 8 };

			WebThemeCustomObject dummy = new WebThemeCustomObject();
			dummy.ImageCollection.Add(new WebCustomThemeImageBusinessObject()
			{
				ImageName = name1,
				Data = data1
			});
			dummy.ImageCollection.Add(new WebCustomThemeImageBusinessObject()
			{
				ImageName = name2,
				Data = data2
			});

			dummy.ThemeName = "Test Theme";
			dummy.CSS = "Test CSS";

			WebThemeCustomObjectCollection collection = new WebThemeCustomObjectCollection();
			collection.Add(dummy);

			WebDataRegistry.Instance.WebCampaignCustomTheme.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			using (var control = new WebThemeSelectorChildControl())
			{
				WebThemeCustomObject themeObject = new WebThemeCustomObject();
				control.SetDataBinding(dummy, null);
				AssertEquals("Should have 2 Images.", 2, control.DataSource.ImageCollection.Count);
			}
		}

		public void TestNotificationAppearsWhenOverridingExistingImages()
		{
			var name = "image.jpg";
			var data1 = new byte[] { 1, 2, 3, 4 };
			var data2 = new byte[] { 5, 6, 7, 8 };
			var expectedMessage = Res.GetString("a5b08288-b442-4cd7-868b-7d918fcd6f1c", "There already exists an image with this name. Do you want to override that image?");
			var expectedCaption = Res.GetString("68966445-34f7-416c-ae68-4427d862e9c3", "Image already exists");

			using (var control = new WebThemeSelectorChildControl())
			{
				WebThemeCustomObject themeObject = new WebThemeCustomObject();
				control.SetDataBinding(themeObject, null);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.AddWebCustomImage(name, data1);
				AssertEquals("Should only have one image.", 1, control.DataSource.ImageCollection.Count);
				AssertEquals("Image data should be set.", data1, control.DataSource.ImageCollection[0].Data);
				Assert("Shouldn't have shown a message yet.", UnitTestUserNotification.Instance.LastMessage.WasNone);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.AddWebCustomImage(name, data2);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(expectedCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Should only have one image.", 1, control.DataSource.ImageCollection.Count);
				AssertEquals("Image data should not have changed.", data1, control.DataSource.ImageCollection[0].Data);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				control.AddWebCustomImage(name, data2);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(expectedCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Should only have one image.", 1, control.DataSource.ImageCollection.Count);
				AssertEquals("Image data should have changed.", data2, control.DataSource.ImageCollection[0].Data);
			}
		}

		public void TestOverridingExistingImagesIsCaseInsensitive()
		{
			var data1 = new byte[] { 1, 2, 3, 4 };
			var data2 = new byte[] { 5, 6, 7, 8 };
			var expectedMessage = Res.GetString("530a915c-aa59-47c0-8ef1-b4754ba58ffd", "There already exists an image with this name. Do you want to override that image?");
			var expectedCaption = Res.GetString("e3785887-5209-45bf-9bd6-0316dd4841ea", "Image already exists");

			using (var control = new WebThemeSelectorChildControl())
			{
				WebThemeCustomObject themeObject = new WebThemeCustomObject();
				control.SetDataBinding(themeObject, null);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.AddWebCustomImage("Image.jpg", data1);
				AssertEquals("Should only have one image.", 1, control.DataSource.ImageCollection.Count);
				AssertEquals("Image data should be set.", data1, control.DataSource.ImageCollection[0].Data);
				Assert("Shouldn't have shown a message yet.", UnitTestUserNotification.Instance.LastMessage.WasNone);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.AddWebCustomImage("image.jpg", data2);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(expectedCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Should only have one image.", 1, control.DataSource.ImageCollection.Count);
				AssertEquals("Image data should not have changed.", data1, control.DataSource.ImageCollection[0].Data);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				control.AddWebCustomImage("image.JPG", data2);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(expectedCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Should only have one image.", 1, control.DataSource.ImageCollection.Count);
				AssertEquals("Image data should have changed.", data2, control.DataSource.ImageCollection[0].Data);
			}
		}

		public void TestImportEmptyImageFile()
		{
			var data = Array.Empty<byte>();
			using (var control = new WebThemeSelectorChildControlForTest())
			{
				Assert(control.IsEmptyImage_Exposed(data));
			}

			AssertEquals("The image supplied does not contain a valid image format or it is an empty file, please re-select.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestFormHasChanges_AddWebCustomImage()
		{
			const string imageName = "image1.jpg";
			var data = new byte[] { 1, 2, 3, 4 };

			using (var testForm = new RegistryFormTest.DummyRegistryForm())
			using (var control = new WebThemeSelectorChildControlForTest())
			{
				control.SetDataBinding(new WebThemeCustomObject(), null);

				testForm.Show();
				testForm.Controls.Add(control);
				Assert(!testForm.ImportantMethodWasCalled);

				control.CheckImageData = true;
				control.AddWebCustomImage(imageName, data);
				Assert(testForm.ImportantMethodWasCalled);
			}
		}
	}
}
