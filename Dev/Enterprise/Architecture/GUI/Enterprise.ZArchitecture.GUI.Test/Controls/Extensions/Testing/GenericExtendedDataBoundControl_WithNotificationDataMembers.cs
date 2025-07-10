using System;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions.Testing
{
	sealed class GenericExtendedDataBoundControl_WithNotificationDataMembers : GenericExtendedControl, INotificationDataMembers, IDataBoundControl
	{
		public GenericExtendedDataBoundControl_WithNotificationDataMembers(IControlExtensionCollection extensions)
			: base(extensions)
		{
		}

		public GenericExtendedDataBoundControl_WithNotificationDataMembers(string[] propertyMembers, object dataSource, string dataMember)
		{
			this.propertyMembers = propertyMembers;
			this.DataSource = dataSource;
			this.DataMember = dataMember;
		}

		#region INotificationBindingMembers Members

		readonly string[] propertyMembers;

		public string[] NotificationDataMembers
		{
			get { return propertyMembers; }
		}

		#endregion

		#region IDataBoundControl Members

		public string DataMember { get; private set; }
		public object DataSource { get; private set; }

		Type IDataBoundControl.DataSourceType
		{
			get { return typeof(object); }
		}

		void IDataBoundControl.SetDataBinding(object dataSource, string dataMember)
		{
		}

		#endregion
	}
}
