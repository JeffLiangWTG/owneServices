using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(ComplianceDocumentRePrintRestrictionControl))]
	class ComplianceDocumentRePrintRestrictionControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ComplianceDocumentRePrintRestrictionCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ComplianceDocumentRePrintRestrictionControl)control).ComplianceDocumentRePrintRestrictionGrid_ForTestOnly.ReadOnly;
		}
	}
}
