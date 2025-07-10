using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	public class CusSupportingInfoDisabledValidationTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			cusSupportingInfo = Factory.New<CusSupportingInfoForTest>();
		}
		CusSupportingInfoForTest cusSupportingInfo;

		public void TestUsesDisabledValidation()
		{
			AssertType<CusSupportingInfoDisabledValidation>(cusSupportingInfo.Validation);
		}

		public void TestValidateAll()
		{
			cusSupportingInfo.Validation.ValidateAll();
			AssertEquals(0, cusSupportingInfo.RowNotifications.Count());
		}

		public void TestCheckCSI_Code()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_CodeInfo);
				AssertNoNotificationsForInvalidValue(cusSupportingInfo.CSI_CodeInfo);
			});
		}

		public void TestCheckCSI_Status()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_StatusInfo);
				AssertNoNotificationsForInvalidValue(cusSupportingInfo.CSI_StatusInfo);
			});
		}

		public void TestCheckCSI_LineNo()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_LineNoInfo);
		}

		public void TestCheckCSI_Quantity()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_QuantityInfo);
		}

		public void TestCheckCSI_Quantity2()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_Quantity2Info);
		}

		public void TestCheckCSI_ItemNumber()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_ItemNumberInfo);
		}

		public void TestCheckCSI_AdditionalDescription()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_AdditionalDescriptionInfo);
		}

		public void TestCheckCSI_CSI_SupportingInfo()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_CSI_SupportingInfoInfo);
		}

		public void TestCheckCSI_CustomsOffice()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_CustomsOfficeInfo);
				AssertNoNotificationsForInvalidValue(cusSupportingInfo.CSI_CustomsOfficeInfo);
			});
		}

		public void TestCheckCSI_DateOfExpiry()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_DateOfExpiryInfo);
		}

		public void TestCheckCSI_DateOfIssue()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_DateOfIssueInfo);
		}

		public void TestCheckCSI_Description()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_DescriptionInfo);
		}

		public void TestCheckCSI_IssuerType()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_IssuerTypeInfo);
		}

		public void TestCheckCSI_PackQty()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_PackQtyInfo);
		}

		public void TestCheckCSI_PackType()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_PackTypeInfo);
		}

		public void TestCheckCSI_ParentID()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_ParentIDInfo);
		}

		public void TestCheckCSI_Procedure()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_ProcedureInfo);
				AssertNoNotificationsForInvalidValue(cusSupportingInfo.CSI_ProcedureInfo);
			});
		}

		public void TestCheckCSI_Quantity3()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_Quantity3Info);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_ReferenceNumberInfo);
		}

		public void TestCheckCSI_ReferenceNumber2()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_ReferenceNumber2Info);
		}

		public void TestCheckCSI_RN_NKCountryCode()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_RN_NKCountryCodeInfo);
				cusSupportingInfo.CSI_RN_NKCountryCode = "XY";
				AssertNoNotifications(cusSupportingInfo.CSI_RN_NKCountryCodeInfo);
			});
		}

		public void TestCheckCSI_RX_NKCurrency()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_RX_NKCurrencyInfo);
				cusSupportingInfo.CSI_RX_NKCurrency = "XYZ";
				AssertNoNotificationsForInvalidValue(cusSupportingInfo.CSI_RX_NKCurrencyInfo);
			});
		}

		public void TestCheckCSI_SubType()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_SubTypeInfo);
				AssertNoNotificationsForInvalidValue(cusSupportingInfo.CSI_SubTypeInfo);
			});
		}

		public void TestCheckCSI_Tariff()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_TariffInfo);
		}

		public void TestCheckCSI_Type()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_TypeInfo);
				AssertNoNotificationsForInvalidValue(cusSupportingInfo.CSI_TypeInfo);
			});
		}

		public void TestCheckCSI_UnitOfQuantity()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_UnitOfQuantityInfo);
				AssertNoNotificationsForInvalidValue(cusSupportingInfo.CSI_UnitOfQuantityInfo);
			});
		}

		public void TestCheckCSI_UnitOfQuantity2()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_UnitOfQuantity2Info);
				AssertNoNotificationsForInvalidValue(cusSupportingInfo.CSI_UnitOfQuantity2Info);
			});
		}

		public void TestCheckCSI_UnitOfQuantity3()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_UnitOfQuantity3Info);
				AssertNoNotificationsForInvalidValue(cusSupportingInfo.CSI_UnitOfQuantity3Info);
			});
		}

		public void TestCheckCSI_Value()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(cusSupportingInfo.CSI_ValueInfo);
		}

		void AssertNoNotificationsForInvalidValue(ZPropertyInfo info)
		{
			info.Value = new ZString("XYZ");
			AssertNoNotifications(info);
		}

		class CusSupportingInfoForTest : CusSupportingInfo
		{
			public CusSupportingInfoForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override CusSupportingInfoValidation GetNewValidation()
			{
				return new CusSupportingInfoDisabledValidation(this);
			}
		}
	}
}
