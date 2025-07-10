using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class CountryTierPriceCodeMapping : AutoCountryTierPriceCodeMapping
	{
		public CountryTierPriceCodeMapping(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public CountryTierPriceCodeMapping()
		{
		}

		#region Mapping Lines

		[BusinessObjectTestExclude] // These properties implement setters on collections, which is generally not advised.
		public CountryTierPriceCodeMappingLineCollection MappingLines
		{
			get
			{
				if (mappingLines == null)
				{
					mappingLines = new CountryTierPriceCodeMappingLineCollection();
					RegisterEditableChildObject(mappingLines);
				}
				return mappingLines;
			}
			private set { mappingLines = value; }
		}

		CountryTierPriceCodeMappingLineCollection mappingLines;

		protected override void ReadMappingLines(XmlReader reader)
		{
			MappingLines.RemoveAll();

			if (reader.IsEmptyElement)
			{
				reader.Skip();
			}
			else
			{
				reader.ReadStartElement();

				while (reader.IsStartElement("CountryTierPriceCodeMappingLine"))
				{
					var mappingLine = MappingLines.AddNew();
					((IXmlSerializable)mappingLine).ReadXml(reader);
				}

				reader.ReadEndElement();
			}
		}

		protected override void WriteMappingLines(XmlWriter writer)
		{
			foreach (var mappingLine in MappingLines)
			{
				writer.WriteStartElement("CountryTierPriceCodeMappingLine");
				((IXmlSerializable)mappingLine).WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new CountryTierPriceCodeMapping(fallbackLevel, factory);
			clone.MappingLines = (CountryTierPriceCodeMappingLineCollection)MappingLines.Clone(fallbackLevel, factory);
			return clone;
		}

		#region Validation

		public override void ValidatePriceCode()
		{
			base.ValidatePriceCode();
			MandatoryValidation.CheckEntered(PriceCodeInfo);
		}

		public override void ValidateSystemCode()
		{
			base.ValidateSystemCode();
			MandatoryValidation.CheckEntered(SystemCodeInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();

			if (MappingLines.Cast<CountryTierPriceCodeMappingLine>().Any(x => x.HasErrors || x.HasRowErrors))
			{
				AddRowError(Res.GetString("af8899d8-a991-4d2f-ba05-944ef8a5f556", "One or more country tier codes for Price Code: {0}, System: {1} have an error", PriceCode, SystemCode));
			}
		}

		#endregion
	}
}
