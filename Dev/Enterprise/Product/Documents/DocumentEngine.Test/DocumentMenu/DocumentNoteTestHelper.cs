using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.SDF;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class DocumentNoteTestHelper
	{
		internal DocumentNoteTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		internal StmSystemDefinedField TextField1 { get; private set; }

		internal StmSystemDefinedField DateOnlyField1 { get; private set; }

		internal StmSystemDefinedField IntegerField1 { get; private set; }

		internal StmSystemDefinedField DateTimeField1 { get; private set; }

		internal StmSystemDefinedField DecimalField1 { get; private set; }

		internal DocumentNote GetNewDocumentNote() => GetNewDocumentNote(factory);

		internal DocumentNote GetNewDocumentNote(BusinessObjectFactory factory, bool useExclusiveMutex = true)
		{
			var consol = factory.New<DummyConsolBusinessObject>();
			consol.Z0_Code = "DC1";
			return GetNewDocumentNote(factory, consol, useExclusiveMutex);
		}

		internal DocumentNote GetNewDocumentNote(BusinessObjectFactory factory, DummyConsolBusinessObject consol, bool withExclusiveMutex = true)
		{
			TestCaseHelper.ClearTable(StmSystemDefinedField.Schema.TableName);

			var systemDefinedFields = new StmSystemDefinedFieldCollection(factory);

			TextField1 = systemDefinedFields.AddNew();
			TextField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			TextField1.S1_Name = "TextField1";
			TextField1.S1_Category = "CuckooSqueaker";
			TextField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Text;
			TextField1.S1_Hint = "TextField1 Hint";

			DateOnlyField1 = systemDefinedFields.AddNew();
			DateOnlyField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			DateOnlyField1.S1_Name = "DateOnlyField1";
			DateOnlyField1.S1_Category = "Internet";
			DateOnlyField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.DateOnly;
			DateOnlyField1.S1_Hint = "DateOnlyField1 Hint";

			IntegerField1 = systemDefinedFields.AddNew();
			IntegerField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			IntegerField1.S1_Name = "IntegerField1";
			IntegerField1.S1_Category = "COMMON";
			IntegerField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Integer;
			IntegerField1.S1_Hint = "IntegerField1 Hint";

			DateTimeField1 = systemDefinedFields.AddNew();
			DateTimeField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			DateTimeField1.S1_Name = "DateTimeField1";
			DateTimeField1.S1_Category = "aids";
			DateTimeField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.DateTime;
			DateTimeField1.S1_Hint = "DateTimeField1 Hint";

			DecimalField1 = systemDefinedFields.AddNew();
			DecimalField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			DecimalField1.S1_Name = "DecimalField1";
			DecimalField1.S1_Category = "HOW2BINDFORMLOL";
			DecimalField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Decimal;
			DecimalField1.S1_Hint = "DecimalField1 Hint";

			factory.Save();

			return withExclusiveMutex ? DocumentNote.LoadNoteWithExclusiveMutex(consol) : DocumentNote.LoadNote(consol);
		}
	}
}
