using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class BillingSystemChargeCodeMappingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new BillingSystemChargeCodeMapping this[int index]
		{
			get { return (BillingSystemChargeCodeMapping)Elements[index]; }
		}

		#region New

		public new BillingSystemChargeCodeMapping AddNew()
		{
			return (BillingSystemChargeCodeMapping)base.AddNew();
		}

		public BillingSystemChargeCodeMapping AddNew(ZString productCode, ZString systemCode, ZString submodule)
		{
			return AddNew(productCode, systemCode, submodule, ZString.Empty, ZString.Empty);
		}

		public BillingSystemChargeCodeMapping AddNew(ZString productCode, ZString systemCode, ZString submodule, ZString description, ZString chargeCode)
		{
			var result = AddNew();
			using (result.GetValidationSuspender())
			{
				result.ProductCode = productCode;
				result.SystemCode = systemCode;
				result.SubModule = submodule;
				result.Description = description;
				result.ChargeCode = chargeCode;
			}

			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BillingSystemChargeCodeMapping();
		}

		#endregion

		#region GetSystems

		public IEnumerable<ZString> GetSystemCodes()
		{
			return
				(from BillingSystemChargeCodeMapping mapping in this
				group mapping by mapping.ProductCode into g
				select g).SelectMany(g => g.Select(x => x.SystemCode).Where(x => !x.IsEmpty).Distinct());
		}

		public IEnumerable<ZString> GetSystemCodes(ZString product)
		{
			return
				(from BillingSystemChargeCodeMapping mapping in this
				where
					mapping.ProductCode == product &&
					!mapping.SystemCode.IsEmpty
				select mapping.SystemCode).Distinct();
		}

		#endregion

		#region GetSubModules

		public IEnumerable<ZString> GetSubModuleCodes()
		{
			return
				from BillingSystemChargeCodeMapping mapping in this
				where !mapping.SubModule.IsEmpty
				select mapping.SubModule;
		}

		internal IEnumerable<BillingSystemChargeCodeMapping> GetSubModuleMappings(ZString product, ZString system)
		{
			return
				from BillingSystemChargeCodeMapping mapping in this
				where
					mapping.ProductCode == product &&
					mapping.SystemCode == system &&
					!mapping.SubModule.IsEmpty
				select mapping;
		}

		public IEnumerable<ZString> GetSubModuleCodes(ZString product, ZString system)
		{
			return GetSubModuleMappings(product, system).Select(mapping => mapping.SubModule);
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BillingSystemChargeCodeMappingCollection();
		}

		#endregion
	}
}

