using System.Reflection;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlWriting
{
	class SimpleTypeWriter : Element, IElementWriter
	{
		internal SimpleTypeWriter(PropertyInfo propertyInfo, XmlBuilder builder)
			: base(propertyInfo, builder)
		{
			this.builder = builder;
		}

		readonly XmlBuilder builder;

		internal override PlacingWithinXml DefaultPlacing
		{
			get { return PlacingWithinXml.FieldsAndRelatedObjects; }
		}

		void IElementWriter.WriteXML(IDataObject dataStructure, DataOverrideManager overrideManager)
		{
			if (dataStructure != null)
			{
				var value = PropertyInfo.GetValue(dataStructure, null);
				if (value != null)
				{
					var formattedValue = SimpleTypeFormatter.GetFormattedValueForWritingToXml(value, delegate { return builder.GetMaxLengthCached(PropertyInfo); });
					builder.AddElementWithValue(ElementName, formattedValue);
				}
			}
		}
	}
}
