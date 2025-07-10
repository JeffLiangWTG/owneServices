using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPEJobRelatedWayBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEB_WaybillNumber()
		{
			WayBill1.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Parent;
			WayBill1.Validation.ValidateEB_WaybillNumber();
			AssertMandatoryValidationError(WayBill1.EB_WaybillNumberInfo, false);
			WayBill1.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Child;
			WayBill1.Validation.ValidateEB_WaybillNumber();
			AssertMandatoryValidationError(WayBill1.EB_WaybillNumberInfo, true);
			WayBill1.EB_WaybillNumber = "NEWNUMBER";
			WayBill1.Validation.ValidateEB_WaybillNumber();
			AssertMandatoryValidationError(WayBill1.EB_WaybillNumberInfo, false);
			WayBill2.EB_WaybillNumber = "NEWNUMBER";
			WayBill2.Validation.ValidateEB_WaybillNumber();
			Assert("Should have error", WayBill2.EB_WaybillNumberInfo.HasErrors());
			Assert(WayBill2.EB_WaybillNumberInfo.Name + "should have a unique property validation", WayBill2.EB_WaybillNumberInfo.GetErrors().GetFirstMessage().IndexOf("duplicated and must be unique.") > -1);
			WayBill2.EB_WaybillNumber = "OTHERNUMBER";
			WayBill2.Validation.ValidateEB_WaybillNumber();
			Assert("Should not have any errors now", !WayBill2.EB_WaybillNumberInfo.HasErrors());
			WayBill3.EB_WaybillNumber = "OTHERNUMBER";
			Assert("Should not have any errors, as this waybill is not in collection", !WayBill3.EB_WaybillNumberInfo.HasErrors());
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		UPEJobRelatedWayBill WayBill1
		{
			get
			{
				if (fWayBill1 == null)
				{
					fWayBill1 = Collection.AddNew();
				}

				return fWayBill1;
			}
		}

		UPEJobRelatedWayBill WayBill2
		{
			get
			{
				if (fWayBill2 == null)
				{
					fWayBill2 = Collection.AddNew();
				}

				return fWayBill2;
			}
		}

		UPEJobRelatedWayBill WayBill3
		{
			get
			{
				if (fWayBill3 == null)
				{
					fWayBill3 = Collection.AddNew();
				}

				return fWayBill2;
			}
		}

		WayBillChildPackageCollection Collection
		{
			get
			{
				if (fCollection == null)
				{
					fCollection = new WayBillChildPackageCollection(HouseBill);
				}

				return fCollection;
			}
		}

		UPECusHAWB HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					fHouseBill = Factory.New<UPECusHAWB>();
				}

				return fHouseBill;
			}
		}

		UPEJobRelatedWayBill fWayBill1;
		UPEJobRelatedWayBill fWayBill2;
		UPEJobRelatedWayBill fWayBill3;
		WayBillChildPackageCollection fCollection;
		UPECusHAWB fHouseBill;
		#endregion
	}
}
