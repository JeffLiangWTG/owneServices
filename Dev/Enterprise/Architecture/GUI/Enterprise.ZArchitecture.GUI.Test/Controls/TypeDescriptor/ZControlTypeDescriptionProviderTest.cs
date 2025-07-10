using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZControlTypeDescriptionProviderTest : TestCase
	{
		public void TestGetTypeDescriptor()
		{
			var provider = new ZControlTypeDescriptionProvider();
			ICustomTypeDescriptor customTypeDescriptor;
			using (var control1 = new TestControl1())
			{
				customTypeDescriptor = provider.GetTypeDescriptor(typeof(TestControl1), control1);
			}
			AssertEquals("Should create a new ZControlTypeDescriptor for ZTextBox control type", typeof(ZControlTypeDescriptor), customTypeDescriptor.GetType());

			ICustomTypeDescriptor anotherCustomTypeDescriptor;
			using (var control11 = new TestControl1())
			{
				anotherCustomTypeDescriptor = provider.GetTypeDescriptor(typeof(TestControl1), control11);
			}
			AssertEquals("Should be retrieving it from the cache if already exist", customTypeDescriptor, anotherCustomTypeDescriptor);

			ICustomTypeDescriptor yetAnotherCustomTypeDescriptor;
			using (var control2 = new TestControl2())
			{
				yetAnotherCustomTypeDescriptor = provider.GetTypeDescriptor(typeof(TestControl2), control2);
			}
			AssertEquals("Should create a new ZControlTypeDescriptor for ZButton control type", typeof(ZControlTypeDescriptor), yetAnotherCustomTypeDescriptor.GetType());
			Assert("Should not be using TypeDescriptor for ZTextBox", customTypeDescriptor != yetAnotherCustomTypeDescriptor);
		}

		public void TestGetProperties_ReturnsCorrectList_WhenCalledFromMultipleThreads()
		{
			const int parallelCount = 5;
			const int runCount = 50;

			void FindUnknownProperty(PropertyDescriptorCollection pdc)
			{
				PropertyDescriptor result = null;

				AssertNoExceptionThrown(() => result = pdc.Find(nameof(TestControlWithPropertyDescriptors.MaxLength), false));
				AssertNotNull(result);
			}

			FindUnknownProperty(TypeDescriptor.GetProperties(typeof(TestControlWithPropertyDescriptors)));

			var defaultPropertyDescriptors = TestControlWithPropertyDescriptors.GetPropertyDescriptors();
			var expectedPropertyCount = defaultPropertyDescriptors.Length + 1;

			AssertEquals(expectedPropertyCount, TypeDescriptor.GetProperties(typeof(TestControlWithPropertyDescriptors)).Count);

			CombineAssertions(() =>
			{
				for (var i = 0; i < runCount; i++)
				{
					var descriptionProvider = new ZControlTypeDescriptionProvider();
					var providers = Enumerable.Repeat(descriptionProvider, parallelCount);

					Parallel.ForEach(providers, p =>
					{
						var pdc = TypeDescriptor.GetProperties(typeof(TestControlWithPropertyDescriptors));
						FindUnknownProperty(pdc);

						Assert(pdc.Cast<PropertyDescriptor>().All(pd => pd != null));
						AssertEquals(expectedPropertyCount, pdc.Count);
					});
				}
			});
		}

		public void TestGetTypeDescriptor_DesignerMode()
		{
			DesignModeFinder.SetIsDesigningForTest(true);
			try
			{
				var provider = new ZControlTypeDescriptionProvider();
				using (var control = new TestControl1())
				{
					var expectedTypeDescriptor = TypeDescriptor.GetProvider(typeof(Component)).GetTypeDescriptor(typeof(TestControl1), control).GetType();
					AssertEquals("Should return the base one if in designer mode", expectedTypeDescriptor, provider.GetTypeDescriptor(typeof(TestControl1), control).GetType());
					Assert("Should return the base one if in designer mode", expectedTypeDescriptor != typeof(ZControlTypeDescriptor));
				}
			}
			finally
			{
				DesignModeFinder.SetIsDesigningForTest(false);
			}
		}

		#region Test Classes

		[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
		class TestControl1 : Control
		{ }

		[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
		class TestControl2 : Control
		{ }

		[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
		class TestControlWithPropertyDescriptors : Control
		{
			[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
			[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength")]
			public override string Text { get; set; }

			public int MaxLength { get; set; }

			public static PropertyDescriptor[] GetPropertyDescriptors()
			{
				return new ControlPropertyDescriptorBuilder<ZDropEdit>()
					.Property("Text", "") // Property name
					.Property("ReadOnly", false)
					.Property("ReadOnlyForBinding", false, false)
					.Property("ReadOnlyForBindingIsNull", false, false)
					.Property("IsVisibleForBinding", ZBool.True)
					.Property("ShowDescriptionBox", false, false)
					.Property("ShowDescriptionInDropDown", false, false)
					.Result;
			}
		}

		#endregion
	}
}
