using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPackagePhase5Validation : NctsPackageCommonValidation
	{
		public NctsPackagePhase5Validation(NctsPackage parent)
			: base(parent)
		{
		}

		protected override void CheckB5_GrossWeight()
		{
			base.CheckB5_GrossWeight();
			CheckRuleNR0061();
		}

		void CheckRuleNR0061()
		{
			var parent = Parent;

			if (parent.ValidationDecider is INctsPackageArrivalPhase5ValidationDecider arrivalPhase5ValidationDecider
				&& arrivalPhase5ValidationDecider.IsRuleNR0061Active
				&& parent.B5_B5_ParentPackage.IsEmpty
				&& parent.B5_GrossWeight.IsEmpty)
			{
				var warningMessage = parent.Parent.Header.Configuration.ValidationRuleConfiguration.Messages.NR0061Message;
				parent.B5_GrossWeightInfo.AddWarning(warningMessage);
			}
		}

		protected override void CheckB5_UnitCount()
		{
			base.CheckB5_UnitCount();

			CheckB5_UnitCount_TR0066Rule();
			var parent = Parent;
			if (parent.IsPhase5Departure)
			{
				CheckB5_UnitCount_C0060Rule();
				CheckB5_UnitCount_C0060_1Rule();
				CheckB5_UnitCount_R0364_1Rule();
				CheckB5_UnitCount_R0364_2Rule();
				CheckB5_UnitCount_R0364_3Rule();
				CheckB5_UnitCount_NR0003Rule();
				CheckB5_UnitCount_E1111Rule();
				CheckB5_UnitCount_NR0027Rule();
			}

			CheckRuleR0219(parent);
		}

		protected virtual ZLong B5_UnitCountMaxValue => 99999999;

		protected override void CheckB5_UnitType()
		{
			base.CheckB5_UnitType();
			var parent = Parent;

			var differenceType = parent.B5_TypeOfDifference;
			if (parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider departurePhase5ValidationDecider
				&& departurePhase5ValidationDecider.IsRuleTR0083Active
				&& (differenceType == NctsUnloadedStateList.Codes.DIF || differenceType == NctsUnloadedStateList.Codes.NEW))
			{
				var ruleCode = ValidationRuleConfiguration.Messages.TR0083RuleCode;
				MandatoryValidation.MessageErrorIfNotEntered(parent.B5_UnitTypeInfo, messagePrefix: ruleCode.GetRuleCodeMessagePrefix(true));
			}
			CheckRuleR0220(parent);

			if (parent.ValidationDecider is INctsPackageArrivalPhase5ValidationDecider { IsRuleNR0029Active: true }
				&& parent.B5_TypeOfDifference == NctsUnloadedStateList.Codes.NEW
				&& parent.B5_UnitType == string.Empty)
			{
				parent.B5_UnitTypeInfo.AddMessageError(ValidationRuleConfiguration.Messages.GetNR0029cMessage());
			}
		}

		protected override void CheckB5_TypeOfDifference()
		{
			base.CheckB5_TypeOfDifference();

			if (Parent.B5_B5_ParentPackage.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.B5_TypeOfDifferenceInfo);
			}
		}

		protected override void CheckB5_MarksAndNumbers()
		{
			base.CheckB5_MarksAndNumbers();

			var parent = Parent;
			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(parent.IsInPhase5TransitionPeriod, parent.B5_MarksAndNumbersInfo, 42);
		}

		protected override void CheckB5_UnitCount_Mandatory()
		{
			if (IsDepartureMovement)
			{
				if (!Parent.B5_UnitType.IsEmpty && Parent.B5_UnitCount.IsEmpty)
				{
					var isRuleC0060_2ActiveAndIsUnpacked = Parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleC0060_2Active: true } && Parent.IsUnpacked;
					if (isRuleC0060_2ActiveAndIsUnpacked)
					{
						var ruleCode = ValidationRuleConfiguration.Messages.C0060_2RuleCode;
						MandatoryValidation.MessageErrorIfNotEntered(Parent.B5_UnitCountInfo, messagePrefix: ruleCode.GetRuleCodeMessagePrefix(true));
					}
				}
			}
			else
			{
				base.CheckB5_UnitCount_Mandatory();
			}
		}

		protected virtual bool NeededZeroPackageError() => !Parent.IsBulk;

		protected override void CheckB5_MarksAndNumbers_Mandatory()
		{
			if (IsDepartureMovement)
			{
				if (!IsPluggedIntoShipment && ShouldCheckMandatoryMarksAndNumbers && Parent.B5_MarksAndNumbers.IsEmpty)
				{
					var isRuleC0060Active = Parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleC0060Active: true };
					var isRuleC0060_3Active = Parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleC0060_3Active: true };
					if (isRuleC0060Active || isRuleC0060_3Active)
					{
						var ruleCode = isRuleC0060Active ? ValidationRuleConfiguration.Messages.C0060RuleCode : ValidationRuleConfiguration.Messages.C0060_3RuleCode;
						MandatoryValidation.MessageErrorIfNotEntered(Parent.B5_MarksAndNumbersInfo, messagePrefix: ruleCode.GetRuleCodeMessagePrefix(true));
					}
				}

				if (IsPluggedIntoShipment && Parent.B5_MarksAndNumbers.IsEmpty)
				{
					Parent.B5_MarksAndNumbersInfo.AddMessageError(Res.GetString("EU.NCTS.PackageMarksAndNumbersRequiredWithinShipment", "Package marks are required. When this field is read only, it is not necessary to override the default values from the shipment to make this field editable. Instead package marks should be supplied on the shipment's packing tab."));
				}
			}
			else
			{
				if (Parent.ValidationDecider is INctsPackageArrivalPhase5ValidationDecider arrivalPhase5ValidationDecider && arrivalPhase5ValidationDecider.IsRuleTR0097Active)
				{
					base.CheckB5_MarksAndNumbers_Mandatory();
				}
			}
		}

		protected NctsCommonCargoDesc GoodsItem => Parent.Parent;

		void CheckB5_UnitCount_C0060Rule()
		{
			var parent = Parent;
			if (parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleC0060Active: true }
				&& !parent.B5_UnitType.IsEmpty
				&& !parent.IsBulk
				&& parent.B5_UnitCount.IsEmpty)
			{
				Parent.B5_UnitCountInfo.AddWarning(ValidationRuleConfiguration.Messages.C0060Message);
			}
		}

		void CheckB5_UnitCount_C0060_1Rule()
		{
			var parent = Parent;
			if (parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleC0060_1Active: true }
				&& parent.B5_UnitCount > 0
				&& parent.IsBulk)
			{
				Parent.B5_UnitCountInfo.AddMessageError(ValidationRuleConfiguration.Messages.C0060_1Message);
			}
		}

		void CheckB5_UnitCount_NR0003Rule()
		{
			var parent = Parent;
			if (parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleNR0003Active: true }
				&& parent.B5_UnitCount.IsEmpty
				&& !parent.IsBulk)
			{
				if (GoodsItem.Packages.Cast<NctsPackage>().Any(x => x.B5_UnitCount > 0))
				{
					parent.B5_UnitCountInfo.AddMessageError(ValidationRuleConfiguration.Messages.NR0003Message);
				}
			}
		}

		void CheckB5_UnitCount_R0364_1Rule()
		{
			var parent = Parent;
			var goodsItem = GoodsItem;
			if (parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleR0364_1Active: true } && parent.B5_UnitCount.IsEmpty && !parent.IsBulk && !parent.IsUnpacked)
			{
				if (!goodsItem.Bill.GoodsItems.Any(x => x.BY_IsMainPack && x.PK != goodsItem.PK && x.Packages.Cast<NctsPackage>().Any(y => y.B5_UnitCount > 0 && y.B5_MarksAndNumbers == parent.B5_MarksAndNumbers)))
				{
					parent.B5_UnitCountInfo.AddMessageError(Res.GetString("06E2E3C9-1CBE-4946-924E-1F0314AD643C", "[R0364-1] You have not entered a Package Quantity or a Main Pack on another Line of this House Consignment."));
				}
			}
		}

		void CheckB5_UnitCount_R0364_2Rule()
		{
			var parent = Parent;
			var goodsItem = GoodsItem;
			if (parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleR0364_2Active: true } && parent.B5_UnitCount.IsEmpty && !parent.IsBulk && !parent.IsUnpacked)
			{
				if (!goodsItem.Bill.GoodsItems.SelectMany(x => x.Packages.Cast<NctsPackage>()).Where(x => x.PK != parent.PK).Any(y => y.B5_UnitCount > 0 && y.B5_MarksAndNumbers == parent.B5_MarksAndNumbers))
				{
					var packTypes = $"{parent.Lookups.UnpackedPackageUnitTypeList.CodesAsString}, {parent.Lookups.BulkPackageUnitTypeList.CodesAsString}";
					parent.B5_UnitCountInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0364_2Message(packTypes));
				}
			}
		}

		void CheckB5_UnitCount_R0364_3Rule()
		{
			var parent = Parent;
			if (parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleR0364_3Active: true } && ShouldCheckB5_UnitCount_R0364_3Rule)
			{
				parent.B5_UnitCountInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0364_3Message());
			}
		}

		bool ShouldCheckB5_UnitCount_R0364_3Rule
		{
			get
			{
				var package = Parent;
				var hasPackageInGoodsItemsInBillWithSameMarksWithQtyMoreThan0 = GoodsItem.Bill?.GoodsItems.Any(x => x.Packages.Any(pack => pack.B5_UnitCount > ZLong.Zero && pack.B5_MarksAndNumbers == package.B5_MarksAndNumbers)) ?? false;
				return package.B5_UnitCount == ZLong.Zero && !package.IsBulk && !hasPackageInGoodsItemsInBillWithSameMarksWithQtyMoreThan0;
			}
		}

		void CheckB5_UnitCount_E1111Rule()
		{
			var parent = Parent;
			var value = parent.B5_UnitCount;
			var maxValueForTransitionPeriod = 99999;
			if (parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleE1111Active: true }
				&& parent.IsInPhase5TransitionPeriod
				&& !value.IsEmpty
				&& value > maxValueForTransitionPeriod)
			{
				parent.B5_UnitCountInfo.AddMessageError(Res.GetString("F8273425-B58B-4043-B9DD-A6B0A31405EF", "[E1111] Entered Number Of Packages exceeding the max value (5 digits) supported Inside Transition Period."));
			}
		}

		void CheckB5_UnitCount_TR0066Rule()
		{
			var parent = Parent;
			var value = parent.B5_UnitCount;
			var b5UnitCountInfo = parent.B5_UnitCountInfo;

			if (value > B5_UnitCountMaxValue)
			{
				string message = Res.GetString("90AFD42E-D644-4A22-B5C8-296DD45791E9", "Please enter a 'Number of Packages' less than or equal to {0}.", B5_UnitCountMaxValue);

				if (parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleTR0066Active: true }
					&& !parent.IsInPhase5TransitionPeriod)
				{
					message = ValidationRuleConfiguration.Messages.TR0066Message(B5_UnitCountMaxValue);
				}
				b5UnitCountInfo.AddMessageError(message);
			}
		}

		void CheckB5_UnitCount_NR0027Rule()
		{
			var parent = Parent;
			var value = parent.B5_UnitCount;
			var b5UnitCountInfo = parent.B5_UnitCountInfo;

			if (parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleNR0027Active: true }
				&& value > 0 && parent.Parent.Packages.Cast<NctsPackage>().Any(p => p.B5_UnitCount.IsEmpty))
			{
				b5UnitCountInfo.AddMessageError(ValidationRuleConfiguration.Messages.NR0027Message);
			}
		}

		protected internal override void CheckRuleC0670()
		{
			var rowMessageError = ValidationRuleConfiguration.Messages.C0670Message;

			var package = Parent;
			package.RemoveRowMessageError(rowMessageError);
			if (package.ValidationDecider is INctsPackagePhase5ValidationDecider { IsRuleC0670Active: true }
				&& (package.Parent?.Header?.HasMultipleContainerisedContainers ?? false)
				&& package.ContainersPivot.Count == 0)
			{
				package.AddRowMessageError(rowMessageError);
			}
		}

		void CheckRuleR0219(NctsPackage package)
		{
			var parent = Parent;
			if (!package.IsPhase5Departure
				|| parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleR0219Active: false }
				|| DisabledByB1819())
			{
				return;
			}

			if (CheckRuleR0219(package.B5_UnitCount, package.Parent.Packages.Cast<NctsPackage>()))
			{
				package.B5_UnitCountInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0219Message);
			}

			bool CheckRuleR0219(ZLong unitCount, System.Collections.Generic.IEnumerable<NctsPackage> packages)
				=> (unitCount == 0 && packages.Any(p => p.B5_UnitCount > 0))
					|| (unitCount > 0 && packages.Any(p => p.B5_UnitCount == 0));

			bool DisabledByB1819() => parent.ValidationDecider is INctsPackageDeparturePhase5ValidationDecider { IsRuleB1819Active: true } && package.IsInPhase5TransitionPeriod;
		}

		protected ValidationRuleConfiguration ValidationRuleConfiguration => GoodsItem.Header?.Configuration.ValidationRuleConfiguration;

		ZBool IsDepartureMovement => GoodsItem.Header.IsDepartureMovement;

		ZBool IsPluggedIntoShipment => GoodsItem.Header?.IsPluggedIntoShipment ?? false;

		void CheckRuleR0220(NctsPackage package)
		{
			if (!(package.ValidationDecider is INctsPackagePhase5ValidationDecider { IsRuleB1919Active: true } && package.IsInPhase5TransitionPeriod)
				&& package.ValidationDecider is INctsPackagePhase5ValidationDecider { IsRuleR0220Active: true }
				&& package.B5_UnitCount.IsEmpty
				&& package.IsUnpacked)
			{
				package.B5_UnitTypeInfo.AddMessageError(Res.GetString("8c9c4b8b-e74a-4013-80b9-6246a00e9e8b",
					"[R0220] Package Type is not valid when number of packages is zero '0'."));
			}
		}
	}
}
