using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class IncidentGroupTypeCollection : RegistryBusinessObjectCollectionTemplate<IncidentGroupType>
	{
		public IncidentGroupTypeCollection() : base(null, null)
		{
		}

		public IncidentGroupTypeCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IncidentGroupType(CurrentFallbackLevel, CurrentFactory);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var setupCollection = ((IncidentGroupType)child).IncidentGroupStatusConfigurations;

			var inv = IncidentGroupStatusConfigurationConstants.ConfigurationINV;
			var esc = IncidentGroupStatusConfigurationConstants.ConfigurationESC;
			var aci = IncidentGroupStatusConfigurationConstants.ConfigurationACI;
			var psi = IncidentGroupStatusConfigurationConstants.ConfigurationPSI;
			var rsv = IncidentGroupStatusConfigurationConstants.ConfigurationRSV;

			setupCollection.AddNew(inv.Code, inv.DescriptionOnGroup, inv.TriggerOn, isSystem: true, sequence: inv.Sequence);
			setupCollection.AddNew(esc.Code, esc.DescriptionOnGroup, esc.TriggerOn, isSystem: true, sequence: esc.Sequence);
			setupCollection.AddNew(aci.Code, aci.DescriptionOnGroup, aci.TriggerOn, isSystem: true, sequence: aci.Sequence);
			setupCollection.AddNew(psi.Code, psi.DescriptionOnGroup, psi.TriggerOn, isSystem: true, sequence: psi.Sequence);
			setupCollection.AddNew(rsv.Code, rsv.DescriptionOnGroup, rsv.TriggerOn, isSystem: true, sequence: rsv.Sequence);
		}

		public IncidentGroupType AddNew(string groupType, string description, bool isSystem = false)
		{
			var newElement = base.AddNew();
			newElement.GroupType = groupType;
			newElement.Description = description;
			newElement.IsSystem = isSystem;

			return newElement;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IncidentGroupTypeCollection(fallbackLevel, factory);
		}

		public IEnumerable<IncidentGroupStatusConfiguration> GetAllStages(bool enableOnly = true)
		{
			var baseGroup = this.Cast<IncidentGroupType>().AsQueryable().SelectMany(x => x.IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>());
			if (enableOnly)
			{
				baseGroup = baseGroup.Where(x => x.Enabled);
			}
			return baseGroup.GroupBy(x => x.Code).Select(x => x.First());
		}

		public IncidentGroupStatusConfigurationCollection GetActiveStages(string type)
		{
			var stagesForType = this.Cast<IncidentGroupType>().FirstOrDefault(x => x.GroupType.EqualsIgnoringCase(type))?.IncidentGroupStatusConfigurations ?? new IncidentGroupStatusConfigurationCollection();
			return new IncidentGroupStatusConfigurationCollection(stagesForType.Cast<IncidentGroupStatusConfiguration>().Where(x => x.Enabled));
		}

		public IncidentGroupStatusConfigurationCollection GetAllThisTypeStages(string type)
		{
			var stagesForType = this.Cast<IncidentGroupType>().FirstOrDefault(x => x.GroupType.EqualsIgnoringCase(type))?.IncidentGroupStatusConfigurations ?? new IncidentGroupStatusConfigurationCollection();
			return new IncidentGroupStatusConfigurationCollection(stagesForType.Cast<IncidentGroupStatusConfiguration>());
		}
	}
}
