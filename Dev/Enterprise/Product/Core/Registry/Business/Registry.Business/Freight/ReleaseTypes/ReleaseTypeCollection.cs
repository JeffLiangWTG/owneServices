using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot("Types")]
	public class ReleaseTypeCollection : RegistryBusinessObjectCollection
	{
		public ReleaseTypeCollection()
		{
		}

		public ReleaseTypeCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list)
		{
			foreach (ReleaseType element in this)
			{
				element.SystemDefined = true;
			}
		}

		public ReleaseTypeCollection SystemDefinedReleaseTypeCollection
		{
			set
			{
				foreach (ReleaseType defaultElement in value)
				{
					ReleaseType element = this.FindByCode(defaultElement.Code);
					if (element != null)
					{
						element.Description = defaultElement.Description;
						element.SystemDefined = true;
					}
					else
					{
						defaultElement.SystemDefined = true;
						Add(defaultElement);
					}
				}
			}
		}

		public ReleaseTypes Parent
		{
			set { this.fParent = value; }
		}
		ReleaseTypes fParent;

		public new ReleaseType AddNew()
		{
			return (ReleaseType)base.AddNew();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (child is ReleaseType && fParent != null)
			{
				((ReleaseType)child).OriginalsNumber = fParent.OriginalsNumber;
				((ReleaseType)child).CopiesNumber = fParent.CopiesNumber;
			}
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ReleaseTypeCollection();
		}

		public new ReleaseType this[int i]
		{
			get { return (ReleaseType)base[i]; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ReleaseType();
		}

		public new ReleaseType FindByCode(string code)
		{
			return (ReleaseType)base.FindByCode(code);
		}
	}
}
