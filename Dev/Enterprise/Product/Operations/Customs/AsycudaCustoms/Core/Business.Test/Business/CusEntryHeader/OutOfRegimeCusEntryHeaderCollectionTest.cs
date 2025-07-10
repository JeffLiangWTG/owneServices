using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(OutOfRegimeCusEntryHeaderCollection))]
	class OutOfRegimeCusEntryHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<OutOfRegimeCusEntryHeaderCollection>
	{
		public void TestModuleIDAttribute()
		{
			AssertEquals(ModuleId.EntryHeader, typeof(OutOfRegimeCusEntryHeaderCollection).GetCustomAttribute<ModuleIDAttribute>().ModuleId);
		}

		public void TestDelete_NotSupportedException()
		{
			var outOfRegimeProcedures = CreateOutOfRegimeProcedures(Factory);
			var nonOutOfRegimeProcedure = CreateNonOutOfRegimeProcedures(Factory);
			Factory.Save();
			CreateJobDeclarationWithEntryHeader(
				Factory,
				JobMessageTypeList.Codes.Export,
				"Entry",
				new[] { outOfRegimeProcedures[0], nonOutOfRegimeProcedure });
			Factory.Save();

			var entryHeaders = new OutOfRegimeCusEntryHeaderCollection(Factory, JobMessageTypeList.Codes.Export);
			AssertExceptionThrown<NotSupportedException>(() =>
			{
				entryHeaders.Delete(entryHeaders[0]);
			});
		}

		public void TestDeleteAll_NotSupportedException()
		{
			var outOfRegimeProcedures = CreateOutOfRegimeProcedures(Factory);
			var nonOutOfRegimeProcedure = CreateNonOutOfRegimeProcedures(Factory);
			Factory.Save();
			CreateJobDeclarationWithEntryHeader(
				Factory,
				JobMessageTypeList.Codes.Export,
				"Entry",
				new[] { outOfRegimeProcedures[0], nonOutOfRegimeProcedure });
			Factory.Save();

			var entryHeaders = new OutOfRegimeCusEntryHeaderCollection(Factory, JobMessageTypeList.Codes.Export);
			AssertExceptionThrown<NotSupportedException>(() =>
			{
				entryHeaders.DeleteAll();
			});
		}

		public void TestFindItemsWithOutOfRegimeProcedureByMessageType_Correct()
		{
			var outOfRegimeProcedures = CreateOutOfRegimeProcedures(Factory);
			var nonOutOfRegimeProcedure = CreateNonOutOfRegimeProcedures(Factory);
			Factory.Save();
			for (var index = 0; index < outOfRegimeProcedures.Length; ++index)
			{
				CreateJobDeclarationWithEntryHeader(
					Factory,
					JobMessageTypeList.Codes.Export,
					$"Entry{index}",
					new[] { outOfRegimeProcedures[index], nonOutOfRegimeProcedure });
			}
			Factory.Save();

			var entryHeaders = new OutOfRegimeCusEntryHeaderCollection(Factory, JobMessageTypeList.Codes.Export);

			var expectedNumbers = new HashSet<string>();
			for (var index = 0; index < outOfRegimeProcedures.Length; ++index)
			{
				expectedNumbers.Add($"Entry{index}");
			}

			var actualNumbers = entryHeaders.Select(x => x.EntryNumber.ToString()).ToHashSet();
			CombineAssertions(() =>
			{
				AssertEquals("Count", outOfRegimeProcedures.Length, actualNumbers.Count);
				AssertEquals("Numbers", true, actualNumbers.SetEquals(expectedNumbers));
			});
		}

		public void TestFindItemsWithOutOfRegimeProcedureByMessageType_NoEntryHeader()
		{
			AssertNoExceptionThrown(() =>
			{
				var entryHeaders = new OutOfRegimeCusEntryHeaderCollection(Factory, JobMessageTypeList.Codes.Import);
				AssertEquals(0, entryHeaders.Count);
			});
		}

		public void TestFindItemsWithOutOfRegimeProcedureByMessageType_EmptyEntryNumber()
		{
			var outOfRegimeProcedures = CreateOutOfRegimeProcedures(Factory);
			Factory.Save();
			CreateJobDeclarationWithEntryHeader(
				Factory,
				JobMessageTypeList.Codes.Export,
				null,
				new[] { outOfRegimeProcedures[0], outOfRegimeProcedures[1] });
			Factory.Save();

			var entryHeaders = new OutOfRegimeCusEntryHeaderCollection(Factory, JobMessageTypeList.Codes.Export);
			AssertEquals(0, entryHeaders.Count);
		}

		public void TestFindItemsWithOutOfRegimeProcedureByMessageType_NoMatchedCompany()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Namibia))
			{
				var outOfRegimeProcedures = CreateOutOfRegimeProcedures(Factory);
				Factory.Save();
				CreateJobDeclarationWithEntryHeader(
					Factory,
					JobMessageTypeList.Codes.Import,
					"OtherCountry",
					new[] { outOfRegimeProcedures[0], outOfRegimeProcedures[1] });
				Factory.Save();
			}

			var entryHeaders = new OutOfRegimeCusEntryHeaderCollection(Factory, JobMessageTypeList.Codes.Import);
			AssertEquals(0, entryHeaders.Count);
		}

		public void TestFindItemsWithOutOfRegimeProcedureByMessageType_NoInvoiceLine()
		{
			CreateJobDeclarationWithEntryHeader(
				Factory,
				JobMessageTypeList.Codes.Import,
				"NoInvLine",
				Array.Empty<RefCusProcedure>());
			Factory.Save();

			var entryHeaders = new OutOfRegimeCusEntryHeaderCollection(Factory, JobMessageTypeList.Codes.Import);
			AssertEquals(0, entryHeaders.Count);
		}

		public void TestFindItemsWithOutOfRegimeProcedureByMessageType_NoMatchedMessageType()
		{
			var outOfRegimeProcedures = CreateOutOfRegimeProcedures(Factory);
			Factory.Save();
			CreateJobDeclarationWithEntryHeader(
				Factory,
				JobMessageTypeList.Codes.Export,
				"ExportEntry",
				new[] { outOfRegimeProcedures[0], outOfRegimeProcedures[1] });
			Factory.Save();

			var entryHeaders = new OutOfRegimeCusEntryHeaderCollection(Factory, JobMessageTypeList.Codes.Import);
			AssertEquals(0, entryHeaders.Count);
		}

		public void TestFindItemsWithOutOfRegimeProcedureByMessageType_NoOutOfRegimeProcedure()
		{
			var nonOutOfRegimeProcedure = CreateNonOutOfRegimeProcedures(Factory);
			Factory.Save();
			CreateJobDeclarationWithEntryHeader(
				Factory,
				JobMessageTypeList.Codes.Import,
				"NoOutOfRegime",
				new[] { nonOutOfRegimeProcedure });
			Factory.Save();

			var entryHeaders = new OutOfRegimeCusEntryHeaderCollection(Factory, JobMessageTypeList.Codes.Import);
			AssertEquals(0, entryHeaders.Count);
		}

		public void TestFindItemsWithOutOfRegimeProcedureByMessageType_EmptyMessageType()
		{
			var outOfRegimeProcedures = CreateOutOfRegimeProcedures(Factory);
			Factory.Save();
			CreateJobDeclarationWithEntryHeader(
				Factory,
				JobMessageTypeList.Codes.Export,
				"ExportEntry1",
				new[] { outOfRegimeProcedures[0], outOfRegimeProcedures[1] });
			CreateJobDeclarationWithEntryHeader(
				Factory,
				JobMessageTypeList.Codes.Import,
				"ExportEntry2",
				new[] { outOfRegimeProcedures[0], outOfRegimeProcedures[1] });
			Factory.Save();

			var entryHeaders = new OutOfRegimeCusEntryHeaderCollection(Factory, ZString.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { "ExportEntry1", "ExportEntry2" }, entryHeaders.Select(x => x.EntryNumber.ToString()).ToArray());
		}

		protected override OutOfRegimeCusEntryHeaderCollection GetCollectionToTest()
		{
			return new OutOfRegimeCusEntryHeaderCollection(Factory, JobMessageTypeList.Codes.Export);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			// relationship uses a dbonlyquery so we'll need to save it to the DB.

			var factory = new BusinessObjectFactory();
			var outOfRegimeProcedures = CreateOutOfRegimeProcedures(factory);
			var declaration = CreateJobDeclarationWithEntryHeader(
				factory,
				JobMessageTypeList.Codes.Export,
				"EntryNum",
				new[] { outOfRegimeProcedures[0] });
			factory.Save();
			return declaration.ActiveEntryHeaders[0];
		}

		static JobDeclaration CreateJobDeclarationWithEntryHeader(
			BusinessObjectFactory factory,
			string messageType,
			string entryNumber,
			RefCusProcedure[] procedures)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = messageType;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.EntryNumber = entryNumber;

			procedures.ForEach(procedure =>
			{
				var entryLine = entryHeader.AllEntryLines.AddNew();

				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				SetInvoiceLineProcedure(invoiceLine, procedure);
			});

			return declaration;
		}

		static RefCusProcedure[] CreateOutOfRegimeProcedures(BusinessObjectFactory factory)
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var helper = new UniversalReferenceTestDataHelper(factory);

			var procedure1 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "90", "71", "F61", "9071F61", "IMP", "10P");
			procedure1.ZZ6_OutOfWarehouse = YesNoList.Codes.Yes;

			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "91", "71", "F61", "9171F61", "IMP", "10P");
			procedure2.ZZ6_OutOfInwardProcessing = YesNoList.Codes.Yes;

			var procedure3 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "92", "71", "F61", "9271F61", "IMP", "10P");
			procedure3.ZZ6_OutofOutwardProcessing = YesNoList.Codes.Yes;

			var procedure4 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "93", "71", "F61", "9371F61", "IMP", "10P");
			procedure4.ZZ6_OutOfTemporaryImport = YesNoList.Codes.Yes;

			var procedure5 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "94", "71", "F61", "9471F61", "IMP", "10P");
			procedure5.ZZ6_OutOfTemporaryExport = YesNoList.Codes.Yes;

			return new[] { procedure1, procedure2, procedure3, procedure4, procedure5 };
		}

		static RefCusProcedure CreateNonOutOfRegimeProcedures(BusinessObjectFactory factory)
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "80", "71", "F61", "8071F61", "IMP", "10P");
			procedure.ZZ6_OutOfWarehouse = YesNoList.Codes.No;
			procedure.ZZ6_OutOfInwardProcessing = YesNoList.Codes.No;
			procedure.ZZ6_OutofOutwardProcessing = YesNoList.Codes.No;
			procedure.ZZ6_OutOfTemporaryImport = YesNoList.Codes.No;
			procedure.ZZ6_OutOfTemporaryExport = YesNoList.Codes.No;
			return procedure;
		}

		static void SetInvoiceLineProcedure(BaseJobComInvoiceLine invoiceLine, RefCusProcedure procedure)
		{
			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
		}
	}
}
