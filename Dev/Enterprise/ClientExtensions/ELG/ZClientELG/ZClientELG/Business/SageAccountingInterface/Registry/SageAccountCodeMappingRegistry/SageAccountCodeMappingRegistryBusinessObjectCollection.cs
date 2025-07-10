using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.ELG
{
	[XmlSerializerAssembly("ZClientELG.XmlSerializers")]
	public class SageAccountCodeMappingRegistryBusinessObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new SageAccountCodeMappingRegistryBusinessObject this[int i]
		{
			get { return (SageAccountCodeMappingRegistryBusinessObject)Elements[i]; }
		}

		public new SageAccountCodeMappingRegistryBusinessObject AddNew()
		{
			return (SageAccountCodeMappingRegistryBusinessObject)base.AddNew();
		}

		public ZString FindSageAccountCode(ZString orgHeaderCode, ZString currencyCode, ZString ledgerType)
		{
			SageAccountCodeMappingRegistryBusinessObject element = FindElement(orgHeaderCode, currencyCode, ledgerType);
			return element != null ? element.SageAccountCode : ZString.Empty;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SageAccountCodeMappingRegistryBusinessObjectCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SageAccountCodeMappingRegistryBusinessObject();
		}

		SageAccountCodeMappingRegistryBusinessObject FindElement(ZString orgHeaderCode, ZString refCurrencyCode, ZString ledgerType)
		{
			SageAccountCodeMappingRegistryBusinessObject result = null;
			if (orgHeaderCode.IsEmpty || refCurrencyCode.IsEmpty || ledgerType.IsEmpty)
			{
				return result;
			}

			foreach (SageAccountCodeMappingRegistryBusinessObject element in Elements)
			{
				if (CurrentOrgHeaderCode(element.CurrentOrgHeader) == orgHeaderCode && CurrentRefCurrencyCode(element.CurrentRefCurrency) == refCurrencyCode && element.LedgerType == ledgerType)
				{
					result = element;
					break;
				}
			}
			return result;
		}

		static ZString CurrentOrgHeaderCode(OrgHeader currentOrgHeader)
		{
			return currentOrgHeader != null ? currentOrgHeader.OH_Code : ZString.Empty;
		}

		static ZString CurrentRefCurrencyCode(RefCurrency currentRefCurrency)
		{
			return currentRefCurrency != null ? currentRefCurrency.RX_Code : ZString.Empty;
		}
	}
}
