using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RatesPrioritiesControl))]
	sealed class RatesPrioritiesControlTest : RegistryZUserControlTestCase
	{
		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return new RatesPrioritiesCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((RatesPrioritiesControl)control).RatesPriorityGrid.ReadOnly;
		}

		public void TestJobTypeColumn()
		{
			CombineAssertions("Always show JobType column", () =>
			{
				using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (var control = new RatesPrioritiesControl())
				{
					AssertEquals
					(
						"Registry disabled, show column",
						true,
						control.RatesPriorityGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any((c) => c.ColumnName == "JobType" && !c.IsUnavailable)
					);
				}

				using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var control = new RatesPrioritiesControl())
				{
					AssertEquals
					(
						"Registry enabled, show column",
						true,
						control.RatesPriorityGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any((c) => c.ColumnName == "JobType" && !c.IsUnavailable)
					);
				}
			});
		}
	}
}
