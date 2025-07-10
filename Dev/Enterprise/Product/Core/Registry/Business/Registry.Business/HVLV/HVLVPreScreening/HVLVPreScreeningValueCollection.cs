using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HVLVPreScreeningValueCollection : RegistryBusinessObjectCollectionTemplate
	{
		public HVLVPreScreeningValueCollection(HVLVPreScreeningField preScreeningField)
		{
			HVLVPreScreeningField = preScreeningField;
		}

		public HVLVPreScreeningValueCollection()
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
					foreach (HVLVPreScreeningValue screeningValue in this)
					{
						screeningValue.HVLVPreScreeningField = fHVLVPreScreeningField;
					}
				}
			}
		}
		HVLVPreScreeningField fHVLVPreScreeningField;

		public new HVLVPreScreeningValue this[int i]
		{
			get { return (HVLVPreScreeningValue)Elements[i]; }
		}

		public new HVLVPreScreeningValue AddNew()
		{
			return (HVLVPreScreeningValue)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new HVLVPreScreeningValue(HVLVPreScreeningField);
		}

		#region GetClone

		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HVLVPreScreeningValueCollection(HVLVPreScreeningField);
		}

		#endregion

		public ZBool IsEmpty
		{
			get
			{
				return (this.Count == 0) || (this.Count == 1 && this[0].ScreeningValue.IsEmpty);
			}
		}
	}
}
