using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(ArrivalCusTransportMeans))]
	sealed class ArrivalCusTransportMeansTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTPM_TransportState_ReadOnly()
		{
			CombineAssertions(() =>
			{
				Factory.Save();
				arrivalCusTransportMeans.TPM_TransportState = "NEW";
				AssertEquals("Is NEW, was NEW before", true, arrivalCusTransportMeans.TPM_TransportStateInfo.ReadOnly);
				Factory.Save();
				arrivalCusTransportMeans.TPM_TransportState = "DIF";
				AssertEquals("Is not NEW, was NEW before", false, arrivalCusTransportMeans.TPM_TransportStateInfo.ReadOnly);
				Factory.Save();
				arrivalCusTransportMeans.TPM_TransportState = "MIS";
				AssertEquals("Is not NEW, was not NEW before", false, arrivalCusTransportMeans.TPM_TransportStateInfo.ReadOnly);
				Factory.Save();
				arrivalCusTransportMeans.TPM_TransportState = "NEW";
				AssertEquals("Is NEW, was not NEW before", false, arrivalCusTransportMeans.TPM_TransportStateInfo.ReadOnly);
				arrivalCusTransportMeans.TPM_TransportState = ZString.Empty;
				AssertEquals("Is Empty", false, arrivalCusTransportMeans.TPM_TransportStateInfo.ReadOnly);
			});
		}

		public void TestTPM_SequenceNumber()
		{
			NCTSTestHelper.AssertCaptions(arrivalCusTransportMeans.TPM_SequenceNumberInfo, "Sequence", string.Empty, "Seq.");
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("Default state", NctsUnloadedStateList.Codes.NEW, arrivalCusTransportMeans.TPM_TransportState);
		}

		public void TestTPM_TransportState_Caption()
		{
			NCTSTestHelper.AssertCaptions(arrivalCusTransportMeans.TPM_TransportStateInfo, "Unloaded state", string.Empty, "State");
		}

		public void TestTPM_TypeOfIdentification()
		{
			NCTSTestHelper.AssertCaptions(arrivalCusTransportMeans.TPM_TypeOfIdentificationInfo, "Type of Identification", "Type of ID", "Type");
		}

		public void TestTPM_IdentificationNumber()
		{
			NCTSTestHelper.AssertCaptions(arrivalCusTransportMeans.TPM_IdentificationNumberInfo, "Transport Identification", "Transport ID", "Transp. ID");
		}

		public void TestTPM_RN_NKTransportNationality()
		{
			NCTSTestHelper.AssertCaptions(arrivalCusTransportMeans.TPM_RN_NKTransportNationalityInfo, "Nationality", string.Empty, "Nat.");
		}

		public void TestLookups()
		{
			AssertType<ArrivalCusTransportMeansLookups>(arrivalCusTransportMeans.Lookups);
		}

		public void TestValidation()
		{
			AssertType<ArrivalCusTransportMeansValidation>(arrivalCusTransportMeans.Validation);
		}

		public void TestSequenceNumber()
		{
			AssertEquals((ZShort)1, arrivalCusTransportMeans.SequenceNumber);

			var cusTransportMeans2 = nctsBill.ArrivalTransportInfos.AddNew();
			AssertEquals((ZShort)2, cusTransportMeans2.SequenceNumber);
		}

		public void TestSequenceNumber_ReadOnly()
		{
			AssertEquals(true, arrivalCusTransportMeans.TPM_SequenceNumberInfo.ReadOnly);
		}

		public void TestFKToHeader()
		{
			AssertEquals(arrivalCusTransportMeans.TPM_ParentID, arrivalCusTransportMeans.FKToHeader);
		}

		public void TestParentBill()
		{
			AssertEquals("Parent should be of type NctsBill", typeof(NctsBill), arrivalCusTransportMeans.Parent.GetType());
		}

		public void TestParentMovementHeader()
		{
			AssertEquals("Parent should be of type NctsBill", typeof(NctsArrivalMovementHeader), arrivalCusTransportMeans_Movement.Parent.GetType());
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalCusTransportMeans.MovementHeader, x => ((ArrivalCusTransportMeans)x).AreUnloadingRemarksFullyAccepted, arrivalCusTransportMeans);

			var arrivalCusTransportMeansNoHeader = Factory.New<ArrivalCusTransportMeans>();
			AssertEquals("Not Accepted", false, arrivalCusTransportMeansNoHeader.IsUnloadingRemarksReadOnly);
		}

		public void TestTPM_TransportState_ReadOnlyForAcceptedUnloading()
		{
			arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalCusTransportMeans.MovementHeader, x => ((ArrivalCusTransportMeans)x).TPM_TransportStateInfo.ReadOnly, arrivalCusTransportMeans);
		}

		public void TestTPM_TypeOfIdentification_ReadOnlyForAcceptedUnloading()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalCusTransportMeans.MovementHeader, x => ((ArrivalCusTransportMeans)x).TPM_TypeOfIdentificationInfo.ReadOnly, arrivalCusTransportMeans);
		}

		public void TestTPM_IdentificationNumber_ReadOnlyForAcceptedUnloading()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalCusTransportMeans.MovementHeader, x => ((ArrivalCusTransportMeans)x).TPM_IdentificationNumberInfo.ReadOnly, arrivalCusTransportMeans);
		}

		public void TestTPM_RN_NKTransportNationality_ReadOnlyForAcceptedUnloading()
		{
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalCusTransportMeans.MovementHeader, x => ((ArrivalCusTransportMeans)x).TPM_RN_NKTransportNationalityInfo.ReadOnly, arrivalCusTransportMeans);
		}

		public void TestUnloadingRemarksSentToCustoms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Accepted", false, arrivalCusTransportMeans.IsUnloadingRemarksReadOnly);
				arrivalCusTransportMeans.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Accepted", true, arrivalCusTransportMeans.IsUnloadingRemarksReadOnly);

				var arrivalCusTransportMeansNoHeader = Factory.New<ArrivalCusTransportMeans>();
				AssertEquals("Not Accepted", false, arrivalCusTransportMeansNoHeader.IsUnloadingRemarksReadOnly);
			});
		}

		public void TestTPM_TransportState_ReadOnlyForSentToCustoms()
		{
			CombineAssertions(() =>
			{
				arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("This field should be Editable for NCTS arrival phase 5", false, arrivalCusTransportMeans.TPM_TransportStateInfo.ReadOnly);
				arrivalCusTransportMeans.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalCusTransportMeans.TPM_TransportStateInfo.ReadOnly);
			});
		}

		public void TestTPM_TypeOfIdentification_ReadOnlyForSentToCustoms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("This field should be Editable for NCTS arrival phase 5", false, arrivalCusTransportMeans.TPM_TypeOfIdentificationInfo.ReadOnly);
				arrivalCusTransportMeans.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalCusTransportMeans.TPM_TypeOfIdentificationInfo.ReadOnly);
			});
		}

		public void TestTPM_IdentificationNumber_ReadOnlyForSentToCustoms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("This field should be Editable for NCTS arrival phase 5", false, arrivalCusTransportMeans.TPM_IdentificationNumberInfo.ReadOnly);
				arrivalCusTransportMeans.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalCusTransportMeans.TPM_IdentificationNumberInfo.ReadOnly);
			});
		}

		public void TestTPM_RN_NKTransportNationality_ReadOnlyForSentToCustoms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("This field should be Editable for NCTS arrival phase 5", false, arrivalCusTransportMeans.TPM_RN_NKTransportNationalityInfo.ReadOnly);
				arrivalCusTransportMeans.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalCusTransportMeans.TPM_RN_NKTransportNationalityInfo.ReadOnly);
			});
		}

		public void TestParent()
		{
			CombineAssertions(() =>
			{
				AssertType<NctsBill>(arrivalCusTransportMeans.Parent);
				AssertSame(arrivalCusTransportMeans.Parent, nctsBill);

				arrivalCusTransportMeans = Factory.New<ArrivalCusTransportMeans>();
				AssertNull(arrivalCusTransportMeans.Parent);
			});
		}

		public void TestReasonForNotAbleToDelete()
		{
			AssertEquals("Cannot delete Transports which were entered by customs.", arrivalCusTransportMeans.ReasonForNotAbleToDelete);
		}

		public void TestCanDelete_Bill()
		{
			TestCanDelete(arrivalCusTransportMeans);
		}

		public void TestCanDelete_ArrivalMovement()
		{
			TestCanDelete(arrivalCusTransportMeans_Movement);
		}

		void TestCanDelete(ArrivalCusTransportMeans transportMeans)
		{
			CombineAssertions(transportMeans.Parent.GetType().ToString(), () =>
			{
				transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("ArrivalCusTransportMeans.TPM_TransportState = 'DEC'", false, transportMeans.CanDelete);

				transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
				AssertEquals("ArrivalCusTransportMeans.TPM_TransportState = 'MIS'", false, transportMeans.CanDelete);

				transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("ArrivalCusTransportMeans.TPM_TransportState = 'NEW'", true, transportMeans.CanDelete);
			});
		}

		public void TestTPM_TypeOfIdentification_ReadOnly_Bill()
		{
			Test_PropertyInfoReadOnly(arrivalCusTransportMeans, arrivalCusTransportMeans.TPM_TypeOfIdentificationInfo);
		}

		public void TestTPM_TypeOfIdentification_ReadOnly_ArrivalMovement()
		{
			Test_PropertyInfoReadOnly(arrivalCusTransportMeans_Movement, arrivalCusTransportMeans_Movement.TPM_TypeOfIdentificationInfo);
		}

		public void TestTPM_IdentificationNumber_ReadOnly_Bill()
		{
			Test_PropertyInfoReadOnly(arrivalCusTransportMeans, arrivalCusTransportMeans.TPM_IdentificationNumberInfo);
		}

		public void TestTPM_IdentificationNumber_ReadOnly_ArrivalMovement()
		{
			Test_PropertyInfoReadOnly(arrivalCusTransportMeans_Movement, arrivalCusTransportMeans_Movement.TPM_IdentificationNumberInfo);
		}

		public void TestTPM_RN_NKTransportNationality_ReadOnly_Bill()
		{
			Test_PropertyInfoReadOnly(arrivalCusTransportMeans, arrivalCusTransportMeans.TPM_RN_NKTransportNationalityInfo);
		}

		public void TestTPM_RN_NKTransportNationality_ReadOnly_ArrivalMovement()
		{
			Test_PropertyInfoReadOnly(arrivalCusTransportMeans_Movement, arrivalCusTransportMeans_Movement.TPM_RN_NKTransportNationalityInfo);
		}

		void Test_PropertyInfoReadOnly(ArrivalCusTransportMeans transportMeans, ZPropertyInfo info)
		{
			CombineAssertions($"{transportMeans.Parent.GetType()} - {info.HumanReadableName}", () =>
			{
				transportMeans.TPM_TransportState = "NEW";
				AssertEquals("State NEW", false, info.ReadOnly);
				transportMeans.TPM_TransportState = "DEC";
				AssertEquals("State DEC", true, info.ReadOnly);
				transportMeans.TPM_TransportState = "MIS";
				AssertEquals("State MIS", true, info.ReadOnly);
			});
		}

		public void TestUnloadedStatesList()
		{
			var list = arrivalCusTransportMeans.Lookups.TransportStateList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "DEC, MIS, NEW", list.CodesAsString);
				AssertSame("Cached", list, arrivalCusTransportMeans.Lookups.TransportStateList);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			return nctsHeader.Bills.AddNew().ArrivalTransportInfos.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => arrivalCusTransportMeans;
		protected override BusinessObject GetNewBusinessObject() => arrivalCusTransportMeans;

		protected override void SetUp()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsBill = nctsHeader.Bills.AddNew();
			arrivalCusTransportMeans = nctsBill.ArrivalTransportInfos.AddNew();
			arrivalCusTransportMeans_Movement = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		}

		ArrivalCusTransportMeans arrivalCusTransportMeans;
		ArrivalCusTransportMeans arrivalCusTransportMeans_Movement;
		NctsBill nctsBill;
		NctsHeader nctsHeader;
	}
}
