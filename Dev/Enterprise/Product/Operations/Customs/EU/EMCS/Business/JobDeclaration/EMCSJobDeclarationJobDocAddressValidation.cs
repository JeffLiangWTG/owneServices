using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobDeclarationJobDocAddressValidation : JobDocAddressValidation
	{
		public EMCSJobDeclarationJobDocAddressValidation(AutoJobDocAddress parent, EMCSJobDeclaration declaration)
			: base(parent)
		{
			Declaration = declaration;
		}
		protected readonly EMCSJobDeclaration Declaration;

		#region CheckE2_OA_Address

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (!Parent.E2_AddressOverride && !Parent.OrganisationPK.IsEmpty)
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.CarrierAgent:
						if (CarrierAgentIsMandatory)
						{
							CheckE2_OA_AddressFields();
						}
						break;
					case DocAddressTypes.Codes.Transporter:
						CheckE2_OA_AddressFields();
						break;
				}
			}
		}

		void CheckE2_OA_AddressFields()
		{
			if (Parent.Address1.IsEmpty)
			{
				Parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("d35e8d17-2e02-49d7-8077-6b23f5c041c8", "Address 1 is Required"));
			}
			if (Parent.City.IsEmpty)
			{
				Parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("c017feeb-a1dc-4c03-90c5-673d27ff827d", "City is Required"));
			}
			if (Parent.Postcode.IsEmpty)
			{
				Parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("f75a8355-bb2b-4c83-b17d-ebda9d626c30", "Postcode is Required"));
			}
			if (Parent.CompanyName.IsEmpty)
			{
				Parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("cdc983fc-8cd7-4851-a3b5-2b0bd5bda62e", "Company Name is Required"));
			}
		}

		#endregion

		#region CheckOrganisationPK

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			if (!Parent.E2_AddressOverride)
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.CarrierAgent:
						ValidateCarrierAgent();
						break;
					case DocAddressTypes.Codes.DestinationWarehouse:
						ValidateDestinationWarehouse();
						break;
					case DocAddressTypes.Codes.DispatchWarehouse:
						ValidateDispatchWarehouse();
						break;
					case DocAddressTypes.Codes.Transporter:
						ValidateTransporter();
						break;
					case DocAddressTypes.Codes.GoodsOwner:
						ValidateGoodsOwner();
						break;
					case DocAddressTypes.Codes.SupplierDocumentaryAddress:
						ValidateConsignor();
						break;
					case DocAddressTypes.Codes.ImporterDocumentaryAddress:
						ValidateConsignee();
						break;
				}
			}
		}

		void ValidateCarrierAgent()
		{
			if (CarrierAgentIsMandatory)
			{
				MessageErrorIfNotEntered();
			}
		}

		bool CarrierAgentIsMandatory => Declaration.ZG_TransportArrangement != EMCSTransportArrangementList.Codes.Consignor && Declaration.ZG_TransportArrangement != EMCSTransportArrangementList.Codes.Consignee;

		void ValidateDestinationWarehouse()
		{
			if (IsDestinationWarehouseMandatory)
			{
				MessageErrorIfNotEntered();
			}

			if (Declaration.JE_MessageSubType != EMCSDestinationTypeList.Codes.DestinationDirectDelivery)
			{
				MessageErrorIfNoCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID);
			}
		}

		protected virtual ZBool IsDestinationWarehouseMandatory
		{
			get
			{
				var destinationType = Declaration.JE_MessageSubType;
				return destinationType == EMCSDestinationTypeList.Codes.DestinationTaxWarehouse
						|| destinationType == EMCSDestinationTypeList.Codes.DestinationTemporaryRegisteredConsignee
						|| destinationType == EMCSDestinationTypeList.Codes.DestinationDirectDelivery
						|| destinationType == EMCSDestinationTypeList.Codes.DestinationExemptedConsignee;
			}
		}

		void ValidateDispatchWarehouse()
		{
			if (Declaration.JE_MessageSubType == EMCSDestinationTypeList.Codes.DestinationTaxWarehouse)
			{
				MessageErrorIfNotEntered();
			}
			if (Declaration.ZG_SubmissionType != EMCSSubmissionTypeList.Codes.SubmissionForDutyPaidB2B)
			{
				MessageErrorIfNoCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID);
			}
		}

		void ValidateTransporter()
		{
			if (Declaration.ZG_GuarantorType.Contains(EMCSGuarantorTypeList.Codes.Transporter, StringComparison.OrdinalIgnoreCase))
			{
				MessageErrorIfNotEntered();
			}
		}

		void ValidateGoodsOwner()
		{
			if (Declaration.OwnerDocumentaryAddress_Enabled)
			{
				MessageErrorIfNotEntered();
			}
		}

		void ValidateConsignor()
		{
			MessageErrorIfNotEntered();
			MessageErrorIfNoCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber);
			ValidateAddress();
		}

		void ValidateConsignee()
		{
			var messageSubType = Declaration.JE_MessageSubType;
			if (messageSubType != EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown && Declaration.ZG_SubmissionType != EMCSSubmissionTypeList.Codes.SubmissionForExport)
			{
				MessageErrorIfNotEntered();
			}
			if (messageSubType == EMCSDestinationTypeList.Codes.DestinationTaxWarehouse
				|| messageSubType == EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee
				|| messageSubType == EMCSDestinationTypeList.Codes.DestinationTemporaryRegisteredConsignee
				|| messageSubType == EMCSDestinationTypeList.Codes.DestinationDirectDelivery)
			{
				MessageErrorIfNoCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber);
			}
			ValidateAddress();
		}

		void MessageErrorIfNotEntered()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo, DocAddressType);
		}

		void MessageErrorIfNoCusCode(ZString code)
		{
			var organisation = Parent.Organisation;
			if (organisation != null && organisation.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(code).Length == 0)
			{
				Parent.OrganisationPKInfo.AddMessageError(Res.GetString("62A9B4BE-BEED-481B-AE72-8D9057F26FF8", "{0} must have a Registration Number / Code of type '{1}'", DocAddressType, code));
			}
		}

		ZString DocAddressType
		{
			get
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.CarrierAgent:
						return Res.GetString("806CD52E-DDB6-496D-9F1F-672D83EC6681", "Carrier Agent");
					case DocAddressTypes.Codes.DestinationWarehouse:
						return Res.GetString("3D293DAD-D39D-4D68-B526-D900A1B0C640", "Destination Warehouse");
					case DocAddressTypes.Codes.DispatchWarehouse:
						return Res.GetString("DD02E48B-4894-4D34-91F4-C4372A2C38C7", "Dispatch Warehouse");
					case DocAddressTypes.Codes.Transporter:
						return Res.GetString("F6A95731-8066-4FA7-87D6-EB209D0D43C2", "Transporter");
					case DocAddressTypes.Codes.GoodsOwner:
						return Res.GetString("6360b9d2-73e1-4249-b074-ee5c45b22601", "Goods Owner");
					case DocAddressTypes.Codes.SupplierDocumentaryAddress:
						return Res.GetString("3d8fc769-20e3-4e87-8012-be1efd3fd689", "Consignor");
					case DocAddressTypes.Codes.ImporterDocumentaryAddress:
						return Res.GetString("d2a51f42-446f-4cad-ae74-03816b4e39b3", "Consignee");
					default:
						return ZString.Empty;
				}
			}
		}

		void ValidateAddress()
		{
			if (!Parent.E2_AddressOverride && Parent.Address is OrgAddress orgAddress)
			{
				var emptyFields = GetFieldsForNonEmptyCheck(orgAddress).Where(info => info.Value.IsEmpty);
				foreach (var field in emptyFields)
				{
					Parent.OrganisationPKInfo.AddMessageError(Res.GetString("0E10A84C-BEA3-42EB-A218-7F930D4F485A", "{0} must have a valid {1} filled in.", DocAddressType, field.HumanReadableName));
				}
			}
		}

		static ZPropertyInfo[] GetFieldsForNonEmptyCheck(OrgAddress orgAddress) => new[] { orgAddress.OA_PostCodeInfo, orgAddress.OA_CityInfo };

		#endregion

		protected override void CheckE2_GovRegNum()
		{
			base.CheckE2_GovRegNum();

			if (Parent.E2_AddressOverride && !Parent.E2_GovRegNumType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_GovRegNumInfo);
			}
		}

		protected override void CheckE2_GovRegNumType()
		{
			base.CheckE2_GovRegNumType();

			if (Parent.E2_AddressOverride && !Parent.E2_GovRegNum.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.E2_GovRegNumTypeInfo);
			}
		}
	}
}
