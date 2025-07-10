using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	sealed class JobDeclarationDocumentSupporterTest : EU.Business.Declaration.Testing.JobDeclarationDocumentSupporterTest
	{
		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var dictionary = base.MaxDBHitCounts;
				dictionary["CusEntryInstruction"] = 2;
				dictionary["GenPivot"] = 2;
				return dictionary;
			}
		}

		public void TestGetDocumentWrappersInternal_SADHDataContext()
		{
			var menuItemForTesting = Factory.New<IStmMenuItem>();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader1 = CreateEntryHeader(declaration, entryInstruction.PK, DeclarationApplicationCodeList.Codes.Builtin);
			var entryHeader2 = CreateEntryHeader(declaration, entryInstruction.PK, DeclarationApplicationCodeList.Codes.Builtin);
			var entryHeader3 = CreateEntryHeader(declaration, entryInstruction.PK, DeclarationApplicationCodeList.Codes.Interfaced);
			var entryHeader4 = CreateEntryHeader(declaration, ZGuid.Empty, DeclarationApplicationCodeList.Codes.Builtin);
			var documentSupporter = new JobDeclarationDocumentSupporterForTesting(declaration);
			var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.SADH, menuItemForTesting);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Wrapped Objects", wrappers.Select(x => x.WrappedObject), new[] { entryHeader1, entryHeader2 });
				AssertType<DEDocSADH>("Wrapper Type", wrappers[0]);
			});
		}

		public void TestGetDocumentWrappersInternal_CusEntryHeaderDataContext()
		{
			var menuItemForTesting = Factory.New<IStmMenuItem>();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader1 = CreateEntryHeader(declaration, entryInstruction.PK, DeclarationApplicationCodeList.Codes.Builtin);
			var entryHeader2 = CreateEntryHeader(declaration, entryInstruction.PK, DeclarationApplicationCodeList.Codes.Builtin);
			var entryHeader3 = CreateEntryHeader(declaration, entryInstruction.PK, DeclarationApplicationCodeList.Codes.Interfaced);
			var entryHeader4 = CreateEntryHeader(declaration, ZGuid.Empty, DeclarationApplicationCodeList.Codes.Builtin);
			var documentSupporter = new JobDeclarationDocumentSupporterForTesting(declaration);
			var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.CusEntryHeader, menuItemForTesting);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Wrapped Objects", wrappers.Select(x => x.WrappedObject), new[] { entryHeader1, entryHeader2 });
				AssertType<DocCusEntryHeader>("Wrapper Type", wrappers[0]);
			});
		}

		public void TestDocumentSupporter()
		{
			var declarationDocumentSupporter = (JobDeclarationDocumentSupporter)Factory.New<JobDeclaration>().DocumentSupporter;
			AssertType<JobDeclarationDocumentSupporter>("JobDeclarationDocumentSupporter for DE expected", declarationDocumentSupporter);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.CustomsEntryInstructions.AddNew();
			var inv = declaration.Invoices.AddNew();
			inv.FillWithValidTestData();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.FillWithValidTestData();

			var landedCostHeader = (BusinessObject)Factory.New<LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = declaration.TablePrefix;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = declaration.PK;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			return declaration;
		}

		CusEntryHeader CreateEntryHeader(JobDeclaration decl, ZGuid entryInstructionPK, string messageType)
		{
			var eh = decl.CustomsEntryHeaders.AddNew();
			eh.CH_CEI_Instruction = entryInstructionPK;
			eh.CH_MessageType = messageType;
			return eh;
		}

		class JobDeclarationDocumentSupporterForTesting : JobDeclarationDocumentSupporter
		{
			public JobDeclarationDocumentSupporterForTesting(JobDeclaration declaration) : base(declaration)
			{
			}

			public DocumentWrapper[] GetDocumentWrappersInternalExposed(DataContext dataContext, IStmMenuItem commandBeingRun) => GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}
	}
}
