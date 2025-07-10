using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobProfitLossReasonCodeCollection : RegistryBusinessObjectCollection
	{
		public new JobProfitLossReasonCode this[int i]
		{
			get { return (JobProfitLossReasonCode)Elements[i]; }
		}

		public new JobProfitLossReasonCode AddNew()
		{
			return (JobProfitLossReasonCode)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobProfitLossReasonCodeCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new JobProfitLossReasonCode();
		}
	}
}
