using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business.MessageBuilders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ExportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ExportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		#region CheckJI_CountryOfOrigin

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			if (Parent.InvoiceHeader != null && Parent.InvoiceHeader.JZ_RN_NKDefaultOrigin.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_CountryOfOriginInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_CountryOfOriginInfo);
			}
		}

		#endregion

		#region CheckJI_Tariff

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			new TariffValidator(Parent.Factory).Validate(Parent.JI_TariffInfo, !Parent.IsDataLoadingModule, Parent.IsDataLoadingModule ? 8 : 10, Parent.EffectiveDateForDutyRate, false);
		}

		#endregion

		#region CheckJI_Description

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo, Res.GetString("f9c2f70e-7ffc-4226-8bc4-8722a5844a7a", "Product Description"));
			if (Parent.Declaration.IsG7ExportDeclaration && Parent.JI_Description.Length > G7ExportMessageBuilder.G7DescriptionMaxLength)
			{
				Parent.JI_DescriptionInfo.AddWarning(Res.GetString("49086a64-b3a9-4952-83f5-cc8aaf0a35ad", "Product Description is too long. It will be truncated to {0} characters in the message sent to Customs.", G7ExportMessageBuilder.G7DescriptionMaxLength));
			}
		}

		#endregion

		#region CheckJI_CustomsQuantity

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			if (!Parent.CustomsUQ.IsEmpty || !Parent.JI_CustomsUnitQty.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.JI_CustomsQuantityInfo);
				MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_CustomsQuantityInfo);
			}
			if (!Parent.CA_ConveyanceIdentificationNumber.IsEmpty && Parent.JI_CustomsUnitQty == CustomsUnitOfMeasureList.Codes.Number)
			{
				if (Parent.CA_ConveyanceIdentificationNumber.Split(',', ' ').Length != Parent.JI_CustomsQuantity)
				{
					Parent.JI_CustomsQuantityInfo.AddMessageError(Res.GetString("5225C6C6-DAC6-4A16-8560-739A251C5E4E", "The number of Conveyance IDs entered should match the Customs Number Quantity."));
				}
			}
		}

		#endregion

		#region CheckJI_CustomsUnitQty

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();
			if (!Parent.CustomsUQ.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_CustomsUnitQtyInfo, Lookups.CustomsUQList);
			}
		}

		#endregion

		#region CheckJI_InvoiceQuantity

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();
			if (Parent.JI_CustomsUnitQty.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.JI_InvoiceQuantityInfo);
				MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_InvoiceQuantityInfo);
			}
		}

		#endregion

		#region CheckJI_InvoiceUQ

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			if (Parent.JI_CustomsUnitQty.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_InvoiceUQInfo);
			}
			if (!Parent.JI_InvoiceUQ.IsEmpty
				&& !Lookups.InvoiceUQList.ContainsCode(Parent.JI_InvoiceUQ)
				&& !Parent.Factory.GetCachedValue<CustomsUnitOfMeasureList>().ContainsCode(Parent.JI_InvoiceUQ))
			{
				Parent.JI_InvoiceUQInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
			}
		}

		#endregion

		#region CheckJI_StateOrRegionOfOrigin

		protected override void CheckJI_StateOrRegionOfOrigin()
		{
			base.CheckJI_StateOrRegionOfOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_StateOrRegionOfOriginInfo, Parent.Lookups.StateCodesList);
			if (Parent.EffectiveProvinceOfOrigin.IsEmpty)
			{
				var exporterAddress = Parent.Declaration.SupplierDocumentaryAddress;
				if (Parent.Declaration.IsG7ExportDeclaration && exporterAddress != null && exporterAddress.Country != null && exporterAddress.Country.RN_Code == Core.Constants.CountryCodes.Canada && !exporterAddress.E2_State.IsEmpty)
				{
					var warning = Res.GetString("B36A5ED4-BBD1-4520-BF08-D0AFBF517278", "You have not entered a province of Origin/Shipment so the province of the Exporter ({0}) will be sent in the G7 message.", exporterAddress.E2_State);
					Parent.JI_StateOrRegionOfOriginInfo.AddWarning(warning);
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_StateOrRegionOfOriginInfo, Res.GetString("24cb75f6-8769-438d-88ae-56a50ffb12ed", "Province of Origin/Shipment"));
				}
			}
		}

		#endregion
	}
}
