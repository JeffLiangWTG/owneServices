using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI.Testing
{
	class MockMasterObject : ComponentModel.Testing.KComponentWithPropertyChange, IDataSourceEvents
	{
		public abstract class Properties
		{
			[ThreadSafe]
			public static readonly KPropertyDescriptor StringProperty = (KPropertyDescriptor)PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(MockMasterObject))["StringProperty"];
			[ThreadSafe]
			public static readonly KPropertyDescriptor StringProperty2 = (KPropertyDescriptor)PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(MockMasterObject))["StringProperty2"];
			[ThreadSafe]
			public static readonly KPropertyDescriptor DetailObjects = (KPropertyDescriptor)PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(MockMasterObject))["DetailObjects"];
		}

		public MockDetailObjectCollection DetailObjects
		{
			get { return detailObjects ?? (detailObjects = new MockDetailObjectCollection(this)); }
		}
		MockDetailObjectCollection detailObjects;

		public MockRelatedObject RelatedObject { get; set; }

		[DefaultValue("DefaultValue")]
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

		#region StringProperty2

		[ReadOnlyMember(nameof(StringProperty2ReadOnly))]
		public string StringProperty2
		{
			get { return stringProperty2; }
			set
			{
				if (stringProperty2 != value)
				{
					stringProperty2 = value;
					FirePropertyChanged(nameof(StringProperty2));
					OnStringProperty2Changed(EventArgs.Empty);
				}
			}
		}
		string stringProperty2;
		public event EventHandler StringProperty2Changed;

		void OnStringProperty2Changed(EventArgs eventArgs)
		{
			if (StringProperty2Changed != null)
			{
				StringProperty2Changed(this, EventArgs.Empty);
			}
		}

		#endregion

		#region StringProperty2ReadOnly

		public bool StringProperty2ReadOnly
		{
			get { return stringProperty2ReadOnly; }
			set
			{
				stringProperty2ReadOnly = value;
				FirePropertyChanged(nameof(StringProperty2ReadOnly));
			}
		}
		bool stringProperty2ReadOnly;

		#endregion

		#region DataSourcePosting

		public event EventHandler DataSourcePosting;

		public void FireDataSourcePosting()
		{
			if (DataSourcePosting != null)
			{
				DataSourcePosting(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}
