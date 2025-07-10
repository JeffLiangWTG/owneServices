using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(AFRReporterIDRegistryItem))]
	sealed class ExportEntryFilerIDRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<AFRReporterID>
	{
		public void TestRegistryOptions()
		{
			AssertEquals("Options", RegistryOptions.IsOnlyForController, new AFRReporterIDRegistryItem("DUMMY", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hello World").Options);
		}

		protected override StronglyTypedRegistryItem<AFRReporterID, AFRReporterID> GetNewRegistryItem()
		{
			return new AFRReporterIDRegistryItem("", null, null, null);
		}

		protected override AFRReporterID ValidValue => new AFRReporterID
		{
			ReporterID = "12345",
			Password = "1234"
		};
	}
}
