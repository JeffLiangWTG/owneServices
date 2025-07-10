using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.NCTS.Business;

[XmlSerializerAssembly("Enterprise.Customs.DE.NCTS.Business.XmlSerializers")]
public class NctsFallbackConfiguration : RegistryBusinessObjectTemplate<NctsFallbackConfigurationValidation>
{
	#region Schema

	static class Schema
	{
		public const string Start = "Start";
		public const string CustomsIncidentNumber = "CustomsIncidentNumber";
	}

	#endregion

	#region Constructions and cloning

	public NctsFallbackConfiguration()
	{
	}

	public NctsFallbackConfiguration(BusinessObjectFactory factory)
		: base(factory)
	{
	}

	public NctsFallbackConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{
	}

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new NctsFallbackConfiguration(fallbackLevel, factory);

	#endregion

	#region Read / Write Elements

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Date time format")]
	const string DateFormat = "yyyy-MM-dd HH:mm:ss";

	protected override void ReadElements(XmlReaderWrapper reader)
	{
		Start = reader.ReadElementStringAsZDateTime(Schema.Start, DateFormat);
		CustomsIncidentNumber = reader.ReadElementString(Schema.CustomsIncidentNumber);
	}

	protected override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);

		writer.WriteElementString(Schema.Start, start.ToString(DateFormat));
		writer.WriteElementString(Schema.CustomsIncidentNumber, CustomsIncidentNumber);
	}

	#endregion

	#region Start

	[ResourceStringData("6BD53279-9ED5-42AF-858B-C279CE058D23", Caption = "Start")]
	public ZDateTime Start
	{
		get
		{
			return start;
		}
		set
		{
			SetNonPersistentPropertyValue(StartInfo, ref start, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateStart();
			}
		}
	}
	ZDateTime start;

	public ZPropertyInfo StartInfo
	{
		get { return GetZPropertyInfo(Schema.Start); }
	}

	#endregion

	#region CustomsIncidentNumber

	[ResourceStringData("B2C01157-03DC-49BB-A0D5-EC73115D7B66", Caption = "Customs Incident Number")]
	[MaxLength(20)]
	public ZString CustomsIncidentNumber
	{
		get
		{
			return customsIncidentNumber;
		}
		set
		{
			SetNonPersistentPropertyValue(CustomsIncidentNumberInfo, ref customsIncidentNumber, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateCustomsIncidentNumber();
			}
		}
	}
	ZString customsIncidentNumber;

	public ZPropertyInfo CustomsIncidentNumberInfo
	{
		get { return GetZPropertyInfo(Schema.CustomsIncidentNumber); }
	}

	#endregion

	protected override NctsFallbackConfigurationValidation GetNewValidation() => new(this);
}
