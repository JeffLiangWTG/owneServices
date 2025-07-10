using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportJobDeclarationValidation : JobDeclarationValidation
	{
		public ImportJobDeclarationValidation(JobDeclaration parent) : base(parent)
		{
		}

		protected override void CheckJE_GoodsOrigin()
		{
			if (!Parent.IsInwardProcessingAVABR)
			{
				base.CheckJE_GoodsOrigin();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_GoodsOriginInfo);
			}
		}

		protected override void CheckJE_GoodsDestination()
		{
			if (!Parent.IsInwardProcessingAVABR)
			{
				base.CheckJE_GoodsDestination();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_GoodsDestinationInfo);
			}
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			base.CheckJE_RL_NKFinalDestination();

			var parent = Parent;
			var isStatisticalStatus04 = parent.JE_StatisticStatus == StatisticStatusCodeList.Codes.C04;
			if (parent.CustomsEntryInstructions.Any(x =>
				x.CEI_Style == ImportDeclarationTypeList.Codes.EZA
				|| x.CEI_Style == ImportDeclarationTypeList.Codes.EAV
				|| (isStatisticalStatus04 && x.CEI_Style == ImportDeclarationTypeList.Codes.EZL)))
			{
				var destinationCode = parent.JE_RL_NKFinalDestination;
				var targetInfo = parent.JE_RL_NKFinalDestinationInfo;
				var propertyCaption = targetInfo.HumanReadableName;
				if (destinationCode.IsEmpty)
				{
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(propertyCaption));
				}
				else
				{
					var destination = new RefUNLOCO.Loader(parent.Factory).Load(destinationCode);
					if (destination != null && destination.RL_RW.IsEmpty && destination.RL_RN_NKCountryCode == Core.Constants.CountryCodes.Germany)
					{
						targetInfo.AddMessageError(Res.GetString("A209BD78-A7F6-4298-A2FE-5CF625B8098E", "You have not entered a State for this {0}.", propertyCaption));
					}
				}
			}
		}

		protected override void CheckJE_DefermentAccountNumber()
		{
			base.CheckJE_DefermentAccountNumber();
			if (!Parent.JE_PaymentMethod.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_DefermentAccountNumberInfo);
			}
		}

		protected override void CheckJE_OA_Representative()
		{
			base.CheckJE_OA_Representative();

			if (DeclarantTypeIsDIR)
			{
				var parent = Parent;
				var targetInfo = parent.JE_OA_RepresentativeInfo;
				var representativeOrgHeader = parent.Representative?.Header;
				if (representativeOrgHeader != null)
				{
					if (!representativeOrgHeader.HasEUEoriRegNo())
					{
						targetInfo.AddMessageError(Res.GetString("BBEA166F-AA49-4FFB-8788-3842B6705CAD", "Representative must have an EORI Number entered in Organizations Registration Numbers / Codes."));
					}

					if (parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_Procedure.StartsWith(CustomsProcedureCodeList.Import.ProcedureCode._42, StringComparison.OrdinalIgnoreCase) || x.JI_Procedure.StartsWith(CustomsProcedureCodeList.Import.ProcedureCode._63, StringComparison.OrdinalIgnoreCase)))
					{
						foreach (var codeType in new[] { GermanyOrgCusCodeInfo.OrgCusCodes.UST, GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice })
						{
							if (representativeOrgHeader.GetCustomsRegNo(codeType).IsEmpty)
							{
								targetInfo.AddMessageError(Res.GetString("62288BA7-EDE3-48F4-8AF5-6193E1BAA776", "Representative must have a Registration Number / Code of type '{0}' for the requested customs procedure.", codeType));
							}
						}
					}
				}
			}
		}

		protected override void CheckJE_OA_BuyingAgentAddress()
		{
			base.CheckJE_OA_BuyingAgentAddress();

			var parent = Parent;
			if (parent.JE_DeclarantType == RepresentationTypeList.Codes._3Indirect)
			{
				var targetInfo = parent.JE_OA_BuyingAgentAddressInfo;
				if (parent.JE_OA_BuyingAgentAddress.IsEmpty)
				{
					targetInfo.AddMessageError(Res.GetString("378955F5-AE1C-4FAE-B553-4F12254812DE", "Please enter a Represented Party for Rep. Type 'IND'."));
				}
			}
		}

		protected override void CheckJE_GoodsDescription()
		{
		}

		protected override void CheckJE_ShipmentIncoTermPlace()
		{
		}

		protected override void CheckJE_ShipmentIncoTerm()
		{
		}

		protected override void CheckJE_Folio()
		{
		}

		protected override void CheckJE_UseOwnerRefAsQuarantineRef()
		{
		}

		protected override void CheckJE_AgentsReference()
		{
		}

		protected override void CheckJE_RN_NKTransportNationality()
		{
			base.CheckJE_RN_NKTransportNationality();

			var parent = Parent;
			var mandatoryIfTransport = new List<string>() { TransportTypeList.Codes.Air, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Road, TransportTypeList.Codes.Sea };
			if (parent.JE_RN_NKTransportNationality.IsEmpty && mandatoryIfTransport.Contains(parent.JE_TransportMode))
			{
				parent.JE_RN_NKTransportNationalityInfo.AddMessageError(Res.GetString("1b8d2632-bc74-4904-bf66-851d83fa28f0", "Transport Nationality is mandatory."));
			}
		}

		protected override void CheckJE_VesselName()
		{
			var parent = Parent;
			var vessel = parent.JE_VesselName;
			var propertyInfo = parent.JE_VesselNameInfo;
			if (vessel.IsEmpty)
			{
				if (parent.ZG_BorderTransportMeans == ImportBorderTransportMeansList.Codes.Other)
				{
					propertyInfo.AddMessageError(Res.GetString("d4fcb491-7457-44f6-a241-e7ba408764d1", "Transport ID is mandatory."));
				}
			}
			else if (vessel.Length > 17)
			{
				propertyInfo.AddWarning(Res.GetString("BDAEA5DA-5E8F-42A2-840C-C101AE900BE6", "The maximum length for [21] Vessel is 17 characters."));
			}
		}

		protected override string MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantitiesCore
		{
			get { return Res.GetString("4B6AE2F8-17BD-4F3F-831E-848750B4C495", "Total Gross Weight on declaration must equal the sum of the Gross Weight of all lines."); }
		}

		protected override void CheckJE_OA_SellerAddress()
		{
			base.CheckJE_OA_SellerAddress();

			var parent = Parent;
			if (parent.ZG_IsHighValueOvrd)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_OA_SellerAddressInfo);
			}

			CheckTheAbsenceOfEoriNumberAndAddressDetail(parent.JE_OA_SellerAddressInfo, parent.SellerAddress, Res.GetString("1DBBBC77-55A0-4911-83A9-03ABEA98A96B", "Seller"));
		}

		void CheckTheAbsenceOfEoriNumberAndAddressDetail(ZPropertyInfo orgAddressPropertyInfo, OrgAddress orgAddress, ZString orgName)
		{
			if (orgAddress != null)
			{
				bool IsAddressFilled()
				{
					return !orgAddress.CompanyName.IsEmpty
							&& !orgAddress.OA_Address1.IsEmpty
							&& !orgAddress.OA_RN_NKCountryCode.IsEmpty
							&& !orgAddress.OA_City.IsEmpty
							&& (orgAddress.OA_RN_NKCountryCode != Core.Constants.CountryCodes.Germany || !orgAddress.OA_PostCode.IsEmpty);
				}

				bool MissingEoriNumber()
				{
					return !orgAddress.Header?.HasEUEoriRegNo() ?? true;
				}

				if (!IsAddressFilled() && MissingEoriNumber())
				{
					orgAddressPropertyInfo.AddMessageError(Res.GetString("BA1AADFF-3A12-4C0A-B706-14A54B650682", "{0} must have an EORI Number entered in Organizations Registration Numbers / Codes. Or {0} Address must have Company Name, Address 1, Country/Region and City entered and Post Code must be entered if Country/Region is Germany.", orgName));
				}
			}
		}

		protected override void CheckJE_OA_ConsigneeAddress()
		{
			base.CheckJE_OA_ConsigneeAddress();

			var parent = Parent;
			if (parent.ZG_IsHighValueOvrd)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_OA_ConsigneeAddressInfo);
			}

			CheckTheAbsenceOfEoriNumberAndAddressDetail(parent.JE_OA_ConsigneeAddressInfo, parent.ConsigneeAddress, Res.GetString("ED415541-1DBA-481F-A6AE-CF019A69D631", "Buyer"));
		}

		protected override void CheckJE_IATALoadPort()
		{
			base.CheckJE_IATALoadPort();

			var parent = Parent;
			if (parent.JE_IATALoadPort.IsEmpty
				&& parent.ZG_IsHighValueOvrd
				&& parent.IsAir)
			{
				parent.JE_IATALoadPortInfo.AddWarning(Res.GetString("D5469016-E34C-484C-8985-40D82536B22D", "For an automated split of the total air freight costs you should enter an IATA Code!"));
			}
		}

		protected override void CheckJE_PaymentMethod()
		{
			base.CheckJE_PaymentMethod();

			var parent = Parent;
			if (parent.ZG_VATDeferType.IsEmpty && MethodOfPaymentHelper.RequireDeferralPaymentParty(parent.ZG_MethodOfPayment))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_PaymentMethodInfo);
			}
		}

		protected override void CheckJE_OA_DeclarantAddress()
		{
			base.CheckJE_OA_DeclarantAddress();
			ValidateDeclarantForInvoiceLineSupportingDocsEndOfUseAuthorisation();

			var parent = Parent;
			var targetInfo = parent.JE_OA_DeclarantAddressInfo;
			var declarantOrgHeader = parent.DeclarantOrgAddress?.Header;
			if (declarantOrgHeader != null)
			{
				var declarantType = parent.JE_DeclarantType;
				if ((declarantType == RepresentationTypeList.Codes._1Self || declarantType == RepresentationTypeList.Codes._3Indirect) &&
					parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_Procedure.StartsWith(CustomsProcedureCodeList.Import.ProcedureCode._42, StringComparison.OrdinalIgnoreCase) || x.JI_Procedure.StartsWith(CustomsProcedureCodeList.Import.ProcedureCode._63, StringComparison.OrdinalIgnoreCase)))
				{
					foreach (var codeType in new[] { GermanyOrgCusCodeInfo.OrgCusCodes.UST, GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice })
					{
						if (declarantOrgHeader.GetCustomsRegNo(codeType).IsEmpty)
						{
							targetInfo.AddMessageError(Res.GetString("56FAE8D6-756B-42ED-9797-C1B487387F0A", "Declarant must have a Registration Number / Code of type '{0}' for the requested customs procedure.", codeType));
						}
					}
				}
			}
		}

		protected override void CheckJE_OA_DeclarantAddressIsNotEmpty()
		{
			MandatoryValidation.CheckEntered(Parent.JE_OA_DeclarantAddressInfo);
		}

		protected override void CheckJE_ContainerMode_Mandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ContainerModeInfo);
		}

		protected override void CheckJE_ContainerMode_ListValidation()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_ContainerModeInfo, Parent.Lookups.CargoIdTypeList);
		}

		protected override void CheckJE_RS_NKServiceLevel_Mandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RS_NKServiceLevelInfo);
		}

		protected override void CheckJE_RS_NKServiceLevel_ListValidation()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_RS_NKServiceLevelInfo, Parent.Lookups.ServiceLevels);
		}

		protected override void CheckJE_ApplicationCode()
		{
			base.CheckJE_ApplicationCode();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_ApplicationCodeInfo, Parent.Lookups.ApplicationCodeList);
		}

		protected override void CheckJE_OwnerRef()
		{
			var parent = Parent;
			if (!parent.IsInwardProcessingAVABR)
			{
				if (parent.JE_OwnerRef.IsEmpty)
				{
					parent.JE_OwnerRefInfo.AddWarning(Res.GetString("12fb4865-e418-4e97-b40b-4ed53886e114", "If Declarant's Reference is empty, Declaration Reference ({0}) will be determined as Local Reference Number and sent to Customs.", parent.JE_DeclarationReference));
				}
				else if (parent.JE_OwnerRef.Length > 22)
				{
					parent.JE_OwnerRefInfo.AddWarning(Res.GetString("71D10FE7-1FDC-47B1-8177-9155A9EE6CCB", "As the max. length for LRN is 22 characters, {0} will use the first 22 characters of Declarant's Reference to create the LRN.", BrandingFactory.Instance.ProductName));
				}
			}
		}

		protected override void CheckJE_RL_NKPortOfFirstArrival()
		{
			base.CheckJE_RL_NKPortOfFirstArrival();
			var parent = Parent;
			if (parent.ZG_IsHighValueOvrd)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_RL_NKPortOfFirstArrivalInfo);
			}
		}

		protected override void CheckJE_GS_NKCusAgent()
		{
			base.CheckJE_GS_NKCusAgent();
			CheckBrokerHasWorkPhone();
			CheckBrokerHasJobTitle();
		}

		protected override void CheckJE_TotalWeight()
		{
			if (!Parent.IsInwardProcessingAVABR)
			{
				base.CheckJE_TotalWeight();
			}
		}

		void ValidateDeclarantForInvoiceLineSupportingDocsEndOfUseAuthorisation()
		{
			if (Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine => invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Any(x => CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation.Contains(x.CSI_Code) && x.CSI_ReferenceNumber.IsEmpty)))
			{
				Parent.JE_OA_DeclarantAddressInfo.AddMessageError(CusAuthorizationHelper.DeclarantRequiresEndUserAuthorization);
			}
		}

		protected override void CheckJE_VATClaimBack()
		{
			base.CheckJE_VATClaimBack();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_VATClaimBackInfo, Parent.Lookups.VATClaimBackList);
		}
	}
}
