using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public partial class ImportJobDeclarationValidation : JobDeclarationValidation
	{
		public ImportJobDeclarationValidation(JobDeclaration parent) : base(parent)
		{
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			if (Parent.HasAnyDiffT2CAndT2lEntry)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ImporterInfo);
			}
		}

		protected override void CheckJE_RN_NKTransportNationality()
		{
			base.CheckJE_RN_NKTransportNationality();

			if (Parent.HasAnyDiffT2CAndT2lEntry && Parent.HasAnyDiffH2Entry && HasEntryHeaderValidationModeImportNoneOrPDI && Parent.JE_RN_NKTransportNationality.IsEmpty && Parent.IsTransportModeInList(ESConstants.TransportModeTypes.TransportModesForImportTransportIDAndNationality))
			{
				Parent.JE_RN_NKTransportNationalityInfo.AddMessageError(Res.GetString("3BAED3BB-4E43-4B49-BC98-E86BD641A3F9", "If transport is not Post/Mail, Rail Freight or Fixed Transport Installations, [21] Nationality must be filled."));
			}
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			base.CheckJE_RL_NKOrigin();
			if (Parent.HasAnyDiffT2CEntry)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKOriginInfo);
			}
		}

		protected override void CheckJE_LocationOfGoods()
		{
			base.CheckJE_LocationOfGoods();
			if (Parent.HasAnyDiffT2CAndT2lEntry)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationOfGoodsInfo);
			}
		}

		protected override void CheckJE_ShipmentIncoTermPlace()
		{
			base.CheckJE_ShipmentIncoTermPlace();
			if (Parent.HasAnyDiffT2CAndT2lEntry && Parent.HasAnyDiffH2Entry && HasEntryHeaderValidationModeImportNone && Parent.HasInvoiceWithEmptyIncoTermPlace)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ShipmentIncoTermPlaceInfo);
			}
		}

		bool HasEntryHeaderValidationModeImportNoneOrPDI => Parent.ActiveEntryHeaders.Count == 0 || Parent.ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.ValidationMode == ValidationModes.None || x.ValidationMode == ValidationModes.PDI);

		bool HasEntryHeaderValidationModeImportNone => Parent.ActiveEntryHeaders.Count == 0 || Parent.ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.ValidationMode == ValidationModes.None);
	}
}
