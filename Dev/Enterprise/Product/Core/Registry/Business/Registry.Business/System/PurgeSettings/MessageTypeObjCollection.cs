using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class MessageTypeObjCollection : RegistryBusinessObjectCollectionTemplate
	{
		public MessageTypeObjCollection() : base() { }

		public new MessageTypeObj this[int i]
		{
			get { return (MessageTypeObj)base[i]; }
		}

		public new MessageTypeObj AddNew()
		{
			return (MessageTypeObj)base.AddNew();
		}

		#region Clone

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MessageTypeObjCollection();
		}

		#endregion

		#region Override

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new MessageTypeObj();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}
