using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class TNNNCTS5TransitOperationWrapperTest : WrapperHelperTest<TNNNCTS5TransitOperationWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<NctsHeader>();
				AssertExceptionThrown("Constructor Throws Exception if MovementHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "MovementHeader"), () => new NCTS5CommonTransitOperationWrapper(header));

				header.SetMovementType(NctsMovementType.Codes.Arrival);
				AssertExceptionThrown("Constructor Throws Exception if MovementHeader is null because header is arrival", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "MovementHeader"), () => new NCTS5CommonTransitOperationWrapper(header));
			});
		}

		public void TestCommonTransitOperation()
		{
			var commonTransitOperation = wrapper.CommonTransitOperation;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled CommonTransitOperation", commonTransitOperation);
				AssertSame("Cached CommonTransitOperation", wrapper.CommonTransitOperation, commonTransitOperation);
			});
		}

		public void TestDeclarationAcceptanceDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DeclarationAcceptanceDate when no IssueDate is declared for the mrn", ZDateTime.Empty, wrapper.DeclarationAcceptanceDate);

				var mrnEntryNumber = nctsHeader.MovementReferenceEntryNumber;
				var date = ZDateTime.Now;
				mrnEntryNumber.CE_IssueDate = date;
				AssertEquals("Expected filled DeclarationAcceptanceDate when IssueDate is declared for the mrn", date, wrapper.DeclarationAcceptanceDate);
			});
		}

		public void TestReleaseDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty ReleaseDate when no IssueDate is declared for the clearance number", ZDateTime.Empty, wrapper.ReleaseDate);

				var mrnEntryNumber = nctsHeader.ClearanceEntryNumber;
				var date = ZDateTime.Now;
				mrnEntryNumber.CE_IssueDate = date;
				AssertEquals("Expected filled ReleaseDate when IssueDate is declared for the clearance number", date, wrapper.ReleaseDate);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			wrapper = new TNNNCTS5TransitOperationWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		TNNNCTS5TransitOperationWrapper wrapper;

		protected override TNNNCTS5TransitOperationWrapper GetProvider() => wrapper;
	}
}
