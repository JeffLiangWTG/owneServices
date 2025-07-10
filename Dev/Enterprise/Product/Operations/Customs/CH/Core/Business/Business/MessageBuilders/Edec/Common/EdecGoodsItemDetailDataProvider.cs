using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class EdecGoodsItemDetailDataProvider : IEdecGoodsItemDetail
{
	public EdecGoodsItemDetailDataProvider(ZString name, ZString value)
	{
		Name = name;
		Value = value;
	}

	public string Name { get; }

	public string Value { get; }

	public static IEnumerable<EdecGoodsItemDetailDataProvider> NewCollection(CusEntryLine entryLine)
	{
		if (entryLine != null)
		{
			foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
			{
				foreach (AdditionalInformation additionalInformation in invoiceLine.AdditionalInformations)
				{
					if (!additionalInformation.CSI_ReferenceNumber.IsEmpty)
					{
						yield return new EdecGoodsItemDetailDataProvider(additionalInformation.CSI_Code, additionalInformation.CSI_ReferenceNumber);
					}
				}

				foreach (CusVehicle vehicle in invoiceLine.Vehicles)
				{
					if (!vehicle.CVH_ModelName.IsEmpty)
					{
						yield return new EdecGoodsItemDetailDataProvider(VehicleModelNameKey, vehicle.CVH_ModelName);
					}

					if (!vehicle.CVH_VehicleIdentificationNumber.IsEmpty)
					{
						yield return new EdecGoodsItemDetailDataProvider(VehicleIdentificationNumberKey, vehicle.CVH_VehicleIdentificationNumber);
					}

					if (!vehicle.CVH_RegistrationNumber.IsEmpty)
					{
						yield return new EdecGoodsItemDetailDataProvider(VehicleRegistrationNumberKey, vehicle.CVH_RegistrationNumber);
					}
				}

				foreach (Tobacco tobacco in invoiceLine.Tobaccos)
				{
					if (!tobacco.CSI_Code.IsEmpty)
					{
						yield return new EdecGoodsItemDetailDataProvider(TobaccoCodeKey, tobacco.CSI_Code);
					}

					if (!tobacco.CSI_SubType.IsEmpty)
					{
						yield return new EdecGoodsItemDetailDataProvider(TobaccoSubTypeKey, tobacco.CSI_SubType);
					}

					if (!tobacco.CSI_Description.IsEmpty)
					{
						yield return new EdecGoodsItemDetailDataProvider(TobaccoDescriptionKey, tobacco.CSI_Description);
					}

					yield return new EdecGoodsItemDetailDataProvider(TobaccoItemNumberKey, tobacco.CSI_ItemNumber.ToString());
					yield return new EdecGoodsItemDetailDataProvider(TobaccoValueKey, tobacco.CSI_Value.ToString());

					if (!tobacco.CSI_AdditionalDescription.IsEmpty)
					{
						yield return new EdecGoodsItemDetailDataProvider(TobaccoAdditionalDescriptionKey, tobacco.CSI_AdditionalDescription);
					}
				}
			}
		}
	}

	const string VehicleModelNameKey = "1";
	const string VehicleIdentificationNumberKey = "2";
	const string VehicleRegistrationNumberKey = "3";

	const string TobaccoCodeKey = "7";
	const string TobaccoSubTypeKey = "8";
	const string TobaccoDescriptionKey = "9";
	const string TobaccoItemNumberKey = "10";
	const string TobaccoValueKey = "24";
	const string TobaccoAdditionalDescriptionKey = "25";
}
