using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal class CustomFlagFormTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			using (CustomFlagForm form = CustomFlagForm.New(RebillFlags.IsAbandoned, UPECusHAWB))
			{
				AssertEquals(typeof(CustomFlagForm), form.GetType());
				AssertEquals(typeof(AbandonDetails), form.BusinessEntity.GetType());
			}

			using (CustomFlagForm form = CustomFlagForm.New(RebillFlags.IsChangedToFreeDomicile, UPECusHAWB))
			{
				AssertEquals(typeof(CustomFlagForm), form.GetType());
				AssertEquals(typeof(FreeDomicileDetails), form.BusinessEntity.GetType());
			}

			using (CustomFlagForm form = CustomFlagForm.New(RebillFlags.IsRTS, UPECusHAWB))
			{
				AssertEquals(typeof(RTSTranshipmentForm), form.GetType());
				AssertEquals(typeof(RTSDetails), form.BusinessEntity.GetType());
			}

			using (CustomFlagForm form = CustomFlagForm.New(RebillFlags.IsTranshipment, UPECusHAWB))
			{
				AssertEquals(typeof(RTSTranshipmentForm), form.GetType());
				AssertEquals(typeof(TranshipmentDetails), form.BusinessEntity.GetType());
			}
		}

		public void TestFormHeading()
		{
			using (CustomFlagForm form = CustomFlagForm.New(RebillFlags.IsAbandoned, UPECusHAWB))
			{
				AssertEquals("Enter " + new AbandonDetails(UPECusHAWB).FlagName + " details:", form.FormHeading);
			}
		}

		public void TestOKDialogResult()
		{
			AbandonDetails flagDetails = new AbandonDetails(UPECusHAWB);
			using (CustomFlagFormForTest form = new CustomFlagFormForTest(flagDetails))
			{
				form.Show();
				form.DialogResult = DialogResult.OK;
				form.Close();
				AssertEquals("Should not be called, there are validation errors", false, form.OnValidOKButtonClickCalled);
				AssertEquals("Should not be created, there are validation errors", 0, UPECusHAWB.Notes.GetAllNotes().Count);
				AssertEquals("Should not be closed, there are validation errors", true, form.Visible);
				flagDetails.ClearAllNotifications();
				using (flagDetails.GetValidationSuspender())
				{
					form.DialogResult = DialogResult.OK;
					form.Close();
					AssertEquals("Should be called", true, form.OnValidOKButtonClickCalled);
					AssertEquals("Should be created", 1, UPECusHAWB.Notes.GetAllNotes().Count);
					AssertEquals("Should be closed", false, form.Visible);
				}
			}
		}

		public void TestCancelDialogResult()
		{
			AbandonDetails flagDetails = new AbandonDetails(UPECusHAWB);
			using (CustomFlagFormForTest form = new CustomFlagFormForTest(flagDetails))
			{
				form.Show();
				form.DialogResult = DialogResult.Cancel;
				form.Close();
				AssertEquals("Should be closed regardless of validation errors", false, form.Visible);
			}
		}

		UPECusHAWB UPECusHAWB
		{
			get
			{
				if (fUPECusHAWB == null)
				{
					fUPECusHAWB = Factory.New<UPECusHAWB>();
				}

				return fUPECusHAWB;
			}
		}

		UPECusHAWB fUPECusHAWB;
		#region CustomFlagFormForTest
		class CustomFlagFormForTest : CustomFlagForm
		{
			public CustomFlagFormForTest(AbandonDetails flagDetails) : base(flagDetails)
			{
			}

			protected override void OnOKDialogResult()
			{
				OnValidOKButtonClickCalled = true;
			}

			public bool OnValidOKButtonClickCalled;
		}

		#endregion
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
