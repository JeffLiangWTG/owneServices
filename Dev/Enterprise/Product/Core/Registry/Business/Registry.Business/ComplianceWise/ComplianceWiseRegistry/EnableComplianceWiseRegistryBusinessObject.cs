using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EnableComplianceWiseRegistryBusinessObject : RegistryBusinessObjectTemplate
	{
		///<Summary>
		///Required for report menu lists macro execution DocumentEngine.RegistryItem.GetRegistryValueFromMacro
		///</Summary>
		public bool Value
		{
			get { return EnableComplianceWise; }
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			EnableComplianceWise = true;
		}

		public EnableComplianceWiseRegistryBusinessObject()
		{
		}

		public ZBool EnableComplianceWise
		{
			get { return enableComplianceWise; }
			set { SetNonPersistentPropertyValue(EnableComplianceWiseInfo, ref enableComplianceWise, value); }
		}
		ZBool enableComplianceWise;

		public ZPropertyInfo EnableComplianceWiseInfo
		{
			get { return GetZPropertyInfo(Schema.EnableComplianceWise); }
		}

		public EnableComplianceWiseRegistryBusinessObject(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EnableComplianceWiseRegistryBusinessObject();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			enableComplianceWise = new ZBool(reader.ReadElementString(Schema.EnableComplianceWise));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EnableComplianceWise, EnableComplianceWise.ToString());
		}

		#region Schema

		public static class Schema
		{
			public const string EnableComplianceWise = "EnableComplianceWise";
		}

		#endregion
	}
}
