using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignment))]
	sealed class CusExitConsignmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertType<CusExitConsignmentValidation>(exitConsignment.Validation);
		}

		public void TestCXC_ReferenceNumber_Caption()
		{
			CombineAssertions(() =>
			{
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(exitConsignment.CXC_ReferenceNumberInfo);
				AssertEquals("Caption", "Registration Number (ext.)", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Rego. No. (ext.)", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "Rego. No.", resourceStringData.ShortCaption);
			});
		}

		public void TestCusExitConsignmentItems()
		{
			AssertType<ExitControlBase.Business.CusExitConsignmentItemCollection<CusExitConsignmentItem>>(exitConsignment.CusExitConsignmentItems);
		}

		public void TestAdditionalInfoCollection()
		{
			AssertType<ExitControlAdditionalInfoCollection>(exitConsignment.AdditionalInfos);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var supportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)exitConsignment).GetCusSupportingInfoTypes();
			AssertEquals(typeof(ExitControlAdditionalInfo), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		}

		public void TestConsignmentItemsRequiredToCreateCusExitReport() => CombineAssertions(() =>
		{
			exitConsignment.CXC_Status = ZString.Empty;
			AssertEquals(false, exitConsignment.ConsignmentItemsRequiredToCreateCusExitReport);

			exitConsignment.CXC_Status = "123";
			AssertEquals(false, exitConsignment.ConsignmentItemsRequiredToCreateCusExitReport);

			exitConsignment.CXC_Status = "310";
			AssertEquals(true, exitConsignment.ConsignmentItemsRequiredToCreateCusExitReport);

			exitConsignment.CXC_Status = "400";
			AssertEquals(true, exitConsignment.ConsignmentItemsRequiredToCreateCusExitReport);
		});

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CusExitConsignment>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			exitConsignment = Factory.NewWithValidTestData<CusExitConsignment>();
		}
		CusExitConsignment exitConsignment;
	}
}
