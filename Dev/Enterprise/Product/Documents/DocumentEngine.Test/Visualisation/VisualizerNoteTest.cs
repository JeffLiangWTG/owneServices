using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.DocumentEngine.Visualisation.VisualizerNote;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	[TestedType(typeof(VisualizerNote))]
	sealed class VisualizerNoteTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			VisualizerNote result = (VisualizerNote)base.GetNewBusinessObjectForDeleteTest(factory);
			OrgHeader parent = factory.NewWithValidTestData<OrgHeader>();
			parent.OH_Code = "AXA";

			result.DD_ParentID = parent.PK;
			result.DD_ParentTableCode = parent.TableCode;
			result.DD_DocumentData = ZBlob.FromAscii("abc");

			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader parent = Factory.NewWithValidTestData<OrgHeader>();
			var result = Factory.New<VisualizerNote>();
			parent.OH_Code = "AXB";

			result.DD_ParentID = parent.PK;
			result.DD_ParentTableCode = parent.TableCode;
			result.DD_DocumentData = ZBlob.FromAscii("abc");

			return result;
		}

		public void TestUniqueIndexViolationException()
		{
			var documentMenu = Factory.New<StmMenuItem>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			var visualizerNoteSupporter = new VisualizerNoteSupporter(org);
			Factory.Save();
			VisualizerNote foo = VisualizerNote.Get(Factory, documentMenu, documentMenu, visualizerNoteSupporter);
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			VisualizerNote foo2 = VisualizerNote.Get(factory2, documentMenu, documentMenu, visualizerNoteSupporter);
			foo.DD_DocumentData = ZBlob.FromAscii("abc");
			foo2.DD_DocumentData = ZBlob.FromAscii("123");
			Factory.Save();

			try
			{
				factory2.Save();
				Fail("An exception should be thrown.");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				factory2.Save();
			}

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			VisualizerNote foo3 = VisualizerNote.Get(factory3, documentMenu, documentMenu, visualizerNoteSupporter);
			AssertEquals("123", foo3.DD_DocumentData.ToAscii());
		}

		public void TestVisualizerNoteNewFormat()
		{
			var documentMenu = Factory.New<StmMenuItem>();
			var newVisualizerNote = VisualizerNote.Get(Factory, documentMenu, documentMenu, null);
			AssertNull(newVisualizerNote);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			var visualizerNoteSupporter = new VisualizerNoteSupporter(org);
			VisualizerNote foo = VisualizerNote.Get(Factory, documentMenu, documentMenu, visualizerNoteSupporter);
			foo.DD_DocumentData = ZBlob.FromAscii("abc");

			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			VisualizerNote foo2 = VisualizerNote.Get(factory2, documentMenu, documentMenu, visualizerNoteSupporter);
			AssertEquals("Same PK", foo.PK, foo2.PK);
			AssertEquals("ST_ParentID is business object to log against's PK", org.PK, foo2.DD_ParentID);
			AssertEquals("ST_Description is menu item's PK", documentMenu.PK, foo2.DD_SU);
			AssertEquals("ST_Table is business object to log against's table", org.TableCode, foo2.DD_ParentTableCode);
		}

		public void TestIDocumentSupportableIsUsedToLoadNote_CS00387217()
		{
			AssertIDocumentSupportableIsUsedToLoadNote_CS00387217(false);
		}

		public void TestIDocumentSupportableIsUsedToLoadNote_CS00387217_Legacy()
		{
			AssertIDocumentSupportableIsUsedToLoadNote_CS00387217(true);
		}

		void AssertIDocumentSupportableIsUsedToLoadNote_CS00387217(bool isLegacy)
		{
			var registrySQL = $@"
INSERT INTO [dbo].[StmData] (SD_PK, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemCreateTimeUtc, SD_Type, SD_BinaryValue, SD_Owner, SD_Name)
VALUES (newid(), 'A', '2024-01-18T09:00:00', 'A', '2024-01-18T09:00:00', 'BOL', {(isLegacy ? "0x5400720075006500" : "0x460061006C0073006500")}, NULL, 'UseNewFormBuilderBillOfLading')
";
			TestConnection.ExecuteNonQuery(registrySQL);

			var businessObject = (BusinessObject)Factory.New<Enterprise.Freight.Integration.Agency.IBillOfLading>();

			var query = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill of Lading");
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "AgencyDocumentation");
			query.AddToFilter(StmMenuItemSchema.SU_MenuPath, (isLegacy ? (Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments + "/") : string.Empty) + "Export");
			query.AddToFilter(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.Documents);
			query.AddToFilter(StmMenuItemSchema.SU_GS_NKStaffCode, "");
			query.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
			query.AddToFilter(StmMenuItemSchema.SU_IsClientSpecific, false);

			var billOfLadingMenuItem = Factory.LoadTop1<StmMenuItem>(query);
			AssertNotNull("Bill Of Lading Menu should exists", billOfLadingMenuItem);

			var visualizerNoteSupporter = new VisualizerNoteSupporter(businessObject);
			var billOfLadingVisualNote = VisualizerNote.Get(Factory, billOfLadingMenuItem, billOfLadingMenuItem, visualizerNoteSupporter);

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Send Electronic Original Bill of Lading";
			AssertEquals("Should use Bill Of Lading Visual Note", billOfLadingVisualNote, VisualizerNote.Get(Factory, menuItem, menuItem, visualizerNoteSupporter));
		}

		public void TestNoNotesGetCreatedForSupporterWithNoPKOrTableCode()
		{
			var documentMenu = Factory.New<StmMenuItem>();
			var document = Factory.New<StmMenuItem>();
			var org = Factory.New<OrgHeader>();
			AssertNotNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.NewZGuid(), OrgHeaderSchema.Constants.Prefix)));
			AssertNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.Empty, OrgHeaderSchema.Constants.Prefix)));
			AssertNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.NewZGuid(), string.Empty)));
			AssertNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.Empty, string.Empty)));
			ErrorReporter.Clear();
		}

		public void TestNoNotesGetCreatedIfMenuItemDoesntSupportVisualisation()
		{
			var documentMenu = Factory.New<StmMenuItem>();
			documentMenu.SU_SupportsVisualisation = false;
			var org = Factory.New<OrgHeader>();
			AssertNull(VisualizerNote.Get(Factory, documentMenu, documentMenu, new MockVisualizerNoteSupporter(org, ZGuid.NewZGuid(), OrgHeaderSchema.Constants.Prefix)));

			documentMenu.SU_SupportsVisualisation = true;
			var document = Factory.New<StmMenuItem>();
			document.SU_SupportsVisualisation = false;
			AssertNotNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.NewZGuid(), OrgHeaderSchema.Constants.Prefix)));

			document.SU_SupportsVisualisation = true;
			AssertNotNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.NewZGuid(), OrgHeaderSchema.Constants.Prefix)));
		}

		public void TestIssueIsReportedIfSupporterPKOrCodeIsEmpty()
		{
			var documentMenu = Factory.New<StmMenuItem>();
			documentMenu.SU_IsSystemDefined = true;
			documentMenu.SU_MenuName = "Name";
			documentMenu.SU_BusinessContext = "Context";
			documentMenu.SU_ContactType = ContactType.All.Code;
			documentMenu.SU_MenuPath = "Menu/Path/";
			documentMenu.SU_SupportsVisualisation = false;

			var org = Factory.New<OrgHeader>();
			AssertNull(VisualizerNote.Get(Factory, documentMenu, documentMenu, new MockVisualizerNoteSupporter(org, ZGuid.NewZGuid(), OrgHeaderSchema.Constants.Prefix)));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			AssertNull(VisualizerNote.Get(Factory, documentMenu, documentMenu, new MockVisualizerNoteSupporter(org, ZGuid.Empty, OrgHeaderSchema.Constants.Prefix)));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			AssertNull(VisualizerNote.Get(Factory, documentMenu, documentMenu, new MockVisualizerNoteSupporter(org, ZGuid.NewZGuid(), string.Empty)));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			documentMenu.SU_SupportsVisualisation = true;
			var document = Factory.New<StmMenuItem>();
			document.SU_IsSystemDefined = true;
			document.SU_SupportsVisualisation = false;

			AssertNotNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.NewZGuid(), OrgHeaderSchema.Constants.Prefix)));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			AssertNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.Empty, OrgHeaderSchema.Constants.Prefix)));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			AssertNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.NewZGuid(), string.Empty)));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			document.SU_SupportsVisualisation = true;

			var expectedErrorMessage = @"The document 'Context : Name : Menu/Path : ALL' supports modifications (SU_SupportsVisualisation = 1) but it's using 'Enterprise.DocumentEngine.Visualisation.Testing.VisualizerNoteTest+MockVisualizerNoteSupporter' to store them and that business object does not provide a persistent PK and Table Code.
