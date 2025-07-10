using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class InvoiceCharge : EU.Business.Declaration.InvoiceCharge, Integration.Customs.FR.IInvoiceCharge
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : EU.Business.Declaration.InvoiceCharge.Schema
		{
			public const string IsSystemCalculated = nameof(InvoiceCharge.IsSystemCalculated);
		}

		public override void Delete()
		{
			if (J7_ChargeType == FRCustomsChargeTypeList.Codes.Cut)
			{
				RecalculateInvoiceHeaderBalance();
			}
			base.Delete();
		}

		public InvoiceCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceChargeValidation(this);

		[ReadOnlyMember(nameof(IsSystemCalculated))]
		public override ZString J7_ChargeType
		{
			get => base.J7_ChargeType;
			set
			{
				var oldValue = J7_ChargeType;
				var hasChanges = oldValue != value;
				base.J7_ChargeType = value;
				if (!IsCopying && hasChanges)
				{
					SetVatibilityFalseIfNecessary();
				}
				if (oldValue == FRCustomsChargeTypeList.Codes.Cut || value == FRCustomsChargeTypeList.Codes.Cut)
				{
					RecalculateInvoiceHeaderBalance();
				}
			}
		}

		void SetVatibilityFalseIfNecessary()
		{
			var declaration = Parent?.JobDeclaration;
			if (declaration != null && declaration.IsExport && IsTransportCharge)
			{
				J7_IsGSTApplicable = false;
			}
		}

		bool IsTransportCharge => J7_ChargeType == FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge
						|| J7_ChargeType == FRCustomsChargeTypeList.Codes.AirTransportCostsCharge
						|| J7_ChargeType == FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU
						|| J7_ChargeType == FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination
						|| J7_ChargeType == FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU
						|| J7_ChargeType == FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination
						|| J7_ChargeType == FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder
						|| J7_ChargeType == FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU
						|| J7_ChargeType == FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder
						|| J7_ChargeType == FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU
						|| J7_ChargeType == FRCustomsChargeTypeList.Codes.TransportCostsCharge
						|| J7_ChargeType == FRCustomsChargeTypeList.Codes.InsuranceCostsCharge;

		protected override bool GetJ7_Amount_ReadOnly() => base.GetJ7_Amount_ReadOnly() || IsSystemCalculated;

		protected override bool GetJ7_Percentage_ReadOnly() => base.GetJ7_Percentage_ReadOnly() || IsSystemCalculated;

		protected override bool GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly()
		{
			return IsIncludedInInvoiceAmountReadOnly() || base.GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly();
		}

		bool IsIncludedInInvoiceAmountReadOnly()
		{
			return ChargeCode is FlagManagedCharge flagManagedCharge && flagManagedCharge.IsIncludedInInvoiceDeemedForThisCharge;
		}

		[ResourceStringData("D3020BBD-2A8C-441A-AC41-34AF1D625873", Caption = "System Calculated")]
		public ZBool IsSystemCalculated
		{
			get => J7_IsCalculated;
			set
			{
				J7_IsCalculated = value;
				IsSystemCalculatedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSystemCalculatedInfo => GetZPropertyInfo(Schema.IsSystemCalculated);

		protected override void DefaultIsIncludedInInvoice(Common.IncoTermAndCustomsChargeFactory incoTermAndChargeFactory, ICustomsChargeCode charge)
		{
			if (charge is FlagManagedCharge chargeCode)
			{
				J7_Calc_IsIncludedInInvoiceAmount = chargeCode.IsIncludedInInvoice;
			}
			else
			{
				base.DefaultIsIncludedInInvoice(incoTermAndChargeFactory, charge);
			}
		}

		public override ZDecimal J7_Amount
		{
			get => base.J7_Amount;
			set
			{
				base.J7_Amount = value;
				if (J7_ChargeType == FRCustomsChargeTypeList.Codes.Cut)
				{
					RecalculateInvoiceHeaderBalance();
				}
			}
		}

		void RecalculateInvoiceHeaderBalance()
		{
			if (Parent is JobComInvoiceHeader header)
			{
				header.InvalidateJZ_Calc_LinesEnteredCache();
				header.JZ_Calc_BalanceStringInfo.RefreshBinding();
			}
		}
	}
}
