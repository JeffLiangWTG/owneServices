using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(CASSFileImportDefaultTaxIDControl))]
	public class CASSFileImportDefaultTaxIDControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CASSFileImportDefaultTaxID();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CASSFileImportDefaultTaxIDControl)control).StandardRatedTaxIDGuidFindBox_ForTestOnly.ReadOnly && ((CASSFileImportDefaultTaxIDControl)control).ZeroRatedTaxIDGuidFindBox_ForTestOnly.ReadOnly;
		}
	}
}
