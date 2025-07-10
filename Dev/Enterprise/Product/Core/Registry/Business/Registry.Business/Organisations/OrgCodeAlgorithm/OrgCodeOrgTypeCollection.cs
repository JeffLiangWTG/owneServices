using System.Xml.Serialization;

using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;

using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OrgCodeOrgTypeCollection : RegistryBusinessObjectCollectionTemplate
	{
		readonly OrgCodeAlgorithm parent;

		public OrgCodeOrgTypeCollection()
		{
		}

		public OrgCodeOrgTypeCollection(OrgCodeAlgorithm parent)
		{
			this.parent = parent;
		}

		public bool DefaultAlgorithm
		{
			get { return (Parent != null) && (Parent.AlgorithmType == OrgCodeAlgorithmType.Default); }
		}

		public OrgCodeAlgorithm Parent
		{
			get { return parent; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		public new OrgCodeOrgType this[int index]
		{
			get { return (OrgCodeOrgType)base[index]; }
		}

		public OrgCodeOrgType this[ZString description]
		{
			get
			{
				foreach (OrgCodeOrgType element in this)
				{
					if (element.Description == description)
					{
						return element;
					}
				}
				return null;
			}
		}

		public new OrgCodeOrgType AddNew()
		{
			return (OrgCodeOrgType)base.AddNew();
		}

		public void CopyElementValuesFrom(OrgCodeOrgTypeCollection collection)
		{
			if (!DefaultAlgorithm)
			{
				foreach (OrgCodeOrgType sourceElement in collection)
				{
					OrgCodeOrgType targetElement = this[sourceElement.Description];
					if (targetElement != null)
					{
						targetElement.Selected = sourceElement.Selected;
					}
				}
			}
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgCodeOrgTypeCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgCodeOrgType();
		}

		public override void Load(ZQuery filter)
		{
			RemoveAndDeleteAll();
			Add(new OrgCodeOrgType(OrgCodeOrgTypeDescription.Receivables));
			Add(new OrgCodeOrgType(OrgCodeOrgTypeDescription.Payables));
			Add(new OrgCodeOrgType(OrgCodeOrgTypeDescription.Consignor));
			Add(new OrgCodeOrgType(OrgCodeOrgTypeDescription.Consignee));
			Add(new OrgCodeOrgType(OrgCodeOrgTypeDescription.TransportClient));
			Add(new OrgCodeOrgType(OrgCodeOrgTypeDescription.Warehouse));
			Add(new OrgCodeOrgType(OrgCodeOrgTypeDescription.Carrier));
			Add(new OrgCodeOrgType(OrgCodeOrgTypeDescription.Forwarder));
			Add(new OrgCodeOrgType(OrgCodeOrgTypeDescription.Broker));
			Add(new OrgCodeOrgType(OrgCodeOrgTypeDescription.Services));
			Add(new OrgCodeOrgType(OrgCodeOrgTypeDescription.Competitor));
			Add(new OrgCodeOrgType(OrgCodeOrgTypeDescription.Sales));

			if (DefaultAlgorithm)
			{
				OrgCodeAlgorithm otherAlgorithm = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmOverride.Value;
				foreach (OrgCodeOrgType otherElement in otherAlgorithm.SelectableOrgTypes)
				{
					OrgCodeOrgType element = this[otherElement.Description];
					element.Selected = !otherElement.Selected;
				}
			}
		}
	}
}
