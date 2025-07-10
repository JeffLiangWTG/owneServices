using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageHeaderValidation : AsycudaManifestHeaderValidation
	{
		public TemporaryStorageHeaderValidation(TemporaryStorageHeader parent) : base(parent)
		{
		}

		protected new TemporaryStorageHeader Parent => (TemporaryStorageHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateLRN();
			ValidateCRN();
			ValidateMRN();
			ValidateTransportType();
			ValidateDeclarationDate();
			ValidatePlaceOfUnloading();
			ValidateAuthorizationNumber();
			ValidateAuthorizationType();
			ValidateAuthorizationOwner();
			ValidateGoodsLocationDescription();
			ValidateIsENSReuse();
			ValidatePresentationCustomsOffice();
		}

		public void ValidateTransportType()
		{
			ValidateCalculatedProperty(Parent.TransportTypeInfo);
		}

		protected override void CheckAMA_TransportMode()
		{
			var messageType = Parent.AMA_MessageType;
			if (messageType == PNTSMessageTypeList.Codes.CombinedTemporaryStorage || messageType == PNTSMessageTypeList.Codes.PreLodgedTempStorage)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_TransportModeInfo);
			}
		}

		protected virtual void CheckTransportType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.TransportTypeInfo);
		}

		public void ValidateDeclarationDate()
		{
			ValidateCalculatedProperty(Parent.DeclarationDateInfo);
		}

		protected virtual void CheckDeclarationDate()
		{
			var declarationDate = Parent.DeclarationDate;
			var declrationDateInfo = Parent.DeclarationDateInfo;
			var presentationDate = Parent.AMA_DateAtCustomsOffice;
			if (!presentationDate.IsEmpty && presentationDate.IsValid && !declarationDate.IsEmpty && declarationDate.IsValid)
			{
				if (declarationDate == presentationDate)
				{
					declrationDateInfo.AddMessageError(Res.GetString("d235afb9-7259-4da7-beca-35551a2ee86d", "Declaration Date must be different with Presentation Date."));
				}
				if (declarationDate.Date < presentationDate.AddHours(-24).Date)
				{
					declrationDateInfo.AddMessageError(Res.GetString("31ce06cf-0185-489b-a600-efea6d7e00f8", "Declaration Date can't be more than 24 hours prior to Presentation Date."));
				}
			}

			if (declarationDate > ZDateTime.Now)
			{
				declrationDateInfo.AddMessageError(Res.GetString("7bcfb248-97a7-4f09-9d7e-14fa714bceed", "Declaration Date can't be a future date."));
			}
		}

		public void ValidatePlaceOfUnloading()
		{
			ValidateCalculatedProperty(Parent.PlaceOfUnloadingInfo);
		}

		protected virtual void CheckPlaceOfUnloading()
		{
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.PlaceOfUnloadingInfo);
			CheckRule058(parent.PlaceOfUnloadingInfo);
		}

		protected override void CheckAMA_MessageType()
		{
			var parent = Parent;

			base.CheckAMA_MessageType();

			if (parent.ValidationDecider is ITemporaryStorageHeaderValidationDecider validationDecider
				&& validationDecider.IsMessageTypeCheckActive)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.AMA_MessageTypeInfo);
			}
		}

		protected override void CheckAMA_OA_Carrier()
		{
			var parent = Parent;
			var carrierInfo = parent.AMA_OA_CarrierInfo;

			if (parent.ValidationDecider is ITemporaryStorageHeaderValidationDecider validationDecider
				&& validationDecider.IsMessageTypeCheckActive
				&& !parent.IsTransfer
				&& !parent.IsDeconsolidation)
			{
				base.CheckAMA_OA_Carrier();
				if (parent.AMA_MessageType != PNTSMessageTypeList.Codes.PreLodgedTempStorage)
				{
					MandatoryValidation.MessageErrorIfNotEntered(carrierInfo);
				}

				if (!parent.AMA_OA_Carrier.IsEmpty)
				{
					CheckHasEORINumber(parent.Carrier, carrierInfo);
				}
			}
		}

		protected override void CheckAMA_OA_Declarant()
		{
			base.CheckAMA_OA_Declarant();
			var parent = Parent;
			var declarantInfo = parent.AMA_OA_DeclarantInfo;
			MandatoryValidation.MessageErrorIfNotEntered(declarantInfo);
			CheckDeclarantAndRepresentativeAreDifferent();
			if (!parent.AMA_OA_Declarant.IsEmpty)
			{
				CheckHasEORINumber(parent.Declarant, declarantInfo);
			}
		}

		protected virtual void CheckDeclarantAndRepresentativeAreDifferent()
		{
			var parent = Parent;
			if (parent.Declarant != null && parent.AMA_OA_Declarant == parent.AMA_OA_Representative)
			{
				parent.AMA_OA_DeclarantInfo.AddMessageError(DeclarantAndRepresentativeShouldBeDifferentErrorMessage);
			}
		}

		public static string DeclarantAndRepresentativeShouldBeDifferentErrorMessage => Res.GetString("c9a6a2d5-192a-4336-8fb0-850356a98661", "Declarant must not be the same as the Representative.");

		protected override void CheckAMA_OA_Presenter()
		{
			base.CheckAMA_OA_Presenter();

			var parent = Parent;
			if (!parent.IsDeconsolidation)
			{
				if (parent.Presenter is OrgAddress presenter)
				{
					CheckHasEORINumber(presenter, parent.AMA_OA_PresenterInfo);
				}
				if (parent.AMA_MessageType == PNTSMessageTypeList.Codes.PresentationNotification || parent.AMA_MessageType == PNTSMessageTypeList.Codes.CombinedTemporaryStorage)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.AMA_OA_PresenterInfo);
				}
			}
		}

		protected override void CheckAMA_OA_Representative()
		{
			base.CheckAMA_OA_Representative();
			var parent = Parent;
			CheckRepresentativeAndDeclarantAreDifferent();
			if (!parent.AMA_OA_Representative.IsEmpty)
			{
				CheckHasEORINumber(parent.Representative, parent.AMA_OA_RepresentativeInfo);
			}
		}

		protected virtual void CheckRepresentativeAndDeclarantAreDifferent()
		{
			var parent = Parent;
			if (parent.Representative != null && parent.AMA_OA_Representative == parent.AMA_OA_Declarant)
			{
				parent.AMA_OA_RepresentativeInfo.AddMessageError(RepresentativeAndDeclarantShouldBeDifferentErrorMessage);
			}
		}

		public static string RepresentativeAndDeclarantShouldBeDifferentErrorMessage => Res.GetString("6cd89ffa-c3d9-4c1c-9660-a9d0db754ce3", "Representative must not be the same as the Declarant.");

		protected override void CheckAMA_CustomsOffice()
		{
			base.CheckAMA_CustomsOffice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_CustomsOfficeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.AMA_CustomsOfficeInfo);
		}

		protected override void CheckAMA_DateAtCustomsOffice()
		{
			base.CheckAMA_DateAtCustomsOffice();
			var presentationDate = Parent.AMA_DateAtCustomsOffice;
			var presentationDateInfo = Parent.AMA_DateAtCustomsOfficeInfo;
			var messageType = Parent.AMA_MessageType;
			if (messageType == PNTSMessageTypeList.Codes.CombinedTemporaryStorage || messageType == PNTSMessageTypeList.Codes.PresentationNotification)
			{
				MandatoryValidation.MessageErrorIfNotEntered(presentationDateInfo);
			}

			if (!presentationDate.IsEmpty && presentationDate.IsInTheFutureDatePartOnly)
			{
				presentationDateInfo.AddMessageError(Res.GetString("2cde5805-4845-4972-9b29-f370df653f5f", "Goods Presentation Date can't be a future date."));
			}
		}

		public void ValidateLRN()
		{
			ValidateCalculatedProperty(Parent.LRNInfo);
		}

		public void ValidateCRN()
		{
			ValidateCalculatedProperty(Parent.CRNInfo);
		}

		public void ValidateMRN()
		{
			ValidateCalculatedProperty(Parent.MRNInfo);
		}

		public void ValidateGoodsLocationDescription()
		{
			ValidateCalculatedProperty(Parent.GoodsLocationDescriptionInfo);
		}

		protected void CheckLRN()
		{
			var parent = Parent;
			if (parent.Configuration.SupportLRNGeneration && parent.LRN.IsEmpty && parent.GetEORIForLRNGeneration().IsEmpty)
			{
				parent.LRNInfo.AddMessageError(Res.GetString("F6F77AA4-DA75-4005-ABD5-BF9C1596E474", "Company or branch must have an EORI number in order to generate the LRN number."));
			}
		}

		protected void CheckCRN()
		{
			var parent = Parent;
			var crnInfo = parent.CRNInfo;

			if (parent.ValidationDecider is ITemporaryStorageHeaderValidationDecider validationDecider
				&& validationDecider.IsPreLodgedStatusCheckActive
				&& parent.CustomsStatus == UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStoragePreLodged
				&& parent.PreLodgedDate.IsValid
				&& parent.PreLodgedDate.AddDays(30) < ZDateTime.Today)
			{
				crnInfo.AddMessageError(Res.GetString("74EA2E8D-4DCD-4834-83BD-6373A8F4D423", "Pre-Lodged status was granted over 30 days ago, so it\r\nis not allowed by customs to send another TSD message."));
			}

			if (parent.CustomsStatus == UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated)
			{
				CheckCRNAndMRNForTSA(parent, crnInfo);
			}
		}

		protected virtual void CheckMRN()
		{
			var parent = Parent;
			var mrnInfo = parent.MRNInfo;

			if (parent.CustomsStatus == UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated)
			{
				CheckCRNAndMRNForTSA(parent, mrnInfo);
			}
		}

		void CheckCRNAndMRNForTSA(TemporaryStorageHeader parent, ZPropertyInfo info)
		{
			if (parent.ValidationDecider is ITemporaryStorageHeaderValidationDecider validationDecider
				&& validationDecider.IsCheckCRNAndMRNForTSAActive
				&& parent.CRN.IsEmpty
				&& parent.MRN.IsEmpty)
			{
				info.AddMessageError(Res.GetString("D1B59F57-942F-40E5-B514-1FBACDB64E58", "Make sure CRN # Or MRN # exist."));
			}
		}

		protected virtual void CheckGoodsLocationDescription()
		{
			if (!Parent.IsDeconsolidation)
			{
				CusGoodsLocationValidationHelper.ValidateInnerGoodsLocation(Parent);

				CheckRule058(Parent.GoodsLocationDescriptionInfo);
			}
		}

		void CheckHasEORINumber(OrgAddress orgAddressExpectEORI, ZPropertyInfo infoToAddError)
		{
			if (!HasEORINumber(orgAddressExpectEORI))
			{
				infoToAddError.AddMessageError(Res.GetString("93be5fad-6916-4c72-82b2-d04aaa46dded", "An EORI # should exist for {0}.", infoToAddError.Description));
			}
		}

		protected virtual bool HasEORINumber(OrgAddress orgAddressExpectEORI)
			=> (orgAddressExpectEORI?.Header?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(EuropeanUnionSharedCodeTypes.Eori).FirstOrDefault()) != null;

		public void ValidateAuthorizationNumber()
		{
			ValidateCalculatedProperty(Parent.AuthorizationNumberInfo);
		}

		public void ValidateAuthorizationType()
		{
			ValidateCalculatedProperty(Parent.AuthorizationTypeInfo);
		}

		public void ValidateAuthorizationOwner()
		{
			ValidateCalculatedProperty(Parent.AuthorizationOwnerInfo);
		}

		protected virtual void CheckAuthorizationNumber()
		{
			var parent = Parent;

			if (parent.ValidationDecider is ITemporaryStorageHeaderValidationDecider validationDecider
				&& validationDecider.IsAuthorizationUsageCheckActive)
			{
				MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(parent.AuthorizationNumberInfo, parent.AuthorizationTypeInfo);

				var code = parent.AuthorizationType;
				var number = parent.AuthorizationNumber;
				var ownerGuid = parent.AuthorizationOwner;

				if (!code.IsEmpty && !number.IsEmpty && !ownerGuid.IsEmpty
					&& !parent.Lookups.AuthorizationNumberList.Cast<CusAuthorisationHeader>().Any(x => x.CPH_Type == code && x.CPH_Number == number && x.CPH_OH_PermitHolder == ownerGuid))
				{
					var ownerOH = parent.Factory.LoadFromUniqueKey<OrgHeader>(ZArchitecture.Schema.OrgHeaderSchema.PK, ownerGuid);
					parent.AuthorizationNumberInfo.AddMessageError(Res.GetString("10DD217F-AB0B-450D-8D98-8B419258D235", "Authorization number: {0} doesn't exist for Code: {1}, Owner: {2}", number, code, ownerOH?.OH_Code));
				}
			}
		}

		protected virtual void CheckAuthorizationType()
		{
			var parent = Parent;

			if (parent.ValidationDecider is ITemporaryStorageHeaderValidationDecider validationDecider
				&& validationDecider.IsAuthorizationUsageCheckActive)
			{
				if (!parent.AuthorizationType.IsEmpty && (parent.AuthorizationNumber.IsEmpty || parent.AuthorizationOwner.IsEmpty))
				{
					parent.AuthorizationTypeInfo.AddError(Res.GetString("8199E43E-32FE-4DC2-9820-CFAF5D2F579B", "Both Authorization number and owner should be served."));
				}
				ListValidation.ErrorIfInvalidCode(parent.AuthorizationTypeInfo);
			}
		}

		protected virtual void CheckAuthorizationOwner()
		{
			var parent = Parent;

			if (parent.ValidationDecider is ITemporaryStorageHeaderValidationDecider validationDecider
				&& validationDecider.IsAuthorizationUsageCheckActive)
			{
				ListValidation.ErrorIfInvalidPK(parent.AuthorizationOwnerInfo);
			}
		}

		public void ValidateIsENSReuse()
		{
			ValidateCalculatedProperty(Parent.IsENSReuseInfo);
		}

		protected void CheckIsENSReuse()
		{
			var parent = Parent;
			if (parent.IsENSReuse)
			{
				parent.IsENSReuseInfo.AddWarning(Res.GetString("2A3FCF68-07C1-43A9-A64E-4B75117D1468", "When ENS Re-use = True, only minimal data will be sent to customs including MRN and Bill numbers. Bill details and related items will not be sent."));

				if (parent.AMA_MessageType != PNTSMessageTypeList.Codes.PresentationNotification && !parent.PreviousDocuments.Any(p => p.CSI_Code == PreviousDocumentCodeList.Codes.N355) && (!parent.Bills.Any() || !parent.Bills.Cast<TemporaryStorageBill>().All(p => p.PreviousDocuments.Cast<TemporaryStoragePreviousDocument>().Any(x => x.CSI_Code == PreviousDocumentCodeList.Codes.N355))))
				{
					parent.IsENSReuseInfo.AddMessageError(Res.GetString("30FAAF8E-E4B5-4D58-B464-933F3D7DD7C3", "In Case ENS Re-use is enabled, there must be one previous document of type N355 for each of the bill or at Header level."));
				}
			}
		}

		public void ValidatePresentationCustomsOffice()
		{
			ValidateCalculatedProperty(Parent.PresentationCustomsOfficeInfo);
		}

		protected void CheckPresentationCustomsOffice()
		{
			CheckPresentationCustomsOffice_MandatoryValidation();
			ListValidation.MessageErrorIfInvalidCode(Parent.PresentationCustomsOfficeInfo);
		}

		protected virtual void CheckPresentationCustomsOffice_MandatoryValidation()
		{
			var parent = Parent;
			if (parent.AMA_MessageType == PNTSMessageTypeList.Codes.PresentationNotification || parent.AMA_MessageType == PNTSMessageTypeList.Codes.CombinedTemporaryStorage)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.PresentationCustomsOfficeInfo);
			}
		}

		void CheckRule058(ZPropertyInfo propertyInfo)
		{
			var parent = Parent;
			if (parent.Configuration.GetValidationDecider().IsRule058Active)
			{
				if (propertyInfo == parent.GoodsLocationDescriptionInfo)
				{
					if (parent.GoodsLocationDescription.IsEmpty)
					{
						if (parent.AMA_MessageType != PNTSMessageTypeList.Codes.PreLodgedTempStorage)
						{
							MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
						}
						else if (parent.PlaceOfUnloading.IsEmpty)
						{
							propertyInfo.AddMessageError(Rule058ErrorMessage);
						}
					}
				}
				else if (propertyInfo == parent.PlaceOfUnloadingInfo)
				{
					if (parent.PlaceOfUnloading.IsEmpty && parent.AMA_MessageType == PNTSMessageTypeList.Codes.PreLodgedTempStorage && parent.GoodsLocationDescription.IsEmpty)
					{
						propertyInfo.AddMessageError(Rule058ErrorMessage);
					}
				}
			}
		}

		public static string Rule058ErrorMessage => Res.GetString("DD5FA3F7-6332-4AAC-B21C-C1F60DBE06C0", "For a pre-lodged declaration you must enter Place of Unloading or Location Of Goods.");
	}
}
