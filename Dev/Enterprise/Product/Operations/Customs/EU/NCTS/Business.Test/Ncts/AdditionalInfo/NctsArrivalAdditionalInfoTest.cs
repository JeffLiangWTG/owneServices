using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsAdditionalInfo))]
	sealed class NctsArrivalAdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<NctsAdditionalInfo>
	{
		public void TestCSI_Code_ReadOnly()
		{
			AssertPropertyIsReadOnlyWhenUnloadedStateIsDEC(x => x.CSI_CodeInfo);
		}

		public void TestCSI_LineNo_ReadOnly()
		{
			(_, var additionalInfo) = CreateData(Factory);
			AssertEquals("Phase 5, Arrival", true, additionalInfo.CSI_LineNoInfo.ReadOnly);
		}

		public void TestCSI_Status_ReadOnly()
		{
			(_, var additionalInfo) = CreateData(Factory);
			AssertEquals("Phase 5, Arrival", true, additionalInfo.CSI_StatusInfo.ReadOnly);
		}

		public void TestCSI_SubType_ReadOnly()
		{
			(_, var additionalInfo) = CreateData(Factory);
			AssertEquals("Phase 5, Arrival", false, additionalInfo.CSI_SubTypeInfo.ReadOnly);
		}

		public void TestCSI_ReferenceNumber_ReadOnly()
		{
			AssertPropertyIsReadOnlyWhenUnloadedStateIsDEC(x => x.CSI_ReferenceNumberInfo);
		}

		public void TestCSI_Description_ReadOnly()
		{
			(_, var additionalInfo) = CreateData(Factory);
			AssertEquals("Phase 5, Arrival", true, additionalInfo.CSI_DescriptionInfo.ReadOnly);
		}

		void AssertPropertyIsReadOnlyWhenUnloadedStateIsDEC(Func<NctsAdditionalInfo, ZPropertyInfo> getPropertyInfo)
		{
			(_, var additionalInfo) = CreateData(Factory);
			var propertyInfo = getPropertyInfo.Invoke(additionalInfo);
			CombineAssertions(() =>
			{
				additionalInfo.CSI_Status = ZString.Empty;
				AssertEquals("CSI_Status empty", false, propertyInfo.ReadOnly);

				foreach (var state in new NctsUnloadedStateList().GetAllCodes())
				{
					additionalInfo.CSI_Status = state;
					AssertEquals($"CSI_Status = '{state}'", readOnlyStates.Contains(state), propertyInfo.ReadOnly);
				}
			});
		}

		readonly ImmutableHashSet<ZString> readOnlyStates = ImmutableHashSet.Create<ZString>(NctsUnloadedStateList.Codes.DEC, NctsUnloadedStateList.Codes.MIS);

		public void TestCanDelete()
		{
			CombineAssertions(() =>
			{
				(_, var additionalInfo) = CreateData(Factory);
				additionalInfo.CSI_Status = ZString.Empty;
				AssertEquals("CSI_Status empty", true, additionalInfo.CanDelete);

				additionalInfo.CSI_Status = SupportingDocumentStatusList.Codes.DEC;
				AssertEquals("CSI_Status = 'DEC'", false, additionalInfo.CanDelete);

				additionalInfo.CSI_Status = SupportingDocumentStatusList.Codes.NEW;
				AssertEquals("CSI_Status = 'NEW'", true, additionalInfo.CanDelete);
			});
		}

		public void TestReasonForNotAbleToDelete()
		{
			(_, var additionalInfo) = CreateData(Factory);
			AssertEquals("Cannot delete Documents from Customs.", additionalInfo.ReasonForNotAbleToDelete);
		}

		protected override IEnumerable<NctsAdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return CreateData(factory).additionalInfo;
		}

		protected override BusinessObject GetNewBusinessObject() => CreateData(Factory).additionalInfo;

		static (NctsHeader nctsHeader, NctsAdditionalInfo additionalInfo) CreateData(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var additionalInfo = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().AdditionalInfos.AddNew();
			return (nctsHeader, additionalInfo);
		}
	}
}
