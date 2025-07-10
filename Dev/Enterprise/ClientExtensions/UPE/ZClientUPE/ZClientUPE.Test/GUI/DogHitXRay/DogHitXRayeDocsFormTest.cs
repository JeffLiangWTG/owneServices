using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.GUI.Testing
{
	class DogHitXRayeDocsFormTest : TestCaseWithFactory
	{
		public void TestFormCaption()
		{
			using (DogHitXRayeDocsForm form = new DogHitXRayeDocsForm(DogHitXRay.SelectedUPECusHAWB))
			{
				AssertEquals("Dog Hit X-Ray eDocs Form", form.FormHeading);
			}
		}

		public void TesteDocsPluginAdded()
		{
			using (DogHitXRayeDocsForm form = new DogHitXRayeDocsForm(DogHitXRay.SelectedUPECusHAWB))
			{
				AssertNotNull("eDocsPlugIn Plug-in should exist", form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull("ProcessQueue Plug-in should exist", form.PlugIns.GetPlugIn(ControllerIDs.ProcessQueue));
				AssertEquals(ProcessQueueType.Enum.Customs, form.UPECusHAWB.ActiveProcessQueueForBinding[0].QueueType);
			}
		}

		public void TestValidateCausesNoErrors()
		{
			using (DogHitXRayeDocsForm form = new DogHitXRayeDocsForm(DogHitXRay.SelectedUPECusHAWB))
			{
				form.Validate();
				AssertEquals(false, form.UPECusHAWB.HasErrors);
			}
		}

		DogHitXRay DogHitXRay
		{
			get
			{
				if (fDogHitXRay == null)
				{
					fDogHitXRay = new DogHitXRay(Factory);
					UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
					uPECusHAWB.CS_CM = (Factory.New<CusMAWB>()).PK;
					JobRelatedWayBill jobRelatedWayBill = Factory.New<JobRelatedWayBill>();
					jobRelatedWayBill.EB_ParentID = uPECusHAWB.PK;
					jobRelatedWayBill.EB_ParentTableCode = uPECusHAWB.TablePrefix;
					jobRelatedWayBill.EB_WaybillNumber = "111";
					jobRelatedWayBill.EB_WaybillShortNumber = "222";
					jobRelatedWayBill.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Parent;
					Factory.Save();
					fDogHitXRay.TrackingNumber = "111";
					fDogHitXRay.Remarks = "TEST";
				}

				return fDogHitXRay;
			}
		}

		DogHitXRay fDogHitXRay;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
