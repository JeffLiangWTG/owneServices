using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.Testing
{
	public class RatingDocPackBuilderTest : TestCaseWithFactory
	{
		public void TestAddPages_WhenCallingAddPages_ThenDocPackUserDefinedFieldListShouldBeSet()
		{
			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "Master Menu";

			var pivot = CreateFixedMenu(Factory, "TemplateX").Documents[0];
			pivot.SI_MenuTemplateFilter = "\"<JobNumber>\" == \"WrapperX\"";

			Factory.Save();

			var dummyBizO = Factory.New<DummyBODocSupportable>();

			var note = DocumentNote.LoadNote(dummyBizO);
			var userDefinedField = new TextField(Factory) { DisplayName = "FieldX" };
			note.UserDefinedFieldList.Add(userDefinedField);

			var wrapperDummyBizO = Factory.New<DummyBusinessObject>();
			var wrapper = new DummyWrapper(wrapperDummyBizO, Factory) { JobNumber = "WrapperX" };

			var builder = new RatingDocPackBuilder(command, dummyBizO, Callback);
			builder.AddPages(pivot, new DocumentWrapper[] { wrapper });

			const string expected1 =
@"{B}-[TemplateX Pivot]   {C}-[WrapperX]";

			using (builder.DocPack)
			{
				AssertEquals("DocPack count", 1, builder.DocPack.Count);
				var deliverable = builder.DocPack[0];
				AssertReport("Report", expected1, deliverable);

				var report = (Report)deliverable;
				CombineAssertions("UserDefinedField should exist", () =>
				{
					AssertNotNull("UserDefinedField should not null", report.UserDefinedFieldValueList);
					AssertEquals("UserDefinedField count", 1, report.UserDefinedFieldValueList.Count);
					AssertEquals("UserDefinedField DisplayName", "FieldX", report.UserDefinedFieldValueList[0].DisplayName);
				});
			}
		}

		public void TestDocPackContainsCustomizedDocument()
		{
			var parent = Factory.New<DummySupportable>();

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "Master Menu";
			command.SU_IsSystemDefined = true;
			command.Parent = parent;

			var pivot = CreateFixedMenu(Factory, "Fixed").Documents[0];

			Factory.Save();

			var wrapper = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Wrapper" };

			var builder = new RatingDocPackBuilder(command, parent, b => b.AddPages(pivot, new DocumentWrapper[] { wrapper }));

			using (builder.DocPack)
			{
				AssertEquals(1, builder.DocPack.Count);
				var report = (Report)builder.DocPack[0];
				Assert("The report should contains customization", report.ContainsAnyCustomisation);
			}
		}

		public void TestAddPages()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "XXX";

			var parent = Factory.New<DummySupportable>();
			parent.Client = client;

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "Master Menu";
			command.SU_ContactType = ContactType.LocalClient.Code;
			command.Parent = parent;

			StmMenuTemplatePivot pivot1 = CreateFixedMenu(Factory, "Fixed").Documents[0];

			var testTemplate =
@"{A}-[#config]
		{A}-[Name=TemplateWithGenericSections]
		{A}-[#ConfigurableSection:BOD, JobNumber]
		{A}-[#SectionBody]
		{B}-[<JobNumber>]
		{C}-[<Client.OH_Code>]
		{A}-[#ConfigurableSection:BOD, ReportName]
		{A}-[#SectionBody]
		{B}-[<ReportName>] {C}-[Yes]
		{A}-[#ConfigurableSection:BOD, Magic1]
		{A}-[#SectionBody]
		{B}-[Magic]
		{A}-[#ConfigurableSection:BOD, Magic2]
		{A}-[#SectionBody]
		{B}-[Magic] {C}-[More Magic]
		{A}-[#EndOfReport]";
			StmMenuTemplatePivot pivot2 = CreateStripMenu(Factory, "Strip", testTemplate).Documents[0];

			var docType = Factory.New<RefDocType>();
			docType.RT_SE_NKDocumentReceivedEvent = Events.ExportReceivalAdvisePrinted.Code;
			docType.RT_DocType = "AAA";
			docType.RT_ReferenceType = "ALL";
			pivot1.SI_RT_DocType = docType.PK;

			Factory.Save();

			var wrapper1 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Wrapper1" };
			var wrapper2 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Wrapper2" };

			var builder = new RatingDocPackBuilder(command, parent, new RatingDocPackBuilderCalllback(b =>
			{
				b.AddPages(pivot1, new DocumentWrapper[] { wrapper1, wrapper2 });
				b.AddPages(pivot2, new DocumentWrapper[] { wrapper1, wrapper2 });
			}));

			const string expected1 =
@"{B}-[Fixed Pivot]   {C}-[Wrapper1]
";

			const string expected2 =
@"{B}-[Fixed Pivot]   {C}-[Wrapper2]
";

			const string expected3 =
@"{B}-[Strip Pivot]   {C}-[Yes]
{B}-[Wrapper1]
{C}-[XXX]
";

			const string expected4 =
@"{B}-[Strip Pivot]   {C}-[Yes]
{B}-[Wrapper2]
{C}-[XXX]
";

			using (builder.DocPack)
			{
				AssertEquals(4, builder.DocPack.Count);
				AssertReport("Report 1", expected1, builder.DocPack[0]);
				AssertReport("Report 2", expected2, builder.DocPack[1]);
				AssertReport("Report 3", expected3, builder.DocPack[2]);
				AssertReport("Report 4", expected4, builder.DocPack[3]);

				AssertTemplate("Template 1", pivot1.Template, builder.DocPack[0]);
				AssertTemplate("Template 2", pivot1.Template, builder.DocPack[1]);
				AssertTemplate("Template 3", pivot2.Template, builder.DocPack[2]);
				AssertTemplate("Template 4", pivot2.Template, builder.DocPack[3]);

				var instructions = new DeliveryInstructions(builder.DocPack);
				instructions.Language = Enterprise.Core.Constants.Languages.French;
				builder.DocPack.Language = Enterprise.Core.Constants.Languages.French;
				builder.DocPack.RebuildIfLanguageChanged(instructions);
				AssertReport("Report 1", expected1, builder.DocPack[0]);
				AssertReport("Report 2", expected2, builder.DocPack[1]);
				AssertReport("Report 3", expected3.Replace("{C}-[Yes]", "{C}-[Oui]"), builder.DocPack[2]);
				AssertReport("Report 4", expected4.Replace("{C}-[Yes]", "{C}-[Oui]"), builder.DocPack[3]);

				AssertEquals("Document Delivered code: ERA", "ERA", builder.DocPack[0].DocumentDeliveredEventCode);
				AssertEquals("Document Delivered code: ERA", "ERA", builder.DocPack[1].DocumentDeliveredEventCode);
				AssertEquals("Document Delivered code: DDV", "DDV", builder.DocPack[2].DocumentDeliveredEventCode);
				AssertEquals("Document Delivered code: DDV", "DDV", builder.DocPack[3].DocumentDeliveredEventCode);
			}
		}

		public void TestAddPages_IncludedInPrint_ShouldHonorMenuTemplatePivotPrintFlag()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "Master Menu";
			command.SU_ContactType = ContactType.LocalClient.Code;

			var pivot1 = CreateStripMenu(Factory, "Strip1").Documents[0];
			var pivot2 = CreateStripMenu(Factory, "Strip2").Documents[0];

			pivot2.SI_PrintByDefault = false;

			Factory.Save();

			DummySupportable parent = Factory.New<DummySupportable>();
			parent.Client = client;

			DummyWrapper wrapper1 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Wrapper1" };
			DummyWrapper wrapper2 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Wrapper2" };

			RatingDocPackBuilder builder = new RatingDocPackBuilder(command, parent, b =>
			{
				b.AddPages(pivot1, [wrapper1]);
				b.AddPages(pivot2, [wrapper2]);
			});

			AssertEquals(2, builder.DocPack.Count);
			Assert("Should be true because pivot1 is print by default", builder.DocPack[0].IncludedInPrint);
			Assert("Should be false because pivot2 is not print by default", !builder.DocPack[1].IncludedInPrint);

			// mimic user overriding on the UI
			builder.DocPack[0].IncludedInPrint = false;
			builder.DocPack[1].IncludedInPrint = true;
			builder.RebuildAll();

			Assert("After rebuild, the value of IncludedInPrint should not change", !builder.DocPack[0].IncludedInPrint);
			Assert("After rebuild, the value of IncludedInPrint should not change", builder.DocPack[1].IncludedInPrint);
		}

		public void TestAddPagesCopiesDocType()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "Master Menu";
			command.SU_ContactType = ContactType.LocalClient.Code;

			StmMenuTemplatePivot pivot1 = CreateFixedMenu(Factory, "Fixed").Documents[0];

			var docType = Factory.New<RefDocType>();
			docType.RT_SE_NKDocumentReceivedEvent = Events.ExportReceivalAdvisePrinted.Code;
			docType.RT_DocType = "QUO";
			docType.RT_ReferenceType = "ALL";
			pivot1.SI_RT_DocType = docType.PK;

			Factory.Save();

			var parent = Factory.New<DummySupportable>();
			parent.Client = client;

			var wrapper1 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Wrapper1" };
			var wrapper2 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Wrapper2" };

			RatingDocPackBuilder builder = new RatingDocPackBuilder(command, parent, b => b.AddPages(pivot1, new DocumentWrapper[] { wrapper1, wrapper2 }));

			using (builder.DocPack)
			{
				AssertEquals(2, builder.DocPack.Count);
				AssertEquals("Document type has been copied.", "QUO", ((Report)builder.DocPack[0]).DocTypeCode);
				AssertEquals("Document type has been copied.", "QUO", ((Report)builder.DocPack[1]).DocTypeCode);
			}
		}

		public void TestPivotFilter()
		{
			DocumentCommand command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "Master Menu";

			StmMenuTemplatePivot pivot1 = CreateFixedMenu(Factory, "Template1").Documents[0];
			pivot1.SI_MenuTemplateFilter = "\"<JobNumber>\" == \"Wrapper1\"";

			StmMenuTemplatePivot pivot2 = CreateFixedMenu(Factory, "Template2").Documents[0];
			pivot2.SI_MenuTemplateFilter = "\"<JobNumber>\" == \"Wrapper2\"";

			Factory.Save();

			DummyWrapper wrapper1 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Wrapper1" };
			DummyWrapper wrapper2 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Wrapper2" };

			RatingDocPackBuilder builder = new RatingDocPackBuilder(command, Factory.New<DummySupportable>(), Callback);
			builder.AddPages(pivot1, new DocumentWrapper[] { wrapper1, wrapper2 });
			builder.AddPages(pivot2, new DocumentWrapper[] { wrapper1, wrapper2 });

			const string expected1 =
@"{B}-[Template1 Pivot]   {C}-[Wrapper1]";

			const string expected2 =
@"{B}-[Template2 Pivot]   {C}-[Wrapper2]";

			using (builder.DocPack)
			{
				AssertEquals(2, builder.DocPack.Count);
				AssertReport("Report 1", expected1, builder.DocPack[0]);
				AssertReport("Report 2", expected2, builder.DocPack[1]);
			}
		}

		public void TestExpressionEvaluationExceptionPivotFilter()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var command = Factory.New<DocumentCommand>();

			var pivot = CreateStripMenu(Factory, "Strip").Documents[0];
			pivot.SI_MenuTemplateFilter = "Y == Y && \\==SK<\\>>";

			Factory.Save();

			var parent = Factory.New<DummySupportable>();
			parent.Client = client;

			var wrapper1 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory);
			var wrapper2 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory);

			new RatingDocPackBuilder(command, parent, b =>
				AssertNoExceptionThrown("ExpressionEvaluationException is caught.",
					() => b.AddPages(pivot, new DocumentWrapper[] { wrapper1, wrapper2 })));
		}

		public void TestStripFilter()
		{
			DocumentCommand command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "Master Menu";

			StmMenuTemplatePivot pivot = CreateStripMenu(Factory, "Template").Documents[0];

			Factory.Save();

			DummyWrapper wrapper1 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Magic1" };
			DummyWrapper wrapper2 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Magic2" };
			DummyWrapper wrapper3 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "NoMagic" };

			RatingDocPackBuilder builder = new RatingDocPackBuilder(command, Factory.New<DummySupportable>(), Callback);
			builder.AddPages(pivot, new DocumentWrapper[] { wrapper1, wrapper2, wrapper3 });

			const string expected1 =
@"{B}-[Template Pivot]   {C}-[Yes]
{B}-[Magic1]
{B}-[Magic]";

			const string expected2 =
@"{B}-[Template Pivot]   {C}-[Yes]
{B}-[Magic2]
{B}-[Magic]   {C}-[More Magic]";

			const string expected3 =
@"{B}-[Template Pivot]   {C}-[Yes]
{B}-[NoMagic]";

			using (builder.DocPack)
			{
				AssertEquals(3, builder.DocPack.Count);
				AssertReport("Report 1", expected1, builder.DocPack[0]);
				AssertReport("Report 2", expected2, builder.DocPack[1]);
				AssertReport("Report 3", expected3, builder.DocPack[2]);
			}
		}

		public void TestStripFilterForBusinessObjectOnlyProperties()
		{
			var dummySupportable1 = Factory.New<DummySupportable>();
			dummySupportable1.Client = DocumentEngineTestHelper.GetNewOrganization("MAGIC", Factory);

			var command1 = Factory.New<DocumentCommand>();
			command1.SU_MenuName = "Mister Menu 1";
			command1.Parent = dummySupportable1;

			var dummySupportable2 = Factory.New<DummySupportable>();
			dummySupportable2.Client = DocumentEngineTestHelper.GetNewOrganization("BORING", Factory);

			var command2 = Factory.New<DocumentCommand>();
			command2.SU_MenuName = "Mister Menu 2";
			command2.Parent = dummySupportable2;

			var pivot = CreateStripMenu(Factory, "Template").Documents[0];
			Factory.Save();

			var query = new ZQuery(StmMenuDocumentConfigItemSchema.S4_FilterList, "\"<JobNumber>\" == \"Magic1\"");
			var configMenuItem = Factory.LoadTop1<StmMenuDocumentConfigItem>(query);
			configMenuItem.S4_FilterList = "\"<Client.OH_Code>\" == \"MAGIC\"";
			Factory.Save();

			var message = "dummyBusinessObject1 has a code that matches the filter for configMenuItem1";
			var expected =
@"{B}-[Template Pivot]   {C}-[Yes]
{B}-[00001111]
{B}-[Magic]";

			var builder = new RatingDocPackBuilder(command1, dummySupportable1, Callback);
			var dummyWrapper = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "00001111" };
			builder.AddPages(pivot, new DocumentWrapper[] { dummyWrapper });
			using (builder.DocPack)
			{
				AssertEquals("Pre-condition", 1, builder.DocPack.Count);
				AssertReport(message, expected, builder.DocPack[0]);
			}

			builder = new RatingDocPackBuilder(command2, dummySupportable2, Callback);
			dummyWrapper = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "00002222" };
			builder.AddPages(pivot, new DocumentWrapper[] { dummyWrapper });
			message = "dummyBusinessObject2 has a code that matches the filter for configMenuItem2";
			expected =
@"{B}-[Template Pivot]   {C}-[Yes]
{B}-[00002222]";

			using (builder.DocPack)
			{
				AssertEquals("Pre-condition", 1, builder.DocPack.Count);
				AssertReport(message, expected, builder.DocPack[0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddEDocs()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var dummy = Factory.LoadFromNaturalKey<DummyDocManagerTestBizO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			dummy.SetupDocManagerObjects();

			var supporter = new DummyDocManagerTestBizODocumentSupporter(dummy);

			var docType1 = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "AAA"));
			var docType2 = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "BBB"));
			var docType3 = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "CCC"));

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "master";
			var masterTemplate = helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "Master");
			helper.CreateMenuTemplatePivot("Master document", masterTemplate, command);

			var sub = Factory.New<DocumentCommand>();
			sub.SU_MenuName = "child";
			var childTemplate = helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "Child");
			helper.CreateMenuTemplatePivot("Master document", childTemplate, sub);

			var eDoc1 = command.AddEDoc(docType1);
			var eDoc2 = command.AddEDoc(docType2);
			var eDoc2s = sub.AddEDoc(docType2);
			var eDoc3s = sub.AddEDoc(docType3);

			Factory.Save();

			command.Parent = dummy;
			sub.Parent = dummy;

			RatingDocPackBuilder builder;

			builder = new RatingDocPackBuilder(command, dummy, Callback);
			builder.AddEDocsForMainMenuItem();

			using (builder.DocPack)
			{
				var includeCount = builder.DocPack.Cast<IDeliverable>().Count(x => x.IncludeInPrint);

				AssertEquals("DocPack should have 2 documents", 2, builder.DocPack.Count);
				AssertEquals("DocPack should have 2 documents included in print", 2, includeCount);
				AssertEquals("DocPack should have 1 other eDoc", 1, builder.DocPack.OtherEDocsToAttach.Count);
				AssertEDoc("DocPack[0]", "A Test Document AAA", builder.DocPack[0]);
				AssertEDoc("DocPack[1]", "A Test Document BBB", builder.DocPack[1]);
				AssertEDoc("DocPack[1]", "A Test Document CCC", builder.DocPack.OtherEDocsToAttach[0]);

				builder = new RatingDocPackBuilder(command, dummy, Callback);
				builder.AddEDocsForSubMenuItem(sub);
				includeCount = builder.DocPack.Cast<IDeliverable>().Count(x => x.IncludeInPrint);

				AssertEquals("DocPack should have 2 documents", 2, builder.DocPack.Count);
				AssertEquals("DocPack should have 2 documents included in print", 2, includeCount);
				AssertEquals("DocPack should have 1 other eDoc", 1, builder.DocPack.OtherEDocsToAttach.Count);
				AssertEDoc("DocPack[0]", "A Test Document BBB", builder.DocPack[0]);
				AssertEDoc("DocPack[1]", "A Test Document CCC", builder.DocPack[1]);
				AssertEDoc("DocPack[2]", "A Test Document AAA", builder.DocPack.OtherEDocsToAttach[0]);

				builder = new RatingDocPackBuilder(command, dummy, Callback);
				builder.AddEDocsForMainMenuItem();
				builder.AddEDocsForSubMenuItem(sub);
				includeCount = builder.DocPack.Cast<IDeliverable>().Count(x => x.IncludeInPrint);

				AssertEquals("DocPack should have 3 documents", 3, builder.DocPack.Count);
				AssertEquals("DocPack should have 3 documents included in print", 3, includeCount);
				AssertEquals("DocPack should have 1 other eDoc", 1, builder.DocPack.OtherEDocsToAttach.Count);
				AssertEDoc("DocPack[0]", "A Test Document AAA", builder.DocPack[0]);
				AssertEDoc("DocPack[1]", "A Test Document BBB", builder.DocPack[1]);
				AssertEDoc("DocPack[2]", "A Test Document CCC", builder.DocPack[2]);
			}
		}

		public void TestStmMenuCommand()
		{
			DocumentCommand command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "Master Menu";

			RatingDocPackBuilder builder = new RatingDocPackBuilder(command, Factory.New<DummySupportable>(), Callback);
			using (builder.DocPack)
			{
				AssertEquals(command, builder.DocPack.StmMenuCommand);
			}
		}

		public void TestRebuildKeepsIncludedInPrintOption()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();

			DocumentCommand command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "Master Menu";
			command.SU_ContactType = ContactType.LocalClient.Code;

			StmMenuTemplatePivot pivot1 = CreateFixedMenu(Factory, "Fixed").Documents[0];
			StmMenuTemplatePivot pivot2 = CreateStripMenu(Factory, "Strip").Documents[0];

			var docType = Factory.New<RefDocType>();
			docType.RT_SE_NKDocumentReceivedEvent = Events.ExportReceivalAdvisePrinted.Code;
			docType.RT_DocType = "AAA";
			docType.RT_ReferenceType = "ALL";
			pivot1.SI_RT_DocType = docType.PK;

			Factory.Save();

			DummySupportable parent = Factory.New<DummySupportable>();
			parent.Client = client;

			DummyWrapper wrapper1 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Wrapper1" };
			DummyWrapper wrapper2 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Wrapper2" };

			RatingDocPackBuilder builder = new RatingDocPackBuilder(command, parent, new RatingDocPackBuilderCalllback((b) =>
			{
				b.AddPages(pivot1, new DocumentWrapper[] { wrapper1, wrapper2 });
				b.AddPages(pivot2, new DocumentWrapper[] { wrapper1, wrapper2 });
			}));

			using (builder.DocPack)
			{
				AssertEquals(4, builder.DocPack.Count);
				AssertEquals("DocPack[0].IncludedInPrint", true, builder.DocPack[0].IncludedInPrint);
				AssertEquals("DocPack[1].IncludedInPrint", true, builder.DocPack[1].IncludedInPrint);
				AssertEquals("DocPack[2].IncludedInPrint", true, builder.DocPack[2].IncludedInPrint);
				AssertEquals("DocPack[3].IncludedInPrint", true, builder.DocPack[3].IncludedInPrint);
				var instructions = new DeliveryInstructions(builder.DocPack);
				AssertEquals("instructions.DeliverablesToBePrinted[0].IncludedInPrint", true, instructions.DeliverablesToBePrinted[0].IncludedInPrint);
				AssertEquals("instructions.DeliverablesToBePrinted[1].IncludedInPrint", true, instructions.DeliverablesToBePrinted[1].IncludedInPrint);
				AssertEquals("instructions.DeliverablesToBePrinted[2].IncludedInPrint", true, instructions.DeliverablesToBePrinted[2].IncludedInPrint);
				AssertEquals("instructions.DeliverablesToBePrinted[3].IncludedInPrint", true, instructions.DeliverablesToBePrinted[3].IncludedInPrint);

				instructions.DeliverablesToBePrinted[1].IncludedInPrint = false;
				instructions.DeliverablesToBePrinted[3].IncludedInPrint = false;
				instructions.Language = Core.SharedConstants.Languages.French;
				builder.DocPack.RebuildIfLanguageChanged(instructions);
				AssertEquals("DocPack[0].IncludedInPrint", true, builder.DocPack[0].IncludedInPrint);
				AssertEquals("DocPack[1].IncludedInPrint", false, builder.DocPack[1].IncludedInPrint);
				AssertEquals("DocPack[2].IncludedInPrint", true, builder.DocPack[2].IncludedInPrint);
				AssertEquals("DocPack[3].IncludedInPrint", false, builder.DocPack[3].IncludedInPrint);
				AssertEquals("instructions.DeliverablesToBePrinted[0].IncludedInPrint", true, instructions.DeliverablesToBePrinted[0].IncludedInPrint);
				AssertEquals("instructions.DeliverablesToBePrinted[1].IncludedInPrint", false, instructions.DeliverablesToBePrinted[1].IncludedInPrint);
				AssertEquals("instructions.DeliverablesToBePrinted[2].IncludedInPrint", true, instructions.DeliverablesToBePrinted[2].IncludedInPrint);
				AssertEquals("instructions.DeliverablesToBePrinted[3].IncludedInPrint", false, instructions.DeliverablesToBePrinted[3].IncludedInPrint);
			}
		}

		[ExpectNoExceptions]
		public void TestRebuildSupportsStorageDocsAndStorageFile()
		{
			var command = Factory.New<DocumentCommand>();
			var parent = Factory.New<DummySupportable>();

			var documentFactory = (BusinessObjectFactory)ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(Factory);
			var doc1 = (IDeliverable)documentFactory.New<IStorageFile>();
			var doc2 = (IDeliverable)documentFactory.New<IStorageDocs>();
			doc1.IncludedInPrint = true;
			doc2.IncludedInPrint = true;

			var builder = new RatingDocPackBuilder(command, parent, new RatingDocPackBuilderCalllback((b) =>
			{
				b.DocPack.Add(doc1);
				b.DocPack.Add(doc2);
			}));

			using (builder.DocPack)
			{
				AssertEquals(2, builder.DocPack.Count);
				AssertEquals("builder.DocPack[0].IncludedInPrint", true, builder.DocPack[0].IncludedInPrint);
				AssertEquals("builder.DocPack[1].IncludedInPrint", true, builder.DocPack[1].IncludedInPrint);

				var instructions = new DeliveryInstructions(builder.DocPack);
				AssertEquals("instructions.DeliverablesToBePrinted[0].IncludedInPrint", true, instructions.DeliverablesToBePrinted[0].IncludedInPrint);
				AssertEquals("instructions.DeliverablesToBePrinted[1].IncludedInPrint", true, instructions.DeliverablesToBePrinted[1].IncludedInPrint);

				instructions.DeliverablesToBePrinted[0].IncludedInPrint = false;
				instructions.Language = Core.SharedConstants.Languages.French;
				builder.DocPack.RebuildIfLanguageChanged(instructions);

				AssertEquals("builder.DocPack[0].IncludedInPrint", false, builder.DocPack[0].IncludedInPrint);
				AssertEquals("builder.DocPack[1].IncludedInPrint", true, builder.DocPack[1].IncludedInPrint);
				AssertEquals("instructions.DeliverablesToBePrinted[0].IncludedInPrint", false, instructions.DeliverablesToBePrinted[0].IncludedInPrint);
				AssertEquals("instructions.DeliverablesToBePrinted[1].IncludedInPrint", true, instructions.DeliverablesToBePrinted[1].IncludedInPrint);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRebuildSupportsDocPackWithOtherEDocs()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var bizo = Factory.LoadTop1<DummyDocManagerTestBizO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			bizo.SetupDocManagerObjects();

			var docType1 = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "AAA"));
			var docType2 = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "BBB"));

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = bizo;

			var template = helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "DocumentTest");
			helper.CreateMenuTemplatePivot("Document For Print", template, command);

			var eDoc1 = command.AddEDoc(docType1);
			var eDoc2 = command.AddEDoc(docType2);

			Factory.Save();

			var builder = new RatingDocPackBuilder(command, bizo, CallbackForTestingRebuild);
			var pack = builder.DocPack;
			var includeCount = pack.Cast<IDeliverable>().Count(x => x.IncludedInPrint);

			AssertEquals("Pack should have 2 eDoc", 2, pack.Count);
			AssertEquals("Pack should have 2 eDoc included in print", 2, includeCount);
			AssertEquals("Pack should have 1 other eDocs", 1, pack.OtherEDocsToAttach.Count);

			pack.OtherEDocsToAttach[0].IncludedInPrint = true;
			pack.LastTemplateGeneratorLanguage = Core.SharedConstants.Languages.EnglishAmerican;
			pack.Language = Core.SharedConstants.Languages.ChineseSimplified;

			var deliveryInstructions = new DeliveryInstructions(pack);
			pack.RebuildIfLanguageChanged(deliveryInstructions);
			includeCount = pack.Cast<IDeliverable>().Count(x => x.IncludedInPrint);

			AssertEquals("There should now have 1 other eDocs", 1, pack.OtherEDocsToAttach.Count);
			AssertEquals("Pack should now have 3 eDoc included in print", 3, includeCount);
			AssertEquals("Pack should now have 3 eDoc", 3, pack.Count);
		}

		public void TestAddPagesReportIsPasswordProtectedForOpening()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "Master Menu";
			command.SU_ContactType = ContactType.LocalClient.Code;

			var pivot1 = CreateFixedMenu(Factory, "Fixed").Documents[0];
			var pivot2 = CreateStripMenu(Factory, "Strip").Documents[0];

			var docType = Factory.New<RefDocType>();
			docType.RT_SE_NKDocumentReceivedEvent = Events.ExportReceivalAdvisePrinted.Code;
			docType.RT_DocType = "AAA";
			docType.RT_ReferenceType = "ALL";
			pivot1.SI_RT_DocType = docType.PK;
			pivot1.SI_IsPasswordProtectedForOpening = true;
			pivot2.SI_IsPasswordProtectedForOpening = true;

			Factory.Save();

			var parent = Factory.New<DummySupportable>();
			parent.Client = client;

			var wrapper1 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Wrapper1" };
			var wrapper2 = new DummyWrapper(Factory.New<DummyBusinessObject>(), Factory) { JobNumber = "Wrapper2" };

			var builder = new RatingDocPackBuilder(command, parent, new RatingDocPackBuilderCalllback((b) =>
			{
				b.AddPages(pivot1, new DocumentWrapper[] { wrapper1, wrapper2 });
				b.AddPages(pivot2, new DocumentWrapper[] { wrapper1, wrapper2 });
			}));

			AssertEquals("SI_IsPasswordProtectedForOpening should be true.", true, builder.DocPack.All(x => (x as Report).IsPasswordProtectedForOpening));
		}

		void Callback(RatingDocPackBuilder builder)
		{
		}

		void CallbackForTestingRebuild(RatingDocPackBuilder builder)
		{
			builder.AddEDocsForMainMenuItem();
		}

		#region Implementation

		static void AssertReport(string message, string expected, IDeliverable actual)
		{
			AssertType(message, typeof(Report), actual);

			Report report = (Report)actual;

			using (MemoryStream stream = new MemoryStream())
			{
				report.Save(stream);

				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(stream);
					AssertMultilineASCIIEquals(message, expected, excelInterface.WorkSheets[0].ToString());
				}
			}
		}

		static void AssertEDoc(string message, string expected, IDeliverable actual)
		{
			if (!(actual is IStorageDocs))
			{
				FailNotEquals(message, typeof(IStorageDocs), actual == null ? null : actual.GetType(), isHtmlMessage: false);
			}
			else
			{
				AssertEquals(message, expected, actual.Name);
			}
		}

		static void AssertTemplate(string message, StmTemplate template, IDeliverable deliverable)
		{
			AssertType(message, typeof(Report), deliverable);

			Report report = (Report)deliverable;

			AssertEquals(message, template, report.StTemplate);
		}

		static DocumentCommand CreateFixedMenu(BusinessObjectFactory factory, string name)
		{
			StmTemplateBase template = factory.New<StmTemplateBase>();
			template.SO_Name = name + " Template";
			template.SO_DataContext = nameof(DataContext.GenericFreightJob);
			template.SO_IsSystemDefined = true;
			template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#SectionBody]
{B}-[<ReportName>] {C}-[<JobNumber>]
{A}-[#EndOfReport]
");

			DocumentCommand command = factory.New<DocumentCommand>();
			command.SU_MenuName = name + " Menu";

			StmMenuTemplatePivot pivot = factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = command.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = name + " Pivot";

			return command;
		}

		static DocumentCommand CreateStripMenu(BusinessObjectFactory factory, string name)
		{
			var defaultTestTemplate =
			@"{A}-[#config]
{A}-[Name=TemplateWithGenericSections]
{A}-[#ConfigurableSection:BOD, JobNumber]
{A}-[#SectionBody]
{B}-[<JobNumber>]
{A}-[#ConfigurableSection:BOD, ReportName]
{A}-[#SectionBody]
{B}-[<ReportName>] {C}-[Yes]
{A}-[#ConfigurableSection:BOD, Magic1]
{A}-[#SectionBody]
{B}-[Magic]
{A}-[#ConfigurableSection:BOD, Magic2]
{A}-[#SectionBody]
{B}-[Magic] {C}-[More Magic]
{A}-[#EndOfReport]";
			return CreateStripMenu(factory, name, defaultTestTemplate);
		}

		static DocumentCommand CreateStripMenu(BusinessObjectFactory factory, string name, string templateAsString)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(StmTemplateSchema.SO_DataContext, "Blaticus");
			filter.AddToFilter(StmTemplateSchema.SO_Name, "System Document Elements");

			StmTemplateBase template = factory.LoadTop1<StmTemplateBase>(filter);
			if (template == null)
			{
				template = factory.New<StmTemplateBase>();
				template.SO_Name = "System Document Elements";
				template.SO_DataContext = "Blaticus";
				template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(templateAsString);
			}

			DocumentCommand command = factory.New<DocumentCommand>();
			command.SU_MenuName = name + " Menu";

			StmMenuTemplatePivotBase pivot = factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = command.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = name + " Pivot";

			StmMenuDocumentConfig config = pivot.DocConfigs.AddNew();
			config.S3_GC = GlbCompany.CurrentCompany.PK;
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("ReportName")).S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("JobNumber")).S4_SectionType = GenericSectionUsageList.Codes.BodySection;

			StmMenuDocumentConfigItem magic1 = config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Magic1"));
			magic1.S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			magic1.S4_FilterList = "\"<JobNumber>\" == \"Magic1\"";

			StmMenuDocumentConfigItem magic2 = config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Magic2"));
			magic2.S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			magic2.S4_FilterList = "\"<JobNumber>\" == \"Magic2\"";

			return command;
		}

		#endregion
	}
}
