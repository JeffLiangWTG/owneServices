using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ListValidationTest : TestCaseWithDummy
	{
		#region ErrorIfInvalidPK

		[ExpectException(typeof(ArgumentNullException))]
		public void TestErrorIfInvalidPK_RequiresList()
		{
			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, (IBusinessObjectCollection)null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestErrorIfInvalidPK_RequiresMessage()
		{
			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, DummyList, null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestErrorIfInvalidPK_RequiresMessageIsNonEmpty()
		{
			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, DummyList, null);
		}

		public void TestErrorIfInvalidPK_UsesMetadataToFindDummyList()
		{
			DummyBusinessObject dummy1 = NewDummyToList(DummyList);
			NewDummyToList(DummyList);
			NewDummyToList(DummyList);

			SuperDummy.SS_Guid = dummy1.PK;
			SuperDummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo);
			AssertEquals("Dummy should have no errors", false, SuperDummy.HasErrors);

			SuperDummy.SS_Guid = ZGuid.NewZGuid();
			SuperDummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo);
			AssertEquals("Dummy should have errors", true, SuperDummy.HasErrors);
		}

		public void TestErrorIfInvalidPK_NotInList_WithoutListFilter()
		{
			DummyBusinessObject dummy1 = NewDummyToList(DummyList);
			NewDummyToList(DummyList);
			NewDummyToList(DummyList);

			SuperDummy.SS_Guid = dummy1.PK;
			SuperDummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, DummyList);
			AssertEquals("Dummy should have no errors", false, SuperDummy.HasErrors);

			SuperDummy.SS_Guid = ZGuid.NewZGuid();
			SuperDummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, DummyList);
			AssertEquals("Dummy should have errors", true, SuperDummy.HasErrors);
		}

		public void TestErrorIfInvalidPK_NotInList_WithListFilter()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "Code2");
			DummyList = new DummyBusinessObjectCollection(Factory, filter);

			DummyBusinessObject dummy1 = NewDummyToList(DummyList);
			DummyBusinessObject dummy2 = NewDummyToList(DummyList);
			NewDummyToList(DummyList);

			SuperDummy.SS_Guid = dummy1.PK;
			SuperDummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, DummyList);
			AssertEquals("Dummy should have errors - doesn't match List filter", true, SuperDummy.HasErrors);

			SuperDummy.SS_Guid = ZGuid.NewZGuid();
			SuperDummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, DummyList);
			AssertEquals("Dummy should have errors - doesn't exist", true, SuperDummy.HasErrors);

			SuperDummy.SS_Guid = dummy2.PK;
			SuperDummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, DummyList);
			AssertEquals("Dummy should have no errors - matches List filter and exist", false, SuperDummy.HasErrors);
		}

		public void TestErrorIfInvalidPK_NotInList_WithoutListFilter_WithErrorMessage()
		{
			DummyBusinessObject dummy1 = NewDummyToList(DummyList);
			NewDummyToList(DummyList);
			NewDummyToList(DummyList);

			SuperDummy.SS_Guid = dummy1.PK;
			SuperDummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, DummyList, (NoResString)"Message");
			AssertNoErrors("Dummy should have no errors", SuperDummy);

			SuperDummy.SS_Guid = ZGuid.NewZGuid();
			SuperDummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, DummyList, (NoResString)"Message");
			AssertHasError("Dummy should have errors", SuperDummy.SS_GuidInfo, "Message");
		}

		public void TestErrorIfInvalidPK_NotInList_WithListFilter_WithErrorMessage()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "Code2");
			DummyList = new DummyBusinessObjectCollection(Factory, filter);

			DummyBusinessObject dummy1 = NewDummyToList(DummyList);
			DummyBusinessObject dummy2 = NewDummyToList(DummyList);
			NewDummyToList(DummyList);

			SuperDummy.SS_Guid = dummy1.PK;
			SuperDummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, DummyList, (NoResString)"Message");
			AssertHasError("Dummy should have errors - doesn't match List filter", SuperDummy.SS_GuidInfo, "Message");

			SuperDummy.SS_Guid = ZGuid.NewZGuid();
			SuperDummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, DummyList, (NoResString)"Message");
			AssertHasError("Dummy should have errors - doesn't exist", SuperDummy.SS_GuidInfo, "Message");

			SuperDummy.SS_Guid = dummy2.PK;
			SuperDummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, DummyList, (NoResString)"Message");
			AssertNoErrors("Dummy should have no errors - matches List filter and exist", SuperDummy.SS_GuidInfo);
		}

		public void TestErrorIfInvalidPK_NotInList_CodeDescriptionPairList()
		{
			ZGuid pk1 = ZGuid.NewZGuid();
			ZGuid pk2 = ZGuid.NewZGuid();
			ZGuid pk3 = ZGuid.NewZGuid();

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair(pk1, "Code1", "Description 1");
			list.AddPair(pk2, "Code2", "Description 2");
			list.AddPair(pk3, "Code3", "Description 3");

			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Guid = ZGuid.NewZGuid();
				Dummy.Z0_GuidInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidPK(Dummy.Z0_GuidInfo, list);
				AssertHasError(Dummy.Z0_GuidInfo, "Enter a valid Id.");

				ZGuid[] validPKs = { ZGuid.Empty, pk1, pk2, pk3 };

				foreach (ZGuid validPK in validPKs)
				{
					Dummy.Z0_Guid = validPK;
					Dummy.Z0_GuidInfo.ClearAllNotifications();
					ListValidation.ErrorIfInvalidPK(Dummy.Z0_GuidInfo, list);
					AssertNoErrors(Dummy.Z0_GuidInfo);
				}
			}
		}

		public void TestErrorIfInvalidPK_IExternalListValidation()
		{
			var collection = new DummyBizoCollectionWithExternalListValidation(Factory);
			var dummy1 = collection.AddNew();
			var dummy2 = collection.AddNew();

			SuperDummy.SS_Guid = dummy1.PK;
			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, collection, (NoResString)"Message");
			AssertNoErrors(SuperDummy.SS_GuidInfo);

			SuperDummy.SS_Guid = ZGuid.NewZGuid();
			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, collection, (NoResString)"Message");
			AssertHasError("Base validation returns error if not overridden by IExternalListValidation", SuperDummy.SS_GuidInfo, "Message");

			SuperDummy.SS_GuidInfo.ClearAllNotifications();
			collection.ExternalIsValidPKForTets = true;
			ListValidation.ErrorIfInvalidPK(SuperDummy.SS_GuidInfo, collection, (NoResString)"Message");
			AssertNoErrors(SuperDummy.SS_GuidInfo);
		}

		class DummyBizoCollectionWithExternalListValidation : DummyBusinessObjectCollection, IExternalListValidation
		{
			public DummyBizoCollectionWithExternalListValidation(BusinessObjectFactory factory) : base(factory) { }

			bool IExternalListValidation.IsValidTemplateRecordPK(ZGuid pk)
			{
				return ExternalIsValidPKForTets;
			}

			public bool ExternalIsValidPKForTets { get; set; }
		}

		#endregion

		#region ErrorIfInvalidCode

		[ExpectException(typeof(ArgumentNullException))]
		public void TestErrorIfInvalidCode_RequiresList()
		{
			ListValidation.ErrorIfInvalidCode(SuperDummy.SS_GuidInfo, (IBusinessObjectCollection)null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestErrorIfInvalidCode_RequiresMessage()
		{
			ListValidation.ErrorIfInvalidCode(SuperDummy.SS_GuidInfo, DummyList, null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestErrorIfInvalidCode_RequiresMessageIsNonEmpty()
		{
			ListValidation.ErrorIfInvalidCode(SuperDummy.SS_GuidInfo, DummyList, null);
		}

		public void TestErrorIfInvalidCode_NotInList_With_Message_Ovveride()
		{
			SuperDummyBusinessObject superDummy = (SuperDummyBusinessObject)Factory.New(typeof(SuperDummyBusinessObject));
			ListValidation validation = new ListValidation();

			DummyBusinessObjectCollection list = new DummyBusinessObjectCollection(Factory);
			superDummy.SS_NameList = list;
			DummyBusinessObject dummy1 = NewDummyToList(list);
			DummyBusinessObject dummy2 = NewDummyToList(list);
			DummyBusinessObject dummy3 = NewDummyToList(list);

			superDummy.SS_Name = "Code1";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(superDummy.SS_NameInfo, (NoResString)"SS_Name");
			AssertEquals("Dummy should have no errors", false, superDummy.HasErrors);

			superDummy.SS_Name = "Code4";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(superDummy.SS_NameInfo, list);
			AssertEquals("Dummy should have errors", true, superDummy.HasErrors);
			AssertEquals("Dummy error is wrong", "Error - SS_Name: Enter a valid selection.", superDummy.Notifications.GetErrors().GetFirstMessage());
		}

		public void TestErrorIfInvalidCode_NotInListErrorMessage_Without_List()
		{
			SuperDummyBusinessObject superDummy = Factory.New<SuperDummyBusinessObject>();
			ListValidation validation = new ListValidation();

			DummyBusinessObjectCollection list = new DummyBusinessObjectCollection(Factory);
			superDummy.SS_NameList = list;
			DummyBusinessObject dummy1 = NewDummyToList(list);
			DummyBusinessObject dummy2 = NewDummyToList(list);
			DummyBusinessObject dummy3 = NewDummyToList(list);

			superDummy.SS_Name = "Code4";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode((NoResString)"Test Error Message", superDummy.SS_NameInfo);

			AssertEquals("Error - SS_Name: Test Error Message", superDummy.Notifications.GetErrors().GetFirstMessage());
		}

		public void TestErrorIfInvalidCode_NotInList_WithoutListFilter()
		{
			SuperDummyBusinessObject superDummy = (SuperDummyBusinessObject)Factory.New(typeof(SuperDummyBusinessObject));
			ListValidation validation = new ListValidation();

			DummyBusinessObjectCollection list = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject dummy1 = NewDummyToList(list);
			DummyBusinessObject dummy2 = NewDummyToList(list);
			DummyBusinessObject dummy3 = NewDummyToList(list);

			superDummy.SS_Name = "Code1";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(superDummy.SS_NameInfo, list);
			AssertEquals("Dummy should have no errors", false, superDummy.HasErrors);

			superDummy.SS_Name = "Code4";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(superDummy.SS_NameInfo, list);
			AssertEquals("Dummy should have errors", true, superDummy.HasErrors);
		}

		public void TestErrorIfInvalidCode_NotInListErrorMessage()
		{
			SuperDummyBusinessObject superDummy = Factory.New<SuperDummyBusinessObject>();
			ListValidation validation = new ListValidation();

			DummyBusinessObjectCollection list = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject dummy1 = NewDummyToList(list);
			DummyBusinessObject dummy2 = NewDummyToList(list);
			DummyBusinessObject dummy3 = NewDummyToList(list);

			superDummy.SS_Name = "Code4";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(superDummy.SS_NameInfo, list, (NoResString)"Test Error Message");

			AssertEquals("Error - SS_Name: Test Error Message", superDummy.Notifications.GetErrors().GetFirstMessage());
		}

		public void TestErrorIfInvalidCode_NotInList_WithListFilter()
		{
			SuperDummyBusinessObject superDummy = (SuperDummyBusinessObject)Factory.New(typeof(SuperDummyBusinessObject));
			ListValidation validation = new ListValidation();

			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "Code2");
			DummyBusinessObjectCollection list = new DummyBusinessObjectCollection(Factory, filter);
			DummyBusinessObject dummy1 = NewDummyToList(list);
			DummyBusinessObject dummy2 = NewDummyToList(list);
			DummyBusinessObject dummy3 = NewDummyToList(list);

			superDummy.SS_Name = "Code1";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(superDummy.SS_NameInfo, list);
			AssertEquals("Dummy should have errors - doesn't match List filter", true, superDummy.HasErrors);

			superDummy.SS_Name = "Code4";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(superDummy.SS_NameInfo, list);
			AssertEquals("Dummy should have errors - doesn't exist", true, superDummy.HasErrors);

			superDummy.SS_Name = "Code2";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(superDummy.SS_NameInfo, list);
			AssertEquals("Dummy should have no errors - matches List filter and exist", false, superDummy.HasErrors);
		}

		public void TestErrorIfInvalidCode_NotInList_CodeDescriptionPairList()
		{
			ListValidation validation = new ListValidation();
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("Code1", "Description 1");
			list.AddPair("Code2", "Description 2");
			list.AddPair("Code3", "Description 3");

			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Code = "Code1";
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(Dummy.Z0_CodeInfo, list);
				AssertEquals("Dummy should have no errors", false, Dummy.HasErrors);

				Dummy.Z0_Code = "Code4";
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(Dummy.Z0_CodeInfo, list);
				AssertEquals("Dummy should have errors", true, Dummy.HasErrors);
			}
		}

		public void TestErrorIfInvalidCode_NotInList_CodeDescriptionPairList_CustomPropertyName()
		{
			ListValidation validation = new ListValidation();
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("Code1", "Description 1");

			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Code = "Code4";
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(Dummy.Z0_CodeInfo, list, (NoResString)"meh meh");
				AssertEquals("Dummy should have errors", true, Dummy.HasErrors);
				AssertCollectionContains("Enter a valid meh meh.", Dummy.Z0_CodeInfo.GetErrors().GetUniqueMessageList());
			}
		}

		#endregion

		#region ErrorIfInvalidCodeOrEmpty

		public void TestErrorIfInvalidCodeOrEmpty_BusinessObjectCollection()
		{
			var collection = new DummyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Code, "CODE"));
			collection.AddNew().Z0_Code = "CODE";

			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Code = "";
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				AssertEquals("Precondition", false, Dummy.Z0_CodeInfo.HasErrors());
				ListValidation.ErrorIfInvalidCodeOrEmpty(Dummy.Z0_CodeInfo, collection);
				AssertHasErrorContaining(Dummy.Z0_CodeInfo, MandatoryValidation.MustBeEntered);

				Dummy.Z0_Code = "CODE";
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCodeOrEmpty(Dummy.Z0_CodeInfo, collection);
				AssertNoErrorContaining(Dummy.Z0_CodeInfo, MandatoryValidation.MustBeEntered);
				AssertEquals("No error with valid value", false, Dummy.Z0_CodeInfo.HasErrors());

				Dummy.Z0_Code = "XXX";
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCodeOrEmpty(Dummy.Z0_CodeInfo, collection);
				AssertHasErrorContaining(Dummy.Z0_CodeInfo, "Enter a valid Code.");
			}
		}

		public void TestErrorIfInvalidCodeOrEmpty()
		{
			var superDummy = Factory.New<SuperDummyBusinessObject>();
			var list = new DummyBusinessObjectCollection(Factory);
			superDummy.SS_NameList = list;
			NewDummyToList(list);
			NewDummyToList(list);
			NewDummyToList(list);

			using (Dummy.SuspendValidationTesting())
			{
				superDummy.SS_Name = "";
				superDummy.SS_NameInfo.ClearAllNotifications();
				AssertEquals("Precondition", false, superDummy.SS_NameInfo.HasErrors());
				ListValidation.ErrorIfInvalidCodeOrEmpty(superDummy.SS_NameInfo);
				AssertHasErrorContaining(superDummy.SS_NameInfo, "Please enter a value.");

				superDummy.SS_Name = "CODE";
				superDummy.SS_NameInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCodeOrEmpty(superDummy.SS_NameInfo);
				AssertNoErrorContaining(superDummy.SS_NameInfo, "Please enter a value.");
				AssertEquals("No error with valid value", true, superDummy.SS_NameInfo.HasErrors());

				superDummy.SS_Name = "XXX";
				superDummy.SS_NameInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCodeOrEmpty(superDummy.SS_NameInfo);
				AssertHasErrorContaining(superDummy.SS_NameInfo, "Enter a valid selection.");
			}
		}

		#endregion

		#region WarnIfInvalidPK

		public void TestWarnIfInvalidPK_NotInList_WithoutListFilter()
		{
			SuperDummyWithFKBusinessObject superDummy = (SuperDummyWithFKBusinessObject)Factory.New(typeof(SuperDummyWithFKBusinessObject));
			ListValidation validation = new ListValidation();

			DummyBusinessObjectCollection list = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject dummy1 = NewDummyToList(list);
			DummyBusinessObject dummy2 = NewDummyToList(list);
			DummyBusinessObject dummy3 = NewDummyToList(list);

			superDummy.SS_Guid = dummy1.PK;
			superDummy.SS_GuidInfo.ClearAllNotifications();
			ListValidation.WarnIfInvalidPK(superDummy.SS_GuidInfo, list);
			AssertEquals("Dummy should have no warnings", false, superDummy.HasWarnings);

			superDummy.SS_Guid = ZGuid.NewZGuid();
			superDummy.SS_GuidInfo.ClearAllNotifications();
			ListValidation.WarnIfInvalidPK(superDummy.SS_GuidInfo, list);
			AssertEquals("Dummy should have warnings", true, superDummy.HasWarnings);
		}

		public void TestWarnIfInvalidPK_NotInList_WithListFilter()
		{
			SuperDummyWithFKBusinessObject superDummy = (SuperDummyWithFKBusinessObject)Factory.New(typeof(SuperDummyWithFKBusinessObject));
			ListValidation validation = new ListValidation();

			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "Code2");
			DummyBusinessObjectCollection list = new DummyBusinessObjectCollection(Factory, filter);
			DummyBusinessObject dummy1 = NewDummyToList(list);
			DummyBusinessObject dummy2 = NewDummyToList(list);
			DummyBusinessObject dummy3 = NewDummyToList(list);

			superDummy.SS_Guid = dummy1.PK;
			superDummy.SS_GuidInfo.ClearAllNotifications();
			ListValidation.WarnIfInvalidPK(superDummy.SS_GuidInfo, list);
			AssertEquals("Dummy should have warnings - doesn't match List filter", true, superDummy.HasWarnings);

			superDummy.SS_Guid = ZGuid.NewZGuid();
			superDummy.SS_GuidInfo.ClearAllNotifications();
			ListValidation.WarnIfInvalidPK(superDummy.SS_GuidInfo, list);
			AssertEquals("Dummy should have warnings - doesn't exist", true, superDummy.HasWarnings);

			superDummy.SS_Guid = dummy2.PK;
			superDummy.SS_GuidInfo.ClearAllNotifications();
			ListValidation.WarnIfInvalidPK(superDummy.SS_GuidInfo, list);
			AssertEquals("Dummy should have no warnings - matches List filter and exist", false, superDummy.HasWarnings);
		}

		public void TestWarnIfInvalidPK_NotInList_WithoutListFilter_WithErrorMessage()
		{
			SuperDummyWithFKBusinessObject superDummy = (SuperDummyWithFKBusinessObject)Factory.New(typeof(SuperDummyWithFKBusinessObject));
			ListValidation validation = new ListValidation();

			DummyBusinessObjectCollection list = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject dummy1 = NewDummyToList(list);
			DummyBusinessObject dummy2 = NewDummyToList(list);
			DummyBusinessObject dummy3 = NewDummyToList(list);

			superDummy.SS_Guid = dummy1.PK;
			superDummy.SS_GuidInfo.ClearAllNotifications();
			ListValidation.WarnIfInvalidPK(superDummy.SS_GuidInfo, list, (NoResString)"Message");
			AssertNoWarnings("Dummy should have no warnings", superDummy);

			superDummy.SS_Guid = ZGuid.NewZGuid();
			superDummy.SS_GuidInfo.ClearAllNotifications();
			ListValidation.WarnIfInvalidPK(superDummy.SS_GuidInfo, list, (NoResString)"Message");
			AssertHasWarning("Dummy should have warnings", superDummy.SS_GuidInfo, "Message");
		}

		public void TestWarnIfInvalidPK_NotInList_WithListFilter_WithErrorMessage()
		{
			SuperDummyWithFKBusinessObject superDummy = (SuperDummyWithFKBusinessObject)Factory.New(typeof(SuperDummyWithFKBusinessObject));
			ListValidation validation = new ListValidation();

			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "Code2");
			DummyBusinessObjectCollection list = new DummyBusinessObjectCollection(Factory, filter);
			DummyBusinessObject dummy1 = NewDummyToList(list);
			DummyBusinessObject dummy2 = NewDummyToList(list);
			DummyBusinessObject dummy3 = NewDummyToList(list);

			superDummy.SS_Guid = dummy1.PK;
			superDummy.SS_GuidInfo.ClearAllNotifications();
			ListValidation.WarnIfInvalidPK(superDummy.SS_GuidInfo, list, (NoResString)"Message");
			AssertHasWarning("Dummy should have warnings - doesn't match List filter", superDummy.SS_GuidInfo, "Message");

			superDummy.SS_Guid = ZGuid.NewZGuid();
			superDummy.SS_GuidInfo.ClearAllNotifications();
			ListValidation.WarnIfInvalidPK(superDummy.SS_GuidInfo, list, (NoResString)"Message");
			AssertHasWarning("Dummy should have warnings - doesn't exist", superDummy.SS_GuidInfo, "Message");

			superDummy.SS_Guid = dummy2.PK;
			superDummy.SS_GuidInfo.ClearAllNotifications();
			ListValidation.WarnIfInvalidPK(superDummy.SS_GuidInfo, list, (NoResString)"Message");
			AssertNoWarnings("Dummy should have no warnings - matches List filter and exist", superDummy.SS_GuidInfo);
		}

		#endregion

		#region WarnIfInvalidCode

		public void TestWarnIfInvalidCode_NotInList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("Code1", "Description 1");
			list.AddPair("Code2", "Description 2");
			list.AddPair("Code3", "Description 3");

			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Code = "Code1";
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				ListValidation.WarnIfInvalidCode(Dummy.Z0_CodeInfo, list);
				AssertEquals("Dummy should have no warnings", false, Dummy.HasWarnings);

				Dummy.Z0_Code = "Code4";
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				ListValidation.WarnIfInvalidCode(Dummy.Z0_CodeInfo, list);
				AssertEquals("Dummy should have warnings", true, Dummy.HasWarnings);
				AssertHasWarning(Dummy.Z0_CodeInfo, "You have not entered a valid code.");
			}
		}

		public void TestWarnIfInvalidCode_NotInList_WithWarningMessagePrefix()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("Code1", "Description 1");
			list.AddPair("Code2", "Description 2");
			list.AddPair("Code3", "Description 3");

			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Code = "Code1";
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				ListValidation.WarnIfInvalidCode(Dummy.Z0_CodeInfo, list, "BOGAN: ");
				AssertEquals("Dummy should have no warnings", false, Dummy.HasWarnings);

				Dummy.Z0_Code = "Code4";
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				ListValidation.WarnIfInvalidCode(Dummy.Z0_CodeInfo, list, "BOGAN: ");
				AssertEquals("Dummy should have warnings", true, Dummy.HasWarnings);
				AssertHasWarning(Dummy.Z0_CodeInfo, "BOGAN: You have not entered a valid code.");
			}
		}

		public void TestWarnIfInvalidCode_NotInListErrorMessage_Without_List()
		{
			SuperDummyBusinessObject superDummy = Factory.New<SuperDummyBusinessObject>();

			DummyBusinessObjectCollection list = new DummyBusinessObjectCollection(Factory);
			superDummy.SS_NameList = list;
			DummyBusinessObject dummy1 = NewDummyToList(list);
			DummyBusinessObject dummy2 = NewDummyToList(list);
			DummyBusinessObject dummy3 = NewDummyToList(list);

			superDummy.SS_Name = "Code4";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.WarnIfInvalidCode(superDummy.SS_NameInfo, (NoResString)"Test Warning Message");
			AssertHasWarning(superDummy.SS_NameInfo, "Test Warning Message");
			superDummy.SS_Name = ZString.Empty;
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.WarnIfInvalidCode(superDummy.SS_NameInfo, (NoResString)"Test Warning Message");
			AssertNoWarning(superDummy.SS_NameInfo, "Test Warning Message");
			superDummy.SS_Name = "Code2";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.WarnIfInvalidCode(superDummy.SS_NameInfo, (NoResString)"Test Warning Message");
			AssertNoWarning(superDummy.SS_NameInfo, "Test Warning Message");
		}

		#endregion

		#region MessageErrorIfInvalidCodeOrEmpty

		public void TestMessageErrorIfInvalidCodeOrEmptyBusinessObjectCollection()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Code, "CODE"));
			collection.AddNew().Z0_Code = "CODE";
			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Code = "";
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				AssertEquals("Precondition", false, Dummy.Z0_CodeInfo.HasMessageErrors());
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Dummy.Z0_CodeInfo, collection);
				AssertHasMessageErrors(Dummy.Z0_CodeInfo);
				Dummy.Z0_Code = "XYZ";
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Dummy.Z0_CodeInfo, collection);
				AssertHasMessageErrors(Dummy.Z0_CodeInfo);
				Dummy.Z0_Code = "CODE";
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Dummy.Z0_CodeInfo, collection);
				AssertNoMessageErrors(Dummy.Z0_CodeInfo);
			}
		}

		public void TestMessageErrorIfInvalidCodeOrEmpty()
		{
			ListValidation validation = new ListValidation();
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("Code1", "Description 1");

			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Code = "";
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				AssertEquals("Precondition", false, Dummy.Z0_CodeInfo.HasMessageErrors());
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Dummy.Z0_CodeInfo, list);
				AssertHasMessageErrors(Dummy.Z0_CodeInfo);

				Dummy.Z0_Code = list[0].Code;
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Dummy.Z0_CodeInfo, list);
				AssertEquals("No error with valid value", false, Dummy.Z0_CodeInfo.HasMessageErrors());

				Dummy.Z0_Code = "XXX";
				Dummy.Z0_CodeInfo.ClearAllNotifications();
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Dummy.Z0_CodeInfo, list);
				AssertHasMessageErrors(Dummy.Z0_CodeInfo);
			}
		}

		public void TestMessageErrorIfInvalidCodeOrEmptyPropertyDescriptionOverride()
		{
			var dummyWithListAttribute = Factory.New<DummyBusinessObjectWithListAttributeOnZ0_Code>();
			using (dummyWithListAttribute.SuspendValidationTesting())
			{
				dummyWithListAttribute.Z0_Code = ZString.Empty;
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(dummyWithListAttribute.Z0_CodeInfo, "TEST DESCRIPTION");
				AssertHasMessageErrorContaining(dummyWithListAttribute.Z0_CodeInfo, "TEST DESCRIPTION");
			}
		}

		#endregion

		#region MessageErrorIfInvalidCodeCaseSensitive

		public void TestMessageErrorIfInvalidCodeCaseSensitive_ExplicitCodeDescriptionPairList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("Code", "Description");

			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Code = "Code";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitive(Dummy.Z0_CodeInfo, list);
				AssertNoMessageError("Code is in the list", Dummy.Z0_CodeInfo, ListValidation.InvalidCodeMessageError);

				Dummy.Z0_Code = "CODE";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitive(Dummy.Z0_CodeInfo, list);
				AssertHasMessageError("Code is not in the list", Dummy.Z0_CodeInfo, ListValidation.InvalidCodeMessageError);

				Dummy.Z0_Code = "";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitive(Dummy.Z0_CodeInfo, list);
				AssertNoMessageError("Code is empty", Dummy.Z0_CodeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestMessageErrorIfInvalidCodeCaseSensitive_ImplicitCodeDescriptionPairList()
		{
			var dummyWithListAttribute = Factory.New<DummyBusinessObjectWithListAttributeOnZ0_Code>();
			using (dummyWithListAttribute.SuspendValidationTesting())
			{
				dummyWithListAttribute.Z0_Code = "Code";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitive(dummyWithListAttribute.Z0_CodeInfo);
				AssertNoMessageError("Code is in the list", dummyWithListAttribute.Z0_CodeInfo, ListValidation.InvalidCodeMessageError);

				dummyWithListAttribute.Z0_Code = "CODE";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitive(dummyWithListAttribute.Z0_CodeInfo);
				AssertHasMessageError("Code is not in the list", dummyWithListAttribute.Z0_CodeInfo, ListValidation.InvalidCodeMessageError);

				dummyWithListAttribute.Z0_Code = "";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitive(dummyWithListAttribute.Z0_CodeInfo);
				AssertNoMessageError("Code is empty", dummyWithListAttribute.Z0_CodeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestMessageErrorIfInvalidCodeCaseSensitive_BusinessObjectCollection()
		{
			var filter = new ZQuery(DummyBizoSchema.Z0_Code, "Code");
			filter.IsDBOnlyQuery = true;
			var collection = new DummyBusinessObjectCollection(Factory, filter);
			collection.AddNew().Z0_Code = "Code";
			Factory.Save();
			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Code = "Code";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitive(Dummy.Z0_CodeInfo, collection);
				AssertNoMessageError("Code is in the list", Dummy.Z0_CodeInfo, ListValidation.InvalidCodeMessageError);

				Dummy.Z0_Code = "CODE";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitive(Dummy.Z0_CodeInfo, collection);
				AssertHasMessageError("Code is not in the list", Dummy.Z0_CodeInfo, ListValidation.InvalidCodeMessageError);

				Dummy.Z0_Code = "";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitive(Dummy.Z0_CodeInfo, collection);
				AssertNoMessageError("Code is empty", Dummy.Z0_CodeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		#endregion

		#region MessageErrorIfInvalidCodeCaseSensitiveOrEmpty

		public void TestMessageErrorIfInvalidCodeCaseSensitiveOrEmpty_ExplicitList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("Code", "Description");

			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Code = "Code";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitiveOrEmpty(Dummy.Z0_CodeInfo, list);
				AssertNoMessageError("Code is in the list", Dummy.Z0_CodeInfo, ListValidation.InvalidCodeMessageError);

				Dummy.Z0_Code = "CODE";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitiveOrEmpty(Dummy.Z0_CodeInfo, list);
				AssertHasMessageError("Code is not in the list", Dummy.Z0_CodeInfo, ListValidation.InvalidCodeMessageError);

				Dummy.Z0_Code = "";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitiveOrEmpty(Dummy.Z0_CodeInfo, list);
				AssertHasMessageErrorContaining("Code is empty", Dummy.Z0_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestMessageErrorIfInvalidCodeCaseSensitiveOrEmpty_ImplicitList()
		{
			var dummyWithListAttribute = Factory.New<DummyBusinessObjectWithListAttributeOnZ0_Code>();
			using (dummyWithListAttribute.SuspendValidationTesting())
			{
				dummyWithListAttribute.Z0_Code = "Code";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitiveOrEmpty(dummyWithListAttribute.Z0_CodeInfo);
				AssertNoMessageError("Code is in the list", dummyWithListAttribute.Z0_CodeInfo, ListValidation.InvalidCodeMessageError);

				dummyWithListAttribute.Z0_Code = "CODE";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitiveOrEmpty(dummyWithListAttribute.Z0_CodeInfo);
				AssertHasMessageError("Code is not in the list", dummyWithListAttribute.Z0_CodeInfo, ListValidation.InvalidCodeMessageError);

				dummyWithListAttribute.Z0_Code = "";
				ListValidation.MessageErrorIfInvalidCodeCaseSensitiveOrEmpty(dummyWithListAttribute.Z0_CodeInfo);
				AssertHasMessageErrorContaining("Code is empty", dummyWithListAttribute.Z0_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		#endregion

		public void TestErrorOnListWithListFilterWithOrs()
		{
			SuperDummyBusinessObject superDummy = (SuperDummyBusinessObject)Factory.New(typeof(SuperDummyBusinessObject));
			ListValidation validation = new ListValidation();

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "Code2");
			filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "Code3");
			DummyBusinessObjectCollection list = new DummyBusinessObjectCollection(Factory, filter);
			DummyBusinessObject dummy1 = NewDummyToList(list);
			DummyBusinessObject dummy2 = NewDummyToList(list);
			DummyBusinessObject dummy3 = NewDummyToList(list);

			superDummy.SS_Name = "Code1";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(superDummy.SS_NameInfo, list);
			AssertEquals("Dummy should have errors - doesn't match List filter", true, superDummy.HasErrors);

			superDummy.SS_Name = "Code4";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(superDummy.SS_NameInfo, list);
			AssertEquals("Dummy should have errors - doesn't exist", true, superDummy.HasErrors);

			superDummy.SS_Name = "Code2";
			superDummy.SS_NameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(superDummy.SS_NameInfo, list);
			AssertEquals("Dummy should have no errors - matches List filter and exist", false, superDummy.HasErrors);
		}

		#region CancellableDummy

		class CancellableDummy : DummyBusinessObject, ICancellable
		{
			public CancellableDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region ICancellable Members

			public string CanCancel()
			{
				throw new NotImplementedException();
			}

			public string CanReactivate()
			{
				throw new NotImplementedException();
			}

			public bool IsCancelled
			{
				get;
				set;
			}

			public bool IsCancelledHasChanged
			{
				get { throw new NotImplementedException(); }
			}

			#endregion
		}

		class CancellableDummyCollection : DummyBusinessObjectCollection
		{
			public CancellableDummyCollection(BusinessObjectFactory factory)
				: base(factory) { }

			public override Type GetTypeOfElementsFromPK(ZGuid pK)
			{
				return typeof(CancellableDummy);
			}
		}

		#endregion

		public void TestInvalidPKDueToCancelled()
		{
			AssertInvalidPKDueToCancelled(Factory.New<CancellableDummy>(), new CancellableDummyCollection(Factory));
		}

		public void TestInvalidPKDueToCancelledWithNonBizoCollection()
		{
			AssertInvalidPKDueToCancelled(Factory.New<CancellableDummy>(), new CompositeCollectionWithCancellableDummy());
		}

		void AssertInvalidPKDueToCancelled(CancellableDummy cancellableDummy, IList list)
		{
			list.Add(cancellableDummy);
			SuperDummy.SS_List = list;

			SuperDummy.SS_Guid = cancellableDummy.PK;
			SuperDummy.SS_GuidInfo.ClearAllNotifications();

			cancellableDummy.IsCancelled = false;
			ListValidation.ErrorIfCancelledAndEditable(SuperDummy.SS_GuidInfo);
			AssertNoErrors("Should have no errors", SuperDummy.SS_GuidInfo);

			cancellableDummy.IsCancelled = true;
			ListValidation.ErrorIfCancelledAndEditable(SuperDummy.SS_GuidInfo);
			AssertHasError("Should find inactive element via [LIST] and add error", SuperDummy.SS_GuidInfo, "This selection is inactive - it may not be used.");
		}

		public void TestInvalidCodeDueToCancelled()
		{
			AssertInvalidCodeDueToCancelled(Factory.New<CancellableDummy>(), new CancellableDummyCollection(Factory));
		}

		public void TestInvalidCodeDueToCancelledWithNonBizoCollection()
		{
			AssertInvalidCodeDueToCancelled(Factory.New<CancellableDummy>(), new CompositeCollectionWithCancellableDummy());
		}

		void AssertInvalidCodeDueToCancelled(CancellableDummy cancellableDummy, IList list)
		{
			list.Add(cancellableDummy);
			cancellableDummy.Z0_Code = "Canc";
			SuperDummy.SS_NameList = list;

			SuperDummy.SS_Name = cancellableDummy.Z0_Code;
			SuperDummy.SS_NameInfo.ClearAllNotifications();

			cancellableDummy.IsCancelled = false;
			ListValidation.ErrorIfCancelledAndEditable(SuperDummy.SS_NameInfo);
			AssertNoErrors("Should have no errors", SuperDummy.SS_NameInfo);

			cancellableDummy.IsCancelled = true;
			ListValidation.ErrorIfCancelledAndEditable(SuperDummy.SS_NameInfo);
			AssertHasError("Should find inactive element via [LIST] and add error", SuperDummy.SS_NameInfo, "This selection is inactive - it may not be used.");
		}

		public void TestErrorIfCancelled()
		{
			var dummy = Factory.New<SuperDummyWithFKBusinessObject>();
			dummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfCancelled(dummy.SS_GuidInfo, false, true);
			AssertNoErrors(dummy.SS_GuidInfo);
			dummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfCancelled(dummy.SS_GuidInfo, true, false);
			AssertHasError(dummy.SS_GuidInfo, "This selection is inactive - it may not be used.");
			dummy.SS_GuidInfo.ClearAllNotifications();

			ListValidation.ErrorIfCancelled(dummy.SS_GuidInfo, true, true);
			AssertHasWarning(dummy.SS_GuidInfo, "This selection is inactive.");
		}

		public void TestGetNotificationMessage()
		{
			AssertEquals("Enter a valid meh meh.", ListValidation.GetNotificationMessage("meh meh").ToString());
		}

		#region Support

		SuperDummyWithFKBusinessObject superDummy;

		SuperDummyWithFKBusinessObject SuperDummy
		{
			get { return superDummy ?? (superDummy = (SuperDummyWithFKBusinessObject)Factory.New(typeof(SuperDummyWithFKBusinessObject))); }
			set { superDummy = value; }
		}

		DummyBusinessObjectCollection dummyList;

		DummyBusinessObjectCollection DummyList
		{
			get { return dummyList ?? (dummyList = new DummyBusinessObjectCollection(Factory)); }
			set { dummyList = value; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			SuperDummy = null;
			DummyList = null;
			SuperDummy.SS_List = DummyList;
		}

		DummyBusinessObject NewDummyToList(DummyBusinessObjectCollection list)
		{
			DummyBusinessObject newDummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			newDummy.Z0_Code = "Code" + (list.Count + 1);
			list.Add(newDummy);

			return newDummy;
		}

		#endregion

		#region DummyWithListAttributeOnZ0_Code

		public class DummyBusinessObjectWithListAttributeOnZ0_Code : DummyBusinessObject
		{
			public DummyBusinessObjectWithListAttributeOnZ0_Code(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[List("Code_List")]
			public override ZString Z0_Code { get => base.Z0_Code; set => base.Z0_Code = value; }

			public CodeDescriptionPairList Code_List
			{
				get
				{
					var list = new CodeDescriptionPairList();
					list.Add(new CodeDescriptionPair("Code", "Description"));
					return list;
				}
			}
		}

		#endregion

		#region SuperDummyWithFKBusinessObject

		class SuperDummyWithFKBusinessObject : SuperDummyBusinessObject
		{
			#region Property Constants

			public new abstract class Schema : SuperDummyBusinessObject.Schema
			{
				public const string SS_Guid = "Z0_Guid";
			}

			#endregion

			public SuperDummyWithFKBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row) { }

			#region SS_Guid

			[List("SS_List")]
			public virtual ZGuid SS_Guid
			{
				get { return new ZGuid(Row[Schema.SS_Guid]); }
				set
				{
					if (value != SS_Guid)
					{
						HasChanges = true;

						Row[Schema.SS_Guid] = ((IZTypeInternals)value).GetValueForLogicalDataLayer(false);
						SS_GuidInfo.RefreshBinding();
					}
				}
			}

			public ZPropertyInfo SS_GuidInfo
			{
				get { return GetZPropertyInfo(nameof(SS_Guid)); }
			}

			public IList SS_List { get; set; }

			#endregion
		}

		#endregion

		#region CompositeCollectionWithCancellableDummy

		class CompositeCollectionWithCancellableDummy : List<CancellableDummy>, ICompositeCollection
		{
			public Type TypeOfElementFromPK(ZGuid pk)
			{
				return typeof(CancellableDummy);
			}

			public Type TypeOfElementFromCode(ZString code)
			{
				return typeof(CancellableDummy);
			}

			public int MaxLength { get { return 0; } }
		}

		#endregion
	}
}
