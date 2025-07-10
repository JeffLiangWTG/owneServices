using System.Windows.Forms;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.UCC6TemporaryStorage.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageForm))]
	sealed class UCC6TemporaryStorageFormTest : ZFormBasherTest
	{
		public void TestBillsTabPage()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_JobReference = "JobReference";
			tempStorage.AMA_ManifestType = FRConstants.TemporaryStorage.AppCodeIST;
			using (var form = new UCC6TemporaryStorageForm(tempStorage))
			{
				var billsTabPage = form.FindSingle<ZTabPage>("BillsTabPage");
				AssertEquals("Bills tab label should be changed to Bills(IST) according to AMA_ManifestType.", "Bills(IST)", billsTabPage.CaptionResourceString.Caption);
			}
			tempStorage.AMA_ManifestType = FRConstants.TemporaryStorage.AppCodeLAD;
			using (var form = new UCC6TemporaryStorageForm(tempStorage))
			{
				var billsTabPage = form.FindSingle<ZTabPage>("BillsTabPage");
				AssertEquals("Bills tab label should be changed to Bills(LADT) according to AMA_ManifestType.", "Bills(LADT)", billsTabPage.CaptionResourceString.Caption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var tempStorageHeader = Factory.New<TemporaryStorageHeader>();
			var bill = tempStorageHeader.Bills.AddNew();
			bill.FillWithValidTestData();
			bill.PackedItems.AddNew().FillWithValidTestData();
			var presentationCustomsOfficeCode = tempStorageHeader.PresentationCustomsOfficeCode;
			Factory.Save();
			var form = new UCC6TemporaryStorageForm(tempStorageHeader);
			form.ControllerID = ControllerIDs.Customs.EU.UCC6TemporaryStorage;
			return form;
		}
	}
}
