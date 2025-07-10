using System.Xml.Serialization;

using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;

using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OrgCodeElementCollection : RegistryBusinessObjectCollectionTemplate
	{
		readonly OrgCodeAlgorithm parent;

		public OrgCodeElementCollection()
		{
		}

		public OrgCodeElementCollection(OrgCodeAlgorithm parent)
		{
			this.parent = parent;
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

		public OrgCodeAlgorithm Parent
		{
			get { return parent; }
		}

		public new OrgCodeElement this[int index]
		{
			get { return (OrgCodeElement)base[index]; }
		}

		public OrgCodeElement this[ZString description]
		{
			get
			{
				foreach (OrgCodeElement element in this)
				{
					if (element.Description == description)
					{
						return element;
					}
				}
				return null;
			}
		}

		public new OrgCodeElement AddNew()
		{
			return (OrgCodeElement)base.AddNew();
		}

		public void CopyElementValuesFrom(OrgCodeElementCollection collection)
		{
			foreach (OrgCodeElement sourceElement in collection)
			{
				OrgCodeElement targetElement = this[sourceElement.Description];
				if (targetElement != null)
				{
					targetElement.Length = sourceElement.Length;
					targetElement.Order = sourceElement.Order;
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgCodeElement();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgCodeElementCollection();
		}

		public override void Load(ZQuery filter)
		{
			RemoveAndDeleteAll();
			Add(new OrgCodeElement(OrgCodeElementDescription.FirstName));
			Add(new OrgCodeElement(OrgCodeElementDescription.SecondName));
			Add(new OrgCodeElement(OrgCodeElementDescription.LastName));
			Add(new OrgCodeElement(OrgCodeElementDescription.CountryCode, (ZByte)RefCountrySchema.RN_Code.MaxLength));
			Add(new OrgCodeElement(OrgCodeElementDescription.UnlocoCode, (ZByte)RefUNLOCOSchema.RL_Code.MaxLength));
			Add(new OrgCodeElement(OrgCodeElementDescription.IataCode, (ZByte)RefUNLOCOSchema.RL_IATA.MaxLength));
			Add(new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber));
			Add(new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber));
		}
	}
}
