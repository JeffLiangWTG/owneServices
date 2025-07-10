using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class UOMPackTypeCollection : RegistryBusinessObjectCollectionTemplate
	{
		#region Clone

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UOMPackTypeCollection();
		}

		#endregion

		#region CreateNonPersistentBusinessObject

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UOMPackType();
		}

		#endregion

		#region AllowNewCore
		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region AllowRemoveCore
		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion

		#region Defaults

		public static UOMPackTypeCollection GetDefault()
		{
			var result = new UOMPackTypeCollection();
			var packTypeList = new UOMPackTypesList();

			foreach (CodeDescriptionPair pair in packTypeList)
			{
				var packType = result.AddNew();
				packType.Code = pair.Code;
				packType.Description = pair.MultilingualDescription;
				packType.NumberOfLabels = 1;
			}

			return result;
		}

		#endregion

		#region Index

		public new UOMPackType this[int i]
		{
			get { return (UOMPackType)Elements[i]; }
		}

		public new UOMPackType AddNew()
		{
			return (UOMPackType)base.AddNew();
		}

		#endregion
	}
}
