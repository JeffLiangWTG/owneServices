using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business
{
	public class ImportJobComInvoiceLineValidation : BaseImportJobComInvoiceLineValidation
	{
		public ImportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override IEnumerable<IZZRateSelectionCriteria> RateSelectionCriteriaLists => new List<IZZRateSelectionCriteria>() { };

		protected override void CheckJI_CGC_Catalog()
		{
			base.CheckJI_CGC_Catalog();

			var catalogInfo = Parent.JI_CGC_CatalogInfo;
			MandatoryValidation.MessageErrorIfNotEntered(catalogInfo);

			if (Parent.GoodsCatalog is CusGoodsCatalog goodsCatalog)
			{
				if (goodsCatalog.CGC_AuthorityStatus != GoodsCatalogStatusTypeList.Codes.Active)
				{
					catalogInfo.AddMessageError(Res.GetString("02F416FD-98DD-4994-B0C5-F7095A92929F", "The Goods Catalog is not Active."));
				}

				if (Parent.Importer?.GetRootCNPJFromCNPJ() != goodsCatalog.Owner?.GetRootCNPJFromCNPJ())
				{
					catalogInfo.AddMessageError(Res.GetString("F16AE5E2-A3BD-4887-B687-3A3E5E0C2CB1", "The Root CNPJ of the Owner set to this Goods Catalog does not match the Root CNPJ of the Importer of this job."));
				}

				if (!goodsCatalog.HasAuthorityIdentifier)
				{
					catalogInfo.AddMessageError(Res.GetString("33180E1C-1B4C-4CD4-9EE7-5B41EAAF10DF", "This Goods Catalog does not contain an Authority Code."));
				}

				if (!goodsCatalog.ForeignOperators.Any(x => x.CGI_CustomsStatus.IsAccepted()))
				{
					catalogInfo.AddMessageError(Res.GetString("411b21e8-14da-4664-950b-acae020ffcbf", "This Catalog does not contain a valid known or unknown Foreign Operator (Customs Status is currently different from Accepted)."));
				}

				if (!goodsCatalog.IsMessageSent)
				{
					catalogInfo.AddMessageError(Res.GetString("5AF8EBC2-7010-43C5-929C-A72B95FE301E", "This Goods Catalog should not be used because there might be messages that need to be sent (Latest Message Status is currently NST - Not Sent)."));
				}
				else if (goodsCatalog.IsMessageAwaitingResponse)
				{
					catalogInfo.AddMessageError(Res.GetString("A1F5F20B-BD24-4F52-9B36-3F1BD34F2E63", "This Goods Catalog should not be used because there is a message waiting for response (Latest Message Status is currently AWA - Awaiting Response)."));
				}
				else if (goodsCatalog.IsMessageRejected)
				{
					catalogInfo.AddMessageError(Res.GetString("9A1ADCFA-438F-4BA3-BDB0-7A9FC0F54810", "This Goods Catalog should not be used due to its last message being rejected (Latest Message Status is currently REJ - Rejected)."));
				}

				if (Parent.Pivot is CusClassPartPivot pivot && !pivot.CI_CGC_Catalog.IsEmpty && pivot.CI_CGC_Catalog != Parent.JI_CGC_Catalog)
				{
					catalogInfo.AddMessageError(Res.GetString("3FDF793E-30DE-47CF-8461-FB31A5B43250", "Catalog does not match with the product file."));
				}

				if (Parent.JI_CatalogAuthorityVersion != goodsCatalog.CGC_AuthorityVersion)
				{
					var cusEntryHeader = Parent.CusEntryLine?.Header as CusEntryHeader;
					var authorityVersion = int.TryParse(cusEntryHeader?.CH_AuthorityVersion, out var value) ? value : 0;
					if (authorityVersion == 0)
					{
						catalogInfo.AddMessageError(Res.GetString("ABE6AF20-6C75-45C7-A97C-CC961E1FC28A", "The version stored in this invoice line differs from the Goods Catalog registration."));
					}
				}
			}
		}

		protected override void CheckJI_OA_ManufacturerAddress()
		{
			base.CheckJI_OA_ManufacturerAddress();
			var propertyInfo = Parent.JI_OA_ManufacturerAddressInfo;
			if (Parent.IsImportOnly && Parent.JI_OA_ManufacturerAddress.IsValid && Parent.IsAttachedToPersistentDeclaration && BRCustomsDataRegistry.Instance.EnableForeignOperator.Value)
			{
				var foreignOperator = Parent.ForeignOperator;
				if (foreignOperator == null)
				{
					propertyInfo.AddMessageError(Res.GetString("6F63CBDC-9BBC-4415-93CF-DE533221F2A9", "Manufacturer choose is not a Foreign Operator."));
				}
				else if (foreignOperator.BFR_AuthorityIdentifier.IsEmpty)
				{
					propertyInfo.AddMessageError(Res.GetString("F2685F7F-FC57-405F-B34D-436876897BF4", "Manufacturer choose is a Foreign Operator but has no Authority Identifier."));
				}
				else if (foreignOperator.BFR_MessageStatus == BRMessageStatusList.Codes.NotSent)
				{
					propertyInfo.AddMessageError(Res.GetString("35FBEB6F-EBBB-4FB5-8657-F999A0860542", "Manufacturer should not be used because there might be messages that need to be sent (Message Status is currently NST – Not Sent)."));
				}
				else if (foreignOperator.BFR_MessageStatus == BRMessageStatusList.Codes.AwaitingResponse)
				{
					propertyInfo.AddMessageError(Res.GetString("61B11C73-4510-4320-9148-2813954756A5", "Manufacturer choose is a Foreign Operator but there are pending messages (Message Status is currently AWA - Awaiting Response)."));
				}
			}
		}

		protected override bool IsGoodsApplicationMandatory => true;

		protected override void CheckJI_GoodsCondition()
		{
			base.CheckJI_GoodsCondition();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_GoodsConditionInfo);
		}

		protected override void CheckJI_ManufacturerAuthorityIdentifier()
		{
			base.CheckJI_ManufacturerAuthorityIdentifier();
			
			if (Parent.IsAttachedToPersistentDeclaration && Parent.JI_ManufacturerIndicator != ManufacturerIndicatorList.Codes._3)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ManufacturerAuthorityIdentifierInfo);
			}
		}

		protected override void CheckJI_ManufacturerAuthorityVersion()
		{
			base.CheckJI_ManufacturerAuthorityVersion();
			if (Parent.IsAttachedToPersistentDeclaration && Parent.JI_ManufacturerIndicator != ManufacturerIndicatorList.Codes._3)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ManufacturerAuthorityVersionInfo);
			}
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			if (!Parent.JI_Tariff.IsEmpty && Parent.GoodsCatalog is CusGoodsCatalog goodsCatalog && goodsCatalog.CGC_Tariff != Parent.JI_Tariff)
			{
				Parent.JI_TariffInfo.AddMessageError(Res.GetString("40A3E141-0846-4C53-9095-3403B88E60C3", "Tariff does not match with the catalog file."));
			}
		}
	}
}
