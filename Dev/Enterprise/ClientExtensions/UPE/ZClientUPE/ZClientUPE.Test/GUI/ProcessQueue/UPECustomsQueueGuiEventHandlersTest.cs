using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	public class UPECustomsQueueGuiEventHandlersTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestHookUnhookWithNullUPEJobDeclaration()
		{
			using (UPEProcessQueueGuiEventHandlers helper = new UPEProcessQueueGuiEventHandlers(null))
			{
				helper.HookEvents();
				helper.UnhookEvents();
			}
		}

		[TestDate(2006, 5, 5)]
		public void TestHandlesAskHasEIRBeenRaised()
		{
			UPECargoReportQueue uPECargoReportQueue = Factory.New<UPECargoReportQueue>();
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = Factory.New<UPECusMAWB>().PK;
			uPECargoReportQueue.Parent = uPECusHAWB;
			uPECargoReportQueue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.EIR;
			uPECargoReportQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.DEIR.Codes.AM_RefusedCancelledOrder;
			uPECargoReportQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			Factory.Save();
			BusinessObjectFactory factoryForReload = new BusinessObjectFactory();
			uPECargoReportQueue = factoryForReload.Load<UPECargoReportQueue>(uPECargoReportQueue.PK);
			using (UPEProcessQueueGuiEventHandlers guiEventHandlers = new UPEProcessQueueGuiEventHandlers(uPECargoReportQueue))
			{
				guiEventHandlers.HookEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				uPECargoReportQueue.P4_CustomsReason = "Test";
				uPECargoReportQueue.Parent = uPECusHAWB;
				uPECusHAWB.CS_GoodsDescription = "TEST";
				factoryForReload.Save();
				AssertEquals(new ZDateTime(2006, 5, 5), uPECargoReportQueue.P4_CustomDate4);
				guiEventHandlers.UnhookEvents();
			}
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
