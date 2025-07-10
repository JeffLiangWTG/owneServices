using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AmountOrPercentageBasedThreeLevelAuthorisationRequirement : AmountBasedThreeLevelAuthorisationRequirement
	{
		public new class Schema : AmountBasedThreeLevelAuthorisationRequirement.Schema
		{
			public const string Percentage = "Percentage";
		}

		#region Percentage

		[DecimalPlaces(2)]
		public virtual ZDecimal Percentage
		{
			get { return percentage; }
			set
			{
				SetNonPersistentPropertyValue(PercentageInfo, ref percentage, value);
				if (!IsValidationSuspended)
				{
					ValidatePercentage();
				}
			}
		}

		public virtual ZPropertyInfo PercentageInfo
		{
			get { return GetZPropertyInfo(Schema.Percentage); }
		}

		public void ValidatePercentage()
		{
			ValidatePercentageCore();
		}

		protected virtual void ValidatePercentageCore()
		{
			PercentageInfo.ClearAllNotifications();
			AmountEnteredValidation(PercentageInfo);
			ValidateNotBothPercentageAndAmountSpecified();
			UpToLinesWithTheSameAmountValidation(PercentageInfo);
			AboveLineMustBeAsLastUpToLineValidation(PercentageInfo);
			HigherAmountsHigherAuthorisationLevelAndMandatoryAboveLevelValidation(PercentageInfo);
		}

		protected void ValidateNotBothPercentageAndAmountSpecified()
		{
			if (ParentCollection != null)
			{
				bool percentageUsed = false;
				bool amountUsed = false;

				foreach (AmountOrPercentageBasedThreeLevelAuthorisationRequirement settings in ParentCollection)
				{
					percentageUsed |= !settings.Percentage.IsEmpty;
					amountUsed |= !settings.Amount.IsEmpty;
				}

				if (percentageUsed && amountUsed)
				{
					if (!PercentageInfo.HasErrors() && !Percentage.IsEmpty)
					{
						PercentageInfo.AddError(UsePercentagesOrAmountsError);
					}
					if (!AmountInfo.HasErrors() && !Amount.IsEmpty)
					{
						AmountInfo.AddError(UsePercentagesOrAmountsError);
					}
				}
			}
		}

		ZDecimal percentage;

		#endregion

		protected override bool IsEmpty
		{
			get
			{
				return Amount == 0 && Percentage == 0;
			}
		}

		protected override void ValidateAuthorisationRequirementCore()
		{
			base.ValidateAuthorisationRequirementCore();
			if (!AuthorisationRequirementInfo.HasErrors())
			{
				if (Amount.IsEmpty && Percentage.IsEmpty && Range == RangeCodes.UpTo && AuthorisationRequirement != AuthorisationRequirementCodes.NoApprovalRequired)
				{
					AuthorisationRequirementInfo.AddError(Res.GetString("965cfa86-82cf-4800-99f7-0d6978038999", "An 'Up to' range with 'Amount = 0' must have an authorization requirement of 'None'."));
				}
			}
		}

		#region Validation Overrides

		protected override void ValidateAmountCore()
		{
			base.ValidateAmountCore();
			ValidateNotBothPercentageAndAmountSpecified();
		}

		protected override void AmountEnteredValidation(ZPropertyInfo amountProperty)
		{
			CompareValidation.CheckNumberNotNegative(amountProperty);
		}

		bool CollectionContainsNonEmptyValue(ZPropertyInfo amountProperty)
		{
			if (ParentCollection != null)
			{
				var settings = ParentCollection.Cast<AmountOrPercentageBasedThreeLevelAuthorisationRequirement>();
				return settings.Any(s => !((IZType)s[amountProperty.Name]).IsEmpty);
			}

			return false;
		}

		protected override void UpToLinesWithTheSameAmountValidation(ZPropertyInfo amountProperty)
		{
			var shouldValidate = CollectionContainsNonEmptyValue(amountProperty);

			if (shouldValidate)
			{
				base.UpToLinesWithTheSameAmountValidation(amountProperty);
			}
		}

		protected override void AboveLineMustBeAsLastUpToLineValidation(ZPropertyInfo amountProperty)
		{
			var shouldValidate = CollectionContainsNonEmptyValue(amountProperty);

			if (shouldValidate)
			{
				base.AboveLineMustBeAsLastUpToLineValidation(amountProperty);
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatePercentage();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Percentage, Percentage.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			Percentage = ZDecimal.ParseSafe(reader.ReadElementString(Schema.Percentage), 0);
		}

		#endregion

		#region Error Messages

		static string UsePercentagesOrAmountsError
		{
			get { return Res.GetString("84ba3a3a-73f9-4614-967a-3ff1b702efc7", "Please either use percentages in all settings or amounts in all settings. You cannot enter both."); }
		}

		#endregion

		AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection ParentCollection
		{
			get { return (AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection)this.GetParentCollection(this, typeof(AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection)); }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();
		}
	}
}
