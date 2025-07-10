using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RegistryImageCollection : RegistryBusinessObjectCollection
	{
		public new RegistryImage this[int i]
		{
			get { return (RegistryImage)Elements[i]; }
		}

		public new RegistryImage AddNew()
		{
			return (RegistryImage)base.AddNew();
		}

		public new RegistryImage FindByCode(string code)
		{
			return (RegistryImage)base.FindByCode(code);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RegistryImageCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RegistryImage();
		}

		#region Clean Up

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.RemoveAndDelete(elementToDelete);
			DeletedElements.Add((RegistryImage)elementToDelete);
		}

		internal void SetFallbackKey(string fallbackKey)
		{
			foreach (RegistryImage element in this)
			{
				element.FallbackKeyForSaving = fallbackKey;
			}
		}

		internal void CleanUpDeletedElements(string fallbackKey)
		{
			if (deletedElements != null)
			{
				foreach (RegistryImage element in deletedElements)
				{
					element.DeleteImage(fallbackKey);
				}
			}
		}

		List<RegistryImage> DeletedElements
		{
			get
			{
				if (deletedElements == null)
				{
					deletedElements = new List<RegistryImage>();
				}
				return deletedElements;
			}
		}

		List<RegistryImage> deletedElements;

		#endregion
	}
}
