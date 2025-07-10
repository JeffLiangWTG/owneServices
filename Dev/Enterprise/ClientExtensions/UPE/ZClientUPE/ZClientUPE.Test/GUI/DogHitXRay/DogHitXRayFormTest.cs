using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal class DogHitXRayFormTest : TestCaseWithFactory
	{
		public void TestFormCaption()
		{
			using (DogHitXRayForm form = new DogHitXRayForm(DogHitXRay))
			{
				AssertEquals("Dog Hit or X-Ray Hold", form.FormHeading);
			}
		}

		public void TestProcess()
		{
			DogHitXRay.TrackingNumber = "XXX";
			using (DogHitXRayFormTestClass dogHitXRayForm = new DogHitXRayFormTestClass(DogHitXRay))
			{
				dogHitXRayForm.Show();
				dogHitXRayForm.OkButton.PerformClick();
				AssertEquals("Error Please fix the errors before proceeding.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}

			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = (Factory.New<CusMAWB>()).PK;
			JobRelatedWayBill jobRelatedWayBill = Factory.New<JobRelatedWayBill>();
			jobRelatedWayBill.EB_ParentID = uPECusHAWB.PK;
			jobRelatedWayBill.EB_ParentTableCode = uPECusHAWB.TablePrefix;
			jobRelatedWayBill.EB_WaybillNumber = "111";
			jobRelatedWayBill.EB_WaybillShortNumber = "222";
			jobRelatedWayBill.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Parent;
			Factory.Save();
			DogHitXRay.TrackingNumber = "111";
			DogHitXRay.Remarks = "TEST";
			using (DogHitXRayFormTestClass dogHitXRayForm = new DogHitXRayFormTestClass(DogHitXRay))
			{
				UPECusHAWB rememberUPECusHAWB = DogHitXRay.SelectedUPECusHAWB;
				dogHitXRayForm.Show();
				dogHitXRayForm.OkButton.PerformClick();
				AssertEquals("TEST", rememberUPECusHAWB.CurrentQueue.P4_CustomsReason);
				AssertEquals(true, dogHitXRayForm.DogHitEDocsFormShown);
				AssertEquals("", DogHitXRay.TrackingNumber);
				AssertEquals(true, dogHitXRayForm.TrackingNumberTextBox.Focused);
			}
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			DogHitXRay = new DogHitXRay(Factory);
		}

		DogHitXRay DogHitXRay;
		#region DogHitXRayFormTestClass
		class DogHitXRayFormTestClass : DogHitXRayForm
		{
			public DogHitXRayFormTestClass(DogHitXRay dogHitXRay) : base(dogHitXRay)
			{
			}

			protected override void ShowDogHitEDocsForm()
			{
				DogHitEDocsFormShown = true;
			}

			public bool DogHitEDocsFormShown;
		}
		#endregion
	}
}