If your business object in non-persistent, make sure it implements IVisualizerNoteSupporter properly.";

			AssertNotNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.NewZGuid(), OrgHeaderSchema.Constants.Prefix)));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			AssertNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.Empty, OrgHeaderSchema.Constants.Prefix)));
			AssertEquals(expectedErrorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.NewZGuid(), string.Empty)));
			AssertEquals(expectedErrorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			document.SU_IsSystemDefined = false;
			AssertNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.NewZGuid(), string.Empty)));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			documentMenu.SU_IsSystemDefined = false;
			document.SU_IsSystemDefined = true;
			AssertNull(VisualizerNote.Get(Factory, documentMenu, document, new MockVisualizerNoteSupporter(org, ZGuid.NewZGuid(), string.Empty)));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var result = (VisualizerNote)Factory.NewWithValidTestData(GetExpectedBusinessObjectType(), TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections);
			result.DD_ParentTableCode = "Z0";
			return result;
		}

		class MockVisualizerNoteSupporter : NonPersistentBusinessObject, IVisualizerNoteSupporter
		{
			public MockVisualizerNoteSupporter(BusinessObject businessObjectToLogAgainst, ZGuid parentPK, ZString parentTableCode)
			{
				this.businessObjectToLogAgainst = businessObjectToLogAgainst;
				this.parentPK = parentPK;
				this.parentTableCode = parentTableCode;
			}

			public MockVisualizerNoteSupporter(BusinessObject businessObjectToLogAgainst, BusinessObject childBusinessObjectToLogAgainst, ZGuid parentPK, ZString parentTableCode)
				: this(businessObjectToLogAgainst, parentPK, parentTableCode)
			{
				this.childBusinessObjectToLogAgainst = childBusinessObjectToLogAgainst;
				this.parentPK = parentPK;
				this.parentTableCode = parentTableCode;
			}

			readonly BusinessObject businessObjectToLogAgainst;
			readonly BusinessObject childBusinessObjectToLogAgainst;
			readonly ZGuid parentPK;
			readonly ZString parentTableCode;

			#region IVisualizerNoteSupporter Members

			ZGuid IVisualizerNoteSupporter.PK => parentPK;

			ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => childBusinessObjectToLogAgainst == null ? ZGuid.Empty : childBusinessObjectToLogAgainst.PK;

			string IVisualizerNoteSupporter.TableCode => parentTableCode;

			#endregion

			internal IDocumentSupportable GetDocumentSupportable()
			{
				return businessObjectToLogAgainst as IDocumentSupportable;
			}
		}
	}
}
