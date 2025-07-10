using System;
using System.Globalization;
using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlWriting
{
	class UntypedElementWriter : Element, IElementWriter
	{
		internal UntypedElementWriter(string elementName, Type typeOfDataObject, XmlBuilder builder)
			: base(elementName, builder)
		{
			this.builder = builder;
			this.typeOfDataObject = Argument.NotNull(typeOfDataObject, "Type typeOfDataObject");
		}

		readonly XmlBuilder builder;
		readonly Type typeOfDataObject;

		public void WriteXML(IDataObject dataStructure, DataOverrideManager overrideManager)
		{
			var rootElementInfo = typeOfDataObject.GetCustomAttribute<RootElementAttribute>();
			builder.AddStartElementWithAttributes(rootElementInfo.RootElementName, string.Format(CultureInfo.InvariantCulture, @"version=""{0}""", builder.Version));

			var writer = new ComplexTypeWriter(typeOfDataObject.Name, typeOfDataObject, builder);

			writer.WriteXML(dataStructure, overrideManager);

			builder.AddEndElement(rootElementInfo.RootElementName);
		}

		internal override PlacingWithinXml DefaultPlacing
		{
			get { return PlacingWithinXml.FieldsAndRelatedObjects; }
		}
	}
}
