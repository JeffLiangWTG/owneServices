using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.Business;

[XmlSerializerAssembly("Enterprise.Customs.CH.Business.XmlSerializers")]
public class EdecBordereauConfig : AutoEdecBordereauConfig
{
	public EdecBordereauConfig() : base()
	{
	}

	public EdecBordereauConfig(FallbackLevel fallbackLevel) : base(fallbackLevel)
	{
	}

	public override void ValidateNumberOfDays()
	{
		base.ValidateNumberOfDays();
		CompareValidation.CheckWithinRange(NumberOfDaysInfo, 0, 10);
	}

	protected override void SetCustomDefaultValuesCore()
	{
		base.SetCustomDefaultValuesCore();
		IsEnabled = false;
		NumberOfDays = 3;
	}

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new EdecBordereauConfig(fallbackLevel);

	protected override void ReadElements(XmlReaderWrapper reader)
	{
		IsEnabled = reader.ReadElementStringAsZBool(Schema.IsEnabled);
		NumberOfDays = reader.ReadElementStringAsZInt(Schema.NumberOfDays);
	}

	override protected void WriteElements(XmlWriter writer)
	{
		writer.WriteElementString(Schema.IsEnabled, IsEnabled.ToString());
		writer.WriteElementString(Schema.NumberOfDays, NumberOfDays.ToString());
	}
}
