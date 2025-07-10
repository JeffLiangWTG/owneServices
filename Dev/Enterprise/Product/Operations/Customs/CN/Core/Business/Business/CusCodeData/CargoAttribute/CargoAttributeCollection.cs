using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CargoAttributeCollection : CusCodeDataCollection<CargoAttribute>, ICodeDescriptionOptionStorage
	{
		public CargoAttributeCollection(JobComInvoiceLine invoiceLine) : base(invoiceLine, Constants.CusCodeDataTypes.Codes.CargoAttribute)
		{
			cargoAttributesAsStringInfo = invoiceLine.CargoAttributesAsStringInfo;
		}

		public CargoAttributeCollection(CusClassPartPivot pivot) : base(pivot, Constants.CusCodeDataTypes.Codes.CargoAttribute)
		{
			cargoAttributesAsStringInfo = pivot.CargoAttributesAsStringInfo;
		}

		readonly ZPropertyInfo cargoAttributesAsStringInfo;

		BusinessObject ICodeDescriptionOptionStorage.FindByCode(ZString code)
		{
			return GetFirstElementHaving(code);
		}

		void ICodeDescriptionOptionStorage.AddNew(ZString code)
		{
			AddNew(code);

			if (Master is JobComInvoiceLine invoiceLine)
			{
				if (invoiceLine.IsImport && code == CargoAttributeList.Codes._12 && invoiceLine.JI_CIQEndUse.IsEmpty)
				{
					invoiceLine.JI_CIQEndUse = EndUseList.Codes.OnlyIndustrialUse;
				}

				if (CargoAttributeList.IsDangerousGoodsAttribute(code))
				{
					invoiceLine.JI_NonDangerousChemicalFlag = code == CargoAttributeList.Codes._33;

					if (!invoiceLine.IsValidationSuspended)
					{
						invoiceLine.Validation.ValidateDangerousGoodsDGSubs();
						invoiceLine.Validation.ValidateJI_PackageTypeOfUNDG();
					}
				}
			}
		}

		public IEnumerable<ZString> AllCodes => this.Cast<CargoAttribute>().Select(codeData => codeData.CY_Code);

		ICodeDescriptionPairList ICodeDescriptionOptionStorage.GetAllOptions() => Factory.GetCachedValue<CargoAttributeList>();

		void ICodeDescriptionOptionStorage.ValidateSeletedOption(ZString code, bool selected, ZPropertyInfo propertyInfo, IEnumerable<ZString> selectedCodes)
		{
			this.ValidateSelection(code, selected, propertyInfo, selectedCodes);
		}

		ZPropertyInfo ICodeDescriptionOptionStorage.SelectedOptionsAsStringPropertyInfo => cargoAttributesAsStringInfo;

		public IValidationModeProvider ValidationModeProvider => (Master as JobComInvoiceLine).Declaration;

		public bool IsProvidedAny(params string[] cargoAttributeTypes)
		{
			return this.Cast<CargoAttribute>().Any(x => cargoAttributeTypes.Contains(x.CY_Code.ToString()));
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (bizO is CargoAttribute cargoAttribute)
			{
				cargoAttribute.ReloadAttachmentLinksOnInvoiceLine(Master as JobComInvoiceLine);
			}
		}
	}
}
