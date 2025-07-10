using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class TransactionNumberSequenceCustomisation : RegistryBusinessObjectTemplate
	{
		public TransactionNumberSequenceCustomisation()
		{
		}

		public TransactionNumberSequenceCustomisation(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public static class Schema
		{
			public const string ElementName = "ElementName";
			public const string Include = "Include";
			public const string Order = "Order";
			public const string Code = "Code";
			public const string Length = "Length";
			public const string Fountain = "Fountain";
			public const string Description = "Description";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded element name")]
		public static class ElementNames
		{
			public const string TransactionHeaderBranchCode = "Transaction Header Branch Code";
			public const string TransactionHeaderDepartmentCode = "Transaction Header Department Code";
			public const string JobHeaderBranchCode = "Job Header Branch Code";
			public const string JobHeaderDepartmentCode = "Job Header Department Code";
			public const string CustomElement1 = "Custom Element 1";
			public const string CustomElement2 = "Custom Element 2";
			public const string SequenceNumber = "Sequence Number";
			public const string YearAsDigits = "Year as Digit(s)";
			public const string YearAsLetter = "Year as Letter";
			public const string MonthAs2Digits = "Month as 2 Digits";
			public const string MonthAsLetter = "Month as Letter";
			public const string AccountingYearAsDigits = "Accounting Year as Digit(s)";
			public const string AccountingYearAsLetter = "Accounting Year as Letter";
			public const string AccountingPeriodAs2Digits = "Accounting Period as 2 Digits";
			public const string TaxAndNonTax = "Tax/Non-Tax";
			public const string SelfBillingAndStandard = "Self-Billing/Standard";
			public const string CorrectedAndOriginal = "Correction/Original";
			public const string TransactionTypePrefix = "Transaction Type Prefix";
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOrder();
			ValidateCode();
			ValidateLength();
		}

		protected virtual bool IsYearElement => ElementName == ElementNames.AccountingYearAsDigits || ElementName == ElementNames.YearAsDigits;

		#region Properties

		#region ElementName

		ZString fElementName;

		[MaxLength(100)]
		[ReadOnly(true)]
		public virtual ZString ElementName
		{
			get { return fElementName; }
			set { SetNonPersistentPropertyValue(ElementNameInfo, ref fElementName, value); }
		}

		public virtual ZPropertyInfo ElementNameInfo
		{
			get { return GetZPropertyInfo(Schema.ElementName); }
		}

		#endregion

		#region Description

		public virtual ZString Description
		{
			get
			{
				string result = null;
				switch (ElementName)
				{
					case ElementNames.YearAsDigits:
						result = Res.GetString("FCA69804-C204-4fef-9C01-2F85C95F3AB4", "Year as 1, 2 or 4 digits. For example: as a single digit(2005 = 5, 2010 = 0), as a double digit(2005 = 05, 2010 = 10) and as 4 digits(2005 = 2005)");
						break;
					case ElementNames.YearAsLetter:
						result = Res.GetString("3B447A86-F3F3-48fb-8C4D-4B5CE05B29F5", "2001 = A, 2002 = B ... 2026 = Z");
						break;
					case ElementNames.MonthAs2Digits:
						result = Res.GetString("99D36817-C9E5-417e-8286-39E97F5972A5", "Month as 2 digits, JAN = 01, FEB = 02 ... DEC = 12");
						break;
					case ElementNames.MonthAsLetter:
						result = Res.GetString("86BE69A0-2589-4b01-B88E-68BBAEC150DB", "Jan = A, Feb = B ... Dec = L");
						break;
					case ElementNames.AccountingYearAsDigits:
						result = Res.GetString("F573DCAE-DA25-45ae-9E2D-E4B2C96E6A66", "Accounting Year as 1, 2 or 4 digits. For example: as a single digit(2005 = 5, 2010 = 0), as a double digit(2005 = 05, 2010 = 10) and as 4 digits(2005 = 2005)");
						break;
					case ElementNames.AccountingYearAsLetter:
						result = Res.GetString("4B9FCF31-C8FC-468a-B644-D6C133B8E381", "2001 = A, 2002 = B ... 2026 = Z");
						break;
					case ElementNames.AccountingPeriodAs2Digits:
						result = Res.GetString("3A13ECA8-CFF1-4611-8501-2B4AD97D3B95", "Accounting Period as 2 digits, 201701 = 01, 201702 = 02 ... 201712 = 12");
						break;
					case ElementNames.TaxAndNonTax:
						result = Res.GetString("605916E6-9639-4288-A8C8-2959A484D130", "1-3 character length alphanumeric codes for Tax and Non-Tax transactions separated by '/'");
						break;
					case ElementNames.SelfBillingAndStandard:
						result = Res.GetString("ECB07103-B062-41CF-BEBF-0CD0461D334C", "1-3 character length alphanumeric codes for Self-Billing and Standard transactions separated by '/'");
						break;
					case ElementNames.CorrectedAndOriginal:
						result = Res.GetString("8BFC1B5F-F234-4E34-8984-DBBAB5124767", "1-3 character length alphanumeric codes for Amended/Reversed and Original transactions separated by '/'");
						break;
				}
				return result;
			}
		}

		#endregion

		#region Include

		ZBool fInclude;
		public virtual ZBool Include
		{
			get
			{
				return fInclude;
			}
			set
			{
				SetNonPersistentPropertyValue(IncludeInfo, ref fInclude, value);
				if (IsValidationSuspended)
				{
				}
				else
				{
					ValidateInclude();
				}
				if (Include == ZBool.False)
				{
					SetOtherValuesOnIncludeChanged();
				}
			}
		}

		public virtual ZPropertyInfo IncludeInfo
		{
			get { return GetZPropertyInfo(Schema.Include); }
		}

		protected virtual void ValidateInclude()
		{
			ValidateOrder();
		}

		protected bool Include_ReadOnly
		{
			get { return ElementName == ElementNames.SequenceNumber; }
		}

		void SetOtherValuesOnIncludeChanged()
		{
			Code = IsYearElement ? new ZString("4") : ZString.Empty;
			Order = 0;
			Fountain = false;
		}

		#endregion

		#region Order

		ZByte fOrder;
		public virtual ZByte Order
		{
			get
			{
				return fOrder;
			}
			set
			{
				SetNonPersistentPropertyValue(OrderInfo, ref fOrder, value);
				if (!IsValidationSuspended)
				{
					ValidateOrder();
				}
			}
		}

		public virtual ZPropertyInfo OrderInfo
		{
			get { return GetZPropertyInfo(Schema.Order); }
		}

		void ValidateOrder()
		{
			OrderInfo.ClearAllNotifications();

			if (Include)
			{
				MandatoryValidation.CheckNotZero(OrderInfo);

				if (ParentCollections.Count > 0 && !OrderInfo.HasErrors())
				{
					foreach (TransactionNumberSequenceCustomisation element in ParentCollections.First())
					{
						if (!element.Include)
						{
							element.OrderInfo.ClearAllNotifications();
						}
					}

					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(OrderInfo, (IMultilingualString)ResString.GetMultilingualString("532e3baf-30ec-410a-aab3-9f1ad3ae1e25", "Order must be unique."));
				}
			}
		}

		protected bool Order_ReadOnly
		{
			get { return !Include; }
		}

		#endregion

		#region Code

		ZString fCode;
		[MaxLength(nameof(MaxLengthOfCodeField))]
		public virtual ZString Code
		{
			get
			{
				return fCode;
			}
			set
			{
				var formattedValue = value;

				if (IsYearElement)
				{
					formattedValue = value.Trim();
				}

				SetNonPersistentPropertyValue(CodeInfo, ref fCode, formattedValue);

				if (ShouldExpectCodePair)
				{
					InitializeCodePair();
				}

				if (!IsValidationSuspended)
				{
					ValidateCode();
				}

				if (IsYearElement)
				{
					Length = CodeInfo.HasErrors() ? ZInt.Zero : ZInt.ParseSafe(Code, 0);
				}
				else if (ShouldExpectCodePair)
				{
					Length = IsCodePairValid ? (ZInt)Math.Max(codePair.Item1.Length, codePair.Item2.Length) : ZInt.Zero;
				}
			}
		}

		int MaxLengthOfCodeField => ShouldExpectCodePair ? MaxLengthOfCodeFieldForCodePairElement : DefaultMaxLengthOfCodeField;
		readonly int DefaultMaxLengthOfCodeField = 5;
		readonly int MaxLengthOfCodeFieldForCodePairElement = 7;

		public virtual ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();

			if (Include)
			{
				if (IsYearElement)
				{
					var length = ZInt.ParseSafe(Code, 0);
					if (length != 1 && length != 2 && length != 4)
					{
						CodeInfo.AddError(Res.GetString("C25AF556-D1B7-45f9-9EB0-ECD192D26DAB", "The year as digit length should be either 1, 2 or 4."));
					}
				}
				else if (ShouldExpectCodePair)
				{
					ValidateCodePair();
				}
			}
		}

		#region Code Pair

		void InitializeCodePair()
		{
			var splittedCodes = Code.Split('/');
			if (splittedCodes.Length == 2 && !string.IsNullOrEmpty(splittedCodes[0]) && !string.IsNullOrEmpty(splittedCodes[1]))
			{
				codePair = new Tuple<ZString, ZString>(splittedCodes[0], splittedCodes[1]);
			}
			else
			{
				codePair = null;
			}
		}

		void ValidateCodePair()
		{
			if (codePair == null)
			{
				CodeInfo.AddError(Res.GetString("141DC540-FF15-4060-B093-889A31A18066", "Invalid code. Please enter exactly two non-empty codes for {0} separated by '/'", ElementName));
			}
			else
			{
				ValidateValueInCodePair(codePair.Item1, true);
				ValidateValueInCodePair(codePair.Item2, false);
				CheckForSameValuesInCodePair();
			}

			void ValidateValueInCodePair(ZString valueInCodePair, bool isFirstElement)
			{
				if (!valueInCodePair.IsLettersAndNumbersOnlyOrEmpty)
				{
					CodeInfo.AddError(Res.GetString("3D4DE723-CDBC-4792-A261-45C149171854", "The code to the {0} of the '/' is invalid. Please enter alphanumeric characters only.", isFirstElement ? (NoResString)"left" : (NoResString)"right"));
				}
			}
		}

		void CheckForSameValuesInCodePair()
		{
			if (codePair.Item1.Equals(codePair.Item2))
			{
				CodeInfo.AddError(Res.GetString("DF2F72F2-810A-422B-AF53-271349931092", "The codes on either side of the '/' must be different values."));
			}
		}

		public ZString Code1
		{
			get
			{
				var result = ZString.Empty;

				if (ShouldExpectCodePair && IsCodePairValid)
				{
					result = codePair.Item1;
				}

				return result;
			}
		}

		public ZString Code2
		{
			get
			{
				var result = ZString.Empty;

				if (ShouldExpectCodePair && IsCodePairValid)
				{
					result = codePair.Item2;
				}

				return result;
			}
		}

		bool IsCodePairValid
		{
			get
			{
				return codePair != null && !CodeInfo.HasErrors();
			}
		}

		Tuple<ZString, ZString> codePair;

		bool ShouldExpectCodePair
		{
			get
			{
				return (ElementNamesExpectingCodePair.Contains(ElementName.ToString()));
			}
		}

		string[] ElementNamesExpectingCodePair => new[] { ElementNames.TaxAndNonTax, ElementNames.SelfBillingAndStandard, ElementNames.CorrectedAndOriginal };

		#endregion

		protected virtual bool Code_ReadOnly
		{
			get
			{
				return !Include ||
				  (ElementName != ElementNames.CustomElement1 &&
				   ElementName != ElementNames.CustomElement2 &&
				   ElementName != ElementNames.YearAsDigits &&
				   ElementName != ElementNames.AccountingYearAsDigits &&
				   ElementName != ElementNames.TaxAndNonTax &&
				   ElementName != ElementNames.SelfBillingAndStandard &&
				   ElementName != ElementNames.CorrectedAndOriginal);
			}
		}

		#endregion

		#region Length

		ZInt fLength;
		public virtual ZInt Length
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "This method should not be split. One may refactor it with strategies.")]
			get
			{
				ZInt result;

				switch (ElementName)
				{
					case ElementNames.TransactionHeaderBranchCode:
					case ElementNames.JobHeaderBranchCode:
						result = GlbBranchSchema.GB_Code.MaxLength;
						break;
					case ElementNames.TransactionHeaderDepartmentCode:
					case ElementNames.JobHeaderDepartmentCode:
						result = GlbDepartmentSchema.GE_Code.MaxLength;
						break;
					case ElementNames.CustomElement1:
					case ElementNames.CustomElement2:
						result = Code.Length;
						break;
					case ElementNames.MonthAsLetter:
					case ElementNames.YearAsLetter:
					case ElementNames.AccountingYearAsLetter:
						result = 1;
						break;
					case ElementNames.MonthAs2Digits:
					case ElementNames.AccountingPeriodAs2Digits:
						result = 2;
						break;
					case ElementNames.TransactionTypePrefix:
						result = GetTransactionTypePrefixLength();
						break;
					default:
						result = fLength;
						break;
				}

				return result;
			}
			set
			{
				SetNonPersistentPropertyValue(LengthInfo, ref fLength, value);
				if (!IsValidationSuspended)
				{
					ValidateLength();
				}
			}
		}

		public virtual ZPropertyInfo LengthInfo
		{
			get { return GetZPropertyInfo(Schema.Length); }
		}

		protected bool Length_ReadOnly
		{
			get { return !Include || ElementName != ElementNames.SequenceNumber; }
		}

		ZInt GetTransactionTypePrefixLength()
		{
			var result = 0;

			if (CurrentFallbackLevel != null)
			{
				var transactionTypePrefixs = AccountingConfigurationRegistry.Instance.TransactionTypePrefix.GetFallBackValueAtAllLevels(CurrentFallbackLevel.CompanyPK(false), CurrentFallbackLevel.BranchPK, CurrentFallbackLevel.DepartmentPK);
				if (transactionTypePrefixs.Count > 0)
				{
					result = transactionTypePrefixs.Cast<TransactionTypePrefix>().Select(x => x.Prefix.Length).Max();
				}
			}

			return result;
		}

		public virtual void ValidateLength()
		{
			LengthInfo.ClearAllNotifications();

			if (Include)
			{
				switch (ElementName)
				{
					case ElementNames.AccountingYearAsDigits:
					case ElementNames.YearAsDigits:
						CompareValidation.CheckWithinRange(LengthInfo, 1, 4);
						break;
					case ElementNames.SequenceNumber:
						CompareValidation.CheckWithinRange(LengthInfo, 5, 8);
						break;
					case ElementNames.TaxAndNonTax:
					case ElementNames.SelfBillingAndStandard:
					case ElementNames.CorrectedAndOriginal:
						CompareValidation.CheckWithinRange(LengthInfo, 1, 3);
						break;
					default:
						break;
				}
			}
		}

		#endregion

		#region Fountain

		ZBool fFountain;
		public virtual ZBool Fountain
		{
			get
			{
				return fFountain;
			}
			set
			{
				SetNonPersistentPropertyValue(FountainInfo, ref fFountain, value);
				if (IsValidationSuspended)
				{
				}
				else
				{
					ValidateFountain();
				}
			}
		}
		public virtual ZPropertyInfo FountainInfo
		{
			get { return GetZPropertyInfo(Schema.Fountain); }
		}

		protected virtual void ValidateFountain()
		{
			ValidateOrder();
		}

		protected virtual bool Fountain_ReadOnly
		{
			get
			{
				return !Include ||
						ElementName == ElementNames.SequenceNumber ||
						ElementName == ElementNames.TransactionTypePrefix;
			}
		}

		#endregion

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TransactionNumberSequenceCustomisation(fallbackLevel);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ElementName = reader.ReadElementString(Schema.ElementName);
			Include = reader.ReadElementStringAsZBool(Schema.Include);
			Order = ZByte.ParseSafe(reader.ReadElementString(Schema.Order), 0);
			Code = reader.ReadElementString(Schema.Code);
			Length = reader.ReadElementStringAsZInt(Schema.Length);
			Fountain = reader.ReadElementStringAsZBool(Schema.Fountain);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ElementName, ElementName.ToString());
			writer.WriteElementString(Schema.Include, Include.ToString());
			writer.WriteElementString(Schema.Order, Order.ToString());
			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Length, Length.ToString());
			writer.WriteElementString(Schema.Fountain, Fountain.ToString());
		}

		#endregion
	}
}
