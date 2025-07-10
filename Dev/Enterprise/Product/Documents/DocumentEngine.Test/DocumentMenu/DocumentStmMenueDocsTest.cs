using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	[TestedType(typeof(DocumentStmMenuEDocs))]
	sealed class DocumentStmMenueDocsTest : EnterpriseBusinessObjectTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		public void TestSX_Description()
		{
			DocumentStmMenuEDocs eDocsMenu = Factory.New<DocumentStmMenuEDocs>();
			AssertEquals("Description is a blank string if no FK to RefDocType", ZString.Empty, eDocsMenu.SX_Description);

			RefDocType docType = Factory.LoadTop1<RefDocType>(new ZQuery());
			eDocsMenu.SX_RT_DocType = docType.PK;
			AssertEquals("Description is the RefDocType's description when FK to RefDocType is set", docType.RT_Desc, eDocsMenu.SX_Description);
		}

		public void TestSX_DocType()
		{
			DocumentStmMenuEDocs eDocsMenu = Factory.New<DocumentStmMenuEDocs>();
			AssertEquals("DocType is a blank string if no FK to RefDocType", ZString.Empty, eDocsMenu.SX_DocType);

			RefDocType docType = Factory.LoadTop1<RefDocType>(new ZQuery());
			eDocsMenu.SX_RT_DocType = docType.PK;
			AssertEquals("DocType is the RefDocType's doc type when FK to RefDocType is set", docType.RT_DocType, eDocsMenu.SX_DocType);
		}

		public void TestRootTypeProvider()
		{
			var eDocsMenu = Factory.New<DocumentStmMenuEDocs>();
			AssertEquals(0, eDocsMenu.Roots.Length);
			AssertCollectionContains(typeof(IeDoc), eDocsMenu.RootTypes);
		}
	}
}
