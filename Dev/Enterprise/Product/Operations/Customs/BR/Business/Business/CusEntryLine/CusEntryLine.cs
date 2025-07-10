using System.Data;
using System.Linq;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.ImportSiscomex;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public partial class CusEntryLine : Customs.Business.CusEntryLine, IICMSCalculationValues
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool IsActive => CL_CustomsPostedStatus != CustomsPostedStatusList.Codes.Deleted && CL_CustomsPostedStatus != CustomsPostedStatusList.Codes.DeletePending;

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryLine|ImportLicenseNumber", Caption = "Import License No.", FullDescription = "Import License Number.")]
		public ZString ImportLicenseNumber => RandomLine?.ImportLicenseNumber ?? ZString.Empty;

		protected override bool CanBeLinkedUpByPivot => !Header?.IsFormalEntry ?? false;

		#region Duty

		public ZDecimal DutyDueAmount => DutyAdditionTax?.TaxPayable ?? 0m;
		public ZDecimal DutyAgreementRate => DutyAdditionTax?.AgreementPercentual ?? 0m;
		public ZDecimal DutyReducedRate => DutyAdditionTax?.ReducedRatePercentage ?? 0m;
		public ZDecimal DutyReductionPercentage => DutyAdditionTax?.IPTReductionPercentage ?? 0m;

		#endregion

		#region IPI

		public ZDecimal IPIDueAmount => IPIAdditionTax?.TaxPayable ?? 0m;
		public ZDecimal IPIReducedRate => IPIAdditionTax?.ReducedRatePercentage ?? 0m;

		#endregion

		#region Antidumping

		public ZDecimal AntidumpingDueAmount => AntidumpingAdditionTax?.TaxPayable ?? 0m;

		#endregion

		#region PIS / Cofins

		public ZDecimal PISDueAmount => PISAdditionTax?.TaxPayable ?? 0m;
		public ZDecimal PISCofinsReducedRate => PISAdditionTax?.ReducedRatePercentage ?? 0m;
		public ZDecimal PISCofinsReductionPercentage => PISAdditionTax?.IPTReductionPercentage ?? 0m;
		public ZDecimal CofinsDueAmount => CofinsAdditionTax?.TaxPayable ?? 0m;

		#endregion

		#region IDeclarationAdditionTax

		IDeclarationAdditionTax DutyAdditionTax => Factory.GetCached(ref dutyAdditionTax, () => DeclarationAdditionTaxProvider.New(this, ChargeTypesList.Codes.DTY).FirstOrDefault());
		CachedProperty<IDeclarationAdditionTax> dutyAdditionTax;

		IDeclarationAdditionTax IPIAdditionTax => Factory.GetCached(ref ipiAdditionTax, () => DeclarationAdditionTaxProvider.New(this, Constants.RateTypes.IPI).FirstOrDefault());
		CachedProperty<IDeclarationAdditionTax> ipiAdditionTax;

		IDeclarationAdditionTax PISAdditionTax => Factory.GetCached(ref pisAdditionTax, () => DeclarationAdditionTaxProvider.New(this, Constants.RateTypes.PIS).FirstOrDefault());
		CachedProperty<IDeclarationAdditionTax> pisAdditionTax;

		IDeclarationAdditionTax CofinsAdditionTax => Factory.GetCached(ref cofinsAdditionTax, () => DeclarationAdditionTaxProvider.New(this, Constants.RateTypes.Cofins).FirstOrDefault());
		CachedProperty<IDeclarationAdditionTax> cofinsAdditionTax;

		IDeclarationAdditionTax AntidumpingAdditionTax => Factory.GetCached(ref antidumpingAdditionTax, () => DeclarationAdditionTaxProvider.New(this, Constants.RateTypes.Antidumping).FirstOrDefault());
		CachedProperty<IDeclarationAdditionTax> antidumpingAdditionTax;

		#endregion

		#region IICMSCalculationValues

		ZDecimal IICMSCalculationValues.CustomsValue => CL_CustomsValue;
		ZDecimal IICMSCalculationValues.DutyAmount => Fees.GetAmount(ChargeTypesList.Codes.DTY);
		ZDecimal IICMSCalculationValues.IPIAmount => Fees.GetAmount(Constants.RateTypes.IPI);
		ZDecimal IICMSCalculationValues.PISAmount => Fees.GetAmount(Constants.RateTypes.PIS);
		ZDecimal IICMSCalculationValues.CofinsAmount => Fees.GetAmount(Constants.RateTypes.Cofins);
		ZDecimal IICMSCalculationValues.SiscomexUsageAmount => Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee);
		ZDecimal IICMSCalculationValues.AfrmmTaxAmount => Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax);
		ZDecimal IICMSCalculationValues.AntidumpingAmount => Fees.GetAmount(Constants.RateTypes.Antidumping);
		ZDecimal IICMSCalculationValues.EICAmount => Fees.GetAmount(Constants.RateTypes.OtherExpensesICMS);

		#endregion
	}
}
