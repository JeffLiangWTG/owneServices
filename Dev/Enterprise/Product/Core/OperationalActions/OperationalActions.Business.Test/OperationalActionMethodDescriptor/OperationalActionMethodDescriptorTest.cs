using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OperationalActionMethodDescriptor))]
	internal sealed class OperationalActionMethodDescriptorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetMethodFromDescriptor()
		{
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			Action.SU_FilterList = "";
			MethodDescriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			MethodDescriptor.MethodID = TestingConstants.DummyActionMethodWithLargeGUI;
			AssertEquals(@"country in (""AU"", ""NZ"", ""SG"")", Action.SU_FilterList);
		}

		public void TestSettings()
		{
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			Action.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			Action.SU_IsSystemDefined = true;
			MethodDescriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			MethodDescriptor.MethodID = TestingConstants.DummyActionMethodWithGUI;
			AssertNotNull(MethodDescriptor.Settings);
			AssertEquals(typeof(DummyOperationalActionMethodSettings), MethodDescriptor.Settings.GetType());
			AssertEquals(true, MethodDescriptor.ReadOnly);
			MethodDescriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			AssertNull(MethodDescriptor.Settings);
			action.SU_IsSystemDefined = false;
			MethodDescriptor.MethodID = TestingConstants.DummyActionMethodWithGUI;
			AssertNotNull(MethodDescriptor.Settings);
			AssertEquals(typeof(DummyOperationalActionMethodSettings), MethodDescriptor.Settings.GetType());
			AssertEquals(false, MethodDescriptor.ReadOnly);
		}

		public void TestReadOnly()
		{
			Action.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			Action.SU_IsSystemDefined = false;
			AssertEquals("Should not be readonly as not system defined", false, MethodDescriptor.ReadOnly);
			Action.SU_IsSystemDefined = true;
			AssertEquals("Should be readonly as now system defined", true, MethodDescriptor.ReadOnly);
			Action.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("Should not be readonly as editing of system defined actions is now allowed", false, MethodDescriptor.ReadOnly);
		}

		public void TestNameIsReadonlyWhenGroupIsEmpty()
		{
			MethodDescriptor.Context.Supporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			AssertEquals("Group starts empty", true, MethodDescriptor.MethodNameInfo.ReadOnly);
			MethodDescriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			AssertEquals("Group set", false, MethodDescriptor.MethodIDInfo.ReadOnly);
			MethodDescriptor.MethodGroup = ZGuid.Empty;
			AssertEquals("Group cleared", true, MethodDescriptor.MethodNameInfo.ReadOnly);
		}

		public void TestSettingMethodGroupClearsTheMethodID()
		{
			ZGuid group1Id = ZGuid.NewZGuid();
			ZGuid group2Id = ZGuid.NewZGuid();
			MethodDescriptor.MethodGroup = group1Id;
			MethodDescriptor.MethodID = TestingConstants.DummyActionMethodWithGUI;
			AssertEquals("precondition:", group1Id, MethodDescriptor.MethodGroup);
			AssertEquals("precondition:", TestingConstants.DummyActionMethodWithGUI, MethodDescriptor.MethodID);
			MethodDescriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			AssertEquals("changing method name:", group1Id, MethodDescriptor.MethodGroup);
			AssertEquals("changing method name:", TestingConstants.DummyActionMethodWithoutGUI, MethodDescriptor.MethodID);
			MethodDescriptor.MethodGroup = group1Id;
			AssertEquals("setting method group to same value:", group1Id, MethodDescriptor.MethodGroup);
			AssertEquals("setting method group to same value:", TestingConstants.DummyActionMethodWithoutGUI, MethodDescriptor.MethodID);
			MethodDescriptor.MethodGroup = group2Id;
			AssertEquals("changing method group:", group2Id, MethodDescriptor.MethodGroup);
			AssertEquals("changing method group:", ZGuid.Empty, MethodDescriptor.MethodID);
		}

		public void TestActionSupportable()
		{
			OperationalActionSupporter actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter;
			OperationalActionContext context = new OperationalActionContext(actionSupporter, "Module Name");
			OperationalAction action = Factory.New<OperationalAction>();
			action.Context = context;
			OperationalActionFieldDescriptor fieldDescriptor = new OperationalActionFieldDescriptor(action);
			AssertEquals("ActionSupportable", context, fieldDescriptor.Context);
		}

		public void TestSerialization_WithSettings()
		{
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			MethodDescriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			MethodDescriptor.MethodID = TestingConstants.DummyActionMethodWithGUI;
			DummyOperationalActionMethodSettings settings = (DummyOperationalActionMethodSettings)MethodDescriptor.Settings;
			settings.DefaultExcuse = "Blaticus";
			settings.LockExcuse = true;
			string xml;
			using (StringWriter stream = new StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stream))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement(OperationalActionMethodDescriptorCollection.Schema.XmlElementName);
				((IXmlSerializable)MethodDescriptor).WriteXml(writer);
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
				xml = stream.ToString();
			}

			const string expectedValue = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + "<OperationalActionMethodDescriptor>" + "<MethodGroup name=\"Dummy With Methods\">7194cb51-be6a-423d-a57a-e8acc3387167</MethodGroup>" + "<Method name=\"Dummy Action Method With GUI\">" + TestingConstants.DummyActionMethodWithGuiText + "</Method>" + "<Order>0</Order>" + "<Settings>" + "<DefaultExcuse>Blaticus</DefaultExcuse>" + "<LockExcuse>Y</LockExcuse>" + "</Settings>" + "</OperationalActionMethodDescriptor>" + "";
			AssertMultilineASCIIEquals("expected xml", expectedValue.Replace("><", ">\n<"), xml.Replace("><", ">\n<"));
			OperationalActionMethodDescriptor anotherMethodDescriptor = (OperationalActionMethodDescriptor)GetNewBusinessObject();
			using (StringReader stream = new StringReader(xml))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				((IXmlSerializable)anotherMethodDescriptor).ReadXml(reader);
			}

			AssertEquals("MethodGroup", ActionMethodProviderIDs.DummyWithMethods.Guid, anotherMethodDescriptor.MethodGroup);
			AssertEquals("MethodName", TestingConstants.DummyActionMethodWithGUI, anotherMethodDescriptor.MethodID);
			DummyOperationalActionMethodSettings anotherSetting = (DummyOperationalActionMethodSettings)anotherMethodDescriptor.Settings;
			AssertEquals("DefaultExcuse", "Blaticus", anotherSetting.DefaultExcuse);
			AssertEquals("Lock", true, anotherSetting.LockExcuse);
		}

		public void TestSerialization_WithoutSettings()
		{
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			MethodDescriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			MethodDescriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			AssertEquals("Settings", null, MethodDescriptor.Settings);
			string xml;
			using (StringWriter stream = new StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stream))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement(OperationalActionMethodDescriptorCollection.Schema.XmlElementName);
				((IXmlSerializable)MethodDescriptor).WriteXml(writer);
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
				xml = stream.ToString();
			}

			const string expectedValue = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + "<OperationalActionMethodDescriptor>" + "<MethodGroup name=\"Dummy With Methods\">7194cb51-be6a-423d-a57a-e8acc3387167</MethodGroup>" + "<Method name=\"Dummy Action Method Without GUI\">" + TestingConstants.DummyActionMethodWithoutGuiText + "</Method>" + "<Order>0</Order>" + "<Settings />" + "</OperationalActionMethodDescriptor>" + "";
			AssertMultilineASCIIEquals("expected xml", expectedValue.Replace("><", ">\n<"), xml.Replace("><", ">\n<"));
			OperationalActionMethodDescriptor anotherMethodDescriptor = (OperationalActionMethodDescriptor)GetNewBusinessObject();
			using (StringReader stream = new StringReader(xml))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				((IXmlSerializable)anotherMethodDescriptor).ReadXml(reader);
			}

			AssertEquals("MethodGroup", ActionMethodProviderIDs.DummyWithMethods.Guid, anotherMethodDescriptor.MethodGroup);
			AssertEquals("MethodName", TestingConstants.DummyActionMethodWithoutGUI, anotherMethodDescriptor.MethodID);
			AssertEquals("Settings on anotherMethodDescriptor", null, anotherMethodDescriptor.Settings);
		}

		public void TestDeserialiseSettingsWithouhSettingsSupport()
		{
			// Testing what happens if Method.HasSettings becomes false after having previously supported settings and been saved with
			// a non-empty settings element. This shouldn't happen ... but just to make sure it dont blow up if it did.
			const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + "<OperationalActionMethodDescriptor>" + "<MethodGroup name=\"Dummy With Methods\">7194cb51-be6a-423d-a57a-e8acc3387167</MethodGroup>" + "<Method name=\"Dummy Action Method Without GUI\">" + TestingConstants.DummyActionMethodWithoutGuiText + "</Method>" + "<Order>0</Order>" + "<Settings>" + "<Thunkability style=\"XYZ\">1.25</Thunkability>" + "</Settings>" + "</OperationalActionMethodDescriptor>" + "";
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			using (StringReader stream = new StringReader(xml))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				((IXmlSerializable)MethodDescriptor).ReadXml(reader);
			}

			AssertEquals("Method Group.", ActionMethodProviderIDs.DummyWithMethods.Guid, MethodDescriptor.MethodGroup);
			AssertEquals("Method ID.", TestingConstants.DummyActionMethodWithoutGUI, MethodDescriptor.MethodID);
			AssertNotNull("Method should exist.", MethodDescriptor.Method);
			AssertEquals("Method should not support settings.", false, MethodDescriptor.Method.HasSettings);
			AssertNull("No Settings object.", MethodDescriptor.Settings);
		}

		public void TestDeserialiseSettingsWithAddedSettingsSupport()
		{
			// Testing what happens if Method.HasSettings becomes true after having not previously supported settings and been saved with
			// an empty settings element.
			const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + "<OperationalActionMethodDescriptor>" + "<MethodGroup name=\"Dummy With Methods\">7194cb51-be6a-423d-a57a-e8acc3387167</MethodGroup>" + "<Method name=\"Dummy Action Method With GUI\">" + TestingConstants.DummyActionMethodWithGuiText + "</Method>" + "<Order>0</Order>" + "<Settings />" + "</OperationalActionMethodDescriptor>" + "";
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			using (StringReader stream = new StringReader(xml))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				((IXmlSerializable)MethodDescriptor).ReadXml(reader);
			}

			AssertEquals("Method Group.", ActionMethodProviderIDs.DummyWithMethods.Guid, MethodDescriptor.MethodGroup);
			AssertEquals("Method ID.", TestingConstants.DummyActionMethodWithGUI, MethodDescriptor.MethodID);
			AssertNotNull("Method should exist.", MethodDescriptor.Method);
			AssertEquals("Method should support settings.", true, MethodDescriptor.Method.HasSettings);
			AssertNotNull("Should have a Settings object.", MethodDescriptor.Settings);
		}

		[ExpectNoExceptions]
		public void TestSetDefaultMethodDescriptorOrderShouldNotThrow()
		{
			Action.MethodDescriptors.RemoveAndDeleteAll();
			var methodDescriptor1 = Action.MethodDescriptors.AddNew();
			methodDescriptor1.Order = 255;
			var methodDescriptor2 = Action.MethodDescriptors.AddNew();
			AssertEquals((byte)0, methodDescriptor2.Order);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new OperationalActionMethodDescriptor(Action);
		}

		OperationalActionMethodDescriptor MethodDescriptor
		{
			get
			{
				return methodDescriptor ?? (methodDescriptor = (OperationalActionMethodDescriptor)GetNewBusinessObject());
			}
		}

		OperationalActionMethodDescriptor methodDescriptor;
		OperationalAction Action
		{
			get
			{
				if (action == null)
				{
					action = Factory.New<OperationalAction>();
					action.Context = Context;
				}

				return action;
			}
		}

		OperationalAction action;
		OperationalActionContext Context
		{
			get
			{
				return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name"));
			}
		}

		OperationalActionContext context;
		OperationalActionSupporter ActionSupporter
		{
			get
			{
				return actionSupporter ?? (actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter);
			}
		}

		OperationalActionSupporter actionSupporter;
		#endregion
	}
}
