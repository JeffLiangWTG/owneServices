using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public interface IShareSequentialReferenceNumbers
	{
		ZBool Value { get; }
	}

	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ShareSequentialReferenceNumbers : RegistryBusinessObjectTemplate, IShareSequentialReferenceNumbers
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ShareSequentialReferenceNumbers result = new ShareSequentialReferenceNumbers();
			return result;
		}

		#region Schema

		public abstract class Schema
		{
			public const string Value = "Value";
		}

		#endregion

		#region Bound Properties

		#region Value

		public ZBool Value
		{
			get { return fValue; }
			set { SetNonPersistentPropertyValue(ValueInfo, ref fValue, value); }
		}

		public ZPropertyInfo ValueInfo
		{
			get { return GetZPropertyInfo(Schema.Value); }
		}

		ZBool fValue = false;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Value, Value.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Value = new ZBool(reader.ReadElementString(Schema.Value));
		}

		#endregion
	}
}
