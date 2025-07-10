using System;
using Enterprise.Integration;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.CommissionPeriodListRegistryItemEditor, Enterprise.Registry.GUI")]
	public class CommissionPeriodListRegistryEditorInfo : IRegistryEditorInfo
	{
		public CommissionPeriodListRegistryEditorInfo()
		{
		}

		#region IRegistryEditorInfo Members

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(CommissionPeriodCollection); }
		}

		#endregion
	}
}
