using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(DataContentTypes))]

namespace Enterprise.DocumentEngine.Web.Testing
{
	sealed class DocumentUtilityTest : TestCaseWithFactory
	{
		#region TestGetDocument

		public void TestGetDocument()
		{
			string[] supportedContentTypes = new string[] { DataContentTypes.Pdf, DataContentTypes.Excel };

			foreach (string contentType in supportedContentTypes)
			{
				byte[] data = utility.GetDocument(bizO, "Client Overview", contact, contentType);
				AssertGetDocument("Get document using business object and command name", data);

				using (DocumentPack pack = utility.GetDocumentPack(bizO, "Client Overview", contact))
				{
					data = utility.GetDocument(pack, contact, contentType);
				}
				AssertGetDocument("Get document using document pack", data);
			}
		}

		public void TestGetDocument_WithEDocs()
		{
			var documentCommandName = "Client Overview";
			var documentCommand = DocumentCommand.GetDocumentCommand(Factory, bizO, documentCommandName);
			documentCommand.Parent = bizO;

			var docType = Factory.LoadTop1<RefDocType>(new ZQuery());
			var stmEDoc = Factory.New<StmMenuEDocs>();
			stmEDoc.SX_SU = documentCommand.PK;
			stmEDoc.SX_RT_DocType = docType.PK;

			var org = bizO as OrgHeader;
			var eDoc = org.DocManagerInfo().AddFileOrDocument(new byte[] { 0x4b, 0x65, 0x6c, 0x76, 0x69, 0x6e }, "Kelvin.pdf", docType.RT_DocType);
			eDoc.IsPublished = true;

			Factory.Save();

			var supportedContentTypes = new string[] { DataContentTypes.Pdf, DataContentTypes.Excel };

			foreach (string contentType in supportedContentTypes)
			{
				byte[] data = utility.GetDocument(bizO, documentCommandName, contact, contentType);
				AssertGetDocument("Get document using business object and command name", data);

				using (DocumentPack pack = utility.GetDocumentPack(bizO, documentCommandName, contact))
				{
					AssertEquals(2, pack.Count);
					AssertEquals(1, pack.OfType<Report>().Count());
					AssertEquals(1, pack.OfType<IeDoc>().Count());

					data = utility.GetDocument(pack, contact, contentType);
					AssertGetDocument("Get document using document pack", data);
				}
			}
		}

		void AssertGetDocument(string message, byte[] data)
		{
			AssertNotNull(data);
			Assert(message + ": expected non-zero length", data.Length > 0);
		}

		#endregion

		#region TestInvalidInputData

		public void TestInvalidInputData()
		{
			var data = utility.GetDocument(null, "Client Overview", contact, DataContentTypes.Pdf);
			AssertInvalidInputData("Business object equals null", data);

			data = utility.GetDocument(bizO, "NonExistentCommand", contact, DataContentTypes.Pdf);
			AssertInvalidInputData("Document Command name is invalid", data);

			data = utility.GetDocument(bizO, "Client Overview", null, DataContentTypes.Pdf);
			AssertInvalidInputData("Contact equals null", data);

			data = new DocumentUtility(Factory, false).GetDocument(bizO, "Client Overview", null, DataContentTypes.Pdf);
			Assert("Contact equals null but not required so should have data", data.Length > 0);

			data = utility.GetDocument(bizO, "Client Overview", contact, "foo");
			AssertInvalidInputData("Unknown content type", data);
		}

		#region TestGetDocumentInternalStreamLengthZero

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDocumentInternalStreamLengthZero()
		{
			var packWithOneReport = new DocumentPack();
			packWithOneReport.Add(new MockReport());
			utility.GetDocument(packWithOneReport, contact, DataContentTypes.Pdf);

			var packWithMultiReport = new DocumentPack();
			packWithMultiReport.Add(new MockReport());
			packWithMultiReport.Add(new MockReport());
			packWithMultiReport.Add(new MockReport());
			AssertEquals(Array.Empty<byte>(), utility.GetDocument(packWithMultiReport, contact, DataContentTypes.Pdf));
		}

		#endregion

		void AssertInvalidInputData(string message, byte[] data)
		{
			AssertNotNull(data);
			AssertEquals(message + ": expected zero length", 0, data.Length);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			utility = new DocumentUtility(Factory);
			bizO = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			contact = Factory.NewWithValidTestData<OrgHeader>().Contacts.AddNew();
		}

		DocumentUtility utility;
		IDocumentSupportable bizO;
		OrgContact contact;

		#endregion

		class MockReport : Report
		{
			public MockReport()
				: base(new DocumentPack(), new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", TestFilesSubFolder.ReportTestFiles))
			{
			}

#if NETFRAMEWORK
			protected MockReport(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext ctx)
				: base(new DocumentPack(), new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", TestFilesSubFolder.ReportTestFiles))
			{
			}
#endif

			public override void Save(DocDeliveryContact deliveryContact, DocDeliveryContact mostOfficialContact, Stream fileContent)
			{
			}
		}
	}
}
