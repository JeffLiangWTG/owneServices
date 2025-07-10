using System;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationAESProcedureWrapperTest : WrapperHelperTest<DeclarationAESProcedureWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if invoiceLine is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","invoiceLine"), () => GetWrapper(null));
		}

		public void TestRequestedCPC()
		{
			invoiceLine.JI_Procedure = "1049123";
			AssertEquals("Expected filled RequestedCPC", "10", wrapper.RequestedCPC);
		}

		public void TestPreviousCPC()
		{
			invoiceLine.JI_Procedure = "1049123";
			AssertEquals("Expected filled PreviousCPC", "49", wrapper.PreviousCPC);
		}

		public void TestAdditionalProcedures()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalProcedures when no data declared", 0, wrapper.AdditionalProcedures.Count);

				invoiceLine.JI_Procedure = "1049123";
				wrapper = GetWrapper(invoiceLine);
				var additionalProcedures = wrapper.AdditionalProcedures;
				AssertEquals("Expected filled AdditionalProcedures with only JI_Procedure declared count 1", 1, additionalProcedures.Count);
				AssertSame("Cached AdditionalProcedures", wrapper.AdditionalProcedures, additionalProcedures);

				var additionalProceduresList = additionalProcedures.ToList();
				AssertEquals("Expected filled AdditionalProcedures with only JI_Procedure declared, SequenceNumber", "1", additionalProceduresList[0].SequenceNumber);
				AssertEquals("Expected filled AdditionalProcedures with only JI_Procedure declared, Code", "123", additionalProceduresList[0].Code);

				invoiceLine.AdditionalProcedureCodes.AddNew("456F89");
				invoiceLine.AdditionalProcedureCodes.AddNew("789100");
				wrapper = GetWrapper(invoiceLine);
				additionalProceduresList = wrapper.AdditionalProcedures.ToList();
				AssertEquals("Expected filled AdditionalProcedures with JI_Procedure and an additionalCode declared count 2", 2, additionalProceduresList.Count);
				AssertEquals("Expected filled AdditionalProcedures with JI_Procedure and an additionalCode declared, first SequenceNumber", "1", additionalProceduresList[0].SequenceNumber);
				AssertEquals("Expected filled AdditionalProcedures with JI_Procedure and an additionalCode declared, first Code", "F89", additionalProceduresList[0].Code);
				AssertEquals("Expected filled AdditionalProcedures with JI_Procedure and an additionalCode declared, second SequenceNumber", "2", additionalProceduresList[1].SequenceNumber);
				AssertEquals("Expected filled AdditionalProcedures with JI_Procedure and an additionalCode declared, second Code", "123", additionalProceduresList[1].Code);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			invoiceLine = Factory.New<JobComInvoiceLine>();

			wrapper = GetWrapper(invoiceLine);
		}

		JobComInvoiceLine invoiceLine;
		DeclarationAESProcedureWrapper wrapper;

		DeclarationAESProcedureWrapper GetWrapper(JobComInvoiceLine invoiceLine) => new DeclarationAESProcedureWrapper(invoiceLine);

		protected override DeclarationAESProcedureWrapper GetProvider() => wrapper;
	}
}
