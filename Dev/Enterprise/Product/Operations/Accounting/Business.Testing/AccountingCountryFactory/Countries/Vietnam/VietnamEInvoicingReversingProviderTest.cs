using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Vietnam.Testing
{
	public class VietnamEInvoicingReversingProviderTest : TestCaseWithFactory
	{
		[TestDate(2025, 5, 31)]
		public void TestGetPreventInvoiceReversingFlag()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				AssertCanReverseInvoice(invoice);

				var eInvoicingStates = new List<string>()
				{
					EInvoicingPivotState.Queued,
					EInvoicingPivotState.Batched,
					EInvoicingPivotState.BatchedWithError,
					EInvoicingPivotState.Sent,
					EInvoicingPivotState.Delivered,
					EInvoicingPivotState.Succeed,
					EInvoicingPivotState.Failed,
					EInvoicingPivotState.Discarded,
					EInvoicingPivotState.Pending,
					EInvoicingPivotState.AwaitingReview,
					EInvoicingPivotState.InProcessing,
				};

				foreach (var state in eInvoicingStates)
				{
					TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, status: state);
					Factory.Save();
					AssertCanReverseInvoice(invoice);
				}

				TestDateAttribute.AddDays(1);
				foreach (var state in eInvoicingStates)
				{
					TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, status: state);
					Factory.Save();
					AssertCanReverseInvoice(invoice, canReverseForSuccess: false);
				}
			}
		}

		void AssertCanReverseInvoice(InvoicingBase invoice, bool canReverseForSuccess = true)
		{
			var isEmptyState = invoice.EInvoicingStatus.IsEmpty;
			var expectedStatusForReverse = (canReverseForSuccess && invoice.EInvoicingStatus == EInvoicingPivotState.Succeed) || invoice.EInvoicingStatus == EInvoicingPivotState.Discarded;

			AssertEquals("eInvoicing is disabled, test type:" + invoice.EInvoicingStatus, isEmptyState, InvoicingReversingProviderInstance.CanReverseInvoice(invoice));
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("eInvoicing is enabled, test type:" + invoice.EInvoicingStatus, isEmptyState || expectedStatusForReverse, InvoicingReversingProviderInstance.CanReverseInvoice(invoice));
			}
		}

		[TestDate(2025, 6, 30)]
		public void TestGetPreventInvoiceReversingPrompt_WhenFeatureControlHasValue()
		{
			var mockIFeatureControlManager = CreateMockIFeatureControlManager();

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			{
				AssertEquals("Invoices can only be reversed when e-Reporting status is 'SUC' or 'DCD'", InvoicingReversingProviderInstance.GetPreventInvoiceReversingPrompt());

				TestDateAttribute.AddDays(1);
				AssertEquals("Invoices can only be reversed when e-Reporting status is either 'DCD' or blank. \r\nIn compliance with Vietnam tax regulations, reversal is not allowed. If needed, please issue a Credit Note to amend the original transaction through the Job Billing module.", InvoicingReversingProviderInstance.GetPreventInvoiceReversingPrompt());
			}
		}

		[TestDate(2025, 5, 31)]
		public void TestGetPreventInvoiceReversingPrompt_WhenFeatureControlIsNull()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			{
				AssertEquals("Invoices can only be reversed when e-Reporting status is 'SUC' or 'DCD'", InvoicingReversingProviderInstance.GetPreventInvoiceReversingPrompt());

				TestDateAttribute.AddDays(1);
				AssertEquals("Invoices can only be reversed when e-Reporting status is either 'DCD' or blank. \r\nIn compliance with Vietnam tax regulations, reversal is not allowed. If needed, please issue a Credit Note to amend the original transaction through the Job Billing module.", InvoicingReversingProviderInstance.GetPreventInvoiceReversingPrompt());
			}
		}

		Mock<IFeatureControlManager> CreateMockIFeatureControlManager()
		{
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			var vietnamEInvoicingConfig = """
										{
											"VN": 
											{
												"CountrySpecific": 
												{
													"DisableCancellationDateTime": "01-07-2025"
												}
											}
										}
										""";
			var mockIFeatureData = new Mock<IFeatureData>();
			mockIFeatureData.Setup(x => x.Parameter).Returns(vietnamEInvoicingConfig);
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.AccountingEInvoicingConfiguration, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));
			return mockIFeatureControlManager;
		}

		IEInvoicingReversingProvider InvoicingReversingProviderInstance;

		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			InvoicingReversingProviderInstance = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.VietNam) as IInstanceProvider<IEInvoicingReversingProvider>).Get();
		}

		#endregion
	}
}
