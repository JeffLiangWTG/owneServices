using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalMovementHeaderValidation : NctsCommonMovementHeaderValidation
	{
		public NctsArrivalMovementHeaderValidation(NctsArrivalMovementHeader parent) : base(parent)
		{
		}

		new NctsArrivalMovementHeader Parent => (NctsArrivalMovementHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateGoodsLocationDescription();
			ValidateAuthorizationCode();
			ValidateAuthorizationNumber();
			ValidateAuthorizationOwner();
			ValidateDestinationCustomsOfficeCodeForArrival();
		}

		protected override void CheckBM_ArrivalDate()
		{
			if (ValidationEnabled)
			{
				base.CheckBM_ArrivalDate();
				if (ValidationDecider is INctsArrivalMovementHeaderPhase5ValidationDecider decider)
				{
					if (decider.IsRuleTR0022Active && !Parent.BM_ArrivalDate.IsEmpty && !Parent.BM_ArrivalDate.IsInThePast())
					{
						Parent.BM_ArrivalDateInfo.AddMessageError(NctsHeader.Configuration.ValidationRuleConfiguration.Messages.TR0022Message);
					}

					if (decider.IsRuleTR0072Active)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_ArrivalDateInfo, messagePrefix: NctsHeader.Configuration.ValidationRuleConfiguration.Messages.TR0072RuleCode.GetRuleCodeMessagePrefix(true));
					}
				}
			}
		}

		protected override void CheckBM_PaperlessInbondNum()
		{
			if (ValidationEnabled)
			{
				base.CheckBM_PaperlessInbondNum();
				if (Parent.IsPhase5)
				{
					if (ValidationDecider is INctsArrivalMovementHeaderPhase5ValidationDecider decider)
					{
						if (Parent.BM_PaperlessInbondNum.Length <= 4 && decider.IsRuleTR0034Active)
						{
							Parent.BM_PaperlessInbondNumInfo.AddMessageError(Res.GetString("CE27221A-750F-4884-A0C5-730A88996A2A", "[TR0034] This field must have a value and the value must be longer than 4 characters."));
						}

						if (decider.IsRuleTR0098Active && Parent.Header?.Configuration.ValidationRuleConfiguration is ValidationRuleConfiguration configuration)
						{
							MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_PaperlessInbondNumInfo, messagePrefix: configuration.Messages.TR0098RuleCode.GetRuleCodeMessagePrefix(true));
						}
					}

					if (!Parent.BM_PaperlessInbondNum.IsWesternEuropeanOrEmpty)
					{
						EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.BM_PaperlessInbondNumInfo);
					}
				}
			}
		}

		protected override void CheckBM_GrossWeightUnloaded()
		{
			var parent = Parent;
			if (ValidationEnabled || !parent.BM_GrossWeightInfo.ReadOnly)
			{
				base.CheckBM_GrossWeightUnloaded();

				if (ValidationDecider is INctsArrivalMovementHeaderPhase5ValidationDecider decider
					&& decider.IsRuleTR0091Active
					&& Parent.BM_NoChangesToReport == false
					&& parent.BM_GrossWeightUnloaded < GetSumOfRelatedGrossWeights())
				{
					parent.BM_GrossWeightUnloadedInfo.AddMessageError(NctsHeader.Configuration.ValidationRuleConfiguration.Messages.TR0091Message);
				}

				ZDecimal GetSumOfRelatedGrossWeights() => parent.Header.Bills
					.Where(bill => bill.MovementDetail.B9_UnloadedState.In(new ZString[] { NctsUnloadedStateList.Codes.DIF, NctsUnloadedStateList.Codes.DEC, NctsUnloadedStateList.Codes.NEW }))
					.Aggregate(ZDecimal.Zero, (total, bill)
						=> total + (bill.MovementDetail.B9_UnloadedState == NctsUnloadedStateList.Codes.DIF ? bill.B0_GrossWeightUnloaded : bill.B0_Weight));
			}
		}

		public void ValidateGoodsLocationDescription()
		{
			ValidateCalculatedProperty(Parent.GoodsLocationDescriptionInfo);
		}

		protected void CheckGoodsLocationDescription()
		{
			if (ValidationEnabled)
			{
				CheckGoodsLocationDescriptionCore();
			}
		}

		protected virtual void CheckGoodsLocationDescriptionCore()
		{
			var movementHeader = Parent;
			if (movementHeader.IsPhase5)
			{
				CusGoodsLocationValidationHelper.ValidateInnerGoodsLocation(movementHeader);
			}
		}

		public void ValidateAuthorizationCode()
		{
			ValidateCalculatedProperty(Parent.AuthorizationCodeInfo);
		}

		protected virtual void CheckAuthorizationCode()
		{
			if (Parent.LoadAuthorization() is CusAuthorizationUsage authorization)
			{
				authorization.Validation.ValidateAGC_Code();
				Parent.AuthorizationCodeInfo.AddAllNotificationsFrom(authorization.AGC_CodeInfo);
			}
		}

		public void ValidateAuthorizationNumber()
		{
			ValidateCalculatedProperty(Parent.AuthorizationNumberInfo);
		}

		protected virtual void CheckAuthorizationNumber()
		{
			if (Parent.LoadAuthorization() is CusAuthorizationUsage authorization)
			{
				authorization.Validation.ValidateAGC_Number();
				Parent.AuthorizationNumberInfo.AddAllNotificationsFrom(authorization.AGC_NumberInfo);
			}
		}

		public void ValidateAuthorizationOwner()
		{
			ValidateCalculatedProperty(Parent.AuthorizationOwnerInfo);
		}

		protected virtual void CheckAuthorizationOwner()
		{
			if (Parent.LoadAuthorization() is CusAuthorizationUsage authorization)
			{
				authorization.Validation.ValidateAGC_OH_Owner();
				Parent.AuthorizationOwnerInfo.AddAllNotificationsFrom(authorization.AGC_OH_OwnerInfo);
			}
		}

		protected sealed override void CheckBM_DischargeType()
		{
			if (ValidationEnabled)
			{
				CheckBM_DischargeTypeCore();
			}
		}

		protected virtual void CheckBM_DischargeTypeCore()
		{
			var parent = Parent;
			if ((ValidationDecider is INctsArrivalMovementHeaderPhase5ValidationDecider decider && decider.IsRuleTR0042Active) && !totalPageNumberAndDischargeTIRAreBothEmptyOrSpecified)
			{
				parent.BM_DischargeTypeInfo.AddError(totalPageNumberAndDischargeTIRMustBeBothEmptyOrSpecifiedMessage);
			}
		}

		protected sealed override void CheckBM_CarnetTotalPages()
		{
			if (ValidationEnabled)
			{
				CheckBM_CarnetTotalPagesCore();
			}
		}

		protected virtual void CheckBM_CarnetTotalPagesCore()
		{
			var parent = Parent;
			if ((ValidationDecider is INctsArrivalMovementHeaderPhase5ValidationDecider decider && decider.IsRuleTR0042Active) && !totalPageNumberAndDischargeTIRAreBothEmptyOrSpecified)
			{
				parent.BM_CarnetTotalPagesInfo.AddError(totalPageNumberAndDischargeTIRMustBeBothEmptyOrSpecifiedMessage);
			}
		}

		protected override void CheckBM_StateOfSeals()
		{
			base.CheckBM_StateOfSeals();
			CheckRuleNR0009(Parent);
		}

		protected override void CheckBM_NoChangesToReport()
		{
			base.CheckBM_NoChangesToReport();
			CheckRuleNR0028();
			CheckRuleNR0026();
			CheckRuleTR0063();
		}

		void CheckRuleNR0026()
		{
			var parent = Parent;
			if (ValidationDecider is INctsArrivalMovementHeaderPhase5ValidationDecider { IsRuleNR0026Active: true } &&
				parent.BM_NoChangesToReport && !parent.BM_StateOfSeals.IsEmpty && !parent.BM_StateOfSealsBoolean)
			{
				parent.BM_NoChangesToReportInfo.AddMessageError(NctsHeader.Configuration.ValidationRuleConfiguration.Messages.NR0026Message);
			}
		}

		void CheckRuleNR0009(NctsArrivalMovementHeader parent)
		{
			if (!parent.BM_StateOfSealsBoolean &&
				(ValidationDecider is INctsArrivalMovementHeaderPhase5ValidationDecider decider && decider.IsRuleNR0009Active) &&
				parent.Header.ArrivalHeaderContainers.Count == 0)
			{
				parent.BM_StateOfSealsInfo.AddMessageError(Res.GetString("184fa216-0908-407e-9a4b-3ee7ba717a75", "[NR0009] At Least one Container/Equipment record is required."));
			}
		}

		void CheckRuleNR0028()
		{
			if (Parent.BM_StateOfSeals == YesNoList.Codes.No && Parent.BM_NoChangesToReport
				&& ValidationDecider is INctsArrivalMovementHeaderPhase5ValidationDecider decider && decider.IsRuleNR0028Active)
			{
				Parent.BM_NoChangesToReportInfo.AddMessageError(NctsHeader.Configuration.ValidationRuleConfiguration.Messages.NR0028Message);
			}
		}

		void CheckRuleTR0063()
		{
			if (!Parent.BM_NoChangesToReport
				&& Parent.Header is NctsHeader header
				&& (ValidationDecider is INctsArrivalMovementHeaderPhase5ValidationDecider decider && decider.IsRuleTR0063Active)
				&& (Parent.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted || Parent.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks)
				&& !header.IsAnyArrivalContainerSealDiscrepancy
				&& !header.IsAnyArrivalHouseConsignmentDiscrepancy
				&& !header.IsAnyArrivalGoodsItemDiscrepancy)
			{
				Parent.BM_NoChangesToReportInfo.AddMessageError(Parent.Header.Configuration.ValidationRuleConfiguration.Messages.TR0063Message);
			}
		}

		protected override void CheckBM_GONumber()
		{
			base.ValidateBM_GONumber();
			CheckRuleTR0071();
		}

		void CheckRuleTR0071()
		{
			var movementHeader = Parent;
			if (ValidationDecider is INctsArrivalMovementHeaderPhase5ValidationDecider { IsRuleTR0071Active: true }
				&& !movementHeader.AuthorizationNumber.IsEmpty && !movementHeader.IsSimplifiedNctsProcedure)
			{
				movementHeader.IsSimplifiedNctsProcedureInfo.AddMessageError(movementHeader.Header.Configuration.ValidationRuleConfiguration.Messages.TR0071Message);
			}
		}

		protected virtual bool ValidationEnabled => !(Parent.Header is NctsHeader header) || !header.IsArrivalDetailsReadOnly;

		bool totalPageNumberAndDischargeTIRAreBothEmptyOrSpecified => Parent is NctsArrivalMovementHeader parent && parent.BM_DischargeType.IsEmpty == parent.BM_CarnetTotalPages.IsEmpty;
		string totalPageNumberAndDischargeTIRMustBeBothEmptyOrSpecifiedMessage => Res.GetString("3B1AAFBC-6655-41CE-B79B-6A8DEB74B0B0", "[TR0042] Total Page Number and Discharge TIR must be filled, or both must be empty.");

		protected virtual void CheckDestinationCustomsOfficeCodeForArrivalCore()
		{
			ValidateCustomsOfficeCode(Parent.DestinationCustomsOfficeForArrival, Parent.DestinationCustomsOfficeCodeForArrivalInfo);

			if (Parent.Header is NctsHeader header)
			{
				if (header.Configuration.ValidationRuleConfiguration.IsRuleNR0010Active
					&& Parent.GoodsLocation is EU.Business.CusGoodsLocation goodsLocation
					&& goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier
					&& Parent.DestinationCustomsOfficeCodeForArrival != goodsLocation.CGL_CustomsOffice)
				{
					Parent.DestinationCustomsOfficeCodeForArrivalInfo.AddMessageError(Res.GetString("86C2C90B-DDBB-433B-AD97-CA37516DE1F0", "[NR0010] Destination Office must be equal to the Goods Location Customs Office."));
				}
				else if (header.IsPhase5 && Parent.DestinationCustomsOfficeForArrival is null)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.DestinationCustomsOfficeCodeForArrivalInfo);
				}
			}
		}
		public void ValidateDestinationCustomsOfficeCodeForArrival()
		{
			ValidateCalculatedProperty(Parent.DestinationCustomsOfficeCodeForArrivalInfo);
		}

		protected void CheckDestinationCustomsOfficeCodeForArrival()
		{
			if (!(Parent.Header is NctsHeader header) || !header.IsArrivalDetailsReadOnly)
			{
				CheckDestinationCustomsOfficeCodeForArrivalCore();
			}
		}

		protected override void CheckBM_UnloadingDate()
		{
			base.CheckBM_UnloadingDate();
			CheckRuleNR0076();

			void CheckRuleNR0076()
			{
				var movementHeader = Parent;
				var nctsHeader = movementHeader.Header;
				var unloadingDate = movementHeader.BM_UnloadingDate;
				if (ValidationDecider is INctsArrivalMovementHeaderPhase5ValidationDecider decider
					&& decider.IsRuleNR0076Active
					&& unloadingDate.IsValid
					&& unloadingDate.ToDateTime() < nctsHeader.MovementReferenceIssueDate)
				{
					movementHeader.BM_UnloadingDateInfo.AddWarning(movementHeader.Header.Configuration.ValidationRuleConfiguration.Messages.NR0076Message);
				}
			}
		}
	}
}
