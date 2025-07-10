using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageRegHeader))]
	public class CusTempStorageRegHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetStorageRegLineTypeCore()
			=> AssertEquals(typeof(CusTempStorageRegLine), header.GetStorageRegLineType());

		public void TestSRH_InternalReference_Caption()
		{
			AssertEquals("Customer Reference", DataBoundResourceStrings.GetDataForProperty(header.SRH_InternalReferenceInfo).Caption);
		}

		public void TestSRH_InternalReference_ReadOnly()
		{
			AssertEquals(true, header.SRH_InternalReferenceInfo.ReadOnly);
		}

		public void TestLoad()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Reference = "SBC1312";
			header1.SRH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Reference = "SBC1312";
			header2.SRH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-3);
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_Reference = "SBC1312";
			header3.SRH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			var header4 = Factory.New<CusTempStorageRegHeader>();
			header4.SRH_Reference = "SBC1312";
			header4.SRH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-4);
			header4.SRH_AppCode = "TST";
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var header = CusTempStorageRegHeader.Load(factory, "SBC1312");
			AssertEquals(header2.PK, header.PK);
		}

		public void TestLoad_MRN()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Reference = "23DE586601055987B7";
			header1.SRH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);

			Factory.Save();
			var factory = new BusinessObjectFactory();
			var header = CusTempStorageRegHeader.Load(factory, null, "23DE586601055987B7");
			AssertEquals(header1.PK, header.PK);
		}

		public void TestDefaultAppCode()
		{
			AssertEquals("SRH_AppCode", "SUM", header.SRH_AppCode);
		}

		public void TestSRH_Reference()
		{
			header.SRH_Reference = "ATB150000010320006001";
			AssertEquals("Formatted ATB No.", "AT/B/15/000001/03/2000/6001", header.SRH_Reference);
		}

		public void TestSRH_Reference_Caption()
		{
			AssertEquals("Registration Number", DataBoundResourceStrings.GetDataForProperty(header.SRH_ReferenceInfo).Caption);
		}

		public void TestHumanReadableName()
		{
			header.SRH_Reference = "ATB150000010320006001";
			AssertEquals("Human readable name from ATB No.", "AT/B/15/000001/03/2000/6001", header.HumanReadableName);
		}

		public void TestGetEDocsProviderSupporter()
		{
			AssertType<EDocsProviderSupporter>(header.GetEDocsProviderSupporter());
		}

		public void TestDocumentSupporter()
		{
			var supporter = header.DocumentSupporter;
			CombineAssertions(() =>
			{
				AssertType<CusTempStorageRegHeaderDocumentSupporter>("Type", supporter);
				AssertSame("Cached", supporter, header.DocumentSupporter);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_Reference = "TEST";
		}
		CusTempStorageRegHeader header;

		protected override BusinessObject GetNewBusinessObject() => header;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => header;

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => header;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CusTempStorageRegHeader>();
			header.SRH_Reference = "TEST";
			return header;
		}
	}
}
