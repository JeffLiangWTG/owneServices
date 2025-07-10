using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class GLJournalApprovalThresholdCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new GLJournalApprovalThreshold this[int i]
		{
			get { return (GLJournalApprovalThreshold)base[i]; }
		}

		public new GLJournalApprovalThreshold AddNew()
		{
			return (GLJournalApprovalThreshold)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GLJournalApprovalThresholdCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GLJournalApprovalThreshold();
		}
	}
}