using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(Port))]
	public class PortTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var port1 = CreatePort(Factory, "GB", "LON", "LONDON");
			AssertEquals("LON", port1.Code);
			AssertEquals("LONDON", port1.Name);
			AssertEquals("", port1.Type);
			AssertEquals(false, port1.IsSea);

			var port2 = CreatePort(Factory, "GB", "FAL", "FALMOUTH", portType: "SEA");
			AssertEquals("FAL", port2.Code);
			AssertEquals("FALMOUTH", port2.Name);
			AssertEquals("SEA", port2.Type);
			AssertEquals(true, port2.IsSea);
		}

		public void TestLoadByCode()
		{
			var port1 = CreatePort(Factory, "GB", "LON", "LONDON");
			var port2 = CreatePort(Factory, "GB", "FAL", "FALMOUTH");
			var port3 = CreatePort(Factory, "FR", "FAL", "FR FAL");
			Factory.Save();

			AssertEquals("LONDON", Business.Port.LoadByCode(Factory, "GB", "LON").Name);
			AssertEquals("FALMOUTH", Business.Port.LoadByCode(Factory, "GB", "FAL").Name);
			AssertEquals("FR FAL", Business.Port.LoadByCode(Factory, "FR", "FAL").Name);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Port(Factory.New<ZZRefCusCodeListCombined>());
		}

		public static Port CreatePort(BusinessObjectFactory factory, ZString dataGroupingCode, ZString portCode, ZString portDescription, string portType = "")
		{
			var cusCodeList = factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CountryOrGrouping = dataGroupingCode;
			cusCodeList.ZZD_CodeType = UniversalReferenceConstants.RefCusCodeListType.Port;
			cusCodeList.ZZD_Code = portCode;
			cusCodeList.ZZD_Description = portDescription;
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddYears(-10);
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddYears(10);

			if (!string.IsNullOrEmpty(portType))
			{
				cusCodeList.Attributes.AddNew(UniversalReferenceConstants.PortAttributes.Type, portType);
			}

			return new Port(cusCodeList);
		}
	}
}
