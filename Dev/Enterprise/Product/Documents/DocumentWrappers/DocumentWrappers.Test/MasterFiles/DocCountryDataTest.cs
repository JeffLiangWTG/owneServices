using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocCountryData))]
	public class DocCountryDataTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var header = Factory.New<OrgHeader>();

			return new DocumentWrapper[]
			{
				DocCountryData.New(header.CountryData, Factory)
			};
		}

		public void TestCode()
		{
			testCountryData.OV_EXExportPermissionDetails = "Export Code";
			AssertEquals("Wrapped Value", "Export Code", testDocCountryData.ExportPermissionDetails);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			testHeader = Factory.New<OrgHeader>();
			testCountryData = testHeader.CountryData;
			testDocCountryData = DocCountryData.New(testCountryData, Factory);
			AssertNotNull("PreCondition: Valid DocAddress", testCountryData);
		}

		DocCountryData testDocCountryData;
		OrgCountryData testCountryData;
		OrgHeader testHeader;

		#endregion
	}
}
