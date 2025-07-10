using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax
{
	/// <summary>
	/// Interface to calculate US Sales tax on AR / AP transactions.
	/// </summary>
	public interface IUSSalesTaxCalculator : IDisposable
	{
		/// <summary>
		/// The human readable name of the US Sales Tax provider.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Returns true if the US Sales Tax functionality is enabled in this branch. False otherwise.
		/// </summary>
		bool IsEnabled(GlbBranch branch);

		/// <summary>
		/// Returns the configuration state for this branch: Off, Sandbox or Production.
		/// </summary>
		ConfigurationStatus GetConfiguration(GlbBranch branch);

		/// <summary>
		/// Returns the configuration charge code and PK for this branch.
		/// </summary>
		(ZGuid pk, ZString code) GetChargeCode(GlbBranch branch);

		/// <summary>
		/// Returns true if menu items relating to US Sales Tax should be shown on Invoice form to allow manual calculation or submission of sales tax.
		/// </summary>
		bool ShouldShowMenuItemsOnInvoiceForm(InvoicingBase transaction);

		/// <summary>
		/// Text to show on the calculate menu item. If null, a generic string is used.
		/// </summary>
		MultilingualString CalculateMenuItemText { get; }

		/// <summary>
		/// Text to show on the submit menu item. If null, a generic string is used.
		/// </summary>
		MultilingualString SubmitMenuItemText { get; }

		/// <summary>
		/// Returns the security check point applied to calculation menu item.
		/// </summary>
		SecurityCheckpoint CheckpointForCalculationMenuItem(InvoicingBase transaction);

		/// <summary>
		/// Returns the security check point applied to submit menu item.
		/// </summary>
		SecurityCheckpoint CheckpointForSubmitMenuItem(InvoicingBase transaction);

		/// <summary>
		/// Text which is shown in the calculation / submission menu item popup to hint where the user can find additional information.
		/// </summary>
		MultilingualString MenuItemTroubleshootingHint { get; }

		/// <summary>
		/// Calculates sales tax for the transaction. Result includes sales tax amount, and base amount used for calculation.
		/// </summary>
		/// <remarks>
		/// This may be called with a blank or invalid transaction and should not throw null reference exceptions. Validation exceptions may be returned.
		/// </remarks>
		(CalculationResult result, Exception error) CalculateSalesTax(InvoicingBase transaction);

		/// <summary>
		/// Submits sales tax for the transaction. Result includes sales tax amount, base amount used for calculation, and final status.
		/// </summary>
		/// <remarks>
		/// This may be called with a blank or invalid transaction and should not throw null reference exceptions. Validation exceptions may be returned.
		/// Errors may be returned if the submission fails, or if unexpected status is returned.
		/// </remarks>
		(CalculationResult result, Exception error) SubmitSalesTax(InvoicingBase transaction);

		/// <summary>
		/// Determines the current amount of sales tax applied to this transaction, or zero if no tax charge code is present.
		/// </summary>
		/// <remarks>
		/// If many sales tax lines are present, they should be summed.
		/// </remarks>
		decimal GetCurrentSalesTaxAmount(InvoicingBase transaction);

		/// <summary>
		/// Returns true if sales tax should be calculated and added on post, false otherwise.
		/// </summary>
		bool ShouldSetSalesTaxOnPost(InvoicingBase transaction);

		/// <summary>
		/// Adds or updates the sales tax line item to the amount specified.
		/// </summary>
		/// <remarks>
		/// This may add, update or delete line items such that GetCurrentSalesTaxAmount() == amount.
		/// May throw if ShouldSetSalesTaxOnPost() returns false.
		/// </remarks>
		void SetSalesTaxLineItem(InvoicingBase transaction, decimal amount);

		/// <summary>
		/// Returns details of transaction to include in Issue Manager reports on error during post.
		/// </summary>
		/// <remarks>
		/// The information included should be sufficient to identify the transaction.
		/// </remarks>
		string GetTransactionDetailsForIssueManager(InvoicingBase transaction);
	}
}
