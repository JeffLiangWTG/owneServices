using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	public class SupportingDocumentWrapperWithoutDocumentTest : TestCaseWithFactory
	{
		public void TestISupportingDocumentOnlyMembers()
		{
			var wrapper = new SupportingDocumentWrapperWithoutDocument("code", "ref", new ZDateTime(2023, 08, 31));
			AssertEquals(false, wrapper.IsD48AndNotClosed);
			AssertEquals(ZDecimal.Zero, wrapper.D48Amount);
			AssertEquals((sbyte)0, wrapper.D48Deadline);
			AssertEquals(false, wrapper.IsUnderInvoiceLine);
			AssertEquals(Enumerable.Empty<IImputationSheet>(), wrapper.ImputationsSheets);
			AssertEquals("code", wrapper.Code);
			AssertEquals("", wrapper.Description);
			AssertEquals("", wrapper.Type);
			AssertEquals("ref", wrapper.RefNumber);
			AssertEquals(new ZDateTime(2023, 08, 31), wrapper.DateIssue);
			AssertEquals("", wrapper.PFAIdentification);
			AssertEquals("", wrapper.PFADocument);
		}
	}
}
