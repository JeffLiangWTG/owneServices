using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(NctsDepartureMovementHeader))]
	class NctsDepartureMovementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAllowMixedCaseAuthorisationNumbers()
		{
			AssertEquals(true, departureMovement.AllowMixedCaseAuthorisationNumbers);
		}

		public void TestMessages()
		{
			var messages = departureMovement.Messages;
			AssertEquals(departureMovement, messages.Master);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			messages = nctsHeader.LinkedMessages;
			AssertEquals(departureMovement, messages.Master);
		}

		public void TestGoodsItems()
		{
			AssertType<NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>(departureMovement.GoodsItems);
		}

		public void TestValidation()
		{
			AssertType<NctsDepartureMovementHeaderValidation>(departureMovement.Validation);
		}

		public void TestTirCarnetNumber_AddTirSupportingDocIfNeeded()
		{
			CombineAssertions(() =>
			{
				departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				AssertEquals("Empty Carnet Number", 0, departureMovement.GoodsItems.Count);
				departureMovement.TirCarnetNumber = "DX00000000";
				var supportingDocument = departureMovement.GoodsItems[0].SupportingDocuments[0];
				AssertEquals("Supporting Document Type", NctsHeaderValidationHelper.TirCarnetDocumentCode, supportingDocument.CSI_Code);
				AssertEquals("Supporting Document Number", "DX00000000", supportingDocument.CSI_ReferenceNumber);

				departureMovement.TirCarnetNumber = "DX11111111";
				AssertEquals("Only single supporting document", 1, departureMovement.GoodsItems[0].SupportingDocuments.Count);
				supportingDocument = departureMovement.GoodsItems[0].SupportingDocuments[0];
				AssertEquals("Same Document Type", NctsHeaderValidationHelper.TirCarnetDocumentCode, supportingDocument.CSI_Code);
				AssertEquals("Updated Document Number", "DX11111111", supportingDocument.CSI_ReferenceNumber);
			});
		}

		public void TestGuarantees()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertType<NctsGuaranteeCollection<Guarantee>>(nctsHeader.MovementHeader.Guarantees);
		}

		public void TestFallbackInformation()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_JobReference = "NCTS12345";
			var officeCode = departureMovement.CustomsOffices.AddNew();
			officeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			officeCode.CY_Data = "GB12345";
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader.MovementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, officeCode.CY_Data, ZDateTime.Empty, false);
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CE1", nctsHeader.Consignor, "3");
			var cusAuthorizationHeader = CreateCusAuthorizationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, Factory.NewWithValidTestData<OrgHeader>());
			nctsHeader.Principal.OrganisationPK = cusAuthorizationHeader.CPH_OH_PermitHolder;
			Factory.Save();

			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.IsSimplifiedNctsProcedure = true;
			AssertEquals(ZString.Empty, nctsHeader.FallbackInformation);
			GBCustomsDataRegistry.Instance.NCTS_Fallback_Business_Continuity_Is_Active.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(ZString.Empty, nctsHeader.FallbackInformation);

			var expectedFallbackInformation = FormattableString.Invariant($@"UK {officeCode.CY_OfficeDescription}
{nctsHeader.BH_JobReference}             {ZDateTime.Today:dd/MM/yy}
{nctsHeader.Consignor.E2_CompanyName}    {cusAuthorizationHeader.CPH_Number}");

			GBCustomsDataRegistry.Instance.NCTS_Fallback_Business_Continuity_Is_Active.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(expectedFallbackInformation, nctsHeader.FallbackInformation);
		}

		CusAuthorisationHeader CreateCusAuthorizationHeader(string type, OrgHeader organization)
		{
			var cusAuthorisationHeader = Factory.New<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_Type = type;
			cusAuthorisationHeader.CPH_OH_PermitHolder = organization.PK;
			cusAuthorisationHeader.CPH_Number = "123";
			cusAuthorisationHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			cusAuthorisationHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			return cusAuthorisationHeader;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader.MovementHeader;
		}

		protected override BusinessObject GetNewBusinessObject() => departureMovement;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;
		}
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
	}
}
