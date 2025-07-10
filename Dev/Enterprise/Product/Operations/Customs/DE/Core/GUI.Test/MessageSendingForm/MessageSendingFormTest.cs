using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm<MessageSendingActionParent>))]
	sealed class MessageSendingFormTest : ZFormBasherTest
	{
		public void TestFormDetailsForChangeCustodyInformation()
		{
			var temporaryStorageDec1 = JobHeader.CHGTSTCusTempStorageDecs.AddNew();
			temporaryStorageDec1.CusTempStorageLines.AddNew();
			temporaryStorageDec1.STH_OwnerReferenceNumber = "123";

			var temporaryStorageDec2 = JobHeader.CHGTSTCusTempStorageDecs.AddNew();
			temporaryStorageDec2.CusTempStorageLines.AddNew();
			temporaryStorageDec2.STH_OwnerReferenceNumber = "235";

			var parent = new MessageSendingActionParent(JobHeader, JobHeader.CHGTSTCusTempStorageDecs.Cast<CusTempStorageDec>(), x => ((CusTempStorageDec)x).STH_OwnerReferenceNumber, Env.Security.CustomsTemporaryStorageSendWithMessageErrors);
			using (var form = new MessageSendingForm<MessageSendingActionParent>(parent, "Change Custody Information"))
			{
				form.Show();
				CombineAssertions(() =>
				{
					var groupBox = (ZGroupBox)form.Controls.Find("MessageSendingObjectsGroupBox", true).Single();
					AssertEquals("GroupBox Text", "Messages to be sent", groupBox.Text);
					var grid = (ZGrid)form.Controls.Find("MessageSendingObjectsGrid", true).Single();
					AssertEquals("Columns", 2, grid.ColumnStyles.Count);
					AssertEquals("Form heading", "Send Change Custody Information", form.FormHeading);
				});
			}
		}

		public void TestFormDetailsForFinalSumAWithAPreliminary()
		{
			var temporaryStorageDec = CUSPRLCusTempStorageDec.New(JobHeader);
			temporaryStorageDec.CusTempStorageLines.AddNew();
			var parent = new FinalSumAWithAPreliminaryMessageSendingActionParent(JobHeader, temporaryStorageDec.CusTempStorageLines.Cast<CusTempStorageLine>(), Env.Security.CustomsTemporaryStorageSendWithMessageErrors);
			using (var form = new MessageSendingForm<FinalSumAWithAPreliminaryMessageSendingActionParent>(parent, "Final SumA with a Preliminary", "Lines to be sent"))
			{
				form.Show();
				CombineAssertions(() =>
				{
					var groupBox = (ZGroupBox)form.Controls.Find("MessageSendingObjectsGroupBox", true).Single();
					AssertEquals("GroupBox Text", "Lines to be sent", groupBox.Text);
					var grid = (ZGrid)form.Controls.Find("MessageSendingObjectsGrid", true).Single();
					AssertEquals("Column count", 9, grid.ColumnStyles.Count);
					AssertEquals("Form Heading", "Send Final SumA with a Preliminary", form.FormHeading);
				});
			}
		}

		public void TestSendWithAdditionalWarningCheckBox_Visibility()
		{
			var temporaryStorageDec = JobHeader.CHGOFFCusTempStorageDecs.AddNew();
			temporaryStorageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			var tempStorageLine = temporaryStorageDec.CusTempStorageLines.AddNew();
			tempStorageLine.TSL_CustodianIdentifier = "DE9000348";
			tempStorageLine.TSL_CustodianIdentifierBranchNo = string.Empty;
			tempStorageLine.Validation.ValidateTSL_CustodianIdentifierBranchNo();
			AssertHasWarnings("Eori Missing Branch Warning", tempStorageLine.TSL_CustodianIdentifierBranchNoInfo);
			var parent = new FinalSumAWithAPreliminaryMessageSendingActionParent(JobHeader, temporaryStorageDec.CusTempStorageLines.Cast<CusTempStorageLine>(), Env.Security.CustomsTemporaryStorageSendWithMessageErrors);
			using (var form = new MessageSendingForm<FinalSumAWithAPreliminaryMessageSendingActionParent>(parent, "Final SumA with a Preliminary", "Lines to be sent"))
			{
				form.Show();
				AssertEquals(false, form.FindSingle<ZCheckBox>("SendWithAdditionalWarningCheckBox").Visible);
			}
		}

		public void TestBizObjMessageErrorsExcludeParentErrors()
		{
			var temporaryStorageDec1 = JobHeader.CHGTSTCusTempStorageDecs.AddNew();
			temporaryStorageDec1.CusTempStorageLines.AddNew();
			temporaryStorageDec1.STH_OwnerReferenceNumber = "123";

			var temporaryStorageDec2 = JobHeader.CHGTSTCusTempStorageDecs.AddNew();
			temporaryStorageDec2.CusTempStorageLines.AddNew();
			temporaryStorageDec2.STH_OwnerReferenceNumber = "235";

			var parent = new MessageSendingActionParent(JobHeader, JobHeader.CHGTSTCusTempStorageDecs.Cast<CusTempStorageDec>(), x => ((CusTempStorageDec)x).STH_OwnerReferenceNumber, Env.Security.CustomsTemporaryStorageSendWithMessageErrors);
			using (var form = new MessageSendingForm<MessageSendingActionParent>(parent, "Change Custody Information"))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Precondition", 2, parent.SendingObjectsCollection.Count);

				var grid = (ZGrid)form.Controls.Find("MessageSendingObjectsGrid", true)[0];
				grid.Focus();
				grid.ListManager.Position = 0;
				Application.DoEvents();
				parent.SendingObjectsCollection[0].ShouldSend = true;

				grid.ListManager.Position = 1;
				Application.DoEvents();
				AssertNoExceptionThrown(@"RunPreSaveValidation results in Getter of BizObjMessageErrors to be invoked which results in RunPreSaveValidation again. Here are the stacks

 	Enterprise.Customs.Business.dll!Enterprise.Customs.Business.BaseMessageSendingObjectParent.GetMessageErrorsOThisObject() Line 125	C#
 	Enterprise.Customs.Business.dll!Enterprise.Customs.Business.BaseMessageSendingObjectParent.GetBizObjValidationMessageErrors() Line 92	C#
 	Enterprise.Customs.Business.dll!Enterprise.Customs.Business.BaseMessageSendingObjectParent.BizObjValidationMessageErrors.get() Line 81	C#
 	[External Code]	
 	CargoWise.ComponentModel.dll!CargoWise.ComponentModel.KReflectPropertyDescriptor.GetValue(object component) Line 33	C#
 	CargoWise.EntityFramework.dll!CargoWise.EntityFramework.BusinessObjectPropertyDescriptor.GetValueCore(object component) Line 20	C#
 	CargoWise.ComponentModel.dll!CargoWise.ComponentModel.KPropertyDescriptor.GetValue(object component) Line 122	C#
 	[External Code]	
 	CargoWise.EntityFramework.dll!CargoWise.EntityFramework.BusinessObject.OnListChanged(System.ComponentModel.ListChangedEventArgs e) Line 57	C#
 	CargoWise.EntityFramework.dll!CargoWise.EntityFramework.BusinessObject.OnElementChanged() Line 38	C#
 	CargoWise.EntityFramework.dll!CargoWise.EntityFramework.BusinessObject.OnNotificationsChanged(bool raiseElementChanged) Line 4649	C#
 	CargoWise.EntityFramework.dll!CargoWise.EntityFramework.BusinessObject.RunPreSaveValidationInternal(bool validateChildren) Line 3454	C#
 	CargoWise.EntityFramework.dll!CargoWise.EntityFramework.BusinessObject.RunPreSaveValidation() Line 3391	C#
 	Enterprise.Customs.Business.dll!Enterprise.Customs.Business.BaseMessageSendingObjectParent.GetMessageErrorsOThisObject() Line 125	C#
 	Enterprise.Customs.Business.dll!Enterprise.Customs.Business.BaseMessageSendingObjectParent.GetBizObjValidationMessageErrors() Line 92	C#
 	Enterprise.Customs.Business.dll!Enterprise.Customs.Business.BaseMessageSendingObjectParent.BizObjValidationMessageErrors.get() Line 81	C#
 	[External Code]	
 	CargoWise.ComponentModel.dll!CargoWise.ComponentModel.KReflectPropertyDescriptor.GetValue(object component) Line 33	C#
 	CargoWise.EntityFramework.dll!CargoWise.EntityFramework.BusinessObjectPropertyDescriptor.GetValueCore(object component) Line 20	C#
>	CargoWise.ComponentModel.dll!CargoWise.ComponentModel.KPropertyDescriptor.GetValue(object component) Line 122	C#
 	[External Code]	
 	CargoWise.EntityFramework.dll!CargoWise.EntityFramework.BusinessObject.OnListChanged(System.ComponentModel.ListChangedEventArgs e) Line 57	C#
 	CargoWise.EntityFramework.dll!CargoWise.EntityFramework.BusinessObject.OnElementChanged() Line 38	C#
 	CargoWise.EntityFramework.dll!CargoWise.EntityFramework.BusinessObject.OnNotificationsChanged(bool raiseElementChanged) Line 4649	C#
 	CargoWise.EntityFramework.dll!CargoWise.EntityFramework.BusinessObject.RunPreSaveValidationInternal(bool validateChildren) Line 3454	C#
 	CargoWise.EntityFramework.dll!CargoWise.EntityFramework.BusinessObject.RunPreSaveValidation() Line 3391	C#
 	Enterprise.Customs.Business.dll!Enterprise.Customs.Business.BaseMessageSendingObjectParent.GetMessageErrorsOThisObject() Line 125	C#
 	Enterprise.Customs.Business.dll!Enterprise.Customs.Business.BaseMessageSendingObjectParent.GetBizObjValidationMessageErrors() Line 92	C#
 	Enterprise.Customs.Business.dll!Enterprise.Customs.Business.BaseMessageSendingObjectParent.BizObjValidationMessageErrors.get() Line 81	C#

", () => parent.SendingObjectsCollection[1].ShouldSend = true);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var dec = JobHeader.CHGTSTCusTempStorageDecs.AddNew();
			dec.CusTempStorageLines.AddNew();

			var parent = new MessageSendingActionParent(JobHeader, JobHeader.CHGTSTCusTempStorageDecs.Cast<CusTempStorageDec>(), x => ((CusTempStorageDec)x).STH_OwnerReferenceNumber, Env.Security.CustomsTemporaryStorageSendWithMessageErrors);
			return new MessageSendingForm<MessageSendingActionParent>(parent, "Change Custody Information");
		}

		CusTempStorageJobHeader jobHeader;
		CusTempStorageJobHeader JobHeader => jobHeader ?? (jobHeader = Factory.New<CusTempStorageJobHeader>());
	}
}
