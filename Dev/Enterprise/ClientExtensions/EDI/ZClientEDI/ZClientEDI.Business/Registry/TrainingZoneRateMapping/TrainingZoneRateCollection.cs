using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class TrainingZoneRateCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new TrainingZoneRate this[int i]
		{
			get { return (TrainingZoneRate)Elements[i]; }
		}

		public new TrainingZoneRate AddNew()
		{
			return (TrainingZoneRate)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TrainingZoneRateCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TrainingZoneRate();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((TrainingZoneRate)child).SetParentCollection(this);
		}

		public TrainingZoneRate FindByZone(RefZoneHeader zone)
		{
			foreach (TrainingZoneRate zoneRate in this)
			{
				if (zoneRate.ZonePK == zone.PK)
				{
					return zoneRate;
				}
			}

			return null;
		}
	}
}

