using System;
using CargoWise.Types;
using Enterprise.Client.JAS.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCForwardingSeaConsolValidationTest : JXCForwardingConsolObsoleteValidationTest
	{
		public void TestValidateJK_RL_NKLoadPort()
		{
			AssertHasNoJXCWarnings("Pre-condition", Consol.JK_RL_NKLoadPortInfo);
			Consol.JK_RL_NKLoadPort = "TB252";
			AssertHasInvalidCodeJXCWarning(Consol.JK_RL_NKLoadPortInfo);
			Consol.JK_RL_NKLoadPort = "";
			AssertHasNotEnteredJXCWarning(Consol.JK_RL_NKLoadPortInfo);
			Consol.JK_RL_NKLoadPort = "AUSYD";
			AssertHasNoJXCWarnings(Consol.JK_RL_NKLoadPortInfo);
		}

		public void TestValidateJK_RL_NKDischargePort()
		{
			AssertHasNoJXCWarnings("Pre-condition", Consol.JK_RL_NKDischargePortInfo);
			Consol.JK_RL_NKDischargePort = "RW201";
			AssertHasInvalidCodeJXCWarning(Consol.JK_RL_NKDischargePortInfo);
			Consol.JK_RL_NKDischargePort = "";
			AssertHasNotEnteredJXCWarning(Consol.JK_RL_NKDischargePortInfo);
			Consol.JK_RL_NKDischargePort = "ITMIL";
			AssertHasNoJXCWarnings(Consol.JK_RL_NKDischargePortInfo);
		}

		public void TestValidateJK_OA_ShippingLineAddress()
		{
			try
			{
				JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
				AssertHasNoJXCWarnings("Pre-condition", Consol.JK_OA_ShippingLineAddressInfo);
				Consol.SetDefaultShippingLineAddress(ZGuid.Empty);
				Consol.Validation.ValidateJK_OA_ShippingLineAddress();
				AssertHasNotEnteredJXCWarning(Consol.JK_OA_ShippingLineAddressInfo);
				Consol.SetDefaultShippingLineAddress(Factory.New<JASOrgHeader>());
				string expectedWarning1 = JXCConstants.JXCWarningPrefix + "Organisation Name cannot be blank";
				string expectedWarning2 = JXCConstants.JXCWarningPrefix + "JAS SSL Code does not exist for this Carrier. Please use the Config tab in the Organisation screen to setup the SSL code mapping";
				AssertHasJXCWarning(Consol.JK_OA_ShippingLineAddressInfo, expectedWarning1, expectedWarning2);
				Consol.ShippingLine.OH_FullName = "FULLName";
				Consol.Validation.ValidateJK_OA_ShippingLineAddress();
				AssertHasJXCWarning(Consol.JK_OA_ShippingLineAddressInfo, expectedWarning2);
				OrgPatternMatchOverride @override = Consol.ShippingLine.CreatePatternMatchOverrideForTest();
				@override.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
				@override.OO_LocalGuid = JASDataRegistry.Instance.JASWWOrganisationPK;
				@override.OO_ForeignCode = "TEST";
				Consol.Validation.ValidateJK_OA_ShippingLineAddress();
				AssertHasNoJXCWarnings(Consol.JK_OA_ShippingLineAddressInfo);
			}
			finally
			{
				JASDataRegistryTest.UnsetJASWWOrganisationItemForTest();
			}
		}

		public void TestValidateJK_MasterBillNum()
		{
			Consol.JK_MasterBillNum = "";
			Consol.Validation.ValidateJK_MasterBillNum();
			AssertHasNoJXCWarnings("Should allow empty OBL", Consol.JK_MasterBillNumInfo);
			Consol.JK_MasterBillNum = "123456789012345678901";
			AssertHasMaxLengthJXCWarning(Consol.JK_MasterBillNumInfo, JXCConstants.OHBLFieldBoundaries.BillOfLadingMaxLength);
			Consol.JK_MasterBillNum = "12345678901234567890";
			AssertHasNoJXCWarnings(Consol.JK_MasterBillNumInfo);
		}

		protected override Type ValidationTypeToTest
		{
			get
			{
				return typeof(JXCForwardingSeaConsolValidation);
			}
		}
	}
}
