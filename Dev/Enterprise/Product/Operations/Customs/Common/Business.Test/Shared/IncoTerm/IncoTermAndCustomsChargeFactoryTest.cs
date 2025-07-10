using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	public partial class IncoTermAndCustomsChargeFactoryTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public virtual void TestGetAllIncoTerms()
		{
			NUnit.Framework.Assert.That(incoTermAndChargeFactory.GetAllIncoTerms().Length, Is.EqualTo(12), "Count");
		}

		[ExpectNoExceptions]
		public virtual void TestCanThisChargeBeIncludedOnLineButNotOnInvoice()
		{
			NUnit.Framework.Assert.That(!incoTermAndChargeFactory.CanThisChargeBeIncludedOnLineButNotOnInvoice("XXX"), Is.True, "CanThisChargeBeIncludedOnLineButNotOnInvoice default false for any codes");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public virtual void TestIncoTermAndCustomsChargeConfiguration()
		{
			var resultData = new ZStringBuilder(DataHeading);
			var incoTermFactory = IncoTermAndCustomsChargeFactory.GetByCountryCode(GetCountryContext());
			var allCharges = incoTermFactory.GetAllCharges();
			foreach (var incoTerm in incoTermFactory.GetAllIncoTerms())
			{
				foreach (var charge in allCharges)
				{
					var chargeCode = charge.Code;
					resultData.Append(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
						incoTerm, chargeCode,
						incoTermFactory.IsIncludedInInvoiceAmountFixed(incoTerm, charge),
						incoTermFactory.GetDefaultIsIncludedInInvoice(incoTerm, charge),
						incoTermFactory.CanThisIncoTermHaveThisCharge(incoTerm, charge),
						incoTermFactory.IsThisChargeMandatory(incoTerm, chargeCode),
						incoTermFactory.IsThisChargeRecommendedForThisIncoTerm(incoTerm, chargeCode),
						incoTermFactory.IsIncludedInITOTReadOnlyForGroupCharge(chargeCode)));
				}
			}
			NUnit.Framework.Assert.That(IncoTermAndCustomsChargeConfigurationFilename, CustomConstraints.ASCIIFileSameAsString(resultData.ToStringWithNewLineBetweenAppends()));
		}
		const string DataHeading = "IncoTerm,CustomsCharge,IsIncludedInInvoiceAmountFixed,GetDefaultIsIncludedInInvoice,CanThisIncoTermHaveThisCharge,IsThisChargeMandatory,IsThisChargeRecommendedForThisIncoTerm,IsIncludedInITOTReadOnlyForGroupCharge";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected virtual string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\Common\Business.Test\Shared\IncoTerm\TestFile\IncoTermAndCustomsChargeConfiguration.csv";

		protected IncoTermAndCustomsChargeFactory incoTermAndChargeFactory;

		[ExpectNoExceptions]
		public void TestGetChargeListMatchingParentTypes()
		{
			var incoTermFactory = IncoTermAndCustomsChargeFactory.GetByCountryCode(GetCountryContext());
			var charge = incoTermFactory.GetAllCharges()[0];
			var originalParentTypes = charge.ParentTypes;

			try
			{
				((CustomsChargeCode)charge).ParentTypes = ChargeParentTypes.Invoice;

				NUnit.Framework.Assert.That(!incoTermFactory.GetChargeList(ChargeParentTypes.GroupInvoice).Any(x => x.Code == charge.Code), Is.True);
				NUnit.Framework.Assert.That(incoTermFactory.GetChargeList(ChargeParentTypes.Invoice).Any(x => x.Code == charge.Code), Is.True);
				NUnit.Framework.Assert.That(!incoTermFactory.GetChargeList(ChargeParentTypes.InvoiceLine).Any(x => x.Code == charge.Code), Is.True);
				NUnit.Framework.Assert.That(incoTermFactory.GetChargeList(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice).Any(x => x.Code == charge.Code), Is.True);
				NUnit.Framework.Assert.That(incoTermFactory.GetChargeList(ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine).Any(x => x.Code == charge.Code), Is.True);
				NUnit.Framework.Assert.That(!incoTermFactory.GetChargeList(ChargeParentTypes.GroupInvoice | ChargeParentTypes.InvoiceLine).Any(x => x.Code == charge.Code), Is.True);
				NUnit.Framework.Assert.That(incoTermFactory.GetChargeList(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine).Any(x => x.Code == charge.Code), Is.True);

				((CustomsChargeCode)charge).ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine;

				NUnit.Framework.Assert.That(incoTermFactory.GetChargeList(ChargeParentTypes.GroupInvoice).Any(x => x.Code == charge.Code), Is.True);
				NUnit.Framework.Assert.That(incoTermFactory.GetChargeList(ChargeParentTypes.Invoice).Any(x => x.Code == charge.Code), Is.True);
				NUnit.Framework.Assert.That(incoTermFactory.GetChargeList(ChargeParentTypes.InvoiceLine).Any(x => x.Code == charge.Code), Is.True);
				NUnit.Framework.Assert.That(incoTermFactory.GetChargeList(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice).Any(x => x.Code == charge.Code), Is.True);
				NUnit.Framework.Assert.That(incoTermFactory.GetChargeList(ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine).Any(x => x.Code == charge.Code), Is.True);
				NUnit.Framework.Assert.That(incoTermFactory.GetChargeList(ChargeParentTypes.GroupInvoice | ChargeParentTypes.InvoiceLine).Any(x => x.Code == charge.Code), Is.True);
				NUnit.Framework.Assert.That(incoTermFactory.GetChargeList(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine).Any(x => x.Code == charge.Code), Is.True);
			}
			finally
			{
				((CustomsChargeCode)charge).ParentTypes = originalParentTypes;//revert back to what it was as static instance. 
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			incoTermAndChargeFactory = IncoTermAndCustomsChargeFactory.GetByCountryCode(GetCountryContext());
		}

		protected virtual string GetCountryContext()
		{
			return GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		[ExpectNoExceptions]
		public void TestAllChargeCodesShouldNotBeThreadStatic()
		{
			var allCharges = incoTermAndChargeFactory.GetAllCharges();
			var properties = GetCustomsChargeCodeProviderActualType().GetProperties();
			properties.Where(x => x.PropertyType == typeof(CustomsChargeCode)).ForEach(x =>
			{
				NUnit.Framework.Assert.That(!Attribute.IsDefined(x, typeof(ThreadStaticAttribute)), Is.True);
			});
		}

		protected virtual Type GetCustomsChargeCodeProviderActualType() => typeof(CustomsChargeCodeProvider);

		[ExpectNoExceptions]
		public virtual void TestIsThisChargeDiscount()
		{
			NUnit.Framework.Assert.That(incoTermAndChargeFactory.IsThisChargeDiscount(CustomsChargeTypeList.Codes.Discount), Is.True, "DIS is discount charge");
		}

		[ExpectNoExceptions]
		public virtual void TestGetAllCharges()
		{
			var allCharges = incoTermAndChargeFactory.GetAllCharges();
			NUnit.Framework.Assert.That(allCharges.Length, Is.EqualTo(11), "There should be 11 charges");
		}

		[ExpectNoExceptions]
		public virtual void TestGetCharge()
		{
			AssertGetCharge(CustomsChargeTypeList.Codes.PackingCost, CustomsChargeCodeProvider.PackingCost);
			AssertGetCharge(CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeCodeProvider.OverseasFreight);
			AssertGetCharge(CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeCodeProvider.OverseasInsurance);
			AssertGetCharge(CustomsChargeTypeList.Codes.Discount, CustomsChargeCodeProvider.Discount);
			AssertGetCharge(CustomsChargeTypeList.Codes.Commission, CustomsChargeCodeProvider.Commission);
			AssertGetCharge(CustomsChargeTypeList.Codes.ExWorks, CustomsChargeCodeProvider.ExWorks);
			AssertGetCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight, CustomsChargeCodeProvider.ForeignInlandFreight);
			AssertGetCharge(CustomsChargeTypeList.Codes.LandingCharges, CustomsChargeCodeProvider.LandingCharges);
			AssertGetCharge(CustomsChargeTypeList.Codes.OtherCharges, CustomsChargeCodeProvider.OtherCharges);
			AssertGetCharge(CustomsChargeTypeList.Codes.AdditionCharge, CustomsChargeCodeProvider.AdditionCharge);
			AssertGetCharge(CustomsChargeTypeList.Codes.DeductionCharge, CustomsChargeCodeProvider.DeductionCharge);
		}

		[ExpectNoExceptions]
		protected void AssertGetCharge(string chargeType, CustomsChargeCode expectedChargeCode)
		{
			var actualChargeCode = incoTermAndChargeFactory.GetCharge(chargeType);
			NUnit.Framework.Assert.That(actualChargeCode.GetType(), Is.EqualTo(expectedChargeCode.GetType()), "Right Charge Code should have been retrieved");
		}
	}
}
