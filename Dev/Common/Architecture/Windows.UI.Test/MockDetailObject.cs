using CargoWise.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI.Testing
{
	sealed class MockDetailObject : ComponentModel.Testing.KComponentWithPropertyChange
	{
		public abstract class Properties
		{
			[ThreadSafe]
			public static readonly KPropertyDescriptor StringProperty = (KPropertyDescriptor)PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(MockDetailObject))["StringProperty"];
		}

		public string StringProperty
		{
			get { return stringProperty; }
			set
			{
				if (stringProperty != value)
				{
					stringProperty = value;
					FirePropertyChanged(nameof(StringProperty));
				}
			}
		}
		string stringProperty;

		public MockDetailDetailObjectCollection DetailDetailObjects
		{
			get { return detailDetailObjects; }
		}
		readonly MockDetailDetailObjectCollection detailDetailObjects = new MockDetailDetailObjectCollection();

		public MockMasterObject Parent { get; set; }
	}
}
