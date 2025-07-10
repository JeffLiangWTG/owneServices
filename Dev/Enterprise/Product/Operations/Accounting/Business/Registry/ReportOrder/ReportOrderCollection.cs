using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ReportOrderCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ReportOrderCollection()
		{
		}

		public ReportOrderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new ReportOrder this[int x]
		{
			get { return (ReportOrder)Elements[x]; }
		}

		public new ReportOrder AddNew()
		{
			return (ReportOrder)base.AddNew();
		}

		public ReportOrder FindByLanguageAndCountryCode(string language, string countryCode)
		{
			return this.Cast<ReportOrder>().FirstOrDefault(reportOrder => reportOrder.Language == language && reportOrder.CountryCode == countryCode);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ReportOrderCollection(factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ReportOrder(CurrentFactory);
		}
	}
}
