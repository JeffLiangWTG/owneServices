using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class MFLRunDocsTest : ClientSpecificRunDocsTest
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.ARInvoice; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return new ZQuery(StmMenuItemSchema.SU_MenuName, "MFL Invoice"); }
			set { }
		}

		protected override ZString ClientName
		{
			get { return "MFL"; }
		}

		[ExpectNoExceptions()]
		public void TestInvoice()
		{
			RunDocument();
		}
	}
}
