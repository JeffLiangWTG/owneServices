using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class PredefinedNoteTypeTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			PredefinedNoteType noteType = new PredefinedNoteType((NoResString)"Desc", StmNoteVisibility.INT, true, false, true, false);
			AssertEquals("Description", "Desc", noteType.Description);
			AssertEquals("DefaultVisibility", StmNoteVisibility.INT, noteType.DefaultVisibility);
			AssertEquals("IsOnlyOneAllowed", true, noteType.IsOnlyOneAllowed);
			AssertEquals("IsReadOnlyAfterAdd", false, noteType.IsReadOnlyAfterAdd);
			AssertEquals("IsTextOnly", true, noteType.IsTextOnly);
			AssertEquals("IsPopupLog", false, noteType.IsPopupLog);
			AssertEquals("TextOnlyMaxLength", 50000, noteType.TextOnlyMaxLength);
		}

		public void TestConstructorWithSerializableNoteType()
		{
			Type serializablenoteType = ObjectFactory.GetType<MasterFiles.Integration.IUnmatchOrgDetailRecords>();
			PredefinedNoteType noteType = new PredefinedNoteType((NoResString)"Desc", StmNoteVisibility.INT, true, serializablenoteType, false);
			AssertEquals("Description", "Desc", noteType.Description);
			AssertEquals("DefaultVisibility", StmNoteVisibility.INT, noteType.DefaultVisibility);
			AssertEquals("IsOnlyOneAllowed", true, noteType.IsOnlyOneAllowed);
			AssertEquals("IsReadOnlyAfterAdd", true, noteType.IsReadOnlyAfterAdd);
			AssertEquals("IsTextOnly", true, noteType.IsTextOnly);
			AssertEquals("IsPopupLog", true, noteType.IsPopupLog);
			AssertEquals("SerializableNoteType", serializablenoteType, noteType.SerializableNoteType);
			AssertEquals("TextOnlyMaxLength", 50000, noteType.TextOnlyMaxLength);
		}

		public void TestConstructorWithTextOnlyMaxLength()
		{
			PredefinedNoteType noteType = new PredefinedNoteType((NoResString)"Desc", StmNoteVisibility.INT, true, false, true, 100, false);
			AssertEquals("Description", "Desc", noteType.Description);
			AssertEquals("DefaultVisibility", StmNoteVisibility.INT, noteType.DefaultVisibility);
			AssertEquals("IsOnlyOneAllowed", true, noteType.IsOnlyOneAllowed);
			AssertEquals("IsReadOnlyAfterAdd", false, noteType.IsReadOnlyAfterAdd);
			AssertEquals("IsTextOnly", true, noteType.IsTextOnly);
			AssertEquals("IsPopupLog", false, noteType.IsPopupLog);
			AssertEquals("TextOnlyMaxLength", 100, noteType.TextOnlyMaxLength);
		}

		public void TestConstructorWithIsPopupLog()
		{
			PredefinedNoteType noteType = new PredefinedNoteType((NoResString)"Desc", StmNoteVisibility.INT, true, false, true, true, 100, false);
			AssertEquals("Description", "Desc", noteType.Description);
			AssertEquals("DefaultVisibility", StmNoteVisibility.INT, noteType.DefaultVisibility);
			AssertEquals("IsOnlyOneAllowed", true, noteType.IsOnlyOneAllowed);
			AssertEquals("IsReadOnlyAfterAdd", false, noteType.IsReadOnlyAfterAdd);
			AssertEquals("IsTextOnly", true, noteType.IsTextOnly);
			AssertEquals("IsPopupLog", true, noteType.IsPopupLog);
			AssertEquals("TextOnlyMaxLength", 100, noteType.TextOnlyMaxLength);
		}

		public void TestIsCustomNoteType()
		{
			PredefinedNoteType noteType = new PredefinedNoteType((NoResString)"Desc", StmNoteVisibility.INT, true, false, true, true, 100, false);
			Assert(!noteType.IsCustomNoteType);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "IsTextOnly must be true if IsPopupLog.\r\nParameter name: isTextOnly")]
		public void TestIsTextOnlyMustBeTrueIfIsPopupLog()
		{
			PredefinedNoteType noteType = new PredefinedNoteType((NoResString)"Desc", StmNoteVisibility.INT, true, false, false, true, false);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "IsOnlyOneAllowed must be true if IsPopupLog.\r\nParameter name: isOnlyOneAllowed")]
		public void TestIsOnlyOneAllowedMustBeTrueIfIsPopupLog()
		{
			PredefinedNoteType noteType = new PredefinedNoteType((NoResString)"Desc", StmNoteVisibility.INT, false, false, true, true, false);
		}
	}
}
