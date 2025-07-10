using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebCustomImagesControl))]
	sealed class WebCustomImagesControlTest : RegistryZUserControlTestCase
	{
		public void TestNormalUsageIsOkay()
		{
			var name1 = "image1.jpg";
			var name2 = "image2.jpg";
			var url1 = "BLU.webtracker.com/";
			var url2 = "RED.webtracker.com/";
			var data1 = new byte[] { 1, 2, 3, 4 };
			var data2 = new byte[] { 5, 6, 7, 8 };

			WebDataRegistry.Instance.WebTrackerUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { url1, url2 });

			using (var control = new WebCustomImagesControl(WebDataRegistry.Instance.WebTrackerCustomImages))
			{
				control.Value = Array.Empty<WebTrackerCustomImage>();
				AssertEquals("Should have 3 URLs.", 3, control.UrlComboBox.Items.Count);
				AssertEquals("All URLs", (string)control.UrlComboBox.Items[0]);
				AssertEquals(url1, (string)control.UrlComboBox.Items[1]);
				AssertEquals(url2, (string)control.UrlComboBox.Items[2]);

				control.UrlComboBox.SelectedIndex = 0;
				control.AddWebTrackerCustomImage(name1, data1);
				control.UrlComboBox.SelectedIndex = 1;
				control.AddWebTrackerCustomImage(name2, data2);

				var finalValue = control.Value;
				AssertEquals(2, finalValue.Length);
				AssertEquals(name1, finalValue[0].Name);
				AssertEquals(name2, finalValue[1].Name);
				AssertEquals("", finalValue[0].Url);
				AssertEquals(url1, finalValue[1].Url);
				AssertEquals(data1, finalValue[0].Data);
				AssertEquals(data2, finalValue[1].Data);
			}
		}

		public void TestDataForDeletedUrlsRemains()
		{
			var name1 = "image1.jpg";
			var name2 = "image2.jpg";
			var url1 = "BLU.webtracker.com/";
			var url2 = "RED.webtracker.com/";
			var data1 = new byte[] { 1, 2, 3, 4 };
			var data2 = new byte[] { 5, 6, 7, 8 };

			var registryData = new WebTrackerCustomImage[] { new WebTrackerCustomImage(name1, url1, data1), new WebTrackerCustomImage(name2, url2, data2) };
			WebDataRegistry.Instance.WebTrackerUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { url1 });

			using (var control = new WebCustomImagesControl(WebDataRegistry.Instance.WebTrackerCustomImages))
			{
				control.Value = registryData;
				AssertEquals("Should have 3 URLs.", 3, control.UrlComboBox.Items.Count);
				AssertEquals("All URLs", (string)control.UrlComboBox.Items[0]);
				AssertEquals(url1, (string)control.UrlComboBox.Items[1]);
				AssertEquals(url2, (string)control.UrlComboBox.Items[2]);

				control.UrlComboBox.SelectedIndex = 2;
				AssertEquals("Should have data for the non-existant URL.", name2, control.ImageList.Items[0].Text);
			}
		}

		public void TestNotificationAppearsWhenOverridingExistingImages()
		{
			var name = "image.jpg";
			var data1 = new byte[] { 1, 2, 3, 4 };
			var data2 = new byte[] { 5, 6, 7, 8 };
			var expectedMessage = Res.GetString("4ebc280b-b89a-4a5e-819d-1785e0d9157c", "There already exists an image with this name. Do you want to override that image?");
			var expectedCaption = Res.GetString("cf0d5ad6-ffa5-467a-a570-869f3cb1e6ff", "Image already exists");

			using (var control = new WebCustomImagesControl(WebDataRegistry.Instance.WebTrackerCustomImages))
			{
				control.Value = Array.Empty<WebTrackerCustomImage>();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.AddWebTrackerCustomImage(name, data1);
				AssertEquals("Should only have one image.", 1, control.Value.Length);
				AssertEquals("Image data should be set.", data1, control.Value[0].Data);
				Assert("Shouldn't have shown a message yet.", UnitTestUserNotification.Instance.LastMessage.WasNone);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.AddWebTrackerCustomImage(name, data2);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(expectedCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Should only have one image.", 1, control.Value.Length);
				AssertEquals("Image data should not have changed.", data1, control.Value[0].Data);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var previousValue = control.Value;
				control.AddWebTrackerCustomImage(name, data2);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(expectedCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Should only have one image.", 1, control.Value.Length);
				AssertEquals("Image data should have changed.", data2, control.Value[0].Data);
				Assert("Registry item should have changes", !WebDataRegistry.Instance.WebTrackerCustomImages.DataType.ValuesAreEqual(previousValue, control.Value));
			}
		}

		public void TestOverridingExistingImagesIsCaseInsensitive()
		{
			var data1 = new byte[] { 1, 2, 3, 4 };
			var data2 = new byte[] { 5, 6, 7, 8 };
			var expectedMessage = Res.GetString("4ebc280b-b89a-4a5e-819d-1785e0d9157c", "There already exists an image with this name. Do you want to override that image?");
			var expectedCaption = Res.GetString("cf0d5ad6-ffa5-467a-a570-869f3cb1e6ff", "Image already exists");

			using (var control = new WebCustomImagesControl(WebDataRegistry.Instance.WebTrackerCustomImages))
			{
				control.Value = Array.Empty<WebTrackerCustomImage>();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.AddWebTrackerCustomImage("Image.jpg", data1);
				AssertEquals("Should only have one image.", 1, control.Value.Length);
				AssertEquals("Image data should be set.", data1, control.Value[0].Data);
				Assert("Shouldn't have shown a message yet.", UnitTestUserNotification.Instance.LastMessage.WasNone);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.AddWebTrackerCustomImage("image.jpg", data2);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(expectedCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Should only have one image.", 1, control.Value.Length);
				AssertEquals("Image data should not have changed.", data1, control.Value[0].Data);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var previousValue = control.Value;
				control.AddWebTrackerCustomImage("image.JPG", data2);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(expectedCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Should only have one image.", 1, control.Value.Length);
				AssertEquals("Image data should have changed.", data2, control.Value[0].Data);
				Assert("Registry item should have changes", !WebDataRegistry.Instance.WebTrackerCustomImages.DataType.ValuesAreEqual(previousValue, control.Value));
			}
		}

		public void TestSameImageNameForDifferentURLs()
		{
			var name1 = "Logo.jpg";
			var name2 = "Logo.jpg";
			var url1 = "BLU.webtracker.com/";
			var url2 = "RED.webtracker.com/";
			var data1 = new byte[] { 1, 2, 3, 4, 5 };
			var data2 = new byte[] { 5, 6, 7, 8 };

			var registryData = new WebTrackerCustomImage[] { new WebTrackerCustomImage(name1, url1, data1), new WebTrackerCustomImage(name2, url2, data2) };
			WebDataRegistry.Instance.WebTrackerUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { url1 });

			using (var control = new WebTrackerCustomImagesControlForTest(WebDataRegistry.Instance.WebTrackerCustomImages))
			{
				control.Value = registryData;
				control.CheckImageData = true;

				control.UrlComboBox.SelectedIndex = 1;
				control.ImageList.Select();
				control.ImageList.Items[0].Selected = true;
				control.ImageList.Size = new Size(20, 30);
				AssertEquals(control.ImageDataLength, data1.Length);

				control.UrlComboBox.SelectedIndex = 2;
				control.ImageList.Select();
				control.ImageList.Items[0].Selected = true;
				control.ImageList.Size = new Size(10, 20);
				AssertEquals(control.ImageDataLength, data2.Length);
			}
		}

		public void TestImportEmptyImageFile()
		{
			AssertInvalidImageFile(Array.Empty<byte>());
		}

		public void TestImportInvalidImageFile()
		{
			AssertInvalidImageFile(new byte[] { 1, 2, 3 });
		}

		void AssertInvalidImageFile(byte[] imageData)
		{
			using (var control = new WebTrackerCustomImagesControlForTest(WebDataRegistry.Instance.WebTrackerCustomImages))
			{
				Assert("Image should not be valid", !control.IsValidImage_Exposed(imageData));
			}

			AssertEquals("The image supplied does not contain a valid image format or it is an empty file, please re-select.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestFormHasChanges_AddWebTrackerCustomImage()
		{
			const string imageName = "image1.jpg";
			const string url = "BLU.webtracker.com/";
			var data = new byte[] { 1, 2, 3, 4 };

			WebDataRegistry.Instance.WebTrackerUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { url });

			using (var testForm = new RegistryFormTest.DummyRegistryForm())
			using (var control = new WebTrackerCustomImagesControlForTest(WebDataRegistry.Instance.WebTrackerCustomImages))
			{
				testForm.Show();

				control.Value = Array.Empty<WebTrackerCustomImage>();
				testForm.Controls.Add(control);
				Assert(!testForm.ImportantMethodWasCalled);

				control.CheckImageData = true;
				control.AddWebTrackerCustomImage(imageName, data);
				Assert(testForm.ImportantMethodWasCalled);
			}
		}

		public void TestRefreshPreviewPictureBox_NullReferenceException()
		{
			const string name1 = "Logo.jpg";
			const string url1 = "BLU.webtracker.com/";
			var data1 = new byte[] { 1, 2, 3, 4, 5 };

			var registryData = new[] { new WebTrackerCustomImage(name1, url1, data1) };
			WebDataRegistry.Instance.WebTrackerUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { url1 });

			using (var control = new WebTrackerCustomImagesControlForTest(WebDataRegistry.Instance.WebTrackerCustomImages))
			{
				control.Value = registryData;
				control.CheckImageData = true;
				control.UrlComboBox.SelectedIndex = 1;
				control.ImageList.Select();
				control.ImageList.Items[0].Selected = true;

				AssertNoExceptionThrown(() => control.ImageList.Size = new Size(20, 30));
			}
		}

		protected override IBusiness GetNewBusinessEntity() => null;

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;

		class WebTrackerCustomImagesControlForTest : WebCustomImagesControl
		{
			public WebTrackerCustomImagesControlForTest(IRegistryItem registryItem)
				: base(WebDataRegistry.Instance.WebTrackerCustomImages)
			{
			}
			public bool CheckImageData { get; set; }

			public int ImageDataLength { get; set; }

			protected override void RefreshPreviewPictureBox(Stream stream, string name)
			{
				if (!CheckImageData)
				{
					base.RefreshPreviewPictureBox(stream, name);

					return;
				}

				using (var memoryStream = new MemoryStream())
				{
					stream.CopyTo(memoryStream);
					ImageDataLength = memoryStream.ToArray().Length;
				}
			}

			internal override IEnumerable<ListViewItem> GetImageListViewItem()
			{
				var list = ImageList.Items.Cast<ListViewItem>().ToList();
				list.Add(null);

				return list;
			}

			public bool IsValidImage_Exposed(byte[] data) => IsValidImage(data);
		}
	}
}
