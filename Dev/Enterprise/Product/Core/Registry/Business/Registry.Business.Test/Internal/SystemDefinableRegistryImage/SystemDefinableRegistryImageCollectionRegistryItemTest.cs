using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing;

[TestedType(typeof(SystemDefinableRegistryImageCollectionRegistryItem))]
sealed class SystemDefinableRegistryImageCollectionRegistryItemTest
	: RegistryImageCollectionRegistryItemTest<SystemDefinableRegistryImageCollectionRegistryItem, SystemDefinableRegistryImageCollection>
{
	#region Default Value

	public void TestDefaultValue()
	{
		SystemDefinableRegistryImageCollectionRegistryItem registryItem = new SystemDefinableRegistryImageCollectionRegistryItem("RegistryItem", null, null, null, RegistryStorageFlags.All);
		AssertDefaultValueProperties(registryItem.DefaultValue);
	}

	public void TestDefaultValue_WhenRegistryItemHasNoSavedValues_ShouldReturnDefaultValue()
	{
		SystemDefinableRegistryImageCollectionRegistryItem registryItem = new SystemDefinableRegistryImageCollectionRegistryItem("RegistryItem", null, null, null, RegistryStorageFlags.All);
		AssertDefaultValueProperties(registryItem.Value);
	}

	static void AssertDefaultValueProperties(SystemDefinableRegistryImageCollection defaultValue)
	{
		AssertEquals("DefaultValue.Count", 1, defaultValue.Count);

		AssertEquals("DefaultValue[0].Code", "QRB", defaultValue[0].Code);
		AssertEquals("DefaultValue[0].SystemDefined", true, defaultValue[0].SystemDefined);
		AssertEquals("DefaultValue[0].EnglishDescription", "QR Bill Logo", defaultValue[0].EnglishDescription);
		AssertNotNull("Resource should not be null.", typeof(SystemDefinableRegistryImageCollectionRegistryItem).Assembly.GetManifestResourceInfo("Enterprise.Registry.Business.Internal.SystemDefinableRegistryImage.QRB_Flag_of_Switzerland.png"));
		AssertEquals("DefaultValue[0].DefaultImageResourceName", "Enterprise.Registry.Business.Internal.SystemDefinableRegistryImage.QRB_Flag_of_Switzerland.png", defaultValue[0].DefaultImageResourceName);
	}

	#endregion

	#region Missing System Defined Images

	public void TestMissingImages_WhenDoesNotContainAnySystemDefinedImage_ShouldAddMissingSystemDefinedImages()
	{
		var collection = new SystemDefinableRegistryImageCollection();

		var image1 = collection.AddNew();
		image1.Code = "AAA";
		image1.EnglishDescription = "AAA image";
		image1.SystemDefined = false;
		image1.Image = new Bitmap(1, 1);

		var image2 = collection.AddNew();
		image2.Code = "BBB";
		image2.EnglishDescription = "BBB image";
		image2.SystemDefined = false;
		image2.Image = new Bitmap(1, 1);

		var item = new SystemDefinableRegistryImageCollectionRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.All);

		item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

		var currentValue = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
			.Cast<SystemDefinableRegistryImage>()
			.Select(x => x.Code.ToString())
			.ToList();

		AssertContainsExactElementsInAnyOrder("Missing system defined images should be added",
			new[] { "AAA", "BBB", "QRB" },
			currentValue);
	}

	public void TestMissingImages_WhenContainsAllSystemDefinedImages_NothingShouldBeAdded()
	{
		var collection = new SystemDefinableRegistryImageCollection();

		var image1 = collection.AddNew();
		image1.Code = "AAA";
		image1.EnglishDescription = "AAA image";
		image1.SystemDefined = false;
		image1.Image = new Bitmap(1, 1);

		var image2 = collection.AddNew();
		image2.Code = "BBB";
		image2.EnglishDescription = "BBB image";
		image2.SystemDefined = false;
		image2.Image = new Bitmap(1, 1);

		var item = new SystemDefinableRegistryImageCollectionRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.All);

		var systemDefinedImageCodes = new List<string>();
		foreach (var systemDefinedImage in item.DefaultValue.Cast<SystemDefinableRegistryImage>().Where(x => x.SystemDefined))
		{
			collection.Add(systemDefinedImage);
			systemDefinedImageCodes.Add(systemDefinedImage.Code);
		}

		item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

		var currentValue = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
			.Cast<SystemDefinableRegistryImage>()
			.Select(x => x.Code.ToString())
			.ToList();

		AssertContainsExactElementsInAnyOrder("Nothing should be added",
			new[] { "AAA", "BBB" }.Concat(systemDefinedImageCodes).ToArray(),
			currentValue);
	}

	public void TestMissingImages_WhenContainsCustomImageForWhichExistsSystemDefinedImage_ShouldAddSystemDefinedImageToo()
	{
		var collection = new SystemDefinableRegistryImageCollection();

		var image1 = collection.AddNew();
		image1.Code = "AAA";
		image1.EnglishDescription = "AAA image";
		image1.SystemDefined = false;
		image1.Image = new Bitmap(1, 1);

		var image2 = collection.AddNew();
		image2.Code = "QRB";
		image2.EnglishDescription = "QRB image (custom)";
		image2.SystemDefined = false;
		image2.Image = new Bitmap(1, 1);

		var item = new SystemDefinableRegistryImageCollectionRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.All);

		item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

		var currentValue = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
			.Cast<SystemDefinableRegistryImage>()
			.Select(x => x.Code.ToString())
			.ToList();

		AssertContainsExactElementsInAnyOrder("Missing system defined image should be added",
			new[] { "AAA", "QRB", "QRB" },
			currentValue);
	}

	#endregion

	#region Implementation

	protected override StronglyTypedRegistryItem<SystemDefinableRegistryImageCollection, SystemDefinableRegistryImageCollection> GetNewRegistryItem()
	{
		return new SystemDefinableRegistryImageCollectionRegistryItem("TEST_IMAGE", null, null, null, RegistryStorageFlags.All, emptyDefaultValue: true);
	}

	protected override SystemDefinableRegistryImageCollection GetNewCollection()
	{
		return new SystemDefinableRegistryImageCollection();
	}

	#endregion
}
