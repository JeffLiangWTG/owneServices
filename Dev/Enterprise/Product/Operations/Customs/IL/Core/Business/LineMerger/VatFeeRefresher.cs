using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public interface IVatRefresher
	{
		IDisposable SuspendSystemAddedVatFeeRecalculation();
		void Unhook();
	}

	public class VatFeeRefresher<TFee, TLine> : IVatRefresher
		where TFee : CusEntryLineFee
		where TLine : CusEntryLine
	{
		#region Construction

		public static IVatRefresher New(Customs.Business.ICusEntryLineFeeCollection<TFee, TLine> fees)
		{
			Argument.NotNull(fees, nameof(fees));
			var entryLine = Argument.NotNull(fees.Master, nameof(fees.Master));
			var declaration = entryLine.Declaration;
			var useUniversalFeeCalculation = declaration != null && DeclarationConfigurationProvider.UseUniversalFeeCalculation;
			IVatRefresher vatRefresher;
			if (useUniversalFeeCalculation)
			{
				vatRefresher = new VatFeeRefresher<TFee, TLine>(fees, entryLine, declaration);
				((VatFeeRefresher<TFee, TLine>)vatRefresher).Initialize();
			}
			else
			{
				vatRefresher = new DummyVatRefresher();
			}

			return vatRefresher;
		}

		class DummyVatRefresher : IVatRefresher
		{
			IDisposable IVatRefresher.SuspendSystemAddedVatFeeRecalculation() => DisposableAction.NoAction;
			void IVatRefresher.Unhook() { }
		}

		/// <summary>
		/// Redundant arguments entryLine and declaration used since this is a private constructor
		/// and they're already evaluated in the factory method above (New).
		/// Hence this constructor assumes they're consistent witn one antoher.
		/// </summary>
		/// <param name="fees">Entry Line Fee Collection</param>
		/// <param name="entryLine">Must be the same as fees.Master</param>
		/// <param name="declaration">Must be the same as entryLine.Declaration</param>
		VatFeeRefresher(Customs.Business.ICusEntryLineFeeCollection<TFee, TLine> fees, TLine entryLine, JobDeclaration declaration)
		{
			this.fees = fees;
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			calculator = entryLine.GetEntryLineVatCalculator();
		}

		#endregion

		readonly Customs.Business.ICusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> fees;
		readonly JobDeclaration declaration;
		readonly CusEntryLine entryLine;
		EntryLineVatCalculator calculator;

		void Initialize()
		{
			declaration.OnIsDeclarationIntegratedChanged += Declaration_OnIsDeclarationIntegratedChanged;
			HookOrUnHookBasedOnIsDeclarationIntegrated(shouldUpdateSystemAddedVatFee: false);
		}

		void IVatRefresher.Unhook()
		{
			declaration.OnIsDeclarationIntegratedChanged -= Declaration_OnIsDeclarationIntegratedChanged;
			fees.CountChanged -= Fees_CountChanged;
			UnhookAllEntryLineFees();
		}

		void Declaration_OnIsDeclarationIntegratedChanged(object sender, EventArgs e) => HookOrUnHookBasedOnIsDeclarationIntegrated();

		void HookOrUnHookBasedOnIsDeclarationIntegrated(bool shouldUpdateSystemAddedVatFee = true)
		{
			if (declaration.IsDeclarationIntegrated)
			{
				fees.CountChanged -= Fees_CountChanged;
				UnhookAllEntryLineFees();
			}
			else
			{
				HookUpAllEntryLineFees();
				fees.CountChanged += Fees_CountChanged;
				if (shouldUpdateSystemAddedVatFee)
				{
					UpdateSystemAddedVatFeeIfNotSuspended();
				}
			}
		}

		void Fees_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e != null)
			{
				if (e.ItemAdded)
				{
					HookUpEntryLineFee((CusEntryLineFee)e.BizObject);
				}
				else if (e.ItemRemoved)
				{
					UnhookEntryLineFee((CusEntryLineFee)e.BizObject);
				}

				if (!fees.IsLoading)
				{
					UpdateSystemAddedVatFeeIfAllowed((CusEntryLineFee)e.BizObject);
				}
			}
		}

		void HookUpEntryLineFee(CusEntryLineFee fee)
		{
			fee.CF_ChargeTypeInfo.ValueChanged += UpdateSystemAddedVatFeeIfAllowed;
			fee.CF_ChargeAmountInfo.ValueChanged += UpdateSystemAddedVatFeeIfAllowed;
			fee.CF_IsLandedCostOnlyInfo.ValueChanged += UpdateSystemAddedVatFeeIfAllowed;
		}

		void UnhookEntryLineFee(CusEntryLineFee fee)
		{
			fee.CF_ChargeTypeInfo.ValueChanged -= UpdateSystemAddedVatFeeIfAllowed;
			fee.CF_ChargeAmountInfo.ValueChanged -= UpdateSystemAddedVatFeeIfAllowed;
			fee.CF_IsLandedCostOnlyInfo.ValueChanged -= UpdateSystemAddedVatFeeIfAllowed;
		}

		void UpdateSystemAddedVatFeeIfAllowed(object sender, EventArgs e) => UpdateSystemAddedVatFeeIfAllowed((CusEntryLineFee)sender);

		void HookUpAllEntryLineFees() => fees.Cast<CusEntryLineFee>().ForEach(HookUpEntryLineFee);
		void UnhookAllEntryLineFees() => fees.Cast<CusEntryLineFee>().ForEach(UnhookEntryLineFee);

		void UpdateSystemAddedVatFeeIfAllowed(CusEntryLineFee sender)
		{
			if (sender != null && !sender.IsDeleted && !sender.IsSystemAddedVatFee)
			{
				UpdateSystemAddedVatFeeIfNotSuspended();
			}
		}

		void UpdateSystemAddedVatFeeIfNotSuspended()
		{
			if (!IsSystemAddedVatFeeCalculationSuspended)
			{
				using (SuspendSystemAddedVatFeeRecalculationCore())
				{
					calculator = entryLine.GetEntryLineVatCalculator();
					calculator.RefreshFees();
				}
			}
		}

		#region Suspend System Added VAT Fee Calculation

		public bool IsSystemAddedVatFeeCalculationSuspended => systemAddedVatFeeCalculationSuspenderIndex > 0;

		int systemAddedVatFeeCalculationSuspenderIndex;

		IDisposable IVatRefresher.SuspendSystemAddedVatFeeRecalculation() => SuspendSystemAddedVatFeeRecalculationCore();

		IDisposable SuspendSystemAddedVatFeeRecalculationCore()
		{
			systemAddedVatFeeCalculationSuspenderIndex++;
			return new DisposableAction(() => systemAddedVatFeeCalculationSuspenderIndex--);
		}

		#endregion
	}
}
