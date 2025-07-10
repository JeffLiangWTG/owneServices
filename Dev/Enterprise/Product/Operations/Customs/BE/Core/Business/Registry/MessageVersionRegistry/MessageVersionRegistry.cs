using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BE.Business;

[XmlSerializerAssembly("Enterprise.Customs.BE.Business.XmlSerializers")]
public class MessageVersionRegistry : AutoMessageVersionRegistry
{
	public MessageVersionRegistry()
	{
	}

	public MessageVersionRegistry(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{
	}

	public const string NCTSP5DomainCode = "NCTSP5";
	public const string NCTSP5DefaultTarget = "NCTS.BE";
	public const string IDMSDomainCode = "IDMS";
	public const string IDMSDefaultTarget = "IDMS.BE";
	public const string AESDomainCode = "AES";
	public const string AESDefaultTarget = "AES.BE";
	public const string PNTSDomainCode = "PN/TS";
	public const string PNTSDefaultTarget = "PN-TS.BE";
	public const string TSDDomainCode = "TSD";
	public const string TSDDefaultTarget = "TSD.BE";
	public const string RENDomainCode = "REN";
	public const string RENDefaultTarget = "REN.BE";

	protected override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);
		writer.WriteElementString(Schema.DomainCode, DomainCode);
		writer.WriteElementString(Schema.TargetSystemName, TargetSystemName);
	}

	protected override void ReadElements(XmlReaderWrapper reader)
	{
		DomainCode = reader.ReadElementString(Schema.DomainCode);
		TargetSystemName = reader.ReadElementString(Schema.TargetSystemName);
	}

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new MessageVersionRegistry(fallbackLevel, factory);
}

