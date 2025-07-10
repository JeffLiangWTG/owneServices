using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax.Implementation
{
	internal sealed class OverridableUSSalesTaxCalculator : IUSSalesTaxCalculator
	{
		public OverridableUSSalesTaxCalculator()
			: this(GetInnerInstance())
		{
		}

		internal OverridableUSSalesTaxCalculator(IUSSalesTaxCalculator inner)
		{
			Inner = Argument.NotNull(inner, nameof(inner));
		}

		static IUSSalesTaxCalculator GetInnerInstance()
		{
			var clientSpecificOverride = ObjectFactory.Get<ListObject>("IUSSalesTaxCalculator_ClientSpecific");
			var factory = clientSpecificOverride.Count != 1 ? NullUSSalesTaxCalculatorFactory.Instance
						: clientSpecificOverride[0] is IUSSalesTaxCalculatorFactory clientSpecificFactory ? clientSpecificFactory
						: NullUSSalesTaxCalculatorFactory.Instance;
			return factory.Get();
		}

		public IUSSalesTaxCalculator Inner { get; }

		#region IUSSalesTaxCalculator

		public string Name => Inner.Name;

		public bool IsEnabled(GlbBranch branch) => Inner.IsEnabled(branch);

		public ConfigurationStatus GetConfiguration(GlbBranch branch) => Inner.GetConfiguration(branch);

		public (ZGuid pk, ZString code) GetChargeCode(GlbBranch branch) => Inner.GetChargeCode(branch);

		public bool ShouldShowMenuItemsOnInvoiceForm(InvoicingBase transaction) => Inner.ShouldShowMenuItemsOnInvoiceForm(transaction);

		public MultilingualString CalculateMenuItemText => Inner.CalculateMenuItemText;

		public MultilingualString SubmitMenuItemText => Inner.SubmitMenuItemText;

		public SecurityCheckpoint CheckpointForCalculationMenuItem(InvoicingBase transaction) => Inner.CheckpointForCalculationMenuItem(transaction);

		public SecurityCheckpoint CheckpointForSubmitMenuItem(InvoicingBase transaction) => Inner.CheckpointForSubmitMenuItem(transaction);

		public MultilingualString MenuItemTroubleshootingHint => Inner.MenuItemTroubleshootingHint;

		public (CalculationResult result, Exception error) CalculateSalesTax(InvoicingBase transaction)
			=> Inner.CalculateSalesTax(transaction);

		public (CalculationResult result, Exception error) SubmitSalesTax(InvoicingBase transaction)
			=> Inner.SubmitSalesTax(transaction);

		public decimal GetCurrentSalesTaxAmount(InvoicingBase transaction) => Inner.GetCurrentSalesTaxAmount(transaction);

		public bool ShouldSetSalesTaxOnPost(InvoicingBase transaction) => Inner.ShouldSetSalesTaxOnPost(transaction);

		public void SetSalesTaxLineItem(InvoicingBase transaction, decimal amount) => Inner.SetSalesTaxLineItem(transaction, amount);

		public string GetTransactionDetailsForIssueManager(InvoicingBase transaction) => Inner.GetTransactionDetailsForIssueManager(transaction);

		#endregion

		#region IDisposable

		public void Dispose()
		{
			Inner.Dispose();
		}

		#endregion

		#region NullUSSalesTaxCalculator

		sealed class NullUSSalesTaxCalculator : IUSSalesTaxCalculator
		{
			public string Name => "Null";

			public bool IsEnabled(GlbBranch currentBranch) => false;

			public ConfigurationStatus GetConfiguration(GlbBranch currentBranch) => ConfigurationStatus.Off;

			public (ZGuid pk, ZString code) GetChargeCode(GlbBranch branch) => (ZGuid.Empty, ZString.Empty);

			public bool ShouldShowMenuItemsOnInvoiceForm(InvoicingBase transaction) => false;

			public MultilingualString CalculateMenuItemText => null;

			public MultilingualString SubmitMenuItemText => null;

			public SecurityCheckpoint CheckpointForCalculationMenuItem(InvoicingBase transaction) => new DeniedSecurityCheckpoint();

			public SecurityCheckpoint CheckpointForSubmitMenuItem(InvoicingBase transaction) => new DeniedSecurityCheckpoint();

			public MultilingualString MenuItemTroubleshootingHint => (NoResString)string.Empty;

			public (CalculationResult result, Exception error) CalculateSalesTax(InvoicingBase transaction)
				=> (CalculationResult.Zero, null);

			public (CalculationResult result, Exception error) SubmitSalesTax(InvoicingBase transaction)
				=> (CalculationResult.Zero, null);

			public decimal GetCurrentSalesTaxAmount(InvoicingBase transaction) => 0m;

			public bool ShouldSetSalesTaxOnPost(InvoicingBase transaction) => false;

			public void SetSalesTaxLineItem(InvoicingBase transaction, decimal amount) { }

			public string GetTransactionDetailsForIssueManager(InvoicingBase transaction) => string.Empty;

			public void Dispose() { }
		}

		[Immutable]
		sealed class NullUSSalesTaxCalculatorFactory : IUSSalesTaxCalculatorFactory
		{
			public IUSSalesTaxCalculator Get() => new NullUSSalesTaxCalculator();

			public static NullUSSalesTaxCalculatorFactory Instance { get; } = new NullUSSalesTaxCalculatorFactory();
		}

		#endregion
	}
}
