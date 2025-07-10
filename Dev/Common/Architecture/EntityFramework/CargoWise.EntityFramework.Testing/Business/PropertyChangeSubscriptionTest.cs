using System.Collections.Generic;

namespace CargoWise.EntityFramework.Testing
{
	sealed class PropertyChangeSubscriptionTest : TestCaseWithDummy
	{
		public void TestPropertyChanged()
		{
			PropertyChangeSubscription.PropertyChanged += new ZPropertyValueChangedEventHandler(PropertyChangeSubscription_PropertyChanged);
			try
			{
				Dummy.Z0_VarCharMax = "newValue";
				AssertEquals("Property value has changed", Dummy.Z0_VarCharMaxInfo, propertiesChanged[0]);
			}
			finally
			{
				PropertyChangeSubscription.PropertyChanged -= new ZPropertyValueChangedEventHandler(PropertyChangeSubscription_PropertyChanged);
			}
		}

		#region Implementation

		readonly List<ZPropertyInfo> propertiesChanged = new List<ZPropertyInfo>();

		void PropertyChangeSubscription_PropertyChanged(object sender, ZPropertyValueChangedEventArgs e)
		{
			propertiesChanged.Add(e.Property);
		}

		#endregion
	}
}
