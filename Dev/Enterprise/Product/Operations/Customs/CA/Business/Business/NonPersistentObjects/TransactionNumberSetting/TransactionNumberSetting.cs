using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class TransactionNumberSetting : NumberSetting
	{
		const long MaxMaximumNumber = 99_999_999;
		const long MinRemainingCapacity = 100;

		public TransactionNumberSetting(BusinessObjectFactory factory, ZString owner, ZString accountSecurityCode, GlbBranch branch, ZString declarationType, bool isEditable)
			: base(factory)
		{
			this.owner = owner;
			this.accountSecurityCode = accountSecurityCode;
			this.branch = branch;
			this.declrarationType = declarationType;
			this.rangesSeparator = TransactionNumber.GetRangeSeparator(AccountSecurityCode, branch, DeclarationType);
			this.isEditable = isEditable;
		}

		readonly ZString owner;
		readonly ZString accountSecurityCode;
		readonly GlbBranch branch;
		readonly ZString declrarationType;
		readonly ZString rangesSeparator;

		INumberFountainProxy numberFountainCached;
		bool isEditable;

		#region Properties

		#region Owner

		public ZString Owner
		{
			get { return owner; }
		}

		public ZPropertyInfo OwnerInfo
		{
			get { return GetZPropertyInfo(nameof(Owner)); }
		}

		#endregion

		#region AccountSecurityCode

		public ZString AccountSecurityCodeForUI
		{
			get { return accountSecurityCode == ZString.Empty ? DefaultAccountSecurityCode : accountSecurityCode; }
		}

		public ZPropertyInfo AccountSecurityCodeForUIInfo
		{
			get { return GetZPropertyInfo(nameof(AccountSecurityCodeForUI)); }
		}

		public ZString AccountSecurityCode
		{
			get { return accountSecurityCode; }
		}

		#endregion

		public bool IsEditable
		{
			get { return isEditable; }
			set
			{
				isEditable = value;
				ValidateAll(false);
			}
		}

		internal ZString RangesSeparator
		{
			get { return rangesSeparator; }
		}

		public ZString DeclarationType
		{
			get { return declrarationType; }
		}

		public ZString BranchCode
		{
			get { return branch == null ? ZString.Empty : branch.GB_Code; }
		}

		bool MustNotIntersectWith(TransactionNumberSetting other)
		{
			if (this.AccountSecurityCode == other.AccountSecurityCode)
			{
				return true;
			}
			return false;
		}

		bool IntersectsWith(TransactionNumberSetting other)
		{
			return NewOrCurrentMinNumber <= other.NewOrCurrentMaxNumber && other.NewOrCurrentMinNumber <= NewOrCurrentMaxNumber;
		}

		public ZString RangeName
		{
			get
			{
				return string.Format(
					CultureInfo.InvariantCulture,
					"{0} {1} {2} {3}",
					Owner,
					AccountSecurityCodeForUI,
					DeclarationType,
					BranchCode
				).Trim();
			}
		}

		#endregion

		#region Inherited Values (for tests)

#if DEBUG

		internal ZDecimal InheritedNextNumberForTest
		{
			get
			{
				var numberFountain = EffectiveNumberFountainForTest;
				if (numberFountain == null)
				{
					return -1;
				}
				return numberFountain.PeekPreliminary(Factory);
			}
		}

		internal ZDecimal InheritedMinNumberForTest
		{
			get
			{
				var numberFountain = EffectiveNumberFountainForTest;
				if (numberFountain == null)
				{
					return -1;
				}
				long minValue, maxValue;
				numberFountain.GetMinAndMaxValues(Factory, out minValue, out maxValue);
				return minValue;
			}
		}

		internal ZDecimal InheritedMaxNumberForTest
		{
			get
			{
				var numberFountain = EffectiveNumberFountainForTest;
				if (numberFountain == null)
				{
					return -1;
				}
				long minValue, maxValue;
				numberFountain.GetMinAndMaxValues(Factory, out minValue, out maxValue);
				return maxValue;
			}
		}

		INumberFountainProxy EffectiveNumberFountainForTest
		{
			get
			{
				if (effectiveNumberFountainForTestCached != null)
				{
					return effectiveNumberFountainForTestCached;
				}

				if (declrarationType == TransactionNumber.DIFNumberDeclarationType)
				{
					// DIF fountain does not have fallbacks
					effectiveNumberFountainForTestCached = NumberFountain;
				}
				else
				{
					effectiveNumberFountainForTestCached = TransactionNumber.GetEffectiveNumberFountain(AccountSecurityCode, branch, declrarationType, Factory);
				}

				return effectiveNumberFountainForTestCached;
			}
		}

		INumberFountainProxy effectiveNumberFountainForTestCached;

#endif

		#endregion

		protected override INumberFountainProxy NumberFountain
		{
			get
			{
				if (numberFountainCached == null)
				{
					var fountainKey = TransactionNumber.GetNumberFountainKey(RangesSeparator);
					numberFountainCached = TransactionNumber.GetNumberFountain(fountainKey);
				}
				return numberFountainCached;
			}
		}

#if DEBUG
		protected override string NumberFountainNameForTestig => Env.NumberFountains.GetKey("C", TransactionNumber.GetNumberFountainKey(RangesSeparator));
		protected override Guid NumberFountainOwnerForTestig => Guid.Empty;
#endif

		public static ZString DefaultAccountSecurityCode
		{
			get { return "DEFAULT"; }
		}

		public override bool CanDelete
		{
			get
			{
				if (IsEditable && IsInitialized)
				{
					return false;
				}
				return base.CanDelete;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (IsEditable && IsInitialized)
				{
					return ResString.GetMultilingualString("FD9525B9-7F50-49BF-98B2-DDBA13D5C560", "Configuration for numbers that was already saved cannot be removed.");
				}
				return base.ReasonForNotAbleToDelete;
			}
		}

		protected override void ResetCachedValuesCore()
		{
			base.ResetCachedValuesCore();

			numberFountainCached = null;
#if DEBUG
			effectiveNumberFountainForTestCached = null;
#endif
		}

		protected override void ValidateAllCore(bool revalidateRelated)
		{
			base.ValidateAllCore(revalidateRelated);
			ValidateRemainingCapacity();
			ValidateRangeIntersections(revalidateRelated);
		}

		protected override void ValidateNextNumberCore()
		{
			base.ValidateNextNumberCore();
			ValidateExplicitValueForNewRange(NextNumberInfo);
		}

		protected override void ValidateMinNumberCore()
		{
			base.ValidateMinNumberCore();
			ValidateExplicitValueForNewRange(MinNumberInfo);
		}

		protected override void ValidateMaxNumberCore()
		{
			base.ValidateMaxNumberCore();
			ValidateExplicitValueForNewRange(MaxNumberInfo);
			if (MaxNumber != 0 && MaxNumber > MaxMaximumNumber)
			{
				string message = Res.GetString("62F43049-90FE-4937-A364-B83F9B2BE3CB", "The maximum number should not be greater than {0}.", MaxMaximumNumber);
				MaxNumberInfo.AddError(message);
			}
		}

		void ValidateExplicitValueForNewRange(ZPropertyInfo propertyInfo)
		{
			if (IsEditable && (ZDecimal)propertyInfo.Value == 0 && !IsInitialized)
			{
				propertyInfo.AddError(Res.GetString("1787EB52-3386-4D7E-9AFA-F574553CADC3", "For newly added number ranges all parameters must be set explicitly."));
			}
		}

		void ValidateRangeIntersections(bool revalidateRelated)
		{
			if (!IsEditable)
			{
				// list of available ranges should not be validated
				return;
			}
			foreach (var parentCollection in ParentCollections.OfType<TransactionNumberSettingCollection>())
			{
				if (parentCollection.AreSettingsEditable)
				{
					foreach (TransactionNumberSetting other in parentCollection)
					{
						if (this != other && this.MustNotIntersectWith(other))
						{
							if (other.IntersectsWith(this))
							{
								string message = GetIntersectionMessage(this, other);
								if (HasChangesToSave)
								{
									AddRowError(message);
								}
								else
								{
									AddRowWarning(message);
								}
							}
							if (revalidateRelated)
							{
								other.ValidateAll(false);
							}
						}
					}
				}
			}
		}

		void ValidateRemainingCapacity()
		{
			if (NewOrCurrentNextNumber < 0 || NewOrCurrentMaxNumber < 0)
			{
				// Return early to avoid potential OverflowException.
				// Negative numbers should be handled by another validation.
				return;
			}

			ZDecimal remainingCapacity = NewOrCurrentMaxNumber - NewOrCurrentNextNumber + 1;
			if (remainingCapacity < MinRemainingCapacity)
			{
				string message = Res.GetString("B7D79600-964A-4FB9-861A-D084CB3D491A", "There are less than {0} available numbers left.", MinRemainingCapacity);
				if (HasChangesToSave)
				{
					AddRowError(message);
				}
				else
				{
					AddRowWarning(message);
				}
			}
		}

		protected override ZString GenerateNumberWithCheckDigit(ZDecimal number)
		{
			var result = RangesSeparator + number.ToString().PadLeft(8, '0');
			result += TransactionNumber.GetCheckDigit(result);
			return result;
		}

		public override CusEntryNumber GetCusEntryNumberMatching(ZString number)
		{
			return !number.IsEmpty ? CusEntryNumber.Load(Factory, CusEntryNumber.EntryType.CATransactionNumber, number, Core.Constants.CountryCodes.Canada).FirstOrDefault() : null;
		}

		public override BaseJobDeclaration GetJobDeclaration(CusEntryNumber entryNumber)
		{
			return entryNumber.CE_ParentTable == JobDeclarationSchema.Constants.TableName ? Factory.Load<JobDeclaration>(entryNumber.CE_ParentID) : null;
		}

		static string GetIntersectionMessage(TransactionNumberSetting setting1, TransactionNumberSetting setting2)
		{
			return Res.GetString(
				"D8A93FDE-F5B9-4B23-814E-434931358241",
				"The range '{0}' [{1}..{2}] intersects with range '{3}' [{4}..{5}].",
				setting1.RangeName,
				setting1.NewOrCurrentMinNumber,
				setting1.NewOrCurrentMaxNumber,
				setting2.RangeName,
				setting2.NewOrCurrentMinNumber,
				setting2.NewOrCurrentMaxNumber
			);
		}
	}
}
