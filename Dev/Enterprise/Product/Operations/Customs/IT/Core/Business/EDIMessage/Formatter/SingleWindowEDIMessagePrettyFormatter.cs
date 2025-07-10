using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

sealed class SingleWindowEDIMessagePrettyFormatter : ITEDIMessagePrettyFormatter
{
	public SingleWindowEDIMessagePrettyFormatter(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override ZString GetFormattedTextCore(ZString originalText)
	{
		return BuildXmlDeclaration() + originalText;
	}

	#region Implementation

	ZString BuildXmlDeclaration()
	{
		const string xmlVersion = "1.0";

		return new XmlDocument()
			.CreateXmlDeclaration(xmlVersion, null, null)
			.OuterXml;
	}

	#endregion
}
