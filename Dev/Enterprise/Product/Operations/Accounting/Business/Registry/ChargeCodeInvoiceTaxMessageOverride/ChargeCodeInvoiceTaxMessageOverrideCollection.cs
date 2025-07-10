using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ChargeCodeInvoiceTaxMessageOverrideCollection : RegistryBusinessObjectCollectionTemplate
	{
		#region Constructors

		public ChargeCodeInvoiceTaxMessageOverrideCollection()
		{
		}

		public ChargeCodeInvoiceTaxMessageOverrideCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		public ChargeCodeInvoiceTaxMessageOverride GetOverride(ZGuid chargeCodePK, ZGuid invoiceTaxMessagePK)
		{
			ChargeCodeInvoiceTaxMessageOverride result = null;
			foreach (ChargeCodeInvoiceTaxMessageOverride messageOverride in this)
			{
				if (messageOverride.ChargeCode == chargeCodePK && messageOverride.TaxMessage == invoiceTaxMessagePK)
				{
					result = messageOverride;
					break;
				}
			}
			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeCodeInvoiceTaxMessageOverrideCollection(fallbackLevel, CurrentFactory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ChargeCodeInvoiceTaxMessageOverride(CurrentFallbackLevel, CurrentFactory);
		}

		public new ChargeCodeInvoiceTaxMessageOverride this[int index]
		{
			get { return (ChargeCodeInvoiceTaxMessageOverride)Elements[index]; }
		}

		public new ChargeCodeInvoiceTaxMessageOverride AddNew()
		{
			return (ChargeCodeInvoiceTaxMessageOverride)base.AddNew();
		}
	}
}