using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.Ncts.Common;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsBillValidation : CusInBondBillValidation
	{
		public NctsBillValidation(NctsBill parent)
			: base(parent)
		{
		}

		protected new NctsBill Parent => (NctsBill)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			ValidateFirstDepartureTransportMeansNationality();
			ValidateSecondDepartureTransportMeansNationality();
			ValidateThirdDepartureTransportMeansNationality();
			ValidateFirstDepartureTransportMeansID();
			ValidateSecondDepartureTransportMeansID();
			ValidateThirdDepartureTransportMeansID();
			ValidateTransportTypeAtDeparture();
			ValidateRuleR0364();
			ValidateRuleB1896();
			ValidateGrossMassRule_R0221();
			ValidateRuleB1877Extended();
			ValidateRuleNR0062();
			ValidateRuleNR0079();
			ValidateRuleTR0094();
		}

		internal void ValidateRuleNR0079()
		{
			var parent = Parent;

			if (parent.ValidationDecider is INctsBillDeparturePhase5ValidationDecider departurePhase5ValidationDecider
				&& departurePhase5ValidationDecider.IsRuleNR0079Active
				&& !Parent.IsInPhase5TransitionPeriod
				&& parent.PreviousDocuments.Count > 0)
			{
				var messageWarning = parent.Header.Configuration.ValidationRuleConfiguration.Messages.NR0079Message;
				parent.AddRowWarning(messageWarning);
			}
		}

		internal void ValidateRuleTR0094()
		{
			var parent = Parent;

			if (parent.ValidationDecider is INctsBillDeparturePhase5ValidationDecider departurePhase5ValidationDecider
				&& departurePhase5ValidationDecider.IsRuleTR0094Active && parent.GoodsItems.Count == 0)
			{
				var rulesMessages = parent.Header.Configuration.ValidationRuleConfiguration.Messages;
				parent.AddRowMessageError(rulesMessages.TR0094Message);
			}
		}

		internal void ValidateRuleNR0062()
		{
			var parent = Parent;

			if (parent.ValidationDecider is INctsBillArrivalPhase5ValidationDecider arrivalPhase5ValidationDecider
				&& arrivalPhase5ValidationDecider.IsRuleNR0062Active)
			{
				var arrivalGoodsItems = parent.ArrivalGoodsItems;
				if (arrivalGoodsItems.Any(x => x.Packages.Any(p => !p.B5_GrossWeight.IsEmpty)))
				{
					var totalFromGoodsItems = arrivalGoodsItems.Sum(x => x.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF ? x.UnloadedGoodsItem.GrossMassInKilograms : x.GrossMassInKilograms);
					var totalFromPackages = arrivalGoodsItems.Sum(x => x.Packages.Sum(p => p.B5_GrossWeight));

					if (totalFromGoodsItems != totalFromPackages)
					{
						var rulesMessages = parent.Header.Configuration.ValidationRuleConfiguration.Messages;
						parent.AddRowWarning(rulesMessages.NR0062Message(totalFromGoodsItems, totalFromPackages));
					}
				}
			}
		}

		void ValidateRuleB1896()
		{
			var parent = Parent;

			if (!(parent.Header is NctsHeader header)
				|| !(parent.ValidationDecider is INctsBillDeparturePhase5ValidationDecider departurePhase5ValidationDecider && departurePhase5ValidationDecider.IsRuleB1896Active)
				|| !(header.MovementHeader is NctsDepartureMovementHeader movementHeader)
				|| !Parent.IsInPhase5TransitionPeriod)
			{
				return;
			}

			if (movementHeader.IsSecurityTypeENTOrBTH
				&& movementHeader.BM_UniqueConsignmentReference.IsEmpty
				&& !movementHeader.IsTIRDeclaration
				&& parent.GoodsItems.All(p => p.BY_CommercialReferenceNumber.IsEmpty)
				&& parent.AdditionalDocuments.Cast<NctsBillAdditionalDocument>().All(p => p.CSI_SubType != AdditionalInfoSubTypeList.Codes.TransportDocument || p.CSI_Code.IsEmpty))
			{
				var errorMessage = Res.GetString("1C62F980-56C3-4033-9BB7-D241A993E793", "[B1896] Transport Document is required when Security is ENT or BTH, UCR is EMPTY and Declaration type is not 'TIR'.");
				parent.AddRowMessageError(errorMessage);
			}
		}

		bool IsDestinationCountryEmptyAtAllLevels()
		{
			return Parent.Header.MovementHeader.BM_RL_NKDestinationPort.IsEmpty && Parent.B0_RN_NKCountryOfDestination.IsEmpty && Parent.GoodsItems.Any(y => y.BY_RN_NKCountryOfDestination.IsEmpty);
		}

		bool IsDestinationCountryFilledAtHouseConsignmentsAndGoodsItemLevel()
		{
			return Parent.Header.MovementHeader.BM_RL_NKDestinationPort.IsEmpty && !Parent.B0_RN_NKCountryOfDestination.IsEmpty && Parent.GoodsItems.Any(y => !y.BY_RN_NKCountryOfDestination.IsEmpty);
		}

		protected override void CheckB0_RN_NKCountryOfDestination()
		{
			base.CheckB0_RN_NKCountryOfDestination();

			var parent = Parent;
			var header = parent.Header;

			ListValidation.MessageErrorIfInvalidCode(parent.B0_RN_NKCountryOfDestinationInfo);

			if (header.MovementHeader != null && ValidationDecider is INctsBillDeparturePhase5ValidationDecider decider && decider.IsRuleC0343_2Active
				&& (IsDestinationCountryEmptyAtAllLevels() || IsDestinationCountryFilledAtHouseConsignmentsAndGoodsItemLevel()))
			{
				parent.B0_RN_NKCountryOfDestinationInfo.AddMessageError(header.Configuration.ValidationRuleConfiguration.Messages.C0343_2Message);
			}
		}

		protected override void CheckB0_RN_NKCountryOfExport()
		{
			base.CheckB0_RN_NKCountryOfExport();

			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.B0_RN_NKCountryOfExportInfo);
			CheckRuleR0506ForAttribute(parent.B0_RN_NKCountryOfExport, parent.B0_RN_NKCountryOfExportInfo, x => x.B0_RN_NKCountryOfExport);
			CheckRuleC0909();
			CheckRuleE1301ForAttribute(parent.B0_RN_NKCountryOfExport, parent.B0_RN_NKCountryOfExportInfo);
		}

		void CheckRuleC0909()
		{
			var parent = Parent;
			if (!(parent.Header is NctsHeader header)
				|| !(ValidationDecider?.IsRuleC0909Active ?? false)
				|| !(header.MovementHeader is NctsDepartureMovementHeader moveHeader))
			{
				return;
			}

			if (parent.B0_RN_NKCountryOfExport.IsEmpty
				&& moveHeader.BM_RN_NKCountryOfDispatch.IsEmpty
				&& parent.GoodsItems.All(goodsItem => goodsItem.BY_RN_NKCountryOfDispatch.IsEmpty))
			{
				parent.B0_RN_NKCountryOfExportInfo.AddMessageError(NctsHeaderValidationHelper.C0909ValidationMessage);
			}
		}

		protected override void CheckB0_Weight()
		{
			base.CheckB0_Weight();

			var parent = Parent;
			if (!(parent.Header is NctsHeader header))
			{
				return;
			}

			if (header.IsArrivalMovement)
			{
				if (parent.MovementDetail is CusInBondMoveDetail moveDetail)
				{
					MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyHasValue(parent.B0_WeightInfo, moveDetail.B9_UnloadedStateInfo, (ZString)NctsUnloadedStateList.Codes.NEW);
				}
			}
			else
			{
				CheckB0_WeightErrorIfNotEntered(parent);
			}

			if (header.IsDepartureMovement)
			{
				CheckRuleTR0077(header, parent);
				CheckRuleR0983(header, parent);
				CheckRuleNR0078(header, parent);
			}

			CheckRuleR0983_1(header, parent);
		}

		void CheckRuleNR0078(NctsHeader header, NctsBill parent)
		{
			if (parent.ValidationDecider is INctsBillDeparturePhase5ValidationDecider departurePhase5ValidationDecider
				&& departurePhase5ValidationDecider.IsRuleNR0078Active)
			{
				var totalGrossWeightOfGoodsItemsInKg = parent.GoodsItems.Sum(x => x.GrossMassInKilograms);
				if (parent.GrossWeightInKilograms != totalGrossWeightOfGoodsItemsInKg)
				{
					parent.B0_WeightInfo.AddWarning(header.Configuration.ValidationRuleConfiguration.Messages.NR0078Message(totalGrossWeightOfGoodsItemsInKg));
				}
			}
		}

		void CheckRuleR0983(NctsHeader header, NctsBill parent)
		{
			if (parent.ValidationDecider is INctsBillDeparturePhase5ValidationDecider departurePhase5ValidationDecider
				&& departurePhase5ValidationDecider.IsRuleR0983Active
				&& IsGoodsItemsMassSumGreaterThanParent)
			{
				parent.B0_WeightInfo.AddMessageError(Res.GetString("b13cfac3-d55b-45e9-9fcc-5599abc0c612", "[R0983] Gross Weight for a House Consignment can't be less than sum of Gross Weights of its underlying Goods Items."));
			}
		}

		void CheckRuleR0983_1(NctsHeader header, NctsBill parent)
		{
			if (parent.ValidationDecider is INctsBillDeparturePhase5ValidationDecider { IsRuleR0983_1Active: true }
				&& GoodsItemsMassSumInKilograms != parent.GrossWeightInKilograms)
			{
				parent.B0_WeightInfo.AddWarning(Res.GetString("D20A08C4-B9AF-4ABE-B444-2A3ECCEACC94", "[R0983-1] Total Gross Weight on House Consignment should be equal or greater than the sum of Gross Weight of its underlying Goods Items ({0} kg)", GoodsItemsMassSumInKilograms.ToStringTrimZeros()));
			}
		}

		void CheckRuleTR0077(NctsHeader header, NctsBill parent)
		{
			if (ValidationDecider is INctsBillDeparturePhase5ValidationDecider { IsRuleTR0077Active: true }
				&& IsGoodsItemsMassSumGreaterThanParent
				&& parent.B0_Weight > 0)
			{
				parent.B0_WeightInfo.AddMessageError(header.Configuration.ValidationRuleConfiguration.Messages.TR0077Message);
			}
		}

		ZDecimal GoodsItemsMassSumInKilograms => Parent.GoodsItems.Sum(goodItem => goodItem.GrossMassInKilograms);

		bool IsGoodsItemsMassSumGreaterThanParent => GoodsItemsMassSumInKilograms > Parent.GrossWeightInKilograms;

		protected virtual void CheckB0_WeightErrorIfNotEntered(NctsBill parent)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.B0_WeightInfo);
		}

		protected override void CheckB0_WeightUQ()
		{
			base.CheckB0_WeightUQ();
			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.B0_WeightUQInfo);
		}

		protected override void CheckB0_ReferenceID()
		{
			base.CheckB0_ReferenceID();

			var parent = Parent;
			var referenceID = parent.B0_ReferenceID;
			var referenceIDInfo = parent.B0_ReferenceIDInfo;

			CheckRuleR0506ForAttribute(referenceID, referenceIDInfo, x => x.B0_ReferenceID);
			CheckRuleE1301ForAttribute(referenceID, referenceIDInfo);
		}

		protected override void CheckB0_TransportPaymentMethod()
		{
			base.CheckB0_TransportPaymentMethod();

			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.B0_TransportPaymentMethodInfo);
			CheckRuleR0506ForAttribute(parent.B0_TransportPaymentMethod, parent.B0_TransportPaymentMethodInfo, x => x.B0_TransportPaymentMethod);
		}

		protected override void CheckB0_RX_NKLinePriceCurrency()
		{
			base.CheckB0_RX_NKLinePriceCurrency();

			ListValidation.MessageErrorIfInvalidCode(Parent.B0_RX_NKLinePriceCurrencyInfo);
		}

		public void CheckRuleE1301ForAttribute(ZString attributeValue, ZPropertyInfo attributeInfo)
		{
			if (!(Parent.Header is NctsHeader header) || !(ValidationDecider is INctsBillDeparturePhase5ValidationDecider decider && decider.IsRuleE1301Active))
			{
				return;
			}

			if (Parent.IsInPhase5TransitionPeriod && !attributeValue.IsEmpty)
			{
				attributeInfo.AddMessageError(NctsBillValidationHelper.GetMessageErrorForRuleE1301(attributeInfo.HumanReadableName));
			}
		}

		void CheckRuleR0506ForAttribute(ZString attributeValue, ZPropertyInfo attributeInfo, Func<NctsBill, ZString> attributeProvider)
		{
			if (!(Parent.Header is NctsHeader header)
				|| !(ValidationDecider is INctsBillDeparturePhase5ValidationDecider decider && decider.IsRuleR0506Active))
			{
				return;
			}

			if (!attributeValue.IsEmpty && IsAttributeSameForAllBills())
			{
				attributeInfo.AddMessageError(NctsBillValidationHelper.GetMessageErrorR0506MustBeDifferent(attributeInfo.HumanReadableName));
			}

			bool IsAttributeSameForAllBills() => header.Bills.Count > 1 && header.Bills.AllSame(attributeProvider);
		}

		public void ValidateFirstDepartureTransportMeansID()
		{
			ValidateCalculatedProperty(Parent.FirstDepartureTransportMeansIDInfo);
		}

		protected virtual void CheckFirstDepartureTransportMeansID()
		{
			var parent = Parent;
			if (parent.Header is NctsHeader header && header.IsPhase5Departure)
			{
				ValidateRuleR0473();
				ValidateRuleR0474_1();
				ValidateRuleTR0057();
				ValidateRuleTR0059();
				CheckRuleN0002(parent.FirstDepartureTransportMeansIDInfo);
			}
		}

		void ValidateRuleR0473()
		{
			var parent = Parent;
			if (parent.Header is NctsHeader header)
			{
				new NctsPhase5RuleR0473Validation(header.MovementHeader)
					.CheckTransportAtDeparture(parent.TransportTypeAtDeparture, parent.FirstDepartureTransportMeansIDInfo);
			}
		}

		void ValidateRuleTR0057()
		{
			var parent = Parent;
			var transportTypeAtDeparture = parent.TransportTypeAtDeparture;

			if ((parent.ValidationDecider is INctsBillDeparturePhase5ValidationDecider { IsRuleTR0057Active: true })
				&& !parent.IsInPhase5TransitionPeriod
				&& ((!transportTypeAtDeparture.IsEmpty && ApplicableTR0057TransportModeAtDeparture(parent.InlandTransportModeAtDeparture))
					|| CheckSupportedTransportTypes(transportTypeAtDeparture)))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.FirstDepartureTransportMeansIDInfo, messagePrefix: ValidationRuleMessagePrefixes.TR0057);
			}
		}

		void ValidateRuleTR0059()
		{
			var parent = Parent;
			if (ValidationDecider is not INctsBillDeparturePhase5ValidationDecider decider
				|| !decider.IsRuleTR0059Active)
			{
				return;
			}

			var isRailTransport = parent.InlandTransportModeAtDeparture == ModeOfTransportList.Codes._2_RailTransport;
			var valueIsEmpty = parent.FirstDepartureTransportMeansID.IsEmpty;
			if (isRailTransport && valueIsEmpty && parent.TransportDepartureAdditionalWagonNumbers.Count > 0)
			{
				parent.FirstDepartureTransportMeansIDInfo.AddMessageError(Res.GetString("76446B23-5C60-44B3-B51E-6566DB2A3695", "[TR0059] Please capture first Transport ID in field Wagon No./Train No. before using grid Additional Wagon Numbers."));
			}
		}

		void ValidateRuleR0474_1()
		{
			var parent = Parent;
			var header = parent.Header;
			if (!parent.IsTransportDepartureReadOnly)
			{
				new NctsPhase5RuleR0474_1Validation(parent, header).ValidateTransportAtDeparture(parent.TransportAtDepartureInfo, ValidationDecider as INctsBillDeparturePhase5ValidationDecider);
			}
		}

		void ValidateRuleR0364()
		{
			if (Parent is NctsBill nctsBill
				&& nctsBill.ValidationDecider is INctsBillDeparturePhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleR0364Active
				&& !(validationDecider.IsRuleB1964Active && Parent.IsInPhase5TransitionPeriod)
			)
			{
				var nonBulkNonUnpackedMarksAndNumbersAnyEmpty = nctsBill.GoodsItems.SelectMany(x => x.Packages).Cast<NctsPackage>()
					.Where(package => !package.IsBulk && !package.IsUnpacked)
					.GroupBy(package => package.B5_MarksAndNumbers)
					.Select(packagesGroupedByMarksAndNumbers => new
					{
						MarksAndNumbers = packagesGroupedByMarksAndNumbers.Key,
						AllEmptyQuantity = packagesGroupedByMarksAndNumbers.All(package => package.B5_UnitCount.IsEmpty)
					})
					.Any(marksAndNumbersWithQuantity => marksAndNumbersWithQuantity.AllEmptyQuantity);
				if (nonBulkNonUnpackedMarksAndNumbersAnyEmpty)
				{
					nctsBill.AddRowMessageError(Res.GetString(
						"17069128-3DF4-41EB-BE59-C2B9E15DFF24",
						"[R0364] At least one consignment item must have Package line with Package Quantity greater than 0 for the same Shipping Marks when Package Type is not bulk or unpackaged."
					));
				}
			}
		}

		public void ValidateFirstDepartureTransportMeansNationality()
		{
			ValidateCalculatedProperty(Parent.FirstDepartureTransportMeansNationalityInfo);
		}

		protected virtual void CheckFirstDepartureTransportMeansNationality()
		{
			var parent = Parent;
			var targetInfo = parent.FirstDepartureTransportMeansNationalityInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);

			if (ValidationDecider is INctsBillDeparturePhase5ValidationDecider decider
				&& decider.IsRuleTR0058Active
				&& !Parent.IsInPhase5TransitionPeriod
				&& !parent.FirstDepartureTransportMeansID.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: ValidationRuleMessagePrefixes.TR0058);
			}

			CheckRuleN0002(targetInfo);
		}

		public void ValidateSecondDepartureTransportMeansID()
		{
			ValidateCalculatedProperty(Parent.SecondDepartureTransportMeansIDInfo);
		}

		protected virtual void CheckSecondDepartureTransportMeansID()
		{
			var parent = Parent;
			var targetInfo = parent.SecondDepartureTransportMeansIDInfo;
			var nctsHeader = parent.Header;

			if ((ValidationDecider is INctsBillDeparturePhase5ValidationDecider decider && decider.IsRuleR0474Active)
				&& !Parent.IsInPhase5TransitionPeriod
				&& !targetInfo.Value.IsEmpty
				&& CheckIfFirstTransportDetailsEmpty(parent))
			{
				targetInfo.AddMessageError(nctsHeader?.Configuration.ValidationRuleConfiguration.Messages.R0474Message);
			}

			CheckRuleN0002(targetInfo);
		}

		public void ValidateGrossMassRule_R0221()
		{
			var parent = Parent;

			if (parent.Header is NctsHeader header
				&& parent.ValidationDecider is INctsBillDeparturePhase5ValidationDecider { IsRuleR0221Active: true }
				&& !Parent.IsInPhase5TransitionPeriod
				&& parent.GoodsItems.Count > 0
				&& parent.GoodsItems.All(goodItem => goodItem.BY_GrossWeight.IsEmpty && GetPackageNumber(goodItem).IsEmpty))
			{
				parent.AddRowMessageError(Res.GetString("85ff138c-d926-45ab-9f9a-b2e512fa55e6", "[R0221] Gross Weight must be greater than 0 for at least one goods item."));
			}

			ZLong GetPackageNumber(NctsCommonCargoDesc goodItem) => goodItem.Packages.Cast<NctsPackage>().Sum(package => !package.IsBulk ? package.B5_UnitCount : ZLong.Zero);
		}

		public void ValidateSecondDepartureTransportMeansNationality()
		{
			ValidateCalculatedProperty(Parent.SecondDepartureTransportMeansNationalityInfo);
		}

		protected virtual void CheckSecondDepartureTransportMeansNationality()
		{
			var parent = Parent;
			var nctsHeader = parent.Header;
			var targetInfo = parent.SecondDepartureTransportMeansNationalityInfo;

			ListValidation.MessageErrorIfInvalidCode(targetInfo);

			if (ValidationDecider is INctsBillDeparturePhase5ValidationDecider decider)
			{
				if (decider.IsRuleTR0058Active
				&& !Parent.IsInPhase5TransitionPeriod
				&& !parent.SecondDepartureTransportMeansID.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: ValidationRuleMessagePrefixes.TR0058);
				}

				if (decider.IsRuleR0474Active
					&& !Parent.IsInPhase5TransitionPeriod
					&& !targetInfo.Value.IsEmpty
					&& CheckIfFirstTransportDetailsEmpty(parent))
				{
					targetInfo.AddMessageError(nctsHeader?.Configuration.ValidationRuleConfiguration.Messages.R0474Message);
				}
			}

			CheckRuleN0002(targetInfo);
		}

		public void ValidateThirdDepartureTransportMeansID()
		{
			ValidateCalculatedProperty(Parent.ThirdDepartureTransportMeansIDInfo);
		}

		protected virtual void CheckThirdDepartureTransportMeansID()
		{
			var parent = Parent;
			var targetInfo = parent.ThirdDepartureTransportMeansIDInfo;
			var nctsHeader = parent.Header;

			if ((ValidationDecider is INctsBillDeparturePhase5ValidationDecider decider && decider.IsRuleR0474Active)
				&& !Parent.IsInPhase5TransitionPeriod
				&& !targetInfo.Value.IsEmpty
				&& CheckIfFirstTransportDetailsEmpty(parent))
			{
				targetInfo.AddMessageError(nctsHeader?.Configuration.ValidationRuleConfiguration.Messages.R0474Message);
			}

			CheckRuleN0002(targetInfo);
		}

		public void ValidateThirdDepartureTransportMeansNationality()
		{
			ValidateCalculatedProperty(Parent.ThirdDepartureTransportMeansNationalityInfo);
		}

		protected virtual void CheckThirdDepartureTransportMeansNationality()
		{
			var parent = Parent;
			var targetInfo = parent.ThirdDepartureTransportMeansNationalityInfo;
			var nctsHeader = parent.Header;

			ListValidation.MessageErrorIfInvalidCode(targetInfo);

			if (ValidationDecider is INctsBillDeparturePhase5ValidationDecider decider)
			{
				if (decider.IsRuleTR0058Active
				&& !Parent.IsInPhase5TransitionPeriod
				&& !parent.ThirdDepartureTransportMeansID.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: ValidationRuleMessagePrefixes.TR0058);
				}

				if (decider.IsRuleR0474Active
					&& !Parent.IsInPhase5TransitionPeriod
					&& !targetInfo.Value.IsEmpty
					&& CheckIfFirstTransportDetailsEmpty(parent))
				{
					targetInfo.AddMessageError(nctsHeader?.Configuration.ValidationRuleConfiguration.Messages.R0474Message);
				}
			}

			CheckRuleN0002(targetInfo);
		}

		public void ValidateTransportTypeAtDeparture()
		{
			ValidateCalculatedProperty(Parent.TransportTypeAtDepartureInfo);
		}

		protected virtual void CheckTransportTypeAtDeparture()
		{
			var parent = Parent;
			var targetInfo = parent.TransportTypeAtDepartureInfo;

			ListValidation.ErrorIfInvalidCode(targetInfo);

			if ((parent.ValidationDecider is INctsBillDeparturePhase5ValidationDecider { IsRuleTR0078Active: true })
				&& !Parent.IsInPhase5TransitionPeriod
				&& targetInfo.Value.IsEmpty
				&& (!parent.FirstDepartureTransportMeansID.IsEmpty || !parent.FirstDepartureTransportMeansNationality.IsEmpty))
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: parent.Header.Configuration.ValidationRuleConfiguration.Messages.TR0078RuleCode.GetRuleCodeMessagePrefix(true));
			}

			CheckRuleN0002(targetInfo);
		}

		void ValidateRuleB1877Extended()
		{
			new NctsBillRuleB1877ExtendedValidation(Parent)
				.Validate();
		}

		static bool CheckIfFirstTransportDetailsEmpty(NctsBill parent)
		{
			return parent.TransportAtDeparture.IsEmpty
				&& parent.TransportTypeAtDeparture.IsEmpty
				&& parent.FirstDepartureTransportMeansNationality.IsEmpty;
		}

		bool CheckSupportedTransportTypes(string transportTypeAtDeparture)
		{
			switch (transportTypeAtDeparture)
			{
				case NctsTransportTypeOfIdList.Codes._10:
				case NctsTransportTypeOfIdList.Codes._11:
				case NctsTransportTypeOfIdList.Codes._20:
				case NctsTransportTypeOfIdList.Codes._21:
				case NctsTransportTypeOfIdList.Codes._30:
				case NctsTransportTypeOfIdList.Codes._40:
				case NctsTransportTypeOfIdList.Codes._41:
				case NctsTransportTypeOfIdList.Codes._80:
				case NctsTransportTypeOfIdList.Codes._81:
					return true;
				default:
					return false;
			}
		}

		bool ApplicableTR0057TransportModeAtDeparture(string transportModeAtDeparture)
		{
			return transportModeAtDeparture switch
			{
				ModeOfTransportList.Codes._7_FixedTransportInstallations => true,
				ModeOfTransportList.Codes._9_OwnPropulsion => true,
				_ => false
			};
		}

		void CheckRuleN0002(ZPropertyInfo targetPropertyInfo)
		{
			if (ValidationDecider is INctsBillDeparturePhase5ValidationDecider departurePhase5ValidationDecider
				&& departurePhase5ValidationDecider.IsRuleN0002Active
				&& Parent.Header?.MovementHeader is NctsDepartureMovementHeader movementHeader)
			{
				new NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation(movementHeader).ValidateRuleN0002(targetPropertyInfo);
			}
		}

		INctsBillPhase5ValidationDecider ValidationDecider => Parent.ValidationDecider as INctsBillPhase5ValidationDecider;
	}
}
