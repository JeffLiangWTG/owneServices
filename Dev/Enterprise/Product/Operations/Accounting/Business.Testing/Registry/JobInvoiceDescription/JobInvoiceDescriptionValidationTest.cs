using System.Text;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class JobInvoiceDescriptionValidationTest : JobConfigurationSelectorValidationTest
	{
		protected override string ExpectedDuplicateJobParametersError
		{
			get { return "At least one more record already sets invoice description for the same Job parameters."; }
		}

		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return new JobInvoiceDescription();
			}
		}

		protected override IRegistrySettingCollection GetNewBizObjCollection
		{
			get
			{
				return new JobInvoiceDescriptionCollection();
			}
		}

		public void TestInvoiceDescription()
		{
			BizObj.JobType = "SHP";
			Assert("Pre-condition: InvoiceDescription is empty", BizObj.InvoiceDescription.IsEmpty);
			BizObj.ValidateInvoiceDescription();
			AssertHasError(BizObj.InvoiceDescriptionInfo, "Please enter an Invoice Description.");

			BizObj.InvoiceDescription = "Desc";
			AssertNoErrors(BizObj.InvoiceDescriptionInfo);
		}

		public override void TestRunPreSaveValidation()
		{
			BizObj.JobType = "";
			BizObj.DirectionCode = "!@#";
			BizObj.Mode = "ABC";

			BizObj.RunPreSaveValidation();

			AssertHasErrors(BizObj.JobTypeInfo);
			AssertHasErrors(BizObj.DirectionCodeInfo);
			AssertHasErrors(BizObj.ModeInfo);
			AssertHasErrors(BizObj.InvoiceDescriptionInfo);

			BizObj.JobType = "SHP";

			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.JobTypeInfo);
			AssertHasErrors(BizObj.DirectionCodeInfo);
			AssertHasErrors(BizObj.ModeInfo);
			AssertHasErrors(BizObj.InvoiceDescriptionInfo);

			BizObj.DirectionCode = "ALL";

			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.JobTypeInfo);
			AssertNoErrors(BizObj.DirectionCodeInfo);
			AssertHasErrors(BizObj.ModeInfo);
			AssertHasErrors(BizObj.InvoiceDescriptionInfo);

			BizObj.Mode = "ALL";

			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.JobTypeInfo);
			AssertNoErrors(BizObj.DirectionCodeInfo);
			AssertNoErrors(BizObj.ModeInfo);
			AssertHasErrors(BizObj.InvoiceDescriptionInfo);

			BizObj.InvoiceDescription = "Description";

			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.JobTypeInfo);
			AssertNoErrors(BizObj.DirectionCodeInfo);
			AssertNoErrors(BizObj.ModeInfo);
			AssertNoErrors(BizObj.InvoiceDescriptionInfo);
		}

		public void TestErrorIsThrownWhenInvoiceDescriptionExceedsMaxLength()
		{
			AssertNoExceptionThrown(() => BizObj.InvoiceDescription = "Description");

			var descriptionLongerThan1024 = new StringBuilder();
			for (int i = 0; i <= 1024; i++)
			{
				descriptionLongerThan1024.Append("d");
			}

			AssertExceptionThrown<MaxLengthExceededException>(() => BizObj.InvoiceDescription = descriptionLongerThan1024.ToString());
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		protected new JobInvoiceDescription BizObj
		{
			get { return (JobInvoiceDescription)base.BizObj; }
			set { base.BizObj = value; }
		}
	}
}
