using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI
{
	[TestedType(typeof(QualityIterationAssignmentControl))]
	public class QualityIterationAssignmentControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new QualityIterationAssignmentHeader();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			var control = (QualityIterationAssignmentControl)control1;
			return control.AssignmentGrid.ReadOnly;
		}
	}
}
