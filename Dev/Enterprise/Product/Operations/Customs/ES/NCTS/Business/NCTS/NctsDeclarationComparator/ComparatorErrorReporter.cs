using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business;

public class ComparatorErrorReporter
{
	public ComparatorErrorReporter()
	{
		headerLevel = new();
		consignmentLevel = new();
		goodsLevel = new();
	}

	const int MaxNumberOfErrorsToReport = 5;

	readonly List<ZString> headerLevel;
	readonly List<Tuple<ZString, ZString>> consignmentLevel;
	readonly List<Tuple<ZString, ZString>> goodsLevel;

	public void AddError(ComparatorErrorLevel level, string field, ZString multipleObjectID)
	{
		switch (level)
		{
			case ComparatorErrorLevel.Header:
				headerLevel.Add(field);
				break;
			case ComparatorErrorLevel.Consignment:
				consignmentLevel.Add(new (field, multipleObjectID));
				break;
			case ComparatorErrorLevel.Goods:
				goodsLevel.Add(new(field, multipleObjectID));
				break;
		}
	}

	public string GetFormattedReport()
	{
		var builder = new ZStringBuilder();

		var amountErrors = 0;

		foreach (var error in headerLevel)
		{
			if (amountErrors == MaxNumberOfErrorsToReport)
			{
				break;
			}
			_ = builder.AppendLine(GetHeaderLevelMessage(headerFieldsMapping[error]));
			amountErrors++;
		}

		foreach (var error in consignmentLevel)
		{
			if (amountErrors == MaxNumberOfErrorsToReport)
			{
				break;
			}
			_ = builder.AppendLine(GetConsignmentLevelMessage(consignmentMapping[error.Item1], error.Item2));
			amountErrors++;
		}

		foreach (var error in goodsLevel)
		{
			if (amountErrors == MaxNumberOfErrorsToReport)
			{
				break;
			}
			_ = builder.AppendLine(GetGoodsLevelMessage(goodsMapping[error.Item1], error.Item2));
			amountErrors++;
		}

		var numberOfErrors = headerLevel.Count + consignmentLevel.Count + goodsLevel.Count;
		if (!builder.IsEmpty && numberOfErrors > 5)
		{
			_ = builder.AppendLine(Res.GetString("A8E7F83E-0B81-441F-BA6C-A7FEA0D7E7B4", "And {0} more errors.", numberOfErrors - MaxNumberOfErrorsToReport));
		}

		return builder.ToString().TrimEnd();
	}

	string GetHeaderLevelMessage(string field) => Res.GetString("45E78A26-2A41-4B2A-9A5C-2DDA4E0E2015", "Consignment/{0}", field);

	string GetConsignmentLevelMessage(string field, string id) => Res.GetString("8DF584E0-CCA1-418A-89D2-5B9ED750647E", "House Consignment({0})/{1}", id, field);

	string GetGoodsLevelMessage(string field, string id) => Res.GetString("E6A925D2-BC08-41D2-BCF3-7716298889BA", "Goods Item({0})/{1}", id, field);

