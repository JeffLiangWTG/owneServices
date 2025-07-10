using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Web
{
	[TestedType(typeof(eDocsRequestHandler))]
	sealed class eDocsRequestHandlerTest : DataRequestHandlerTestCase<eDocsRequestHelper>
	{
		#region Setup

		protected override DataRequestHandler<eDocsRequestHelper> GetNewRequestHandler()
		{
			DocumentFactory masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			RefCountry masterBizO = Factory.LoadTop1<RefCountry>(new ZQuery());

			ZQuery mscDocTypeQuery = new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.MiscellaneousDocument);
			RefDocType mscDocType = Factory.LoadTop1<RefDocType>(mscDocTypeQuery);
			mscDocType.RT_IsPublishUpdatable = true;

			Factory.Save();

			StorageMain parent = masterFactory.RetrieveExistingOrCreateStorageMainForPK(masterBizO.PK, ((IDocManagerSupport)masterBizO).DocManagerInfo.DocManagerCode);

			StorageDocs newDocument = parent.Documents.AddNew();
			newDocument.SC_ImageData = new byte[] { 1, 1, 1, 1, 1, 1, 1 };
			newDocument.SC_IsPublished = true;
			newDocument.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			newDocument.SC_SM = parent.PK;
			masterFactory.Save();

			eDocsRequestHandler testHandler = new eDocsRequestHandler();
			testHandler.QueryString.Add("Ref", masterBizO.PK.ToString());
			testHandler.QueryString.Add("Doc", newDocument.PK.ToString());

			return testHandler;
		}

		eDocsRequestHandler eDocsRequestHandler
		{
			get
			{
				return RequestHandler as eDocsRequestHandler;
			}
		}

		#endregion

		#region TestFileName

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "This is necessary to test a situation with the illegal file name symbols in Description")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "This is necessary to test a situation with the illegal file name symbols in Description")]
		public void TestFileName()
		{
			eDocsRequestHandler.Document.SC_Desc = "Test Document";

			eDocsRequestHandler.Document.SC_DataType = ZString.Empty;
			eDocsRequestHandler.Document.SC_FileName = ZString.Empty;

			AssertEquals("The filename must be the description (without an extension) if no file name is specified on the document.", "Test Document", eDocsRequestHandler.FileName);

			eDocsRequestHandler.Document.SC_DataType = "XLS";
			eDocsRequestHandler.Document.SC_FileName = ZString.Empty;

			AssertEquals("The filename must be the description (with an extension) if no file name is specified on the document.", "Test Document.xls", eDocsRequestHandler.FileName);

			eDocsRequestHandler.Document.SC_DataType = "PDF";
			eDocsRequestHandler.Document.SC_FileName = "TestDocument";

			AssertEquals("The filename must be returned as-is if specified on the document.", "TestDocument.pdf", eDocsRequestHandler.FileName);

			eDocsRequestHandler.Document.SC_FileName = "  ";
			eDocsRequestHandler.Document.SC_Desc = "  ";

			AssertEquals("The filename must be its PK with extension if no filename and description is specified on the document.", eDocsRequestHandler.Document.PK.ToString() + ".pdf", eDocsRequestHandler.FileName);

			eDocsRequestHandler.Document.SC_FileName = ZString.Empty;
			eDocsRequestHandler.Document.SC_Desc = ZString.Empty;

			AssertEquals("The filename must be its PK with extension if no filename and description is specified on the document.", eDocsRequestHandler.Document.PK.ToString() + ".pdf", eDocsRequestHandler.FileName);

			eDocsRequestHandler.Document.SC_DataType = ZString.Empty;

			AssertEquals("The filename must be its PK if no filename and description is specified on the document.", eDocsRequestHandler.Document.PK.ToString(), eDocsRequestHandler.FileName);

			eDocsRequestHandler.Document.SC_Desc = @"Some PDF file at c:\Tmp"; // This is necessary to test a situation with the illegal file name symbols in Description
			AssertEquals("The filename must be wwth the illegal symbols replaced with space.", @"Some PDF file at c  Tmp", eDocsRequestHandler.FileName);
		}
		#endregion

		#region TestFileName_DocumentIsNull

		public void TestFileName_DocumentIsNull()
		{
			eDocsRequestHandler.BusinessObjects[0] = null;
			var poke = "poke me!";
			poke = eDocsRequestHandler.FileName;
			AssertEquals("*INVALID FILENAME*", poke);
		}

		#endregion

		#region TestGetContentTypeDoesNotThrowException

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "This is necessary to test a situation with the illegal file name symbols in Description")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "This is necessary to test a situation with the illegal file name symbols in Description")]
		public void TestGetContentTypeDoesNotThrowException()
		{
			eDocsRequestHandler.Document.SC_DataType = "PDF";
			eDocsRequestHandler.Document.SC_FileName = "  ";
			eDocsRequestHandler.Document.SC_Desc = "  ";
			AssertEquals("The ContentType must be PDF.", "application/pdf", eDocsRequestHandler.ContentType);

			eDocsRequestHandler.Document.SC_FileName = ZString.Empty;
			eDocsRequestHandler.Document.SC_Desc = ZString.Empty;
			AssertEquals("The ContentType must be PDF.", "application/pdf", eDocsRequestHandler.ContentType);

			char[] invalidChars = Path.GetInvalidFileNameChars();
			StringBuilder desc = new StringBuilder(@"Some PDF file at c:\Tmp"); // This is necessary to test a situation with the illegal file name symbols in Description
			foreach (char invalidChar in invalidChars)
			{
				desc.Append(invalidChar);
			}
			eDocsRequestHandler.Document.SC_Desc = desc.ToString();
			AssertEquals("The ContentType must be PDF.", "application/pdf", eDocsRequestHandler.ContentType);

			eDocsRequestHandler.Document.SC_Desc = ZString.Empty;
			eDocsRequestHandler.Document.SC_DataType = ZString.Empty;
			AssertNull("The ContentType must be null.", eDocsRequestHandler.ContentType);
		}
		#endregion

		public override void TestGetBinaryDataWithLock()
		{
			AssertNotNull("Nothing to lock in this class");
		}
	}
}
