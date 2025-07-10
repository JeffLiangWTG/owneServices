using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(CusTempStorageForm))]
	public class CusTempStorageFormTest : ZFormBasherTest
	{
		public void TestCheckOnShowPreSaveDialogs_EndDateWarning()
		{
			SetUpJobHeader();
			var jobHeader = header;
			var line = this.line;
			line.TSL_OwnerReferenceType = "AWB";
			line.TSL_OwnerReferenceNumber = "UNITTEST";
			line.TSL_LocationOfGoods = "ORIGINAL";
			line.TSL_GoodsDescription = "Original Goods";
			line.TSL_PackageQty = 25;
			line.TSL_GrossWeight = 50.0m;

			using (var form = new CusTempStorageForm(jobHeader))
			{
				form.Show();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = "TestSave";
				jobHeader.SJH_OH_Customer = orgHeader.PK;
				jobHeader.SJH_TempStorageEndDateUtc = ZDateTime.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				jobHeader.SJH_TempStorageEndDateUtc = ZDateTime.Now;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FireSaveButton();
				AssertEquals("End date cannot be amended once saved. Do you want to continue saving the temporary storage?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			SetUpJobHeader();
			Factory.Save();

			return new CusTempStorageForm(header)
			{
				ControllerID = ControllerIDs.Customs.TemporaryStorage
			};
		}

		void SetUpJobHeader()
		{
			header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = "IST";
			tempStorageDec = GetNewCusTempStorageDec(header);
			line = tempStorageDec.CusTempStorageLines.AddNew();
		}

		CusTempStorageJobHeader header;
		CusTempStorageLine line;
		CusTempStorageDec tempStorageDec;

		protected virtual CusTempStorageDec GetNewCusTempStorageDec(CusTempStorageJobHeader header)
		{
			var result = CusTempStorageDec.New(header);
			return result;
		}
	}
}
