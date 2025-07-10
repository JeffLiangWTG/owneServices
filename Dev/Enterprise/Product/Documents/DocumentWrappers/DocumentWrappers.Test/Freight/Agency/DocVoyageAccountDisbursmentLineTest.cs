using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocVoyageAccountDisbursementLine))]
	sealed class DocVoyageAccountDisbursmentLineTest : DocumentWrapperTestCase
	{
		public void TestDescription()
		{
			AssertEquals("Description", DocVoyageAccountDisbursementLine.New("Group", "Description", Factory).Description);
		}

		public void TestGroup()
		{
			AssertEquals("Group", DocVoyageAccountDisbursementLine.New("Group", "Description", Factory).Group);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocVoyageAccountDisbursementLine.New("group", "description", Factory),
			};
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocVoyageAccountDisbursementLine.New("group", "description", Factory);
		}

		#endregion
	}
}
