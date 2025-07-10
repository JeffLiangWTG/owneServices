using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StmNoteContextsTest : TestCase
	{
		public void TestToString()
		{
			StmNoteContexts contexts = new StmNoteContexts();
			AssertEquals("precondition: StmNoteContexts.Module", StmNoteContextModule.Undefined, contexts.Module);
			AssertEquals("precondition: StmNoteContexts.Direction", StmNoteContextDirection.Undefined, contexts.Direction);
			AssertEquals("precondition: StmNoteContexts.FreightMode", StmNoteContextFreightMode.Undefined, contexts.FreightMode);

			AssertEquals("'Undefined' values should be converted to ' ' (space) by StmNoteContexts.ToString() method", "___", contexts.ToString());

			contexts.Module |= StmNoteContextModule.A;
			contexts.Direction |= StmNoteContextDirection.A;
			contexts.FreightMode |= StmNoteContextFreightMode.A;

			AssertEquals("Other than 'Undefined' values should not be changed while converted by StmNoteContexts.ToString() method", "AAA", contexts.ToString());
		}
	}
}
