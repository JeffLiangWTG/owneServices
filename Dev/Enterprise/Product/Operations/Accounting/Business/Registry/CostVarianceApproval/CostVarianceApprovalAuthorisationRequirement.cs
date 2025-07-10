using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CostVarianceApprovalAuthorisationRequirement : AmountBasedTwoLevelAuthorisationRequirement
	{
		#region Schema

		public new abstract class Schema : AmountBasedMultiLevelAuthorisationRequirement.Schema
		{
			public const string Percentage = "Percentage";
			public const string LocalCostAmount = "LocalCostAmount";
			public const string MonitorTotalInvoiceVariance = "MonitorTotalInvoiceVariance";
			public const string TotalInvoiceVarianceAmount = "TotalInvoiceVarianceAmount";
			public const string VarianceSign = "VarianceSign";
		}

		#endregion

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Percentage = 0M;
			LocalCostAmount = 0M;
			MonitorTotalInvoiceVariance = false;
			TotalInvoiceVarianceAmount = 0M;
			VarianceSign = VarianceSigns.Plus;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CostVarianceApprovalAuthorisationRequirement();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateLocalCostAmount();
			ValidatePercentage();
			ValidateMonitorTotalInvoiceVariance();
			ValidateTotalInvoiceVarianceAmount();
			ValidateVarianceSign();
		}

		CostVarianceApprovalAuthorisationRequirementCollection ParentCollection
		{
			get { return (CostVarianceApprovalAuthorisationRequirementCollection)GetParentCollection(this, typeof(CostVarianceApprovalAuthorisationRequirementCollection)); }
		}

		protected override IEnumerable<AmountBasedMultiLevelAuthorisationRequirement> GetOtherCollectionElements()
		{
			var result = base.GetOtherCollectionElements();
			if (result.Any())
			{
				return result.Cast<CostVarianceApprovalAuthorisationRequirement>().Where(x => x.VarianceSign == VarianceSign);
			}
			else
			{
				return Enumerable.Empty<CostVarianceApprovalAuthorisationRequirement>();
			}
		}

		#region Bound Properties

		#region Percentage

		ZDecimal fPercentage;
		public ZDecimal Percentage
		{
			get { return Percentage_ReadOnly ? (ZDecimal)0M : fPercentage; }
			set
			{
				SetNonPersistentPropertyValue(PercentageInfo, ref fPercentage, value);
				if (!IsValidationSuspended)
				{
					ValidatePercentage();
				}
			}
		}

		public ZPropertyInfo PercentageInfo
		{
			get { return GetZPropertyInfo(Schema.Percentage); }
		}

		protected bool Percentage_ReadOnly
		{
			get { return ParentCollection != null && ParentCollection.Parent.VarianceCalculationStyle == Constants.CostVarianceCalculationStyle.LocalExTaxAmount; }
		}

		public void ValidatePercentage()
		{
			PercentageInfo.ClearAllNotifications();
			if (!Percentage_ReadOnly)
			{
				ValidateAmount();
				CompareValidation.CheckWithinRange(PercentageInfo, 0m, 100m);
			}
		}

		#endregion

		#region LocalCostAmount

		ZDecimal fLocalCostAmount;
		public ZDecimal LocalCostAmount
		{
			get { return LocalCostAmount_ReadOnly ? (ZDecimal)0M : fLocalCostAmount; }
			set
			{
				SetNonPersistentPropertyValue(LocalCostAmountInfo, ref fLocalCostAmount, value);
				if (!IsValidationSuspended)
				{
					ValidateLocalCostAmount();
				}
			}
		}

		public ZPropertyInfo LocalCostAmountInfo
		{
			get { return GetZPropertyInfo(Schema.LocalCostAmount, "Amount"); }
		}

		protected bool LocalCostAmount_ReadOnly
		{
			get { return ParentCollection != null && ParentCollection.Parent.VarianceCalculationStyle == Constants.CostVarianceCalculationStyle.PercentageVariance; }
		}

		public void ValidateLocalCostAmount()
		{
			LocalCostAmountInfo.ClearAllNotifications();
			if (Percentage_ReadOnly)
			{
				ValidateAmount();
			}
			else if (!LocalCostAmount_ReadOnly)
			{
				CompareValidation.CheckNumberGreaterThanZero(LocalCostAmountInfo);
				AboveLineMustBeAsLastUpToLineValidation(LocalCostAmountInfo);
				HigherAmountsHigherAuthorisationLevelAndMandatoryAboveLevelValidation(LocalCostAmountInfo);
			}
		}

		#endregion

		#region Amount

		public override ZDecimal Amount
		{
			get
			{
				return Percentage_ReadOnly ? LocalCostAmount : Percentage;
			}
			set
			{
				if (Percentage_ReadOnly)
				{
					LocalCostAmount = value;
				}
				else
				{
					Percentage = value;
				}
			}
		}

		public override ZPropertyInfo AmountInfo
		{
			get { return GetZPropertyInfo(Percentage_ReadOnly ? Schema.LocalCostAmount : Schema.Percentage); }
		}

		#endregion

		#region MonitorTotalInvoiceVariance

		ZBool fMonitorTotalInvoiceVariance;
		public ZBool MonitorTotalInvoiceVariance
		{
			get { return fMonitorTotalInvoiceVariance; }
			set
			{
				SetNonPersistentPropertyValue(MonitorTotalInvoiceVarianceInfo, ref fMonitorTotalInvoiceVariance, value);
				TotalInvoiceVarianceAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MonitorTotalInvoiceVarianceInfo
		{
			get { return GetZPropertyInfo(Schema.MonitorTotalInvoiceVariance, "Monitor Total Invoice Variance"); }
		}

		public void ValidateMonitorTotalInvoiceVariance()
		{
			MonitorTotalInvoiceVarianceInfo.ClearAllNotifications();
			CheckThatWhenLinesWithHigherAuthorizationLevelTickedLinesWithLowerShouldBeTickedAlso();
		}

		void CheckThatWhenLinesWithHigherAuthorizationLevelTickedLinesWithLowerShouldBeTickedAlso()
		{
			if (!MonitorTotalInvoiceVariance && !AuthorisationRequirement.IsEmpty)
			{
					int currentWeight = GetAuthorisationRequirementWeight(AuthorisationRequirement);

					foreach (CostVarianceApprovalAuthorisationRequirement setting in OtherCollectionElements)
					{
						if (GetAuthorisationRequirementWeight(setting.AuthorisationRequirement) > currentWeight && setting.MonitorTotalInvoiceVariance)
						{
							MonitorTotalInvoiceVarianceInfo.AddError(Res.GetString("01759881-556C-41F4-8D35-00BF0E53C9F4", "When lines with higher Authorization level ticked lines with lower should be ticked also."));
							break;
						}
					}
			}
		}

		#endregion

		#region TotalInvoiceVarianceAmount

		ZDecimal fTotalInvoiceVarianceAmount;
		public ZDecimal TotalInvoiceVarianceAmount
		{
			get { return TotalInvoiceVarianceAmount_ReadOnly ? (ZDecimal)0M : fTotalInvoiceVarianceAmount; }
			set
			{
				SetNonPersistentPropertyValue(TotalInvoiceVarianceAmountInfo, ref fTotalInvoiceVarianceAmount, value);
				if (!IsValidationSuspended)
				{
					ValidateTotalInvoiceVarianceAmount();
				}
			}
		}

		public ZPropertyInfo TotalInvoiceVarianceAmountInfo
		{
			get { return GetZPropertyInfo(Schema.TotalInvoiceVarianceAmount, "Total Invoice Variance Amount"); }
		}

		protected bool TotalInvoiceVarianceAmount_ReadOnly
		{
			get { return !MonitorTotalInvoiceVariance; }
		}

		public void ValidateTotalInvoiceVarianceAmount()
		{
			TotalInvoiceVarianceAmountInfo.ClearAllNotifications();
			AboveLineMustBeAsUpToLineWithHighestAuthorizationLevel();

			if (!TotalInvoiceVarianceAmount_ReadOnly)
			{
				if (TotalInvoiceVarianceAmount < LocalCostAmount)
				{
					TotalInvoiceVarianceAmountInfo.AddError(Res.GetString("3596c155-4380-4779-a07c-525b140210aa", "Total Invoice Variance Amount Can not be less than Amount"));
				}

				HigherTotalInvoiceVarianceAmountHigherAuthorisationLevel();
			}
		}

		void AboveLineMustBeAsUpToLineWithHighestAuthorizationLevel()
		{
			if (Range == RangeCodes.Above)
			{
				CostVarianceApprovalAuthorisationRequirement largestUpTo = null;

				foreach (CostVarianceApprovalAuthorisationRequirement setting in OtherCollectionElements)
				{
					if (setting.Range == RangeCodes.UpTo)
					{
						ZDecimal settingWeight = GetAuthorisationRequirementWeight(setting.AuthorisationRequirement);

						if (largestUpTo == null || settingWeight > GetAuthorisationRequirementWeight(largestUpTo.AuthorisationRequirement))
						{
							largestUpTo = setting;
						}
					}
				}

				if (largestUpTo != null && TotalInvoiceVarianceAmount != largestUpTo.TotalInvoiceVarianceAmount)
				{
					TotalInvoiceVarianceAmountInfo.AddError(Res.GetString("F4A1D4BA-C103-4317-8055-B1BA6B1FDC41", "The Above Line's Total Invoice Variance Amount must be {0}.", largestUpTo.TotalInvoiceVarianceAmount.ToString()));
				}
			}
		}

		void HigherTotalInvoiceVarianceAmountHigherAuthorisationLevel()
		{
			if (Range == RangeCodes.UpTo && !AuthorisationRequirement.IsEmpty)
			{
				int currentWeight = GetAuthorisationRequirementWeight(AuthorisationRequirement);

				foreach (CostVarianceApprovalAuthorisationRequirement setting in OtherCollectionElements)
				{
					if (setting.Range == Range && setting.MonitorTotalInvoiceVariance)
					{
						int settingWeight = GetAuthorisationRequirementWeight(setting.AuthorisationRequirement);
						ZDecimal settingAmount = setting.TotalInvoiceVarianceAmount;
						if ((settingAmount <= TotalInvoiceVarianceAmount && settingWeight > currentWeight) ||
							(settingAmount >= TotalInvoiceVarianceAmount && settingWeight < currentWeight))
						{
							TotalInvoiceVarianceAmountInfo.AddError(Res.GetString("2E101C80-345D-4506-ACF2-602BE3DACC05", "Higher Total Invoice Variance Amount must have Authorization level higher than lower Total Invoice Variance Amount."));
						}
					}
				}
			}
		}

		#endregion

		#region VarianceSign

		public static class VarianceSigns
		{
			public const string Plus = "+";
			public const string Minus = "-";
		}

		[List("VarianceSignList")]
		[MaxLength(1)]
		public ZString VarianceSign
		{
			get { return fVarianceSign; }
			set
			{
				SetNonPersistentPropertyValue(VarianceSignInfo, ref fVarianceSign, value);
				if (!IsValidationSuspended)
				{
					ValidateVarianceSign();
				}
			}
		}
		ZString fVarianceSign;

		public ZPropertyInfo VarianceSignInfo
		{
			get { return GetZPropertyInfo(Schema.VarianceSign, "Variance Sign"); }
		}

		public void ValidateVarianceSign()
		{
			VarianceSignInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(VarianceSignInfo);
			ListValidation.ErrorIfInvalidCode(VarianceSignInfo, VarianceSignList);
		}

		public int VarianceSignMultiplier
		{
			get { return VarianceSign == VarianceSigns.Minus ? -1 : 1; }
		}

		#endregion

		#endregion

		#region Lookups

		#region Variance Sign List

		public CodeDescriptionPairList VarianceSignList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(VarianceSigns.Plus);
				result.AddPair(VarianceSigns.Minus);
				return result;
			}
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Percentage, Percentage.ToString());
			writer.WriteElementString(Schema.LocalCostAmount, LocalCostAmount.ToString());
			writer.WriteElementString(Schema.MonitorTotalInvoiceVariance, MonitorTotalInvoiceVariance.ToString());
			writer.WriteElementString(Schema.TotalInvoiceVarianceAmount, TotalInvoiceVarianceAmount.ToString());
			writer.WriteElementString(Schema.VarianceSign, VarianceSign.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			Percentage = ZDecimal.Parse(reader.ReadElementString(Schema.Percentage));
			LocalCostAmount = ZDecimal.Parse(reader.ReadElementString(Schema.LocalCostAmount));
			MonitorTotalInvoiceVariance = reader.ReadElementStringAsZBool(Schema.MonitorTotalInvoiceVariance);
			TotalInvoiceVarianceAmount = ZDecimal.Parse(reader.ReadElementString(Schema.TotalInvoiceVarianceAmount));
			VarianceSign = reader.ReadElementString(Schema.VarianceSign) == VarianceSigns.Minus ? VarianceSigns.Minus : VarianceSigns.Plus;
		}

		#endregion
	}
}
