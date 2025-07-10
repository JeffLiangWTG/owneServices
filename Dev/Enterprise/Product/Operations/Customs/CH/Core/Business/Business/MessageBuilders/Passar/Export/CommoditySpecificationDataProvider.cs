using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class CommoditySpecificationDataProvider : ICommoditySpecification
{
	public static CommoditySpecificationDataProvider New(CusEntryLine entryLine) => entryLine == null ? null : new CommoditySpecificationDataProvider(entryLine);

	CommoditySpecificationDataProvider(CusEntryLine entryLine)
	{
		this.entryLine = entryLine;
		invoiceLine = entryLine.RandomLine;
		isSimplified = entryLine.Header.EntryInstruction.IsSimplified;
	}
	readonly CusEntryLine entryLine;
	readonly JobComInvoiceLine invoiceLine;
	readonly bool isSimplified;

	public int BorderValue => (int)entryLine.CL_StatisticalValue;

	public string InvoiceCurrency => Core.Constants.CurrencyCodes.Switzerland;

	public bool? GoodsReturned => invoiceLine.EntryInstruction.IsSimplified ? null : invoiceLine.JI_GoodsReturned;

	public bool? NonTradingGoods => isSimplified ? null : invoiceLine.JI_NonTradingGoods;

	public bool? OwnPropulsion => isSimplified ? null : entryLine.Declaration.IsOwnPropulsion;

	public int? CompensationType => invoiceLine.EntryInstruction.IsSimplified ? null : int.TryParse(invoiceLine.JI_RefundType, out var value) ? value : null;

	public bool? RestrictionObligation => invoiceLine.EntryInstruction.IsSimplified && !Restrictions.Any() ? null : Restrictions.Any();

	public bool? Repair => isSimplified && !invoiceLine.InAndOutwardProcessingRepair ? null : invoiceLine.InAndOutwardProcessingRepair;

	public IReadOnlyCollection<IRestriction> Restrictions => restrictions ??= RestrictionDataProvider.NewCollection(invoiceLine.Restrictions).ToArray();
	IReadOnlyCollection<IRestriction> restrictions;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??= GetAdditionalInformations().ToArray();

	public string CountryOfOrigin => null;

	public string CountryOfProduction => null;

	public string InvoiceValue => null;

	public bool? NetAssessment => null;

	public bool? Preference => null;

	public IReadOnlyCollection<IAdditionalTax> AdditionalTaxes => null;

	public IReadOnlyCollection<IFee> Fees => null;

	public IAssessment NetWeightAssessment => null;

	public ITaxInformation TaxInformation => null;

	IReadOnlyCollection<IAdditionalInformation> additionalInformations;

	IEnumerable<AdditionalInformationDataProvider> GetAdditionalInformations()
	{
		var providers = AdditionalInformationDataProvider.NewCollection(invoiceLine.AdditionalInformations);
		providers = providers.Concat(GetVehiclesAdditionalInformations(providers.Count()));
		providers = providers.Concat(GetTobaccoAdditionalInformations(providers.Count()));
		return providers;
	}

	IEnumerable<AdditionalInformationDataProvider> GetVehiclesAdditionalInformations(int offset)
	{
		foreach (var v in invoiceLine.Vehicles.Cast<CusVehicle>())
		{
			if (!v.CVH_VehicleIdentificationNumber.IsEmpty)
			{
				yield return AdditionalInformationDataProvider.New(++offset, AdditionalInformationTypeCodes.VehicleIdentificationNumber, v.CVH_VehicleIdentificationNumber);
			}

			if (!v.CVH_RegistrationNumber.IsEmpty)
			{
				yield return AdditionalInformationDataProvider.New(++offset, AdditionalInformationTypeCodes.VehicleMatriculationNumber, v.CVH_RegistrationNumber);
			}

			if (!v.CVH_ModelName.IsEmpty)
			{
				yield return AdditionalInformationDataProvider.New(++offset, AdditionalInformationTypeCodes.VehicleBrand, v.CVH_ModelName);
			}
		}
	}
	
	IEnumerable<AdditionalInformationDataProvider> GetTobaccoAdditionalInformations(int offset)
	{
		foreach (var tobacco in invoiceLine.Tobaccos)
		{
			if (!tobacco.CSI_Code.IsEmpty)
			{
				yield return AdditionalInformationDataProvider.New(++offset, UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup, tobacco.CSI_Code);
			}
			if (!tobacco.CSI_SubType.IsEmpty)
			{
				yield return AdditionalInformationDataProvider.New(++offset, UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductSubgroup, tobacco.CSI_SubType);
			}
			if (!tobacco.CSI_ItemNumber.IsEmpty)
			{
				yield return AdditionalInformationDataProvider.New(++offset, UniversalReferenceConstants.AdditionalInformationTypeCodes.A1404, tobacco.CSI_ItemNumber.ToStringInvariantCulture());
			}
			if (!tobacco.CSI_ReferenceNumber.IsEmpty)
			{
				yield return AdditionalInformationDataProvider.New(++offset, UniversalReferenceConstants.AdditionalInformationTypeCodes.A1401, tobacco.CSI_ReferenceNumber);
			}
			if (!tobacco.CSI_Value.IsEmpty)
			{
				yield return AdditionalInformationDataProvider.New(++offset, UniversalReferenceConstants.AdditionalInformationTypeCodes.A1405, tobacco.CSI_Value.ToStringInvariantCulture());
			}
			if (!tobacco.CSI_UnitOfQuantity.IsEmpty)
			{
				var text = tobacco.CSI_UnitOfQuantity == TobaccoSpecialUnitOfMeasureList.Codes.Pieces ? (NoResString)"Stück" : Core.Constants.Weight.Kilograms.ToLowerInvariant();
				yield return AdditionalInformationDataProvider.New(++offset, UniversalReferenceConstants.AdditionalInformationTypeCodes.A1406, text);
			}
		}
	}
}
