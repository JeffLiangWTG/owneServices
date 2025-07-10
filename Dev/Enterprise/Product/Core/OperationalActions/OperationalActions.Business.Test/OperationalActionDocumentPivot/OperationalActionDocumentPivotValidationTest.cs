using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionDocumentPivotValidationTest : BusinessObjectValidationTestCase
	{
		OperationalActionDocumentPivot pivot;
		OperationalActionDocumentPivotCollection pivots;
		public void TestValidateSF_IsSystemDefined()
		{
			Pivot.Action.SU_IsSystemDefined = true;
			Pivot.SF_IsSystemDefined = false;
			AssertHasError(Pivot.SF_IsSystemDefinedInfo, "User-defined relationships cannot be added to system-defined Operational Actions.");
			Pivot.SF_IsSystemDefined = true;
			AssertNoErrors(Pivot.SF_IsSystemDefinedInfo);
			Pivot.Action.SU_IsSystemDefined = false;
			Pivot.Validation.ValidateSF_IsSystemDefined();
			AssertHasError(Pivot.SF_IsSystemDefinedInfo, "System-defined relationships cannot be added to user-defined Operational Actions.");
			Pivot.SF_IsSystemDefined = false;
			AssertNoErrors(Pivot.SF_IsSystemDefinedInfo);
		}

		public void TestValidateSF_SU_Outward()
		{
			Pivot.SF_SU_Outward = ZGuid.Empty;
			AssertHasError(Pivot.SF_SU_OutwardInfo, "Please enter a " + Pivot.SF_SU_OutwardInfo.Description + ".");
			DocumentCommand document1 = Factory.New<DocumentCommand>();
			DocumentCommand document2 = Factory.New<DocumentCommand>();
			document1.SU_BusinessContext = MockOperationalActionSupportable.BusinessContext.ToString();
			document2.SU_BusinessContext = nameof(BusinessContext.Test);
			document1.SU_GS_NKStaffCode = "";
			document2.SU_GS_NKStaffCode = "";
			document1.SU_ContactType = ContactType.Consignor.Code;
			document2.SU_ContactType = ContactType.Consignor.Code;
			Pivot.SF_SU_Outward = document2.PK;
			AssertHasError(Pivot.SF_SU_OutwardInfo, "Enter a valid " + Pivot.SF_SU_OutwardInfo.Description + ".");
			Pivot.SF_SU_Outward = document1.PK;
			AssertNoErrors(Pivot.SF_SU_OutwardInfo);
		}

		public void TestValidateSF_SU_OutwardWithNCTDocGroup_ShouldNotShowError()
		{
			Pivot.SF_SU_Outward = ZGuid.Empty;
			AssertHasError(Pivot.SF_SU_OutwardInfo, "Please enter a " + Pivot.SF_SU_OutwardInfo.Description + ".");
			var document = Factory.New<DocumentCommand>();
			document.SU_BusinessContext = MockOperationalActionSupportable.BusinessContext.ToString();
			document.SU_GS_NKStaffCode = "";
			document.SU_ContactType = ContactType.NoContactType.Code;
			Pivot.SF_SU_Outward = document.PK;
			AssertNoErrors(Pivot.SF_SU_OutwardInfo);
		}

		OperationalActionDocumentPivot Pivot
		{
			get
			{
				return pivot ?? (pivot = Pivots.AddNew());
			}
		}

		OperationalActionDocumentPivotCollection Pivots
		{
			get
			{
				if (pivots == null)
				{
					OperationalAction action = Factory.New<OperationalAction>();
					action.Context = new OperationalActionContext(new MockOperationalActionSupportable().OperationalActionSupporter, "Module Name");
					pivots = new OperationalActionDocumentPivotCollection(action);
				}

				return pivots;
			}
		}
	}
}
