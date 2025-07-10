using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	abstract class JobComInvoiceLineValidationTest<TValidation> : BusinessObjectValidationTestCase
	where TValidation : JobComInvoiceLineValidation
	{
		protected abstract string MessageType { get; }

		protected (TValidation validation, JobComInvoiceLine invoiceLine, JobComInvoiceHeader invoice, JobDeclaration declaration) SetupData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageType;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return ((TValidation)invoiceLine.Validation, invoiceLine, invoice, declaration);
		}
	}

	sealed class JobComInvoiceLineValidationBaseOnlyTest : JobComInvoiceLineValidationTest<JobComInvoiceLineValidation>
	{
		public void TestHasValidPackagePivots()
		{
			(_, var invoiceLine, _, var declaration) = SetupData();
			var validation = new JobComInvoiceLineValidationForTest(invoiceLine);
			AssertEquals("When no package pivot is available", false, validation.HasValidPackagePivotsExposed);

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "999";
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_PackType = "CT";
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			var packagePivot = invoiceLine.PackagesPivot.AddNew();
			packagePivot.CHC_CW = package.PK;
			packagePivot.CHC_NumberOfPacks = 0;
			packagePivot.CHC_JE = declaration.PK;
			AssertEquals("When a package pivot is available and CHC_NumberOfPacks is 0", true, validation.HasValidPackagePivotsExposed);
		}

		public void TestParent()
		{
			(var validation, var invoiceLine, _, _) = SetupData();
			AssertSame(invoiceLine, validation.Parent);
		}

		const string NetWeigthLessThanGrossMassMessageError = "The Net Weight must be less than or equal to the Gross Mass";

		public void TestJI_CustomsQuantity()
		{
			var errorMessage = NetWeigthLessThanGrossMassMessageError;
			(var validation, var invoiceLine, _, _) = SetupData();
			invoiceLine.JI_CustomsQuantity = 1001;
			invoiceLine.JI_CustomsUnitQty = RefCusCodeList.CustomsUq.Weight.Gram;

			invoiceLine.JI_Weight = 1;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			validation.ValidateJI_CustomsQuantity();
			AssertNoMessageError("No error when JI_CustomsUnitQty is not KGM", invoiceLine.JI_CustomsQuantityInfo, errorMessage);

			invoiceLine.JI_CustomsUnitQty = RefCusCodeList.CustomsUq.Weight.Kilogram;
			validation.ValidateJI_CustomsQuantity();
			AssertHasMessageError("Has error when JI_CustomsQuantity is greater than JI_Weight", invoiceLine.JI_CustomsQuantityInfo, errorMessage);

			invoiceLine.JI_CustomsQuantity = 1000;
			validation.ValidateJI_CustomsQuantity();
			AssertNoMessageError("No error when JI_CustomsUnitQty is equal to JI_Weight", invoiceLine.JI_CustomsQuantityInfo, errorMessage);

			invoiceLine.JI_Weight = 2;
			validation.ValidateJI_CustomsQuantity();
			AssertNoMessageError("No error when JI_CustomsUnitQty is less then JI_Weight", invoiceLine.JI_CustomsQuantityInfo, errorMessage);
		}

		public void TestJI_Weight()
		{
			var errorMessage = NetWeigthLessThanGrossMassMessageError;
			(var validation, var invoiceLine, _, _) = SetupData();
			invoiceLine.JI_CustomsUnitQty = RefCusCodeList.CustomsUq.Weight.Kilogram;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 10;

			invoiceLine.JI_Weight = 9;
			AssertHasMessageError("Has error when JI_Weight is less than JI_CustomsQuantity", invoiceLine.JI_CustomsQuantityInfo, errorMessage);

			invoiceLine.JI_Weight = 10;
			AssertNoMessageError("No error when JI_Weight is equal to JI_CustomsQuantity", invoiceLine.JI_CustomsQuantityInfo, errorMessage);
		}

		public void TestJI_WeightUQ()
		{
			var errorMessage = NetWeigthLessThanGrossMassMessageError;
			(var validation, var invoiceLine, _, _) = SetupData();
			invoiceLine.JI_CustomsUnitQty = RefCusCodeList.CustomsUq.Weight.Kilogram;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_Weight = 10;

			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;
			AssertHasMessageError("Has error when JI_Weight is less than JI_CustomsQuantity (Grams vs Kilograms)", invoiceLine.JI_CustomsQuantityInfo, errorMessage);

			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoMessageError("No error when JI_Weight is less then JI_CustomsQuantity (Both in Kilograms)", invoiceLine.JI_CustomsQuantityInfo, errorMessage);
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.MiscellaneousCustoms;

		class JobComInvoiceLineValidationForTest : JobComInvoiceLineValidation
		{
			public JobComInvoiceLineValidationForTest(JobComInvoiceLine parent) : base(parent)
			{
			}

			public bool HasValidPackagePivotsExposed => HasValidPackagePivots;
		}
	}
}
