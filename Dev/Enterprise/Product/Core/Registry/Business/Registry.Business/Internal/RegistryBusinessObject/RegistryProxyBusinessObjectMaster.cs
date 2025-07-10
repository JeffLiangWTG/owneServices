using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public abstract class RegistryProxyBusinessObjectMaster<T> : RegistryBusinessObjectTemplate
		where T : RegistryProxyBusinessObject
	{
		protected RegistryProxyBusinessObjectMaster()
		{
		}

		#region FindBoxCollection

		public IBusinessObjectCollection FindBoxCollection
		{
			get { return CurrentFactory.GetCachedValue("RegistryProxyBusinessObjectMaster|FindBoxCollection", GetNewFindBoxCollection); }
		}

		protected abstract IBusinessObjectCollection GetNewFindBoxCollection();

		#endregion

		#region Elements

		public RegistryProxyBusinessObjectCollection<T> Items
		{
			get { return items ?? (items = GetNewCollection()); }
		}

		protected abstract RegistryProxyBusinessObjectCollection<T> GetNewCollection();

		RegistryProxyBusinessObjectCollection<T> items;

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = GetNewMaster();
			clone.items = (RegistryProxyBusinessObjectCollection<T>)Items.Clone(fallbackLevel, factory);

			return clone;
		}

		protected abstract RegistryProxyBusinessObjectMaster<T> GetNewMaster();

		#endregion

		#region Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			var serialiser = ZXmlSerializer.New(Items.GetType());
			items = (RegistryProxyBusinessObjectCollection<T>)serialiser.Deserialize(reader.Reader);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			var serialiser = ZXmlSerializer.New(Items.GetType());
			serialiser.Serialize(writer, Items);
		}

		#endregion
	}
}
