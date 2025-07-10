using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GLJournalControllerImport))]
	public class GLJournalControllerXMLImportTest : GLJournalControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GLJournalImport;
		}
	}
}
