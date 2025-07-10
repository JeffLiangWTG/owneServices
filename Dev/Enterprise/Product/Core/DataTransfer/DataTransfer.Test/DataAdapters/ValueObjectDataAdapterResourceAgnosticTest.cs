using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestTimeZone]
	[TestsSubclassesOf(typeof(ValueObjectDataAdapter<BusinessObject, IValueObject>))]
	public abstract class ValueObjectDataAdapterResourceAgnosticTest<TBusinessObject, TValueObject> : TestCaseWithFactory
			where TBusinessObject : BusinessObject
			where TValueObject : IValueObject
	{
		#region FileName

		public void TestFileName()
		{
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			adapter.FileName = "";
			AssertEquals("", adapter.FileName);

			adapter.FileName = "X.XML";
			AssertEquals("X.XML", adapter.FileName);

			adapter.FileName = "R.XML";
			AssertEquals("R.XML", adapter.FileName);
		}

		#endregion

		#region TestRootCollectionElementName / TestRootElementName

		protected abstract string ExpectedRootCollectionElementName { get; }
		protected abstract string ExpectedRootElementName { get; }

		#region EDI Interchange And Messages

		public void TestEDIInterchangeAndEDIMessage()
		{
			if (IsCreateOrUpdateFromValueObjectSupported)
			{
				var adapter = GetNewBizObjXmlDataAdapter();
				var xmlInterchange = XmlInterchange.NewPopulatedInterchange(Factory);
				PopulateXmlInterchange(xmlInterchange);
				var context = new ValueObjectImportContext(Factory, xmlInterchange, new NotificationBuffer());
				var valueObject1 = (TValueObject)PopulateValueObject(adapter.ValueObjectType, 20);
				SetUniqueReference(valueObject1, "1");
				xmlInterchange.Payload.Data = null;
				var bizObj1 = adapter.CreateOrUpdateFromValueObject(valueObject1, context);
				AssertEDIInterchangeObject(context);

				var valueObject2 = (TValueObject)PopulateValueObject(adapter.ValueObjectType, 20);
				SetUniqueReference(valueObject2, "2");
				var bizObj2 = adapter.CreateOrUpdateFromValueObject(valueObject2, context);
				AssertEDIInterchangeObject(context);
				if (context.EDIInterchange != null)
				{
					AssertEDIInterchangeDetails(context.EDIInterchange, xmlInterchange, adapter);
					AssertEquals(2, context.EDIInterchange.ContainedMessages.Count);
					context.EDIInterchange.ContainedMessages.Sort("EM_SystemCreateTimeUtc", ListSortDirection.Ascending);
					AssertAdditionalEDIMessageDetails(bizObj1, context.EDIInterchange.ContainedMessages[0]);
					AssertAdditionalEDIMessageDetails(bizObj2, context.EDIInterchange.ContainedMessages[1]);
				}
			}
			else
			{
				Assert("Test Not Required", true);
			}
		}

		[TestDate(2007, 10, 11, 12, 13, 14, 100)]
		public void TestEDIInterchangeNumWhenNumberSpecified()
		{
			if (IsCreateOrUpdateFromValueObjectSupported)
			{
				var adapter = GetNewBizObjXmlDataAdapter();
				var xmlInterchange = XmlInterchange.NewPopulatedInterchange(Factory);
				PopulateXmlInterchange(xmlInterchange);
				var context = new ValueObjectImportContext(Factory, xmlInterchange, new NotificationBuffer());
				var valueObject1 = (TValueObject)PopulateValueObject(adapter.ValueObjectType, 20);
				xmlInterchange.Payload.Data = null;
				SetUniqueReference(valueObject1, "1");
				var bizObj1 = adapter.CreateOrUpdateFromValueObject(valueObject1, context);
				AssertEDIInterchangeObject(context);

				if (context.EDIInterchange != null)
				{
					AssertEDIInterchangeNumber(context.EDIInterchange, xmlInterchange, adapter);
				}
			}
			else
			{
				Assert("Test Not Required", true);
			}
		}

		[TestDate(2007, 10, 11, 12, 13, 14, 100)]
		public void TestEDIInterchangeNumWhenNoNumberSpecified()
		{
			if (IsCreateOrUpdateFromValueObjectSupported)
			{
				var adapter = GetNewBizObjXmlDataAdapter();
				var xmlInterchange = XmlInterchange.NewPopulatedInterchange(Factory);
				PopulateXmlInterchange(xmlInterchange);
				xmlInterchange.InterchangeInfo.ReferenceKeys.Clear();

				var context = new ValueObjectImportContext(Factory, xmlInterchange, new NotificationBuffer());
				var valueObject1 = (TValueObject)PopulateValueObject(adapter.ValueObjectType, 20);
				xmlInterchange.Payload.Data = null;
				SetUniqueReference(valueObject1, "1");
				adapter.CreateOrUpdateFromValueObject(valueObject1, context);
				AssertEDIInterchangeObject(context);

				if (context.EDIInterchange != null)
				{
					AssertEDIInterchangeNumber(context.EDIInterchange, xmlInterchange, adapter);
				}
			}
			else
			{
				Assert("Test Not Required", true);
			}
		}

		protected virtual void SetUniqueReference(IValueObject value, ZString reference)
		{
		}

		protected virtual void AssertAdditionalEDIMessageDetails(TBusinessObject bizObj, EDIMessage eDIMessage)
		{
			AssertEquals(GetExpectedEDIMessageType(), eDIMessage.GetType());
			AssertEquals(bizObj, eDIMessage.EM_LinkedObject);
		}

		protected virtual void AssertEDIInterchangeObject(IValueObjectImportContext context)
		{
			AssertNull(context.EDIInterchange);
		}

		protected void AssertEDIInterchangeDetails(EDIInterchange interchange, XmlInterchange xmlInterchange, IValueObjectDataAdapter adapter)
		{
			AssertEquals(GetExpectedEDIInterchangeType(), interchange.GetType());
			AssertEquals("EI_BodyText incorrect", xmlInterchange.Payload.GetOuterXml(), interchange.EI_BodyText);
			AssertEquals("EI_ReceiveTransmit incorrect", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
			AssertEquals("EI_Status incorrect", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("EI_RetryCount incorrect", 0, interchange.EI_RetryCount);
			AssertEquals("EI_From incorrect", "Test Origin", interchange.EI_From);
			AssertEDIInterchangeNumber(interchange, xmlInterchange, adapter);
		}

		protected virtual void AssertEDIInterchangeNumber(EDIInterchange interchange, XmlInterchange xmlInterchange, IValueObjectDataAdapter adapter)
		{
			AssertEquals("EI_InterchangeNum incorrect", "Test Num", interchange.EI_InterchangeNum);
		}

		protected virtual void PopulateXmlInterchange(XmlInterchange xmlInterchange)
		{
			xmlInterchange.Payload.Data = new[] { Factory.New<TestImportingBizObj>() };
			xmlInterchange.InterchangeInfo.Source.OriginServer = "Test Origin";
			var refKey = xmlInterchange.InterchangeInfo.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = ReferenceType.OwnerReference;
			refKey.Value = "Test Owner Ref";
			refKey = xmlInterchange.InterchangeInfo.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = ReferenceType.UniqueIdentifier;
			refKey.Value = "Test Num";
			refKey = xmlInterchange.InterchangeInfo.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = ReferenceType.BatchNumber;
			refKey.Value = "Test Batch";
			AssertEquals(3, xmlInterchange.InterchangeInfo.ReferenceKeys.Count);
		}

		protected virtual Type GetExpectedEDIInterchangeType()
		{
			return typeof(NullType);
		}

		protected virtual Type GetExpectedEDIMessageType()
		{
			return typeof(NullType);
		}

		#endregion

		public void TestRootCollectionElementName()
		{
			try
			{
				AssertEquals(ExpectedRootCollectionElementName, GetNewBizObjXmlDataAdapter().RootCollectionElementName);
			}
			catch (NotSupportedException)
			{
				Assert(true);
			}
		}

		public void TestRootElementName()
		{
			AssertEquals(ExpectedRootElementName, GetNewBizObjXmlDataAdapter().RootElementName);
		}

		#endregion

		#region TestExportToValueObject

		public virtual void TestExportToValueObject_ForEmptyBizO()
		{
			TestExportToValueObject(GetEmptyBusinessObjectSampleAndExpectedOutput());
		}

		public virtual void TestExportToValueObject_ForPopulatedBizObjWithEmptyFields()
		{
			TestExportToValueObject(GetPopulatedBusinessObjectWithEmptyFieldsSampleAndExpectedOutput());
		}

		public virtual void TestExportToValueObject_ForFullyPopulatedBizO()
		{
			TestExportToValueObject(GetFullyPopulatedBusinessObjectSampleAndExpectedOutput());
		}

		public void TestExportToValueObject_ForMiscSamples()
		{
			if (IsExportToValueObjectSupported)
			{
				var miscSamples = GetMiscBusinessObjectSamplesAndExpectedOutputs();
				if (miscSamples != null && miscSamples.Length > 0)
				{
					foreach (var sample in miscSamples)
					{
						TestExportToValueObject(sample);
					}
				}
				else
				{
					Assert(true);
				}
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual void TestExportToValueObject(BusinessObjectSampleAndExpectedOutput sample)
		{
			Assert("There are conditions where we don't want this test to be run", true);

			if (IsExportToValueObjectSupported && sample.ExpectedResourceName != null)
			{
				using (StmALogValueObjectDataAdapter.SuspendExportingLogs())
				using (var expectedOutputStream = sample.GetExpectedOutput())
				using (var expectedOutputStreamReader = new StreamReader(expectedOutputStream))
				{
					var actualOutput = TransformVolatilePartsBeforeComparing(WriteBusinessObjectToXml(sample.BizObj, sample.ConstructedValueObject, sample.Description, (sample.ValidationKind & ValidationKind.Xsd) != 0));
					var expectedOutput = TransformVolatilePartsBeforeComparing(expectedOutputStreamReader.ReadToEnd());

					if (!TestingState.IsRunningOnDAT && Directory.Exists(@"c:\xmltest\"))
					{
						var xmlTestFilePath = @"c:\xmltest\" + Path.GetFileName(sample.ExpectedResourceName);
						WriteStringToFile(actualOutput, xmlTestFilePath);
					}

					if (expectedOutput.IndexOf("\t") != -1)
					{
						Fail("There are tabs in the test file (" + sample.ExpectedResourceName + "). Consider changing your xml text editor options:\r\n" +
							 "- Open the options window. Click Tools->Options.\r\n" +
							 "- Go to Text Editor->XML->Tabs\r\n" +
							 "- Click the 'Insert Spaces' radio button.");
					}

					var expectedOutputAfterReplace = expectedOutput.Replace("o;?", "");

					if (TestExpectedContentsLengthOnly)
					{
						if (expectedOutputAfterReplace.Length != actualOutput.Length)
						{
							AssertMultilineASCIIEquals("Expected contents length (" + expectedOutputAfterReplace.Length + ") doesn't match the output length (" + actualOutput.Length + "), showing text comparison for debugging purposes", expectedOutputAfterReplace, actualOutput);
						}
					}
					else
					{
						AssertMultilineASCIIEquals(
							"Expected content at '" + sample.ExpectedResourceName + "'\r\n" +
							"for sample '" + sample.Description + "'\r\n" +
							"actual output will save to directory c:\\xmltest if it exists\r\n",
							expectedOutputAfterReplace, actualOutput);
					}
				}
			}
		}

		protected virtual string TransformVolatilePartsBeforeComparing(string originalOutput)
		{
			return originalOutput;
		}

#if FAILURESAREOK && DEBUG

		bool CheckoutFileFromSourceControl(string filePath)
		{
			bool Result = false;
			//make sure you build BuildTools in release and specify your username in the EnterpriseDatabase property when creating the sourcesafe db connection
			SourceSafe ss = SourceSafe.EnterpriseDatabase;
			if (ss.FileExistsInSourceSafe(filePath))
			{
				ss.UndoCheckOutOfWinFile(filePath, false);
				ss.GetLatestVersionOfWinFile(filePath, WritableFileAction.Merge);
				ss.CheckOutWinFileWithMultipleCheckoutsAllowed(filePath);
				Result = true;
			}
			return Result;
		}

#endif

		#endregion

		#region TestExportToAndImportFromAndExportToValueObject

		public void TestExportToAndImportFromAndExportToValueObject_ForEmptyBizO()
		{
			var sample = GetEmptyBusinessObjectSampleAndExpectedOutput();
			TestExportToAndImportFromAndExportToValueObject(sample);
		}

		public void TestExportToAndImportFromAndExportToValueObject_ForPopulatedBizObjWithEmptyFields()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var sample = GetPopulatedBusinessObjectWithEmptyFieldsSampleAndExpectedOutput();
			TestExportToAndImportFromAndExportToValueObject(sample);
		}

		public void TestExportToAndImportFromAndExportToValueObject_ForFullyPopulatedBizO()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var sample = GetFullyPopulatedBusinessObjectSampleAndExpectedOutput();
			TestExportToAndImportFromAndExportToValueObject(sample);
		}

		public void TestExportToAndImportFromAndExportToValueObject_ForMiscSamples()
		{
			if (IsExportToValueObjectSupported)
			{
				var samples = GetMiscBusinessObjectSamplesAndExpectedOutputs();
				if (samples == null || samples.Length == 0)
				{
					Assert(true);
				}
				else
				{
					foreach (var sample in samples)
					{
						TestExportToAndImportFromAndExportToValueObject(sample);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual void TestExportToAndImportFromAndExportToValueObject(BusinessObjectSampleAndExpectedOutput sample)
		{
			if (IsImportFromValueObjectSupported && IsExportToValueObjectSupported)
			{
				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
				TValueObject exportedValueObject;
				if (sample.ConstructedValueObject != null)
				{
					exportedValueObject = sample.ConstructedValueObject;
				}
				else
				{
					exportedValueObject = (TValueObject)Activator.CreateInstance(adapter.ValueObjectType);
				}

				using (StmALogValueObjectDataAdapter.SuspendExportingLogs())
				{
					adapter.ExportToValueObject(sample.BizObj, exportedValueObject, new ValueObjectExportContext(new NotificationBuffer()));
					var exportedValueObjectXml = WriteBusinessObjectToXml(sample.BizObj, sample.ConstructedValueObject, sample.Description, (sample.ValidationKind & ValidationKind.Xsd) != 0);

					var bizObjToImportTo = NewBusinessObjectFromIValueObject(exportedValueObject);
					AssertImportFromThenExportToProducesSameXml(sample, bizObjToImportTo, exportedValueObject, exportedValueObjectXml, "From a new business object");
					AssertImportFromThenExportToProducesSameXml(sample, bizObjToImportTo, exportedValueObject, exportedValueObjectXml, "Updating an already populated business object to ensure that updating updates existing collection items and doesn't add them if they already exist");
				}
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual void AssertImportFromThenExportToProducesSameXml(BusinessObjectSampleAndExpectedOutput sample, TBusinessObject bizObjToImportTo, TValueObject exportedValueObject, string exportedValueObjectXml, string bizObjToImportToDescription)
		{
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			OnBeforeImportFromValueObjectForExportImportExportTest(sample.BizObj, bizObjToImportTo);
			var context = new ValueObjectImportContext(bizObjToImportTo.Factory, InterchangeThatImportsEDICode, new NotificationBuffer());
			adapter.ImportFromValueObject(bizObjToImportTo, exportedValueObject, context);
			NeedApportionment(bizObjToImportTo);
			var outputXmlAfterToAndImportFromValueObject = WriteBusinessObjectToXml(bizObjToImportTo, sample.ConstructedValueObject, sample.Description, (sample.ValidationKind & ValidationKind.Xsd) != 0);
			var message =
				"Results of ExportToValueObject() are not consistent with the results of " +
				"(ExportToValueObject then ImportFromValueObject then ExportToValueObject).\r\n\r\n" +
				bizObjToImportToDescription + "\r\n\r\n" +
				".\r\nImplement and test ExportToValueObject() first, then fix this test by implementing the ImportFromValueObject; " +
				"Sample type: " + sample.Description;
			AssertMultilineASCIIEquals(message, exportedValueObjectXml, outputXmlAfterToAndImportFromValueObject);
			ZQuery query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			var jobs = bizObjToImportTo.Factory.Load<JobHeader>(query);
			foreach (JobHeader job in jobs)
			{
				job.Dispose();
			}
		}

		protected virtual void NeedApportionment(BusinessObject bizObjToImportTo)
		{
		}

		XmlInterchange InterchangeThatImportsEDICode
		{
			get
			{
				if (fInterchangeThatImportsEDICode == null)
				{
					fInterchangeThatImportsEDICode = new XmlInterchange();
					fInterchangeThatImportsEDICode.ImportEDICode = true;
				}
				return fInterchangeThatImportsEDICode;
			}
		}
		XmlInterchange fInterchangeThatImportsEDICode;

		protected virtual void OnBeforeImportFromValueObjectForExportImportExportTest(BusinessObject bizObjOriginallyExportedFrom, BusinessObject bizObjToImportTo)
		{
		}

		#endregion

		#region TestExportToAndImportFromXmlInterchange

		public void TestOnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible()
		{
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			Assert(!adapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible);
		}

		public void TestOnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked()
		{
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			Assert(!adapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked);
		}

		public void TestExportToAndImportFromXmlInterchange()
		{
			if (IsImportFromValueObjectSupported &&
				IsExportToValueObjectSupported &&
				IsExportToCollectionSupported)
			{
				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
				var notify = new NotificationBuffer();
				foreach (var sample in GetSampleBusinessObjects())
				{
					var interchange = adapter.ToXmlInterchange(new TBusinessObject[] { sample.BizObj }, new ValueObjectExportContext(notify)) as XmlInterchange;
					// BusinessObject BizObjToImportTo = NewBusinessObject(); NOT REQUIRED
					var factory = AllowDifferentImportContextFactory ? new BusinessObjectFactory() : Factory;
					var collection = new TestBusinessObjectCollection(factory);
					var context = new ValueObjectImportContext(factory, interchange, notify);
					adapter.FromXmlInterchange(collection, context);

					if ((sample.ValidationKind & ValidationKind.DontExpectToImportAnythingFromInterchange) != 0)
					{
						AssertEquals(sample.Description + ": Didn't expect to import anything", 0, collection.Count);
					}
					else
					{
						AssertEquals(sample.Description + ": Should have imported something", 1, collection.Count);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool AllowDifferentImportContextFactory
		{
			get { return false; }
		}

		class TestBusinessObjectCollection : BusinessObjectCollection<BusinessObject>
		{
			public TestBusinessObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		#endregion

		#region TestImportFromValueObject_WithNewlyCreatedValueObject

		[ExpectNoExceptions]
		public void TestImportFromValueObject_WithNewlyCreatedValueObject()
		{
			for (var i = 0; i < 20; i++)
			{
				TestImportFromValueObject_WithNewlyCreatedValueObject(i);
			}
		}

		[ExpectNoExceptions]
		void TestImportFromValueObject_WithNewlyCreatedValueObject(int fieldPopulateDepth)
		{
			if (IsImportFromValueObjectSupported)
			{
				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
				var emptyValueObject = (TValueObject)PopulateValueObject(adapter.ValueObjectType, fieldPopulateDepth);
				var newBO = NewBusinessObjectFromIValueObject(emptyValueObject);
				var notify = new NotificationBuffer();
				var context = new ValueObjectImportContext(newBO.Factory, notify);
				adapter.ImportFromValueObject(newBO, emptyValueObject, context);
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual IValueObject PopulateValueObject(Type valueType, int fieldPopulateDepth)
		{
			var result = (IValueObject)Activator.CreateInstance(valueType);
			foreach (var field in valueType.GetFields())
			{
				var fieldType = field.FieldType;
				var elements = (XmlElementAttribute[])field.GetCustomAttributes(typeof(XmlElementAttribute), true);
				if (elements.Length > 0)
				{
					fieldType = elements[0].Type;
				}
				if (typeof(IValueObject).IsAssignableFrom(fieldType))
				{
					if (fieldPopulateDepth > 0)
					{
						var fieldValue = PopulateValueObject(fieldType, fieldPopulateDepth - 1);
						field.SetValue(result, fieldValue);
					}
				}
				if (typeof(IList).IsAssignableFrom(fieldType) && fieldType.GetConstructor(Array.Empty<Type>()) != null)
				{
					var newList = (IList)Activator.CreateInstance(fieldType);
					var indexerType = DetermineIndexerType(fieldType);
					if (indexerType != null)
					{
						var listElementValue = PopulateValueObject(indexerType, fieldPopulateDepth - 1);
						newList.Add(listElementValue);
						field.SetValue(result, newList);
					}
				}
			}
			return result;
		}

		Type DetermineIndexerType(Type listType)
		{
			Type result = null;
			foreach (var property in listType.GetProperties())
			{
				if (property.Name == "Item")
				{
					result = property.PropertyType;
					break;
				}
			}
			return result;
		}

		#endregion

		#region TestTestCoverageOfValueObject

		public virtual void TestTestCoverageOfValueObject()
		{
			if (IsExportToValueObjectSupported)
			{
				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
				var coverage = new ValueObjectTestCoverageHelper(adapter.ValueObjectType, XmlNodesToExcludeFromCoverageTestBaseAndDerived);
				foreach (var sample in GetSampleBusinessObjects())
				{
					var valueObject = adapter.ExportToValueObject(sample.BizObj, new ValueObjectExportContext(new NotificationBuffer()));
					var v = new XmlValueObjectSerializer(valueObject.GetType());
					StringWriter x;
					x = new StringWriter();
					v.Serialize(x, valueObject);
					var s = x.GetStringBuilder().ToString();
					coverage.NotifyCovered(valueObject);
				}
				if (coverage.UncoveredPaths.Length > 0)
				{
					Fail("The following xml nodes were never populated in the xml-based base unit tests,\r\n" +
						"which may indicate the nodes are never actually used in code at all.\r\n" +
						"Override XmlNodesToExcludeFromCoverageTest and specify a partial or full xml node path\r\n" +
						"to exclude from this test.\r\n" +
						"ONLY EXCLUDE IF THEY ARE COVERED BY ANOTHER XML DATA ADAPTER and give a reason in a comment.\r\n" +
						"To ensure you're testing the new nodes, add xml to the fully-populated test xml file\r\n" +
						"(" + GetFullyPopulatedBusinessObjectSampleAndExpectedOutput().ExpectedResourceName + ")\r\n" +
						") and add code in GetFullyPopulatedBizObjSample() to add the data to the test business object:" +
						"\r\n\r\n" + coverage.UncoveredPathsAsString +
						"\r\n");
					throw new ApplicationException("The following xml nodes were never populated in this unit test, which may indicate the nodes are never actually used at all and need to be used. If :\r\n\r\n" + coverage.UncoveredPathsAsString + "\r\n");
				}
			}
			Assert(true);
		}

		string[] XmlNodesToExcludeFromCoverageTestBaseAndDerived
		{
			get
			{
				var baseShitThatHenryWillFix = new List<string>
					{   "Routing/Items/DepartureReference",
						"Routings/Items/DepartureReference",
						"Routings/Item/DepartureReference",
						"Routing/Item/DepartureReference",

						"ConsolDetail/Item/DepartureReference",
						"ConsolDetails/Item/DepartureReference",
						"ConsolDetail/Items/DepartureReference",
						"ConsolDetails/Items/DepartureReference",

						"ConsolDetail/PlannedLeg/Item/DepartureReference",
						"ConsolDetails/PlannedLeg/Item/DepartureReference",
						"ConsolDetail/PlannedLegs/Item/DepartureReference",
						"ConsolDetails/PlannedLegs/Item/DepartureReference",

						"ConsolDetail/PlannedLeg/Items/DepartureReference",
						"ConsolDetails/PlannedLeg/Items/DepartureReference",
						"ConsolDetail/PlannedLegs/Items/DepartureReference",
						"ConsolDetails/PlannedLegs/Items/DepartureReference",

						"ShipmentDetails/TransportPlan/Item/DepartureReference",
						"DepartureReference",

						"Sailing/DepartureReference",

						"SailingInfo/Item/DepartureReference",

						"ShipmentBookingDetail/Item/DepartureReference",

						"JournalLines/SubAccount/Code", // The 3 sub account elements are obsolate. Please use new SubAccounts Collection element.
						"JournalLines/SubAccount/Type/Code",
						"JournalLines/SubAccount/Type/Description",
					};
				baseShitThatHenryWillFix.AddRange(XmlNodesToExcludeFromCoverageTest);
				return baseShitThatHenryWillFix.ToArray();
			}
			// WI00038019 - Sailig Data Feed - ask Henry about this.  Henry will go through all the sample XML files and add this node where needed, then he will yell at (?)Ben for such a pain-in-the-arse unit test, then he will buy Daniel a beer.
		}

		protected virtual string[] XmlNodesToExcludeFromCoverageTest
		{
			get { return Array.Empty<string>(); }
		}

		#endregion

		#region Importing of Long Strings

		[ExpectNoExceptions]
		public void TestImportOfLongStrings()
		{
			if (IsImportFromValueObjectSupported)
			{
				IValueObjectDataAdapter dataAdapter = GetNewBizObjXmlDataAdapter();
				var navigator = new ValueObjectPropertyNavigator(dataAdapter.ValueObjectType);

				var value = (TValueObject)Activator.CreateInstance(dataAdapter.ValueObjectType);
				PopulateValueObjectWithLongStrings(value, navigator);

				var bizObjToImportTo = NewBusinessObjectFromIValueObject(value);
				var context = new ValueObjectImportContext(bizObjToImportTo.Factory, new NotificationBuffer());
				dataAdapter.ImportFromValueObject(bizObjToImportTo, value, context);
			}
		}

		protected virtual void PopulateValueObjectWithLongStrings(IValueObject value, ValueObjectPropertyNavigator navigator)
		{
			foreach (var childProperty in navigator.GetNextPropertiesInPath())
			{
				var childValue = childProperty.GetValue(value);
				if (childValue is IList && !(childValue is Array))
				{
					var childValueList = childValue as IList;
					if (value.GetType() != childProperty.PropertyType)
					{
						if (childValueList.Count == 0)
						{
							var addNewMethod = childValue.GetType().GetMethod("AddNew", Array.Empty<Type>());
							addNewMethod.Invoke(childValueList, Array.Empty<object>());
						}
						PopulateValueObjectWithLongStrings((IValueObject)childValueList[0], childProperty);
					}
				}
				else if (childValue is IValueObject)
				{
					if (childValue is IValueObjectShouldNotPopulateLongString)
					{
						return;
					}

					PopulateValueObjectWithLongStrings((IValueObject)childValue, childProperty);
				}
				else if (typeof(ZString).IsAssignableFrom(childProperty.Property.PropertyType))
				{
					if (!StringFieldsToIgnore.Contains(childProperty.ElementName))
					{
						childProperty.Property.SetValue(value, LongString);
					}
				}
			}
		}
		protected ZString LongString = "voteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclintyvoteclinty";

		protected virtual string[] StringFieldsToIgnore
		{
			get { return Array.Empty<string>(); }
		}
		#endregion

		#region Tests for coverage of strongly typed new'd method

		[ExpectNoExceptions]
		public void TestStronglyTypedCreateOrUpdateBusinessObject_ForCodeCoverage()
		{
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			if (HasStronglyTypedMethod(adapter, "CreateOrUpdateFromValueObject"))
			{
				foreach (var sample in GetSampleBusinessObjects())
				{
					IValueObject constructedValueObject = sample.ConstructedValueObject ?? (IValueObject)Activator.CreateInstance(adapter.ValueObjectType);
					var notify = new NotificationBuffer();
					try
					{
						InvokeStronglyTypedMethod(adapter, "CreateOrUpdateFromValueObject", new object[] { Factory, null, constructedValueObject, notify });
					}
					catch (NotSupportedException)
					{
					}
					catch (TargetInvocationException ex)
					{
						if (!(ex.InnerException is NotSupportedException))
						{
							throw;
						}
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		[ExpectNoExceptions]
		public void TestStronglyTypedImportFromValueObject_ForCodeCoverage()
		{
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			if (HasStronglyTypedMethod(adapter, "ImportFromValueObject"))
			{
				var sample = GetSampleBusinessObjects()[0];
				var constructedValueObject = sample.ConstructedValueObject ?? Activator.CreateInstance(adapter.ValueObjectType);
				var constructedBusinessObject = NewBusinessObject();

				var notify = new NotificationBuffer();
				InvokeStronglyTypedMethod(adapter, "ImportFromValueObject", new object[] { constructedBusinessObject, null, constructedValueObject, notify });
			}
		}

		[ExpectNoExceptions]
		public void TestStronglyTypedExportToValueObject_ForCodeCoverage()
		{
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			if (HasStronglyTypedMethod(adapter, "ExportToValueObject"))
			{
				var sample = GetSampleBusinessObjects()[0];
				IValueObject constructedValueObject = sample.ConstructedValueObject ?? (IValueObject)Activator.CreateInstance(adapter.ValueObjectType);
				BusinessObject constructedBusinessObject = NewBusinessObject();
				var notify = new NotificationBuffer();

				// try one or the other of the overloads
				try
				{
					InvokeStronglyTypedMethod(adapter, "ExportToValueObject", new object[] { constructedBusinessObject, notify });
				}
				catch (MethodAccessException)
				{
					InvokeStronglyTypedMethod(adapter, "ExportToValueObject", new object[] { constructedBusinessObject, constructedValueObject, notify });
				}
			}
		}

		bool HasStronglyTypedMethod(IValueObjectDataAdapter adapter, string methodName)
		{
			return FindStronglyTypedMethod(adapter, methodName, null) != null;
		}

		object InvokeStronglyTypedMethod(IValueObjectDataAdapter adapter, string methodName, object[] parameters)
		{
			var method = FindStronglyTypedMethod(adapter, methodName, parameters);
			if (method.GetParameters().Length != parameters.Length)
			{
				throw new MethodAccessException();
			}
			return method.Invoke(adapter, parameters);
		}

		MethodInfo FindStronglyTypedMethod(IValueObjectDataAdapter adapter, string methodName, object[] parameters)
		{
			var currentAdapterType = adapter.GetType();
			while (currentAdapterType != null)
			{
				foreach (var method in adapter.GetType().GetMethods())
				{
					if (method.Name == methodName &&
						method.DeclaringType == currentAdapterType &&
						AreParametersOfCorrectType(method, parameters))
					{
						return method;
					}
				}
				currentAdapterType = currentAdapterType.BaseType;
			}
			return null;
		}

		bool AreParametersOfCorrectType(MethodInfo method, object[] parameters)
		{
			var result = (parameters != null) && (method.GetParameters().Length == parameters.Length);
			if (result)
			{
				for (var i = 0; i < parameters.Length; i++)
				{
					if (!method.GetParameters()[i].ParameterType.IsInstanceOfType(parameters[i]))
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		#endregion

		#region Test NotSupportedException thrown if member not supported

		public void TestImportFromValueObjectNotSupportedException()
		{
			if (IsImportFromValueObjectSupported)
			{
				Assert(true);
			}
			else
			{
				try
				{
					var adapter = GetNewBizObjXmlDataAdapter();
					var dummyValueObject = adapter.ExportToValueObject(GetEmptyBusinessObjectSampleAndExpectedOutput().BizObj, new ValueObjectExportContext(new NotificationBuffer()));
					var newBizO = NewBusinessObject();
					var context = new ValueObjectImportContext(newBizO.Factory, new NotificationBuffer());
					adapter.ImportFromValueObject(NewBusinessObject(), dummyValueObject, context);
					Fail("Expected an exception");
				}
				catch (NotSupportedException)
				{
					Assert(true);
				}
			}
		}

		public void TestExportToValueObjectNotSupportedException()
		{
			AssertExportFromValueObjectNotSupportedException();
		}

		protected virtual void AssertExportFromValueObjectNotSupportedException()
		{
			if (IsExportToValueObjectSupported)
			{
				Assert(true);
			}
			else
			{
				try
				{
					var adapter = GetNewBizObjXmlDataAdapter();
					adapter.ExportToValueObject(NewBusinessObject(), new ValueObjectExportContext(new NotificationBuffer()));
					Fail("Expected an exception");
				}
				catch (NotSupportedException)
				{
					Assert(true);
				}
			}
		}

		#endregion

		#region Getting Sample Business Objects

		protected BusinessObjectSampleAndExpectedOutput[] GetSampleBusinessObjects()
		{
			var result = new List<BusinessObjectSampleAndExpectedOutput>
			{
				GetEmptyBusinessObjectSampleAndExpectedOutput(),
				GetPopulatedBusinessObjectWithEmptyFieldsSampleAndExpectedOutput(),
				GetFullyPopulatedBusinessObjectSampleAndExpectedOutput()
			};
			result.AddRange(GetMiscBusinessObjectSamplesAndExpectedOutputs());
			return result.ToArray();
		}

		protected abstract BusinessObjectSampleAndExpectedOutput GetEmptyBusinessObjectSampleAndExpectedOutput();
		protected abstract BusinessObjectSampleAndExpectedOutput GetPopulatedBusinessObjectWithEmptyFieldsSampleAndExpectedOutput();
		protected abstract BusinessObjectSampleAndExpectedOutput GetFullyPopulatedBusinessObjectSampleAndExpectedOutput();

		protected virtual BusinessObjectSampleAndExpectedOutput[] GetMiscBusinessObjectSamplesAndExpectedOutputs() =>
			Array.Empty<BusinessObjectSampleAndExpectedOutput>();

		#endregion

		#region TestCreateOrUpdateFromValueObject_ShouldCreateOrUpdateBusinessObject

		public virtual void TestCreateOrUpdateFromValueObject_ShouldCreateOrUpdateBusinessObject()
		{
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var value = new TestValueObject();

			var adapter = new TestValueObjectDataAdapter();
			var importingBO = adapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull(importingBO);
		}

		#endregion

		protected virtual Type GetDataAdapterType()
		{
			return GetNewBizObjXmlDataAdapter().GetType();
		}

		protected abstract ValueObjectDataAdapter<TBusinessObject, TValueObject> GetNewBizObjXmlDataAdapter();

		protected virtual TBusinessObject NewBusinessObjectFromIValueObject(IValueObject value)
		{
			return NewBusinessObject();
		}

		protected virtual TBusinessObject NewBusinessObject()
		{
			return (TBusinessObject)Factory.New(GetNewBizObjXmlDataAdapter().BusinessObjectType);
		}

		protected virtual bool IsCreateOrUpdateFromValueObjectSupported
		{
			get { return IsImportFromValueObjectSupported; }
		}

		protected virtual bool IsExportToValueObjectSupported
		{
			get { return true; }
		}

		protected virtual bool IsImportFromValueObjectSupported
		{
			get { return true; }
		}

		protected virtual bool IsExportToCollectionSupported
		{
			get { return true; }
		}

		protected virtual bool TestExpectedContentsLengthOnly
		{
			get { return false; }
		}

		#region ValidationKind and BusinessObjectAndExpectedOutputFileName

		[Flags]
		protected enum ValidationKind
		{
			None = 0,
			Xsd = 1,
			DontExpectToImportAnythingFromInterchange = 2,
			FactorySave = 4,
		}

		protected abstract class BusinessObjectSampleAndExpectedOutput
		{
			protected BusinessObjectSampleAndExpectedOutput(TBusinessObject bizObj, TValueObject constructedValueObject, ValidationKind validationKind, string description)
			{
				BizObj = bizObj;
				ConstructedValueObject = constructedValueObject;
				ValidationKind = validationKind;
				Description = description;
			}

			protected BusinessObjectSampleAndExpectedOutput(TBusinessObject bizObj, ValidationKind validationKind, string description)
				: this(bizObj, default, validationKind, description)
			{
			}

			public abstract string ExpectedResourceName { get; }
			public abstract Stream GetExpectedOutput();
			public TBusinessObject BizObj { get; }
			public TValueObject ConstructedValueObject { get; }
			public ValidationKind ValidationKind { get; }
			public string Description { get; }
		}

		protected class BusinessObjectAndExpectedOutputFileName : BusinessObjectSampleAndExpectedOutput
		{
			public BusinessObjectAndExpectedOutputFileName(TBusinessObject bizObj, string expectedOutputFileName, ValidationKind validationKind, string description)
				: base(bizObj, validationKind, description)
			{
				this.expectedOutputFileName = expectedOutputFileName;
			}

			public BusinessObjectAndExpectedOutputFileName(TBusinessObject bizObj, TValueObject constructedValueObject, string expectedOutputFileName, ValidationKind validationKind, string description)
				: base(bizObj, constructedValueObject, validationKind, description)
			{
				this.expectedOutputFileName = expectedOutputFileName;
			}

			public override string ExpectedResourceName => expectedOutputFileName;

			public override Stream GetExpectedOutput() => File.OpenRead(expectedOutputFileName);

			readonly string expectedOutputFileName;
		}

		protected class BusinessObjectAndExpectedOutputResource : BusinessObjectSampleAndExpectedOutput
		{
			public BusinessObjectAndExpectedOutputResource(TBusinessObject bizObj, string manifestResourceName, ValidationKind validationKind, string description)
				: base(bizObj, validationKind, description)
			{
				this.assembly = Assembly.GetCallingAssembly();
				this.manifestResourceName = manifestResourceName;
			}

			public BusinessObjectAndExpectedOutputResource(TBusinessObject bizObj, TValueObject constructedValueObject, string manifestResourceName, ValidationKind validationKind, string description)
				: base(bizObj, constructedValueObject, validationKind, description)
			{
				this.assembly = Assembly.GetCallingAssembly();
				this.manifestResourceName = manifestResourceName;
			}

			public override Stream GetExpectedOutput()
			{
				using (var retriever = new EmbeddedResourceRetriever(assembly))
				{
					return retriever.GetStream(manifestResourceName);
				}
			}

			public override string ExpectedResourceName => manifestResourceName;

			readonly Assembly assembly;

			readonly string manifestResourceName;
		}

		#endregion

		#region Helper Methods

		public class UTF8StringWriter : StringWriter
		{
			public override Encoding Encoding
			{
				get { return Encoding.UTF8; }
			}
		}

		protected virtual StringWriter GetNewStringWriter()
		{
			return new UTF8StringWriter();
		}

		protected string WriteBusinessObjectToXml(TBusinessObject bizObj, TValueObject constructedValueObject, string description, bool expectXsdValidationToPass)
		{
			if (constructedValueObject == null)
			{
				constructedValueObject = (TValueObject)Activator.CreateInstance(GetNewBizObjXmlDataAdapter().ValueObjectType);
			}

			string result;
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();

			var writer = GetNewStringWriter();
			var xmlWriter = new XmlTextWriter(writer);
			xmlWriter.Formatting = Formatting.Indented;
			var serializer = new XmlValueObjectSerializer(constructedValueObject.GetType());
			serializer.WriteToXml(xmlWriter, adapter, bizObj, constructedValueObject, new ValueObjectExportContext(new NotificationBuffer()));
			xmlWriter.Flush();
			writer.Flush();
			result = writer.GetStringBuilder().ToString();

			if (expectXsdValidationToPass)
			{
				var notify = new NotificationBuffer();
				new XmlValidator(adapter.Schema).Validate(result, notify);
				if (notify.HasErrors)
				{
					Fail(description + "\n\n" + notify.AsString + "\n\nFor xml:\n\n" + result);
				}
			}
			return result;
		}

		void WriteStringToFile(string content, string fileName)
		{
			using (var writer = File.CreateText(fileName))
			{
				writer.Write(content);
				writer.Flush();
				writer.Close();
			}
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			if (!string.IsNullOrEmpty(TestingCountry))
			{
				StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			}
		}
		protected string StoredCountry;

		protected override void TearDown()
		{
			base.TearDown();
			if (!string.IsNullOrEmpty(StoredCountry) && StoredCountry != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			}
		}

		protected virtual string TestingCountry => null;
	}
}
