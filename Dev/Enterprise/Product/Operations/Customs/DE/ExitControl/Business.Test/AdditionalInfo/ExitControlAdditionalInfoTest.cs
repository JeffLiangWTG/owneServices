using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	[TestedType(typeof(ExitControlAdditionalInfo))]
	sealed class ExitControlAdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<ExitControlAdditionalInfo>
	{
		public void TestLookups()
		{
			AssertType<ExitControlAdditionalInfoLookups>(exitControlAdditionalInfo.Lookups);
		}

		public void TestValidation()
		{
			AssertType<ExitControlAdditionalInfoValidation>(exitControlAdditionalInfo.Validation);
		}

		public void TestExitConsignment()
		{
			AssertSame(consignment, exitControlAdditionalInfo.ExitConsignment);
		}

		public void TestCSI_Code_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(exitControlAdditionalInfo.CSI_CodeInfo);
			AssertEquals("Full Type", resourceStringData.Caption);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(AdditionalInfoSubTypeList.Codes.AdditionalInformation, exitControlAdditionalInfo.CSI_SubType);
		}

		protected override IEnumerable<ExitControlAdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return GetNewBusinessObject(factory).ExitControlAdditionalInfo;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).ExitControlAdditionalInfo;

		protected override void SetUp()
		{
			base.SetUp();

			(consignment, exitControlAdditionalInfo) = GetNewBusinessObject(Factory);
		}
		CusExitConsignment consignment;
		ExitControlAdditionalInfo exitControlAdditionalInfo;

		(CusExitConsignment Consignment, ExitControlAdditionalInfo ExitControlAdditionalInfo) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<CusExitHeader>();
			header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
			var consignment = header.CusExitConsignments.AddNew();
			consignment.CXC_Status = "REJ";
			var exitControlAdditionalInfo = consignment.AdditionalInfos.AddNew();
			return (consignment, exitControlAdditionalInfo);
		}
	}
}
