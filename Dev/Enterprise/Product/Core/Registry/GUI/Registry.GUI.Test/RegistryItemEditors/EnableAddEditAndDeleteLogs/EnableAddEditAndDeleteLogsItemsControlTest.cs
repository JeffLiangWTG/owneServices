#if WINZOR
using CargoWise.Common.Testing;
#endif
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EnableAddEditAndDeleteLogsItemsControl))]
	sealed class EnableAddEditAndDeleteLogsItemsControlTest : RegistryZUserControlTestCase
	{
		#region Implementation

		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return new EnableAddEditAndDeleteLogsItemCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((EnableAddEditAndDeleteLogsItemsControl)control).EnableAddEditAndDeleteLogsItemsGrid.ReadOnly;
		}

#if WINZOR
		protected override void MasterSetUp()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;
			base.MasterSetUp();
		}
#endif

		#endregion
	}
}
