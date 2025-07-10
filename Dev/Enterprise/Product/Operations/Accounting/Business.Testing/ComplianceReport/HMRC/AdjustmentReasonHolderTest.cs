using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	[TestedType(typeof(AdjustmentReasonHolder))]
	internal class AdjustmentReasonHolderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHolderProperties()
		{
			AssertHolderProperties(Holder, string.Empty, string.Empty);
			Holder.Code = "CLS";
			AssertHolderProperties(Holder, "CLS", "Incorrect Classification");
			Holder.Code = "CLSS";
			AssertHolderProperties(Holder, "CLS", "Incorrect Classification");
			Holder.Reason = "cls reason";
			AssertHolderProperties(Holder, "CLS", "cls reason");
			var veryLongReasonBuilder = new ZStringBuilder();
			for (int i = 0; i < 100; i++)
			{
				veryLongReasonBuilder.Append("cls reason");
			}
			var expectedReason = veryLongReasonBuilder.ToString();
			AssertEquals(1000, expectedReason.Length);
			veryLongReasonBuilder.Append("cls reason");
			Holder.Reason = veryLongReasonBuilder.ToString();
			AssertHolderProperties(Holder, "CLS", expectedReason);

			Holder.Code = "DEL";
			AssertHolderProperties(Holder, "DEL", "Values not known at the time of input into software");
			Holder.Reason = "del reason";
			AssertHolderProperties(Holder, "DEL", "del reason");
			Holder.Code = "IDE";
			AssertHolderProperties(Holder, "IDE", "Incorrect Data Entry");
			Holder.Reason = "ide reason";
			AssertHolderProperties(Holder, "IDE", "ide reason");
			Holder.Code = string.Empty;
			AssertHolderProperties(Holder, string.Empty, string.Empty);
		}

		public void TestPropertiesValidationWithNonZeroAdjustmentAmount()
		{
			AssertHolderProperties(Holder, string.Empty, string.Empty);
			AssertNoErrors(Holder.CodeInfo);
			AssertNoErrors(Holder.ReasonInfo);

			Holder.RunPreSaveValidation();
			AssertHasError(Holder.CodeInfo, "Please enter a value.");
			AssertHasError(Holder.ReasonInfo, "Please enter a value.");

			Holder.Code = "CLS";
			AssertNoErrors(Holder.CodeInfo);
			AssertNoErrors(Holder.ReasonInfo);

			Holder.Code = "DEL";
			AssertNoErrors(Holder.CodeInfo);
			AssertNoErrors(Holder.ReasonInfo);

			Holder.Code = "IDE";
			AssertNoErrors(Holder.CodeInfo);
			AssertNoErrors(Holder.ReasonInfo);

			Holder.Code = "ZZZ";
			AssertHasError(Holder.CodeInfo, "Enter a valid selection.");
			AssertHasError(Holder.ReasonInfo, "Please enter a value.");

			Holder.Code = "CLS";
			AssertNoErrors(Holder.CodeInfo);
			AssertNoErrors(Holder.ReasonInfo);

			Holder.Reason = ZString.Empty;
			AssertNoErrors(Holder.CodeInfo);
			AssertHasError(Holder.ReasonInfo, "Please enter a value.");

			Holder.Code = string.Empty;
			AssertHasError(Holder.CodeInfo, "Please enter a value.");
			AssertHasError(Holder.ReasonInfo, "Please enter a value.");
		}

		public void TestPropertiesValidationWithZeroAdjustmentAmount()
		{
			Holder = new AdjustmentReasonHolder(() => ZDecimal.Zero);

			AssertHolderProperties(Holder, string.Empty, string.Empty);
			AssertNoErrors(Holder.CodeInfo);
			AssertNoErrors(Holder.ReasonInfo);

			Holder.RunPreSaveValidation();

			Holder.Code = "CLS";
			AssertHasError(Holder.CodeInfo, "Please do not enter a value.");
			AssertHasError(Holder.ReasonInfo, "Please do not enter a value.");

			Holder.Reason = ZString.Empty;
			AssertHasError(Holder.CodeInfo, "Please do not enter a value.");
			AssertNoErrors(Holder.ReasonInfo);

			Holder.Code = string.Empty;
			AssertNoErrors(Holder.CodeInfo);
			AssertNoErrors(Holder.ReasonInfo);
		}

		public void TestHolderReasonHasSameLengthConstraintAsATC_Comment()
		{
			var taxReturn = new TestObjectCreator(Factory).CreateAccTaxReturn(addTaxReturnColumn: true);
			taxReturn.Columns[0].ATC_Comment = new ZString('A', 1000);
			Factory.Save();

			Holder.Reason = taxReturn.Columns[0].ATC_Comment;
			AssertEquals("Reason field can support 1000 characters like ATC_Comment", new ZString('A', 1000), Holder.Reason);
		}

		void AssertHolderProperties(AdjustmentReasonHolder holder, string code, string reason)
		{
			AssertEquals("Code", code, holder.Code);
			AssertEquals("Reason", reason, holder.Reason);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AdjustmentReasonHolder(() => new ZDecimal(10m));
		}

		protected override void SetUp()
		{
			base.SetUp();
			Holder = new AdjustmentReasonHolder(() => new ZDecimal(10m));
		}

		AdjustmentReasonHolder Holder;
	}
}
