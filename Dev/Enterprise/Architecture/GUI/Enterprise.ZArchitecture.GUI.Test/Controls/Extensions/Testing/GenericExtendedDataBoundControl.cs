using System;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions.Testing
{
	sealed class GenericExtendedDataBoundControl : GenericExtendedControl, IDataBoundControl
	{
		public GenericExtendedDataBoundControl(IControlExtensionCollection extensions)
			: base(extensions)
		{
		}

		public GenericExtendedDataBoundControl(object dataSource, string dataMember)
		{
			this.DataSource = dataSource;
			this.DataMember = dataMember;
		}

		#region IDataBoundControl Members

		public object DataSource { get; private set; }
		public string DataMember { get; private set; }

		Type IDataBoundControl.DataSourceType
		{
			get { throw new NotImplementedException(); }
		}

		void IDataBoundControl.SetDataBinding(object dataSource, string dataMember)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
