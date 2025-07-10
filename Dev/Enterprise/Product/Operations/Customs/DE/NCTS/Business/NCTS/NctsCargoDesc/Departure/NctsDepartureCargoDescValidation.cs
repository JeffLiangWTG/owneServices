using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NctsDepartureCargoDescValidation : NctsDepartureCargoDescPhase5Validation
	{
		public NctsDepartureCargoDescValidation(NctsDepartureCargoDesc parent)
			: base(parent)
		{
		}

		protected override void ValidateAllSuspendListChanged()
		{
		}

		protected override void CheckBY_HarmonisedTariff()
		{
			base.CheckBY_HarmonisedTariff();

			var parent = Parent;
			var header = parent.Header;
			if (header != null)
			{
				if (header.MovementHeader.BM_InBondEntryType != NctsDeclarationTypeList.Codes.TIR
					|| ThisOrParentConsignmentHasN830PreDocument)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.BY_HarmonisedTariffInfo);
				}
			}

			if (!parent.BY_HarmonisedTariff.IsEmpty)
			{
				var loadingQuery = TariffViewCollection.GetLoadingQuery(parent.Factory, parent.DataGroupingCode, parent.TariffType, ZDateTime.Today);
				loadingQuery.AddToFilter(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, parent.BY_HarmonisedTariff);
				var tariffWithCodePrefixExists = parent.Factory.Exists(typeof(TariffView), loadingQuery);

				if (!tariffWithCodePrefixExists)
				{
					parent.BY_HarmonisedTariffInfo.AddMessageError(Res.GetString(
						"58849d7c-850a-456b-82fe-4a981e4ac316",
						"The Commodity Code you have entered is not valid for the current context."));
				}
			}
		}

		protected override void CheckBY_HarmonisedTariffLength()
		{
			var parent = Parent;
			var length = parent.BY_HarmonisedTariff.Length;
			var thisOrParentConsignmentHasN830PreDocument = ThisOrParentConsignmentHasN830PreDocument;
			var lengthIsValid = length == 8 || length == 11 || length == 0 || (length == 6 && !thisOrParentConsignmentHasN830PreDocument);
			if (!lengthIsValid)
			{
				var messageError = thisOrParentConsignmentHasN830PreDocument
					? Res.GetString("ec75d946-3e3c-4c24-b1f8-f5b1d585e311", "You have not entered a valid 8- or 11-digit Code.")
					: Res.GetString("c7a94376-342c-4a5c-b0a3-7402b29fcdcc", "You have not entered a valid 6-, 8- or 11-digit Code.");
				parent.BY_HarmonisedTariffInfo.AddMessageError(messageError);
			}
		}

		protected override void BY_HarmonisedTariffCharacterCheck()
		{
		}

		protected override void CheckBY_GrossWeightIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.BY_GrossWeightInfo, 14, 3);
		}

		protected override void CheckBY_NetWeightIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.BY_NetWeightInfo, 14, 3);
		}

		protected override void CheckBY_TransportChargesMethodOfPayment()
		{
			base.CheckBY_TransportChargesMethodOfPayment();
			ListValidation.MessageErrorIfInvalidCode(Parent.BY_TransportChargesMethodOfPaymentInfo,
				Parent.Lookups.TransportChargesModeOfPaymentList);
		}

		protected override void CheckBY_Type()
		{
			base.CheckBY_Type();

			var parent = Parent;
			if (parent.Header is NctsHeader header)
			{
				var targetInfo = parent.BY_TypeInfo;
				var movementDeclarationType = header.MovementHeader.BM_InBondEntryType;
				var cargoDeclarationType = parent.BY_Type;

				if (parent.BY_RN_NKCountryOfDestination == Constants.CountryCodes.SanMarino &&
					!movementDeclarationType.In(new ZString[]
					{
						NctsDeclarationTypeList.Codes.T2, NctsDeclarationTypeList.Codes.T2F,
					}) &&
					!cargoDeclarationType.In(new ZString[]
					{
						NctsDeclarationTypeList.Codes.T2, NctsDeclarationTypeList.Codes.T2F,
					}))
				{
					targetInfo.AddMessageError(Res.GetString("5950B494-CC4C-4393-8E4D-834FECC1ACEC",
						"Destination Country/Region San Marino requires a Declaration Type of 'T2' or 'T2F'."));
				}

				if (movementDeclarationType == NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
		}

		static bool HasPreviousDocument(ICusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> parent, string code)
		{
			return parent?.Any(doc => doc.CSI_Code == code) ?? false;
		}

		new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		bool ThisOrParentConsignmentHasN830PreDocument =>
			HasPreviousDocument(Parent.PreviousDocuments, NctsTypeOfPreviousDocument.Codes.N830) ||
			HasPreviousDocument(Parent.Bill?.PreviousDocuments, NctsTypeOfPreviousDocument.Codes.N830);
	}
}
