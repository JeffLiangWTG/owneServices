using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HVLVPreScreeningFieldCollection : RegistryBusinessObjectCollectionTemplate
	{
		public HVLVPreScreeningFieldCollection(HVLVPreScreeningRule validationRule)
		{
			HVLVPreScreeningRule = validationRule;
		}

		public HVLVPreScreeningFieldCollection()
		{
		}

		internal HVLVPreScreeningRule HVLVPreScreeningRule
		{
			get { return fHVLVPreScreeningRule; }
			set
			{
				if (fHVLVPreScreeningRule != value)
				{
					fHVLVPreScreeningRule = value;
					foreach (HVLVPreScreeningField field in this)
					{
						field.HVLVPreScreeningRule = fHVLVPreScreeningRule;
					}
				}
			}
		}
		HVLVPreScreeningRule fHVLVPreScreeningRule;

		public new HVLVPreScreeningField AddNew()
		{
			return (HVLVPreScreeningField)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new HVLVPreScreeningField(HVLVPreScreeningRule);
		}

		#region GetClone

		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HVLVPreScreeningFieldCollection(HVLVPreScreeningRule);
		}

		#endregion

		public new HVLVPreScreeningField this[int i]
		{
			get { return (HVLVPreScreeningField)Elements[i]; }
		}
	}
}
