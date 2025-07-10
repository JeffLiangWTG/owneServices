using System;
using System.ComponentModel;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.Design
{
	sealed class AvailableColumnItemTest : TestCaseWithFactory
	{
		public void TestAvailableColumnStyles()
		{
			AssertContainsExactElementsInAnyOrder(new[] {
						typeof(ZTextBoxColumnStyleInfo),
						typeof(ZCalcEditColumnStyleInfo),
						typeof(ZCheckBoxColumnStyleInfo),
						typeof(ZCodeFindBoxColumnStyleInfo),
						typeof(ZDateEditColumnStyleInfo),
						typeof(ZDateTimeOffsetEditColumnStyleInfo),
						typeof(ZDropEditColumnStyleInfo),
						typeof(ZDynamicMultilineTextBoxColumnStyleInfo),
						typeof(ZGuidDropEditColumnStyleInfo),
						typeof(ZGuidFindBoxColumnStyleInfo),
						typeof(ZMultiControlColumnStyleInfo),
						typeof(ZMultiLineTextBoxColumnInfo),
						typeof(ZTimeEditExColumnStyleInfo),
						typeof(ZCodeFindBoxWithSelectedEventColumnStyleInfo),
						ObjectFactory.GetTypeDesignerSafe("ZOrganisationFindBoxColumnStyleInfo"),
						ObjectFactory.GetTypeDesignerSafe<IZAddressDropEditColumnStyleInfo>(),
				}, new EmptyColumnItem().AvailableColumnStyles);
		}

		public void TestPopulateColumnStyles()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			AssertColumnStyleForType(dummy.Z0_NumberInfo, typeof(ZCalcEditColumnStyleInfo));
			AssertColumnStyleForType(dummy.Z0_DecimalInfo, typeof(ZCalcEditColumnStyleInfo));
			AssertColumnStyleForType(dummy.Z0_ShortInfo, typeof(ZCalcEditColumnStyleInfo));
			AssertColumnStyleForType(dummy.Z0_ByteInfo, typeof(ZCalcEditColumnStyleInfo));
			AssertColumnStyleForType(dummy.Z0_NVarCharInfo, typeof(ZTextBoxColumnStyleInfo), typeof(ZCodeFindBoxColumnStyleInfo), typeof(ZDropEditColumnStyleInfo), typeof(ZMultiLineTextBoxColumnInfo), typeof(ZMultiControlColumnStyleInfo), typeof(ZDynamicMultilineTextBoxColumnStyleInfo));
			AssertColumnStyleForType(dummy.Z0_DateInfo, typeof(ZDateEditColumnStyleInfo), typeof(ZTimeEditExColumnStyleInfo));
			AssertColumnStyleForType(dummy.Z0_DateTimeOffsetInfo, typeof(ZDateTimeOffsetEditColumnStyleInfo));
		}

		void AssertColumnStyleForType(ZPropertyInfo info, params Type[] expectedColumnStypes)
		{
			var item = new AvailableColumnItem(info.PropertyDescriptor);
			AssertContainsExactElementsInAnyOrder(string.Format(CultureInfo.InvariantCulture, "Broken for type: {0}", info.Name), expectedColumnStypes, item.AvailableColumnStyles);
		}

		public void TestCaption()
		{
			var columnItem = new AvailableColumnItem(new PropertyDescriptorForTesting("AB_ThisIsThePropertyName"));
			AssertEquals("Caption [AB_ThisIsThePropertyName]", "This Is The Property Name", columnItem.Caption);

			columnItem = new AvailableColumnItem(new PropertyDescriptorForTesting("ABC_ThisIsAnotherPropertyName"));
			AssertEquals("Caption [ABC_ThisIsAnotherPropertyName]", "This Is Another Property Name", columnItem.Caption);
		}

		class PropertyDescriptorForTesting : PropertyDescriptor
		{
			public PropertyDescriptorForTesting(string name)
				: base(name, Array.Empty<Attribute>())
			{
			}

			public override bool CanResetValue(object component)
			{
				throw new NotImplementedException();
			}

			public override Type ComponentType
			{
				get { throw new NotImplementedException(); }
			}

			public override object GetValue(object component)
			{
				throw new NotImplementedException();
			}

			public override bool IsReadOnly
			{
				get { throw new NotImplementedException(); }
			}

			public override Type PropertyType
			{
				get { throw new NotImplementedException(); }
			}

			public override void ResetValue(object component)
			{
				throw new NotImplementedException();
			}

			public override void SetValue(object component, object value)
			{
				throw new NotImplementedException();
			}

			public override bool ShouldSerializeValue(object component)
			{
				throw new NotImplementedException();
			}
		}
	}
}
