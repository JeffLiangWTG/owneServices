using System.Linq;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AWBSendChileWrapperReferenceTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestAWBSendChileWrapperReference()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.RegistrationNumber = "211054";

			var entryNum = CusEntryNumber.Load(header, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Chile, false);
			entryNum.CE_IssueDate = new ZDate(2021, 04, 04);

			CreateAndPopulateHouseBill();
			CreateAndPopulateMasterBill();

			IAWBRequest wrapper = new AWBSendChileWrapper(header.Bills[0], WrappersConstants.ActionType.A, WrappersConstants.ObservationName.Gral);

			IDocReferences reference1 = wrapper.DocReferences.ElementAt(0);
			IDocReferences reference2 = wrapper.DocReferences.ElementAt(1);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(2, wrapper.DocReferences.Count);

				AssertEquals(WrappersConstants.ReferenceType.Ref, reference1.ReferenceType);
				AssertEquals(WrappersConstants.DocumentType.Mftoa, reference1.DocumentType);
				AssertEquals("211054", reference1.Number);
				AssertEquals("04-04-2021", reference1.Date);

				AssertEquals(WrappersConstants.ReferenceType.Madre, reference2.ReferenceType);
				AssertEquals(WrappersConstants.DocumentType.Ga, reference2.DocumentType);
				AssertEquals("MEDUQ2394375", reference2.Number);
				AssertEquals("05-06-2020", reference2.Date);
			});
		}

		void CreateAndPopulateHouseBill()
		{
			AsycudaBill bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_GrossWeight = 127.000m;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_ManifestQty = 1;
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.ABL_RL_NKFinalDestination = "CLSCL";
			bill.ABL_RL_NKOrigin = "UYMVD";
			bill.ABL_RL_NKPortOfDischarge = "CLSCL";
			bill.ABL_RoRo = true;
			bill.ABL_Volume = 1000m;
			bill.ABL_VolumeUQ = "M3";
		}

		void CreateAndPopulateMasterBill()
		{
			AsycudaBill bill = (AsycudaBill)header.MasterBill;
			bill.ABL_BillNumber = "MEDUQ2394375";
			bill.ABL_BillIssueDate = new ZDate(2020, 06, 05);
		}
	}
}
