using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class GroupInvoiceCharge : EU.Business.Declaration.GroupInvoiceCharge, Integration.Customs.FR.IGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : EU.Business.Declaration.InvoiceCharge.Schema
		{
			public const string IsSystemCalculated = "IsSystemCalculated";
		}

		[ReadOnlyMember(nameof(IsSystemCalculated))]
		public override ZString J7_ChargeType
		{
			get => base.J7_ChargeType;
			set
			{
				var hasChanges = base.J7_ChargeType != value;
				base.J7_ChargeType = value;
				if (!IsCopying && hasChanges)
				{
					SetVatibilityFalseIfNecessary();
				}
			}
		}

		protected override bool GetJ7_Amount_ReadOnly() => base.GetJ7_Amount_ReadOnly() || IsSystemCalculated;

		protected override bool GetJ7_RX_NKCurrency_ReadOnly() => base.GetJ7_RX_NKCurrency_ReadOnly() || IsSystemCalculated;

		protected override bool GetJ7_Percentage_ReadOnly() => base.GetJ7_Percentage_ReadOnly() || IsSystemCalculated;

		[ResourceStringData("047914BC-5806-46EA-A2AE-CAACAA4FC2E9", Caption = "System Calculated")]
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

		protected override void UpdateIsDutiableRelatedFieldsIfNecessary()
		{
			base.UpdateIsDutiableRelatedFieldsIfNecessary();
			SetVatibilityFalseIfNecessary();
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

		void SetVatibilityFalseIfNecessary()
		{
			var declaration = Parent?.JobDeclaration;
			if (declaration != null && declaration.IsExport && IsTransportCharge)
			{
				J7_IsGSTApplicable = false;
			}
		}
	}
}
