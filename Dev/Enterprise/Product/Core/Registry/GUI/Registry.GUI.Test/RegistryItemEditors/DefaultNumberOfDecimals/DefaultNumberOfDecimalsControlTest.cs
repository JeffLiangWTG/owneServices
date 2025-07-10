using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DefaultNumberOfDecimalsControl))]
	sealed class DefaultNumberOfDecimalsControlTest : RegistryZUserControlTestCase
	{
		public void TestTransportModeColumn()
		{
			using (var control = new DefaultNumberOfDecimalsControl(true))
			{
				AssertEquals(false, control.DefaultNumberOfDecimalsGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any((c) => c.ColumnName == "TransportMode"
						&& c.IsUnavailable));
			}

			using (var control = new DefaultNumberOfDecimalsControl(false))
			{
				AssertEquals(true, control.DefaultNumberOfDecimalsGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any((c) => c.ColumnName == "TransportMode"
						&& c.IsUnavailable));
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new DefaultNumberOfDecimalsCollection(Module.Freight);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DefaultNumberOfDecimalsControl)control).DefaultNumberOfDecimalsGrid.ReadOnly;
		}
	}
}
