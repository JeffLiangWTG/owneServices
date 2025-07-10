using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.Business;

[XmlSerializerAssembly("Enterprise.Customs.CH.Business.XmlSerializers")]
public class PassarSearchRequestConfig : AutoPassarSearchRequestConfig
{
	public PassarSearchRequestConfig() : base()
	{
	}

	public PassarSearchRequestConfig(FallbackLevel fallbackLevel) : base(fallbackLevel)
	{
	}

	[ReadOnlyMember(nameof(TimeLimitReadOnly))]
	public override ZInt TimeLimit { get => base.TimeLimit; set => base.TimeLimit = value; }

	public bool TimeLimitReadOnly => !IsEnabled;

	public override void ValidateTimeLimit()
	{
		base.ValidateTimeLimit();
		if (TimeLimit < 1 || TimeLimit > 720)
		{
			TimeLimitInfo.AddError(Res.GetString("EEF59008-885A-40C9-8233-6C33C375D1D2", "Time limit must be between 1 and 720 hours (30 days)."));
		}
	}
	protected override void SetCustomDefaultValuesCore()
	{
		base.SetCustomDefaultValuesCore();
		IsEnabled = true;
		TimeLimit = 720;
	}

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new PassarSearchRequestConfig(fallbackLevel);

	protected override void ReadElements(XmlReaderWrapper reader)
	{
		IsEnabled = reader.ReadElementStringAsZBool(Schema.IsEnabled);
		TimeLimit = reader.ReadElementStringAsZInt(Schema.TimeLimit);
	}

	override protected void WriteElements(XmlWriter writer)
	{
		writer.WriteElementString(Schema.IsEnabled, IsEnabled.ToString());
		writer.WriteElementString(Schema.TimeLimit, TimeLimit.ToString());
	}
}
