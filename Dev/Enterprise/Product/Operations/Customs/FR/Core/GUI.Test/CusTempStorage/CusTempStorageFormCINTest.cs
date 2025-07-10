using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	[TestedType(typeof(CINTemporyStorageUserControlForPlugin))]
	sealed class CusTempStorageFormCINTest : CusTempStorageFormAbstractTest
	{
		public void TestSaveClick()
		{
			var (header, line) = SetUpJobHeader();
			header.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			line.TSL_OwnerReferenceType = "AWB";
			line.TSL_OwnerReferenceNumber = "UNITTEST";
			line.TSL_LocationOfGoods = "ORIGINAL";
			line.TSL_GoodsDescription = "Original Goods";
			line.TSL_PackageQty = 25;
			line.TSL_GrossWeight = 50.0m;

			header.CusTempStorageDec.STH_MessageStatus = CusTempStorageDec.DeclarationStatusForCorrectionMessage;
			Factory.Save();

			using (var frm = new CusTempStorageForm(header))
			{
				frm.Show();

				line.TSL_PackageQty = 27; // Force Change

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				frm.FireSaveButton();
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCheckOnShowPreSaveDialogs()
		{
			var (header, line) = SetUpJobHeader();
			line.TSL_OwnerReferenceType = "AWB";
			line.TSL_OwnerReferenceNumber = "UNITTEST";
			line.TSL_LocationOfGoods = "ORIGINAL";
			line.TSL_GoodsDescription = "Original Goods";
			line.TSL_PackageQty = 25;
			line.TSL_GrossWeight = 50.0m;
			header.CusTempStorageDec.STH_MessageStatus = CusTempStorageDec.DeclarationStatusForCorrectionMessage;

			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = "TestSave";
				header.SJH_OH_Customer = orgHeader.PK;
				header.SJH_TempStorageEndDateUtc = ZDateTime.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				header.SJH_TempStorageEndDateUtc = ZDateTime.Now;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FireSaveButton();
				AssertEquals("End date cannot be amended once saved. Do you want to continue saving the temporary storage?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Type TemporyStorageDecUserControlType => typeof(CusTempStorageDecUserControl);

		protected override CINTemporyStorageUserControlForPlugin TemporyStorageUserControlForPlugin => new CINTemporyStorageUserControlForPlugin();

		protected override ZString TemporaryStorageHeaderApplicationCode => FRConstants.TemporaryStorage.AppCodeFRC;

		(CusTempStorageJobHeader Header, CusTempStorageLine Line) SetUpJobHeader()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = TemporaryStorageHeaderApplicationCode;
			var storageDec = FRCCusTempStorageDec.New(header);
			var line = (CusTempStorageLine)storageDec.CusTempStorageLines.AddNew();
			return (header, line);
		}
	}
}
