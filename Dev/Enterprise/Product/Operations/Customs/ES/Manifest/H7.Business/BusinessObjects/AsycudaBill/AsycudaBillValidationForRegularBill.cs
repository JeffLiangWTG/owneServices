using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class AsycudaBillValidationForRegularBill : EU.H7.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			AddMessageErrorForMissingSupportingDocuments();
		}

		void AddMessageErrorForMissingSupportingDocuments()
		{
			if (!Parent.SupportingDocuments.Any())
			{
				SupportingDocumentValidationHelper.CheckRequiredTypesForProcedureC07((AsycudaBill)Parent, Parent.AddRowMessageError);
				SupportingDocumentValidationHelper.CheckIfType1018IsRequired((AsycudaBill)Parent, Parent.AddRowMessageError);
			}
		}

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BillNumberInfo);

			if (!Parent.ABL_BillNumber.IsEmpty
				&& Parent.ABL_BillNumber.ToString().Any(c => char.IsLower(c) || !char.IsLetterOrDigit(c)))
			{
				Parent.ABL_BillNumberInfo.AddWarning(InvalidBillNumberWarningMessage);
			}
		}

		protected override void CheckABL_Procedure()
		{
			base.CheckABL_Procedure();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ProcedureInfo);
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();

			if (Parent.Consignee != null && !AdditionalProcedures.Contains(Parent.ABL_Procedure))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeRegNoInfo, Res.GetString("b2417a31-f924-4a05-baa0-ffe37b8bf5a8", "Importer Identification No"));
			}

			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.ABL_ConsigneeRegNoInfo, Parent.ABL_ConsigneeRegNoTypeInfo,
				Res.GetString("e73f2998-dd43-41cb-9e37-36aa1aa95e12", "You have not entered an Importer Identification No."));
		}

		protected override void CheckABL_ConsigneeRegNoType()
		{
			base.CheckABL_ConsigneeRegNoType();
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.ABL_ConsigneeRegNoTypeInfo, Parent.ABL_ConsigneeRegNoInfo, Res.GetString("46964a4b-0e48-4613-b926-c8f73d0f3e00", "You have not entered an Importer ID No. Type."));

			if (Parent.Consignee != null)
			{
				if (!AdditionalProcedures.Contains(Parent.ABL_Procedure)
					&& Parent.ABL_ConsigneeRegNoType != ESH7ImporterIdentificationTypes.Codes.EOR
					&& Parent.ABL_ConsigneeRegNoType != ESH7ImporterIdentificationTypes.Codes.NIF)
				{
					Parent.ABL_ConsigneeRegNoTypeInfo.AddMessageError(Res.GetString("4e7d5e0c-c327-4ce7-9445-01d07318945a", "Importer ID No. Type must be ‘EOR’ or ‘NIF’."));
				}
			}
		}

		protected override void CheckABL_RN_NKConsigneeCountry()
		{
			base.CheckABL_RN_NKConsigneeCountry();

			if (Parent.ABL_Procedure != ESH7AdditionalProcedureCodeList.Codes.C07F48 &&
				Parent.ABL_RN_NKConsigneeCountry != Constants.CountryCodes.Spain)
			{
				Parent.ABL_RN_NKConsigneeCountryInfo.AddMessageError(Res.GetString("50d12136-a756-4d46-8938-abb5290b77ac", "Importer Country/Region must be ‘ES’."));
			}
			else if (Parent.ABL_Procedure == ESH7AdditionalProcedureCodeList.Codes.C07F48 &&
				!Constants.CountryCodes.IsInEuropeanCustomsUnion(Parent.ABL_RN_NKConsigneeCountry))
			{
				Parent.ABL_RN_NKConsigneeCountryInfo.AddMessageError(Res.GetString("e9dcf261-1a12-4e73-9a9c-ec434aff60b9", "Importer Country/Region must be an EU Country Code."));
			}
		}

		protected override void CheckABL_ConsigneePhone()
		{
			base.CheckABL_ConsigneePhone();

			if (Parent.ABL_ConsigneeEmail.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneePhoneInfo, ImporterPhoneOrEmailDescription);
			}
		}

		protected override void CheckABL_ConsigneeEmail()
		{
			base.CheckABL_ConsigneeEmail();

			if (Parent.ABL_ConsigneePhone.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeEmailInfo, ImporterPhoneOrEmailDescription);
			}
		}

		static string ImporterPhoneOrEmailDescription => Res.GetString("ba87f5d7-5638-4cac-8467-2027019e3d4d", "Importer Phone Number and/or Importer Email Address");

		protected override void CheckABL_ShipperName()
		{
			base.CheckABL_ShipperName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperNameInfo, Res.GetString("9a270864-4674-4558-a377-52f728173731", "Exporter Name"));
		}

		protected override void CheckABL_ShipperStreet1()
		{
			base.CheckABL_ShipperLocalStreet1();

			if (Parent.ABL_ShipperStreet2.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperStreet1Info, ExporterStreetAddressDescription);
			}
		}

		protected override void CheckABL_ShipperStreet2()
		{
			base.CheckABL_ShipperLocalStreet2();

			if (Parent.ABL_ShipperStreet1.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperStreet2Info, ExporterStreetAddressDescription);
			}
		}

		static string ExporterStreetAddressDescription => Res.GetString("78adaa65-0ae6-4c06-89d1-b5a41b8f5b65", "Exporter Street Address");

		protected override void CheckABL_ShipperCity()
		{
			base.CheckABL_ShipperCity();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperCityInfo, Res.GetString("581fc78e-3458-4621-9d28-836432c7275c", "Exporter City"));
		}

		protected override void CheckABL_RN_NKShipperCountry()
		{
			base.CheckABL_RN_NKShipperCountry();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RN_NKShipperCountryInfo, Res.GetString("2eea0b67-426b-4c5a-9456-8e699c973400", "Exporter Country/Region"));
		}

		protected override void CheckABL_ShipperPostcode()
		{
			base.CheckABL_ShipperPostcode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperPostcodeInfo, Res.GetString("37097db2-b953-414b-be8b-0e2daa804910", "Exporter Postcode"));
		}

		protected override void CheckABL_GrossWeight()
		{
			base.CheckABL_GrossWeight();
			var packedItemsGrossWeight = 0m;

			foreach (var packedItem in Parent.PackedItems)
			{
				packedItemsGrossWeight += Weight.Convert(packedItem.API_GrossWeight, packedItem.API_GrossWeightUQ, Parent.ABL_GrossWeightUQ);
			}

			if (Parent.ABL_GrossWeight != packedItemsGrossWeight)
			{
				Parent.ABL_GrossWeightInfo.AddMessageError(Res.GetString(
					"1050db37-4ac0-4017-94a3-3714e7b5abcd",
					"The sum of gross weight {0} {1} in Items does not match with the total gross mass of the Bill.",
					packedItemsGrossWeight,
					Parent.ABL_GrossWeightUQ));
			}
		}

		IReadOnlyList<IZType> AdditionalProcedures =>
		[
			(ZString)ESH7AdditionalProcedureCodeList.Codes.C07F48,
			(ZString)ESH7AdditionalProcedureCodeList.Codes.C35,
			(ZString)ESH7AdditionalProcedureCodeList.Codes.C36
		];

		protected override string ValidationErrorMessageForABL_ConsigneeName => Res.GetString("68eaa53a-b675-44ce-92aa-038ad69010f6", "You have not entered an Importer Name.");

		protected override string ValidationErrorMessageForABL_ConsigneeStreet => Res.GetString("03560f96-fcb4-43a4-9638-3cbaed317e2e", "You have not entered an Importer Street Address.");

		protected override string ValidationErrorMessageForABL_ConsigneePostcode => Res.GetString("67ecadde-1e0f-4251-b9ff-6d2bdc4561d9", "You have not entered an Importer Postcode.");

		protected override string ValidationErrorMessageForABL_ConsigneeCity => Res.GetString("153869f3-8f56-43ec-8a0a-9847bacd7884", "You have not entered an Importer City.");

		protected override string ValidationErrorMessageForABL_RN_NKConsigneeCountry => Res.GetString("4e9a80d4-712d-4a66-87c6-f9201e5850e9", "You have not entered an Importer Country/Region.");

		protected string InvalidBillNumberWarningMessage => Res.GetString("810faabc-ff52-4754-ab9d-268a720da3fa", "When ES Customs processes G3 messages, non alphanumeric characters will be ignored, and lowercase letters will be accepted but will be converted to uppercase. Eg: Test / 00-1* will be converted and recorded as TEST001.");
	}
}
