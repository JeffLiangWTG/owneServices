using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class DraftTransactionStatusReasonCodeCollection : CodeDescriptionCollection<DraftTransactionStatusReasonCode, ZBool>
	{
		public DraftTransactionStatusReasonCodeCollection()
			: this(null, null, default)
		{
		}

		public DraftTransactionStatusReasonCodeCollection(
			FallbackLevel fallbackLevel,
			ReadOnlyCodeDescriptionPairList list,
			ZBool defaultValueForNewChild)
			: base(fallbackLevel, null, list, defaultValueForNewChild, 3)
		{
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DraftTransactionStatusReasonCodeCollection(fallbackLevel, null, DefaultValueForNewChild);
		}
	}
}
