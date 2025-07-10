using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class OverrideMatchStatusValidationTest : OverrideInvoiceDetailValidationTest
	{
		public void TestCheckAH_MatchStatusReasonCode()
		{
			var statusCollection = new SystemDefinableCodeDescriptionBoolCollection();
			statusCollection.Add("AAA", (NoResString)"", true);
			var statusReasonCollection = new SystemDefinableCodeDescriptionBoolCollection();
			statusReasonCollection.Add("BBB", (NoResString)"", true);

			using (AccountingConfigurationRegistry.Instance.MatchStatus.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, statusCollection))
			using (AccountingConfigurationRegistry.Instance.MatchStatusReason.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, statusReasonCollection))
			{
				Header.SetContext(BusinessContext.OverrideMatchStatus);
				Header.AH_MatchStatus = ZString.Empty;
				Header.AH_MatchStatusReasonCode = ZString.Empty;
				Header.Validation.ValidateAH_MatchStatusReasonCode();
				AssertNoErrors(Header.AH_MatchStatusReasonCodeInfo);

				Header.AH_MatchStatus = "AAA";
				Header.Validation.ValidateAH_MatchStatusReasonCode();
				AssertHasError(Header.AH_MatchStatusReasonCodeInfo, "Please enter a Match Status Reason.");

				Header.AH_MatchStatusReasonCode = "CCC";
				Header.Validation.ValidateAH_MatchStatusReasonCode();
				AssertHasError(Header.AH_MatchStatusReasonCodeInfo, "Enter a valid Match Status Reason.");

				Header.AH_MatchStatus = ZString.Empty;
				Header.AH_MatchStatusReasonCode = "BBB";
				Header.Validation.ValidateAH_MatchStatusReasonCode();
				AssertHasError(Header.AH_MatchStatusReasonCodeInfo, "If a match status is not specified, a reason must not be specified.");

				Header.AH_MatchStatus = "AAA";
				Header.AH_MatchStatusReasonCode = "BBB";
				Header.Validation.ValidateAH_MatchStatusReasonCode();
				AssertNoErrors(Header.AH_MatchStatusReasonCodeInfo);
			}
		}

		public void TestCheckAH_MatchStatus()
		{
			var statusCollection = new SystemDefinableCodeDescriptionBoolCollection();
			statusCollection.Add("AAA", (NoResString)"", true);
			using (AccountingConfigurationRegistry.Instance.MatchStatus.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, statusCollection))
			{
				Header.SetContext(BusinessContext.OverrideMatchStatus);
				Header.AH_MatchStatus = ZString.Empty;
				AssertNoErrors(Header.AH_MatchStatusInfo);

				Header.AH_MatchStatus = "CCC";
				AssertHasError(Header.AH_MatchStatusInfo, "Enter a valid Match Status.");

				Header.AH_MatchStatus = "AAA";
				AssertNoErrors(Header.AH_MatchStatusInfo);
			}
		}

		protected override Type InvoiceType
		{
			get
			{
				return typeof(ARInvoice);
			}
		}

		protected override OverrideInvoiceDetailValidation GetValidation(TransactionHeader parent)
		{
			return parent.Validation as OverrideMatchStatusValidation;
		}

		protected override void SetBusinessContext(InvoicingBase invoice)
		{
			invoice.SetContext(BusinessContext.OverrideMatchStatus);
		}
	}
}
