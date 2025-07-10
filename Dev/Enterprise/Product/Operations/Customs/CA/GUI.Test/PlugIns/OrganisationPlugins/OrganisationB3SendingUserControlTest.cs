using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class OrganisationB3SendingUserControlTest : TestCaseWithFactory
	{
		public void TestCaptionsNotLostByStupidDumbDesigner()
		{
			using (var userControl = new OrganisationB3SendingUserControl())
			{
				AssertEquals("HVS", userControl.AutoSaveHVSDelayIntervalCalcEdit.CaptionResourceString.Caption);
				AssertEquals("CourierLVS-F", userControl.AutoSaveCONDelayIntervalCalcEdit.CaptionResourceString.Caption);
				AssertEquals("HVS", userControl.FailSafeHVSDelayIntervalCalcEdit.CaptionResourceString.Caption);
				AssertEquals("CourierLVS-F", userControl.FailSafeCONDelayIntervalCalcEdit.CaptionResourceString.Caption);
				AssertEquals("Normal Ship.", userControl.DeferredNormalB3SendActionDropEdit.CaptionResourceString.Caption);
				AssertEquals("Low Value Ship.", userControl.DeferredLowValueB3SendActionDropEdit.CaptionResourceString.Caption);
			}
		}
	}
}
