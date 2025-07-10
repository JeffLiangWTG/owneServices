using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HVLVPreScreeningSpecialCharacterValueCollection : RegistryBusinessObjectCollectionTemplate
	{
		public HVLVPreScreeningSpecialCharacterValueCollection(HVLVPreScreeningField preScreeningField)
		{
			HVLVPreScreeningField = preScreeningField;
		}

		public HVLVPreScreeningSpecialCharacterValueCollection()
		{
		}

		internal HVLVPreScreeningField HVLVPreScreeningField
		{
			get { return fHVLVPreScreeningField; }
			set
			{
				if (fHVLVPreScreeningField != value)
				{
					fHVLVPreScreeningField = value;
					foreach (HVLVPreScreeningSpecialCharacterValue characterValue in this)
					{
						characterValue.HVLVPreScreeningField = fHVLVPreScreeningField;
					}
				}
			}
		}
		HVLVPreScreeningField fHVLVPreScreeningField;

		public new HVLVPreScreeningSpecialCharacterValue this[int i]
		{
			get { return (HVLVPreScreeningSpecialCharacterValue)Elements[i]; }
		}

		public new HVLVPreScreeningSpecialCharacterValue AddNew()
		{
			return (HVLVPreScreeningSpecialCharacterValue)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new HVLVPreScreeningSpecialCharacterValue(HVLVPreScreeningField);
		}

		#region GetClone

		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HVLVPreScreeningSpecialCharacterValueCollection(HVLVPreScreeningField);
		}

		#endregion

		public ZBool IsEmpty
		{
			get
			{
				return (this.Count == 0) || (this.Count == 1 && this[0].CharacterValue.IsEmpty);
			}
		}
	}
}
