using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EntryChargeTypeSettingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public EntryChargeTypeSettingCollection()
			: base()
		{
		}

		public EntryChargeTypeSettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new EntryChargeTypeSetting this[int i] => (EntryChargeTypeSetting)Elements[i];

		public new EntryChargeTypeSetting AddNew()
		{
			return (EntryChargeTypeSetting)base.AddNew();
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			var chargeType = bizOAdded as EntryChargeTypeSetting;
			if (chargeType != null)
			{
				chargeType.SetParentCollection(this);
			}
		}

		public EntryChargeTypeSetting FindByCode(string code)
		{
			foreach (EntryChargeTypeSetting element in this)
			{
				if (element.ChargeType == code)
				{
					return element;
				}
			}
			return null;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EntryChargeTypeSettingCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EntryChargeTypeSetting(CurrentFallbackLevel, CurrentFactory, this);
		}
	}
}
