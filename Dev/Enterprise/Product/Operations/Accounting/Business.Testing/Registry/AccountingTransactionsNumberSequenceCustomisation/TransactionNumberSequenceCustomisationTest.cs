using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(TransactionNumberSequenceCustomisation))]
	public class TransactionNumberSequenceCustomisationTest : RegistryBusinessObjectTemplateTestCase<TransactionNumberSequenceCustomisation>
	{
		public void TestIncludeIsReadOnly()
		{
			Assert(GetBusinessObjectToClone().IncludeInfo.ReadOnly);
		}

		public void TestFountainOfTransactionTypePrefixIsReadOnly()
		{
			Assert(elementTransactionTypePrefix.FountainInfo.ReadOnly);
		}

		public void TestSetOtherValuesOnIncludeChanged()
		{
			var collection = new TransactionNumberSequenceCustomisationCollection();
			collection.Add(new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax });
			collection.Add(new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard });
			collection.Add(new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal });
			var bizObjToClone = GetBusinessObjectToClone();
			collection.Add(bizObjToClone);

			foreach (var currentTransactionNumberSequenceCustomisation in collection.Cast<TransactionNumberSequenceCustomisation>())
			{
				currentTransactionNumberSequenceCustomisation.Include = true;

				currentTransactionNumberSequenceCustomisation.Code = "XY/Z";
				currentTransactionNumberSequenceCustomisation.Order = 1;
				currentTransactionNumberSequenceCustomisation.Fountain = true;
				if (currentTransactionNumberSequenceCustomisation != bizObjToClone)
				{
					AssertEquals(2, currentTransactionNumberSequenceCustomisation.Length);
				}

				currentTransactionNumberSequenceCustomisation.Include = false;

				AssertEquals("", currentTransactionNumberSequenceCustomisation.Code);
				AssertEquals(ZByte.Zero, currentTransactionNumberSequenceCustomisation.Order);
				AssertEquals(false, currentTransactionNumberSequenceCustomisation.Fountain);
				AssertEquals(0, currentTransactionNumberSequenceCustomisation.Length);
			}
		}

		public void TestMaxLength()
		{
			RunValidationOnAllElements();

			AssertEquals(0, elementSnNum.Length);
			AssertHasError(elementSnNum.LengthInfo, "Please enter a 'Length' within the range 5 to 8.");
			elementSnNum.Length = 9;
			AssertEquals(9, elementSnNum.Length);
			AssertHasError(elementSnNum.LengthInfo, "Please enter a 'Length' within the range 5 to 8.");
			elementSnNum.Length = 6;
			AssertEquals(6, elementSnNum.Length);
			AssertNoErrors(elementSnNum.LengthInfo);

			AssertEquals(0, elementElm1.Length);
			AssertNoErrors(elementElm1.LengthInfo);
			elementElm1.Code = "XXX";
			AssertEquals(3, elementElm1.Length);
			AssertNoErrors(elementElm1.LengthInfo);

			AssertEquals(0, elementElm2.Length);
			AssertNoErrors(elementElm2.LengthInfo);
			elementElm2.Code = "XXX";
			AssertEquals(3, elementElm2.Length);
			AssertNoErrors(elementElm2.LengthInfo);

			AssertEquals(0, elementAccYearDigits.Length);
			AssertHasError(elementAccYearDigits.LengthInfo, "Please enter a 'Length' within the range 1 to 4.");
			elementAccYearDigits.Code = "5";
			AssertEquals(0, elementAccYearDigits.Length);
			AssertHasError(elementAccYearDigits.LengthInfo, "Please enter a 'Length' within the range 1 to 4.");
			elementAccYearDigits.Code = "2";
			AssertEquals(2, elementAccYearDigits.Length);
			AssertNoErrors(elementAccYearDigits.LengthInfo);

			AssertEquals(2, elementAccPeriod.Length);
			AssertNoErrors(elementAccPeriod.LengthInfo);

			AssertEquals(1, elementAccYearLetter.Length);
			AssertNoErrors(elementAccYearLetter.LengthInfo);

			AssertEquals(0, elementYearDigits.Length);
			AssertHasError(elementYearDigits.LengthInfo, "Please enter a 'Length' within the range 1 to 4.");
			elementYearDigits.Code = "5";
			AssertEquals(0, elementYearDigits.Length);
			AssertHasError(elementYearDigits.LengthInfo, "Please enter a 'Length' within the range 1 to 4.");
			elementYearDigits.Code = "2";
			AssertEquals(2, elementYearDigits.Length);
			AssertNoErrors(elementYearDigits.LengthInfo);

			AssertEquals(1, elementYearLetter.Length);
			AssertNoErrors(elementYearLetter.LengthInfo);

			AssertEquals(2, elementMonDigits.Length);
			AssertNoErrors(elementMonDigits.LengthInfo);

			AssertEquals(1, elementMonLetter.Length);
			AssertNoErrors(elementMonLetter.LengthInfo);

			AssertEquals(GlbBranchSchema.GB_Code.MaxLength, elementJobBrn.Length);
			AssertNoErrors(elementJobBrn.LengthInfo);

			AssertEquals(GlbDepartmentSchema.GE_Code.MaxLength, elementJobDpt.Length);
			AssertNoErrors(elementJobDpt.LengthInfo);

			AssertEquals(GlbBranchSchema.GB_Code.MaxLength, elementTransBrn.Length);
			AssertNoErrors(elementTransBrn.LengthInfo);

			AssertEquals(GlbDepartmentSchema.GE_Code.MaxLength, elementTransDpt.Length);
			AssertNoErrors(elementTransDpt.LengthInfo);

			AssertMaxLengthForCodePairElement(elementTaxAndNonTax);
			AssertMaxLengthForCodePairElement(elementSelfBillingAndStandard);
			AssertMaxLengthForCodePairElement(elementCorrectedAndOriginal);
		}

		void AssertMaxLengthForCodePairElement(TransactionNumberSequenceCustomisation element)
		{
			AssertEquals(0, element.Length);
			AssertHasError(element.LengthInfo, "Please enter a 'Length' within the range 1 to 3.");
			element.Length = 5;
			AssertEquals(5, element.Length);
			AssertHasError(element.LengthInfo, "Please enter a 'Length' within the range 1 to 3.");
			element.Length = 3;
			AssertEquals(3, element.Length);
			AssertNoErrors(element.LengthInfo);
		}

		public void TestLengthForCodePairElements()
		{
			AssertLengthForCodePairElement(elementTaxAndNonTax);
			AssertLengthForCodePairElement(elementSelfBillingAndStandard);
			AssertLengthForCodePairElement(elementCorrectedAndOriginal);
		}

		void AssertLengthForCodePairElement(TransactionNumberSequenceCustomisation element)
		{
			element.Code = "XY/Z";
			AssertEquals(2, element.Length);
			element.Code = "X/Y/Z";
			AssertEquals(0, element.Length);
			element.Code = "XY/Z";
			AssertEquals(2, element.Length);
			element.Code = "@/~";
			AssertEquals(0, element.Length);
		}

		public void TestMaxLengthOfCodeField()
		{
			AssertEquals(7, elementTaxAndNonTax.CodeInfo.MaxLength);
			AssertEquals(7, elementSelfBillingAndStandard.CodeInfo.MaxLength);
			AssertEquals(7, elementCorrectedAndOriginal.CodeInfo.MaxLength);
			AssertEquals(5, elementTransBrn.CodeInfo.MaxLength);
			AssertEquals(5, elementTransDpt.CodeInfo.MaxLength);
			AssertEquals(5, elementElm1.CodeInfo.MaxLength);
			AssertEquals(5, elementElm2.CodeInfo.MaxLength);
			AssertEquals(5, elementAccYearDigits.CodeInfo.MaxLength);
			AssertEquals(5, elementAccPeriod.CodeInfo.MaxLength);
			AssertEquals(5, elementAccYearLetter.CodeInfo.MaxLength);
			AssertEquals(5, elementYearDigits.CodeInfo.MaxLength);
			AssertEquals(5, elementYearLetter.CodeInfo.MaxLength);
			AssertEquals(5, elementMonDigits.CodeInfo.MaxLength);
			AssertEquals(5, elementMonLetter.CodeInfo.MaxLength);
			AssertEquals(5, elementJobBrn.CodeInfo.MaxLength);
			AssertEquals(5, elementJobDpt.CodeInfo.MaxLength);
		}

		public void TestTransactionTypePrefixLength()
		{
			var prefixCollection = new TransactionTypePrefixCollection();
			var prefix1 = prefixCollection.AddNew();
			prefix1.Ledger = "AP";
			prefix1.TransactionType = "JNL";
			prefix1.Prefix = "XX";

			var prefix2 = prefixCollection.AddNew();
			prefix2.Ledger = "AR";
			prefix2.TransactionType = "PAY";
			prefix2.Prefix = "1";

			using (AccountingConfigurationRegistry.Instance.TransactionTypePrefix.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, prefixCollection))
			{
				var transactionTypePrefixElement = AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).Cast<TransactionNumberSequenceCustomisation>().FirstOrDefault(x => x.ElementName == TransactionNumberSequenceCustomisation.ElementNames.TransactionTypePrefix);
				transactionTypePrefixElement.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

				AssertEquals(2, transactionTypePrefixElement.Length);
			}
		}

		public void TestCode1AndCode2()
		{
			AssertCode1AndCode2ForCodePairElement(elementTaxAndNonTax);
			AssertCode1AndCode2ForCodePairElement(elementSelfBillingAndStandard);
			AssertCode1AndCode2ForCodePairElement(elementCorrectedAndOriginal);

			AssertCode1AndCode2ForNonCodePairElement(elementTransBrn);
			AssertCode1AndCode2ForNonCodePairElement(elementTransDpt);
			AssertCode1AndCode2ForNonCodePairElement(elementElm1);
			AssertCode1AndCode2ForNonCodePairElement(elementElm2);
			AssertCode1AndCode2ForNonCodePairElement(elementAccYearDigits);
			AssertCode1AndCode2ForNonCodePairElement(elementAccPeriod);
			AssertCode1AndCode2ForNonCodePairElement(elementAccYearLetter);
			AssertCode1AndCode2ForNonCodePairElement(elementYearDigits);
			AssertCode1AndCode2ForNonCodePairElement(elementYearLetter);
			AssertCode1AndCode2ForNonCodePairElement(elementMonDigits);
			AssertCode1AndCode2ForNonCodePairElement(elementJobBrn);
			AssertCode1AndCode2ForNonCodePairElement(elementTransBrn);
			AssertCode1AndCode2ForNonCodePairElement(elementJobDpt);
		}

		void AssertCode1AndCode2ForCodePairElement(TransactionNumberSequenceCustomisation element)
		{
			element.Code = "XYZ/ABC";
			AssertEquals("XYZ", element.Code1);
			AssertEquals("ABC", element.Code2);

			element.Code = "XY/Z/AB";
			AssertEquals(ZString.Empty, element.Code1);
			AssertEquals(ZString.Empty, element.Code2);

			element.Code = "XY/#";
			AssertEquals(ZString.Empty, element.Code1);
			AssertEquals(ZString.Empty, element.Code2);

			element.Code = "XY/ZZZZ";
			AssertEquals("XY", element.Code1);
			AssertEquals("ZZZZ", element.Code2);

			element.Code = "XYZ";
			AssertEquals(ZString.Empty, element.Code1);
			AssertEquals(ZString.Empty, element.Code2);
		}

		void AssertCode1AndCode2ForNonCodePairElement(TransactionNumberSequenceCustomisation element)
		{
			element.Code = "XY/AB";
			AssertEquals(ZString.Empty, element.Code1);
			AssertEquals(ZString.Empty, element.Code2);

			element.Code = "X/Z/A";
			AssertEquals(ZString.Empty, element.Code1);
			AssertEquals(ZString.Empty, element.Code2);

			element.Code = "XY/#";
			AssertEquals(ZString.Empty, element.Code1);
			AssertEquals(ZString.Empty, element.Code2);

			element.Code = "XYZ";
			AssertEquals(ZString.Empty, element.Code1);
			AssertEquals(ZString.Empty, element.Code2);
		}

		public void TestIncludeDefaultValueForYearElements()
		{
			elementAccYearDigits.Include = true;
			elementAccYearDigits.Code = "1";
			AssertEquals("1", elementAccYearDigits.Code);
			AssertEquals(1, elementAccYearDigits.Length);

			elementAccYearDigits.Include = false;
			AssertEquals("4", elementAccYearDigits.Code);
			AssertEquals(4, elementAccYearDigits.Length);
		}

		public void TestCodeReadOnly()
		{
			AssertEquals(true, elementSnNum.Include);
			AssertEquals(true, elementAccPeriod.Include);
			AssertEquals(true, elementAccYearLetter.Include);
			AssertEquals(true, elementYearLetter.Include);
			AssertEquals(true, elementMonDigits.Include);
			AssertEquals(true, elementMonLetter.Include);
			AssertEquals(true, elementJobBrn.Include);
			AssertEquals(true, elementJobDpt.Include);
			AssertEquals(true, elementTransBrn.Include);
			AssertEquals(true, elementTransDpt.Include);
			AssertEquals(true, elementElm1.Include);
			AssertEquals(true, elementElm2.Include);
			AssertEquals(true, elementAccYearDigits.Include);
			AssertEquals(true, elementYearDigits.Include);

			AssertEquals(true, elementSnNum.CodeInfo.ReadOnly);
			AssertEquals(true, elementAccPeriod.CodeInfo.ReadOnly);
			AssertEquals(true, elementAccYearLetter.CodeInfo.ReadOnly);
			AssertEquals(true, elementYearLetter.CodeInfo.ReadOnly);
			AssertEquals(true, elementMonDigits.CodeInfo.ReadOnly);
			AssertEquals(true, elementMonLetter.CodeInfo.ReadOnly);
			AssertEquals(true, elementJobBrn.CodeInfo.ReadOnly);
			AssertEquals(true, elementJobDpt.CodeInfo.ReadOnly);
			AssertEquals(true, elementTransBrn.CodeInfo.ReadOnly);
			AssertEquals(true, elementTransDpt.CodeInfo.ReadOnly);
			AssertEquals(true, elementTransactionTypePrefix.CodeInfo.ReadOnly);

			AssertEquals(false, elementElm1.CodeInfo.ReadOnly);
			AssertEquals(false, elementElm2.CodeInfo.ReadOnly);
			AssertEquals(false, elementAccYearDigits.CodeInfo.ReadOnly);
			AssertEquals(false, elementYearDigits.CodeInfo.ReadOnly);
			AssertEquals(false, elementTaxAndNonTax.CodeInfo.ReadOnly);
			AssertEquals(false, elementSelfBillingAndStandard.CodeInfo.ReadOnly);
			AssertEquals(false, elementCorrectedAndOriginal.CodeInfo.ReadOnly);
		}

		public virtual void TestCode()
		{
			RunValidationOnAllElements();

			AssertYearCode(elementYearDigits);
			AssertYearCode(elementAccYearDigits);

			elementElm1.Code = "AB34";
			AssertNoErrors(elementElm1.CodeInfo);

			elementElm2.Code = "AB34";
			AssertNoErrors(elementElm2.CodeInfo);

			AssertExceptionThrown<MaxLengthExceededException>(() => elementElm1.Code = "AB3456");
			ExceptionReporterTestListener.Instance.Clear();

			AssertCodeForElement(elementTaxAndNonTax);
			AssertCodeForElement(elementSelfBillingAndStandard);
			AssertCodeForElement(elementCorrectedAndOriginal);
		}

		void AssertCodeForElement(TransactionNumberSequenceCustomisation element)
		{
			element.Code = "XY/Z";
			AssertNoErrors(element.CodeInfo);
			element.Code = "X/Y/Z";
			AssertHasError(element.CodeInfo, $"Invalid code. Please enter exactly two non-empty codes for {element.ElementName} separated by '/'");
			element.Code = "/YZ";
			AssertHasError(element.CodeInfo, $"Invalid code. Please enter exactly two non-empty codes for {element.ElementName} separated by '/'");
			element.Code = "XY/";
			AssertHasError(element.CodeInfo, $"Invalid code. Please enter exactly two non-empty codes for {element.ElementName} separated by '/'");
			element.Code = "";
			AssertHasError(element.CodeInfo, $"Invalid code. Please enter exactly two non-empty codes for {element.ElementName} separated by '/'");

			element.Code = "@/~";
			AssertHasError(element.CodeInfo, "The code to the left of the '/' is invalid. Please enter alphanumeric characters only.");
			AssertHasError(element.CodeInfo, "The code to the right of the '/' is invalid. Please enter alphanumeric characters only.");
			AssertEquals(2, element.CodeInfo.GetErrors().Count());
			element.Code = "@/Z";
			AssertHasError(element.CodeInfo, "The code to the left of the '/' is invalid. Please enter alphanumeric characters only.");
			AssertEquals(1, element.CodeInfo.GetErrors().Count());
			element.Code = "X/~";
			AssertHasError(element.CodeInfo, "The code to the right of the '/' is invalid. Please enter alphanumeric characters only.");
			AssertEquals(1, element.CodeInfo.GetErrors().Count());
			element.Code = "XY/XY";
			AssertHasError(element.CodeInfo, "The codes on either side of the '/' must be different values.");
			AssertEquals(1, element.CodeInfo.GetErrors().Count());
			element.Code = "!/!";
			AssertHasError(element.CodeInfo, "The code to the left of the '/' is invalid. Please enter alphanumeric characters only.");
			AssertHasError(element.CodeInfo, "The code to the right of the '/' is invalid. Please enter alphanumeric characters only.");
			AssertHasError(element.CodeInfo, "The codes on either side of the '/' must be different values.");
			AssertEquals(3, element.CodeInfo.GetErrors().Count());
		}

		protected override TransactionNumberSequenceCustomisation GetBusinessObjectToClone()
		{
			return new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber, Include = true, Order = 1 };
		}

		protected override TransactionNumberSequenceCustomisation GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			elementSnNum = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber, Include = true, Order = 1 };
			elementElm1 = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.CustomElement1, Include = true, Order = 1 };
			elementElm2 = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.CustomElement2, Include = true, Order = 1 };

			elementAccYearDigits = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits, Include = true, Order = 1 };
			elementAccPeriod = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits, Include = true, Order = 1 };
			elementAccYearLetter = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsLetter, Include = true, Order = 1 };
			elementYearDigits = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits, Include = true, Order = 1 };
			elementYearLetter = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.YearAsLetter, Include = true, Order = 1 };
			elementMonDigits = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.MonthAs2Digits, Include = true, Order = 1 };
			elementMonLetter = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.MonthAsLetter, Include = true, Order = 1 };

			elementJobBrn = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.JobHeaderBranchCode, Include = true, Order = 1 };
			elementJobDpt = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.JobHeaderDepartmentCode, Include = true, Order = 1 };
			elementTransBrn = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderBranchCode, Include = true, Order = 1 };
			elementTransDpt = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode, Include = true, Order = 1 };

			elementTaxAndNonTax = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax, Include = true, Order = 1 };
			elementSelfBillingAndStandard = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard, Include = true, Order = 1 };
			elementCorrectedAndOriginal = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal, Include = true, Order = 1 };
			elementTransactionTypePrefix = new TransactionNumberSequenceCustomisation { ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionTypePrefix, Include = true, Order = 1 };
		}

		#region Implementation
		protected void AssertYearCode(TransactionNumberSequenceCustomisation yearElementWithCode)
		{
			AssertEquals(ZString.Empty, yearElementWithCode.Code);
			AssertHasError(yearElementWithCode.CodeInfo, "The year as digit length should be either 1, 2 or 4.");
			AssertEquals(0, yearElementWithCode.Length);
			AssertHasError(yearElementWithCode.LengthInfo, "Please enter a 'Length' within the range 1 to 4.");

			yearElementWithCode.Code = "  5  ";

			AssertEquals("5", yearElementWithCode.Code);
			AssertHasError(yearElementWithCode.CodeInfo, "The year as digit length should be either 1, 2 or 4.");
			AssertEquals(0, yearElementWithCode.Length);
			AssertHasError(yearElementWithCode.LengthInfo, "Please enter a 'Length' within the range 1 to 4.");

			yearElementWithCode.Code = "XXX";

			AssertEquals("XXX", yearElementWithCode.Code);
			AssertHasError(yearElementWithCode.CodeInfo, "The year as digit length should be either 1, 2 or 4.");
			AssertEquals(0, yearElementWithCode.Length);
			AssertHasError(yearElementWithCode.LengthInfo, "Please enter a 'Length' within the range 1 to 4.");

			yearElementWithCode.Code = "1";

			AssertEquals("1", yearElementWithCode.Code);
			AssertNoErrors(yearElementWithCode.CodeInfo);
			AssertEquals(1, yearElementWithCode.Length);
			AssertNoErrors(yearElementWithCode.LengthInfo);

			yearElementWithCode.Code = "2";

			AssertEquals("2", yearElementWithCode.Code);
			AssertNoErrors(yearElementWithCode.CodeInfo);
			AssertEquals(2, yearElementWithCode.Length);
			AssertNoErrors(yearElementWithCode.LengthInfo);

			yearElementWithCode.Code = "4";

			AssertEquals("4", yearElementWithCode.Code);
			AssertNoErrors(yearElementWithCode.CodeInfo);
			AssertEquals(4, yearElementWithCode.Length);
			AssertNoErrors(yearElementWithCode.LengthInfo);
		}

		void RunValidationOnAllElements()
		{
			elementSnNum.RunPreSaveValidation();
			elementElm1.RunPreSaveValidation();
			elementElm2.RunPreSaveValidation();
			elementAccYearDigits.RunPreSaveValidation();
			elementAccPeriod.RunPreSaveValidation();
			elementAccYearLetter.RunPreSaveValidation();
			elementYearDigits.RunPreSaveValidation();
			elementYearLetter.RunPreSaveValidation();
			elementMonDigits.RunPreSaveValidation();
			elementMonLetter.RunPreSaveValidation();
			elementJobBrn.RunPreSaveValidation();
			elementJobDpt.RunPreSaveValidation();
			elementTransBrn.RunPreSaveValidation();
			elementTransDpt.RunPreSaveValidation();
			elementTaxAndNonTax.RunPreSaveValidation();
			elementSelfBillingAndStandard.RunPreSaveValidation();
			elementCorrectedAndOriginal.RunPreSaveValidation();
			elementTransactionTypePrefix.RunPreSaveValidation();
		}

		TransactionNumberSequenceCustomisation elementSnNum, elementElm1, elementElm2,
			elementAccYearDigits, elementAccPeriod, elementAccYearLetter, elementYearDigits, elementYearLetter, elementMonDigits, elementMonLetter,
			elementJobBrn, elementJobDpt, elementTransBrn, elementTransDpt, elementTaxAndNonTax, elementSelfBillingAndStandard, elementCorrectedAndOriginal, elementTransactionTypePrefix;

		#endregion
	}
}
