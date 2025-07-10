using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(EInvoicingReceivingFileTypeConfigurationControl))]
	public class EInvoicingReceivingFileTypeConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new EInvoicingReceivingFileTypeConfigurationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((EInvoicingReceivingFileTypeConfigurationControl)control).EInvoicingReceivingFileTypeConfigurationGrid.ReadOnly;
		}
	}
}
