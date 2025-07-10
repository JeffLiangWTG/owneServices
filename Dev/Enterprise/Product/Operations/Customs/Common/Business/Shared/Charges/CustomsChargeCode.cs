using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public class CustomsChargeCode : ICustomsChargeCode
	{
		public CustomsChargeCode(string code, MultilingualString description)
		{
			this.Code = Argument.NotNullOrEmpty(code, nameof(code));
			this.Description = Argument.NotNull(description, nameof(description));
			this.ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine;
		}

		#region Members

		public string Code { get; private set; }
		public MultilingualString Description { get; private set; }
		public bool IsDutiable { get; set; }
		public bool IsDutiableDeemedForThisCharge { get; set; }
		public bool IsVATible { get; set; }
		public bool IsVATibleDeemedForThisCharge { get; set; }

		#endregion

		public bool IsIncludedInITOTDeemedForThisCharge { get; set; }

		public bool? IsIncludedInITOTIfDeemed { get; set; }

		public bool IsPercentageApplicable { get; set; }
		public bool IsIncoTermNeutral { get; set; }
		public string DistributeBy { get; set; }
		public bool DistributeByDeemedForThisCharge { get; set; }

		public bool ConsiderIncotermWhenGroupChargeIsAppoorting { get; set; }
		public bool ConsiderIncotermWhenAppoorting { get; set; }

		public ChargeCodeChargeKey ChargeCodeChargeKey
		{
			get { return new ChargeCodeChargeKey(Code, IsDutiable, IsVATible, IsStatisticalValueApplicable); }
		}

		public bool IsStatisticalValueApplicable { get; set; }

		public bool IsStatisticalValueApplicableDeemed { get; set; }

		public ChargeParentTypes ParentTypes { get; set; }

		public ChargeCodeOperationType ChargeOperationType { get; set; }

		public bool IsForExport { get; set; }
	}

	public enum ChargeCodeOperationType { Added, Deducted }

	/// <summary>
	/// ChargeKey with IsIncludedInITOT flag
	/// </summary>
	public class MessageChargeKey
	{
		public MessageChargeKey(string chargeCode, bool isDutiable, bool isVATible, bool isIncludedInITOT, bool isStatisticalValueApplicable = false)
		{
			IsDutiable = isDutiable;
			IsVATible = isVATible;
			IsIncludedInITOT = isIncludedInITOT;
			ChargeCode = chargeCode;
			IsStatisticalValueApplicable = isStatisticalValueApplicable;
		}

		public ChargeCodeChargeKey ChargeKey
		{
			get { return new ChargeCodeChargeKey(ChargeCode, IsDutiable, IsVATible, IsStatisticalValueApplicable); }
		}

		public override bool Equals(object obj)
		{
			MessageChargeKey theOther = (MessageChargeKey)obj;

			return ChargeCode == theOther.ChargeCode &&
				IsDutiable == theOther.IsDutiable &&
				IsVATible == theOther.IsVATible &&
				IsStatisticalValueApplicable == theOther.IsStatisticalValueApplicable &&
				IsIncludedInITOT == theOther.IsIncludedInITOT;
		}

		public override int GetHashCode()
		{
			return ChargeCode.GetHashCode() ^ IsDutiable.GetHashCode() ^ IsVATible.GetHashCode() ^ IsStatisticalValueApplicable.GetHashCode() ^ IsIncludedInITOT.GetHashCode();
		}

		public override string ToString()
		{
			return ChargeCode + IsDutiable + IsVATible + IsStatisticalValueApplicable + IsIncludedInITOT;
		}

		public readonly bool IsDutiable;
		public readonly bool IsVATible;
		public readonly bool IsIncludedInITOT;
		public readonly string ChargeCode;
		public readonly bool IsStatisticalValueApplicable;
	}

	public class ChargeCodeChargeKey
	{
		public ChargeCodeChargeKey(string chargeCode, bool isDutiable, bool isVATible, bool isStatisticalValueApplicable = false)
		{
			ChargeCode = chargeCode;
			IsDutiable = isDutiable;
			IsVATible = isVATible;
			IsStatisticalValueApplicable = isStatisticalValueApplicable;
		}

		public readonly string ChargeCode;
		public readonly bool IsDutiable;
		public readonly bool IsVATible;
		public readonly bool IsStatisticalValueApplicable;

		public override string ToString()
		{
			return ChargeCode + IsDutiable + IsVATible + IsStatisticalValueApplicable;
		}

		public override bool Equals(object obj)
		{
			ChargeCodeChargeKey theOther = (ChargeCodeChargeKey)obj;

			return ChargeCode == theOther.ChargeCode &&
				IsDutiable == theOther.IsDutiable &&
				IsVATible == theOther.IsVATible &&
				IsStatisticalValueApplicable == theOther.IsStatisticalValueApplicable;
		}

		public override int GetHashCode()
		{
			return ChargeCode.GetHashCode() ^ IsDutiable.GetHashCode() ^ IsVATible.GetHashCode() ^ IsStatisticalValueApplicable.GetHashCode();
		}
	}

	public class GroupIsIncludedInLinesOptionList : CodeDescriptionPairList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public static class Codes
		{
			public const string Yes = "Yes";
			public const string No = "No";
			public const string NotApplicable = "N/A";
		}

		public static class Descriptions
		{
			public static string Yes
			{
				get { return Res.GetString("84d01037-f299-4597-a6c0-720723af114b", "Yes"); }
			}
			public static string No
			{
				get { return Res.GetString("e99ecea3-ccd6-467f-9fc2-d9b3abdf8331", "No"); }
			}
			public static string NotApplicable
			{
				get { return Res.GetString("6d30bf08-1ee5-41c0-8921-38998d7f60bb", "Not Applicable"); }
			}
		}

		public GroupIsIncludedInLinesOptionList()
		{
			AddPair(Codes.Yes, Descriptions.Yes);
			AddPair(Codes.No, Descriptions.No);
			AddPair(Codes.NotApplicable, Descriptions.NotApplicable);
		}
	}

	public class ApportionChargeKey
	{
		public ApportionChargeKey(string chargeCode, bool isDutiable, bool isVATible, string apportionType, string isIncludedInITOT, string isIncludedInInvoice, string distributeBy, decimal percentage, bool isAdjustedCharge, string chargeDesc, bool isSystem, bool isStatisticalValueApplicable = false)
			: this(new ChargeCodeChargeKey(chargeCode, isDutiable, isVATible, isStatisticalValueApplicable), apportionType, isIncludedInITOT, isIncludedInInvoice, distributeBy, percentage, isAdjustedCharge, chargeDesc, isSystem)
		{
		}

		public ApportionChargeKey(ChargeCodeChargeKey chargeKey, string apportionType, string isIncludedInITOT, string isIncludedInInvoice, string distributeBy, decimal percentage, bool isAdjustedCharge, string chargeDesc, bool isSystem)
		{
			this.ChargeKey = chargeKey;
			this.ApportionType = apportionType;
			this.IsIncludedInITOT = isIncludedInITOT;
			this.IsIncludedInInvoice = isIncludedInInvoice;
			this.DistributeBy = distributeBy;
			this.Percentage = percentage;
			this.IsAdjustedCharge = isAdjustedCharge;
			this.ChargeDesc = chargeDesc;
			this.IsSystem = isSystem;
		}

		public readonly ChargeCodeChargeKey ChargeKey;
		public readonly string ApportionType;
		public readonly string IsIncludedInITOT;
		public readonly string IsIncludedInInvoice;
		public readonly string DistributeBy;
		public readonly decimal Percentage;
		public readonly bool IsAdjustedCharge;
		public readonly string ChargeDesc;
		public readonly bool IsSystem;

		public bool IsFullApportionment
		{
			get { return ApportionType == ApportionmentTypeList.Codes.FullApportionment; }
		}

		public override bool Equals(object obj)
		{
			ApportionChargeKey theOther = (ApportionChargeKey)obj;

			return ChargeKey.Equals(theOther.ChargeKey) &&
				ApportionType == theOther.ApportionType &&
				DistributeBy == theOther.DistributeBy &&
				Percentage == theOther.Percentage &&
				IsSystem == theOther.IsSystem &&
				IsAdjustedCharge == theOther.IsAdjustedCharge &&
				EqualsForIsIncludedInITOT(theOther) &&
				EqualsForIsIncludedInInvoice(theOther) &&
				EqualsForChargeDesc(theOther);
		}

		bool EqualsForIsIncludedInITOT(ApportionChargeKey theOther)
		{
			return IsIncludedInITOT == GroupIsIncludedInLinesOptionList.Codes.NotApplicable ||
				theOther.IsIncludedInITOT == GroupIsIncludedInLinesOptionList.Codes.NotApplicable ||
				IsIncludedInITOT == theOther.IsIncludedInITOT;
		}

		bool EqualsForIsIncludedInInvoice(ApportionChargeKey theOther)
		{
			return IsIncludedInInvoice == GroupIsIncludedInLinesOptionList.Codes.NotApplicable ||
				theOther.IsIncludedInInvoice == GroupIsIncludedInLinesOptionList.Codes.NotApplicable ||
				IsIncludedInInvoice == theOther.IsIncludedInInvoice;
		}

		bool EqualsForChargeDesc(ApportionChargeKey theOther)
		{
			return string.Compare(ChargeDesc, theOther.ChargeDesc, true) == 0;
		}

		public override int GetHashCode()
		{
			return ChargeKey.GetHashCode() ^ ApportionType.GetHashCode() ^ DistributeBy.GetHashCode() ^ Percentage.GetHashCode() ^ IsSystem.GetHashCode() ^ IsAdjustedCharge.GetHashCode() ^ ChargeDesc.ToUpper().GetHashCode();
		}

		public override string ToString()
		{
			return ChargeKey.ToString() + ApportionType + IsIncludedInITOT + IsIncludedInInvoice + DistributeBy + Percentage + IsAdjustedCharge + IsSystem;
		}
	}
}
