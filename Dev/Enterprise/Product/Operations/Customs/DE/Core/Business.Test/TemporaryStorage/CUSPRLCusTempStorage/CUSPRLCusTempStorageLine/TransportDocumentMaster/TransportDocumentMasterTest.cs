using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TransportDocumentMaster))]
	sealed class TransportDocumentMasterTest : Customs.Business.Testing.CusSupportingInfoTest<TransportDocumentMaster>
	{
		public void TestValidation() => AssertType<TransportDocumentMasterValidation>(transportDocumentMaster.Validation);

		public void TestLookups() => AssertType<TransportDocumentMasterLookups>(transportDocumentMaster.Lookups);

		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Type", CusSupportingInfoTypeList.Codes.AdditionalInfo, transportDocumentMaster.CSI_Type);
				AssertEquals("CSI_SubType", AdditionalDocTypeList.Codes.TransportDocuments, transportDocumentMaster.CSI_SubType);
			});
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Transport Document Master", transportDocumentMaster.HumanReadableName);
		}

		public void TestCSICodeCaption()
		{
			var data = DataBoundResourceStrings.GetDataForProperty(transportDocumentMaster.CSI_CodeInfo);
			AssertEquals("Type", data.Caption);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var storageHeader = factory.New<CusTempStorageJobHeader>();
			var org = factory.New<OrgHeader>();
			org.OH_Code = "TEST1";
			storageHeader.SJH_OH_Customer = org.PK;
			var cusprlDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageHeader);
			var cusprlLine = cusprlDec.CusTempStorageLines.AddNew();
			transportDocumentMaster = cusprlLine.TransportDocumentMaster;

			return transportDocumentMaster;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var cusprlDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageHeader);
			var cusprlLine = cusprlDec.CusTempStorageLines.AddNew();
			transportDocumentMaster = cusprlLine.TransportDocumentMaster;
		}
		TransportDocumentMaster transportDocumentMaster;
	}
}
