using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using IEuTax = Enterprise.Customs.EU.Business.Declaration.IEuTax;
using RefundMethodOfCalculation = Enterprise.Customs.IE.Business.Constants.CusEntryLineFeeRefundDutyMethodOfCalculation;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(RefundDuty))]
	public class RefundDutyTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var refundDuty = (RefundDuty)GetNewBusinessObject();
			AssertType<RefundDutyLookups>($"Lookups should be with the correct type {typeof(RefundDutyLookups).FullName}.", refundDuty.Lookups);
		}

		public void TestCaption_TaxType()
		{
			var refundDuty = (RefundDuty)GetNewBusinessObject();
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(refundDuty.TaxTypeInfo);
			AssertEquals("Caption", "Tax Type", resourceStringDataAttribute.Caption);
		}

		public void TestCaption_TaxAmountConfirmedRelease()
		{
			var refundDuty = (RefundDuty)GetNewBusinessObject();
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(refundDuty.TaxAmountConfirmedReleaseInfo);
			CombineAssertions("TaxAmountConfirmedRelease captions", () =>
			{
				AssertEquals("Caption", "Confirmed Release", resourceStringDataAttribute.Caption);
				AssertEquals("FullDescription", "Tax Amount Confirmed Release", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestCaption_TaxAmountConfirmedAmendment()
		{
			var refundDuty = (RefundDuty)GetNewBusinessObject();
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(refundDuty.TaxAmountConfirmedAmendmentInfo);
			CombineAssertions("TaxAmountConfirmedAmendment captions", () =>
			{
				AssertEquals("Caption", "Confirmed Amendment", resourceStringDataAttribute.Caption);
				AssertEquals("FullDescription", "Tax Amount Confirmed Amendment", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestCaption_TaxAmountDifferenceForRefunds()
		{
			var refundDuty = (RefundDuty)GetNewBusinessObject();
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(refundDuty.TaxAmountDifferenceForRefundsInfo);
			CombineAssertions("TaxAmountDifferenceForRefunds captions", () =>
			{
				AssertEquals("Caption", "Difference for Refunds", resourceStringDataAttribute.Caption);
				AssertEquals("FullDescription", "Tax amount difference for Refunds", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestCaption_TaxAmountOfDutyToBeRepaid()
		{
			var refundDuty = (RefundDuty)GetNewBusinessObject();
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(refundDuty.TaxAmountOfDutyToBeRepaidInfo);
			CombineAssertions("TaxAmountOfDutyToBeRepaid captions", () =>
			{
				AssertEquals("Caption", "Confirmed Refund", resourceStringDataAttribute.Caption);
				AssertEquals("FullDescription", "Confirmed Refund Amount To-Be-Paid", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestSetter_TaxAmountConfirmedRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var refundDuty = entryLine.RefundDuties.AddNew(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);

			var query =
				new ZQuery(CusEntryLineFeeSchema.CF_ChargeType, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat)
				.AddToFilter(CusEntryLineFeeSchema.CF_CL, entryLine.PK);

			AssertNull("Refund Fee should not be created before RefundDuty property is set", Factory.LoadTop1<CusEntryLineFee>(query));

			refundDuty.TaxAmountConfirmedRelease = 123.45m;
			AssertEquals("Property setter.", 123.45m, refundDuty.TaxAmountConfirmedRelease);
			var confirmedReleaseFee = Factory.LoadTop1<CusEntryLineFee>(query);
			AssertEquals("CF_ChargeType", EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, confirmedReleaseFee.CF_ChargeType);
			AssertEquals("CF_MethodOfCalculation", RefundMethodOfCalculation.ConfirmedRelease, confirmedReleaseFee.CF_MethodOfCalculation);
			AssertEquals("CF_ChargeAmount", 123.45m, confirmedReleaseFee.CF_ChargeAmount);

			refundDuty.TaxAmountConfirmedRelease = 0;
			AssertEquals("Refund Fee should be deleted when property set to Zero.", true, confirmedReleaseFee.IsDeleted);
		}

		public void TestSetter_TaxAmountConfirmedAmendment()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var refundDuty = entryLine.RefundDuties.AddNew(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);

			var query =
				new ZQuery(CusEntryLineFeeSchema.CF_ChargeType, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat)
				.AddToFilter(CusEntryLineFeeSchema.CF_CL, entryLine.PK);

			AssertNull("Refund Fee should not be created before RefundDuty property is set", Factory.LoadTop1<CusEntryLineFee>(query));

			refundDuty.TaxAmountConfirmedAmendment = 234.56m;
			AssertEquals("Property setter.", 234.56m, refundDuty.TaxAmountConfirmedAmendment);
			var confirmedAmendmentFee = Factory.LoadTop1<CusEntryLineFee>(query);
			AssertEquals("CF_ChargeType", EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, confirmedAmendmentFee.CF_ChargeType);
			AssertEquals("CF_MethodOfCalculation", RefundMethodOfCalculation.ConfirmedAmendment, confirmedAmendmentFee.CF_MethodOfCalculation);
			AssertEquals("CF_ChargeAmount", 234.56m, confirmedAmendmentFee.CF_ChargeAmount);

			refundDuty.TaxAmountConfirmedAmendment = 0;
			AssertEquals("Refund Fee should be deleted when property set to Zero.", true, confirmedAmendmentFee.IsDeleted);
		}

		public void TestSetter_TaxAmountDifferenceForRefunds()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var refundDuty = entryLine.RefundDuties.AddNew(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);

			var query =
				new ZQuery(CusEntryLineFeeSchema.CF_ChargeType, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat)
				.AddToFilter(CusEntryLineFeeSchema.CF_CL, entryLine.PK);

			AssertNull("Refund Fee should not be created before RefundDuty property is set", Factory.LoadTop1<CusEntryLineFee>(query));

			refundDuty.TaxAmountDifferenceForRefunds = 234.56m;
			AssertEquals("Property setter.", 234.56m, refundDuty.TaxAmountDifferenceForRefunds);
			var differenceForRefundsFee = Factory.LoadTop1<CusEntryLineFee>(query);
			AssertEquals("CF_ChargeType", EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, differenceForRefundsFee.CF_ChargeType);
			AssertEquals("CF_MethodOfCalculation", RefundMethodOfCalculation.DifferenceForRefunds, differenceForRefundsFee.CF_MethodOfCalculation);
			AssertEquals("CF_ChargeAmount", 234.56m, differenceForRefundsFee.CF_ChargeAmount);

			refundDuty.TaxAmountDifferenceForRefunds = 0;
			AssertEquals("Refund Fee should be deleted when property set to Zero.", true, differenceForRefundsFee.IsDeleted);
		}

		public void TestSetter_TaxAmountOfDutyToBeRepaid()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var refundDuty = entryLine.RefundDuties.AddNew(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);

			var query =
				new ZQuery(CusEntryLineFeeSchema.CF_ChargeType, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat)
				.AddToFilter(CusEntryLineFeeSchema.CF_CL, entryLine.PK);

			AssertNull("Refund Fee should not be created before RefundDuty property is set", Factory.LoadTop1<CusEntryLineFee>(query));

			refundDuty.TaxAmountOfDutyToBeRepaid = 234.56m;
			AssertEquals("Property setter.", 234.56m, refundDuty.TaxAmountOfDutyToBeRepaid);
			var toBeRepaidFee = Factory.LoadTop1<CusEntryLineFee>(query);
			AssertEquals("CF_ChargeType", EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, toBeRepaidFee.CF_ChargeType);
			AssertEquals("CF_MethodOfCalculation", RefundMethodOfCalculation.ToBeRepaid, toBeRepaidFee.CF_MethodOfCalculation);
			AssertEquals("CF_ChargeAmount", 234.56m, toBeRepaidFee.CF_ChargeAmount);

			refundDuty.TaxAmountOfDutyToBeRepaid = 0;
			AssertEquals("Refund Fee should be deleted when property set to Zero.", true, toBeRepaidFee.IsDeleted);
		}

		public void TestDefaultValuesEmpty()
		{
			var refundDuty = (RefundDuty)GetNewBusinessObject();
			CombineAssertions("Properties should be accessible on empty item.", () =>
			{
				AssertEquals("TaxType", EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, refundDuty.TaxType);
				AssertEquals("TaxAmountConfirmedRelease", ZDecimal.Zero, refundDuty.TaxAmountConfirmedRelease);
				AssertEquals("TaxAmountConfirmedAmendment", ZDecimal.Zero, refundDuty.TaxAmountConfirmedAmendment);
				AssertEquals("TaxAmountDifferenceForRefunds", ZDecimal.Zero, refundDuty.TaxAmountDifferenceForRefunds);
				AssertEquals("TaxAmountOfDutyToBeRepaid", ZDecimal.Zero, refundDuty.TaxAmountOfDutyToBeRepaid);
			});
		}

		public void TestDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();

			var confirmedReleaseFee = Factory.New<CusEntryLineFee>();
			confirmedReleaseFee.CF_CL = entryLine.PK;
			confirmedReleaseFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			confirmedReleaseFee.CF_MethodOfCalculation = RefundMethodOfCalculation.ConfirmedRelease;
			confirmedReleaseFee.CF_ChargeAmount = 234.56m;

			var confirmedAmendmentFee = Factory.New<CusEntryLineFee>();
			confirmedAmendmentFee.CF_CL = entryLine.PK;
			confirmedAmendmentFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			confirmedAmendmentFee.CF_MethodOfCalculation = RefundMethodOfCalculation.ConfirmedAmendment;
			confirmedAmendmentFee.CF_ChargeAmount = 123.45m;

			var differenceForRefundsFee = Factory.New<CusEntryLineFee>();
			differenceForRefundsFee.CF_CL = entryLine.PK;
			differenceForRefundsFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			differenceForRefundsFee.CF_MethodOfCalculation = RefundMethodOfCalculation.DifferenceForRefunds;
			differenceForRefundsFee.CF_ChargeAmount = 111.11m;

			var toBeRepaidFee = Factory.New<CusEntryLineFee>();
			toBeRepaidFee.CF_CL = entryLine.PK;
			toBeRepaidFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			toBeRepaidFee.CF_MethodOfCalculation = RefundMethodOfCalculation.ToBeRepaid;
			toBeRepaidFee.CF_ChargeAmount = 100m;

			var loadedRefund = (RefundDuty)entryLine.RefundDuties.FirstOrDefault();
			CombineAssertions("RefundDuty loaded with values", () =>
			{
				AssertEquals("TaxType", EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, loadedRefund.TaxType);
				AssertEquals("TaxAmountConfirmedRelease", 234.56m, loadedRefund.TaxAmountConfirmedRelease);
				AssertEquals("TaxAmountConfirmedAmendment", 123.45m, loadedRefund.TaxAmountConfirmedAmendment);
				AssertEquals("TaxAmountDifferenceForRefunds", 111.11m, loadedRefund.TaxAmountDifferenceForRefunds);
				AssertEquals("TaxAmountOfDutyToBeRepaid", 100m, loadedRefund.TaxAmountOfDutyToBeRepaid);
			});
		}

		public void TestIEuTaxMembers()
		{
			var refundDuty = GetNewBusinessObject();
			var iEuTaxType = typeof(IEuTax);

			CombineAssertions("IEuTax members", () =>
			{
				foreach (var property in iEuTaxType.GetProperties())
				{
					var propertyName = property.Name;
					switch (propertyName)
					{
						case nameof(IEuTax.Factory):
							AssertSame("IEuTax.Factory should return the correct result.", Factory, property.GetValue(refundDuty));
							break;
						case nameof(IEuTax.ImportExportParent):
							AssertSame("IEuTax.ImportExportParent should return the correct result.", jobDeclaration, property.GetValue(refundDuty));
							break;
						case nameof(IEuTax.CountryCode):
							AssertEquals("IEuTax.CountryCode should return JobDeclaration CountryCode.", jobDeclaration.CountryCode, property.GetValue(refundDuty));
							break;
						default:
							AssertExceptionThrown<TargetInvocationException>($"Property {propertyName} is Not Implemented.", () => property.GetValue(refundDuty));
							break;
					}
				}

				foreach (var method in iEuTaxType.GetMethods())
				{
					AssertExceptionThrown<TargetInvocationException>($"Method {method.Name} is Not Implemented.", () => method.Invoke(refundDuty, null));
					break;
				}
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			jobDeclaration = Factory.New<JobDeclaration>();
			var entryLine = jobDeclaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			return entryLine.RefundDuties.AddNew(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);
		}
		JobDeclaration jobDeclaration;
	}
}
