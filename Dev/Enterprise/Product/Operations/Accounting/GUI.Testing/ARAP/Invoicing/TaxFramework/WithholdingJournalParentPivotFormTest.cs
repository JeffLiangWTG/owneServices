using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.TaxFramework.Testing
{
	[TestedType(typeof(WithholdingJournalParentPivotForm))]
	class WithholdingJournalParentPivotFormTest : ZFormBasherTest
	{
		public void TestWithholdingJournalsGrid()
		{
			var withholdingJournalProcessorForModuleMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			using (var form = new WithholdingJournalParentPivotForm(new[] { new WithholdingJournalsPerInvoice(Factory.New<APInvoice>(), Array.Empty<WithholdingJournalForDisplay>()) }, withholdingJournalProcessorForModuleMock.Object))
			{
				form.Show();

				var grid = form.GetControl<ZGrid>("JournalsGrid", true);

				var expectedListOfColumns = new[]
				{
					"TransactionCategory (ZTextBoxColumnStyleInfo) IsVisible:True",
					"Organisation (ZTextBoxColumnStyleInfo) IsVisible:True",
					"PostDate (ZDateEditColumnStyleInfo) IsVisible:True",
					"InvoiceDate (ZDateEditColumnStyleInfo) IsVisible:True",
					"Description (ZTextBoxColumnStyleInfo) IsVisible:True",
					"Amount (ZCalcEditColumnStyleInfo) IsVisible:True",
					"ExchangeRate (ZCalcEditColumnStyleInfo) IsVisible:True",
					"LocalAmount (ZCalcEditColumnStyleInfo) IsVisible:True",
					"Currency (ZTextBoxColumnStyleInfo) IsVisible:True",
					"DebitCreditSign (ZTextBoxColumnStyleInfo) IsVisible:True",
					"GLAccount (ZTextBoxColumnStyleInfo) IsVisible:True",
					"Branch (ZTextBoxColumnStyleInfo) IsVisible:True",
					"Department (ZTextBoxColumnStyleInfo) IsVisible:True",
				};
				var realListOfColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{!x.IsReadOnly}").ToArray();
				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);

				AssertDateEditColumnStyle(grid.GetColumnStyle("PostDate"), ZDateTimePickerFormat.Short);
				AssertDateEditColumnStyle(grid.GetColumnStyle("InvoiceDate"), ZDateTimePickerFormat.Short);
			}

			void AssertDateEditColumnStyle(ZGridColumnInfo columnStyle, ZDateTimePickerFormat dateFormat)
			{
				AssertType<ZDateEditColumnStyleInfo>(columnStyle);
				var dateColumnStyle = (ZDateEditColumnStyleInfo)columnStyle;
				AssertEquals(columnStyle.ColumnName, dateFormat, dateColumnStyle.DateTimeFormat);
			}
		}

		public void TestPostDateAndDescriptionEditable()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();

			var apJournal = Factory.NewWithValidTestData<APJournal>();

			var journalForDisplay = new WithholdingJournalForDisplay(apJournal, ZDate.Empty);

			var withholdingJournalsPerInvoice = new[]
			{
				new WithholdingJournalsPerInvoice(apInvoice, new[] { journalForDisplay })
			};

			var withholdingJournalProcessorForModuleMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			using (var form = new WithholdingJournalParentPivotForm(withholdingJournalsPerInvoice, withholdingJournalProcessorForModuleMock.Object))
			{
				form.Show();

				AssertEquals("DisplayMode", ODisplayMode.Edit, form.DisplayMode);
				Assert("AllowNew", !((IPostingButtonsProvider)form).AllowNew);

				var grid = form.GetControl<ZGrid>("JournalsGrid", true);
				AssertEquals(1, grid.List.Count);

				var bizObj = grid.List[0] as WithholdingJournalForDisplay;
				Assert("Post Date is not readonly", !bizObj.PostDateInfo.ReadOnly);
				Assert("Description is not readonly", !bizObj.DescriptionInfo.ReadOnly);

				withholdingJournalProcessorForModuleMock.Setup(x => x.Realize(It.IsAny<IReadOnlyCollection<WithholdingJournalsPerInvoice>>())).Returns(string.Empty);

				var formBusinessEntity = (WithholdingJournalForDisplayCollection)form.BusinessEntity;
				formBusinessEntity.RunPreSaveValidation();
				Assert("Precondition: HasErrors", formBusinessEntity.HasErrors());

				form.FireSaveButton();
				var expectedMessage = "There are errors - can't save.";
				AssertEquals("Error since accounting period is not setup", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				new AccountingPeriodTestHelper(new BusinessObjectFactory()).SetupPeriods();
				formBusinessEntity.RunPreSaveValidation();
				Assert("Precondition: HasErrors", !formBusinessEntity.HasErrors());

				form.FireSaveButton();
				Assert("Post Date is now readonly", bizObj.PostDateInfo.ReadOnly);
				Assert("Description is now readonly", bizObj.DescriptionInfo.ReadOnly);
				Assert("Journal is saved", apJournal.IsInDatabase);
			}
		}

		public void TestConstructor_Sets_JournalCollectionPerInvoice()
		{
			var withholdingJournalsPerInvoice = Array.Empty<WithholdingJournalsPerInvoice>();
			using (var form = new WithholdingJournalParentPivotForm(withholdingJournalsPerInvoice, new Mock<IWithholdingJournalRealizerForMultipleInvoices>().Object))
			{
				AssertEquals(withholdingJournalsPerInvoice, form.JournalCollectionPerInvoice);
			}
		}

		public void TestConstructor_Sets_WithholdingJournalRealizerForMultipleInvoices()
		{
			var realizer = new Mock<IWithholdingJournalRealizerForMultipleInvoices>().Object;
			using (var form = new WithholdingJournalParentPivotForm(Array.Empty<WithholdingJournalsPerInvoice>(), realizer))
			{
				AssertEquals(realizer, form.WithholdingJournalRealizerForMultipleInvoices);
			}
		}

		public void TestConstructor_ThrowsExceptionIfJournalCollectionPerInvoiceIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WithholdingJournalParentPivotForm(null, new Mock<IWithholdingJournalRealizerForMultipleInvoices>().Object));
		}

		public void TestConstructor_ThrowsExceptionIfRealizeWithholdingJournalsForMultipleInvoicesIsNull()
		{
			var apInvoice = Factory.New<APInvoice>();
			var apJournal = Factory.New<APJournal>();

			var withholdingJournalsPerInvoice = new[]
			{
				new WithholdingJournalsPerInvoice(apInvoice, new[] { new WithholdingJournalForDisplay(apJournal, ZDate.Empty) })
			};

			AssertExceptionThrown<ArgumentNullException>(() => new WithholdingJournalParentPivotForm(withholdingJournalsPerInvoice, null));
		}

		public void TestRealizeButtonClick_RealizationFailed()
		{
			var withholdingJournalProcessorForModuleMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var apJournal = Factory.NewWithValidTestData<APJournal>();

			var withholdingJournalsPerInvoice = new[]
			{
				new WithholdingJournalsPerInvoice(apInvoice, new[] { new WithholdingJournalForDisplay(apJournal, ZDate.Empty) })
			};

			using (var form = new WithholdingJournalParentPivotForm(withholdingJournalsPerInvoice, withholdingJournalProcessorForModuleMock.Object))
			{
				AssertSuccessfulSavePrecondition(form);

				var expectedErrorMessage = "Some error";
				withholdingJournalProcessorForModuleMock.Setup(x => x.Realize(It.IsAny<IReadOnlyCollection<WithholdingJournalsPerInvoice>>())).Returns(expectedErrorMessage);
				form.FireSaveButton();
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestRealize_PassesSameInstanceToRealizeMethod_AsProvidedInConstructor()
		{
			var withholdingJournalProcessorForModuleMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var apJournal = Factory.NewWithValidTestData<APJournal>();

			var withholdingJournalsPerInvoice = new[]
			{
				new WithholdingJournalsPerInvoice(apInvoice, new[] { new WithholdingJournalForDisplay(apJournal, ZDate.Empty) })
			};
			withholdingJournalProcessorForModuleMock.Setup(x => x.Realize(It.IsAny<IReadOnlyCollection<WithholdingJournalsPerInvoice>>())).Returns(string.Empty);

			using (var form = new WithholdingJournalParentPivotForm(withholdingJournalsPerInvoice, withholdingJournalProcessorForModuleMock.Object))
			{
				AssertSuccessfulSavePrecondition(form);

				form.FireSaveButton();
			}

			withholdingJournalProcessorForModuleMock.Verify(x => x.Realize(withholdingJournalsPerInvoice), Times.Once, "Realize should be called");
		}

		public void TestRealize_ShouldStopSaving_WhenDependencyReturnsErrorMessageForEmptyInput()
		{
			var withholdingJournalProcessorForModuleMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var apJournal = Factory.NewWithValidTestData<APJournal>();
			var withholdingJournalsPerInvoice = Array.Empty<WithholdingJournalsPerInvoice>();
			withholdingJournalProcessorForModuleMock.Setup(x => x.Realize(It.IsAny<IReadOnlyCollection<WithholdingJournalsPerInvoice>>())).Returns("some error");

			using (var form = new WithholdingJournalParentPivotForm(withholdingJournalsPerInvoice, withholdingJournalProcessorForModuleMock.Object))
			{
				AssertSuccessfulSavePrecondition(form);

				form.FireSaveButton();
			}

			Assert("journal IsInDatabase", !apJournal.IsInDatabase);
		}

		public void TestRealize_ShouldNotShowError_WhenDependencyReturnsSuccessForSingleJournal()
		{
			var withholdingJournalProcessorForModuleMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var apJournal = Factory.NewWithValidTestData<APJournal>();

			var withholdingJournalsPerInvoice = new[]
			{
				new WithholdingJournalsPerInvoice(apInvoice, new[] { new WithholdingJournalForDisplay(apJournal, ZDate.Empty) })
			};
			withholdingJournalProcessorForModuleMock.Setup(x => x.Realize(It.IsAny<IReadOnlyCollection<WithholdingJournalsPerInvoice>>())).Returns(string.Empty);

			using (var form = new WithholdingJournalParentPivotForm(withholdingJournalsPerInvoice, withholdingJournalProcessorForModuleMock.Object))
			{
				AssertSuccessfulSavePrecondition(form);

				form.FireSaveButton();
			}

			AssertNull("No error", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestRealize_ShouldDoSaving_WhenDependencyReturnsSuccessForSingleJournal()
		{
			var withholdingJournalProcessorForModuleMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var apJournal = Factory.NewWithValidTestData<APJournal>();

			var withholdingJournalsPerInvoice = new[]
			{
				new WithholdingJournalsPerInvoice(apInvoice, new[] { new WithholdingJournalForDisplay(apJournal, ZDate.Empty) })
			};
			withholdingJournalProcessorForModuleMock.Setup(x => x.Realize(It.IsAny<IReadOnlyCollection<WithholdingJournalsPerInvoice>>())).Returns(string.Empty);

			using (var form = new WithholdingJournalParentPivotForm(withholdingJournalsPerInvoice, withholdingJournalProcessorForModuleMock.Object))
			{
				AssertSuccessfulSavePrecondition(form);

				form.FireSaveButton();
			}
			Assert("journal IsInDatabase", apJournal.IsInDatabase);
		}

		public void TestBusinessEntity_CollectionContainsAllPassedJournalsFromAllInvoices()
		{
			var withholdingJournalProcessorForModuleMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			var apInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			var apInvoice2 = Factory.NewWithValidTestData<APInvoice>();

			var apJournal1_1 = new WithholdingJournalForDisplay(Factory.NewWithValidTestData<APJournal>(), ZDate.Empty);
			var apJournal1_2 = new WithholdingJournalForDisplay(Factory.NewWithValidTestData<APJournal>(), ZDate.Empty);
			var apJournal2 = new WithholdingJournalForDisplay(Factory.NewWithValidTestData<APJournal>(), ZDate.Empty);

			var withholdingJournalsPerInvoice = new[]
			{
				new WithholdingJournalsPerInvoice(apInvoice1, new[] { apJournal1_1, apJournal1_2 }),
				new WithholdingJournalsPerInvoice(apInvoice2, new[] { apJournal2 })
			};
			withholdingJournalProcessorForModuleMock.Setup(x => x.Realize(It.IsAny<IReadOnlyCollection<WithholdingJournalsPerInvoice>>())).Returns(string.Empty);

			using (var form = new WithholdingJournalParentPivotForm(withholdingJournalsPerInvoice, withholdingJournalProcessorForModuleMock.Object))
			{
				var formBusinessEntity = (WithholdingJournalForDisplayCollection)form.BusinessEntity;
				AssertContainsExactElementsInAnyOrder(new[] { apJournal1_1, apJournal1_2, apJournal2 }, formBusinessEntity);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var withholdingJournalProcessorForModuleMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			var apInvoice = Factory.New<APInvoice>();
			var apJournal = Factory.New<APJournal>();

			var withholdingJournalsPerInvoice = new[]
			{
				new WithholdingJournalsPerInvoice(apInvoice, new[] { new WithholdingJournalForDisplay(apJournal, ZDate.Empty) })
			};

			return new WithholdingJournalParentPivotForm(withholdingJournalsPerInvoice, withholdingJournalProcessorForModuleMock.Object);
		}

		static void AssertSuccessfulSavePrecondition(WithholdingJournalParentPivotForm form)
		{
			new AccountingPeriodTestHelper(new BusinessObjectFactory()).SetupPeriods();
			var formBusinessEntity = (WithholdingJournalForDisplayCollection)form.BusinessEntity;
			formBusinessEntity.RunPreSaveValidation();
			Assert("Precondition: HasErrors", !formBusinessEntity.HasErrors());
		}
	}
}
