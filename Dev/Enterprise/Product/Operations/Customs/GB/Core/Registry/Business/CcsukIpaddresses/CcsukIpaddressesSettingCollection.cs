using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class CcsukIpAddressesSettingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CcsukIpAddressesSettingCollection()
			: base()
		{
		}

		public CcsukIpAddressesSettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new CcsukIpaddressesSetting this[int i]
		{
			get { return (CcsukIpaddressesSetting)Elements[i]; }
		}

		public new CcsukIpaddressesSetting AddNew()
		{
			return (CcsukIpaddressesSetting)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CcsukIpAddressesSettingCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CcsukIpaddressesSetting(CurrentFallbackLevel, CurrentFactory);
		}

		public string AllAsString()
		{
			var sb = new ZStringBuilder();
			foreach (CcsukIpaddressesSetting pair in this)
			{
				sb.Append("Local=" + pair.LocalIpAddress + "	Participant=" + pair.CcsukParticipantIpAddress);
			}
			return sb.ToStringWithNewLineBetweenAppends();
		}
	}
}
