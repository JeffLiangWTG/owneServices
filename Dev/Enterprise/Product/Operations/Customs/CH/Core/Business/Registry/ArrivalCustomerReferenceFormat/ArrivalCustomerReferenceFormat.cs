using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.CH.Business;

[XmlSerializerAssembly("Enterprise.Customs.CH.Business.XmlSerializers")]
public sealed class ArrivalCustomerReferenceFormat : RegistryBusinessObjectTemplate
{
	public static class Schema
	{
		public const string UseSystemDefinedFormat = nameof(ArrivalCustomerReferenceFormat.UseSystemDefinedFormat);
	}

	public ArrivalCustomerReferenceFormat()
	{
	}

	internal ArrivalCustomerReferenceFormat(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
	{
	}

	[ResourceStringData("CH.ArrivalCustomerReferenceFormat.UseSystemDefinedFormat", Caption = "Use system-defined format")]
	public ZBool UseSystemDefinedFormat
	{
		get { return useSystemDefinedFormat; }
		set { SetNonPersistentPropertyValue(UseSystemDefinedFormatInfo, ref useSystemDefinedFormat, value); }
	}
	ZBool useSystemDefinedFormat;

	public ZPropertyInfo UseSystemDefinedFormatInfo => GetZPropertyInfo(Schema.UseSystemDefinedFormat);

	public ZBool NotUseSystemDefinedFormat
	{
		get { return !UseSystemDefinedFormat; }
		set { UseSystemDefinedFormat = !value; }
	}

	public CustomArrivalCustomerReferenceFormatCollection CustomFormats
	{
		get
		{
			if (customFormats == null)
			{
				customFormats = new CustomArrivalCustomerReferenceFormatCollection(CurrentFallbackLevel, CurrentFactory);
				RegisterEditableChildObject(customFormats);
			}
			return customFormats;
		}
	}
	CustomArrivalCustomerReferenceFormatCollection customFormats;

	ZXmlSerializer CustomArrivalCustomerReferenceFormatsSerializer => customArrivalCustomerReferenceFormatsSerializer ?? (customArrivalCustomerReferenceFormatsSerializer = ZXmlSerializer.New(typeof(CustomArrivalCustomerReferenceFormatCollection)));
	ZXmlSerializer customArrivalCustomerReferenceFormatsSerializer;

	void CloneCustomFormatsFrom(CustomArrivalCustomerReferenceFormatCollection source)
	{
		customFormats = (CustomArrivalCustomerReferenceFormatCollection)source.Clone(CurrentFallbackLevel, CurrentFactory);
		RegisterEditableChildObject(customFormats);
	}

	protected override void SetCustomDefaultValuesCore()
	{
		base.SetCustomDefaultValuesCore();
		UseSystemDefinedFormat = ZBool.True;
	}

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		return new ArrivalCustomerReferenceFormat(fallbackLevel, factory);
	}

	protected override void CopyCollectionsToClone(RegistryBusinessObjectTemplate clone, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
	{
		base.CopyCollectionsToClone(clone, currentFallbackLevel, factory);
		if (clone is ArrivalCustomerReferenceFormat referenceFormat)
		{
			referenceFormat.CloneCustomFormatsFrom(CustomFormats);
		}
	}

	protected override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);
		writer.WriteElementString(Schema.UseSystemDefinedFormat, UseSystemDefinedFormat.ToString());
		CustomArrivalCustomerReferenceFormatsSerializer.Serialize(writer, CustomFormats);
	}

	protected override void ReadElements(XmlReaderWrapper reader)
	{
		useSystemDefinedFormat = reader.ReadElementStringAsZBool(Schema.UseSystemDefinedFormat);
		CloneCustomFormatsFrom((CustomArrivalCustomerReferenceFormatCollection)CustomArrivalCustomerReferenceFormatsSerializer.Deserialize(reader));
	}

	public CustomArrivalCustomerReferenceFormat GetFormatByAuthorizationLocationCode(string authorizationLocationCode)
	{
		return CustomFormats.Cast<CustomArrivalCustomerReferenceFormat>().FirstOrDefault(x => x.AuthorizationLocationCode == authorizationLocationCode);
	}
}
