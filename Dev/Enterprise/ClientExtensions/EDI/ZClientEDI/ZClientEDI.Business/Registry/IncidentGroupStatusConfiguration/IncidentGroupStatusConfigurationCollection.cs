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
	public class IncidentGroupStatusConfigurationCollection : RegistryBusinessObjectCollectionTemplate<IncidentGroupStatusConfiguration>
	{
		public IncidentGroupStatusConfigurationCollection() : this(null, null)
		{
		}

		public IncidentGroupStatusConfigurationCollection(IEnumerable<IncidentGroupStatusConfiguration> stages) : this(null, null)
		{
			foreach (var stage in stages)
			{
				Add(stage);
			}
		}

		public IncidentGroupStatusConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public List<BusinessObject> RemovedBusinessObjList = new ();

		public string IncidentGroupTypeCode { set; get; } = string.Empty;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IncidentGroupStatusConfiguration(CurrentFallbackLevel, CurrentFactory);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var newElement = (IncidentGroupStatusConfiguration)child;
			newElement.Sequence = GetNextSequenceNumber();
			newElement.ParentCode = IncidentGroupTypeCode;
		}

		ZInt GetNextSequenceNumber()
		{
			var nextSequence = new ZInt(10);
			if (this.Count != 0)
			{
				nextSequence = this.OfType<IncidentGroupStatusConfiguration>().OrderBy(x => x.Sequence).LastOrDefault().Sequence + 10;
			}
			return nextSequence;
		}

		public IncidentGroupStatusConfiguration AddNew(
			string code,
			string descriptionOnGroup,
			string triggerOn,
			bool approvalGate = false,
			bool controlIncidents = false,
			bool cascadeCriticality = false,
			bool cascadeProductDetails = false,
			bool groupCompleted = false,
			bool incidentCompleted = false,
			bool enabled = true,
			bool isSystem = false,
			int? sequence = null)
		{
			var newElement = base.AddNew();
			newElement.Code = code;
			newElement.DescriptionOnGroup = descriptionOnGroup;
			newElement.ApprovalGate = approvalGate;
			newElement.ControlIncidents = controlIncidents;
			newElement.CascadeCriticality = cascadeCriticality;
			newElement.CascadeProductDetails = cascadeProductDetails;
			newElement.GroupCompleted = groupCompleted;
			newElement.IncidentCompleted = incidentCompleted;
			newElement.TriggerOn = triggerOn;
			newElement.Enabled = enabled;
			newElement.IsSystem = isSystem;

			if (sequence.HasValue)
			{
				newElement.Sequence = sequence.Value;
			}

			return newElement;
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			var element = (IncidentGroupStatusConfiguration)businessObject;
			element.CurrentFallbackLevel = CurrentFallbackLevel;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IncidentGroupStatusConfigurationCollection(fallbackLevel, factory);
		}

		public ZInt GetSequenceByCode(ZString code)
		{
			var status = this.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == code);
			return status?.Sequence ?? 0;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			RemovedBusinessObjList.Add(bizO);
		}
	}
}
