using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class InterchangeObjCollection : RegistryBusinessObjectCollectionTemplate
	{
		public InterchangeObjCollection() : base() { }

		public new InterchangeObj this[int i]
		{
			get { return (InterchangeObj)base[i]; }
		}

		public new InterchangeObj AddNew()
		{
			return (InterchangeObj)base.AddNew();
		}

		#region Clone

		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InterchangeObjCollection();
		}

		#endregion

		#region Override

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InterchangeObj();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}
