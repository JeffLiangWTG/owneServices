using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class ConsolBranchLocatorTest : BranchLocatorTest
	{
		protected override StronglyTypedRegistryItem<ImportBranchRule> ImportBranchRuleRegistryItem
		{
			get { return SystemDataRegistry.Instance.ConsolImportBranchRules; }
		}

		protected override BranchLocatorObjectWrapper GetTestObjectWrapper()
		{
			var consolXsd = new Xsd.Consol();
			consolXsd.ConsolDetail.PortOfDischarge = new Xsd.Movement() { Port = Xsd.UNLOCO.FromPortCode(Factory, "YEBLH") };
			consolXsd.ConsolDetail.PortOfLoading = new Xsd.Movement() { Port = Xsd.UNLOCO.FromPortCode(Factory, "ZAAMZ") };
			return new BranchLocatorObjectWrapper(new ValueObjectImportContext(Factory, new NotificationBuffer()), consolXsd);
		}

		protected override BranchLocatorObjectWrapper GetTestObjectWrapperWithEmptyPorts()
		{
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var consolXsd = new Xsd.Consol();
			return new BranchLocatorObjectWrapper(context, consolXsd);
		}
	}
}
