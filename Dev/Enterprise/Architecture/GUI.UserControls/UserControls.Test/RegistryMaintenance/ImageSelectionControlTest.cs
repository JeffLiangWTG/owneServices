using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ImageSelectionControlTest : TestCaseWithFactory
	{
		public void TestBindControl()
		{
			using var form = new ZForm();
			var control = new ImageSelectionControl();
			form.Controls.Add(control);
			form.Show();
			Application.DoEvents();

			var bO = new TestNonPersistentBusinessObject();
			control.SetDataBinding(bO, "ImageProperty");
			bO.ImageProperty = new Bitmap(10, 10);
			AssertEquals("Image should be bound properly", bO.ImageProperty, control.Image);

			bO.ImageProperty = null;
			AssertNull("Image on control should be set to null when null on business object", control.Image);

			bO.ImageProperty = new Bitmap(10, 10);
			AssertNotNull(control.Image);
			control.Image = null;
			var x = new Label();
			form.Controls.Add(x);
			x.Focus();
			AssertNull("Image on business object should be set to null when null on control", bO.ImageProperty);
		}

		public void TestReadOnly()
		{
			using var control = new ImageSelectionControlForTest();
			AssertEquals("Precondition: ReadOnly should be false", false, control.ReadOnly);
			AssertEquals("Precondition: BrowseButton should be enabled", true, control.BrowseButton.Enabled);
			AssertEquals("Precondition: ClearImage button should be enabled", true, control.ClearImageButton.Enabled);

			control.ReadOnly = true;
			AssertEquals("ReadOnly", true, control.ReadOnly);
			AssertEquals("BrowseButton.Enabled", false, control.BrowseButton.Enabled);
			AssertEquals("ClearImageButton.Enabled", false, control.ClearImageButton.Enabled);
			AssertEquals("ViewModeButton.Enabled", false, control.ViewModeButton.Enabled);

			control.ReadOnly = false;
			AssertEquals("ReadOnly", false, control.ReadOnly);
			AssertEquals("BrowseButton.Enabled", true, control.BrowseButton.Enabled);
			AssertEquals("ClearImageButton.Enabled", true, control.ClearImageButton.Enabled);
			AssertEquals("ViewModeButton.Enabled", true, control.ViewModeButton.Enabled);
		}

		public void TestJpegOpensCorrectly()
		{
			var file = Path.Combine(Env.TempPath, "ImageSelectionControlValid.jpg");
			var output = Path.Combine(Env.TempPath, "ImageSelectionControlOutput.jpg");
			var newoutput = Path.Combine(Env.TempPath, "ImageSelectionControlOutput.png");
			try
			{
				if (File.Exists(file))
				{
					File.Delete(file);
				}

				using var control = new ImageSelectionControlForTest();
				Image image = new Bitmap(1, 1);
				image.Save(file, ImageFormat.Jpeg);

				control.FileDialog.FileName = file;
				var info = control.FileDialog.GetType().GetMethod("OnFileOk", BindingFlags.NonPublic | BindingFlags.Instance);
				info.Invoke(control.FileDialog, new object[] { new CancelEventArgs() });

				AssertNoExceptionThrown(() => control.fPictureBox.Image.Save(output));

				Assert(!File.Exists(newoutput));

				control.SaveFileDialog.FileName = newoutput;
				info = control.SaveFileDialog.GetType().GetMethod("OnFileOk", BindingFlags.NonPublic | BindingFlags.Instance);
				info.Invoke(control.SaveFileDialog, new object[] { new CancelEventArgs() });

				Assert(File.Exists(newoutput));

				control.fPictureBox.Image.Dispose(); //to fix the fact that it's still holding on to a handle
			}
			finally
			{
				TempFile.Delete(file);
				TempFile.Delete(output);
				TempFile.Delete(newoutput);
			}
		}

		public void TestHandleImageFromFileDialog()
		{
			var validFileName = Path.Combine(Env.TempPath, "ImageSelectionControlValid.bmp");
			var invalidFileName = Path.Combine(Env.TempPath, "ImageSelectionControlInvalid.bmp");
			var imageFileWithValidSize = Path.Combine(Env.TempPath, "ImageSize20mb.jpg");
			var imageFileWithInvalidSize = Path.Combine(Env.TempPath, "ImageSize30mb.jpg");

			try
			{
				using var control = new ImageSelectionControlForTest();
				Image image = new Bitmap(1, 1);
				image.Save(validFileName);

				var isImageObjectChangedByUserEventFired = false;
				control.ImageObjectChangedByUser += (sender, e) =>
				{
					isImageObjectChangedByUserEventFired = true;
				};

				control.FileDialog.FileName = validFileName;
				var info = control.FileDialog.GetType().GetMethod("OnFileOk", BindingFlags.NonPublic | BindingFlags.Instance);
				info.Invoke(control.FileDialog, new object[] { new CancelEventArgs() });
				Assert(isImageObjectChangedByUserEventFired);

				using (control.fPictureBox.Image)
				{
					AssertEquals("fPictureBox.Image should be equal to the image that was selected in FileDialog", true, Utilities.IsImageEqual(image, control.fPictureBox.Image));
				}

				control.fPictureBox.Image = null;

				using var writer = File.CreateText(invalidFileName);
				writer.WriteLine("!@$$#%@%@%!#@@!#@!$@#$#@%@%#%");

				AssertEquals("Precondition: There should not be an error message yet", null, UnitTestUserNotification.Instance.LastMessage.Text);

				control.FileDialog.FileName = invalidFileName;
				info.Invoke(control.FileDialog, new object[] { new CancelEventArgs() });

				var errorMessage = string.Format("The following error occurred when attempting to load the image file \"{0}\":\r\n\r\n{1}\r\n\r\nThe image file might be corrupted or not in the correct format.", invalidFileName, "Out of memory.");

				AssertEquals("Error Message", errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				using (control.fPictureBox.Image)
				{
					AssertEquals("fPictureBox.Image should be null", null, control.fPictureBox.Image);
				}

				UnitTestUserNotification.Instance.ClearMessages();

				using (TestFileHelper.GenerateRandomJpg(imageFileWithValidSize, 19))
				{
					control.FileDialog.FileName = imageFileWithValidSize;
					info.Invoke(control.FileDialog, new object[] { new CancelEventArgs() });

					AssertEquals("Precondition: There should not be an error message", null, UnitTestUserNotification.Instance.LastMessage.Text);

					using (control.fPictureBox.Image)
					{
					}
					control.fPictureBox.Image = null;
				}

				using (TestFileHelper.GenerateRandomJpg(imageFileWithInvalidSize, 30))
				{
					control.FileDialog.FileName = imageFileWithInvalidSize;
					info.Invoke(control.FileDialog, new object[] { new CancelEventArgs() });

					errorMessage = string.Format("The following error occurred when attempting to load the image file \"{0}\":\r\n\r\n{1}\r\n\r\nThe image file might be corrupted or not in the correct format.", imageFileWithInvalidSize, "The size of the image cannot be larger than 20MB.");

					AssertEquals("Error Message", errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					using (control.fPictureBox.Image)
					{
						AssertEquals("fPictureBox.Image should be null", null, control.fPictureBox.Image);
					}
				}
			}
			finally
			{
				File.Delete(validFileName);
				File.Delete(invalidFileName);
				File.Delete(imageFileWithValidSize);
				File.Delete(imageFileWithInvalidSize);
			}
		}

		public void TestHandleImageFromFileDialogWhenImageFileWithInvalidDimension()
		{
			var imageFileWithValidDimension = Path.Combine(Env.TempPath, "ImageHeight65535.png");
			var imageFileWithInvalidDimension = Path.Combine(Env.TempPath, "ImageHeight65536.png");
			UnitTestUserNotification.Instance.ClearMessages();

			try
			{
				using (var control = new ImageSelectionControlForTest())
				{
					var info = control.FileDialog.GetType().GetMethod("OnFileOk", BindingFlags.NonPublic | BindingFlags.Instance);

					TestFileHelper.GeneratePng(imageFileWithValidDimension, 65535);
					control.FileDialog.FileName = imageFileWithValidDimension;
					info.Invoke(control.FileDialog, new object[] { new CancelEventArgs() });
					AssertEquals("Precondition: There should not be an error message", null, UnitTestUserNotification.Instance.LastMessage.Text);
					using (control.fPictureBox.Image)
					{
					}
					control.fPictureBox.Image = null;

					TestFileHelper.GeneratePng(imageFileWithInvalidDimension, 65536);
					control.FileDialog.FileName = imageFileWithInvalidDimension;
					info.Invoke(control.FileDialog, new object[] { new CancelEventArgs() });

					var errorMessage = string.Format("The following error occurred when attempting to load the image file \"{0}\":\r\n\r\n{1}\r\n\r\nThe image file might be corrupted or not in the correct format.", imageFileWithInvalidDimension, "The image dimensions exceed the maximum allowed size of 65535 pixels.");
					AssertEquals("Error Message", errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					using (control.fPictureBox.Image)
					{
						AssertEquals("fPictureBox.Image should be null", null, control.fPictureBox.Image);
					}
				}
			}
			finally
			{
				File.Delete(imageFileWithValidDimension);
				File.Delete(imageFileWithInvalidDimension);
			}
		}

		public void TestEXIFOrientationIsUsed()
		{
			var inputImagePath = Path.Combine(Env.TempPath, "F7.jpg");

			try
			{
				using var control = new ImageSelectionControlForTest();
				using var inputImage = TestFileHelper.GenerateRandomJpg(inputImagePath, null, RotateFlipType.Rotate270FlipX);
				control.FileDialog.FileName = inputImagePath;
				var info = control.FileDialog.GetType().GetMethod("OnFileOk", BindingFlags.NonPublic | BindingFlags.Instance);
				info.Invoke(control.FileDialog, new object[] { new CancelEventArgs() });

				using var expectedImage = TestFileHelper.CloneJpgAndApplyExifOrientation(inputImagePath);
				using var resultingImage = control.fPictureBox.Image;
				Assert("EXIF orientation property should be present in input image", inputImage.PropertyIdList.Contains(ImageHelper.ExifOrientationId));
				Assert("EXIF orientation property should have been removed from the resulting image", !resultingImage.PropertyIdList.Contains(ImageHelper.ExifOrientationId));
				AssertImageEquals("fPictureBox.Image should have been rotated/flipped based on the input image EXIF orientation", expectedImage, resultingImage, 1);
			}
			finally
			{
				File.Delete(inputImagePath);
			}
		}

		public void TestImageClears()
		{
			var validFileName = Path.Combine(Env.TempPath, "ImageSelectionControlValid.bmp");

			if (File.Exists(validFileName))
			{
				File.Delete(validFileName);
			}

			using var control = new ImageSelectionControlForTest();

			Image image = new Bitmap(1, 1);
			image.Save(validFileName);

			control.FileDialog.FileName = validFileName;
			var info = control.FileDialog.GetType().GetMethod("OnFileOk", BindingFlags.NonPublic | BindingFlags.Instance);
			info.Invoke(control.FileDialog, new object[] { new CancelEventArgs() });

			using (control.fPictureBox.Image)
			{
				AssertEquals("fPictureBox.Image should be equal to the image that was selected in FileDialog", true, Utilities.IsImageEqual(image, control.fPictureBox.Image));
			}

			bool isEventFired = false;
			control.ImageObjectForBindingChanged += delegate (object sender, EventArgs e)
			{
				isEventFired = true;
			};

			Assert(!isEventFired);

			var isImageObjectChangedByUserEventFired = false;
			control.ImageObjectChangedByUser += (sender, e) =>
			{
				isImageObjectChangedByUserEventFired = true;
			};

			control.ClearImageButton.PerformClick();

			using (control.fPictureBox.Image)
			{
				AssertEquals("fPictureBox.Image should be null", null, control.fPictureBox.Image);
			}

			Assert(isEventFired);
			Assert(isImageObjectChangedByUserEventFired);

			File.Delete(validFileName);
		}

		public void TestFileDialogFilter()
		{
			using ImageSelectionControlForTest control = new ImageSelectionControlForTest();
			var expectedFilter = "Image Files (*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG";
			AssertEquals("FileDialog.Filter", expectedFilter, control.FileDialog.Filter);
			AssertEquals("FileDialogFilter", expectedFilter, control.FileDialogFilter);

			expectedFilter = "Image Files (*.BMP)|*.BMP";
			control.FileDialogFilter = expectedFilter;
			AssertEquals("FileDialog.Filter", expectedFilter, control.FileDialog.Filter);
			AssertEquals("FileDialogFilter", expectedFilter, control.FileDialogFilter);
		}

		#region Implementation

		#region class TestNonPersistentBusinessObject

		class TestNonPersistentBusinessObject : NonPersistentBusinessObject
		{
			public Image ImageProperty
			{
				get { return fImageProperty; }
				set
				{
					fImageProperty = value;
					RefreshBinding();
				}
			}
			Image fImageProperty;
		}

		#endregion

		#region class ImageSelectionControlForTest

		class ImageSelectionControlForTest : ImageSelectionControl
		{
			public new ZButton BrowseButton
			{
				get { return base.BrowseButton; }
			}

			public new ZButton ClearImageButton
			{
				get { return base.ClearImageButton; }
			}

			public new ZButton ViewModeButton
			{
				get { return base.ViewModeButton; }
			}

			public new PictureBox fPictureBox
			{
				get { return base.fPictureBox; }
			}

			public new ZOpenFileDialog FileDialog
			{
				get { return base.FileDialog; }
			}

			public new ZSaveFileDialog SaveFileDialog
			{
				get { return base.SaveFileDialog; }
			}
		}

		#endregion

		#endregion
	}
}
