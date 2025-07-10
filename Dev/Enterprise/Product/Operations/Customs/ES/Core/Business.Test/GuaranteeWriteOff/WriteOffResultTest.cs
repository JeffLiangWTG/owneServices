using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(WriteOffResult))]
	public class WriteOffResultTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new WriteOffResult("", "", "", "");
		}

		public void TestSendColumn()
		{
			CombineAssertions(() =>
			{
				var writeOfResult = new WriteOffResult("Number", "ES12345678912345", "Gua Status", "Dec Status");
				AssertEquals("JobNumber has the correct value", "Number", writeOfResult.JobNumber);
				AssertEquals("Mrn has the correct value", "ES12345678912345", writeOfResult.Mrn);
				AssertEquals("GuaranteeStatus has the correct value", "Gua Status", writeOfResult.GuaranteeStatus);
				AssertEquals("DeclarationStatus has the correct value", "Dec Status", writeOfResult.DeclarationStatus);
			});
		}
	}
}
