using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	abstract class RegistryImageTestCase : RegistryBusinessObjectTestCaseBase
	{
		public void TestGettingImage()
		{
			string resourceName = "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-ITC_Aus_Backv22.gif";

			AssertEquals("Precondition: Image should be null.", null, BizObj.Image);
			BizObj.DefaultImageResourceName = resourceName;

			using (Stream stream = typeof(RegistryImage).Assembly.GetManifestResourceStream(resourceName))
			using (MemoryStream expectedStream = new MemoryStream())
			using (MemoryStream actualStream = new MemoryStream())
			using (Image expectedImage = Image.FromStream(stream))
			using (Image actualImage = BizObj.Image)
			{
				expectedImage.Save(expectedStream, expectedImage.RawFormat);
				actualImage.Save(actualStream, actualImage.RawFormat);

				byte[] expectedBytes = new byte[expectedStream.Length];
				byte[] actualBytes = new byte[actualStream.Length];

				expectedStream.Read(expectedBytes, 0, expectedBytes.Length);
				actualStream.Read(actualBytes, 0, actualBytes.Length);

				AssertEquals("RegistryImage.Image", expectedBytes, actualBytes);
			}

			using (Image image = new Bitmap(1, 1))
			{
				BizObj.Image = image;
				AssertEquals("Image should be the same as the image that was explicitly set.", true, Utilities.IsImageEqual(image, BizObj.Image));
			}
		}

		public void TestSavingAndDeletingImage()
		{
			TestCaseHelper.ClearTable(StmDataSchema.Constants.TableName);

			using (Bitmap image1 = new Bitmap(1, 2))
			using (Bitmap image2 = new Bitmap(3, 4))
			{
				ImageRegistryItem registryItem = FreightDataRegistry.Instance.RegistryImageContainer;
				DummyNonPersistentBusinessObjectRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(ExpectedBusinessObjectType);
				ZQuery countQuery = new ZQuery(StmDataSchema.SD_Name, registryItem.Name);

				AssertEquals("imagePk.IsEmpty", true, BizObj.imagePk.IsEmpty);
				BizObj.Image = null;
				BizObj.FallbackKeyForSaving = "x";
				int initialDbHitCount = Db.Connection.ExecutedCommandCount;
				byte[] bytes = dataType.Serialise(BizObj);
				AssertEquals("DB hit count", 0, Db.Connection.ExecutedCommandCount - initialDbHitCount);
				AssertEquals("Image registry count", 0, Factory.GetDatabaseCount(typeof(AutoStmData), countQuery));
				AssertEquals("imagePk.IsEmpty", true, BizObj.imagePk.IsEmpty);
				AssertEquals("fallbackKeyInDb", "x", BizObj.fallbackKeyInDb);

				AssertSerialisedValue(bytes, dataType, registryItem, Guid.Empty, "x", null);

				BizObj.Image = image1;
				initialDbHitCount = Db.Connection.ExecutedCommandCount;
				bytes = dataType.Serialise(BizObj);
				AssertEquals("DB hit count", 1, Db.Connection.ExecutedCommandCount - initialDbHitCount);
				ZGuid imagePk1 = BizObj.imagePk;
				AssertEquals("imagePk.IsEmpty", false, imagePk1.IsEmpty);
				AssertEquals("Image registry count", 1, Factory.GetDatabaseCount(typeof(AutoStmData), countQuery));

				AssertSerialisedValue(bytes, dataType, registryItem, imagePk1, "x", image1);

				BizObj.Image = image2;
				initialDbHitCount = Db.Connection.ExecutedCommandCount;
				bytes = dataType.Serialise(BizObj);
				AssertEquals("DB hit count", 1, Db.Connection.ExecutedCommandCount - initialDbHitCount);
				AssertEquals("Image registry count", 1, Factory.GetDatabaseCount(typeof(AutoStmData), countQuery));

				AssertSerialisedValue(bytes, dataType, registryItem, imagePk1, "x", image2);

				BizObj.Image = image2;
				initialDbHitCount = Db.Connection.ExecutedCommandCount;
				bytes = dataType.Serialise(BizObj);
				AssertEquals("DB hit count", 0, Db.Connection.ExecutedCommandCount - initialDbHitCount);
				AssertEquals("Image registry count", 1, Factory.GetDatabaseCount(typeof(AutoStmData), countQuery));

				AssertSerialisedValue(bytes, dataType, registryItem, imagePk1, "x", image2);

				BizObj.FallbackKeyForSaving = "y";
				initialDbHitCount = Db.Connection.ExecutedCommandCount;
				bytes = dataType.Serialise(BizObj);
				AssertEquals("DB hit count", 1, Db.Connection.ExecutedCommandCount - initialDbHitCount);
				AssertEquals("fallbackKeyInDb", "y", BizObj.fallbackKeyInDb);
				AssertEquals("Image registry count", 2, Factory.GetDatabaseCount(typeof(AutoStmData), countQuery));
				ZGuid imagePk2 = BizObj.imagePk;
				Assert("imagePk should have changed.", imagePk1 != imagePk2);

				AssertSerialisedValue(bytes, dataType, registryItem, imagePk2, "y", image2);

				BizObj.FallbackKeyForSaving = "o";
				initialDbHitCount = Db.Connection.ExecutedCommandCount;
				bytes = dataType.Serialise(BizObj);
				AssertEquals("DB hit count", 1, Db.Connection.ExecutedCommandCount - initialDbHitCount);
				AssertEquals("Image registry count", 3, Factory.GetDatabaseCount(typeof(AutoStmData), countQuery));
				AssertEquals("fallbackKeyInDb", "o", BizObj.fallbackKeyInDb);
				AssertEquals("Image registry count", 3, Factory.GetDatabaseCount(typeof(AutoStmData), countQuery));
				ZGuid imagePk3 = BizObj.imagePk;
				Assert("imagePk should have changed.", (imagePk3 != imagePk2) && (imagePk3 != imagePk1));

				BizObj.Image = null;
				AssertEquals("Image should not be reloaded after set.", null, BizObj.Image);
				initialDbHitCount = Db.Connection.ExecutedCommandCount;
				BizObj.fallbackKeyForSaving = "p";
				bytes = dataType.Serialise(BizObj);
				AssertEquals("DB hit count", 0, Db.Connection.ExecutedCommandCount - initialDbHitCount);
				AssertEquals("Image registry count", 3, Factory.GetDatabaseCount(typeof(AutoStmData), countQuery));
				AssertSerialisedValue(bytes, dataType, registryItem, Guid.Empty, "p", null);
				AssertNotNull("Registry image should not be null.", registryItem.GetValueWithoutFallback(imagePk3.ToGuid(), Guid.Empty, Guid.Empty));

				BizObj.Image = image2;
				BizObj.Image = null;
				BizObj.imagePk = imagePk3;
				initialDbHitCount = Db.Connection.ExecutedCommandCount;
				bytes = dataType.Serialise(BizObj);
				AssertEquals("DB hit count", 1, Db.Connection.ExecutedCommandCount - initialDbHitCount);
				AssertEquals("Image registry count", 3, Factory.GetDatabaseCount(typeof(AutoStmData), countQuery));
				AssertSerialisedValue(bytes, dataType, registryItem, Guid.Empty, "p", null);
				AssertNull("Registry image should be null.", registryItem.GetValueWithoutFallback(imagePk3.ToGuid(), Guid.Empty, Guid.Empty));

				initialDbHitCount = Db.Connection.ExecutedCommandCount;
				BizObj.DeleteImage("z");
				AssertEquals("DB hit count", 0, Db.Connection.ExecutedCommandCount - initialDbHitCount);
				AssertEquals("Image registry count", 3, Factory.GetDatabaseCount(typeof(AutoStmData), countQuery));

				BizObj.imagePk = ZGuid.Empty;
				initialDbHitCount = Db.Connection.ExecutedCommandCount;
				BizObj.DeleteImage("y");
				AssertEquals("DB hit count", 0, Db.Connection.ExecutedCommandCount - initialDbHitCount);
				AssertEquals("Image registry count", 3, Factory.GetDatabaseCount(typeof(AutoStmData), countQuery));

				BizObj.imagePk = imagePk2;
				BizObj.fallbackKeyInDb = "y";
				initialDbHitCount = Db.Connection.ExecutedCommandCount;
				BizObj.DeleteImage("y");
				AssertEquals("DB hit count", 1, Db.Connection.ExecutedCommandCount - initialDbHitCount);
				AssertEquals("Image registry count", 3, Factory.GetDatabaseCount(typeof(AutoStmData), countQuery));
				AssertNotNull("Registry value for imagePk1 should not be deleted.", registryItem.GetValueWithoutFallback(imagePk1.ToGuid(), Guid.Empty, Guid.Empty));
				AssertNull("Registry value for imagePk2 should be deleted.", registryItem.GetValueWithoutFallback(imagePk2.ToGuid(), Guid.Empty, Guid.Empty));

				BizObj.imagePk = imagePk1;
				BizObj.fallbackKeyInDb = "x";
				initialDbHitCount = Db.Connection.ExecutedCommandCount;
				BizObj.DeleteImage("x");
				AssertEquals("DB hit count", 1, Db.Connection.ExecutedCommandCount - initialDbHitCount);
				AssertEquals("Image registry count", 3, Factory.GetDatabaseCount(typeof(AutoStmData), countQuery));
				AssertNull("Registry value for imagePk1 should be deleted.", registryItem.GetValueWithoutFallback(imagePk1.ToGuid(), Guid.Empty, Guid.Empty));
				AssertNull("Registry value for imagePk2 should be deleted.", registryItem.GetValueWithoutFallback(imagePk2.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		public void TestImageWouldBeSaved()
		{
			AssertNull("Precondition", BizObj.Image);
			AssertEquals("Precondition", true, BizObj.imagePk.IsEmpty);

			string resourceName = "Enterprise.Registry.Business.Freight.HouseBills.HouseBillOfLadingImages.HBoL-ITC_Aus_Backv22.gif";
			BizObj.DefaultImageResourceName = resourceName;
			BizObj.FallbackKeyForSaving = "triggering the save";

			var dataType = new DummyNonPersistentBusinessObjectRegistryDataType(ExpectedBusinessObjectType);
			dataType.Serialise(BizObj);

			AssertEquals(false, BizObj.imagePk.IsEmpty);
			Image savedImage = FreightDataRegistry.Instance.RegistryImageContainer.GetValueWithoutFallback(BizObj.imagePk.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals(true, Utilities.IsImageEqual(savedImage, BizObj.Image));
		}

		public virtual void TestImageIsMandatory()
		{
			BizObj.Image = null;
			BizObj.RunPreSaveValidation();
			AssertEquals("There should be an error if image is null.", true, BizObj.RowErrors.Contains("Please select an image."));

			BizObj.Image = new Bitmap(1, 1);
			BizObj.RunPreSaveValidation();
			AssertEquals("There should not be any row errors.", false, BizObj.HasRowErrors);
		}

		public void TestValidateDescription()
		{
			AssertNoErrors("Precondition: Description should not have errors.", BizObj.DescriptionInfo);

			BizObj.Description = (NoResString)"";
			AssertHasError(BizObj.DescriptionInfo, "Please enter a Description.");

			BizObj.Description = (NoResString)"123";
			AssertNoErrors(BizObj.DescriptionInfo);
		}

		public void TestDefaultImageResourceNameIsCloned()
		{
			BizObj.DefaultImageResourceName = "x";
			RegistryImage clone = (RegistryImage)BizObj.Clone(null, null);
			AssertEquals("DefaultImageResourceName", "x", clone.DefaultImageResourceName);
		}

		public void TestRegistryImage_Equals()
		{
			var image1 = (RegistryImage)GetBusinessObjectToClone();
			var image2 = (RegistryImage)GetBusinessObjectToClone();
			Assert(image1.Equals(image2));

			image1.Code = "EFG";
			Assert(!image1.Equals(image2));

			image1.Code = "ABC";
			image1.Description = (NoResString)"EFG Description";
			Assert(!image1.Equals(image2));

			image1.Description = (NoResString)"ABC Description";
			image1.fallbackKeyInDb = "opq";
			Assert(!image1.Equals(image2));

			image1.fallbackKeyInDb = null;
			image2.fallbackKeyInDb = string.Empty;
			Assert(image1.Equals(image2));

			image1.fallbackKeyInDb = string.Empty;
			image2.fallbackKeyInDb = null;
			Assert(image1.Equals(image2));

			image1.Image = null;
			image2.Image = null;
			Assert(image1.Equals(image2));

			image1.Image = CreateATestImage(2, 2);
			image2.Image = CreateATestImage();
			Assert(!image1.Equals(image2));
		}

		public void TestRegistryImage_GetHashCode()
		{
			var image1 = (RegistryImage)GetBusinessObjectToClone();
			var image2 = (RegistryImage)GetBusinessObjectToClone();
			AssertEquals(image1.GetHashCode(), image2.GetHashCode());

			image1.fallbackKeyInDb = null;
			image2.fallbackKeyInDb = string.Empty;
			AssertEquals(image1.GetHashCode(), image2.GetHashCode());

			image1.fallbackKeyInDb = string.Empty;
			image2.fallbackKeyInDb = null;
			AssertEquals(image1.GetHashCode(), image2.GetHashCode());

			image1.Image = null;
			image2.Image = null;
			AssertEquals(image1.GetHashCode(), image2.GetHashCode());
		}

		#region Implementation

		Image CreateATestImage(int width = 1, int height = 1)
		{
			var bitmap = new Bitmap(width, height);
			byte[] bytes;

			using (TempFile tempFile = TempFile.New())
			{
				bitmap.Save(tempFile.Filename);
				bytes = File.ReadAllBytes(tempFile.Filename);
			}

			var stream = new MemoryStream(bytes);
			return Image.FromStream(stream);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			RegistryImage result = (RegistryImage)GetNewBusinessObject();
			result.Code = "ABC";
			result.Description = (NoResString)"ABC Description";
			result.Image = CreateATestImage();
			result.imagePk = Guid.NewGuid();
			result.fallbackKeyInDb = "xyz";
			result.FallbackKeyForSaving = "xyz";
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);

			RegistryImage originalHBOLImage = (RegistryImage)originalBusinessObject;
			RegistryImage newHBOLImage = (RegistryImage)newBusinessObject;

			if (!isClone)
			{
				AssertEquals("Image should be serialised to Png.", ImageFormat.Png, newHBOLImage.Image.RawFormat);
			}

			AssertEquals("imagePK", originalHBOLImage.imagePk, newHBOLImage.imagePk);
			AssertEquals("fallbackKeyInDb", originalHBOLImage.fallbackKeyInDb, newHBOLImage.fallbackKeyInDb);
			AssertEquals("Image should be equal.", true, Utilities.IsImageEqual(originalHBOLImage.Image, newHBOLImage.Image));
		}

		protected sealed override bool RequiresFactory
		{
			get { return false; }
		}

		protected sealed override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new RegistryImage BizObj
		{
			get { return (RegistryImage)base.BizObj; }
		}

		void AssertSerialisedValue(byte[] bytes, IRegistryDataType dataType,
			ImageRegistryItem registryItem, ZGuid expectedImagePk, string expectedFallbackKeyInDb, Image expectedImage)
		{
			RegistryImage bizObj = (RegistryImage)dataType.Deserialise(bytes);
			AssertEquals("imagePk", expectedImagePk, BizObj.imagePk);
			AssertEquals("fallbackKeyInDb", expectedFallbackKeyInDb, BizObj.fallbackKeyInDb);

			if (expectedImage == null)
			{
				AssertNull("Image should be null.", bizObj.Image);
			}
			else
			{
				AssertEquals("Image should be equal.", true, Utilities.IsImageEqual(expectedImage, bizObj.Image));
				AssertEquals("Registry image should be equal.", true, Utilities.IsImageEqual(expectedImage, registryItem.GetValueWithoutFallback(expectedImagePk.ToGuid(), Guid.Empty, Guid.Empty)));
			}
		}

		#endregion
	}
}
