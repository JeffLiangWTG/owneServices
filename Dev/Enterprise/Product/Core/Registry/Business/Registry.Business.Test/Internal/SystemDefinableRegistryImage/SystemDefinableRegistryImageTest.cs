using System.Drawing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing;

[TestedType(typeof(SystemDefinableRegistryImage))]
sealed class SystemDefinableRegistryImageTest : RegistryImageTestCase
{
	public void TestReadOnlyFields()
	{
		var image = new SystemDefinableRegistryImage();
		image.SystemDefined = false;

		AssertEquals("Precondition", false, image.SystemDefined);
		Assert(!image.CodeInfo.ReadOnly);
		Assert(!image.EnglishDescriptionInfo.ReadOnly);

		image.SystemDefined = true;
		AssertEquals("Precondition", true, image.SystemDefined);
		Assert(image.CodeInfo.ReadOnly);
		Assert(image.EnglishDescriptionInfo.ReadOnly);
	}

	public void TestFindByCodeAndSystemDefined()
	{
		var collection = new SystemDefinableRegistryImageCollection();
		
		var item1 = collection.AddNew();
		item1.Code = "AAA";
		item1.SystemDefined = false;

		var item2 = collection.AddNew();
		item2.Code = "AAA";
		item2.SystemDefined = true;

		var item3 = collection.AddNew();
		item3.Code = "BBB";
		item3.SystemDefined = false;

		var item4 = collection.AddNew();
		item4.Code = "CCC";
		item4.SystemDefined = true;

		AssertEquals("AAA", collection.FindByCodeAndSystemDefined("AAA", false).Code);
		AssertEquals("BBB", collection.FindByCodeAndSystemDefined("BBB", false).Code);
		AssertEquals("CCC", collection.FindByCodeAndSystemDefined("CCC", true).Code);
		AssertNull(collection.FindByCodeAndSystemDefined("CCC", false));
		AssertEquals("BBB", collection.FindByCodeAndSystemDefined("BBB", false).Code);
		AssertNull(collection.FindByCodeAndSystemDefined("BBB", true));
		AssertNull(collection.FindByCode("XXX"));

		AssertEquals("AAA", collection.FindByCodeAndSystemDefined("AAa", false).Code);
	}

	public void TestValidation_WhenOneSystemImageAndOneUserImageWithTheSameCode_ShouldBeNoError()
	{
		var collection = new SystemDefinableRegistryImageCollection();
		const string imageCode = "IMG";

		var systemImage = collection.AddNew();
		systemImage.SystemDefined = true;
		systemImage.Code = imageCode;
		systemImage.EnglishDescription = "system image";
		systemImage.Image = new Bitmap(1, 1);

		var userImage = collection.AddNew();
		userImage.SystemDefined = false;
		userImage.Code = imageCode;
		userImage.EnglishDescription = "user image";
		userImage.Image = new Bitmap(1, 1);

		collection.RunPreSaveValidation();

		AssertNoNotifications(systemImage.CodeInfo);
		AssertNoNotifications(userImage.CodeInfo);
	}

	public void TestValidation_WhenTwoUserImagesWithTheSameCode_ShouldBeError()
	{
		var collection = new SystemDefinableRegistryImageCollection();
		const string imageCode = "IMG";

		var userImage1 = collection.AddNew();
		userImage1.SystemDefined = false;
		userImage1.Code = imageCode;
		userImage1.EnglishDescription = "user image 1";
		userImage1.Image = new Bitmap(1, 1);

		var userImage2 = collection.AddNew();
		userImage2.SystemDefined = false;
		userImage2.Code = imageCode;
		userImage2.EnglishDescription = "user image 2";
		userImage2.Image = new Bitmap(1, 1);

		collection.RunPreSaveValidation();

		var errorMessage = "The code has been duplicated and must be unique. A maximum of one system image and one custom image is allowed.";
		AssertEquals("Image1.Code should have errors", true, userImage1.CodeInfo.HasError(errorMessage));
		AssertEquals("Image2.Code should have errors", true, userImage2.CodeInfo.HasError(errorMessage));
	}

	public void TestValidation_WhenTwoSystemImagesWithTheSameCode_ShouldBeError()
	{
		var collection = new SystemDefinableRegistryImageCollection();
		const string imageCode = "IMG";

		var systemImage1 = collection.AddNew();
		systemImage1.SystemDefined = true;
		systemImage1.Code = imageCode;
		systemImage1.EnglishDescription = "system image 1";
		systemImage1.Image = new Bitmap(1, 1);

		var systemImage2 = collection.AddNew();
		systemImage2.SystemDefined = true;
		systemImage2.Code = imageCode;
		systemImage2.EnglishDescription = "system image 2";
		systemImage2.Image = new Bitmap(1, 1);

		collection.RunPreSaveValidation();

		var errorMessage = "The code has been duplicated and must be unique. A maximum of one system image and one custom image is allowed.";
		AssertEquals("Image1.Code should have errors", true, systemImage1.CodeInfo.HasError(errorMessage));
		AssertEquals("Image2.Code should have errors", true, systemImage2.CodeInfo.HasError(errorMessage));
	}

	#region Implementation

	protected override bool IsCodeUniqueInCollection => false;

	protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
	{
		SystemDefinableRegistryImage result = (SystemDefinableRegistryImage)base.GetBusinessObjectToClone();
		return result;
	}

	protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
	{
		return GetBusinessObjectToClone();
	}

	#endregion
}
