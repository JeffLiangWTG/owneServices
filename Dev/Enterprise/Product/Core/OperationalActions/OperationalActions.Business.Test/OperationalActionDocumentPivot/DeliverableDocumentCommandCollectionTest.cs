using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(DeliverableDocumentCommandCollection))]
	internal sealed class DeliverableDocumentCommandCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFilterDefaults()
		{
			DeliverableDocumentCommandCollection collection = new DeliverableDocumentCommandCollection(Factory, "Blaticus", ZString.Empty, true);
			AssertHasDefault(collection, "Business Context", "Property", new ZString("Blaticus"));
		}

		public void TestBusinessContextConstraint()
		{
			DocumentCommand command1 = Factory.New<DocumentCommand>();
			command1.SU_BusinessContext = "DummyContext";
			command1.SU_GS_NKStaffCode = "";
			command1.SU_ContactType = ContactType.Receivables.Code;
			DocumentCommand command2 = Factory.New<DocumentCommand>();
			command2.SU_BusinessContext = "OtherContext";
			command2.SU_GS_NKStaffCode = "";
			command2.SU_ContactType = ContactType.Receivables.Code;
			DeliverableDocumentCommandCollection collection = new DeliverableDocumentCommandCollection(Factory, "DummyContext", ZString.Empty, false);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Documents expected to match.", (c) => c.SU_BusinessContext.ToString(), new DocumentCommand[] { command1, }, collection.ToArray<DocumentCommand>());
			string error;
			error = collection.GetAllNotificationsWhenAdditionalFilterNotMet(command2);
			AssertEquals("This document has the wrong business context.", error);
		}

		public void TestStaffConstraint()
		{
			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "XX1";
			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "XX2";
			DocumentCommand command1 = Factory.New<DocumentCommand>();
			command1.SU_MenuName = "command1";
			command1.SU_BusinessContext = "DummyContext";
			command1.SU_GS_NKStaffCode = staff1.GS_Code;
			command1.SU_ContactType = ContactType.Receivables.Code;
			DocumentCommand command2 = Factory.New<DocumentCommand>();
			command2.SU_MenuName = "command2";
			command2.SU_BusinessContext = "DummyContext";
			command2.SU_GS_NKStaffCode = staff2.GS_Code;
			command2.SU_ContactType = ContactType.Receivables.Code;
			DocumentCommand command3 = Factory.New<DocumentCommand>();
			command3.SU_MenuName = "command3";
			command3.SU_BusinessContext = "DummyContext";
			command3.SU_GS_NKStaffCode = "";
			command3.SU_ContactType = ContactType.Receivables.Code;
			string error;
			DeliverableDocumentCommandCollection collection;
			collection = new DeliverableDocumentCommandCollection(Factory, "DummyContext", ZString.Empty, false);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Documents expected to match. (published-only)", (c) => c.SU_MenuName, new DocumentCommand[] { command3 }, collection.ToArray<DocumentCommand>());
			error = collection.GetAllNotificationsWhenAdditionalFilterNotMet(command1);
			AssertEquals("Only published documents may be selected.", error);
			error = collection.GetAllNotificationsWhenAdditionalFilterNotMet(command2);
			AssertEquals("Only published documents may be selected.", error);
			collection = new DeliverableDocumentCommandCollection(Factory, "DummyContext", staff2.GS_Code, false);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Documents expectd to match. (inc. staff2)", (c) => c.SU_MenuName, new DocumentCommand[] { command2, command3 }, collection.ToArray<DocumentCommand>());
			error = collection.GetAllNotificationsWhenAdditionalFilterNotMet(command1);
			AssertEquals("Unpublished documents belonging to other users may not be selected.", error);
		}

		public void TestSystemDefinedConstraint()
		{
			DocumentCommand command1 = Factory.New<DocumentCommand>();
			command1.SU_MenuName = "command1";
			command1.SU_BusinessContext = "DummyContext";
			command1.SU_GS_NKStaffCode = "";
			command1.SU_ContactType = ContactType.Receivables.Code;
			command1.SU_IsClientSpecific = false;
			command1.SU_IsSystemDefined = false;
			command1.SU_IsPublished = true;
			DocumentCommand command2 = Factory.New<DocumentCommand>();
			command2.SU_MenuName = "command2";
			command2.SU_BusinessContext = "DummyContext";
			command2.SU_GS_NKStaffCode = "";
			command2.SU_ContactType = ContactType.Receivables.Code;
			command2.SU_IsClientSpecific = false;
			command2.SU_IsSystemDefined = true;
			DocumentCommand command3 = Factory.New<DocumentCommand>();
			command3.SU_MenuName = "command3";
			command3.SU_BusinessContext = "DummyContext";
			command3.SU_GS_NKStaffCode = "";
			command3.SU_ContactType = ContactType.Receivables.Code;
			command3.SU_IsClientSpecific = true;
			command3.SU_IsSystemDefined = true;
			string error;
			DeliverableDocumentCommandCollection collection;
			collection = new DeliverableDocumentCommandCollection(Factory, "DummyContext", ZString.Empty, false);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Documents expected to match. (any)", (c) => c.SU_MenuName, new DocumentCommand[] { command1, command2, command3 }, collection.ToArray<DocumentCommand>());
			collection = new DeliverableDocumentCommandCollection(Factory, "DummyContext", ZString.Empty, true);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Documents expected to match. (system-only)", (c) => c.SU_MenuName, new DocumentCommand[] { command2 }, collection.ToArray<DocumentCommand>());
			error = collection.GetAllNotificationsWhenAdditionalFilterNotMet(command1);
			AssertEquals("Only system defined documents may be selected.", error);
			error = collection.GetAllNotificationsWhenAdditionalFilterNotMet(command3);
			AssertEquals("Client specific documents may not be selected.", error);
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DeliverableDocumentCommandCollection(Factory, MockOperationalActionSupportable.BusinessContext.ToString(), ZString.Empty, true);
		}
		#endregion
	}
}
