using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Test.RegistryItemEditors.AirlineMessagingCargoIMPVersion
{
	[TestedType(typeof(CargoImpVersionControl))]

	sealed class CargoImpVersionControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CargoImpVersionConfiguration();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var gird = control.Controls.Find("AirlineImpVersionGrid", true).Single() as ZGrid;
			var dropEdit = control.Controls.Find("defaultVersionDropEdit", true).Single() as ZDropEdit;

			return gird.ReadOnly && dropEdit.ReadOnly;
		}
	}
}
