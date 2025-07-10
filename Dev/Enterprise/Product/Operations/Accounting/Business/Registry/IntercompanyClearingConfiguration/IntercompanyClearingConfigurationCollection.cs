using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class IntercompanyClearingConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new IntercompanyClearingConfiguration this[int x]
		{
			get { return (IntercompanyClearingConfiguration)base[x]; }
		}

		public new IntercompanyClearingConfiguration AddNew()
		{
			return (IntercompanyClearingConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IntercompanyClearingConfigurationCollection();
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				return false;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IntercompanyClearingConfiguration();
		}

		public void AddDefaultValues(Guid companyPK)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			Guid demoCompanyGuid = new Guid("03052ED3-2C64-49AC-97D8-C6079D5015B5");
			ZQuery companyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, demoCompanyGuid);
			BusinessObject[] companies = factory.Load<GlbCompany>(companyQuery);
			foreach (BusinessObject company in companies)
			{
				Add(new IntercompanyClearingConfiguration(new ZString(company[GlbCompanySchema.GC_Code])));
			}
		}
	}
}