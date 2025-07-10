using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using Newtonsoft.Json;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class APInvoicingBaseFromDraftInvoiceControllerTest<T> : ZControllerBasherTest
		where T : InvoicingBase
	{
		public void TestControllerType()
		{
			Assert("Controller should inherit from APInvoicingBaseFromDraftInvoiceController", Controller is APInvoicingBaseFromDraftInvoiceController<T>);
		}

		public override void TestNewForm()
		{
			var exp = AssertExceptionThrown<InvalidOperationException>(() => Controller.ShowNewForm());
			AssertEquals($"{ControllerToBashType.Name} is not designed to have new form.", exp.Message);
		}

		public override void TestViewForm()
		{
			var exp = AssertExceptionThrown<InvalidOperationException>(() => Controller.ShowViewForm(GetBusinessObjectThatIsInTheDatabase()));
			AssertEquals($"{ControllerToBashType.Name} is not designed to have view form.", exp.Message);
		}

		public override void TestDeleteForm()
		{
			var exp = AssertExceptionThrown<InvalidOperationException>(() => Controller.ShowDeleteForm(GetBusinessObjectThatIsInTheDatabase()));
			AssertEquals($"{ControllerToBashType.Name} is not designed to have delete form.", exp.Message);
		}

		public void TestSecurityRight()
		{
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			var dummyNewSecurity = new SecurityCheckpoint("TS1", (NoResString)"Test Security", null, security.SecurityInstance);
			var mockSecuritySetting = new Mock<IInvoiceSecurityChecker>(MockBehavior.Strict);
			mockSecuritySetting
				.Setup(x => x.GetBasicSecuritySettings(LedgerTypes.AccountsPayable, TransactionType))
				.Returns(new BasicSecuritySettings
				{
					New = dummyNewSecurity,
				});

			using (ObjectFactory.Substitute(mockSecuritySetting.Object))
			{
				var controller = ZControllerFactory.Create(GetControllerID());
				AssertExceptionThrown<InvalidOperationException>("CheckPointForView", () => _ = Controller.CheckPointForViewExposedForTest);
				AssertExceptionThrown<InvalidOperationException>("CheckPointForNew", () => _ = Controller.CheckPointForNewExposedForTest);
				AssertExceptionThrown<InvalidOperationException>("CheckPointForDelete", () => _ = Controller.CheckPointForDeleteExposedForTest);
				AssertEquals("CheckPointForCopy", null, Controller.CheckPointForCopyExposedForTest);
				AssertEquals("CheckPointForEdit", dummyNewSecurity, Controller.CheckPointForEditExposedForTest);
			}
		}

		public void TestEditForm_NoSecurity()
		{
			Controller.CheckPointForEditExposedForTest.IsAllowed = false;
			var bizO = GetBusinessObjectThatIsInTheDatabase();

			var result = Controller.ShowEditForm(bizO);

			AssertEquals($"Access Denied: {Controller.CheckPointForEditExposedForTest.DisplayText}"
				, UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals(@$"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{Controller.CheckPointForEditExposedForTest.DisplayTextPathToSecurityRight}"
				, UnitTestUserNotification.Instance.LastMessage.Text);

			AssertNull(result);
			AssertNull(Controller.LastShownForm);
		}

		public void TestEditForm_WhenHavingAdditionalDto()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase() as AccDraftInvoiceHeader;
			var dummyInvoice = Factory.New<T>();

			var additionalDto = JsonConvert.SerializeObject(new PosterConfigurationDTO
			{
				SelectedAccruals = null
			});

			var mockIInvoiceConverter = MockIInvoiceConverter(draftInvoice
				, additionalDto
				, dummyInvoice
				, default
				, default);

			using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
			using (Controller.SetArgsForNewForm(new[] { additionalDto }))
			using (var form = (ZForm)Controller.ShowEditForm(draftInvoice))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(dummyInvoice, form.DataSource);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			mockIInvoiceConverter.Verify(
				x => x.PostFromDraftInvoice<T>(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<PosterConfigurationDTO>(), out It.Ref<(string, string)>.IsAny, out It.Ref<(string, string)>.IsAny)
				, Times.Exactly(1)
			);
		}

		public void TestEditForm_InvoiceConvertingSuccess()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase() as AccDraftInvoiceHeader;
			var dummyInvoice = Factory.New<T>();
			var mockIInvoiceConverter = MockIInvoiceConverter(draftInvoice
				, dtoJson: null
				, dummyInvoice
				, default
				, default);

			using(ObjectFactory.Substitute(mockIInvoiceConverter.Object))
			using (var form = (ZForm)Controller.ShowEditForm(draftInvoice))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(dummyInvoice, form.DataSource);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			mockIInvoiceConverter.Verify(
				x => x.PostFromDraftInvoice<T>(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<PosterConfigurationDTO>(), out It.Ref<(string, string)>.IsAny, out It.Ref<(string, string)>.IsAny)
				, Times.Exactly(1)
			);
		}

		public void TestEditForm_InvoiceConvertingSuccess_ReconciliationFail()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase() as AccDraftInvoiceHeader;
			var dummyInvoice = Factory.New<T>();
			var mockIInvoiceConverter = MockIInvoiceConverter(draftInvoice
				, dtoJson: null
				, dummyInvoice
				, default
				, new("DummyReconciliationErrorMessage", "DummyReconciliationErrorCaption"));

			using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
			using (var form = (ZForm)Controller.ShowEditForm(draftInvoice))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(dummyInvoice, form.DataSource);
				AssertEquals("DummyReconciliationErrorMessage", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("DummyReconciliationErrorCaption", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			mockIInvoiceConverter.Verify(
				x => x.PostFromDraftInvoice<T>(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<PosterConfigurationDTO>(), out It.Ref<(string, string)>.IsAny, out It.Ref<(string, string)>.IsAny)
				, Times.Exactly(1)
			);
		}

		public void TestEditForm_InvoiceConvertingFail()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase() as AccDraftInvoiceHeader;
			var mockIInvoiceConverter = MockIInvoiceConverter(draftInvoice
				, dtoJson: null
				, Factory.New<T>()
				, new("DummyValidationErrorMessage", "DummyValidationErrorCaption")
				, new("DummyMessage that will not be used.", "DummyCaption that will not be used."));

			using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
			{
				var result = Controller.ShowEditForm(draftInvoice);
				AssertEquals("DummyValidationErrorMessage", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("DummyValidationErrorCaption", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertNull(result);
				AssertNull(Controller.LastShownForm);
			}

			mockIInvoiceConverter.Verify(
				x => x.PostFromDraftInvoice<T>(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<PosterConfigurationDTO>(), out It.Ref<(string, string)>.IsAny, out It.Ref<(string, string)>.IsAny)
				, Times.Exactly(1)
			);
		}

		public void TestEditForm_InvoiceConvertingFail_NullInvoiceResult()
		{
			var draftInvoice = GetBusinessObjectThatIsInTheDatabase() as AccDraftInvoiceHeader;
			var mockIInvoiceConverter = MockIInvoiceConverter(draftInvoice
				, dtoJson: null
				, outputInvoice: null
				, default
				, default);

			using (ObjectFactory.Substitute(mockIInvoiceConverter.Object))
			{
				var result = Controller.ShowEditForm(draftInvoice);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertNull(result);
				AssertNull(Controller.LastShownForm);
			}

			mockIInvoiceConverter.Verify(
				x => x.PostFromDraftInvoice<T>(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<PosterConfigurationDTO>(), out It.Ref<(string, string)>.IsAny, out It.Ref<(string, string)>.IsAny)
				, Times.Exactly(1)
			);
		}

		Mock<IAPReconciliationPoster> MockIInvoiceConverter(AccDraftInvoiceHeader passingInDraftInvoiceHeader
			, string dtoJson
			, T outputInvoice
			, (string, string) outputValidationError
			, (string, string) outputReconciliationError)
		{
			var mockIInvoiceConverter = new Mock<IAPReconciliationPoster>(MockBehavior.Strict);
			var dto = null as PosterConfigurationDTO;
			try
			{
				dto = JsonConvert.DeserializeObject<PosterConfigurationDTO>(dtoJson);
			}
			catch { }

			mockIInvoiceConverter
				.Setup(x => x.PostFromDraftInvoice<T>(
					It.Is<AccDraftInvoiceHeader>(x => x.PK == passingInDraftInvoiceHeader.PK)
					, It.Is<PosterConfigurationDTO>(x => (x == null && dto == null) || (JsonConvert.SerializeObject(x) == dtoJson))
					, out outputValidationError, out outputReconciliationError))
				.Returns(outputInvoice);
			return mockIInvoiceConverter;
		}

		public void TestEditForm_IncorrectTransactionType()
		{
			var bizO = GetBusinessObjectThatIsInTheDatabase() as AccDraftInvoiceHeader;
			bizO.AIH_TransactionType = TransactionTypes.AdjustmentNote;
			Factory.Save();

			var exp = AssertExceptionThrown<InvalidOperationException>(() => Controller.ShowEditForm(bizO));
			AssertEquals($@"Incorrect AccDraftInvoiceHeader Type is allocated.
Company:EDI - Eagle Datamation International
Transaction Type: ADJ
InternalReference:00001001", exp.Message);
		}

		public void TestEditForm_DraftInvoiceNotExist()
		{
			var newFactory = new BusinessObjectFactory();
			assertException("Unsaved Draft Invoice Case1.", newFactory.NewWithValidTestData<AccDraftInvoiceHeader>());
			assertException("Unsaved Draft Invoice Case2.", newFactory.NewWithValidTestData<AccDraftInvoiceHeader>());
			assertException("Incorrect BusinessObject Case.", Factory.NewWithValidTestData<AccTransactionHeader>());

			void assertException(string comment, BusinessObject businessObject)
			{
				var expectedUsefulMessage = $@"Unable to open '{EditFormCaption}' form for Draft Invoice.
Please ensure that CargoWise is open in the same environment as the Invoice Processing Portal, then try to post the Draft Invoice again.
If the issue persists, please raise an eRequest.";

				CombineAssertions(comment, () =>
				{
					AssertNull(null, Controller.ShowEditForm(businessObject));
					AssertSequencesEqual("Popup Message for ShowEditForm"
						, new[] {
							"The selected record has been deleted by another user. It cannot be displayed.",
							expectedUsefulMessage,
							null
						}
						, UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text).ToArray()
					);
					UnitTestUserNotification.Instance.ClearMessages();

					var exp = AssertExceptionThrown<Exception>(() => ZControllerFactory.GetCorrectControllerAndBusinessObject(GetControllerID(), businessObject.PK, false));
					AssertEquals("Exception Type", "EnterpriseUrlHandlerException", exp.GetType().Name);
					AssertEquals("Exception Messasge", $"The system has searched all open {Core.Constants.ProductName} programs and could not find the record", exp.Message);
					AssertSequencesEqual("Popup Message for GetCorrectControllerAndBusinessObject"
						, new[] {
							expectedUsefulMessage,
							null
						}
						, UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text).ToArray()
					);
					UnitTestUserNotification.Instance.ClearMessages();
				});
			}
		}

		public void TestEditForm_ControllerID()
		{
			var bizO = GetBusinessObjectThatIsInTheDatabase();
			var editForm = Controller.ShowEditForm(bizO);
			AssertEquals(ExpectedFormControllerID, editForm.ControllerID);
		}

		protected abstract bool ExpectedShouldShowOriginalInvoiceReferenceFields { get; }

		protected abstract string TransactionType { get; }

		protected abstract string EditFormCaption { get; }

		protected abstract ControllerID ExpectedFormControllerID { get; }

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var draftInvoice = Factory.New<AccDraftInvoiceHeader>();
			draftInvoice.AIH_TransactionType = TransactionType;
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;
			draftInvoice.AIH_GB_Branch = TestObjectCreator.NonCurrentBranch.PK;
			draftInvoice.AIH_GE_Department = TestObjectCreator.NonCurrentDepartment.PK;
			draftInvoice.AIH_InternalReference = "00001001";
			draftInvoice.AIH_OH_Creditor = TestObjectCreator.Creditor4.PK;
			draftInvoice.AIH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
			draftInvoice.AIH_Description = "AP Desc";

			Factory.Save();
			return draftInvoice;
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
