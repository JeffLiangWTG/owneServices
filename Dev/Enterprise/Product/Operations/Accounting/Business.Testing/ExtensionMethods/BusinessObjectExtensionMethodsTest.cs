using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	using System.Data;
	using CargoWise.EntityFramework.Testing;

	public class BusinessObjectExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestCopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder()
		{
			var source = Factory.New<DummyBizoWithRecalculationsBetweenFields>();
			source.Z0_Bool = ZBool.True;
			source.Z0_Date = ZDateTime.BrettsBirthday;
			source.Z0_Decimal = 33m;
			source.Z0_Guid = ZGuid.NewZGuid();
			var destination = Factory.New<DummyBizoWithRecalculationsBetweenFields>();
			var expectedZGuid = ZGuid.NewZGuid();
			destination.Z0_Guid = expectedZGuid;
			destination.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(source, DummyBizoSchema.Z0_Decimal.Name, DummyBizoSchema.Z0_Bool.Name, DummyBizoSchema.Z0_Date.Name);
			var messagePrefix = "Incorrect filed order";
			AssertEquals(messagePrefix + "Z0_Bool", ZBool.True, destination.Z0_Bool);
			AssertEquals(messagePrefix + "Z0_Date", ZDateTime.BrettsBirthday, destination.Z0_Date);
			AssertEquals(messagePrefix + "Z0_Decimal should be recalculated as Z0_Date was set later", -187m, destination.Z0_Decimal);
			AssertEquals(messagePrefix + "Z0_Guid should ramain unchanged as it's not in a copied filed list", expectedZGuid, destination.Z0_Guid);
			destination = Factory.New<DummyBizoWithRecalculationsBetweenFields>();
			expectedZGuid = ZGuid.NewZGuid();
			destination.Z0_Guid = expectedZGuid;
			destination.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(source, DummyBizoSchema.Z0_Date.Name, DummyBizoSchema.Z0_Bool.Name, DummyBizoSchema.Z0_Decimal.Name);
			messagePrefix = "Correct filed order";
			AssertEquals(messagePrefix + "Z0_Bool", ZBool.True, destination.Z0_Bool);
			AssertEquals(messagePrefix + "Z0_Date", ZDateTime.BrettsBirthday, destination.Z0_Date);
			AssertEquals(messagePrefix + "Z0_Decimal should be as in source", 33m, destination.Z0_Decimal);
			AssertEquals(messagePrefix + "Z0_Guid should ramains unchanged as it's not in a copied filed list", expectedZGuid, destination.Z0_Guid);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		public void TestCopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder_Errors()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => BusinessObjectExtensions.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(null, null, null));
			var destinationObject = Factory.New<DummyBusinessObject>();
			AssertExceptionThrown(typeof(ArgumentNullException), () => BusinessObjectExtensions.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(destinationObject, null, null));
			var sourceObject = Factory.New<DummyBaseBusinessObject>();
			AssertExceptionThrown(typeof(ArgumentNullException), () => BusinessObjectExtensions.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(destinationObject, sourceObject, null));
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), "Value '0' cannot be less than or equal to 0.\r\nParameter name: fieldsToCopyAndInValidOrder.Length", () => BusinessObjectExtensions.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(destinationObject, sourceObject));
			BusinessObjectExtensions.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(destinationObject, sourceObject, "");
			AssertEquals("destinationObject 'CargoWise.EntityFramework.Testing.DummyBusinessObject' and sourceObject 'CargoWise.EntityFramework.Testing.DummyBaseBusinessObject' must have the same type.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			sourceObject = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject), new Guid("1cd35d57-9468-4fef-9a26-eef57a9b4c2d"));
			BusinessObjectExtensions.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(sourceObject, sourceObject, "");
			AssertEquals("Object type 'CargoWise.EntityFramework.Testing.DummyBusinessObject': destinationObject and sourceObject must have different PKs. PK is '1cd35d57-9468-4fef-9a26-eef57a9b4c2d'.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), "Value '0' cannot be less than or equal to 0.\r\nParameter name: fieldNamesAndValuesToSet.Count", () => BusinessObjectExtensions.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(destinationObject, sourceObject, ""));
			AssertEquals("Object type 'CargoWise.EntityFramework.Testing.DummyBusinessObject': field name '' is not found.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			BusinessObjectExtensions.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(destinationObject, sourceObject, DummyBizoSchema.Z0_Bool.Name, "Some invalid field name", DummyBizoSchema.Z0_Code.Name);
			AssertEquals("Object type 'CargoWise.EntityFramework.Testing.DummyBusinessObject': field name 'Some invalid field name' is not found.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertNoExceptionThrown(() => BusinessObjectExtensions.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(destinationObject, sourceObject, DummyBizoSchema.Z0_Bool.Name, DummyBizoSchema.Z0_Code.Name));
		}

		public void TestSetFieldsInParticularOrder()
		{
			var source = Factory.New<DummyBizoWithRecalculationsBetweenFields>();
			var filedValues = new Dictionary<string, IZType>();
			filedValues.Add(DummyBizoSchema.Z0_Bool.Name, ZBool.True);
			filedValues.Add(DummyBizoSchema.Z0_Date.Name, ZDateTime.BrettsBirthday);
			filedValues.Add(DummyBizoSchema.Z0_Decimal.Name, (ZDecimal)33);
			filedValues.Add(DummyBizoSchema.Z0_Guid.Name, ZGuid.NewZGuid());
			var destination = Factory.New<DummyBizoWithRecalculationsBetweenFields>();
			var expectedZGuid = ZGuid.NewZGuid();
			destination.Z0_Guid = expectedZGuid;
			destination.SetFieldsInParticularOrder(filedValues, DummyBizoSchema.Z0_Decimal.Name, DummyBizoSchema.Z0_Bool.Name, DummyBizoSchema.Z0_Date.Name);
			var messagePrefix = "Incorrect filed order";
			AssertEquals(messagePrefix + "Z0_Bool", ZBool.True, destination.Z0_Bool);
			AssertEquals(messagePrefix + "Z0_Date", ZDateTime.BrettsBirthday, destination.Z0_Date);
			AssertEquals(messagePrefix + "Z0_Decimal should be recalculated as Z0_Date was set later", -187m, destination.Z0_Decimal);
			AssertEquals(messagePrefix + "Z0_Guid should ramain unchanged as it's not in a copied filed list", expectedZGuid, destination.Z0_Guid);
			destination = Factory.New<DummyBizoWithRecalculationsBetweenFields>();
			expectedZGuid = ZGuid.NewZGuid();
			destination.Z0_Guid = expectedZGuid;
			destination.SetFieldsInParticularOrder(filedValues, DummyBizoSchema.Z0_Date.Name, DummyBizoSchema.Z0_Bool.Name, DummyBizoSchema.Z0_Decimal.Name);
			messagePrefix = "Correct filed order";
			AssertEquals(messagePrefix + "Z0_Bool", ZBool.True, destination.Z0_Bool);
			AssertEquals(messagePrefix + "Z0_Date", ZDateTime.BrettsBirthday, destination.Z0_Date);
			AssertEquals(messagePrefix + "Z0_Decimal should be as in source", 33m, destination.Z0_Decimal);
			AssertEquals(messagePrefix + "Z0_Guid should ramains unchanged as it's not in a copied filed list", expectedZGuid, destination.Z0_Guid);
		}

		public void TestSetFieldsInParticularOrder_Errors()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => BusinessObjectExtensions.SetFieldsInParticularOrder(null, null, null));
			var destinationObject = Factory.New<DummyBusinessObject>();
			AssertExceptionThrown(typeof(ArgumentNullException), () => BusinessObjectExtensions.SetFieldsInParticularOrder(destinationObject, null, null));
			var filedValues = new Dictionary<string, IZType>();
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), "Value '0' cannot be less than or equal to 0.\r\nParameter name: fieldNamesAndValuesToSet.Count", () => BusinessObjectExtensions.SetFieldsInParticularOrder(destinationObject, filedValues));
			filedValues.Add("", ZString.Empty);
			AssertExceptionThrown(typeof(ArgumentNullException), () => BusinessObjectExtensions.SetFieldsInParticularOrder(destinationObject, filedValues, null));
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), "Value '0' cannot be less than or equal to 0.\r\nParameter name: fieldsToSetAndInValidOrder.Length", () => BusinessObjectExtensions.SetFieldsInParticularOrder(destinationObject, filedValues));
			BusinessObjectExtensions.SetFieldsInParticularOrder(destinationObject, filedValues, "");
			AssertEquals("Object type 'CargoWise.EntityFramework.Testing.DummyBusinessObject': field name '' is not found.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			filedValues.Add(DummyBizoSchema.Z0_Bool.Name, ZBool.True);
			filedValues.Add(DummyBizoSchema.Z0_Guid.Name, ZBool.True);
			BusinessObjectExtensions.SetFieldsInParticularOrder(destinationObject, filedValues, DummyBizoSchema.Z0_Bool.Name, "Some invalid field name", DummyBizoSchema.Z0_Code.Name);
			AssertEquals("Object type 'CargoWise.EntityFramework.Testing.DummyBusinessObject': field name 'Some invalid field name' is not found.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			BusinessObjectExtensions.SetFieldsInParticularOrder(destinationObject, filedValues, DummyBizoSchema.Z0_Bool.Name, DummyBizoSchema.Z0_Code.Name);
			AssertEquals("Object type 'CargoWise.EntityFramework.Testing.DummyBusinessObject': field name 'Z0_Code' does not have a value to set.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			filedValues.Add(DummyBizoSchema.Z0_Code.Name, ZBool.True);
			AssertExceptionThrown(typeof(InvalidOperationException), "Object type 'CargoWise.EntityFramework.Testing.DummyBusinessObject': field name 'Z0_Code' ", () => BusinessObjectExtensions.SetFieldsInParticularOrder(destinationObject, filedValues, DummyBizoSchema.Z0_Bool.Name, DummyBizoSchema.Z0_Code.Name));
			filedValues[DummyBizoSchema.Z0_Code.Name] = ZString.Empty;
			BusinessObjectExtensions.SetFieldsInParticularOrder(destinationObject, filedValues, DummyBizoSchema.Z0_Bool.Name, DummyBizoSchema.Z0_Code.Name, "Z0_Calculated");
			AssertEquals("Object type 'CargoWise.EntityFramework.Testing.DummyBusinessObject': field name 'Z0_Calculated' does not have a setter.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			BusinessObjectExtensions.SetFieldsInParticularOrder(destinationObject, filedValues, DummyBizoSchema.Z0_Bool.Name, DummyBizoSchema.Z0_Code.Name);
		}

		public void TestCreateSystemNote()
		{
			var dummyEnterpriseBusinessObject = Factory.New<ZArchitecture.Testing.DummyEnterpriseBizo>();
			AssertEquals("PreCondition", 0, dummyEnterpriseBusinessObject.Notes.GetAllNotes().Count);

			var stmNote = BusinessObjectExtensions.CreateSystemNote(dummyEnterpriseBusinessObject, ZArchitecture.Business.PredefinedNoteTypes.Instance.DataImportLogNote);
			CombineAssertions("One stmNote is added to Bizo", () => {
				AssertEquals(1, dummyEnterpriseBusinessObject.Notes.GetAllNotes().Count);
				AssertEquals(stmNote, dummyEnterpriseBusinessObject.Notes.FindByPK(stmNote.PK));
			});

			var stmNote2 = BusinessObjectExtensions.CreateSystemNote(dummyEnterpriseBusinessObject, ZArchitecture.Business.PredefinedNoteTypes.Instance.DataImportLogNote);
			CombineAssertions("Should create new StmNote.", () => {
				AssertEquals(2, dummyEnterpriseBusinessObject.Notes.GetAllNotes().Count);
				AssertNotEquals(stmNote, stmNote2);
			});
		}

		class DummyBizoWithRecalculationsBetweenFields : DummyBusinessObject
		{
			public DummyBizoWithRecalculationsBetweenFields(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZDateTime Z0_Date
			{
				get
				{
					return base.Z0_Date;
				}

				set
				{
					base.Z0_Date = value;
					Z0_Decimal = -187;
				}
			}
		}
	}
}
