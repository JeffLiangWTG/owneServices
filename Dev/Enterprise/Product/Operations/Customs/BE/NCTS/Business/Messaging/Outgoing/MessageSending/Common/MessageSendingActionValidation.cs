using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class MessageSendingActionValidation : NctsHeaderMessageSendingObjectValidation
	{
		public MessageSendingActionValidation(MessageSendingAction parent) : base(parent)
		{
		}

		protected new MessageSendingAction Parent => (MessageSendingAction)base.Parent;

		public override Type AutoValidationType => typeof(MessageSendingActionValidation);

		public override void ValidateAll()
		{
			ValidateEntryType();
			ValidatePresentationDateTime();
			ValidateLRN();
			ValidateMRN();
			ValidateActualOfficeOfDestination();
			ValidateQueryInformation();
			ValidateRepresentativeCBRNumber();
		}

		protected override void CheckQueryInformation()
		{
			if (Parent.EntryType == NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement)
			{
				CheckQueryInformationRuleC0220();
			}
			else if (!Parent.QueryInformation.IsEmpty
				&& Parent.Consignee.IsEmpty
				&& Parent.ActualOfficeOfDestination.IsEmpty)
			{
				Parent.QueryInformationInfo.AddError(Res.GetString("A6658F2F-C074-442D-92E2-19209FC5AD54", "Actual Consignee or Actual Office of Destination must be filled if query information is filled."));
			}
		}

		protected override void CheckActualOfficeOfDestination()
		{
			if (Parent.EntryType == NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement)
			{
				CheckRuleTR0021(Parent.ActualOfficeOfDestinationInfo);
				CheckActualOfficeOfDestinationRuleC0315();
			}
			else
			{
				base.CheckActualOfficeOfDestination();
				MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.ActualOfficeOfDestinationInfo, Parent.TCI11Info);
			}
		}

		public void ValidateEntryType()
		{
			ValidateCalculatedProperty(Parent.EntryTypeInfo);
		}

		public void ValidatePresentationDateTime()
		{
			ValidateCalculatedProperty(Parent.PresentationDateTimeInfo);
		}

		protected void CheckEntryType()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.EntryTypeInfo);
			CheckAvailableBalance();
			CheckGoodsLocation();
			CheckEntryTypeRuleNR0053();
			CheckEntryTypeRuleNR0054();
		}

		void CheckRuleTR0021(ZPropertyInfo propertyInfo)
		{
			var parent = Parent;
			if (parent.ActualOfficeOfDestination.IsEmpty
				&& parent.ActualConsignee.OrganisationPK.IsEmpty
				&& !parent.QueryInformation.IsEmpty)
			{
				propertyInfo.AddError(NctsHeaderValidationHelper.TR0021ValidationMessage);
			}
		}

		void CheckQueryInformationRuleC0220()
		{
			if (!Parent.TCI11.IsEmpty && Parent.QueryInformation.IsEmpty)
			{
				Parent.QueryInformationInfo.AddError(Res.GetString("006D27A7-2505-4E5B-A44E-128F41905D93", "[C0220] If 'TC11 Delivery Date' is filled, then 'Query Information' must be filled."));
			}
		}

		void CheckActualOfficeOfDestinationRuleC0315()
		{
			if (!Parent.TCI11.IsEmpty && Parent.ActualOfficeOfDestination.IsEmpty)
			{
				Parent.ActualOfficeOfDestinationInfo.AddError(Res.GetString("169AA569-DB90-4AE3-BDB7-A0EE081B89A9", "[C0315] If 'TC11 Delivery date' is filled, then 'Actual Office of Destination' must be filled."));
			}
		}

		void CheckEntryTypeRuleNR0053()
		{
			var parent = Parent;
			var nctsHeader = parent.Header;

			if ((parent.EntryType == NctsMessageTypeList.Codes.Declaration || parent.EntryType == NctsMessageTypeList.Codes.Amendment) && nctsHeader.IsDepartureMovement)
			{
				if (nctsHeader.MovementHeader.GoodsLocation is CusGoodsLocation goodsLocation
					&& goodsLocation.Validation is CusGoodsLocationValidation goodsLocationValidation
					&& goodsLocationValidation.IsRuleNR0053Violated())
				{
					parent.EntryTypeInfo.AddError(Parent.Header?.Configuration.ValidationRuleConfiguration.Messages.NR0053Message);
				}
			}
		}

		void CheckEntryTypeRuleNR0054()
		{
			var parent = Parent;
			var nctsHeader = parent.Header;
			var isNctsPhase5Departure = nctsHeader.IsPhase5Departure;
			var configuration = nctsHeader.Configuration.ValidationRuleConfiguration;
			var isRuleNR0054Active = configuration.IsRuleNR0054Active;

			if (parent.EntryType == NctsMessageTypeList.Codes.PresentationNotification && isNctsPhase5Departure && isRuleNR0054Active)
			{
				var movementHeader = nctsHeader.MovementHeader;
				var goodsLocation = movementHeader.GoodsLocation;
				if (movementHeader.BM_AdditionalDeclarationType != NctsTypeOfAdditionalDeclarationList.Codes.D || !((goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier && goodsLocation.CGL_Type == CusGoodsLocationTypeList.Codes.DesignatedLocation) || (goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.UnLocode && goodsLocation.CGL_Type == CusGoodsLocationTypeList.Codes.ApprovedPlace)))
				{
					Parent.EntryTypeInfo.AddError(Res.GetString("69C2F237-2A93-4D90-8A83-6F80FB58D719", "[NR0054] The qualifier of the location must be U and its type C OR the qualifier must be V and its type A when sending a presentation notification."));
				}
			}
		}

		protected void CheckPresentationDateTime()
		{
			if (Parent.PresentationDateTime.IsInThePast() && !Parent.PresentationDateTimeInfo.ReadOnly)
			{
				Parent.PresentationDateTimeInfo.AddError(Res.GetString("9FB33BFF-9B1B-4CEB-B8DA-5F81518987A0", "Presentation Date cannot be in the past."));
			}
		}

		public void ValidateLRN()
		{
			ValidateCalculatedProperty(Parent.LRNInfo);
		}

		protected void CheckLRN()
		{
			if (Parent.Header.IsDepartureMovement && Parent.LRN.IsEmpty)
			{
				Parent.LRNInfo.AddError(Res.GetString("D94ADAFC-A850-48C0-91C0-0D9B8B87F94B", "Customer Reference (LRN) is mandatory. If 'Customer Reference' is not enabled, check if the logon company has an EORI number."));
			}
		}

		public void ValidateMRN()
		{
			ValidateCalculatedProperty(Parent.MRNInfo);
		}

		public void CheckAvailableBalance()
		{
			if (Parent.ShouldSend && Parent.EntryType.In<ZString>(NctsMessageTypeList.Codes.Declaration, NctsMessageTypeList.Codes.Amendment))
			{
				var nctsHeader = Parent.Header;

				foreach (NctsGuarantee nctsGuarantee in nctsHeader.MovementHeader.Guarantees)
				{
					var cusGuarantee = nctsGuarantee.CusGuarantee;

					if (cusGuarantee != null)
					{
						var remainingBalance = cusGuarantee.CPH_Calc_TotalBalanceIncludingPending.Amount;
						var bondAmount = nctsGuarantee.PW_BondAmount;

						if (remainingBalance < bondAmount)
						{
							Parent.EntryTypeInfo.AddError(Res.GetString("B407BC0A-643E-4FA4-8FAF-D1A332B872D2", "The available guarantee balance is {0} but {1} is required.", Utilities.FormatNumberNationalWithGroupSeparators((decimal)remainingBalance, 2), Utilities.FormatNumberNationalWithGroupSeparators((decimal)bondAmount, 2)));
						}
					}
				}
			}
		}

		public void CheckGoodsLocation()
		{
			if (Parent.Header.IsPhase5Departure && Parent.EntryType == NctsMessageTypeList.Codes.PresentationNotification)
			{
				var cusGoodsLocation = Parent.Header.MovementHeader.GoodsLocation;
				if (cusGoodsLocation.CGL_Type.IsEmpty || cusGoodsLocation.CGL_Qualifier.IsEmpty || cusGoodsLocation.CGL_AdditionalIdentifier.IsEmpty)
				{
					Parent.EntryTypeInfo.AddError(Res.GetString("1AC2C28E-562C-4214-925C-B0C3363D541B", "You have not entered a complete Location of Goods. This is required when sending a Presentation message."));
				}
			}
		}

		protected void CheckMRN()
		{
			if (Parent.Header.IsArrivalMovement && Parent.MRN.IsEmpty)
			{
				Parent.MRNInfo.AddError(Res.GetString("5387D5CC-CC8A-488F-8284-CD6720876451", "MRN is mandatory."));
			}
		}

		public void ValidateRepresentativeCBRNumber()
		{
			ValidateCalculatedProperty(Parent.RepresentativeCBRNumberInfo);
		}

		protected void CheckRepresentativeCBRNumber()
		{
			if (Parent.Header.IsDepartureMovement && Parent.EntryType == NctsMessageTypeList.Codes.Declaration && Parent.HasRepresentative && Parent.RepresentativeCBRNumber.IsEmpty)
			{
				Parent.RepresentativeCBRNumberInfo.AddError(Res.GetString("124F16F2-91B9-40AE-A506-5743497A4D6A", "The representative of the declaration has no CBR number registered in the config tab of the organization (Master Data)"));
			}
		}
	}
}
