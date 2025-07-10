using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.AU.Declaration.Business.XmlSerializers")]
	public class DefaultDestinationPremiseID : DefaultPremiseID
	{
		public DefaultDestinationPremiseID()
		{
		}

		public DefaultDestinationPremiseID(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new class Schema : DefaultPremiseID.Schema
		{
			public const string UseDischargePort = "UseDischargePort";
		}

		#region Properties

		public ZBool UseDischargePort
		{
			get { return fUseDischargePort; }
			set { SetNonPersistentPropertyValue(UseDischargePortInfo, ref fUseDischargePort, value); }
		}
		ZBool fUseDischargePort;

		public ZPropertyInfo UseDischargePortInfo
		{
			get { return GetZPropertyInfo(Schema.UseDischargePort); }
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultDestinationPremiseID(factory);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.UseDischargePort, UseDischargePort.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			UseDischargePort = reader.ReadElementStringAsZBool(Schema.UseDischargePort);
		}

		#endregion
	}
}
