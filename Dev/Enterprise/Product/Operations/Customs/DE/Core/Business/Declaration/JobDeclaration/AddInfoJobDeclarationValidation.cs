using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public partial class JobDeclarationValidation
	{
		JobDeclaration Declaration => Parent;

		protected override void CheckJE_VATDeferType()
		{
			base.CheckJE_VATDeferType();

			var declaration = Declaration;
			if (declaration.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_VATDeferTypeInfo, declaration.Lookups.PaymentPartyList);

				if (declaration.JE_PaymentMethod.IsEmpty && MethodOfPaymentHelper.RequireDeferralPaymentParty(declaration.ZG_MethodOfPayment))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VATDeferTypeInfo);
				}
			}
		}

		protected override void CheckJE_VATDeferNumber()
		{
			var declaration = Declaration;
			base.CheckJE_VATDeferNumber();
			if (declaration.IsImport && !Parent.JE_VATDeferType.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_VATDeferNumberInfo, declaration.Lookups.VATAccountNumberList);
			}
		}

		protected override void CheckJE_AgreedPlaceCode()
		{
			var declaration = Declaration;
			if (!declaration.IsImport && !declaration.IsWarehouseAdjustment)
			{
				base.CheckJE_AgreedPlaceCode();
			}
		}

		protected override void CheckJE_BorderTransportMeans()
		{
			var declaration = Declaration;
			if (!declaration.IsWarehouseAdjustment && !declaration.IsStockMovement && !declaration.IsInwardProcessingAVABR)
			{
				base.CheckJE_BorderTransportMeans();
				var targetInfo = Parent.JE_BorderTransportMeansInfo;
				var transportMode = declaration.JE_TransportMode;
				if (declaration.IsImport || (declaration.IsExport && !transportMode.IsEmpty && transportMode != TransportTypeList.Codes.Rail && transportMode != TransportTypeList.Codes.Mail && transportMode != TransportTypeList.Codes.FixedTransportInstallations))
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
		}

		protected override void CheckJE_StatisticsGoodsStatus()
		{
			var declaration = Declaration;
			if (declaration.IsImport)
			{
				base.CheckJE_StatisticsGoodsStatus();
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_StatisticsGoodsStatusInfo, declaration.Lookups.StatisticStatusCodeList);
			}
		}

		protected override void CheckJE_SpecificCircumstanceIndicator()
		{
			var declaration = Declaration;
			if (!declaration.IsImport && !declaration.IsWarehouseAdjustment)
			{
				base.CheckJE_SpecificCircumstanceIndicator();
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_SpecificCircumstanceIndicatorInfo, declaration.AddInfoLookups.SpecificCircumstanceIndicatorList);
			}
		}

		protected override void CheckJE_Box18TransportID()
		{
			var declaration = Declaration;
			if (declaration.IsImportAndNotStockMovement && !declaration.IsInwardProcessingAVABR)
			{
				base.CheckJE_Box18TransportID();

				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_Box18TransportIDInfo);
			}
		}

		protected override void CheckJE_MethodOfPayment()
		{
			base.CheckJE_MethodOfPayment();

			var declaration = Declaration;
			var entryInstructions = declaration.CustomsEntryInstructions;
			if (declaration.IsImport && entryInstructions.Any() && entryInstructions.All(x => x.CEI_Style == ImportDeclarationTypeList.Codes.EZA))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_MethodOfPaymentInfo);
			}
		}

		protected override void CheckJE_IsHighValueOvrd()
		{
			base.CheckJE_IsHighValueOvrd();
			if (!Parent.JE_IsHighValueOvrd)
			{
				var declaration = Declaration;
				if (declaration.IsImportAndNotStockMovement && !HasAllInstructionsWithCEI_StyleEAV(declaration) && !declaration.IsInwardProcessingAVABR)
				{
					var customsDeclarationValue = declaration.Factory.GetCustomsDeclarationValue();
					if (customsDeclarationValue > 0)
					{
						var sumOfCustomsValueOfAllInvoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_CustomsValue);
						if (sumOfCustomsValueOfAllInvoiceLines >= customsDeclarationValue)
						{
							Parent.JE_IsHighValueOvrdInfo.AddMessageError(Res.GetString("0195C1B6-A19B-4592-9718-CB9457F922D8"
								, "D.V.1 Flag must be set to true if the sum of all Customs Values on Inv. Lines >={0}€."
								, Utilities.FormatNumberNationalWithGroupSeparators((decimal)customsDeclarationValue, 2)));
						}
					}
				}
			}
		}

		bool HasAllInstructionsWithCEI_StyleEAV(JobDeclaration declaration)
		{
			var entryInstructions = declaration.CustomsEntryInstructions;
			return entryInstructions.Any() && entryInstructions.All(x => x.CEI_Style == ImportDeclarationTypeList.Codes.EAV);
		}
	}
}
