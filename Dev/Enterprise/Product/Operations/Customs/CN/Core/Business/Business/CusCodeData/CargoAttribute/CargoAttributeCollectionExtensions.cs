using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public static class CargoAttributeCollectionExtensions
	{
		public static void ValidateSelection(this CargoAttributeCollection cargoAttributes, ZString code, bool selected, ZPropertyInfo targetInfo, IEnumerable<ZString> selectedCodes)
		{
			if (cargoAttributes.Master is JobComInvoiceLine invoiceLine)
			{
				if (selected)
				{
					CheckRequiredProductQualification(invoiceLine, code, targetInfo, selectedCodes);
					CheckRequiredAttachmentTypes(invoiceLine, code, targetInfo, selectedCodes);
				}
				else
				{
					CheckRequiredCargoAttributeByEndUse(invoiceLine, targetInfo, selectedCodes);
					CheckRequiredCargoAttributeByDangerousGoods(invoiceLine, code, targetInfo, selectedCodes);
				}
			}
		}

		static void CheckRequiredProductQualification(JobComInvoiceLine invoiceLine, string cargoAttribute, ZPropertyInfo targetInfo, IEnumerable<ZString> selectedCodes)
		{
			if (invoiceLine.CIQRequires)
			{
				var requiredQualifications = GetRequiredProductQualifications(invoiceLine, cargoAttribute, selectedCodes);
				if (requiredQualifications != null && requiredQualifications.Any() && !invoiceLine.CIQProductQualifications.IsProvidedAny(requiredQualifications))
				{
					targetInfo.AddNotification(Res.GetString("85a3c2ce-bff0-4f9f-b386-ddc87c1f5c9d", "Product Qualification {1} is required for Cargo Attribute {0}", cargoAttribute, string.Join(",", requiredQualifications)), invoiceLine);
				}
				var notRequiredQualifications = GetNotRequiredProductQualifications(invoiceLine, cargoAttribute);
				if (notRequiredQualifications != null && notRequiredQualifications.Any() && invoiceLine.CIQProductQualifications.IsProvidedAny(notRequiredQualifications))
				{
					targetInfo.AddNotification(Res.GetString("add08101-6db3-4d59-94b4-74c441598d9d", "Product Qualification {1} is NOT required for Cargo Attribute {0}", cargoAttribute, string.Join(",", notRequiredQualifications)), invoiceLine);
				}
			}
		}

		static string[] GetRequiredProductQualifications(JobComInvoiceLine invoiceLine, string cargoAttribute, IEnumerable<ZString> selectedCodes)
		{
			string[] qualifications = null;

			if (invoiceLine.IsImport)
			{
				switch (cargoAttribute)
				{
					case CargoAttributeList.Codes._11:
						qualifications = new[] { ProductQualificationCodeList.Codes._411 };
						break;
					case CargoAttributeList.Codes._12:
					case CargoAttributeList.Codes._13:
						qualifications = new[] { ProductQualificationCodeList.Codes._410 };
						break;
					case CargoAttributeList.Codes._14:
						qualifications = selectedCodes.Contains(CargoAttributeList.Codes._18)
								 ? new[] { ProductQualificationCodeList.Codes._519 }
								 : new[] { ProductQualificationCodeList.Codes._517, ProductQualificationCodeList.Codes._519, ProductQualificationCodeList.Codes._401 };
						break;
				}
			}

			switch (cargoAttribute)
			{
				case CargoAttributeList.Codes._25:
				case CargoAttributeList.Codes._26:
				case CargoAttributeList.Codes._27:
				case CargoAttributeList.Codes._28:
				case CargoAttributeList.Codes._29:
					qualifications = new[] { ProductQualificationCodeList.Codes._203, ProductQualificationCodeList.Codes._401 };
					break;
			}

			return qualifications;
		}

		static string[] GetNotRequiredProductQualifications(JobComInvoiceLine invoiceLine, string cargoAttribute)
		{
			string[] qualifications = null;

			if (invoiceLine.IsImport)
			{
				switch (cargoAttribute)
				{
					case CargoAttributeList.Codes._15:
						qualifications = new[] { ProductQualificationCodeList.Codes._517 };
						break;
				}
			}

			return qualifications;
		}

		static void CheckRequiredCargoAttributeByEndUse(JobComInvoiceLine invoiceLine, ZPropertyInfo targetInfo, IEnumerable<ZString> selectedCodes)
		{
			var requiredCargoAttributes = EndUseList.GetRequiredCargoAttributesByCIQEndUse(invoiceLine.JI_CIQEndUse);
			if (requiredCargoAttributes != null && requiredCargoAttributes.Any() && !requiredCargoAttributes.Any(c => selectedCodes.Contains(c)))
			{
				targetInfo.AddNotification(Res.GetString("94275C95-94D1-4E51-8C13-7CC79310E80E", "Cargo Attribute {0} needs to be selected for End Use ‘{1}’.", string.Join(",", requiredCargoAttributes), invoiceLine.JI_CIQEndUse), invoiceLine);
			}
		}

		static void CheckRequiredCargoAttributeByDangerousGoods(JobComInvoiceLine invoiceLine, ZString code, ZPropertyInfo targetInfo, IEnumerable<ZString> selectedCodes)
		{
			var tariff = invoiceLine.UniversalTariff;
			if (tariff != null)
			{
				if (code == CargoAttributeList.Codes._21 && invoiceLine.IsImport && invoiceLine.NameOfGoodsSeemsTobeUsedProduct && tariff.HasCommodityTypeUME() && !selectedCodes.Contains(CargoAttributeList.Codes._21))
				{
					targetInfo.AddWarning(Res.GetString("A918D3F2-B598-4493-9FD7-0C1D8B506A22", "The goods seems to be used mechanical and electrical products, '{0}' may need to be selected.", CargoAttributeList.Descriptions._21));
				}

				if (CargoAttributeList.IsDangerousGoodsAttribute(code) && tariff.HasCommodityTypeDGC() && !selectedCodes.Any(c => CargoAttributeList.IsDangerousGoodsAttribute(c)))
				{
					var cargoAttributeList = invoiceLine.Factory.GetCachedValue<CargoAttributeList>();

					targetInfo.AddNotification(Res.GetString(
						"4396BA10-0F15-4E43-B04D-471832A3C67D",
						"Tariff '{0}' seem to be dangerous chemical, please select one of '{1}' for Cargo Attributes.",
						invoiceLine.JI_Tariff, CargoAttributeList.DangerousGoodsAttributes.Select(c => cargoAttributeList.GetDescriptionFromCode(c)).JoinAsString("', '")
					), invoiceLine);
				}
			}
		}

		static void CheckRequiredAttachmentTypes(JobComInvoiceLine invoiceLine, ZString code, ZPropertyInfo targetInfo, IEnumerable<ZString> selectedCodes)
		{
			if (invoiceLine.IsImport)
			{
				var requiredAttachmentTypes = CSDDocTypeList.GetRequiredAttachmentTypesByCargoAttribute(code);
				if (requiredAttachmentTypes.Any())
				{
					var attachmentTypes = invoiceLine.LinkedAttachmentTypes;
					foreach (var attachmentType in requiredAttachmentTypes.Where(type => !attachmentTypes.Contains(type)))
					{
						targetInfo.AddNotification(Res.GetString("B6CD9F30-8831-4D03-BEB8-E0BD5410A451", "Invoice line: {0} should be linked to attachment type as {1} when Cargo Attributes is {2}", invoiceLine.JI_LineNo, attachmentType, code), invoiceLine);
					}
				}
			}
		}

		public static bool IsUsedProductAttributeSelected(this CargoAttributeCollection cargoAttributes)
		{
			return cargoAttributes.ContainsCode(CargoAttributeList.Codes._21);
		}

		public static bool IsAnyDangerousGoodsAttributeSelected(this CargoAttributeCollection cargoAttributes)
		{
			return cargoAttributes.AllCodes.Any(c => CargoAttributeList.IsDangerousGoodsAttribute(c));
		}
	}
}
