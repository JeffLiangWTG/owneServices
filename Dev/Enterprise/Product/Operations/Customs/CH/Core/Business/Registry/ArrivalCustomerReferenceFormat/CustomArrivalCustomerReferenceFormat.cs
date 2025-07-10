using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.Business;

[XmlSerializerAssembly("Enterprise.Customs.CH.Business.XmlSerializers")]
public sealed class CustomArrivalCustomerReferenceFormat : AutoCustomArrivalCustomerReferenceFormat
{
	public CustomArrivalCustomerReferenceFormat()
	{
	}

	public CustomArrivalCustomerReferenceFormat(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(factory)
	{
		CurrentFallbackLevel = fallbackLevel;
	}

	[List(nameof(Lookups) + "." + nameof(CustomArrivalCustomerReferenceFormatLookups.AuthorizationLocationCodeList))]
	public override ZString AuthorizationLocationCode { get => base.AuthorizationLocationCode; set => base.AuthorizationLocationCode = value; }

	[List(nameof(Lookups) + "." + nameof(CustomArrivalCustomerReferenceFormatLookups.YearOptionList))]
	public override ZString YearOption
	{
		get => base.YearOption;
		set
		{
			base.YearOption = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidatePrefix();
				Validation.ValidateSuffix();
			}
		}
	}

	protected override void SetCustomDefaultValuesCore()
	{
		base.SetCustomDefaultValuesCore();
		YearOption = CustomArrivalCustomerReferenceFormatYearOptionList.Codes.None;
		SequenceNumberLength = DefaultSequenceNumberLength;
	}

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		return new CustomArrivalCustomerReferenceFormat(fallbackLevel, factory);
	}

	protected override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);
		writer.WriteElementString(Schema.AuthorizationLocationCode, AuthorizationLocationCode);
		writer.WriteElementString(Schema.Prefix, Prefix);
		writer.WriteElementString(Schema.Suffix, Suffix);
		writer.WriteElementString(Schema.YearOption, YearOption);
		writer.WriteElementString(Schema.SequenceNumberLength, SequenceNumberLength.ToString());
		writer.WriteElementString(Schema.IsRemoveLeadingZeros, IsRemoveLeadingZeros.ToString());
		writer.WriteElementString(Schema.IsRestartOnNewYear, IsRestartOnNewYear.ToString());
	}

	protected override void ReadElements(XmlReaderWrapper reader)
	{
		AuthorizationLocationCode = reader.ReadElementString(Schema.AuthorizationLocationCode);
		Prefix = reader.ReadElementString(Schema.Prefix);
		Suffix = reader.ReadElementString(Schema.Suffix);
		YearOption = reader.ReadElementString(Schema.YearOption);
		SequenceNumberLength = (byte)reader.ReadElementStringAsZInt(Schema.SequenceNumberLength);
		IsRemoveLeadingZeros = reader.ReadElementStringAsZBool(Schema.IsRemoveLeadingZeros);
		IsRestartOnNewYear = reader.ReadElementStringAsZBool(Schema.IsRestartOnNewYear);
	}

	public CustomArrivalCustomerReferenceFormatLookups Lookups => lookups ?? (lookups = new CustomArrivalCustomerReferenceFormatLookups(this));
	CustomArrivalCustomerReferenceFormatLookups lookups;

	internal new BusinessObjectFactory CurrentFactory => base.CurrentFactory;

	const byte DefaultSequenceNumberLength = 8;
}
