using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(IncidentClosureDispositionControl))]
	public class IncidentClosureDispositionControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new IncidentClosureDispositionCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((IncidentClosureDispositionControl)control).IsControlOrBusinessEntityReadOnly;
		}

		public void TestControlsSetToReadOnly()
		{
			using (var form = new ZForm())
			using (var incidentClosureDispositionControlForTest = new IncidentClosureDispositionControlForTest())
			{
				form.Controls.Add(incidentClosureDispositionControlForTest);
				form.Show();

				AssertEquals(4, incidentClosureDispositionControlForTest.Grids.Count);
				incidentClosureDispositionControlForTest.SetControlOrBusinessEntityReadOnly(true);
				incidentClosureDispositionControlForTest.Grids.ForEach(grid => AssertEquals(true, grid.ReadOnly));

				incidentClosureDispositionControlForTest.SetControlOrBusinessEntityReadOnly(false);
				incidentClosureDispositionControlForTest.Grids.ForEach(grid => AssertEquals(false, grid.ReadOnly));
			}
		}
	}

	public class IncidentClosureDispositionControlForTest : IncidentClosureDispositionControl
	{
		public new void SetControlOrBusinessEntityReadOnly(bool readOnly) => base.SetControlOrBusinessEntityReadOnly(readOnly);

		public new List<IncidentClosureDispositionGridControl> Grids => base.Grids;
	}
}
