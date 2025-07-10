using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders
{
	class AdditionalInformationComparer : IComparer<XElement>
	{
		public int Compare(XElement x, XElement y)
		{
			var xValue = x.Elements().FirstOrDefault(e => e.Name.LocalName == "StatementTypeCode")?.Value ?? ZString.Empty;
			var yValue = y.Elements().FirstOrDefault(e => e.Name.LocalName == "StatementTypeCode")?.Value ?? ZString.Empty;
			return xValue.CompareTo(yValue);
		}
	}
}
