using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	public abstract class CustomsChargeCodeTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestDescriptionShouldAlwaysBeCorrect()
		{
			CombineAssertions("Some errors while running test for " + ExpectedCode + ":" + ExpectedDescription, () =>
			{
				var chargeCode = GetChargeCodeToTest();

				var descInfo = typeof(CustomsChargeCode).GetProperty("Description");
				NUnit.Framework.Assert.That(descInfo.GetValue(chargeCode), Is.EqualTo(ExpectedDescription).Using(CustomComparers.TypeComparison));

				descInfo.SetValue(chargeCode, (NoResString)"@#$_TestValue");
				NUnit.Framework.Assert.That(chargeCode.Description, Is.EqualTo("@#$_TestValue").Using(CustomComparers.TypeComparison), "Description can be set by some tests like TestFormIsFullyTranslatable()");
				NUnit.Framework.Assert.That(GetChargeCodeToTest().Description, Is.EqualTo(ExpectedDescription).Using(CustomComparers.TypeComparison), "Because the code was cached by Dictionary, we will not get correct description." +
					" unless we use new factory or directly use the code, description should be correct whenever you get it");
			});
		}

		[ExpectNoExceptions]
		public void TestCode()
		{
			var chargeCode = GetChargeCodeToTest();
			NUnit.Framework.Assert.That(chargeCode.Code, Is.EqualTo(ExpectedCode), "Code:" + chargeCode.Code);
		}

		[ExpectNoExceptions]
		public void TestDescription()
		{
			var chargeCode = GetChargeCodeToTest();
			NUnit.Framework.Assert.That(chargeCode.Description, Is.EqualTo(ExpectedDescription).Using(CustomComparers.TypeComparison), "Description:" + chargeCode.Code);
		}

		[ExpectNoExceptions]
		public void TestIsDutiable()
		{
			var chargeCode = GetChargeCodeToTest();
			NUnit.Framework.Assert.That(chargeCode.IsDutiable, Is.EqualTo(ExpectedIsDutiable), "IsDutiable:" + chargeCode.Code);
		}

		[ExpectNoExceptions]
		public void TestIsVATible()
		{
			var chargeCode = GetChargeCodeToTest();
			NUnit.Framework.Assert.That(chargeCode.IsVATible, Is.EqualTo(ExpectedIsVATible), "IsVATApplicable:" + chargeCode.Code);
		}

		[ExpectNoExceptions]
		public void TestIsDutiableDeemedForThisCharge()
		{
			var chargeCode = GetChargeCodeToTest();
			NUnit.Framework.Assert.That(chargeCode.IsDutiableDeemedForThisCharge, Is.EqualTo(ExpectedIsDutiableDeemedForThisCharge), "IsDutiable Deemed:" + chargeCode.Code);
		}

		[ExpectNoExceptions]
		public void TestIsVATApplicableDeemedForThisCharge()
		{
			var chargeCode = GetChargeCodeToTest();
			NUnit.Framework.Assert.That(chargeCode.IsVATibleDeemedForThisCharge, Is.EqualTo(ExpectedIsVATibleDeemedForThisCharge), "IsVAT Deemed:" + chargeCode.Code);
		}

		[ExpectNoExceptions]
		public void TestIsIncludedInITOTDeemedForThisCharge()
		{
			var chargeCode = GetChargeCodeToTest();
			NUnit.Framework.Assert.That(chargeCode.IsIncludedInITOTDeemedForThisCharge, Is.EqualTo(ExpectedIsIncludedInITOTDeemedForThisCharge), "IsIncludedInITOT deemed for this charge:" + chargeCode.Code);
		}

		[ExpectNoExceptions]
		public void TestIsPercentageApplicableAndParentTypes()
		{
			var chargeCode = GetChargeCodeToTest();
			NUnit.Framework.Assert.That(chargeCode.IsPercentageApplicable, Is.EqualTo(ExpectedIsPercentageApplicable), "Is Percentage applicable" + chargeCode.Code);

			NUnit.Framework.Assert.That(chargeCode.ParentTypes, Is.EqualTo(ExpectedChargeParentTypes), "ParentTypes");
		}

		[ExpectNoExceptions]
		public void TestIsIncoTermNeutral()
		{
			var chargeCode = GetChargeCodeToTest();
			NUnit.Framework.Assert.That(chargeCode.IsIncoTermNeutral, Is.EqualTo(ExpectedIsIncoTermNeutral), "Is IncoTerm Neutral" + chargeCode.Code);
		}

		[ExpectNoExceptions]
		public void TestIsIncludedInInvoiceDeemedForThisCharge()
		{
			CombineAssertions(() =>
			{
				var chargeCode = GetChargeCodeToTest();
				foreach (var incoTerm in IncoTermAndChargeFactory.GetAllIncoTerms())
				{
					NUnit.Framework.Assert.That(IncoTermAndChargeFactory.IsIncludedInInvoiceAmountFixed(incoTerm, chargeCode), Is.EqualTo(ExpectedIsIncludedInInvoiceDeemedForThisCharge(incoTerm)), "IsIncludedInInvoiceDeemedForThisCharge for Charge:" + chargeCode.Code + " for incoterm " + incoTerm);
				}
			});
		}

		[ExpectNoExceptions]
		public void TestGetDefaultIsIncludedInInvoice()
		{
			CombineAssertions(() =>
			{
				var chargeCode = GetChargeCodeToTest();
				foreach (var incoterm in IncoTermAndChargeFactory.GetAllIncoTerms())
				{
					NUnit.Framework.Assert.That(IncoTermAndChargeFactory.GetDefaultIsIncludedInInvoice(incoterm, chargeCode), Is.EqualTo(ExpectedIsDefaultIsIncludedInInvoice(incoterm)), $"IsIncludedInInvoiceDeemedForThisCharge for Charge '{chargeCode.Code}' IncoTerm '{incoterm}'");
				}
			});
		}

		#region Implementation

		protected abstract string ExpectedCode { get; }
		protected abstract string ExpectedDescription { get; }
		protected abstract bool ExpectedIsDutiable { get; }
		protected abstract bool ExpectedIsVATible { get; }
		protected abstract bool ExpectedIsDutiableDeemedForThisCharge { get; }
		protected abstract bool ExpectedIsVATibleDeemedForThisCharge { get; }
		protected abstract bool ExpectedIsIncludedInITOTDeemedForThisCharge { get; }
		protected abstract bool ExpectedIsPercentageApplicable { get; }
		protected abstract bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm);
		protected abstract bool ExpectedIsIncoTermNeutral { get; }
		protected abstract bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable);

		protected virtual ChargeParentTypes ExpectedChargeParentTypes => ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine;

		protected virtual bool ExpectedIsDefaultIsIncludedInInvoice(ZString incoterm)
		{
			return IncoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(incoterm, GetChargeCodeToTest());
		}

		protected IncoTermAndCustomsChargeFactory IncoTermAndChargeFactory => IncoTermAndCustomsChargeFactory.GetByCountryCode(GetCountryContext());

		protected virtual string GetCountryContext() => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected abstract ICustomsChargeCode GetChargeCodeToTest();

		#endregion
	}
}
