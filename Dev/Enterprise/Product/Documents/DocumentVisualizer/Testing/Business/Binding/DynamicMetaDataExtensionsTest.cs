using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.DocDataObjects.Testing;
using Enterprise.ZArchitecture.Core;
using MetaDataType = Enterprise.DocumentVisualizer.Core.MetaDataType;
using MetaDataTypes = CargoWise.ComponentModel.MetaDataTypes;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DynamicMetaDataExtensionsTest : TestCaseWithFactory
	{
		#region TestMaxLengthMetaData

		public void TestMaxLengthMetaData()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			var metaDataProvider = new MetaDataProviderForTest();
			metaDataProvider.GetMetaDataImplementer = (data, type) =>
			{
				if (type == MetaDataType.MaxLength)
				{
					return 99;
				}

				return null;
			};

			var dynamicData = dummy.MakeDynamic(metaDataProvider);

			var metaData = dynamicData
				.GetMetaData()
				.FirstOrDefault(m => m.Id == MetaDataTypes.MaxLength);

			AssertNotNull("MaxLength metadata", metaData);
			AssertEquals("MaxLength metadata Value", 99, metaData.Value);
		}

		#endregion

		#region TestListMetaData

		public void TestListMetaData()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			var volumeUnitsList = new CodeDescriptionPairList(OLookUpEditType.Volume);

			var metaDataProvider = new MetaDataProviderForTest();
			metaDataProvider.GetMetaDataImplementer = (data, type) =>
			{
				if (type == MetaDataType.ListDataSource)
				{
					return volumeUnitsList;
				}

				return null;
			};

			var dynamicData = dummy.MakeDynamic(metaDataProvider);

			var metaData = dynamicData
				.GetMetaData()
				.FirstOrDefault(m => m.Id == MetaDataTypes.ListDataSource);

			AssertNotNull("ListDataSource metadata", metaData);
			AssertEquals("ListDataSource metadata Value", volumeUnitsList, metaData.Value);
		}

		#endregion

		#region TestReadOnlyMetaData

		public void TestReadOnlyMetaData()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			var metaDataProvider = new MetaDataProviderForTest();
			metaDataProvider.GetMetaDataImplementer = (data, type) =>
			{
				if (type == MetaDataType.IsReadOnly)
				{
					return true;
				}

				return null;
			};

			var dynamicData = dummy.MakeDynamic(metaDataProvider);

			var metaData = dynamicData
				.GetMetaData()
				.FirstOrDefault(m => m.Id == MetaDataTypes.ReadOnly);

			AssertNotNull("ReadOnly metadata", metaData);
			AssertEquals("ReadOnly metadata Value", true, metaData.Value);
		}

		#endregion

		#region TestDecimalPlacesMetaData

		public void TestDecimalPlacesMetaData()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			var metaDataProvider = new MetaDataProviderForTest();
			metaDataProvider.GetMetaDataImplementer = (data, type) =>
			{
				if (type == MetaDataType.DecimalPlaces)
				{
					return 3;
				}

				return null;
			};

			var dynamicData = dummy.MakeDynamic(metaDataProvider);

			var metaData = dynamicData
				.GetMetaData()
				.FirstOrDefault(m => m.Id == MetaDataTypes.DecimalPlaces);

			AssertNotNull("DecimalPlaces metadata", metaData);
			AssertEquals("DecimalPlaces metadata Value", 3, metaData.Value);
		}

		#endregion

		#region TestDateTimeFormatMetaData

		public void TestDateTimeFormatMetaData()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			var metaDataProvider = new MetaDataProviderForTest();
			metaDataProvider.GetMetaDataImplementer = (data, type) =>
			{
				if (type == MetaDataType.DateTimeFormat)
				{
					return KDateTimeFormat.Long;
				}

				return null;
			};

			var dynamicData = dummy.MakeDynamic(metaDataProvider);

			var metaData = dynamicData
				.GetMetaData()
				.FirstOrDefault(m => m.Id == MetaDataTypes.DateTimeFormat);

			AssertNotNull("DateTimeFormat metadata", metaData);
			AssertEquals("DateTimeFormat metadata Value", KDateTimeFormat.Long, metaData.Value);
		}

		#endregion

		#region TestCustomFindBoxPopup

		public void TestCustomFindBoxPopup()
		{
			var popup = new DummyCustomFindBoxPopup();

			using (ObjectFactory.Substitute<DummyCustomFindBoxPopup>(() => popup))
			{
				var dummy = new DummyDocDataObject();

				var dynamicData = dummy.MakeDocDataDynamic();
				var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));

				var metaData = code
					.GetMetaData()
					.FirstOrDefault(m => m.Id == CustomFindBoxMetaData.Identifier);

				AssertNotNull("CustomFindBox metadata found", metaData);

				if (metaData is CustomFindBoxMetaData customFindBoxMetaData)
				{
					AssertType<DummyCustomFindBoxPopup>("Returned instance of DummyCustomFindBoxPopup", customFindBoxMetaData.CustomFindBoxProvider());
					Assert("Show Description should be true by default", customFindBoxMetaData.ShowDescription);
				}
				else
				{
					Fail("Expeced custom find box metadata to be of type CustomFindBoxMetaData");
				}
			}
		}

		public void TestCustomFindBoxPopup_ShowDescription()
		{
			var popup = new DummyCustomFindBoxPopup();

			using (ObjectFactory.Substitute<DummyCustomFindBoxPopup>(() => popup))
			{
				var dummy = new DummyDocDataObjectWithFindBoxWithoutDescription();

				var dynamicData = dummy.MakeDocDataDynamic();
				var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));

				var metaData = code
					.GetMetaData()
					.FirstOrDefault(m => m.Id == CustomFindBoxMetaData.Identifier);

				AssertNotNull("CustomFindBox metadata found", metaData);

				if (metaData is CustomFindBoxMetaData customFindBoxMetaData)
				{
					Assert("Show Description should be true by default", !customFindBoxMetaData.ShowDescription);
				}
				else
				{
					Fail("Expected custom find box metadata to be of type CustomFindBoxMetaData");
				}
			}
		}

		#endregion

		public void TestDisableModifiableMetaData()
		{
			var popup = new DummyCustomFindBoxPopup();

			using (ObjectFactory.Substitute<DummyCustomFindBoxPopup>(() => popup))
			{
				var dummy = new DummyDocDataObject();

				var dynamicData = dummy.MakeDocDataDynamic();
				var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));

				var propertyInfo = code.GetMetaData<PropertyInfo>(MetaDataType.DeclaringProperty);
				AssertNotNull("DisableModifiableMemberAttribute found", propertyInfo?.GetCustomAttribute(typeof(DisableModifiableMemberAttribute)));

				var metaData = code
					.GetMetaData()
					.FirstOrDefault(m => m.Id == MetaDataTypes.DisableModifiable);
				AssertNull("DisableModifiable metadata not found", metaData);

				dummy.Code_DisableModifiable = true;
				metaData = code
					.GetMetaData()
					.FirstOrDefault(m => m.Id == MetaDataTypes.DisableModifiable);
				AssertNotNull("DisableModifiable metadata found", metaData);
			}
		}
	}
}
