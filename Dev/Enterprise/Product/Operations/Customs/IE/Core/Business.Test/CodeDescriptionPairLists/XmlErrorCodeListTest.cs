using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	class XmlErrorCodeListTest : TestCase
	{
		public void TestEnsureListMatchXsdDefinition()
		{
			var xsdList = Enum.GetValues(typeof(XmlErrorCodes)).Cast<XmlErrorCodes>().Where(x => x != XmlErrorCodes.Empty).Select(x => x.GetXmlEnumAttributeValue()).ToArray();
			var xmdList = new XmlErrorCodeList().GetAllCodes();
			AssertContainsExactElementsInAnyOrder(xsdList, xmdList);
		}
	}
}
