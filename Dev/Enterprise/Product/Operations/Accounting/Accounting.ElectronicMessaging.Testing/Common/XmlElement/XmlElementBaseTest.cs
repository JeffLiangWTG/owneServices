using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public abstract class XmlElementBaseTest : TestCaseWithFactory
	{
		public abstract void TestHasValue();

		public abstract void TestToXElements();

		public abstract void TestToWriteToXmlStream();

		public abstract void TestToString();

		protected readonly XNamespace DummyDocumentXmlns = "http://schemas.wtg.com";
	}
}