	readonly ImmutableDictionary<string, string> headerFieldsMapping = new Dictionary<string, string>()
	{
		{ nameof(IComparablePredeclaration.ConsignorID), Res.GetString("5A2C2CFD-B11C-4716-AD5F-B50DDBF63FB1", "Consignor") },
		{ nameof(IComparablePredeclaration.ConsigneeID), Res.GetString("28B8918B-BC19-4409-8DFC-6349DA61AD25", "Consignee") },
		{ nameof(IComparablePredeclaration.DeclarationType), Res.GetString("81ED5E18-40BC-4F07-AA83-3D1A65349B3E", "Declaration Type") },
		{ nameof(IComparablePredeclaration.TIRCarnetNum), Res.GetString("7B43668E-7698-4AEA-AAB5-41757406B93E", "TIR Carnet Number") },
		{ nameof(IComparablePredeclaration.ReducedDatasetIndicator), Res.GetString("3C68D287-3D29-4329-A4DF-A995BB84D134", "Reduced Dataset Indicator") },
		{ nameof(IComparablePredeclaration.DispatchCountry), Res.GetString("FC502695-CF73-47F7-A1C4-21AD82570646", "Dispatch Country-Region") },
		{ nameof(IComparablePredeclaration.DestinationCountry), Res.GetString("243706EB-19DB-494A-879A-8D89BA737C95", "Destination Country-Region") },
		{ nameof(IComparablePredeclaration.GrossWeight), Res.GetString("3A245A80-4C2B-4774-8BB0-F2174BFFB518", "Gross Weight") },
		{ nameof(IComparablePredeclaration.UnloadingPlace), Res.GetString("032DCFF5-B18A-471A-BB54-37F68351E51C", "Place of Unloading") },
		{ nameof(IComparablePredeclaration.Circumstance), Res.GetString("F4F29AD5-E736-41F6-95D8-12C0C0284FE3", "Circumstance") },
		{ nameof(IComparablePredeclaration.UniqueConsignmentReference), Res.GetString("73F82205-3CCD-4017-8AB8-3DEDC33024C3", "Unique Consignment Reference") },
		{ nameof(IComparablePredeclaration.MethodOfPayment), Res.GetString("3DF40334-978D-40AF-BA83-397DF2C7810E", "Transport Method Of Payment") },
		{ nameof(IComparablePredeclaration.CarrierID), Res.GetString("18D54727-A22A-4C71-8CD8-A2A726049F16", "Carrier") },
		{ nameof(IComparablePredeclaration.CustomsOfficeOfDeparture), Res.GetString("3582AA4E-C0C1-4FD1-ABDE-205E91B0919D", "Customs Office Of Departure") },
		{ nameof(IComparablePredeclaration.CustomsOfficeOfDestination), Res.GetString("B19E2557-B803-4CF1-966A-6428C6146E0C", "Customs Office Of Destination") },
		{ nameof(IComparablePredeclaration.CustomsOfficeOfTransit), Res.GetString("BE920083-F2C6-45FB-95E8-7912EBAF8D3C", "Customs Office Of Transit") },
		{ nameof(IComparablePredeclaration.CustomsOfficeOfExitForTransit), Res.GetString("D6737F5E-3CFA-49B6-BE58-A259131C8100", "Customs Office Of Exit For Transit") },
		{ nameof(IComparablePredeclaration.Guarantees), Res.GetString("B3F5189F-1836-480A-BEA0-F496927409BC", "Guarantees") },
		{ nameof(IComparablePredeclaration.Authorizations), Res.GetString("0ACA0E7A-640C-4237-AD72-CE43D905AA58", "Authorizations") },
		{ nameof(IComparablePredeclaration.SupportingDocuments), Res.GetString("37D824A3-4EC0-4195-950F-48D84DFCA13D", "Supporting Documents") },
		{ nameof(IComparablePredeclaration.AdditionalDocumentsTD), Res.GetString("50B59584-AD04-48E8-A721-7DD6C72B5FF0", "Additional Documents: Transport Document") },
		{ nameof(IComparablePredeclaration.AdditionalDocumentsAR), Res.GetString("4DA3DC00-60FD-4C22-8326-0072E3BF1C41", "Additional Documents: Additional Reference") },
		{ nameof(IComparablePredeclaration.AdditionalDocumentsAI), Res.GetString("C944D66F-3B0B-4AB6-A65F-9671E9CB4AFA", "Additional Documents: Additional Information") },
		{ nameof(IComparablePredeclaration.CountriesOfRouting), Res.GetString("C362B300-5BBB-4438-8673-9E1B578E28F1", "Countries Of Routing") },
		{ nameof(IComparablePredeclaration.SupplyChainActors), Res.GetString("91AA9375-89F2-4721-9E75-45FCF487D5B6", "Supply Chain Actors") },
		{ nameof(IComparablePredeclaration.HouseConsignments), Res.GetString("A8B3150D-F08C-464A-82BA-CCF21200DC90", "House Consignments: The elements of this data group are different") },
	}.ToImmutableDictionary();

	public readonly ImmutableDictionary<string, string> consignmentMapping = new Dictionary<string, string>()
	{
		{ nameof(IComparableHouseConsignment.HouseNumber), Res.GetString("C90AD602-BEA7-4906-80A3-D693654B3664", "Sequence Number") },
		{ nameof(IComparableHouseConsignment.TotalGrossWeight), Res.GetString("1E28D344-D951-4DF9-8F9F-D2127B353742", "Total Gross Weight") },
		{ nameof(IComparableHouseConsignment.SupportingDocuments), Res.GetString("6E76D9F5-006B-41B9-AF26-69F92E1AE57B", "Supporting Documents") },
		{ nameof(IComparableHouseConsignment.AdditionalDocumentsTD), Res.GetString("36BEA243-CF1D-4EC4-84E6-BEA81C597C23", "Additional Documents: Transport Document") },
		{ nameof(IComparableHouseConsignment.AdditionalDocumentsAR), Res.GetString("477A8E8B-3308-4194-AFA1-FF823658CEE2", "Additional Documents: Additional Reference") },
		{ nameof(IComparableHouseConsignment.AdditionalDocumentsAI), Res.GetString("86829C5D-1849-48C7-950B-4C6B2BB51673", "Additional Documents: Additional Information") },
		{ nameof(IComparableHouseConsignment.PreviousDocuments), Res.GetString("F018755E-308A-48CE-B388-0289E2CA9F58", "Previous Documents") },
		{ nameof(IComparableHouseConsignment.SupplyChainActors), Res.GetString("9389E8AA-FF17-495C-84C7-559D9B4EBCC6", "Supply Chain Actors") },
		{ nameof(IComparableHouseConsignment.GoodsItems), Res.GetString("720D35C2-657F-40C3-BA91-9534D08BDAC4", "Goods Items: The elements of this data group are different") },
	}.ToImmutableDictionary();

