using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using static Enterprise.Customs.DE.NCTS.Business.Testing.NCTSConditionalFunctionalityTestHelper;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsAdditionalInfo))]
	sealed class NctsAdditionalInfoTest : EU.NCTS.Business.Testing.NctsAdditionalInfoTest<NctsAdditionalInfo>
	{
		public void TestCSI_ReferenceNumber_MaxLength_Departure()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var additionalInfo = GetNewBusinessObject(Factory);
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals(70, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertEquals(70, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber_MaxLength_Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			var additionalInfo = goodsItem.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals(70, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertEquals(70, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber_MaxLength_DuringTransition()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var additionalInfo = GetNewBusinessObject(Factory);
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals(35, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertEquals(35, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestGetValidation()
		{
			var additionalInfo = GetNewBusinessObject(Factory);
			AssertType<NctsAdditionalInfoValidation>(additionalInfo.Validation);
		}

		public void TestGetValidation_Arrival()
		{
			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = arrivalNctsHeader.Bills.AddNew();
			var arrivalGoodsItem = bill.ArrivalGoodsItems.AddNew();
			var arrivalAdditionalInfo = arrivalGoodsItem.AdditionalInfos.AddNew();

			AssertType<CusSupportingInfoDisabledValidation>(arrivalAdditionalInfo.Validation);
		}

		public void TestReadOnlyProviderType()
		{
			CombineAssertions(() =>
			{
				AssertType<NctsAdditionalInfoReadOnlyProvider>(
					CreateHeaderAdditionalDocument(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival).GetNewReadOnlyProvider());

				AssertType<NctsAdditionalInfoReadOnlyProvider>(
					CreateHeaderAdditionalDocument(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure, false).GetNewReadOnlyProvider());
			});
		}

		NctsAdditionalInfo GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var additionalInfo = goodsItem.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			return additionalInfo;
		}

		#region Implementation

		NctsAdditionalInfoForTest CreateHeaderAdditionalDocument(string phase, string movementType, bool parentIsHeader = true)
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(movementType);
			header.BH_ApplicationCode = phase;
			var additionalDocument = Factory.New<NctsAdditionalInfoForTest>();

			if (parentIsHeader)
			{
				additionalDocument.AttachToParent(movementType == NctsMovementType.Codes.Departure ? header : header.ArrivalMovementHeader);
			}
			else
			{
				var bill = header.Bills.AddNew();
				NctsCommonCargoDesc goodsItem;
				if (movementType == NctsMovementType.Codes.Arrival)
				{
					goodsItem = bill.ArrivalGoodsItems.AddNew();
				}
				else
				{
					goodsItem = bill.GoodsItems.AddNew();
				}

				additionalDocument.AttachToParent(goodsItem);
			}

			return additionalDocument;
		}

		class NctsAdditionalInfoForTest : NctsAdditionalInfo
		{
			public NctsAdditionalInfoForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public void AttachToParent(BusinessObject parent)
			{
				CSI_ParentTableCode = parent.TablePrefix;
				CSI_ParentID = parent.PK;
			}

			public new IAdditionalDocumentReadOnlyProvider GetNewReadOnlyProvider() => base.GetNewReadOnlyProvider();

			protected override bool AutomaticSequenceNumberEnabled => false;
		}

		#endregion
	}
}
