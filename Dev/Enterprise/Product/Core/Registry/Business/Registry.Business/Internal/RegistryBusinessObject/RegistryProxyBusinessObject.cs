using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public abstract class RegistryProxyBusinessObject : RegistryBusinessObjectTemplate
	{
		protected RegistryProxyBusinessObject()
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string ProxyPK = "ProxyPK";
		}

		#endregion

		#region Related Entities

		#region Proxy

		protected T GetProxy<T>()
			where T : class
		{
			return ProxyPK.IsValid ? CurrentFactory.Load<T>(ProxyPK) : null;
		}

		#endregion

		#endregion

		#region PK

		protected override ZGuid GetPK()
		{
			// use Proxy PK here so that it integrates easily with the module button grid
			return ProxyPK;
		}

		#endregion

		#region Properties

		#region ProxyPK

		public ZGuid ProxyPK
		{
			get;
			set;
		}

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();

			var parent = Parent;
			if (parent != null)
			{
				parent.Remove(this);
			}
		}

		RegistryBusinessObjectCollectionTemplate Parent
		{
			get { return GetParentCollection(this, ParentCollectionType); }
		}

		protected abstract Type ParentCollectionType { get; }

		#endregion

		#region Clone

		protected sealed override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = GetNew(fallbackLevel, factory);
			clone.ProxyPK = ProxyPK;

			return clone;
		}

		protected abstract RegistryProxyBusinessObject GetNew(FallbackLevel fallbackLevel, BusinessObjectFactory factory);

		#endregion

		#region Xml Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ZGuid pk;
			var proxyPKText = reader.ReadElementString(Schema.ProxyPK);
			if (ZGuid.TryParse(proxyPKText, out pk))
			{
				ProxyPK = pk;
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ProxyPK, ProxyPK.ToString());
		}

		#endregion
	}
}