	public readonly ImmutableDictionary<string, string> goodsMapping = new Dictionary<string, string>()
	{
		{ nameof(IComparableGoodsItems.ItemNumber), Res.GetString("C875F0F5-E4C9-4234-B81A-FEA8DCF69CE1", "Item Number") },
		{ nameof(IComparableGoodsItems.DeclarationItemNumber), Res.GetString("3B7A1D21-C13D-44E1-BC27-8275018E54F5", "Declaration Item Number") },
		{ nameof(IComparableGoodsItems.DeclarationType), Res.GetString("AF2B7D74-FAAE-49D4-B838-538ED2A06A2E", "Declaration Type") },
		{ nameof(IComparableGoodsItems.OriginCountry), Res.GetString("0895FFB4-F3FB-4043-913E-FD0DD07D9118", "Origin Country-Region") },
		{ nameof(IComparableGoodsItems.DestinationCountry), Res.GetString("A5818B88-BE0F-4154-8887-51381B0BAECC", "Destination Country-Region") },
		{ nameof(IComparableGoodsItems.CommercialReference), Res.GetString("211CC72B-CFB2-4D7A-B1F6-5EE3733A4BF4", "Commercial Reference") },
		{ nameof(IComparableGoodsItems.ConsigneeID), Res.GetString("EF79863F-785C-43EA-8EEF-7D3476F17B2F", "Consignee") },
		{ nameof(IComparableGoodsItems.GoodsDescription), Res.GetString("1B23878D-D63C-4E60-A21C-8C266D8C572B", "Goods Description") },
		{ nameof(IComparableGoodsItems.CusCode), Res.GetString("4B4411A9-83BB-4159-9C03-55B3FA19B7B1", "CUS-Code") },
		{ nameof(IComparableGoodsItems.CommodityCode), Res.GetString("E4DEDF0B-ED4B-49EB-B63F-E7DD5A28B677", "Commodity Code") },
		{ nameof(IComparableGoodsItems.GrossWeight), Res.GetString("35F83FFE-3E00-4C49-A931-8BAE9B958617", "Gross Weight") },
		{ nameof(IComparableGoodsItems.NetWeight), Res.GetString("E0F6144C-9063-44B7-8A9F-3C4A79606B3C", "Net Weight") },
		{ nameof(IComparableGoodsItems.SupplementaryQty), Res.GetString("3F8ADD35-02ED-4313-AE1A-5F374E51FF7C", "Supplementary Quantity") },
		{ nameof(IComparableGoodsItems.DangerousGoods), Res.GetString("7868F805-C80D-472C-9B91-827CF24B4D25", "Dangerous Goods") },
		{ nameof(IComparableGoodsItems.SupplyChainActors), Res.GetString("AE043DF5-15A6-43FE-BF63-2A2A30BE1815", "Supply Chain Actors") },
		{ nameof(IComparableGoodsItems.PackagesAndVehicles), Res.GetString("F8B76BBD-153B-49B8-9C7C-D886CB7089CD", "Packages-Vehicles") },
		{ nameof(IComparableGoodsItems.PreviousDocuments), Res.GetString("EB2008E3-A19C-4FCD-A861-13796159E51C", "Previous Documents") },
		{ nameof(IComparableGoodsItems.SupportingDocuments), Res.GetString("2BA5DE64-EF5B-4A8E-9FDA-92AA7ADE95BB", "Supporting Documents") },
		{ nameof(IComparableGoodsItems.AdditionalDocumentsTD), Res.GetString("AA919C80-CEC0-4B01-9412-861E406A7FCE", "Additional Documents: Transport Document") },
		{ nameof(IComparableGoodsItems.AdditionalDocumentsAR), Res.GetString("F2DBDD01-9C97-4EA6-98B5-92B7DA4943E2", "Additional Documents: Additional Reference") },
		{ nameof(IComparableGoodsItems.AdditionalDocumentsAI), Res.GetString("8C3CAEC5-8A5C-4E04-85E3-5B29616E8DDC", "Additional Documents: Additional Information") },
	}.ToImmutableDictionary();
}

public enum ComparatorErrorLevel { Header, Consignment, Goods }
