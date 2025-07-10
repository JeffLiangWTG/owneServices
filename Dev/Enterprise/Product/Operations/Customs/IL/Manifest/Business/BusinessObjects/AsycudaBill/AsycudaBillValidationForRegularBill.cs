using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent)
			: base(parent)
		{
		}

		protected override ZBool NeedsToCheckABL_ManifestQty => true;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateAdditionInfos();
			ValidateABL_Condition();
			ValidateAtLeastOnPackageIsRequired();
			ValidateAtLeastOnItemIsRequired();
			ValidateIL3TransportDocumentExistsWhenRoad();
			ValidatePackageFeeTypeExistsWhenRoad();
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected AsycudaManifestHeader Header => Parent.Header;

		#region ABL_Condition

		public void ValidateABL_Condition()
		{
			ValidateCalculatedProperty(Parent.ABL_ConditionInfo);
		}

		protected void CheckABL_Condition()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_ConditionInfo);
		}

		#endregion ABL_Condition

		protected override void CheckABL_SequenceNumber()
		{
			base.CheckABL_SequenceNumber();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_SequenceNumberInfo);

			if (Parent.ABL_SequenceNumber > AsycudaBill.Schema.ABL_SequenceNumberMaxValue)
			{
				Parent.ABL_SequenceNumberInfo.AddMessageError(ValidationCaptions.AsycudaBill.SequenceMaxLength);
			}
		}

		protected override void CheckABL_OA_Shipper()
		{
			base.CheckABL_OA_Shipper();

			var parent = Parent;
			MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_OA_ShipperInfo);
			ValidateVATNumberForPartner(Header.IsExport, parent.Shipper, parent.ABL_OA_ShipperInfo, ValidationCaptions.Partners.Consignor);
		}

		protected override void CheckABL_ShipperRegNo()
		{
			base.CheckABL_ShipperRegNo();

			var parent = Parent;
			var header = parent.Header;
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(parent.ABL_ShipperRegNoInfo, header.AMA_TransportModeInfo, (ZString)TransportModes.Road, ValidationCaptions.AsycudaBill.MandatoryConsigorRegNo);
		}

		protected override void CheckABL_OA_Consignee()
		{
			base.CheckABL_OA_Consignee();

			var parent = Parent;
			ValidateVATNumberForPartner(Header.IsImport, parent.Consignee, parent.ABL_OA_ConsigneeInfo, ValidationCaptions.Partners.Consignee);
		}

		protected override void CheckABL_RL_NKOrigin()
		{
			base.CheckABL_RL_NKOrigin();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RL_NKOriginInfo);
		}

		protected override void CheckABL_RL_NKPortOfDischarge()
		{
			base.CheckABL_RL_NKPortOfDischarge();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RL_NKPortOfDischargeInfo);
		}

		protected override void CheckABL_ManifestQty()
		{
			base.CheckABL_ManifestQty();

			if (Parent.ABL_ManifestQty > AsycudaBill.Schema.ABL_ManifestQtyMaxValue)
			{
				Parent.ABL_ManifestQtyInfo.AddMessageError(ValidationCaptions.AsycudaBill.QuantityMaxLength);
			}
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();

			var parent = Parent;
			if (parent.Header.IsImport && parent.ABL_ConsigneeRegNo.IsEmpty)
			{
				parent.ABL_ConsigneeRegNoInfo.AddMessageError(ValidationCaptions.AsycudaBill.TheVATNoShouldNotEmptyWhenManifestNatureIsImport);
			}
		}

		void ValidateVATNumberForPartner(bool expectedNature, OrgAddress partnerAddress, ZPropertyInfo propertyInfo, string partnerType)
		{
			if (expectedNature && partnerAddress != null)
			{
				var vatCustomsCode = partnerAddress.Header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, CountryCodes.Israel);
				if (vatCustomsCode?.OK_CustomsRegNo.IsEmpty ?? true)
				{
					propertyInfo.AddMessageError(ValidationCaptions.Manifest.VATNumberIsMissingForPartner(partnerType));
				}
			}
		}

		void ValidateAdditionInfos()
		{
			var parent = Parent;
			if (!parent.AdditionalInfos.Cast<AsycudaAdditionalInfo>()
				.Any(additionalInfo => additionalInfo.CSI_Code == Constants.AsycudaAdditionalInfoCodes.PartnerVatNumber && !additionalInfo.CSI_Description.IsEmpty))
			{
				parent.AddRowMessageError(ValidationCaptions.AsycudaBill.EachBillMustIncludeOneAdditionalInfoRecordWithStatementTypeIs2AndConsigneeVAT);
			}
		}

		void ValidateAtLeastOnItemIsRequired()
		{
			var parent = Parent;
			if (parent.PackedItems.Count == 0)
			{
				parent.AddRowMessageError(ValidationCaptions.AsycudaBill.AtLeastOnItemIsRequired);
			}
		}

		void ValidateAtLeastOnPackageIsRequired()
		{
			var parent = Parent;
			if (parent.Packs.Count == 0)
			{
				parent.AddRowMessageError(ValidationCaptions.AsycudaBill.AtLeastOnPackageIsRequired);
			}
		}

		void ValidateIL3TransportDocumentExistsWhenRoad()
		{
			var parent = Parent;
			var header = parent.Header;

			if (header.AMA_TransportMode == TransportModes.Road && parent.TransportDocuments.All(s => s.CSI_Code != TransportDocsTypeList.Codes.IL3))
			{
				parent.AddRowMessageError(ValidationCaptions.AsycudaBill.IL3ShouldExistWhenRoad);
			}
		}

		void ValidatePackageFeeTypeExistsWhenRoad()
		{
			var parent = Parent;
			var header = parent.Header;

			if (header.AMA_TransportMode == TransportModes.Road && parent.AdditionalInfos.All(s => s.CSI_Code != Constants.AsycudaAdditionalInfoCodes.PackageFeeType))
			{
				parent.AddRowMessageError(ValidationCaptions.AsycudaBill.PackageFeeTypeShouldExistWhenRoad);
			}
		}
	}
}
