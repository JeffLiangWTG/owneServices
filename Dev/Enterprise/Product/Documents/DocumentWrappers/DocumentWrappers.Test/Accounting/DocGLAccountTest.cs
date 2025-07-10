using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocGLAccount))]
	sealed class DocGLAccountTest : DocumentWrapperTestCase
	{
		public void TestStatisticalUnits()
		{
			var docGLAccount = DocGLAccount.New(GLHeader, Factory);
			AssertEquals("Pre-condition", "", docGLAccount.StatisticalUnits);

			GLHeader.AG_AccountType = AccountType.Note;
			GLHeader.AG_StatisticalUnits = "KWH";
			AssertEquals("Statistical Units", "KWH", docGLAccount.StatisticalUnits);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocGLAccount.New(GLHeader, Factory)
			};
		}

		AccGLHeader GLHeader;
		protected override void SetUp()
		{
			GLHeader = Factory.New<AccGLHeader>();
			base.SetUp();
		}

		#endregion
	}
}
