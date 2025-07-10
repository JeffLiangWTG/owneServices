using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public abstract class AmountBasedMultiLevelAuthorisationRequirement : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Amount = "Amount";
			public const string AmountDecimals = "AmountDecimals";
			public const string AuthorisationRequirement = "AuthorisationRequirement";
			public const string Range = "Range";
		}

		#endregion

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Amount = 0;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateAmount();
			ValidateAuthorisationRequirement();
			ValidateRange();
		}

		AmountBasedAuthorisationRequirementCollection ParentCollection
		{
			get { return (AmountBasedAuthorisationRequirementCollection)GetParentCollection(this, typeof(AmountBasedAuthorisationRequirementCollection)); }
		}

		protected IEnumerable<AmountBasedMultiLevelAuthorisationRequirement> OtherCollectionElements
		{
			get { return GetOtherCollectionElements(); }
		}

		protected virtual IEnumerable<AmountBasedMultiLevelAuthorisationRequirement> GetOtherCollectionElements()
		{
			return ParentCollection != null ?
				ParentCollection.Cast<AmountBasedMultiLevelAuthorisationRequirement>().Where(x => x != this) :
				Enumerable.Empty<AmountBasedMultiLevelAuthorisationRequirement>();
		}

		#region Bound Properties

		#region Range

		[MaxLength(5)]
		public ZString Range
		{
			get { return range; }
			set
			{
				CheckMaximumLength(RangeInfo, value);
				SetNonPersistentPropertyValue<ZString>(RangeInfo, ref range, value);
				if (!IsValidationSuspended)
				{
					ValidateRange();
				}
			}
		}

		public ZPropertyInfo RangeInfo
		{
			get { return GetZPropertyInfo(Schema.Range); }
		}

		[List("RangeList")]
		public ZString RangeLocalized
		{
			get
			{
				return RangeMultilingual.ToString();
			}
			set
			{
				var rangeTemp = value;
				foreach (CodeDescriptionPair rangeIndex in RangeList)
				{
					if (rangeIndex.MultilingualCode.ToString() == value || string.Equals(rangeIndex.Code, value, StringComparison.OrdinalIgnoreCase))
					{
						rangeTemp = rangeIndex.Code;
						break;
					}
				}
				Range = rangeTemp;
			}
		}

		public ZPropertyInfo RangeLocalizedInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(RangeLocalized), o => RangeInfo); }
		}

		public MultilingualString RangeMultilingual
		{
			get
			{
				CodeDescriptionPair match = RangeList[Range] as CodeDescriptionPair;
				return match != null ? match.MultilingualCode : (NoResString)Range;
			}
		}

		public void ValidateRange()
		{
			ValidateRangeCore();
		}

		protected virtual void ValidateRangeCore()
		{
			RangeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(RangeInfo);
			ListValidation.ErrorIfInvalidCode(RangeInfo, RangeList);

			if (OtherCollectionElements.Any() && Range == RangeCodes.Above)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(RangeInfo, OtherCollectionElements, Res.GetString("0235ead0-871c-4380-af4d-9312550fbeba", "There must be only one 'Above' line."));
			}
			if (!RangeInfo.HasErrors())
			{
				if ((Range != RangeCodes.Above || !IsEmpty) &&
					((Range != RangeCodes.UpTo && OtherCollectionElements.FirstOrDefault(x => x.Range == RangeCodes.UpTo) == null) ||
					(Range != RangeCodes.Above && OtherCollectionElements.FirstOrDefault(x => x.Range == RangeCodes.Above) == null)))
				{
					RangeInfo.AddError(UpToAndAboveRequiredMessage);
				}
				else
				{
					var requirementsWithError = OtherCollectionElements.Where(x => x.RangeInfo.HasError(UpToAndAboveRequiredMessage) && x.RangeInfo.GetErrors().Count() == 1);
					foreach (var requirement in requirementsWithError)
					{
						requirement.RangeInfo.ClearAllNotifications();
					}
				}
			}
		}

		protected virtual bool IsEmpty
		{
			get
			{
				return Amount == 0;
			}
		}

		static string UpToAndAboveRequiredMessage
		{
			get { return Res.GetString("32858182-5d19-46cb-b08f-e63c9e2ea64a", "There must be at least one 'Up to' and one 'Above' line."); }
		}

		ZString range;

		#endregion

		#region Amount

		public virtual ZDecimal Amount
		{
			get { return fAmount; }
			set
			{
				SetNonPersistentPropertyValue<ZDecimal>(AmountInfo, ref fAmount, value);
				if (!IsValidationSuspended)
				{
					ValidateAmount();
				}
			}
		}

		public virtual ZPropertyInfo AmountInfo
		{
			get { return GetZPropertyInfo(Schema.Amount); }
		}

		public void ValidateAmount()
		{
			ValidateAmountCore();
		}

		protected virtual void ValidateAmountCore()
		{
			AmountInfo.ClearAllNotifications();

			AmountEnteredValidation(AmountInfo);

			UpToLinesWithTheSameAmountValidation(AmountInfo);
			AboveLineMustBeAsLastUpToLineValidation(AmountInfo);
		}

		protected virtual void AmountEnteredValidation(ZPropertyInfo amountProperty)
		{
			if ((Range.IsEmpty || Range == RangeCodes.Above) && !OtherCollectionElements.Any())
			{
				CompareValidation.CheckNumberNotNegative(amountProperty);
			}
			else
			{
				CompareValidation.CheckNumberGreaterThanZero(amountProperty);
			}
		}

		protected virtual void UpToLinesWithTheSameAmountValidation(ZPropertyInfo amountProperty)
		{
			if (Range == RangeCodes.UpTo)
			{
				foreach (AmountBasedMultiLevelAuthorisationRequirement setting in OtherCollectionElements)
				{
					if (setting.Range == Range && (ZDecimal)setting[amountProperty.Name] == (ZDecimal)amountProperty.Value)
					{
						amountProperty.AddError(Res.GetString("d1a4135c-c088-48bf-b76c-ce7aacdc73d4", "No two 'Up to' Lines can have the same amount."));
						break;
					}
				}
			}
		}

		protected virtual void AboveLineMustBeAsLastUpToLineValidation(ZPropertyInfo amountProperty)
		{
			if (Range == RangeCodes.Above && OtherCollectionElements.Any())
			{
				ZDecimal largestUpTo = 0;

				foreach (AmountBasedMultiLevelAuthorisationRequirement setting in OtherCollectionElements)
				{
					ZDecimal settingAmount = (ZDecimal)setting[amountProperty.Name];
					if (setting.Range == RangeCodes.UpTo && settingAmount > largestUpTo)
					{
						largestUpTo = settingAmount;
					}
				}
				if ((ZDecimal)amountProperty.Value != largestUpTo)
				{
					amountProperty.AddError(Res.GetString("804d74dc-7d2d-4d1d-a922-5894ab6c2b44", "The Above Line's amount must be {0}.", largestUpTo.ToString()));
				}
			}
		}

		ZDecimal fAmount;

		#endregion

		#region AmountDecimals

		public ZInt AmountDecimals
		{
			get { return ParentCollection != null ? ParentCollection.CurrentFallbackCompanyCurrencyDecimals : 0; }
		}

		public ZPropertyInfo AmountDecimalsInfo
		{
			get { return GetZPropertyInfo(Schema.AmountDecimals); }
		}

		#endregion

		#region Authorisation Requirement

		[MaxLength(50)]
		public ZString AuthorisationRequirement
		{
			get { return authorisationRequirement; }
			set
			{
				CheckMaximumLength(AuthorisationRequirementInfo, value);
				SetNonPersistentPropertyValue<ZString>(AuthorisationRequirementInfo, ref authorisationRequirement, value);
				if (!IsValidationSuspended)
				{
					ValidateAuthorisationRequirement();
				}
			}
		}

		public ZPropertyInfo AuthorisationRequirementInfo
		{
			get { return GetZPropertyInfo(Schema.AuthorisationRequirement); }
		}

		[List("AuthorisationRequirementList")]
		public ZString AuthorisationRequirementLocalized
		{
			get
			{
				return AuthorisationRequirementMultilingual.ToString();
			}
			set
			{
				var authorisationRequirementTemp = value;
				foreach (CodeDescriptionPair authorisationRequirementIndex in AuthorisationRequirementList)
				{
					if (authorisationRequirementIndex.MultilingualCode.ToString() == value || string.Equals(authorisationRequirementIndex.Code, value, StringComparison.OrdinalIgnoreCase))
					{
						authorisationRequirementTemp = authorisationRequirementIndex.Code;
						break;
					}
				}
				AuthorisationRequirement = authorisationRequirementTemp;
			}
		}

		public ZPropertyInfo AuthorisationRequirementLocalizedInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(AuthorisationRequirementLocalized), o => AuthorisationRequirementInfo); }
		}
		public MultilingualString AuthorisationRequirementMultilingual
		{
			get
			{
				CodeDescriptionPair match = AuthorisationRequirementList[AuthorisationRequirement] as CodeDescriptionPair;
				return match != null ? match.MultilingualCode : (NoResString)AuthorisationRequirement;
			}
		}

		public void ValidateAuthorisationRequirement()
		{
			ValidateAuthorisationRequirementCore();
		}

		protected virtual void ValidateAuthorisationRequirementCore()
		{
			AuthorisationRequirementInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(AuthorisationRequirementInfo);
			ListValidation.ErrorIfInvalidCode(AuthorisationRequirementInfo, AuthorisationRequirementList);
		}

		ZString authorisationRequirement;

		#endregion

		#endregion

		#region Lookups

		#region Range List

		public static class RangeCodes
		{
			#region SuppressResourceStringsCheckRegion
			// Codes used as storage values

			public const string UpTo = "Up to";
			public const string Above = "Above";

			#endregion
		}

		public CodeDescriptionPairList RangeList
		{
			get
			{
				if (fRangeList == null)
				{
					fRangeList = new CodeDescriptionPairList();
					fRangeList.AddPair(ResString.GetMultilingualString("Range|UpTo", RangeCodes.UpTo));
					fRangeList.AddPair(ResString.GetMultilingualString("Range|Above", RangeCodes.Above));
				}

				return fRangeList;
			}
		}

		CodeDescriptionPairList fRangeList;

		#endregion

		#region Authorisation Requirement List		

		public static class AuthorisationRequirementCodes
		{
			#region SuppressResourceStringsCheckRegion
			// Codes used as storage values

			public const string MissingExRate = "Missing Exchange Rate";
			public const string NoApprovalRequired = "None";
			public const string FirstApprovalRequiredOnly = "1st Level Only";
			public const string SecondApprovalRequiredOnly = "2nd Level Only";
			public const string ThirdApprovalRequiredOnly = "3rd Level Only";
			public const string FourthApprovalRequiredOnly = "4th Level Only";
			public const string FifthApprovalRequiredOnly = "5th Level Only";
			public const string SixthApprovalRequiredOnly = "6th Level Only";
			public const string SeventhApprovalRequiredOnly = "7th Level Only";
			public const string EighthApprovalRequiredOnly = "8th Level Only";
			public const string NinthApprovalRequiredOnly = "9th Level Only";
			public const string TenthApprovalRequiredOnly = "10th Level Only";
			public const string EleventhApprovalRequiredOnly = "11th Level Only";
			public const string TwelfthApprovalRequiredOnly = "12th Level Only";
			public const string FirstAndSecondApprovalRequired = "1st and 2nd Level";
			public const string FirstAndThirdApprovalRequired = "1st and 3rd Level";
			public const string SecondAndThirdApprovalRequired = "2nd and 3rd Level";
			public const string AllThreeApprovalRequired = "1st, 2nd and 3rd Level";

			#endregion
		}

		public static class AuthorisationRequirementDescriptions
		{
			public static MultilingualString MissingExRate { get { return ResString.GetMultilingualString("AuthorisationRequirement|MissingExRate", AuthorisationRequirementCodes.MissingExRate); } }
			public static MultilingualString NoApprovalRequired { get { return ResString.GetMultilingualString("AuthorisationRequirement|NoApprovalRequired", AuthorisationRequirementCodes.NoApprovalRequired); } }
			public static MultilingualString FirstApprovalRequiredOnly { get { return ResString.GetMultilingualString("AuthorisationRequirement|FirstApprovalRequiredOnly", AuthorisationRequirementCodes.FirstApprovalRequiredOnly); } }
			public static MultilingualString SecondApprovalRequiredOnly { get { return ResString.GetMultilingualString("AuthorisationRequirement|SecondApprovalRequiredOnly", AuthorisationRequirementCodes.SecondApprovalRequiredOnly); } }
			public static MultilingualString ThirdApprovalRequiredOnly { get { return ResString.GetMultilingualString("AuthorisationRequirement|ThirdApprovalRequiredOnly", AuthorisationRequirementCodes.ThirdApprovalRequiredOnly); } }
			public static MultilingualString FourthApprovalRequiredOnly { get { return ResString.GetMultilingualString("AuthorisationRequirement|FourthApprovalRequiredOnly", AuthorisationRequirementCodes.FourthApprovalRequiredOnly); } }
			public static MultilingualString FifthApprovalRequiredOnly { get { return ResString.GetMultilingualString("AuthorisationRequirement|FifthApprovalRequiredOnly", AuthorisationRequirementCodes.FifthApprovalRequiredOnly); } }
			public static MultilingualString SixthApprovalRequiredOnly { get { return ResString.GetMultilingualString("AuthorisationRequirement|SixthApprovalRequiredOnly", AuthorisationRequirementCodes.SixthApprovalRequiredOnly); } }
			public static MultilingualString SeventhApprovalRequiredOnly { get { return ResString.GetMultilingualString("AuthorisationRequirement|SeventhApprovalRequiredOnly", AuthorisationRequirementCodes.SeventhApprovalRequiredOnly); } }
			public static MultilingualString EighthApprovalRequiredOnly { get { return ResString.GetMultilingualString("AuthorisationRequirement|EighthApprovalRequiredOnly", AuthorisationRequirementCodes.EighthApprovalRequiredOnly); } }
			public static MultilingualString NinthApprovalRequiredOnly { get { return ResString.GetMultilingualString("AuthorisationRequirement|NinthApprovalRequiredOnly", AuthorisationRequirementCodes.NinthApprovalRequiredOnly); } }
			public static MultilingualString TenthApprovalRequiredOnly { get { return ResString.GetMultilingualString("AuthorisationRequirement|TenthApprovalRequiredOnly", AuthorisationRequirementCodes.TenthApprovalRequiredOnly); } }
			public static MultilingualString EleventhApprovalRequiredOnly { get { return ResString.GetMultilingualString("AuthorisationRequirement|EleventhApprovalRequiredOnly", AuthorisationRequirementCodes.EleventhApprovalRequiredOnly); } }
			public static MultilingualString TwelfthApprovalRequiredOnly { get { return ResString.GetMultilingualString("AuthorisationRequirement|TwelfthApprovalRequiredOnly", AuthorisationRequirementCodes.TwelfthApprovalRequiredOnly); } }
			public static MultilingualString FirstAndSecondApprovalRequired { get { return ResString.GetMultilingualString("AuthorisationRequirement|FirstAndSecondApprovalRequired", AuthorisationRequirementCodes.FirstAndSecondApprovalRequired); } }
			public static MultilingualString FirstAndThirdApprovalRequired { get { return ResString.GetMultilingualString("AuthorisationRequirement|FirstAndThirdApprovalRequired", AuthorisationRequirementCodes.FirstAndThirdApprovalRequired); } }
			public static MultilingualString SecondAndThirdApprovalRequired { get { return ResString.GetMultilingualString("AuthorisationRequirement|SecondAndThirdApprovalRequired", AuthorisationRequirementCodes.SecondAndThirdApprovalRequired); } }
			public static MultilingualString AllThreeApprovalRequired { get { return ResString.GetMultilingualString("AuthorisationRequirement|AllThreeApprovalRequired", AuthorisationRequirementCodes.AllThreeApprovalRequired); } }
		}

		public CodeDescriptionPairList AuthorisationRequirementList
		{
			get
			{
				return fAuthorisationRequirementList ?? (fAuthorisationRequirementList = GetAuthorisationRequirementList());
			}
		}

		protected virtual CodeDescriptionPairList GetAuthorisationRequirementList()
		{
			CodeDescriptionPairList resultPairList = new CodeDescriptionPairList();
			resultPairList.AddPair(AuthorisationRequirementDescriptions.NoApprovalRequired);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.FirstApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.SecondApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.ThirdApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.FirstAndSecondApprovalRequired);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.FirstAndThirdApprovalRequired);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.SecondAndThirdApprovalRequired);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.AllThreeApprovalRequired);
			return resultPairList;
		}

		CodeDescriptionPairList fAuthorisationRequirementList;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Amount, Amount.ToString());
			writer.WriteElementString(Schema.Range, Range);
			writer.WriteElementString(Schema.AuthorisationRequirement, AuthorisationRequirement);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Amount = ZDecimal.Parse(reader.ReadElementString(Schema.Amount));
			Range = reader.ReadElementString(Schema.Range);
			AuthorisationRequirement = reader.ReadElementString(Schema.AuthorisationRequirement);
		}

		#endregion
	}
}
