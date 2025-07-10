using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	using System.Threading;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.Schema;
	using Moq;
	using Moq.Protected;
	using NUnit.Framework;

	sealed class BusinessObjectTest : TestCaseWithDummyForValidationTesting
	{
		public void TestResumeValidationUnderflow()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			Assert(!bizo.IsValidationSuspended);
			bizo.ResumeValidation();
			Assert(!bizo.IsValidationSuspended);
			bizo.SuspendValidation();
			Assert(bizo.IsValidationSuspended);
			bizo.ResumeValidation();
			Assert(!bizo.IsValidationSuspended);
		}

		[ExpectNoExceptions]
		public void TestHasChangesReentrancyCycle()
		{
			var dummyBizoCollection = new DummyBizoCollectionForTest(Factory);
			var bizo1 = dummyBizoCollection.AddNew();
			var bizo2 = Factory.New<DummyBusinessObject>();
			bizo1.RegisterEditableChildObject(bizo2);
			bizo2.RegisterEditableChildObject(dummyBizoCollection);
			bizo1.HasChanges = false;
			bizo1.HasChanges = true;
			bizo2.HasChanges = false;
			bizo2.HasChanges = true;
			AssertEquals(@"We are a DummyBusinessObject.
Our direct children are: DummyBusinessObject.
The cycle is DummyBusinessObject -> DummyBusinessObject -> DummyBizoCollectionForTest -> (the start).", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestBlobFieldsNeedLoading()
		{
			var dummyBizoCollection = new DummyBizoCollectionForTest(Factory);
			var dummyBizo = dummyBizoCollection.AddNew();
			dummyBizo.Z0_VarBinaryMax = new byte[] { 0, 0 };
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var dummyBizoReload = newFactory.Load<DummyBusinessObject>(dummyBizo.PK);
			Assert(dummyBizoReload.IsInDatabase);
			dummyBizoReload.Delete();
			newFactory.Save();

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		class DummyBizoCollectionForTest : DummyBusinessObjectCollection
		{
			public DummyBizoCollectionForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
			{
				_ = (child as DummyBusinessObject).Z0_VarBinaryMax;
			}
		}

		public void TestIsForcePublishForNonPersistentObject()
		{
			var dummy = Factory.New<DummyUnsavablePersistentObjectThatRequiresRefresh>();
			AssertEquals("NonPersistentBusinessObject's IsForcePublishForNonPersistentObject should be true", true, dummy.IsForcePublishForNonPersistentBusinessObject);
		}

		public void TestUpdateBitField()
		{
			var bizO = Factory.New<DummyBusinessObject>();
			Factory.Save();
			bizO.Z0_BitFalse = true;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			AssertEquals(true, factory2.Load<DummyBusinessObject>(bizO.PK).Z0_BitFalse);
		}

		public void TestDecimalRounding()
		{
			var bizO = Factory.New<DummyBusinessObject>();
			var value = 12.345678m;

			bizO.Z0_Decimal = value;
			AssertEquals("Should be rounded to zero decimal places", 12m, bizO.Z0_Decimal);

			bizO.Z0_AnotherDecimal = value;
			AssertEquals("Should be rounded to three decimal places", 12.346m, bizO.Z0_AnotherDecimal);

			Factory.Save();
			AssertEquals("Precondition", false, bizO.HasChanges);
			bizO.Z0_AnotherDecimal = value;
			AssertEquals("Assigning the same rounded value should not cause change detection", false, bizO.HasChanges);

			value = 12.2m;
			bizO.Z0_AnotherDecimal = value;
			AssertEquals("Should not be rounded", 12.2m, bizO.Z0_AnotherDecimal);
		}

		class DummyBusinessObjectWithInvalidPKSchemaColumn : DummyBusinessObject
		{
			public DummyBusinessObjectWithInvalidPKSchemaColumn(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override SchemaGuidColumn PKSchemaColumn
			{
				get { return new SchemaPKColumn(DummyBizoSchema.Instance, "WRONG"); }
			}
		}

		public void TestPKSchemaColumnNotInTable()
		{
			try
			{
				var bizO = Factory.New<DummyBusinessObjectWithInvalidPKSchemaColumn>();
			}
			catch (ApplicationException e)
			{
				AssertContains("Can't create a DummyBusinessObjectWithInvalidPKSchemaColumn as its constructor threw an exception.", e.ToString());
				AssertContains(@"Row unexpectely lacks PKSchemaColumn.
PKSchemaColumn.Name: WRONG, Row.Table.TableName: DummyBizo, this.GetType().ToString(): CargoWise.EntityFramework.Testing.BusinessObjectTest+DummyBusinessObjectWithInvalidPKSchemaColumn. Columns information:
", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestXMLLazyLoad()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			var xmlData = string.Format("<Root>0123456789 ღ 0123456789 {0}ღ 0123456789 </Root>", "".PadRight(1024, '1'));
			dummyBizO.Z0_Xml = xmlData;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummyBizOInDiffFactory = factory.Load<DummyBusinessObject>(dummyBizO.PK);
			var row = ((IBusinessObjectInternals)dummyBizOInDiffFactory).Row;
			AssertEquals("Row value", "<?placeholder LazyLoading=\"Yes\"?>", row[DummyBizoSchema.Z0_Xml.Name]);
			AssertEquals("BizO value", xmlData, dummyBizOInDiffFactory.Z0_Xml);
		}

		public void TestXMLLazyLoad_WithColumnXmlSchema()
		{
			using (var command = TestConnection.Command(string.Format(@"
ALTER TABLE {0}
DROP COLUMN {1}",
					   DummyBizoSchema.Constants.TableName,
					   DummyBizoSchema.Constants.Z0_ComputedXml)))
			{
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(@"
CREATE XML SCHEMA COLLECTION [xmlZ0_XmlForTest] AS
'<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
	<xs:element name=""Root"" type=""xs:string"" />
</xs:schema>'"))
			{
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(string.Format(@"
ALTER TABLE {0}
ALTER COLUMN {1} XML (CONTENT [xmlZ0_XmlForTest]);",
	 DummyBizoSchema.Constants.TableName,
	 DummyBizoSchema.Constants.Z0_Xml)))
			{
				command.ExecuteNonQuery();
			}

			var dummyBizO = Factory.New<DummyBusinessObject>();
			var xmlData = string.Format("<Root>0123456789 ღ 0123456789 {0}ღ 0123456789 </Root>", "".PadRight(1024, '1'));
			dummyBizO.Z0_Xml = xmlData;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummyBizOInDiffFactory = factory.Load<DummyBusinessObject>(dummyBizO.PK);
			var row = ((IBusinessObjectInternals)dummyBizOInDiffFactory).Row;
			AssertEquals("Row value", "<?placeholder LazyLoading=\"Yes\"?>", row[DummyBizoSchema.Z0_Xml.Name]);
			AssertEquals("BizO value", xmlData, dummyBizOInDiffFactory.Z0_Xml);
		}

		public void TestReloadSafe()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.ReloadSafe();
			AssertNotEquals("Argh", ErrorReporter.LastKeyReported);
		}

		public void TestReloadUnsafe()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Reload();
			AssertEquals("Argh", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestPropertyUsedDelegateStopsDataUpdate()
		{
			var mock = Factory.NewMoq<DummyBusinessObject>();
			mock.Setup(m => m.IsPropertySupported(It.IsAny<ZPropertyInfo>())).Returns(false);
			mock.Object.Z0_AnotherDate = ZDate.BrettsBirthday;
			Assert(mock.Object.Z0_AnotherDate.IsEmpty);
		}

		public void TestIsNullOrDeleted()
		{
			AssertEquals("null", true, BusinessObject.IsNullOrDeleted(null));
			AssertEquals("alive", false, BusinessObject.IsNullOrDeleted(Dummy));
			Dummy.Delete();
			AssertEquals("deleted", true, BusinessObject.IsNullOrDeleted(Dummy));
		}

		public void TestChildrenOrderDoesNotFlipFlop()
		{
			var bizO1 = Factory.New<DummyBusinessObject>();
			IBusiness b = bizO1;
			var bizO2 = Factory.New<DummyBusinessObject>();
			var bizO3 = Factory.New<DummyBusinessObject>();
			bizO1.RegisterEditableChildObject(bizO2);
			AssertEquals(bizO2, b.Children[0]);

			bizO1.RegisterEditableChildObject(bizO3);
			AssertEquals(bizO2, b.Children[0]);
			AssertEquals(bizO3, b.Children[1]);
		}

		public void TestHasChangesDoesNotAffectLightValidation()
		{
			Dummy.MarkLightValidationAsValidForTesting();
			Dummy.HasChanges = false;
			AssertEquals(true, Dummy.LightValidationIsValid);

			Dummy.HasChanges = true;
			AssertEquals("HasChanges does NOT affect the state of IsValid", true, Dummy.LightValidationIsValid);

			Dummy.Z0_AnotherDecimalInfo.AddError("Crap");
			AssertEquals("Adding an error DOES NOT affect the state of IsValid", true, Dummy.LightValidationIsValid);

			Dummy.MarkAsNeedingValidation();
			AssertEquals("This does affect the state of IsValid", false, Dummy.LightValidationIsValid);
		}

		public void TestTablePrefix()
		{
			AssertEquals("TablePrefix", ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(Dummy.TableName), Dummy.TablePrefix);
		}

		public void TestSetNonPersistentPropertyValue()
		{
			Dummy.HasChanges = false;
			Dummy.NonPersistentProperty = "Hello";
			AssertEquals("Dummy has changes", true, Dummy.HasChanges);
			AssertEquals("Dummy has changes", "Hello", Dummy.NonPersistentProperty);
			AssertEquals("Dummy has changes", "Hello", Dummy.NonPersistentPropertyInfo.Value);
		}

		public void TestSubscribeToDataRefreshOnInstantiation()
		{
			AssertEquals(true, ((IBusinessObjectInternals)BizO).SubscribeToDataRefreshOnInstantiation);
		}

		public void TestPushValueIntoRow()
		{
			DummyWithOverridenDescription pushTestBizO = Factory.New<DummyWithOverridenDescription>();

			pushTestBizO.Z0_Description = "byebye";
			Factory.Save();
			DummyBusinessObject pushTestBizO2 = Factory.Load<DummyBusinessObject>(pushTestBizO.PK);
			AssertEquals("Value should be saved", "byebye", pushTestBizO2.Z0_Description);

			pushTestBizO.Z0_Description = "hello";
			Factory.Save();
			pushTestBizO2 = Factory.Load<DummyBusinessObject>(pushTestBizO.PK);
			AssertEquals("Value should not be saved", "byebye", pushTestBizO2.Z0_Description);

			pushTestBizO.Z0_Description = "hello";
			pushTestBizO.Z0_DescriptionInfo.PushValueIntoRow();
			Factory.Save();
			pushTestBizO2 = Factory.Load<DummyBusinessObject>(pushTestBizO.PK);
			AssertEquals("Value should be saved", "hello", pushTestBizO2.Z0_Description);
		}

		public void TestHasChangesInAuditDetails()
		{
			Assert(!Dummy.HasChangesInAuditDetails);
		}

		public void TestNoExceptionWhenComparingNullBizOAndNonBizO()
		{
			DummyBusinessObject bizO = null;
			AssertNoExceptionThrown(() => { if (bizO == Guid.Empty) { } });
		}

		public void TestCheckMaximumLengthGivesSpecificKey_ReallyLongMessage()
		{
			ErrorReporter.SuppressReportingOfErrors = false;
			var dummyObj = new BusinessObjectFactory().NewWithValidTestData<DummyForCheckingMaximumValue>();

			var noteText = string.Empty;
			AssertExceptionThrown(typeof(MaxLengthExceededException), () =>
			{
				for (var i = 0; i < 9000; i++)
				{
					noteText += "we're making a long string to check it gets truncated.";
				}

				dummyObj.NoteText = noteText;
			});

			var startIndex = ErrorReporter.LastMessageReported.IndexOf("New value:");
			var exceptionMessage = ErrorReporter.LastMessageReported.Substring(
				startIndex,
				ErrorReporter.LastMessageReported.IndexOf("Old value") - startIndex);

			var removedText = "\r\n... content removed for the sake of brevity ...\r\n";
			var key = ErrorReporter.LastKeyReported;

			Assert(key.Contains("at CargoWise.EntityFramework.Testing.BusinessObjectTest") && key.Contains("TestCheckMaximumLengthGivesSpecificKey_ReallyLongMessage"));
			Assert(exceptionMessage.Contains(removedText));
			AssertEquals(exceptionMessage.Trim().Length, 5000 + removedText.Length + "New value:..".Length);

			ErrorReporter.Clear();
		}

		public void TestThrowAndDontReportErrorForMaxLengthExceededInsideUsing()
		{
			var dummyObj = new BusinessObjectFactory().NewWithValidTestData<DummyForCheckingMaximumValue>();

			using (BusinessObject.ThrowWhenMaxPropertyLengthExceeded())
			{
				AssertExceptionThrown<MaxLengthExceededException>(() => dummyObj.Z0_Code = "111111111111111111111");
			}
		}

		public void TestCheckMaximumLengthGivesSpecificKey_ShortMessage()
		{
			ErrorReporter.SuppressReportingOfErrors = false;
			var dummyObj = new BusinessObjectFactory().NewWithValidTestData<DummyForCheckingMaximumValue>();

			var noteText = string.Empty;
			AssertExceptionThrown(typeof(MaxLengthExceededException), () =>
			{
				for (var i = 0; i < 20; i++)
				{
					noteText += "we're making a long string to check it gets truncated.";
				}

				dummyObj.NoteText = noteText;
			});

			var exceptionMessage = ErrorReporter.LastMessageReported.Substring(ErrorReporter.LastMessageReported.IndexOf("New value:"), noteText.Length);
			var removedText = "\r\n... content removed for the sake of brevity ...\r\n";
			var key = ErrorReporter.LastKeyReported;

			Assert(key.Contains("at CargoWise.EntityFramework.Testing.BusinessObjectTest") && key.Contains("TestCheckMaximumLengthGivesSpecificKey_ShortMessage"));
			Assert(!exceptionMessage.Contains(removedText));
			AssertEquals(exceptionMessage.Length, noteText.Length);

			ErrorReporter.Clear();
		}

		public void TestCheckMaximumLengthDoesntThrow()
		{
			ErrorReporter.SuppressReportingOfErrors = false;
			var dummyObj = new BusinessObjectFactory().NewWithValidTestData<DummyForCheckingMaximumValue>();

			var noteText = string.Empty;
			AssertNoExceptionThrown(() => dummyObj.NoteText = "short message.");

			Assert(!ErrorReporter.LastMessageReported.Contains("CheckMaximumLength"));
			Assert(!ErrorReporter.LastMessageReported.Contains("\r\n... content removed for the sake of brevity ...\r\n"));

			ErrorReporter.Clear();
		}

		class DummyForCheckingMaximumValue : DummyBusinessObject
		{
			public DummyForCheckingMaximumValue(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			ZString noteText;

			[MaxLength(100)]
			public ZString NoteText
			{
				get
				{
					if (string.IsNullOrEmpty(noteText))
					{
						return "this is a string";
					}
					else
					{
						return noteText;
					}
				}

				set
				{
					CheckMaximumLength(NoteTextInfo, value);
					noteText = value;
				}
			}

			public ZPropertyInfo NoteTextInfo
			{
				get
				{
					return GetZPropertyInfo(nameof(NoteText));
				}
			}
		}

		class DummyWithOverridenDescription : DummyBusinessObject
		{
			public DummyWithOverridenDescription(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZString Z0_Description
			{
				get
				{
					return "hello";
				}
				set
				{
					base.Z0_Description = value;
				}
			}
		}

		public void TestNonZTypeForStringPropertyInfo()
		{
			BusinessObjectWithNonZTypes bizObj = new BusinessObjectWithNonZTypes();
			AssertEquals("Value", bizObj.ValueInfo.Name);
		}

		class BusinessObjectWithNonZTypes : NonPersistentBusinessObject
		{
			public string Value
			{
				get { return value; }
				set { this.value = value; }
			}

			public ZPropertyInfo ValueInfo
			{
				get { return GetZPropertyInfo(nameof(Value)); }
			}

			string value;
		}

		public void TestValidationMethodsOnlyCalledOnce()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			AssertEquals("Precondition", 0, bizO.Z0_DescriptionValidationCount);
			bizO.RunPreSaveValidation();
			AssertEquals("Validation Count for Z0_Description", 1, bizO.Z0_DescriptionValidationCount);
		}

		public void TestOnSavingClearsHasHadOnSavingCalled()
		{
			BusinessObject bizO = DummyBusinessObject.New(Factory);
			Factory.Save();
			AssertEquals(true, bizO.OnSavingCalledInternal);

			// Second save will clear flag
			Factory.Save();
			AssertEquals(false, bizO.OnSavingCalledInternal);
		}

		[ExpectNoExceptions]
		public void TestOnSaving_WhenDeletedBeforeCallingBase()
		{
			DummyDeleteBeforeCallingBaseOnSaving dummy = Factory.New<DummyDeleteBeforeCallingBaseOnSaving>();
			Factory.Save();

			dummy.DeleteBeforeCallingBaseOnSaving = true;
			dummy.Z0_Description = "modified";
			Factory.Save();
		}

		class DummyDeleteBeforeCallingBaseOnSaving : DummyBusinessObject
		{
			public DummyDeleteBeforeCallingBaseOnSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool DeleteBeforeCallingBaseOnSaving { get; set; }

			public override void OnSaving()
			{
				if (DeleteBeforeCallingBaseOnSaving)
				{
					Delete();
				}
				base.OnSaving();
			}
		}

		public void TestAccessingPropertyOnDeletedObject()
		{
			BusinessObject bizO = DummyBusinessObject.New(Factory);
			bizO[DummyBizoSchema.Z0_VarCharMax] = "123";
			bizO.Delete();
			AssertEquals("", bizO[DummyBizoSchema.Z0_VarCharMax]);
			Assert("Expected Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported.Contains("Developer Error: Should not be accessing a property on a deleted business object"));
			ErrorReporter.Clear();
			AssertEquals(ZGuid.Empty, bizO[DummyBizoSchema.Z0_Guid]);
			Assert("Expected Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported.Contains("Developer Error: Should not be accessing a property on a deleted business object"));
			ErrorReporter.Clear();
			AssertEquals(ZDateTime.Empty, bizO[DummyBizoSchema.Z0_Date]);
			Assert("Expected Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported.Contains("Developer Error: Should not be accessing a property on a deleted business object"));
			ErrorReporter.Clear();
			AssertEquals(0, bizO[DummyBizoSchema.Z0_Number]);
			Assert("Expected Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported.Contains("Developer Error: Should not be accessing a property on a deleted business object"));
			ErrorReporter.Clear();
		}

		public void TestHasHadOnSavingIsSetForDeletedObject()
		{
			Thread thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					RunHasHadOnSavingIsSetForDeletedObject();
				}
			});

			thread.Name = "TestHasHadOnSavingIsSetForDeletedObject";
			thread.IsBackground = true;

			thread.Start();
			thread.Join(5000); // if the implementation is wrong the test won't fail - it will go into an infinite loop
			Assert(!thread.IsAlive);
		}

		static void RunHasHadOnSavingIsSetForDeletedObject()
		{
			Db.Connection.BeginTransaction(); // Separate thread requires new transaction as is new connection
			try
			{
				var factory = new BusinessObjectFactory();
				BusinessObject bizO = DummyBusinessObject.New(factory);
				factory.Save();
				AssertEquals(true, bizO.OnSavingCalledInternal);

				bizO.OnSavingCalledInternal = false;
				bizO.Delete();
				factory.Save();
				AssertEquals(true, bizO.OnSavingCalledInternal);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public void TestSchemaColumnIndexer()
		{
			BusinessObject bizO = DummyBusinessObject.New(Factory);
			bizO[DummyBizoSchema.Z0_Code] = "123";
			AssertEquals("123", bizO[DummyBizoSchema.Z0_Code]);
		}

		public void TestOnFactorySavingBeforeTransactionWithFactory()
		{
			DummyForOnFactorySavingBeforeTransaction dummy = Factory.New<DummyForOnFactorySavingBeforeTransaction>();
			AssertEquals("OnFactorySavingBeforeTransaction not called", 0, dummy.OnFactorySavingBeforeTransactionCoreCallCount);

			Factory.Save();
			AssertEquals("OnFactorySavingBeforeTransactionCalledCount", 1, dummy.OnFactorySavingBeforeTransactionCoreCallCount);

			Factory.Save();
			AssertEquals("OnFactorySavingBeforeTransactionCalledCount", 2, dummy.OnFactorySavingBeforeTransactionCoreCallCount);
		}

		public void TestOnFactorySavingBeforeTransactionCallsThroughToCore()
		{
			DummyForOnFactorySavingBeforeTransaction dummy = Factory.New<DummyForOnFactorySavingBeforeTransaction>();
			AssertEquals("OnFactorySavingBeforeTransaction not called", 0, dummy.OnFactorySavingBeforeTransactionCoreCallCount);

			dummy.OnFactorySavingBeforeTransaction();
			AssertEquals("OnFactorySavingBeforeTransactionCalledCount", 1, dummy.OnFactorySavingBeforeTransactionCoreCallCount);
		}

		public void TestCanContinueWithSaveDefaultValue()
		{
			AssertEquals(true, DummyBusinessObject.New(Factory).CanContinueWithSave);
		}

		public void TestCanContinueWithSaveCallsVirtualMethod()
		{
			var mock = Factory.NewMoq<DummyBusinessObject>();
			mock.Protected().Setup<bool>("CanContinueWithSaveCore").Returns(false);
			DummyBusinessObject businessObject = mock.Object;
			AssertEquals(false, businessObject.CanContinueWithSave);
			mock.Protected().Setup<bool>("CanContinueWithSaveCore").Returns(true);
			AssertEquals(true, businessObject.CanContinueWithSave);
		}

		public class DummyForOnFactorySavingBeforeTransaction : DummyBusinessObject
		{
			public DummyForOnFactorySavingBeforeTransaction(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new static DummyForOnFactorySavingBeforeTransaction New(BusinessObjectFactory factory)
			{
				return factory.New<DummyForOnFactorySavingBeforeTransaction>();
			}

			public int OnFactorySavingBeforeTransactionCoreCallCount;

			protected override void OnFactorySavingBeforeTransactionCore()
			{
				base.OnFactorySavingBeforeTransactionCore();
				AssertEquals("Must be in transactioned test case", true, TransactionedTestCase.InTransactionedTestCase);
				AssertEquals("One open transaction since this in in transactioned test case", 1, Db.Connection.AppTransactionCount);
				OnFactorySavingBeforeTransactionCoreCallCount++;
			}
		}

		public void TestChangeNumber()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = factory.New<DummyBusinessObject>();
			AssertEquals("FactoryChangeNumber", factory.LastChangeNumber, 0);
			AssertEquals("DummyChangeNumber", dummy.LastChangeNumber, 0);

			dummy.Z0_Code = "123";
			AssertEquals("FactoryChangeNumber", factory.LastChangeNumber, 1);
			AssertEquals("DummyChangeNumber", dummy.LastChangeNumber, 1);

			dummy.Z0_Code = "ABC";
			AssertEquals("FactoryChangeNumber", factory.LastChangeNumber, 2);
			AssertEquals("DummyChangeNumber", dummy.LastChangeNumber, 2);

			DummyBusinessObject dummy2 = factory.New<DummyBusinessObject>();
			AssertEquals("FactoryChangeNumber", factory.LastChangeNumber, 2);
			AssertEquals("Dummy2ChangeNumber", dummy2.LastChangeNumber, 0);

			dummy2.Z0_Code = "GGG";
			AssertEquals("FactoryChangeNumber", factory.LastChangeNumber, 3);
			AssertEquals("DummyChangeNumber", dummy.LastChangeNumber, 2);
			AssertEquals("Dummy2ChangeNumber", dummy2.LastChangeNumber, 3);
		}

		public void TestSuspendOnValueChanged()
		{
			var info = Dummy.Z0_DescriptionInfo;

			var calledOnValueChanged = false;
			info.ValueChanged += (s, e) => calledOnValueChanged = true;

			using (info.SuspendOnValueChanged())
			{
				info.Value = new ZString("some description");
			}

			AssertEquals("OnValueChanged wasn't called due to suspension", false, calledOnValueChanged);

			info.Value = new ZString("other description");
			AssertEquals("OnValueChanged was called after suspension was removed", true, calledOnValueChanged);

			calledOnValueChanged = false;
			using (info.SuspendOnValueChanged())
			{
				info.Value = new ZString("some description (2)");
			}

			AssertEquals("OnValueChanged wasn't called due to suspension", false, calledOnValueChanged);

			info.Value = new ZString("other description (2)");
			AssertEquals("OnValueChanged was called after suspension was removed", true, calledOnValueChanged);
		}

		public void TestSuspendOnValueChanged_MultipleOfTheSameProperty()
		{
			var info = Dummy.Z0_DescriptionInfo;

			var calledOnValueChanged = false;
			info.ValueChanged += (s, e) => calledOnValueChanged = true;

			using (info.SuspendOnValueChanged())
			{
				using (info.SuspendOnValueChanged())
				{
					using (info.SuspendOnValueChanged())
					{
						info.Value = new ZString("some description lvl1");
					}

					info.Value = new ZString("some description lvl2");
				}

				info.Value = new ZString("some description lvl3");
			}

			AssertEquals("OnValueChanged wasn't called due to suspension", false, calledOnValueChanged);

			info.Value = new ZString("other description");
			AssertEquals("OnValueChanged was called after suspension was removed", true, calledOnValueChanged);

			calledOnValueChanged = false;
			using (info.SuspendOnValueChanged())
			{
				info.Value = new ZString("some description (2)");
			}

			AssertEquals("OnValueChanged wasn't called due to suspension", false, calledOnValueChanged);

			info.Value = new ZString("other description (2)");
			AssertEquals("OnValueChanged was called after suspension was removed", true, calledOnValueChanged);
		}

		public void TestSuspendOnValueChanged_DifferentProperties()
		{
			var info1 = Dummy.Z0_CodeInfo;
			var info2 = Dummy.Z0_DescriptionInfo;

			var calledOnValueChanged_Z0_Code = false;
			info1.ValueChanged += (s, e) => calledOnValueChanged_Z0_Code = true;

			var calledOnValueChanged_Z0_Description = false;
			info2.ValueChanged += (s, e) => calledOnValueChanged_Z0_Description = true;

			using (info1.SuspendOnValueChanged())
			{
				using (info2.SuspendOnValueChanged())
				{
					info2.Value = new ZString("some description");
					info1.Value = new ZString("AAA");
				}

				info1.Value = new ZString("BBB");

				AssertEquals("Z0_Code.OnValueChanged wasn't called due to suspension", false, calledOnValueChanged_Z0_Code);
				AssertEquals("Z0_Description.OnValueChanged wasn't called due to suspension", false, calledOnValueChanged_Z0_Description);

				info2.Value = new ZString("other description");
				AssertEquals("Z0_Description.OnValueChanged was called after suspension was removed", true, calledOnValueChanged_Z0_Description);
			}

			info1.Value = new ZString("CCC");
			AssertEquals("Z0_Code.OnValueChanged was called after suspension was removed", true, calledOnValueChanged_Z0_Code);
		}

		public void TestSuspendingSettingHasChangesSuspendsSettingChangeNumber()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = factory.New<DummyBusinessObject>();
			using (dummy.SuspendSettingHasChanges())
			{
				dummy.Z0_Code = "NNN";
				AssertEquals("FactoryChangeNumber", factory.LastChangeNumber, 0);
				AssertEquals("DummyChangeNumber", dummy.LastChangeNumber, 0);
			}

			dummy.Z0_Code = "EEE";
			AssertEquals("FactoryChangeNumber", factory.LastChangeNumber, 1);
			AssertEquals("DummyChangeNumber", dummy.LastChangeNumber, 1);
		}

		public void TestSuspendingSettingHasChangesKeepValidationAndSavingSuspendsSettingChangeNumber()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			using (dummy.SuspendSettingHasChangesKeepValidationAndSaving())
			{
				dummy.Z0_Code = "NNN";
				AssertEquals("FactoryChangeNumber", factory.LastChangeNumber, 0);
				AssertEquals("DummyChangeNumber", dummy.LastChangeNumber, 0);
			}

			dummy.Z0_Code = "EEE";
			AssertEquals("FactoryChangeNumber", factory.LastChangeNumber, 1);
			AssertEquals("DummyChangeNumber", dummy.LastChangeNumber, 1);
		}

		public void TestSuspendingSettingHasChangesKeepValidationAndSaving()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();
			Assert("HasChanges is false", !dummy.HasChanges);
			dummy.MarkLightValidationAsValidForTesting();
			Assert("LightValidationIsValid", dummy.LightValidationIsValid);

			using (dummy.SuspendSettingHasChangesKeepValidationAndSaving())
			{
				dummy.Z0_Code = "NNN";
				Assert("HasChanges is false", !dummy.HasChanges);
				Assert("LightValidationIsValid is false", !dummy.LightValidationIsValid);
			}

			dummy.OnFactorySavingBeforeTransaction(); // To restore HasChanges
			Assert("HasChanges", dummy.HasChanges);
			Assert("LightValidationIsValid is still false", !dummy.LightValidationIsValid);
			factory.Save();

			dummy.ClearHasChanges();
			Assert("HasChanges is false", !dummy.HasChanges);
			dummy.MarkLightValidationAsValidForTesting(); // To simulate Validation run
			Assert("LightValidationIsValid", dummy.LightValidationIsValid);

			using (dummy.SuspendSettingHasChangesKeepValidationAndSaving())
			{
				dummy.Delete();
				Assert("HasChanges is false", !dummy.HasChanges);
				Assert("LightValidationIsValid", dummy.LightValidationIsValid);
			}

			dummy.OnFactorySavingBeforeTransaction();
			Assert("IsDeleted", dummy.IsDeleted);
			Assert("HasChanges", dummy.HasChanges);
			Assert("LightValidationIsValid", dummy.LightValidationIsValid);
		}

		public void TestSuspendSettingHasChangesKeepValidationAndSaving_DoesNotSuspendHasChangesDuringSaving()
		{
			var factory = new BusinessObjectFactory();
			var dummy1 = factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();
			Assert("Precondition - HasChanges is false", !dummy1.HasChanges);

			dummy1.OnFactorySavingBeforeTransaction();
			using (dummy1.SuspendSettingHasChangesKeepValidationAndSaving())
			{
				dummy1.Z0_Code = "NNN";
				Assert("HasChanges is true", dummy1.HasChanges);
			}

			using (dummy2.SuspendSettingHasChangesKeepValidationAndSaving())
			{
				dummy2.Z0_Code = "NNN";
				Assert("HasChanges is false", !dummy2.HasChanges);
			}
		}

		public void TestLazyLoadedTextFieldsGetCopiedWhenCopyPersistentValuesFrom()
		{
			Dummy.Z0_VarCharMax = "Noodle";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = factory2.Load<DummyBusinessObject>(Dummy.PK);

			DummyBusinessObject newDummy = factory2.New<DummyBusinessObject>();
			newDummy.CopyPersistentValuesFromPublic(dummy2);

			AssertEquals("Z0_VarCharMax", "Noodle", newDummy.Z0_VarCharMax);
		}

		public void TestFetchStrategyLoadEnabled()
		{
			BusinessObject bizO = Factory.New(typeof(DummyWithOverridenName));
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.LoadFetchingEnabled = true;
			BusinessObject bizO2 = factory2.Load(typeof(DummyBusinessObject), bizO.PK);
			AssertEquals(1, ((BusinessObjectFetchStrategyForTest)bizO2.FetchStrategy).FetchForLoadCoreCount);
		}

		public void TestRegistryAffectsStrategy()
		{
			BusinessObject bizO = Factory.New(typeof(DummyWithOverridenName));
			Factory.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			factory3.LoadFetchingEnabled = true;
			BusinessObject bizO3 = factory3.Load(typeof(DummyBusinessObject), bizO.PK);
			AssertEquals(typeof(BusinessObjectFetchStrategyForTest), bizO3.FetchStrategy.GetType());
		}

		public void TestFetchStrategyLoadDisabled()
		{
			BusinessObject bizO = Factory.New(typeof(DummyWithOverridenName));
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.LoadFetchingEnabled = false;
			BusinessObject bizO2 = factory2.Load(typeof(DummyBusinessObject), bizO.PK);
			AssertEquals(0, ((BusinessObjectFetchStrategyForTest)bizO2.FetchStrategy).FetchForLoadCoreCount);
		}

		#region Name

		class DummyWithOverridenName : DummyBusinessObject
		{
			public DummyWithOverridenName(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZString HumanReadableNameCore
			{
				get { return "NewName"; }
			}

			protected override ZString HumanReadableShortcutNameCore
			{
				get { return "New Shortcut"; }
			}
		}

		public void TestName()
		{
			AssertEquals("Test overriding", "NewName", Factory.New(typeof(DummyWithOverridenName)).HumanReadableName);
			AssertEquals("Using res.get string", "DummyBizo", Dummy.HumanReadableName);
			Dummy.Table = null;
			AssertEquals("With null", "record", Dummy.HumanReadableName);
		}

		#endregion

		#region HumanReadableShortcutName

		[ExpectNoExceptions]
		public void TestHumanReadableShortcutName()
		{
			Dummy.Z0_Code = "CODE";
			Dummy.Z0_Description = "";
			AssertEquals("Should return code", "CODE", Dummy.HumanReadableShortcutName);

			Dummy.Z0_Description = "DESCRIPTION";
			AssertEquals("Should return code and description", "CODE - DESCRIPTION", Dummy.HumanReadableShortcutName);

			Dummy.Z0_Description = "CODE";
			AssertEquals("Should return code", "CODE", Dummy.HumanReadableShortcutName);

			AssertEquals("Should return overridden name", "New Shortcut", Factory.New(typeof(DummyWithOverridenName)).HumanReadableShortcutName);
		}

		[ExpectNoExceptions]
		public void TestHumanReadableShortcutNameNoCodeOrDescription()
		{
			var dummy = Factory.New<BizOWithoutCodeProperty>();
			AssertEquals("Shortcut same as human readable property", dummy.HumanReadableName, dummy.HumanReadableShortcutName);

			var dummy2 = Factory.New<BizOWithoutDescriptionProperty>();
			dummy2.Z0_Code = "Code";
			AssertEquals("Shortcut should return code", dummy2.Z0_Code, dummy2.HumanReadableShortcutName);

			var dummy3 = Factory.New<BizOWithDescriptionButNoCodeProperty>();
			dummy3.Z0_Description = "Description";
			AssertEquals("Shortcut should return description", dummy3.Z0_Description, dummy3.HumanReadableShortcutName);
		}

		#region Dummy BizO

		class BizOWithoutCodeProperty : BusinessObject
		{
			public abstract class Schema
			{
				public const string TableName = "DummyBizo";
				public const string PK = "Z0_PK";
			}

			public BizOWithoutCodeProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZString HumanReadableNameCore
			{
				get { return "Almost Human"; }
			}

			public override SchemaGuidColumn PKSchemaColumn
			{
				get { return DummyBizoSchema.PK; }
			}
		}

		[CodeProperty("Z0_Code")]
		class BizOWithoutDescriptionProperty : BusinessObject
		{
			public abstract class Schema
			{
				public const string TableName = "DummyBizo";
				public const string PK = "Z0_PK";
				public const string Z0_Code = "Z0_Code";
			}

			public BizOWithoutDescriptionProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZString HumanReadableNameCore
			{
				get { return "Almost Human"; }
			}

			public override SchemaGuidColumn PKSchemaColumn
			{
				get { return DummyBizoSchema.PK; }
			}

			public ZString Z0_Code { get; set; }
		}

		#region HumanReadableItemCode

		[ExpectNoExceptions]
		public void TestHumanReadableItemCode()
		{
			var dummy = Factory.New<BizOWithoutDescriptionProperty>();
			dummy.Z0_Code = "Code";
			AssertEquals("Shortcut should return code", dummy.Z0_Code, dummy.HumanReadableItemCode);
		}

		#endregion

		[DescriptionProperty("Z0_Description")]
		class BizOWithDescriptionButNoCodeProperty : BusinessObject
		{
			public abstract class Schema
			{
				public const string TableName = "DummyBizo";
				public const string PK = "Z0_PK";
				public const string Z0_Description = "Z0_Description";
			}

			public BizOWithDescriptionButNoCodeProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZString HumanReadableNameCore
			{
				get { return "Almost Human"; }
			}

			public override SchemaGuidColumn PKSchemaColumn
			{
				get { return DummyBizoSchema.PK; }
			}

			public ZString Z0_Description { get; set; }
		}

		#endregion

		#endregion

		public void TestSettingWithReadOnlyFactory()
		{
			ZBlob blobValue = ZBlob.FromAscii("test original value");
			Dummy.Z0_Number = 1;
			Dummy.Z0_VarBinaryMax = blobValue;

			((IBusinessObjectFactoryInternals)Factory).ReadOnly = true;

			Dummy.Z0_Number = 345345;
			AssertEquals("Not set", 1, Dummy.Z0_Number);

			Dummy.Z0_VarBinaryMax = ZBlob.FromAscii("some other value");
			AssertEquals("Not set", blobValue, Dummy.Z0_VarBinaryMax);
		}

		public void TestReadOnlyReturnsTrueInReadOnlyFactory()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BusinessObject bizO = factory.New(typeof(DummyBusinessObject));
			((IBusinessObjectFactoryInternals)factory).ReadOnly = true;
			AssertEquals(true, bizO.ReadOnly);
		}
		public void TestUpdateHasChangesOnOtherBusinessObjectsAroundThisRow()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyBusinessObject dummy = factory.New<DummyBusinessObject>();
			DummyBaseBusinessObject dummyBase = factory.Load<DummyBaseBusinessObject>(dummy.PK);

			dummy.HasChanges = true;
			Assert("HasChanges", dummy.HasChanges);
			Assert("HasChanges", dummyBase.HasChanges);

			dummyBase.HasChanges = false;
			Assert("HasChanges", !dummy.HasChanges);
			Assert("HasChanges", !dummyBase.HasChanges);
		}

		public void TestIsSavedByFactoryIsBasedOnHasChangesAndNew()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Assert("IsSavedByFactory", dummy.IsSavedByFactory);
			Assert("HasChanges", !dummy.HasChanges);

			dummy.Z0_Description = "BlahBlahBlah";

			Assert("IsSavedByFactory", dummy.IsSavedByFactory);
			Assert("HasChanges", dummy.HasChanges);

			dummy.HasChanges = false;
			Assert("IsSavedByFactory", dummy.IsSavedByFactory);
			Assert("HasChanges", !dummy.HasChanges);

			Assert("IsSavedByFactory", !dummy.IsInDatabase);
			Factory.Save();

			Assert("IsSavedByFactory", !dummy.IsSavedByFactory);
			Assert("HasChanges", !dummy.HasChanges);
			Assert(dummy.IsInDatabase);

			dummy.Z0_AnotherNumber = 3434535;
			Assert(dummy.IsSavedByFactory);
			Assert("HasChanges", dummy.HasChanges);

			dummy.HasChanges = false;
			Assert("IsSavedByFactory", !dummy.IsSavedByFactory);
			Assert("HasChanges", !dummy.HasChanges);

			dummy.Delete();
			Assert("IsSavedByFactory", dummy.IsSavedByFactory);
			Assert("HasChanges", dummy.HasChanges);

			dummy.HasChanges = false;
			Assert("IsSavedByFactory", !dummy.IsSavedByFactory);
			Assert("HasChanges", !dummy.HasChanges);
		}

		public void TestIsSettingHasChangesSuspended()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Assert(!dummy.IsSettingHasChangesSuspended);
			using (dummy.SuspendSettingHasChanges())
			{
				Assert(dummy.IsSettingHasChangesSuspended);
				dummy.Z0_AnotherDecimal = 123m;
				Assert(!dummy.HasChanges);
			}
			Assert(!dummy.HasChanges);
			dummy.Z0_AnotherDecimal = 124m;
			Assert(dummy.HasChanges);
		}

		public void TestNewObjectsAreSavedEvenIfHasChangesIsFalse()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.HasChanges = false;
			Factory.Save();
			AssertNotNull("Should be saved", new BusinessObjectFactory().Load(typeof(DummyBusinessObject), dummy.PK));
		}

		public void TestClearHasChanges()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_AnotherDecimal = 123m;
			AssertEquals("Precondition : HasChanges", true, dummy.HasChanges);
			dummy.ClearHasChanges();
			AssertEquals("HasChanges", false, dummy.HasChanges);
		}

		public void TestObjectsAreNotUpdatedIfHasChangesIsFalse()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Number = 100;
			Factory.Save();

			AssertEquals("Should be saved", 100, ((DummyBusinessObject)(new BusinessObjectFactory().Load(typeof(DummyBusinessObject), dummy.PK))).Z0_Number);

			dummy.Z0_Number = 200;
			Assert("HasChanges", dummy.HasChanges);
			Factory.Save();
			AssertEquals("Should be saved", 200, ((DummyBusinessObject)(new BusinessObjectFactory().Load(typeof(DummyBusinessObject), dummy.PK))).Z0_Number);

			dummy.Z0_Number = 300;
			dummy.HasChanges = false;
			Factory.Save();
			AssertEquals("Should not be saved", 200, ((DummyBusinessObject)(new BusinessObjectFactory().Load(typeof(DummyBusinessObject), dummy.PK))).Z0_Number);
		}

		public void TestObjectsWillNotBeDeletedIfHasChangesIsFalse()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();
			AssertNotNull("Should be saved", new BusinessObjectFactory().Load(typeof(DummyBusinessObject), dummy.PK));
			dummy.Delete();
			dummy.HasChanges = false;
			Factory.Save();
			AssertNotNull("Should not be deleted", new BusinessObjectFactory().Load(typeof(DummyBusinessObject), dummy.PK));
		}

		public void TestAddToFactoryCache()
		{
			BusinessObject dummy = Factory.New(typeof(DummyBusinessObject));
			AssertEquals("Right number of dummies", 1, Factory.GetBizOsForPK(dummy.PK.ToGuid()).Length);
			AssertEquals("Right dummy", dummy, Factory.GetBizOsForPK(dummy.PK.ToGuid())[0]);
		}

		public void TestNotifyThatDataSetHasBeenRolledBack()
		{
			Factory.Save();
			Dummy.Delete();
			Dummy.Row.RejectChanges();
			Assert("Deleted", Dummy.IsDeleted);
			Dummy.OnSaveRollbackInternal();
			Assert("UnDeleted", !Dummy.IsDeleted);
		}

		public void TestNotificationsProperty()
		{
			AssertEquals("Initial no notifications", 0, Dummy.Notifications.Count());

			Dummy.Z0_AnotherDateInfo.AddError("Bad");
			Dummy.Z0_DecimalInfo.AddWarning("Teapot");
			AssertEquals("2 notification", 2, Dummy.Notifications.GetUniqueMessageList().Length);
			AssertEquals("Warnings", 1, Dummy.Notifications.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals("Errors", 1, Dummy.Notifications.GetErrors().Count());
			AssertEquals("MessageErrors", 0, Dummy.Notifications.GetMessageErrors().Count());

			Dummy.AddRowMessageError("Noodle");
			AssertEquals("3 notification", 3, Dummy.Notifications.GetUniqueMessageList().Length);
			AssertEquals("MessageErrors", 1, Dummy.Notifications.GetMessageErrors().Count());

			Dummy.RegisterEditableChildObject(Dummy.Collection);
			Dummy.Collection.AddNew().AddRowError("BAD!");
			AssertEquals("4 notifications", 4, Dummy.NotificationsIncludingChildren.GetUniqueMessageList().Length);
			AssertEquals("Errors", 2, Dummy.NotificationsIncludingChildren.GetErrors().Count());
		}

		#region Child-exclusive validation

		public void TestHasErrorsNotIncludingChildren_ShouldIgnoreErrorsOnChildObjects()
		{
			const string EternalErrorMessage = "Owo";
			DummyBusinessObject kid = Factory.New<DummyBusinessObject>();
			Dummy.RegisterEditableChildObject(kid);
			AssertEquals("Our object has a child", true, Dummy.IsRegisteredEditableChildObject(kid));

			kid.AddRowError(EternalErrorMessage);
			AssertEquals("Our child object's errors exist", true, kid.HasErrors);
			AssertEquals("Our object has errors through its child", true, Dummy.HasErrors);
			AssertEquals("No errors on the object itself yet", false, Dummy.HasErrorsNotIncludingChildren);

			Dummy.AddRowError(EternalErrorMessage);
			AssertEquals("Our child object's errors exist", true, kid.HasErrors);
			AssertEquals("Our object's errors exist, both on itself and on its child", true, Dummy.HasErrors);
			AssertEquals("Our object is in error, so the flag must become true", true, Dummy.HasErrorsNotIncludingChildren);

			kid.RemoveRowError(EternalErrorMessage);
			AssertEquals("Our child object's errors don't exist", false, kid.HasErrors);
			AssertEquals("Our object's errors exist, just not on its child", true, Dummy.HasErrors);
			AssertEquals("Our object is in error, so the flag must become true, regardless of the state of its child object", true, Dummy.HasErrorsNotIncludingChildren);

			Dummy.RemoveRowError(EternalErrorMessage);
			AssertEquals("Our child object's errors don't exist", false, kid.HasErrors);
			AssertEquals("Our object's errors don't exist", false, Dummy.HasErrors);
			AssertEquals("No more errors on the object mean no more errors to care about", false, Dummy.HasErrorsNotIncludingChildren);
		}

		public void TestHasMessageErrorsNotIncludingChildren_ShouldIgnoreErrorsOnChildObjects()
		{
			var messageError = "This is a test error message.";
			var child = Factory.New(typeof(DummyBusinessObject));
			Dummy.RegisterEditableChildObject(child);
			AssertEquals("The object has a child", true, Dummy.IsRegisteredEditableChildObject(child));

			child.AddRowMessageError(messageError);
			AssertEquals("The child has message errors", true, child.HasMessageErrors);
			AssertEquals("The object has message errors through its child", true, Dummy.HasMessageErrors);
			AssertEquals("No message errors on the object itself yet", false, Dummy.HasMessageErrorsNotIncludingChildren);

			Dummy.AddRowMessageError(messageError);
			AssertEquals("The child has message errors", true, child.HasMessageErrors);
			AssertEquals("The object has message errors both on itself and on its child", true, Dummy.HasMessageErrors);
			AssertEquals("The object itself has message errors so the flag must be true", true, Dummy.HasMessageErrorsNotIncludingChildren);

			child.RemoveRowMessageError(messageError);
			AssertEquals("The child does not have message errors", false, child.HasMessageErrors);
			AssertEquals("The object itself has message errors but not on its child", true, Dummy.HasMessageErrors);
			AssertEquals("The object itself has message errors so the flag must be true", true, Dummy.HasMessageErrorsNotIncludingChildren);

			Dummy.RemoveRowMessageError(messageError);
			AssertEquals("The child does not have message errors", false, child.HasMessageErrors);
			AssertEquals("The object does not have message errors", false, Dummy.HasMessageErrors);
			AssertEquals("No message errors both on the object and on its child", false, Dummy.HasMessageErrorsNotIncludingChildren);
		}

		public void TestRunPreSaveValidationExcludingChildren_ShouldNotValidateChildBizos()
		{
			DummyBusinessObject kid = Factory.New<DummyBusinessObject>();
			Dummy.RegisterEditableChildObject(kid);
			AssertEquals("Our object has a child", true, Dummy.IsRegisteredEditableChildObject(kid));

			using (kid.GetValidationSuspender())
			{
				kid.Z0_Description = "Bad"; // apparently this description is VERY bad
			}

			Dummy.RunPreSaveValidationExcludingChildren();

			AssertEquals("Should have validated the bizo once", 1, Dummy.RunPreSaveValidationCount);
			AssertEquals("Should not have validated this child at all", 0, kid.RunPreSaveValidationCount);

			Assert("No error", !Dummy.HasErrors);
			Assert("No error", !kid.HasErrors);

			Dummy.RunPreSaveValidation();

			AssertEquals("Should have validated the bizo once more but it won't be listed because it has no changes in need of validation", 1, Dummy.RunPreSaveValidationCount);
			AssertEquals("Should have validated this child only once", 1, kid.RunPreSaveValidationCount);

			Assert("Has error due to child shape", Dummy.HasErrors);
			Assert("Has error", kid.HasErrors);
		}

		#endregion

		#region TestSetReadOnlyIncludingChildren

		public void TestSetReadOnlyIncludingChildren()
		{
			Dummy.RegisterEditableChildObject(Dummy.Collection);
			BusinessObject dummyChild = Dummy.Collection.AddNew();

			Assert("Not readonly", !Dummy.ReadOnly);
			Assert("Not readonly", !Dummy.Collection.ReadOnly);
			Assert("Not readonly", !dummyChild.ReadOnly);

			Dummy.SetReadOnlyIncludingChildren(true);
			Assert("Readonly", Dummy.ReadOnly);
			Assert("Readonly", Dummy.Collection.ReadOnly);
			Assert("Readonly", dummyChild.ReadOnly);

			Dummy.SetReadOnlyIncludingChildren(false);
			Assert("Not readonly", !Dummy.ReadOnly);
			Assert("Not readonly", !Dummy.Collection.ReadOnly);
			Assert("Not readonly", !dummyChild.ReadOnly);

			Dummy.ReadOnly = true;
			Assert("Readonly", Dummy.ReadOnly);
		}

		class DummyWhoRegistersAsChildInReadOnlySet : DummyBusinessObject
		{
			public DummyWhoRegistersAsChildInReadOnlySet(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public BusinessObject Parent;

			public override bool ReadOnly
			{
				get { return base.ReadOnly; }
				set
				{
					Parent.RegisterEditableChildObject(Collection);
					base.ReadOnly = value;
				}
			}
		}

		public void TestSetReadOnlyIncludingChildrenWorksWhenNewChildrenAreRegoedInReadOnlySet()
		{
			DummyWhoRegistersAsChildInReadOnlySet dummyWhoRegistersAsChildInReadOnlySet = Factory.New<DummyWhoRegistersAsChildInReadOnlySet>();
			Dummy.RegisterEditableChildObject(dummyWhoRegistersAsChildInReadOnlySet);
			dummyWhoRegistersAsChildInReadOnlySet.Parent = Dummy;
			BusinessObject dummyWhoRegistersChild = dummyWhoRegistersAsChildInReadOnlySet.Collection.AddNew();

			Dummy.SetReadOnlyIncludingChildren(true);
			Assert("Readonly", dummyWhoRegistersAsChildInReadOnlySet.ReadOnly);
			Assert("Readonly", Dummy.ReadOnly);
			Assert("Readonly", dummyWhoRegistersChild.ReadOnly);
		}

		#endregion

		public void TestErrorsIncludingChildrenOnBusinessObjectWithChildrenAfterAccessingWrappedProperties()
		{
			object unused1 = Dummy.ZPropertyInfoHash["Self+Z0_Description"];

			Dummy.RegisterEditableChildObject(Dummy.Collection);
			AssertEquals("No errors", 0, Dummy.NotificationsIncludingChildren.GetErrors().Count());

			Dummy.AddRowError("Hello!");
			Dummy.Z0_AnotherDateInfo.AddError("teapot");
			AssertEquals("Errors", 2, Dummy.NotificationsIncludingChildren.GetErrors().Count());

			DummyChildBusinessObject child = Dummy.Collection.AddNew();
			using (child.SuspendValidationTesting())
			{
				AssertEquals("Errors", 2, Dummy.NotificationsIncludingChildren.GetErrors().Count());

				child.AddRowError("Alien!");
				child.Z0_AnotherDateInfo.AddError("teapot");
				child.Z0_DescriptionInfo.AddError("Angle");

				object unused2 = child.ZPropertyInfoHash["Self+Z0_Description"];

				StringBuilder errorBuilder = new StringBuilder();
				foreach (INotification error in child.NotificationsIncludingChildren.GetErrors())
				{
					errorBuilder.Append(error.Message);
					errorBuilder.Append(System.Environment.NewLine);
				}

				AssertEquals("Errors: " + errorBuilder.ToString(), 3, child.NotificationsIncludingChildren.GetErrors().Count());
				AssertEquals("Errors", 5, Dummy.NotificationsIncludingChildren.GetErrors().Count());
			}
		}

		public void TestRegisterEditableChildObject()
		{
			DummyBusinessObject kid = Factory.New<DummyBusinessObject>();
			AssertEquals("Registered", false, Dummy.IsRegisteredEditableChildObject(kid));
			AssertEquals("HasChanges", false, Dummy.HasChanges);

			kid.HasChanges = true;
			Dummy.RegisterEditableChildObject(kid);
			AssertEquals("Registered", true, Dummy.IsRegisteredEditableChildObject(kid));
			AssertEquals("HasChanges", true, Dummy.HasChanges);

			kid.HasChanges = false;
			AssertEquals("HasChanges", false, Dummy.HasChanges);
		}

		public void TestUpdateChildReadOnlyWhenRegistering_ShouldNotTriggerHasChanges()
		{
			var childMock = new Mock<IBusiness>();
			childMock.Setup(c => c.IncrementReadOnlyIncludingChildren()).Callback(() =>
			{
				AssertEquals(true, Dummy.IsSettingHasChangesSuspended);
			});
			Dummy.SetReadOnlyIncludingChildren(true);
			Dummy.RegisterEditableChildObject(childMock.Object);
		}

		public void TestRegisterEditableChildObject_DoesntCauseActiveCollectionToLoad()
		{
			ActiveBusinessObjectCollection<DummyDependantBusinessObject> collection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(Factory);
			Dummy.RegisterEditableChildObject(collection);
			AssertEquals("Collection not unnecessarily loaded when registering child editable", false, ((IActiveBusinessObjectCollection)collection).IsLoaded);
		}

		public void TestUnRegsiterEditableChildObject()
		{
			DummyBusinessObject kid = Factory.New<DummyBusinessObject>();
			AssertEquals("Registered", false, Dummy.IsRegisteredEditableChildObject(kid));
			AssertEquals("HasChanges", false, Dummy.HasChanges);

			kid.HasChanges = true;
			Dummy.RegisterEditableChildObject(kid);
			AssertEquals("Registered", true, Dummy.IsRegisteredEditableChildObject(kid));
			AssertEquals("HasChanges", true, Dummy.HasChanges);

			Dummy.UnRegisterEditableChildObject(kid);
			AssertEquals("Registered", false, Dummy.IsRegisteredEditableChildObject(kid));
			AssertEquals("HasChanges", false, Dummy.HasChanges);
		}

		public void TestDeleteUpdatesParentCollections()
		{
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			BusinessObject child = Factory.New(typeof(DummyBaseBusinessObject));

			AssertEquals("Not in collection", 0, Dummy.ParentCollections.Count);
			AssertEquals("Not in collection", 0, dummy2.ParentCollections.Count);
			AssertEquals("Not in collection", 0, child.ParentCollections.Count);

			Dummy.Collection.Add(child);
			dummy2.Collection.Add(child);
			AssertEquals("In dummy and dummy2 collections", 2, child.ParentCollections.Count);
			AssertEquals("In dummy collection", 1, Dummy.Collection.Count);
			AssertEquals("In dummy2 collection", 1, dummy2.Collection.Count);

			child.Delete();
			AssertEquals("No parent collections", 0, child.ParentCollections.Count);
			AssertEquals("Not not in Dummy1", 0, Dummy.Collection.Count);
			AssertEquals("Not in Dummy2", 0, dummy2.Collection.Count);
		}

		public void TestPropertySetAndGet()
		{
			BizO.Z0_Description = "Something";
			Assert(BizO.Z0_Description == "Something");
		}

		public void TestIndexerSetAndGet()
		{
			BizO[DummyBusinessObject.Schema.Z0_Description] = "SomeOtherThing";
			Assert(BizO[DummyBusinessObject.Schema.Z0_Description].Equals("SomeOtherThing"));
		}

		public void TestIsRowChanged()
		{
			var bizo = Factory.New<DummyWithNonPersistentProperty>();
			Assert(bizo.IsRowChanged);
			Factory.Save();
			Assert(!bizo.IsRowChanged);
			bizo.Z0_Score = 1;
			Assert(!bizo.IsRowChanged);
			bizo.Z0_Description = "Change the value of a persistent property";
			Assert(bizo.IsRowChanged);
			Factory.Save();
			Assert(!bizo.IsRowChanged);
		}

		class DummyWithNonPersistentProperty : DummyBusinessObject
		{
			public DummyWithNonPersistentProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new class Schema : AutoDummyBizo.Schema
			{
				public const string Z0_Score = "Z0_Score";
			}

			public ZInt Z0_Score
			{
				get { return score; }
				set { SetNonPersistentPropertyValue(Z0_ScoreInfo, ref score, value); }
			}

			public ZPropertyInfo Z0_ScoreInfo
			{
				get { return GetZPropertyInfo(Schema.Z0_Score); }
			}

			ZInt score;
		}

		public void TestHasChanges()
		{
			TestHasChangesCore();
		}

		public void TestHasChangesWithCached()
		{
			using (Factory.CacheBusinessObjectHasChanges())
			{
				TestHasChangesCore();
			}
		}

		public void TestLostCaches()
		{
			Factory.CacheBusinessObjectHasChanges();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BizO.Z0_Description = "Business objects are cool!";
			Assert(BizO.HasChanges);
			factory.Save();
			factory = null;
			AssertEquals("Support Factory return true", true, Factory.GetHasChangesFromCached(BizO));
		}

		void TestHasChangesCore()
		{
			Assert(!BizO.HasChanges);
			BizO.Z0_Description = "Business objects are cool!";
			Assert(BizO.HasChanges);
			Factory.Save();
			Assert(!BizO.HasChanges);
		}

		public void TestHasChangesNotIncludingChildren()
		{
			Dummy.RegisterEditableChildObject(Dummy.Collection);
			IBusiness iBiz = Dummy;

			Assert("No changes", !Dummy.HasChanges);
			Assert("No changes", !iBiz.HasChangesNotIncludingChildren);

			Dummy.Collection.AddNew().Z0_Number = 2345;

			Assert("Collection changes", Dummy.HasChanges);
			Assert("No changes", !iBiz.HasChangesNotIncludingChildren);

			Dummy.Z0_AnotherNumber = 233;

			Assert("Dummy itself has changes", Dummy.HasChanges);
			Assert("Dummy itself has changes", iBiz.HasChangesNotIncludingChildren);
		}

		public void TestSetDefaultValues()
		{
			Assert(BizO.Z0_Description == "Default");
		}

		public void TestPKIsSet()
		{
			Assert(!BizO.PK.IsEmpty);
		}

		public void TestValidate()
		{
			BizO.Z0_Description = "somejunk";
			AssertEquals(false, BizO.HasErrors);

			BizO.Z0_Description = "Bad";
			AssertEquals(true, BizO.HasErrors);
		}

		public void TestValidateNullRef()
		{
			AssertNoExceptionThrown(() => ((IBusinessObjectInternals)BizO).Validate((ZPropertyInfo)null));
		}

		public void TestHasErrorsClearFromProperty()
		{
			BizO.Z0_Description = "Bad";
			AssertEquals(true, BizO.HasErrors);

			BizO.Z0_DescriptionInfo.ClearAllNotifications();
			AssertEquals(false, BizO.HasErrors);
		}

		public void TestHasErrorsFromProperty()
		{
			AssertEquals(false, BizO.HasErrors);

			BizO.Z0_DescriptionInfo.AddError("Something bad!");
			AssertEquals(true, BizO.HasErrors);
		}

		public void TestHasWarningsFromProperty()
		{
			AssertEquals(false, BizO.HasNotifications());

			BizO.Z0_DescriptionInfo.AddWarning("Something bad!");
			AssertEquals(true, BizO.HasWarnings);
		}

		public void TestHasMessageErrorsFromProperty()
		{
			AssertEquals(false, BizO.HasNotifications());

			BizO.Z0_DescriptionInfo.AddMessageError("Something bad!");
			AssertEquals(true, BizO.HasMessageErrors);
		}

		public void TestRowErrors()
		{
			AssertEquals(false, BizO.HasRowErrors);
			AssertEquals(false, BizO.HasErrors);
			AssertEquals(false, BizO.HasNotifications());

			BizO.AddRowError("Rara");
			AssertEquals(true, BizO.HasRowErrors);
			AssertEquals(true, BizO.HasErrors);
			AssertEquals(true, BizO.HasNotifications());
		}

		public void TestRowWarnings()
		{
			AssertEquals(false, BizO.HasRowWarnings);
			AssertEquals(false, BizO.HasWarnings);
			AssertEquals(false, BizO.HasNotifications());

			BizO.AddRowWarning("Rara");
			AssertEquals(true, BizO.HasRowWarnings);
			AssertEquals(true, BizO.HasWarnings);
			AssertEquals(true, BizO.HasNotifications());
		}

		public void TestRowMessageErrors()
		{
			AssertEquals(false, BizO.HasRowMessageErrors);
			AssertEquals(false, BizO.HasMessageErrors);
			AssertEquals(false, BizO.HasNotifications());

			BizO.AddRowMessageError("Rara");
			AssertEquals(true, BizO.HasRowMessageErrors);
			AssertEquals(true, BizO.HasMessageErrors);
			AssertEquals(true, BizO.HasNotifications());
		}

		public void TestHasNotificationsFromProperty()
		{
			AssertEquals(false, BizO.HasNotifications());

			BizO.Z0_DescriptionInfo.AddWarning("Something bad!");
			AssertEquals(true, BizO.Z0_DescriptionInfo.HasNotifications());

			BizO.Z0_DescriptionInfo.ClearAllNotifications();
			AssertEquals(false, BizO.HasNotifications());
			BizO.Z0_DescriptionInfo.AddWarning("Something bad!");
			AssertEquals(true, BizO.Z0_DescriptionInfo.HasNotifications());

			BizO.Z0_DescriptionInfo.ClearAllNotifications();
			AssertEquals(false, BizO.HasNotifications());
			BizO.Z0_DescriptionInfo.AddMessageError("Something bad!");
			AssertEquals(true, BizO.Z0_DescriptionInfo.HasNotifications());
		}

		public void TestClearAllNotifications()
		{
			AssertEquals(false, BizO.HasNotifications());

			BizO.Z0_DescriptionInfo.AddWarning("Something bad!");
			AssertEquals(true, BizO.Z0_DescriptionInfo.HasNotifications());

			BizO.AddRowMessageError("Rarara");

			AssertEquals(true, BizO.HasNotifications());

			BizO.ClearAllNotifications();
			AssertEquals(false, BizO.HasNotifications());
		}

		public void TestClearRowNotifications()
		{
			AssertEquals(false, BizO.HasNotifications());
			AssertEquals(false, BizO.HasRowNotifications);

			BizO.Z0_DescriptionInfo.AddWarning("Something bad!");
			AssertEquals(true, BizO.Z0_DescriptionInfo.HasNotifications());

			BizO.AddRowMessageError("Rarara");

			AssertEquals(true, BizO.HasNotifications());
			AssertEquals(true, BizO.HasRowNotifications);

			BizO.ClearRowNotifications();
			AssertEquals(true, BizO.HasNotifications());
			AssertEquals(false, BizO.HasRowNotifications);
		}

		public void TestClearRowNotificationsContaining()
		{
			AssertEquals(false, BizO.HasRowNotifications);

			BizO.AddRowError("Rarara");
			BizO.AddRowError("Narara");

			AssertEquals(true, BizO.HasRowNotifications);
			AssertHasRowError(BizO, "Rarara");
			AssertHasRowError(BizO, "Narara");

			BizO.ClearRowNotificationsContaining("Nar");

			AssertEquals(true, BizO.HasRowNotifications);
			AssertHasRowError(BizO, "Rarara");
			AssertNoRowError(BizO, "Narara");
		}

		public void TestSettingReadOnlyOnlyRaisesOnListChangedEventIfReadOnlyDifferent()
		{
			EventFired = false;
			((IBindingList)BizO).ListChanged += new ListChangedEventHandler(BusinessObjectTest_ListChanged);
			BizO.ReadOnly = true;

			AssertEquals("Event Should fire when initially setting ReadOnly to true", true, EventFired);
			EventFired = false;
			BizO.ReadOnly = true;
			AssertEquals("Event should not fire when setting ReadOnly to same value", false, EventFired);
			BizO.ReadOnly = false;
			AssertEquals("Event Should fire when initially setting ReadOnly to true", true, EventFired);
			EventFired = false;
			BizO.ReadOnly = false;
			AssertEquals("Event should not fire when setting ReadOnly to same value", false, EventFired);
			BizO.ReadOnly = true;
			AssertEquals("Event Should fire when initially setting ReadOnly to true", true, EventFired);
		}

		#region DummyForUpdateForDataRefreshTest

		class DummyForUpdateForDataRefreshTest : DummyBusinessObject
		{
			public DummyForUpdateForDataRefreshTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnLoaded()
			{
				base.OnLoaded();
				fCalculatedString = Z0_Code;
			}

			public override ZString Z0_Code
			{
				get { return base.Z0_Code; }
				set
				{
					base.Z0_Code = value;
					fCalculatedString = value; //To make the order of setting Code and Desc important.
				}
			}

			public override ZString Z0_Description
			{
				get { return base.Z0_Description; }
				set
				{
					base.Z0_Description = value;
					fCalculatedString = value; //To make the order of setting Code and Desc important.
				}
			}

			public ZString CalculatedString
			{
				get { return fCalculatedString; }
			}

			ZString fCalculatedString;

			protected override void RunPreSaveValidationCore()
			{
				base.RunPreSaveValidationCore();
				PreSaveValidationHasBeenRun = true;
			}

			public bool PreSaveValidationHasBeenRun;
		}

		#endregion

		public void TestUpdateForDataRefresh()
		{
			var dummy1 = Factory.New<DummyForUpdateForDataRefreshTest>();
			dummy1.Z0_Code = "";
			dummy1.Z0_Description = "";

			Factory.Save();

			dummy1.Z0_Code = "Code";
			dummy1.Z0_Description = "Desc";

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyForUpdateForDataRefreshTest dummy2 = factory2.Load<DummyForUpdateForDataRefreshTest>(dummy1.PK);

			AssertEquals("Code", "", dummy2.Z0_Code);
			AssertEquals("Description", "", dummy2.Z0_Description);
			AssertEquals("CalculatedString", "", dummy2.CalculatedString);

			dummy2.UpdateForDataRefresh(dummy1);

			AssertEquals("Code", "Code", dummy2.Z0_Code);
			AssertEquals("Description", "Desc", dummy2.Z0_Description);
			AssertEquals("CalculatedString", "Code", dummy2.CalculatedString);
		}

		public void TestUpdateForDataRefreshWithValidationSuspended()
		{
			DummyForUpdateForDataRefreshTest dummy1 = Factory.New<DummyForUpdateForDataRefreshTest>();

			dummy1.Z0_Code = "Code";
			AssertEquals("CalculatedString", "Code", dummy1.CalculatedString);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyForUpdateForDataRefreshTest dummy2 = factory2.Load<DummyForUpdateForDataRefreshTest>(dummy1.PK);

			using (dummy2.GetValidationSuspender())
			{
				dummy2.UpdateForDataRefresh(dummy1);
				AssertEquals("Code", "Code", dummy2.Z0_Code);
			}

			Assert("!PreSaveValidationHasBeenRun", !dummy2.PreSaveValidationHasBeenRun);
		}

		public void TestCopyPersistentValuesFrom()
		{
			BizO.Z0_Description = "Set!";
			BizO.Z0_Decimal = 5;

			Dummy.CopyPersistentValuesFromPublic(BizO);

			AssertEquals("Set!", Dummy.Z0_Description);
			AssertEquals((ZDecimal)5, Dummy.Z0_Decimal);
		}

		public void TestCopyPersistentValuesFromWithTextButNotImage()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "Random Text";
			dummy.Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyBusinessObject dummyInNewFactory = newFactory.Load<DummyBusinessObject>(dummy.PK);
			DummyBusinessObject dummyClone = newFactory.New<DummyBusinessObject>();

			List<string> excludedInfos = new List<string>();
			foreach (ZPropertyInfo info in dummyInNewFactory.ZPropertyInfoHash)
			{
				if (info.PropertyType != typeof(ZString))
				{
					excludedInfos.Add(info.Name);
				}
			}

			dummyClone.CopyPersistentValuesFrom(dummyInNewFactory, new BusinessObjectCloneArgs(excludedInfos));

			AssertEquals("should have loaded the text field before copping it", dummy.Z0_VarCharMax, dummyClone.Z0_VarCharMax);
		}

		public void TestCloneWithFixValue()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_VarCharMax = "Text 1";
			var dummy2 = Factory.New<DummyChildBusinessObject>();
			dummy2.Z0_VarCharMax = "Text 2";
			var args = new BusinessObjectCloneArgs();
			args.AddValueOverride(typeof(DummyChildBusinessObject), DummyBusinessObject.Schema.Z0_VarCharMax, (ZString)"Text 3");
			var clonedDummy = (AutoDummyBizo)dummy1.Clone(args);
			AssertEquals("Should cloned", "Text 1", clonedDummy.Z0_VarCharMax);

			clonedDummy = (AutoDummyBizo)dummy2.Clone(args);
			AssertEquals("Should used fix value", "Text 3", clonedDummy.Z0_VarCharMax);
		}

		public void TestGetPropertyValueByName()
		{
			SuperDummy superDummy = Factory.New<SuperDummy>();
			CombineAssertions(() =>
			{
				superDummy.Child = Factory.New<DummyBusinessObject>();
				AssertExceptionThrown(typeof(ArgumentException), () => superDummy.GetPropertyValueByName("Child+MyProperty"));
				AssertExceptionThrown(typeof(ArgumentException), () => superDummy.GetPropertyValueByName("Child.MyProperty"));

				superDummy.Child = Factory.New<DummyBusinessObjectSubClass>();
				AssertEquals("hello", superDummy.GetPropertyValueByName("Child+MyProperty"));
				AssertEquals("hello", superDummy.GetPropertyValueByName("Child.MyProperty"));
			});
		}

		public void TestReportErrorOnDataRefresh_WhenExceptionSettingRow()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "TST";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
			dummy2.Z0_Description = "changed";

			dummy1.Table.RowChanging += (_, __) => throw new InvalidOperationException("On No!");

			factory2.Save();

			AssertEquals("Error During DataRefreshBus publish (publisher type = CargoWise.EntityFramework.BusinessObject[])", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestReportErrorOnDataRefresh_WhenExceptionDeletingRow()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "TST";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var dummy2 = factory2.Load<DummyBaseBusinessObject>(dummy1.PK);

			dummy2.Delete();

			var threadName = "Main Thread";
			if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
			{
				Thread.CurrentThread.Name = threadName;
			}
			else
			{
				threadName = Thread.CurrentThread.Name;
			}

			dummy1.Table.RowDeleting += (_, __) => throw new InvalidOperationException("On No!");

			factory2.Save();

			AssertEquals("Error During DataRefreshBus publish (publisher type = CargoWise.EntityFramework.BusinessObject[])", ErrorReporter.LastMessageReported);

			var exceptionMsg = ErrorReporter.LastExceptionsReported().Single(e => e.Contains("Error on DeleteRow Row.Delete()"));
			AssertContains("Table: DummyBizo,  BusinessObject: DummyBusinessObject, ThreadName: " + threadName, exceptionMsg);

			ErrorReporter.Clear();
		}

		class DummyBusinessObjectSubClass : DummyBusinessObject
		{
			public DummyBusinessObjectSubClass(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
			public ZString MyProperty { get { return "hello"; } }
		}

		class SuperDummy : DummyBusinessObject
		{
			public SuperDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public DummyBusinessObject Child { get; set; }
		}

		public void TestCopyPersistentValuesFromUsingTwoObjectsFromDifferentViews()
		{
			Dummy.Z0_Code = "abc";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = factory2.Load<DummyBusinessObject>(Dummy.PK);
			dummy2.Z0_Code = "xxx";

			var baseResolver = ObjectFactory.Get<IApplicationSchemaResolver>();

			var mockResolver = new Mock<EnterpriseSchemaResolver>();
			mockResolver.Setup(m => m.GetSchemaColumnSafe(It.IsAny<string>(), It.IsAny<string>()))
				.Returns<string, string>(
					(arg1, arg2) =>
					{
						var (columnName, tableName) = (arg1, arg2);
						switch (tableName)
						{
							case "":
								switch (columnName)
								{
									case "TestNonExistentColumn":
										return new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "TestNonExistentColumn", 0, SqlDbType.VarChar, string.Empty, true, 10);

									case "AnotherNonExistentColumn":
										return new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "AnotherNonExistentColumn", 0, SqlDbType.VarChar, string.Empty, true, 10);
								}
								break;
						}

						return baseResolver.GetSchemaColumnSafe(columnName, tableName);
					});

			using (ObjectFactory.Substitute<IApplicationSchemaResolver>(mockResolver.Object))
			{
				Dummy.Table.TableName = "NewTableName1";
				Dummy.Table.Columns.Add("TestNonExistentColumn");

				dummy2.Table.TableName = "NewTableName2";
				dummy2.Table.Columns.Add("AnotherNonExistentColumn");
				dummy2.CopyPersistentValuesFromPublic(Dummy);

				AssertEquals("Code", "abc", dummy2.Z0_Code);
			}
		}

		#region TestCopyValuesFrom

		[Obsolete]
		public void TestCopyValuesFrom_ObsoleteAttribute()
		{
			var dummy = Factory.New<TestCopyValuesFromBusinessObjectContainsObsoleteProperty>();
			dummy.ObsoleteNumber = 3;

			var dummyNew = Factory.New<TestCopyValuesFromBusinessObjectContainsObsoleteProperty>();
			AssertEquals(0, dummyNew.ObsoleteNumber);

			dummyNew.CopyValuesFromPublic(dummy);
			AssertEquals(3, dummyNew.ObsoleteNumber);
		}

		public void TestCopyValuesFrom_SuspendValidation()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "message error";
			AssertEquals(true, dummy.Z0_DescriptionInfo.HasMessageError("Message Error"));

			var dummyNew = Factory.New<DummyBusinessObject>();
			dummyNew.CopyValuesFromPublic(dummy);
			AssertEquals("message error", dummyNew.Z0_Description);
			AssertEquals(false, dummyNew.Z0_DescriptionInfo.HasMessageError("Message Error"));
		}

		public void TestCopyValuesFromWhereOverriddenGettersReturnDataDifferentToWhatIsInTheRow()
		{
			// test that overridden getters in sub-types are not ignored
			TestCopyValuesFromBusinessObject testCopyObject = Factory.New<TestCopyValuesFromBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();

			Dummy.Z0_Number = 99999010;
			dummy2.Z0_Number = -19421;
			testCopyObject.Z0_Number = 951847;

			Dummy.Z0_AnotherDate = new ZDateTime(1951, 10, 15);
			dummy2.Z0_AnotherDate = new ZDateTime(1952, 11, 16);
			testCopyObject.Z0_AnotherDate = new ZDateTime(1953, 12, 17);

			Dummy.Z0_Description = "harmless";
			dummy2.Z0_Description = "mostly harmless";
			testCopyObject.Z0_Description = "big yellow bulldozer";

			dummy2.CopyValuesFromPublic(testCopyObject);
			AssertEquals("CopyValuesFrom() should make these properties equal", testCopyObject.Z0_Number, dummy2.Z0_Number);
			AssertEquals("CopyValuesFrom() should make these properties equal", testCopyObject.Z0_AnotherDate, dummy2.Z0_AnotherDate);
			AssertEquals("CopyValuesFrom() should make these properties equal", testCopyObject.Z0_Description, dummy2.Z0_Description);
		}

		public void TestCopyValuesWhereNoGettersAreOverriden()
		{
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<TestCopyValuesFromBusinessObjectDifferent>();

			Dummy.Z0_Number = 99999010;
			dummy2.Z0_Number = -19421;
			dummy3.Z0_Number = 951847;

			Dummy.Z0_AnotherDate = new ZDateTime(1951, 10, 15);
			dummy2.Z0_AnotherDate = new ZDateTime(1952, 11, 16);
			dummy3.Z0_AnotherDate = new ZDateTime(1953, 12, 17);

			Dummy.Z0_Description = "harmless";
			dummy2.Z0_Description = "mostly harmless";
			dummy3.Z0_Description = "big yellow bulldozer";

			dummy2.CopyValuesFromPublic(Dummy);
			AssertEquals("CopyValuesFrom() should make these properties equal", Dummy.Z0_Number, dummy2.Z0_Number);
			AssertEquals("CopyValuesFrom() should make these properties equal", Dummy.Z0_AnotherDate, dummy2.Z0_AnotherDate);
			AssertEquals("CopyValuesFrom() should make these properties equal", Dummy.Z0_Description, dummy2.Z0_Description);

			dummy2.CopyValuesFromPublic(dummy3);
			AssertEquals("CopyValuesFrom() should make these properties equal", dummy3.Z0_Number, dummy2.Z0_Number);
			AssertEquals("CopyValuesFrom() should make these properties equal", dummy3.Z0_AnotherDate, dummy2.Z0_AnotherDate);
			AssertEquals("CopyValuesFrom() should make these properties equal", dummy3.Z0_Description, dummy2.Z0_Description);
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestCopyValuesFromWithWrongType()
		{
			// it should throw an exception if tried with different types
			TestCopyValuesFromBusinessObject type1 = new TestCopyValuesFromBusinessObject(Dummy.Factory, Dummy.Row);
			TestCopyValuesFromBusinessObjectDifferent type2 = new TestCopyValuesFromBusinessObjectDifferent(Dummy.Factory, Dummy.Row);
			type1.CopyValuesFromPublic(type2);
		}

		// for testing that all 'gets' go through the same objects (i.e. without shortcuts that could screw up overrides)
		public class TestCopyValuesFromBusinessObject : DummyBusinessObject
		{
			public TestCopyValuesFromBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void CopyValuesFromPublic(TestCopyValuesFromBusinessObject sourceObject)
			{
				CopyValuesFrom(sourceObject);
			}

			public override ZInt Z0_Number
			{
				get { return 666; }
			}

			public override ZDateTime Z0_AnotherDate
			{
				get { return new ZDateTime(2004, 1, 30); }
			}

			public override ZString Z0_Description
			{
				get { return "bwahahaha! ph34R my l33+ hA><0rING!"; }
			}
		}

		public class TestCopyValuesFromBusinessObjectDifferent : DummyBusinessObject
		{
			public TestCopyValuesFromBusinessObjectDifferent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		public class TestCopyValuesFromBusinessObjectContainsObsoleteProperty : DummyBusinessObject
		{
			public TestCopyValuesFromBusinessObjectContainsObsoleteProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[Obsolete]
			public virtual ZInt ObsoleteNumber { get; set; }

			[Obsolete]
			public virtual ZPropertyInfo ObsoleteNumberInfo
			{
				get { return GetZPropertyInfo(nameof(ObsoleteNumber)); }
			}
		}

		#endregion

		#region TestNotificationPropogation

		public void TestNotificationPropogation()
		{
			expectedSourceOfNotificationChange = Dummy;
			Dummy.NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(DummyNotificationChangedEventHandler);
			Dummy.Z0_AnotherNumberInfo.AddError("A prop info error");

			AssertEquals("NotificationChanged Fired", 1, NotificationChangedFiredCount);
		}

		public void TestNotificationPropogationOnRegisterUnregisterEditableChild()
		{
			Dummy.NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(DummyNotificationChangedEventHandler);
			expectedSourceOfNotificationChange = Dummy;
			AssertEquals("NotificationChanged Fired", 0, NotificationChangedFiredCount);

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.AddRowError("bad");
			Dummy.RegisterEditableChildObject(dummy2);
			AssertEquals("NotificationChanged Fired", 1, NotificationChangedFiredCount);

			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			Dummy.RegisterEditableChildObject(dummy3);
			AssertEquals("NotificationChanged Fired", 1, NotificationChangedFiredCount);

			Dummy.UnRegisterEditableChildObject(dummy3);
			AssertEquals("NotificationChanged Fired", 1, NotificationChangedFiredCount);

			Dummy.UnRegisterEditableChildObject(dummy2);
			AssertEquals("NotificationChanged Fired", 2, NotificationChangedFiredCount);

			dummy2.AddRowError("blah");
			dummy3.AddRowError("blah");
			AssertEquals("NotificationChanged Fired", 2, NotificationChangedFiredCount);
		}

		object expectedSourceOfNotificationChange;
		void DummyNotificationChangedEventHandler(object sender, NotificationsChangedEventArgs e)
		{
			AssertEquals("expectedSourceOfNotificationChange", expectedSourceOfNotificationChange, e.SourceOfNotificationChange);
			NotificationChangedFiredCount++;
		}

		int NotificationChangedFiredCount;

		#endregion

		public void TestGetActiveFilter()
		{
			ZQuery filter;
			filter = BusinessObject.GetActiveFilter(typeof(DummyBusinessObject));
			AssertNull("Should have no filter defined", filter);

			filter = BusinessObject.GetActiveFilter(typeof(DummyBusinessObjectWithActiveFilter));
			AssertEquals("Z0_Bool = 1", filter.LiteralTextADO);
		}

		public void TestClone()
		{
			BizO.Z0_Description = "Set!";
			BizO.Z0_Decimal = 5;
			BusinessObject clonedBizO = BizO.Clone();
			AssertEquals(clonedBizO.GetType(), clonedBizO.GetType());
			DummyBusinessObject clone = (DummyBusinessObject)clonedBizO;
			AssertEquals("Values copied", "Set!", clone.Z0_Description);
			AssertEquals("Values copied", (ZDecimal)5, clone.Z0_Decimal);
			AssertEquals("IsInDatabase", false, clone.IsInDatabase);
			AssertEquals("HasChanges", false, clone.HasChanges);
		}

		public void TestDeleteNew()
		{
			AssertEquals(false, Dummy.IsDeleted);
			Dummy.Delete();
			AssertEquals(true, Dummy.IsDeleted);

			BusinessObject newDummy = Factory.New(typeof(DummyBusinessObject));
			Factory.Save();

			AssertEquals(false, newDummy.IsDeleted);
			newDummy.Delete();
			AssertEquals(true, newDummy.IsDeleted);
		}

		public void TestDeleteLoaded()
		{
			Factory.Save();

			BusinessObject loadedDummy = Factory.Load(typeof(DummyBusinessObject), Dummy.PK);
			BusinessObject loadedDummyBase = Factory.Load(typeof(DummyBaseBusinessObject), Dummy.PK);

			AssertEquals(false, loadedDummy.IsDeleted);
			AssertEquals(false, loadedDummyBase.IsDeleted);

			loadedDummy.Delete();

			AssertEquals(true, loadedDummy.IsDeleted);
			AssertEquals(true, loadedDummyBase.IsDeleted);
		}

		public void TestOnLoaded()
		{
			DummyBusinessObject newDummy = Factory.New<DummyBusinessObject>();
			AssertEquals("New, not loaded", false, newDummy.OnLoadedCalled);
			Factory.Save();

			DummyBusinessObject loadedDummy = new BusinessObjectFactory().Load<DummyBusinessObject>(newDummy.PK);
			AssertEquals("Loaded", true, loadedDummy.OnLoadedCalled);
		}

		public void TestOnSaved()
		{
			AssertEquals("In test case transaction only", 1, CargoWise.Data.Db.Connection.AppTransactionCount);

			DummyBusinessObject newDummy = Factory.New<DummyBusinessObject>();
			AssertEquals("Saved not called yet", false, newDummy.OnSavedCalled);

			Factory.Save();
			AssertEquals("Should have OnSavedCalled", true, newDummy.OnSavedCalled);

			AssertEquals("In test case transaction only", 1, CargoWise.Data.Db.Connection.AppTransactionCount);
		}

		public void TestOnSaving()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			AssertEquals("In test case transaction only", 1, CargoWise.Data.Db.Connection.AppTransactionCount);

			DummyBusinessObject newDummy = factory.New<DummyBusinessObject>();
			AssertEquals("OnSavingCalled not called yet", false, newDummy.OnSavingCalled);

			factory.Save();
			AssertEquals("Should have OnSavingCalled", true, newDummy.OnSavingCalled);

			AssertEquals("Should be in save transaction", 2, newDummy.OnSavingOpenTransactionCount);
			AssertEquals("In test case transaction only", 1, CargoWise.Data.Db.Connection.AppTransactionCount);
		}

		public void TestOnSavingForDelete()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			AssertEquals("OnSavingCalled not called yet", false, dummy.OnSavingCalled);
			AssertEquals("OnSavingCount should be 0.", 0, dummy.OnSavingCount);
			AssertEquals("OnSavingForDeleteCalled should be 0.", 0, dummy.OnSavingForDeleteCount);

			dummy.Z0_AnotherDate = new ZDateTime(2023, 1, 1);
			factory.Save();
			AssertEquals("Should have OnSavingCalled", true, dummy.OnSavingCalled);
			AssertEquals("OnSavingCount should be 1.", 1, dummy.OnSavingCount);
			AssertEquals("OnSavingForDeleteCount should be 0.", 0, dummy.OnSavingForDeleteCount);

			dummy.Z0_AnotherDate = new ZDateTime(2023, 1, 2);
			dummy.Delete();
			factory.Save();
			AssertEquals("OnSavingCount should be 1.", 1, dummy.OnSavingCount);
			AssertEquals("OnSavingForDeleteCount should be 1.", 1, dummy.OnSavingForDeleteCount);
		}

		public void TestOnSavedForDeletedObject()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			AssertEquals("OnSavedCalled not called yet", false, dummy.OnSavedCalled);
			AssertEquals("OnSavedForDeletedObjectCount should be 0.", 0, dummy.OnSavedForDeletedObjectCount);

			dummy.Z0_AnotherDate = new ZDateTime(2023, 1, 1);
			factory.Save();
			AssertEquals("Should have OnSavedCalled", true, dummy.OnSavedCalled);
			AssertEquals("OnSavedForDeletedObjectCount should be 0.", 0, dummy.OnSavedForDeletedObjectCount);

			dummy.Z0_AnotherDate = new ZDateTime(2023, 1, 2);
			dummy.Delete();
			factory.Save();
			AssertEquals("OnSavedForDeletedObjectCount should be 1.", 1, dummy.OnSavedForDeletedObjectCount);
		}

		public void TestOnSavingGetsCalledOnceOnly()
		{
			DummyBusinessObject newDummy = Factory.New<DummyBusinessObject>();
			AssertEquals("PreCondition", 0, newDummy.OnSavingCount);
			Factory.Save();
			AssertEquals("PreCondition", 1, newDummy.OnSavingCount);
			Factory.Save();
			AssertEquals("PreCondition", 1, newDummy.OnSavingCount);
			newDummy.Z0_AnotherDate = new ZDateTime(2023, 1, 1);
			Factory.Save();
			AssertEquals("PreCondition", 2, newDummy.OnSavingCount);
			newDummy.Z0_AnotherDate = new ZDateTime(2023, 1, 2);
			newDummy.OnSavingInternal();
			Factory.Save();
			AssertEquals("PreCondition", 3, newDummy.OnSavingCount);
		}

		public void TestBeforeUpdatedByDataRefresh()
		{
			DummyWithDependentsBusinessObject dummy1 = Factory.New<DummyWithDependentsBusinessObject>();
			dummy1.Z0_Code = "TST";

			DummyDependantBusinessObject dependent1 = dummy1.Dependents.AddNew();
			dependent1.ZD1_Code = "CDE";

			dummy1.BeforeUpdatedByDataRefresh += new EventHandler(Dummy1_BeforeUpdatedByDataRefresh);

			BeforeUpdatedByDataRefresh = false;
			Factory.Save();
			AssertEquals("BeforeUpdatedByDataRefresh", false, BeforeUpdatedByDataRefresh);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyWithDependentsBusinessObject dummy2 = factory2.Load<DummyWithDependentsBusinessObject>(dummy1.PK);
			dummy2.Z0_Description = "changed";

			BeforeUpdatedByDataRefresh = false;
			factory2.Save();
			AssertEquals("BeforeUpdatedByDataRefresh", true, BeforeUpdatedByDataRefresh);
		}

		public void TestOnUpdatedByDataRefresh()
		{
			DummyWithDependentsBusinessObject dummy1 = Factory.New<DummyWithDependentsBusinessObject>();
			dummy1.Z0_Code = "TST";

			DummyDependantBusinessObject dependent1 = dummy1.Dependents.AddNew();
			dependent1.ZD1_Code = "CDE";

			dummy1.UpdatedByDataRefresh += new EventHandler(Dummy1_UpdatedByDataRefresh);

			UpdatedByDataRefresh = false;
			Factory.Save();
			AssertEquals("OnUpdatedByDataRefresh", false, UpdatedByDataRefresh);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyWithDependentsBusinessObject dummy2 = factory2.Load<DummyWithDependentsBusinessObject>(dummy1.PK);
			dummy2.Z0_Description = "changed";

			UpdatedByDataRefresh = false;
			factory2.Save();
			AssertEquals("OnUpdatedByDataRefresh", true, UpdatedByDataRefresh);
		}

		public void TestOnDeletedByDataRefresh()
		{
			DummyBaseBusinessObject dummy1 = Factory.New<DummyBaseBusinessObject>();
			dummy1.Z0_Code = "TST";
			dummy1.DeletedByDataRefresh += new EventHandler(Dummy1_DeletedByDataRefresh);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBaseBusinessObject dummy2 = factory2.Load<DummyBaseBusinessObject>(dummy1.PK);

			DeletedByDataRefresh = false;
			dummy2.Delete();
			factory2.Save();
			AssertEquals("OnDeletedByDataRefresh", true, DeletedByDataRefresh);
		}

		public void TestUpdatedByDataRefreshIncludingChildren()
		{
			DummyWithDependentsBusinessObject dummy1 = Factory.New<DummyWithDependentsBusinessObject>();
			dummy1.Z0_Code = "TST";

			DummyDependantBusinessObject dependent1 = dummy1.Dependents.AddNew();
			dependent1.ZD1_Code = "CDE";

#pragma warning disable
			((IBusiness)dummy1).UpdatedByDataRefreshIncludingChildren += new EventHandler(Dummy1_UpdatedByDataRefresh);
#pragma warning restore

			UpdatedByDataRefresh = false;
			Factory.Save();
			AssertEquals("UpdatedByDataRefreshIncludingChildren event", false, UpdatedByDataRefresh);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyWithDependentsBusinessObject dummy2 = factory2.Load<DummyWithDependentsBusinessObject>(dummy1.PK);
			dummy2.Z0_Description = "changed";

			UpdatedByDataRefresh = false;
			factory2.Save();
			AssertEquals("UpdatedByDataRefreshIncludingChildren event", true, UpdatedByDataRefresh);

			//test to see if child objects will also cause UpdatedByDataRefresh to fire for Parent.
			//it should still fire as the parent's HasChanges will be true when child's HasChanges is set to true
			dummy2.Dependents[0].ZD1_Code = "CHG";
			UpdatedByDataRefresh = false;
			factory2.Save();
			AssertEquals("UpdatedByDataRefreshIncludingChildren event", true, UpdatedByDataRefresh);

			//Test to see when the child object is updated separately, the parent object's datarefresh should still be called.
			//this is done through the UpdatedByDataRefresh event on ISupportObjectState being implemented by both BusinessObject
			//and BusinessObject Collection.
			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			DummyDependantBusinessObject sameDependent = factory3.Load<DummyDependantBusinessObject>(dependent1.PK);
			sameDependent.ZD1_Code = "CH2";

			UpdatedByDataRefresh = false;
			factory3.Save();
			AssertEquals("UpdatedByDataRefreshIncludingChildren event", true, UpdatedByDataRefresh);
		}

		public void TestFetchStrategyIsNotCached()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			IBusinessObjectFetchStrategy fetch1 = bizO.FetchStrategy;
			IBusinessObjectFetchStrategy fetch2 = bizO.FetchStrategy;
			AssertEquals(fetch1.GetType(), fetch2.GetType());
			AssertNotEquals(fetch2, fetch1);
		}

		public void TestDeletedObjectDoesNotRunFetchValidation()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			BusinessObjectFetchStrategyForTest strategy = (BusinessObjectFetchStrategyForTest)bizO.FetchStrategy;
			AssertEquals(0, strategy.FetchForValidateCoreCount);

			((IBusiness)bizO).RunPreSaveValidationFetch(false);
			AssertEquals(1, strategy.FetchForValidateCoreCount);

			bizO.Delete();
			((IBusiness)bizO).RunPreSaveValidationFetch(false);
			AssertEquals(1, strategy.FetchForValidateCoreCount);
		}

		public void TestRunPreSaveValidationFetchCallsChildObjects()
		{
			DummyBusinessObject bizO1 = DummyBusinessObject.New(Factory);
			BusinessObject dep1 = Factory.New(typeof(DummyDependantBusinessObject));
			bizO1.RegisterEditableChildObject(dep1);

			BusinessObjectFetchStrategyForTest dep1Fetch = (BusinessObjectFetchStrategyForTest)dep1.FetchStrategy;
			BusinessObjectFetchStrategyForTest bizO1Fetch = (BusinessObjectFetchStrategyForTest)bizO1.FetchStrategy;
			AssertEquals(0, dep1Fetch.FetchForValidateCoreCount);
			AssertEquals(0, bizO1Fetch.FetchForValidateCoreCount);

			((IBusiness)bizO1).RunPreSaveValidationFetch(false);
			bizO1.RunPreSaveValidation();
			AssertEquals(1, bizO1Fetch.FetchForValidateCoreCount);
			AssertEquals(1, dep1Fetch.FetchForValidateCoreCount);
		}

		public void TestRunPreSaveValidationFetchTrue()
		{
			IBusiness bizO = DummyBusinessObject.New(Factory);
			Factory.AddFetchHint(DummyBizoSchema.Z0_Code, (ZString)"HELLO");
			AssertEquals(1, Factory.ActiveTableFetchHints);
			bizO.RunPreSaveValidationFetch(true);
			AssertEquals(0, Factory.ActiveTableFetchHints);
		}

		public void TestRunPreSaveValidationFetchFalse()
		{
			IBusiness bizO = DummyBusinessObject.New(Factory);
			Factory.AddFetchHint(DummyBizoSchema.Z0_Code, (ZString)"HELLO");
			AssertEquals(1, Factory.ActiveTableFetchHints);
			bizO.RunPreSaveValidationFetch(false);
			AssertEquals(1, Factory.ActiveTableFetchHints);
		}

		bool UpdatedByDataRefresh;

		void Dummy1_UpdatedByDataRefresh(object sender, EventArgs e)
		{
			UpdatedByDataRefresh = true;
		}

		bool BeforeUpdatedByDataRefresh;

		void Dummy1_BeforeUpdatedByDataRefresh(object sender, EventArgs e)
		{
			BeforeUpdatedByDataRefresh = true;
		}

		bool DeletedByDataRefresh;

		void Dummy1_DeletedByDataRefresh(object sender, EventArgs e)
		{
			DeletedByDataRefresh = true;
		}

		public void TestGetZPropertyInfoOverload()
		{
			ZPropertyInfo zStringInfoUsingOverload = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Code);
			ZPropertyInfo zStringInfoWithoutOverload = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Code);
			AssertEquals(zStringInfoWithoutOverload, zStringInfoUsingOverload);
		}

		public void TestGetZPropertyInfoWithHumanReadableName()
		{
			ZPropertyInfo info;

			info = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Code);
			AssertEquals("Human readable name not set", "Code", info.HumanReadableName);

			info = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Code, "Bob");
			AssertEquals("Human readable name passed in", "Bob", info.HumanReadableName);

			info = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Description);
			AssertEquals("Only length passed in, not Human readable name", "Description", info.HumanReadableName);

			info = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Description, "Fread");
			AssertEquals("Both length and human readable name passed in", "Fread", info.HumanReadableName);
		}

		public void TestGetTypedZPropertyInfo()
		{
			ZPropertyInfo zStringInfo = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Code);
			ZPropertyInfo zTimeInfo = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Time);
			ZPropertyInfo zDateTimeInfo = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Date);
			ZPropertyInfo zDateTimeOffsetInfo = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_DateTimeOffset);
			ZPropertyInfo zGeographyInfo = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Geography);
			ZPropertyInfo zGuidInfo = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Guid);
			ZPropertyInfo zBlobInfo = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_VarBinaryMax);
			ZPropertyInfo zBoolInfo = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Bool);
			ZPropertyInfo zDecimalInfo = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Decimal);
			ZPropertyInfo zIntInfo = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Number);
			ZPropertyInfo zShortInfo = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Short);
			ZPropertyInfo zByteInfo = Dummy.GetZPropertyInfo(AutoDummyBizo.Schema.Z0_Byte);

			Assert("Should have created a ZPropertyInfoString for ZString property.", zStringInfo is ZPropertyInfoString);
			Assert("Should have created a ZPropertyInfoDateTime for ZDateTime property.", zDateTimeInfo is ZPropertyInfoDateTime);
			Assert("Should have created a ZPropertyInfoTime for ZTime property.", zTimeInfo is ZPropertyInfoTime);
			Assert("Should have created a ZPropertyInfoDateTimeOffset for ZDateTimeOffset property.", zDateTimeOffsetInfo is ZPropertyInfoDateTimeOffset);
			Assert("Should have created a ZPropertyInfoGeography for ZGeography property.", zGeographyInfo is ZPropertyInfoGeography);
			Assert("Should have created a ZPropertyInfoGuid for ZGuid property.", zGuidInfo is ZPropertyInfoGuid);
			Assert("Should have created a ZPropertyInfoBlob for ZBlob property.", zBlobInfo is ZPropertyInfoBlob);
			Assert("Should have created a ZPropertyInfoBool for ZBool property.", zBoolInfo is ZPropertyInfoBool);
			Assert("Should have created a ZPropertyInfoDecimal for ZDecimal property.", zDecimalInfo is ZPropertyInfoDecimal);
			Assert("Should have created a ZPropertyInfoInt for ZInt property.", zIntInfo is ZPropertyInfoInt);
			Assert("Should have created a ZPropertyInfoShort for ZShort property.", zShortInfo is ZPropertyInfoShort);
			Assert("Should have created a ZPropertyInfoByte for ZByte property.", zByteInfo is ZPropertyInfoByte);
		}

		public void TestGetDateTypeZPropertyInfo()
		{
			var dateInfo = Dummy.GetZPropertyInfo(DummyBizoSchema.Constants.Z0_DateOnly);
			Assert("Should have created a ZPropertyInfoDate for dateInfo property.", dateInfo is ZPropertyInfoDate);
		}

		public void TestHasChangesTrueWhenDeleted()
		{
			Factory.Save();
			AssertEquals("HasChanges", false, Dummy.HasChanges);
			Dummy.Delete();
			AssertEquals("HasChanges", true, Dummy.HasChanges);
		}

		class DummyWhosePropertyValueCanBeReturnedInRowDeletedErrorReport : DummyBusinessObject
		{
			public DummyWhosePropertyValueCanBeReturnedInRowDeletedErrorReport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool ShouldIncludePropertyValueInRowDeletedError(string propertyName) => true;
		}

		public void TestPropertyValueCanStillBeReturnedEvenWhenObjectIsDeleted()
		{
			Dummy.Z0_Code = "ABC";
			Factory.Save();

			Dummy.Delete();
			AssertEquals("Z0_Code", "ABC", Dummy.Z0_Code);
			AssertContains("Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
			AssertContains("Property name: Z0_Code", ErrorReporter.LastMessageReported);
			AssertNotContains("Property value: ABC", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestPropertyValueForDeletedObjectIsReportedInErrorReportWhenAllowed()
		{
			var dummy = Factory.New<DummyWhosePropertyValueCanBeReturnedInRowDeletedErrorReport>();
			dummy.Z0_Code = "ABC";
			Factory.Save();

			dummy.Delete();
			AssertEquals("Z0_Code", "ABC", dummy.Z0_Code);
			AssertContains("Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
			AssertContains("Property name: Z0_Code", ErrorReporter.LastMessageReported);
			AssertContains("Property value: ABC", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestPropertyValueCanStillBeReturnedAfterDeleteByDataRefresh()
		{
			ErrorReporter.Clear();
			Dummy.Z0_Code = "ABC";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummyFactory2 = factory2.Load<DummyBusinessObject>(Dummy.PK);

			Dummy.Delete();
			Factory.Save();

			AssertEquals("DummyCopy.IsDelete", true, dummyFactory2.IsDeleted);
			AssertEquals("DummyCopy AcceptChangesDelayedUntilJustBeforeSavingToDatabase", true, ((IBusinessObjectInternals)dummyFactory2).AcceptChangesDelayedUntilJustBeforeSavingToDatabase);
			AssertEquals("Developer Error", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			AssertEquals("Z0_Code", "ABC", dummyFactory2.Z0_Code);
			Assert("Developer Error", ErrorReporter.LastMessageReported.Length > 10);
			ErrorReporter.Clear();

			factory2.Save();

			AssertEquals("DummyCopy Row.RowState", DataRowState.Detached, dummyFactory2.Row.RowState);
			AssertEquals("DummyCopy RowState via IsUnCommittedRow", true, ((IBusinessObjectInternals)dummyFactory2).IsUnCommittedRow);
			AssertEquals("DummyCopy AcceptChangesDelayedUntilJustBeforeSavingToDatabase", false, ((IBusinessObjectInternals)dummyFactory2).AcceptChangesDelayedUntilJustBeforeSavingToDatabase);
		}

		public void TestCollectionHasChangesShouldNotChangeAfterDataRefreshBusDeletesBusinessObject()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject aDummy = factory1.New<DummyBusinessObject>();
			aDummy.Z0_Code = "ABC";
			factory1.Save();

			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			collection.IsManagedForDataRefresh = true;
			collection.Load();

			AssertEquals("Count", 1, collection.Count);

			aDummy.Delete();
			factory1.Save();

			AssertEquals("Count", 0, collection.Count);
			AssertEquals("Collection.HasChanges should not change as a result of datarefresh bus publish", false, collection.HasChanges);
		}

		public void TestCollectionHasChangesShouldNotChangeAfterDataRefreshBusAddsNewObjectInIt()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			collection.IsManagedForDataRefresh = true;
			collection.Load();
			AssertEquals("Count", 0, collection.Count);

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject aDummy = factory1.New<DummyBusinessObject>();
			aDummy.Z0_Code = "ABC";
			factory1.Save();

			AssertEquals("Count", 1, collection.Count);
			AssertEquals("Collection.HasChanges should not change as a result of datarefresh bus publish", false, collection.HasChanges);
		}

		#region TestIsDeletedNotSetUntilAfterBeforeDeleteSuccessfulCalled

		public void TestIsDeletedNotSetUntilAfterBeforeDeleteSuccessfulCalled()
		{
			DummyBusinessObject aDummy = Factory.New<DummyBusinessObject>();
			aDummy.Z0_Code = "ABC";
			Factory.Save();

			ADummyIsDeleted = false;

			aDummy.OnBeforeSuccessfulDelete += new EventHandler(ADummy_OnBeforeSuccessfulDelete);

			aDummy.Delete();
			AssertEquals("IsDeletedBeforeSuccessfulDelete", false, ADummyIsDeleted);

			AssertEquals("IsDeleted", true, aDummy.IsDeleted);
		}

		void ADummy_OnBeforeSuccessfulDelete(object sender, EventArgs e)
		{
			ADummyIsDeleted = ((DummyBusinessObject)sender).IsDeleted;
		}

		bool ADummyIsDeleted;

		#endregion

		public void TestDeleteStackOverflow()
		{
			// This tests a situation found in the ConsolShipmentCollection
			TestOverflowDummyCollection testCollection = new TestOverflowDummyCollection(Factory);
			TestOverflowDummy testDummy = Factory.New<TestOverflowDummy>();
			testCollection.Add(testDummy);
			AssertEquals("Dummy should be in collection", 1, testCollection.Count);

			testDummy.Delete();
			AssertEquals("Collection should be empty", 0, testCollection.Count);
			AssertEquals("Dummy should be deleted", true, testDummy.IsDeleted);
		}

		public void TestTwoDifferentTypeNullObjectsReturnFalse()
		{
			BusinessObject dummyBizO = Factory.New(typeof(DummyBusinessObject));
			BusinessObject dummyChildBizO = Factory.New(typeof(DummyChildBusinessObject));

#pragma warning disable 1718
			Assert(dummyBizO == dummyBizO);
			Assert(dummyChildBizO == dummyChildBizO);
#pragma warning restore 1718

			Assert(!(dummyBizO == dummyChildBizO));
			dummyBizO.IsNull = true;
			Assert(!(dummyBizO == dummyChildBizO));
			dummyChildBizO.IsNull = true;
			Assert(!(dummyBizO == dummyChildBizO));

			Assert(null == dummyChildBizO);

			Assert("Unfortunate, but we'll have to work with this", ((object)dummyChildBizO) != null);
		}

		public void TestNullObjectIsNotSaved()
		{
			BusinessObject bizO = Factory.GetNull(typeof(DummyBusinessObject));
			AssertEquals("Precondition", false, bizO.IsSavedByFactory);
			Factory.Save();
			AssertEquals("InDatabase", false, bizO.IsInDatabase);
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BusinessObject bizOInSecondFactory = factory2.Load(typeof(DummyBusinessObject), bizO.PK);
			Assert(bizOInSecondFactory == null);
		}

		public void TestNullObjectCannotBeDeleted()
		{
			BusinessObject bizO = Factory.GetNull(typeof(DummyBusinessObject));
			bizO.Delete();
			AssertEquals(false, bizO.IsDeleted);
			AssertEquals("Developer error reported", true, ErrorReporter.LastMessageReported.Contains("Null object cannot be deleted"));
			ErrorReporter.Clear();
		}

		public void TestIsNullComparedToNullReturnsTrue()
		{
			BusinessObject bizO = Factory.New(typeof(DummyBusinessObject));
			Assert(!(bizO == null));
			Assert(!(bizO == null));
			bizO.IsNull = true;
			Assert(bizO == null);
			Assert(bizO == null);
		}

		public void TestMatchesFilterForNonDBOnlyQuery()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "DESC";

			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Description = "DES";

			Assert("Match", dummy1.MatchesFilter(ZQuery.EmptyQuery));
			Assert("Match", dummy1.MatchesFilter(new ZQuery(DummyBizoSchema.Z0_Description, "DESC")));
			Assert("NoMatch", !dummy1.MatchesFilter(new ZQuery(DummyBizoSchema.Z0_Description, "DES")));

			Assert("Match", dummy2.MatchesFilter(ZQuery.EmptyQuery));
			Assert("NoMatch", !dummy2.MatchesFilter(new ZQuery(DummyBizoSchema.Z0_Description, "DESC")));
			Assert("Match", dummy2.MatchesFilter(new ZQuery(DummyBizoSchema.Z0_Description, "DES")));

			// Test caches
			Assert("Match", dummy1.MatchesFilter(new ZQuery(DummyBizoSchema.Z0_Description, "DESC")));
			Assert("NoMatch", !dummy1.MatchesFilter(new ZQuery(DummyBizoSchema.Z0_Description, "DES")));
			Assert("Match", dummy2.MatchesFilter(new ZQuery(DummyBizoSchema.Z0_Description, "DES")));

			Factory.ResetDatabaseLoadCount();
			dummy1.MatchesFilter(new ZQuery(DummyBizoSchema.Z0_Description, "DES"));
			AssertEquals("MatchesFilter should not hit the database a second time", 0, Factory.DatabaseLoadCount);
		}

		public void TestMatchesFilterForNonDBOnlyQuery_Deleted()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "DESC";

			AssertEquals("Precondition: Should match", true, dummy1.MatchesFilter(new ZQuery(DummyBizoSchema.Z0_Description, "DESC")));

			dummy1.Delete();
			AssertEquals("Should not match.", false, dummy1.MatchesFilter(new ZQuery(DummyBizoSchema.Z0_Description, "DESC")));
		}

		public void TestMatchesFilterForNonDBOnlyQuery_Detached()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "DESC";

			AssertEquals("Precondition: Should match", true, dummy1.MatchesFilter(new ZQuery(DummyBizoSchema.Z0_Description, "DESC")));

			dummy1.Row.Delete();
			AssertEquals("Should not match.", false, dummy1.MatchesFilter(new ZQuery(DummyBizoSchema.Z0_Description, "DESC")));
		}

		public void TestMatchesFilterForNonDBOnlyQuery_NoResultQuery()
			=> TestMatchesFilterForNonDBOnlyQuery_NoResultQuery(withIdentifier: false);

		public void TestMatchesFilterForNonDBOnlyQuery_NoResultQuery_WithIdentifier()
			=> TestMatchesFilterForNonDBOnlyQuery_NoResultQuery(withIdentifier: false);

		void TestMatchesFilterForNonDBOnlyQuery_NoResultQuery(bool withIdentifier)
		{
			var identifier = withIdentifier ? "KEY" : null;
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "DESC";

			var query = new ZQuery(DummyBizoSchema.Z0_Description, "DESC");
			var noResultQuery = new ZQuery(DummyBizoSchema.Z0_Description, "DESC");
			noResultQuery.IsNoResultQuery = true;

			AssertEquals("Should *not* match", false, dummy1.MatchesFilter(noResultQuery, identifier));
			AssertEquals("Should match", true, dummy1.MatchesFilter(query, identifier));

			// Test caches
			AssertEquals("Should *not* match", false, dummy1.MatchesFilter(noResultQuery, identifier));
			AssertEquals("Should match", true, dummy1.MatchesFilter(query, identifier));
		}

		public void TestMatchesFilterForNonDBOnlyQuery_LargeQuery()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "DESC";

			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Description = "DES";

			var query = new ZQuery(DummyBizoSchema.Z0_Description, "DESC");
			query.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, new string('a', 5000));

			AssertEquals("Should match.", true, dummy1.MatchesFilter(query));
			AssertEquals("Should *not* match.", false, dummy2.MatchesFilter(query));

			// Test caches
			AssertEquals("Should match.", true, dummy1.MatchesFilter(query));
			AssertEquals("Should *not* match.", false, dummy2.MatchesFilter(query));
		}

		public void TestMatchesFilterForNonDBOnlyQuery_Identifier()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "DESC";

			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Description = "DES";

			var query1 = new ZQuery(DummyBizoSchema.Z0_Description, "DESC");
			var query2 = new ZQuery(DummyBizoSchema.Z0_Description, "DES");

			AssertEquals("Should match.", true, dummy1.MatchesFilter(query1, "KEY1"));
			AssertEquals("Should *not* match.", false, dummy2.MatchesFilter(query1, "KEY1"));

			AssertEquals("Should *not* match.", false, dummy1.MatchesFilter(query2, "KEY2"));
			AssertEquals("Should match.", true, dummy2.MatchesFilter(query2, "KEY2"));

			AssertEquals("Should match based on the identifier.", true, dummy1.MatchesFilter(query2, "KEY1"));
			AssertEquals("Should match based on the identifier.", true, dummy2.MatchesFilter(query1, "KEY2"));

			AssertEquals("Should *not* match based on the identifier.", false, dummy1.MatchesFilter(query1, "KEY2"));
			AssertEquals("Should *not* match based on the identifier.", false, dummy2.MatchesFilter(query2, "KEY1"));
		}

		public void TestMatchesFilterForNonDBOnlyQuery_Identifier_LargeQuery()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "DESC";

			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Description = "DES";

			var query1 = new ZQuery(DummyBizoSchema.Z0_Description, "DESC");
			query1.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, new string('a', 5000));

			var query2 = new ZQuery(DummyBizoSchema.Z0_Description, "DES");
			query2.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, new string('a', 5000));

			AssertEquals("Should match.", true, dummy1.MatchesFilter(query1, "KEY1"));
			AssertEquals("Should *not* match.", false, dummy2.MatchesFilter(query1, "KEY1"));

			AssertEquals("Should *not* match.", false, dummy1.MatchesFilter(query2, "KEY2"));
			AssertEquals("Should match.", true, dummy2.MatchesFilter(query2, "KEY2"));

			AssertEquals("Should match based on the identifier.", true, dummy1.MatchesFilter(query2, "KEY1"));
			AssertEquals("Should match based on the identifier.", true, dummy2.MatchesFilter(query1, "KEY2"));

			AssertEquals("Should *not* match based on the identifier.", false, dummy1.MatchesFilter(query1, "KEY2"));
			AssertEquals("Should *not* match based on the identifier.", false, dummy2.MatchesFilter(query2, "KEY1"));
		}

		public void TestMatchesFilterOnDBOnlyQuery()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "DESC1";
			DummyBusinessObject otherDummy = Factory.New<DummyBusinessObject>();
			otherDummy.Z0_Description = "DESC2";
			Factory.Save();
			ZQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_Description, "DESC2");
			AssertEquals(false, dummy.MatchesFilter(query));
			AssertEquals(true, otherDummy.MatchesFilter(query));

			Factory.ResetDatabaseLoadCount();
			dummy.MatchesFilter(query);
			AssertEquals("MatchesFilter should not hit the database a second time", 0, Factory.DatabaseLoadCount);
		}

		public void TestMatchesFilterOnDBOnlyQueryWithPreloadedData()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "DESC1";
			dummy1.Z0_Number = 10;

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Description = "DESC2";
			dummy1.Z0_Number = 20;

			Factory.Save();

			ZQuery subQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			subQuery.AddToFilter(DummyBizoSchema.Z0_Description, "DESC2");

			ZQuery bigQuery = new ZQuery();
			bigQuery.AddToFilter(subQuery);
			bigQuery.AddToFilter(DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThanOrEqualTo, 0);

			AssertEquals(1, Factory.Load<DummyBusinessObject>(bigQuery).Length);

			Factory.ResetDatabaseLoadCount();

			Assert(dummy2.MatchesFilter(subQuery));
			AssertEquals("MatchesFilter should not hit the database as 'DESC2' was already met in other query", 0, Factory.DatabaseLoadCount);

			Assert(!dummy1.MatchesFilter(subQuery));
			AssertEquals("DB should be hittes as 'DESC1' was not used in any query before", 1, Factory.DatabaseLoadCount);
		}

		public void TestMatchesFilterOnDBOnlySubQueryWithPreloadedData()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "DESC";
			var dependantBizObj1A = Factory.New<DummyDependantBusinessObject>();
			dependantBizObj1A.ZD1_Z0 = dummy.PK;

			Factory.Save();

			var hasDependantAndDescQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			hasDependantAndDescQuery.AddToFilter(DummyBizoSchema.Z0_Description, "DESC");
			hasDependantAndDescQuery.AddSubQuery(new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0), JoinCondition.And);

			AssertContainsExactElementsInAnyOrder(new[] { dummy }, Factory.Load<DummyBusinessObject>(hasDependantAndDescQuery));  // load the cache

			Factory.ResetDatabaseLoadCount();

			Assert("Should match as dummy has a dependant biz obj", dummy.MatchesFilter(hasDependantAndDescQuery));
			AssertEquals("MatchesFilter should not hit the database", 0, Factory.DatabaseLoadCount);

			var hasDependantQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			hasDependantQuery.AddSubQuery(new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0), JoinCondition.And);

			Assert("Should match as dummy has a dependant biz obj", dummy.MatchesFilter(hasDependantQuery));
			AssertEquals("MatchesFilter should not hit the database as 'has dependant biz obj' was already met in other query", 0, Factory.DatabaseLoadCount);
		}

		public void TestMatchesFilterOnDBOnlySubQueryWithPreloadedData_WhenUsingBothInAndNotInVariations()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "DESC";
			var dependantBizObj1A = Factory.New<DummyDependantBusinessObject>();
			dependantBizObj1A.ZD1_Z0 = dummy.PK;

			Factory.Save();

			var hasDependantQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			hasDependantQuery.AddToFilter(DummyBizoSchema.Z0_Description, "DESC");
			hasDependantQuery.AddSubQuery(new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0), JoinCondition.And);

			AssertContainsExactElementsInAnyOrder(new[] { dummy }, Factory.Load<DummyBusinessObject>(hasDependantQuery)); // load the cache

			Assert("Should match as dummy has a dependant biz obj", dummy.MatchesFilter(hasDependantQuery));

			var doesNotHaveDependantQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			doesNotHaveDependantQuery.AddToFilter(DummyBizoSchema.Z0_Description, "DESC");
			doesNotHaveDependantQuery.AddSubQuery(new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0, true), JoinCondition.And);

			Assert("Should NOT match as dummy has a dependant biz obj", !dummy.MatchesFilter(doesNotHaveDependantQuery));
		}

		public void TestSettingWarningValidationReportsDeveloperErrorOnNullObject()
		{
			DummyBusinessObject nullDummy = Factory.GetNull<DummyBusinessObject>();
			nullDummy.Z0_ByteInfo.AddWarning("Should throw developer error");
			AssertEquals("NullObjectPropertyCountChanged", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestSettingMessageErrorValidationReportsDeveloperErrorOnNullObject()
		{
			DummyBusinessObject nullDummy = Factory.GetNull<DummyBusinessObject>();
			nullDummy.Z0_ByteInfo.AddMessageError("Should throw developer error");
			AssertEquals("NullObjectPropertyCountChanged", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestSettingErrorValidationReportsDeveloperErrorOnNullObject()
		{
			DummyBusinessObject nullDummy = Factory.GetNull<DummyBusinessObject>();
			nullDummy.Z0_ByteInfo.AddError("Should throw developer error");
			AssertEquals("NullObjectPropertyCountChanged", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestPreSaveValidationNotCalledOnNullObject()
		{
			DummyBusinessObject nullDummy = Factory.GetNull<DummyBusinessObject>();
			Factory.Save();
			AssertEquals(0, nullDummy.RunPreSaveValidationCount);
		}

		public void TestIsSavedByFactoryOnNullObject()
		{
			BusinessObject dummy = Factory.New(typeof(DummyBusinessObject));
			AssertEquals("Precondition", true, dummy.IsSavedByFactory);

			BusinessObject nullDummy = Factory.GetNull(typeof(DummyBusinessObject));
			AssertEquals(false, nullDummy.IsSavedByFactory);
		}

		[ExpectException(typeof(CannotDeleteException))]
		public void TestRunDeleteCheckers()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.SetDeleteCheckers(new DummyDeleteChecker());
			dummy.RunDeleteCheckers();
		}

		public void TestRegisterEditableChildObject_ChildHasChanges_NotifyObjectHasChanges()
		{
			var parent = Factory.New<DummyBusinessObject>();
			parent.HasChanges = false;

			var child = Factory.New<DummyBusinessObject>();
			child.HasChanges = true;

			HasChangesChangedEventArgs eventArgs = null;
			parent.HasChangesChanged += (s, e) =>
			{
				AssertNull("HasChangesChanged event is expected to be raised once", eventArgs);

				eventArgs = e;
			};

			parent.RegisterEditableChildObject(child);

			Assert("HasChangesChanged event is raised", eventArgs != null);
			AssertEquals("Object just was changed", true, eventArgs.ObjectJustWasChanged);
			AssertEquals("ChangeInEditableChildCollection", true, eventArgs.ChangeInEditableChildCollection);
		}

		public void TestRegisterEditableChildObject_ChildHasChangesAndFactoryIsSaving_NotifyObjectHasNotChanges()
		{
			var parent = Factory.New<DummyBusinessObject>();
			parent.HasChanges = false;

			var child = Factory.New<DummyBusinessObject>();

			HasChangesChangedEventArgs eventArgs = null;
			parent.HasChangesChanged += (s, e) =>
			{
				AssertNull("HasChangesChanged event is expected to be raised once", eventArgs);

				eventArgs = e;
			};

			Factory.Saved += (s, e) =>
			{
				child.HasChanges = true;
				parent.RegisterEditableChildObject(child);
			};

			Factory.Save();

			Assert("HasChangesChanged event is raised", eventArgs != null);
			AssertEquals("Object was not just changed", false, eventArgs.ObjectJustWasChanged);
			AssertEquals("ChangeInEditableChildCollection", true, eventArgs.ChangeInEditableChildCollection);
		}

		public void TestUnRegisterEditableChildObject_NotifyObjectHasNotChanges()
		{
			var child = Factory.New<DummyBusinessObject>();
			AssertEquals("Registered", false, Dummy.IsRegisteredEditableChildObject(child));
			AssertEquals("HasChanges", false, Dummy.HasChanges);

			child.HasChanges = true;
			Dummy.RegisterEditableChildObject(child);
			AssertEquals("Registered", true, Dummy.IsRegisteredEditableChildObject(child));
			AssertEquals("HasChanges", true, Dummy.HasChanges);

			HasChangesChangedEventArgs eventArgs = null;
			Dummy.HasChangesChanged += (s, e) =>
			{
				AssertNull("HasChangesChanged event is expected to be raised once", eventArgs);
				eventArgs = e;
			};

			Dummy.UnRegisterEditableChildObject(child);

			Assert("HasChangesChanged event is raised", eventArgs != null);
			AssertEquals("Object was not just changed", false, eventArgs.ObjectJustWasChanged);
			AssertEquals("ChangeInEditableChildCollection", true, eventArgs.ChangeInEditableChildCollection);
		}

		public void TestBusinessObjectCollectionLoad_ChildHasChanges_NotifyObjectHasNoChanges()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var child = Factory.New<DummyBusinessObject>();

			HasChangesChangedEventArgs eventArgs = null;
			collection.HasChangesChanged += (s, e) =>
			{
				AssertNull("HasChangesChanged event is expected to be raised once", eventArgs);

				eventArgs = e;
			};

			Factory.Saved += (s, e) =>
			{
				child.HasChanges = true;
				collection.Add(child);
			};

			Factory.Save();

			Assert("HasChangesChanged event should raised", eventArgs != null);
			AssertEquals("Object was not just changed", false, eventArgs.ObjectJustWasChanged);
		}

		public void TestHasChangesChangedEvent_ChildWasChanged_RaiseEvent()
		{
			var parent = Factory.New<DummyBusinessObject>();
			parent.HasChanges = false;

			var child = Factory.New<DummyBusinessObject>();
			child.HasChanges = false;

			HasChangesChangedEventArgs eventArgs = null;
			parent.HasChangesChanged += (s, e) =>
			{
				AssertNull("HasChangesChanged event is expected to be raised once", eventArgs);

				eventArgs = e;
			};

			parent.RegisterEditableChildObject(child);

			child.HasChanges = true;

			Assert("HasChangesChanged event is raised", eventArgs != null);
			AssertEquals("Object just was changed", true, eventArgs.ObjectJustWasChanged);
		}

		public void TestSetHasChanges_AnyValue_RaiseHasChangesChangedEvent()
		{
			var parent = Factory.New<DummyBusinessObject>();
			parent.HasChanges = false;

			HasChangesChangedEventArgs eventArgs = null;
			parent.HasChangesChanged += (s, e) =>
			{
				AssertNull("HasChangesChanged event is expected to be raised once", eventArgs);

				eventArgs = e;
			};

			parent.HasChanges = true;

			Assert("HasChangesChanged event is raised", eventArgs != null);
			AssertEquals("Object just was changed", true, eventArgs.ObjectJustWasChanged);
		}

		class DummyDependentBusinessObjectCollectionThatAccessesParentOnRemove : DummyDependentBusinessObjectCollection
		{
			public DummyDependentBusinessObjectCollectionThatAccessesParentOnRemove(DummyBaseBusinessObject parent, BusinessObjectFactory factory)
				: base(parent, factory)
			{
			}

			public override void Remove(BusinessObject elementToRemove)
			{
				if (((DummyBusinessObject)Master).Z0_Description == "abc")
				{
					//don't care, as long as I touched the Master.
					// Did you enjoy it?
				}
				base.Remove(elementToRemove);
			}
		}

		public void TestDeleteWhenCollectionsMasterIsDeleted()
		{
			DummyDependentBusinessObjectCollection collection = new DummyDependentBusinessObjectCollectionThatAccessesParentOnRemove(Dummy, Factory);
			DummyDependantBusinessObject dummyChild = collection.AddNew();
			Factory.Save();

			Dummy.Delete();

			dummyChild.Delete();

			AssertEquals("Do not try to remove child from collection that has a deleted parent", 1, collection.Count);
		}

		public void TestGetNullIsReadOnly()
		{
			BusinessObject bO = Factory.GetNull(Dummy.GetType());
			AssertEquals("Null objects must be read-only", true, bO.ReadOnly);
		}

		public class DummyWithChildren : DummyBusinessObject
		{
			public DummyWithChildren(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyChildBusinessObjectCollection Children
			{
				get
				{
					if (children == null)
					{
						children = new DummyChildBusinessObjectCollection(Factory);
						RegisterEditableChildObject(children);
					}
					return children;
				}
			}
			DummyChildBusinessObjectCollection children;
		}

		public void TestRefreshBindingIncludingChildren()
		{
			bool child1OnElementFired = false;
			bool child2OnElementFired = false;

			DummyWithChildren testDummy = Factory.New<DummyWithChildren>();
			DummyChildBusinessObject testChild1 = testDummy.Children.AddNew();
			((IBusiness)testChild1).ListChanged += new ListChangedEventHandler(delegate
			{ child1OnElementFired = true; });

			DummyChildBusinessObject testChild2 = testDummy.Children.AddNew();
			((IBusiness)testChild2).ListChanged += new ListChangedEventHandler(delegate
			{ child2OnElementFired = true; });

			testDummy.RefreshBindingIncludingChildren();
			AssertEquals("child 1 element changed event fired", true, child1OnElementFired);
			AssertEquals("child 2 element changed event fired", true, child2OnElementFired);
		}

		public void TestSettingProperty_CallsNotifyPropertyChanged()
		{
			Dummy.Z0_VarCharMax = "OriginalValue";
			ZPropertyValueChangedEventArgs lastEvent = null;
			ZPropertyValueChangedEventHandler handler = (object sender, ZPropertyValueChangedEventArgs e) => { lastEvent = e; };

			PropertyChangeSubscription.PropertyChanged += handler;
			try
			{
				Dummy.Z0_VarCharMax = "NewValue";
				AssertEquals("Property", Dummy.Z0_VarCharMaxInfo, lastEvent.Property);
				AssertEquals("OldValue", "OriginalValue", lastEvent.OldValue);
			}
			finally
			{
				PropertyChangeSubscription.PropertyChanged -= handler;
			}
		}

		public void TestSetPropertySmallDateTime_ShouldNotSetIfConvertedValueSame()
		{
			var date = new ZDateTime(2015, 8, 27, 10, 30, 35);

			Dummy.Z0_SmallDateTime = date;
			AssertEquals("The datetime value has only been set in memory and not saved to the database yet, so the full datetime, including seconds, should have remained as the value. And yet...", date, Dummy.Z0_SmallDateTime);

			Dummy.Z0_SmallDateTime = date.AddSeconds(5);
			AssertEquals("The datetime value has only been set in memory and not saved to the database yet, so the new second value should have been updated. And yet...", date.AddSeconds(5), Dummy.Z0_SmallDateTime);

			Dummy.Z0_SmallDateTime = date.AddSeconds(5).AddMilliseconds(5);
			AssertEquals("The datetime value has only been set in memory and not saved to the database yet, so the new millisecond value should have been updated. And yet...", date.AddSeconds(5).AddMilliseconds(5), Dummy.Z0_SmallDateTime);
			Factory.Save();

			var dummyFromDatabase = new BusinessObjectFactory().Load<DummyBusinessObject>(Dummy.PK);
			AssertEquals("The field should have had its seconds truncated (not rounded up since ToSmallDateTimeFloor is used), and yet...", new ZDateTime(2015, 8, 27, 10, 30, 0), dummyFromDatabase.Z0_SmallDateTime);
			Assert("The field should not have changes before the value is set, and yet...", !dummyFromDatabase.Z0_SmallDateTimeInfo.HasChanges);

			dummyFromDatabase.Z0_SmallDateTime = date;
			AssertEquals("The new date value should be the same as the old because the values should be compared as smalldatetimes since the bizo is in the database and the old value has no seconds or milliseconds recorded (because counting this as a change would be silly when the same value will be stored in the db). And yet...", new ZDateTime(2015, 8, 27, 10, 30, 0), dummyFromDatabase.Z0_SmallDateTime);
			Assert("The field should not have changes even after the value is attempted to be set because it's not really considered a change (the db value will just be the same as the old), and yet...", !dummyFromDatabase.Z0_SmallDateTimeInfo.HasChanges);

			var differentSmallDateTime = date.AddSeconds(100);
			dummyFromDatabase.Z0_SmallDateTime = differentSmallDateTime;
			AssertEquals("The new date value should have the additional minute/seconds, because the value entered was different than the old value, even after converting to smalldatetime. And yet...", differentSmallDateTime, dummyFromDatabase.Z0_SmallDateTime);
			Assert("The field should have changes after the value is set because the value entered was different than the old value, even after converting to smalldatetime, and yet...", dummyFromDatabase.Z0_SmallDateTimeInfo.HasChanges);
		}

		public void TestSetPropertyDateTimeOffset_ShouldSetIfOffsetValueDifferent()
		{
			var dateOffset = new ZDateTimeOffset(2015, 8, 27, 12, 30, 35, TimeSpan.FromHours(10));

			Dummy.Z0_DateTimeOffset = dateOffset;
			AssertEquals("The datetimeoffset value should be set.",
				dateOffset,
				Dummy.Z0_DateTimeOffset);

			Factory.Save();

			var secondFactory = new BusinessObjectFactory();
			var dummyFromDatabase = secondFactory.Load<DummyBusinessObject>(Dummy.PK);
			AssertEquals("The field should have be set.",
				new ZDateTimeOffset(2015, 8, 27, 12, 30, 35, TimeSpan.FromHours(10)),
				dummyFromDatabase.Z0_DateTimeOffset);

			dummyFromDatabase.Z0_DateTimeOffset = dateOffset;
			Assert("The field should not have changes even after the value is attempted to be set because it's not really considered a change.",
				!dummyFromDatabase.Z0_DateTimeOffsetInfo.HasChanges);

			var otherDateOffset = new ZDateTimeOffset(2015, 8, 27, 04, 30, 35, TimeSpan.FromHours(2));
			Assert("The default Equals method should return true as the two values represent the same utc datetime.",
				otherDateOffset.Equals(dateOffset));

			dummyFromDatabase.Z0_DateTimeOffset = otherDateOffset;
			Assert("The field should have changes after the value is set because the offset component changed.",
				dummyFromDatabase.Z0_DateTimeOffsetInfo.HasChanges);

			AssertEquals("The correct offset value should be set on the field.",
				TimeSpan.FromHours(2),
				dummyFromDatabase.Z0_DateTimeOffset.Offset);
		}

		#region TestSetReadOnly_WhenDeleted_ShouldNotThrowDeveloperException

		public void TestSetReadOnly_WhenDeleted_ShouldNotThrowDeveloperException()
		{
			var dummy = Factory.New<DummyWithReadOnlyCheckOfAProperty>();
			dummy.Delete();
			AssertNoExceptionThrown(() => dummy.ReadOnly = true);
		}

		class DummyWithReadOnlyCheckOfAProperty : DummyBusinessObject
		{
			public DummyWithReadOnlyCheckOfAProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override bool ReadOnly
			{
				get { return Z0_Bool && base.ReadOnly; }
				set { base.ReadOnly = value; }
			}
		}

		#endregion

		#region TestEnsureBlobFieldOnDeletedOrDetachedRowDoesNotThrowsException

		public void TestEnsureBlobFieldOnDeletedOrDetachedRowDoesNotThrowsException()
		{
			DummyWithBlobField dummy = Factory.New<DummyWithBlobField>();
			Factory.Save();

			DummyWithBlobField dummy1 = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyWithBlobField>(dummy.PK);
			dummy1.Delete();
			AssertNoExceptionThrown(() => dummy1.EnsureBlobFieldExposed(DummyBizoSchema.Z0_VarCharMax));

			DummyWithBlobField dummy2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyWithBlobField>(dummy.PK);
			dummy2.Row.Table.Rows.Remove(dummy2.Row);
			AssertNoExceptionThrown(() => dummy2.EnsureBlobFieldExposed(DummyBizoSchema.Z0_VarCharMax));

			DummyWithBlobField dummy3 = new DummyWithBlobField(Factory, dummy.Row.Table.NewRow());
			AssertNoExceptionThrown(() => dummy3.EnsureBlobFieldExposed(DummyBizoSchema.Z0_VarCharMax));
		}

		class DummyWithBlobField : DummyBusinessObject
		{
			public DummyWithBlobField(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public void EnsureBlobFieldExposed(SchemaColumn schemaColumn)
			{
				EnsureBlobField(schemaColumn);
			}
		}

		#endregion

		#region LoadChildEditableObjects

		public void TestLoadChildEditableObjects()
		{
			DummyWithDependentsBusinessObject dummy = Factory.New<DummyWithDependentsBusinessObject>();
			dummy.Dependents.AddNew();
			Factory.Save();

			DummyWithDependentsBusinessObject loadedDummy = new BusinessObjectFactory().Load<DummyWithDependentsBusinessObject>(dummy.PK);
			AssertEquals("Dependent collection not registered initially for the test", false, IsDependentCollectionRegistered(loadedDummy));
			loadedDummy.LoadChildEditableObjects();
			AssertEquals("Dependent registered before RunPreSaveValidation", true, IsDependentCollectionRegistered(loadedDummy));
		}

		public void TestMarkAsNeedingValidationIncludingChildren_LoadsChildEditableObjects()
		{
			DummyWithDependentsBusinessObject dummy = Factory.New<DummyWithDependentsBusinessObject>();
			dummy.Dependents.AddNew();
			Factory.Save();

			DummyWithDependentsBusinessObject loadedDummy = new BusinessObjectFactory().Load<DummyWithDependentsBusinessObject>(dummy.PK);
			AssertEquals("Dependent collection not registered initially for the test", false, IsDependentCollectionRegistered(loadedDummy));
			loadedDummy.MarkAsNeedingValidationIncludingChildren();
			AssertEquals("Dependent registered before RunPreSaveValidation", true, IsDependentCollectionRegistered(loadedDummy));
		}

		bool IsDependentCollectionRegistered(DummyWithDependentsBusinessObject dummy)
		{
			IBusiness[] children = ((IBusiness)dummy).Children;
			return Array.Exists(children, (IBusiness child) => { return child is DummyDependentBusinessObjectCollection; });
		}

		#endregion

		#region GetValueFromRowSafely

		public void TestGetValueFromRowSafely_WithZPropertyInfo()
		{
			IBusinessObjectInternals dummyInternals = Dummy;
			Dummy.Z0_Description = "Added";
			AssertEquals("Added", dummyInternals.GetValueFromRowSafely(Dummy.Z0_DescriptionInfo, DataRowVersion.Original));
			AssertEquals("Added", dummyInternals.GetValueFromRowSafely(Dummy.Z0_DescriptionInfo, DataRowVersion.Default));

			Dummy.Z0_Description = "OriginalValue";
			Factory.Save();
			Dummy.Z0_Description = "Value";

			AssertEquals("OriginalValue", dummyInternals.GetValueFromRowSafely(Dummy.Z0_DescriptionInfo, DataRowVersion.Original));
			AssertEquals("Value", dummyInternals.GetValueFromRowSafely(Dummy.Z0_DescriptionInfo));
		}

		public void TestGetValueFromRowSafely_WithSchemaColumn()
		{
			IBusinessObjectInternals dummyInternals = Dummy;
			Dummy.Z0_Description = "Added";
			AssertEquals("Added", dummyInternals.GetValueFromRowSafely(DummyBizoSchema.Z0_Description, DataRowVersion.Original));
			AssertEquals("Added", dummyInternals.GetValueFromRowSafely(DummyBizoSchema.Z0_Description, DataRowVersion.Default));

			Dummy.Z0_Description = "OriginalValue";
			Factory.Save();
			Dummy.Z0_Description = "Value";

			AssertEquals("OriginalValue", dummyInternals.GetValueFromRowSafely(DummyBizoSchema.Z0_Description, DataRowVersion.Original));
			AssertEquals("Value", dummyInternals.GetValueFromRowSafely(DummyBizoSchema.Z0_Description, DataRowVersion.Default));
		}

		public void TestGetValueFromRowSafely_WithColumnName()
		{
			IBusinessObjectInternals dummyInternals = Dummy;
			Dummy.Z0_Description = "Added";
			AssertEquals("Added", dummyInternals.GetValueFromRowSafely(DummyBizoSchema.Z0_Description.Name, DataRowVersion.Original));
			AssertEquals("Added", dummyInternals.GetValueFromRowSafely(DummyBizoSchema.Z0_Description.Name, DataRowVersion.Default));

			Dummy.Z0_Description = "OriginalValue";
			Factory.Save();
			Dummy.Z0_Description = "Value";

			AssertEquals("OriginalValue", dummyInternals.GetValueFromRowSafely(DummyBizoSchema.Z0_Description.Name, DataRowVersion.Original));
			AssertEquals("Value", dummyInternals.GetValueFromRowSafely(DummyBizoSchema.Z0_Description.Name, DataRowVersion.Default));
		}

		#endregion

		#region Test Lazy Loaded Child Is Read-Only

		public class DummyBO : NonPersistentBusinessObject
		{
			public DummyBO(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			DummyBO fDummyChild;
			public DummyBO DummyChild
			{
				get
				{
					if (fDummyChild == null)
					{
						fDummyChild = new DummyBO(Factory);
						RegisterEditableChildObject(fDummyChild);
					}

					return fDummyChild;
				}
			}
		}

		public void TestSetReadOnlyIncludingChildren_ForLazyLoadedChild()
		{
			DummyBO dummy = new DummyBO(Factory);

			dummy.SetReadOnlyIncludingChildren(true);
			AssertEquals("Lazy loaded child should be readonly", true, dummy.DummyChild.ReadOnly);

			dummy.SetReadOnlyIncludingChildren(false);
			AssertEquals("Lazy loaded child should not be readonly", false, dummy.DummyChild.DummyChild.ReadOnly);
		}

		public class DummyBOWithTwoChilds : NonPersistentBusinessObject
		{
			public DummyBOWithTwoChilds(BusinessObjectFactory factory) : base(factory) { }

			public DummyBO DummyChild1
			{
				get
				{
					if (dummyChild1 == null)
					{
						dummyChild1 = new DummyBO(Factory);
						RegisterEditableChildObject(dummyChild1);
						bool someCondition = false;
						dummyChild1.SetReadOnlyIncludingChildren(someCondition);
					}
					return dummyChild1;
				}
			}
			DummyBO dummyChild1;

			public DummyBO DummyChild2
			{
				get
				{
					if (dummyChild2 == null)
					{
						dummyChild2 = new DummyBO(Factory);
						RegisterEditableChildObject(dummyChild2);
						bool someCondition = false;
						dummyChild2.SetReadOnlyIncludingChildren(someCondition);
					}
					return dummyChild2;
				}
			}
			DummyBO dummyChild2;
		}

		public void TestSetReadOnlyIncludingChildren_ForLazyLoadedChild2()
		{
			DummyBOWithTwoChilds dummy = new DummyBOWithTwoChilds(Factory);
			AssertEquals("Load child 1", false, dummy.DummyChild1.ReadOnly);

			dummy.SetReadOnlyIncludingChildren(true);
			AssertEquals("Child 1 should be readonly", true, dummy.DummyChild1.ReadOnly);

			AssertEquals("Load child 2", false, dummy.DummyChild2.ReadOnly);

			dummy.SetReadOnlyIncludingChildren(true);
			AssertEquals("Child 1 should be readonly", true, dummy.DummyChild1.ReadOnly);
			AssertEquals("Child 2 should be readonly", true, dummy.DummyChild2.ReadOnly);
		}

		#endregion

		#region Test Load Ourself in Constructor

		[ExpectNoExceptions]
		public void TestLoadOurselfInConstructor()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObjectThatLoadsItselfInConstructor dummy1 = factory.New<DummyBusinessObjectThatLoadsItselfInConstructor>();
			DummyBusinessObjectThatLoadsItselfInConstructor dummy2 = factory.New<DummyBusinessObjectThatLoadsItselfInConstructor>();
			dummy1.OtherDummyPK = dummy2.PK;
			dummy2.OtherDummyPK = dummy1.PK;
			factory.Save();

			BusinessObjectFactory retrievingFactory = new BusinessObjectFactory();
			retrievingFactory.Load(typeof(DummyBusinessObjectThatLoadsItselfInConstructor), dummy1.PK);
		}

		class DummyBusinessObjectThatLoadsItselfInConstructor : DummyBusinessObject
		{
			public DummyBusinessObjectThatLoadsItselfInConstructor(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				if (OtherDummyPK.IsValid)
				{
					factory.Load(typeof(DummyBusinessObjectThatLoadsItselfInConstructorSubClass), OtherDummyPK);
				}
			}

			public ZGuid OtherDummyPK
			{
				get { return Z0_Guid; }
				set { Z0_Guid = value; }
			}
		}

		class DummyBusinessObjectThatLoadsItselfInConstructorSubClass : DummyBusinessObjectThatLoadsItselfInConstructor
		{
			public DummyBusinessObjectThatLoadsItselfInConstructorSubClass(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		#endregion

		#region HasNotifications, HasMessageErrors, HasErrors, HasWarnings... and each with NotIncludingChildren

		public void TestHasNotificationsAndNotIncludingChildren()
		{
			CheckHasNotifications(Dummy, false, false);

			DummyWithDependentsBusinessObject dummy = Factory.New<DummyWithDependentsBusinessObject>();
			DummyDependantBusinessObject dependent = dummy.Dependents.AddNew();
			using (dummy.SuspendValidationTesting())
			using (dependent.SuspendValidationTesting())
			{
				dummy.Dependents.Add(dependent);
				CheckHasNotifications(dummy, false, false);

				dependent.ZD1_CodeInfo.AddError("Code Notification");
				CheckHasNotifications(dummy, true, false);

				dummy.Z0_DecimalInfo.AddError("Decimal Notification");
				CheckHasNotifications(dummy, true, true);
			}
		}

		public void TestHasErrorsAndNotIncludingChildren()
		{
			DummyWithDependentsBusinessObject dummy = Factory.New<DummyWithDependentsBusinessObject>();
			CheckHasErrors(dummy, false, false);

			DummyDependantBusinessObject dependent = dummy.Dependents.AddNew();
			using (dummy.SuspendValidationTesting())
			using (dependent.SuspendValidationTesting())
			{
				dummy.Dependents.Add(dependent);
				CheckHasErrors(dummy, false, false);

				dependent.ZD1_CodeInfo.AddError("Code Error");
				CheckHasErrors(dummy, true, false);

				dummy.Z0_DecimalInfo.AddError("Decimal Error");
				CheckHasErrors(dummy, true, true);
			}
		}

		class DummyBusinessObjectForDataRefreshTest : DummyBusinessObject
		{
			public DummyBusinessObjectForDataRefreshTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void OnUpdatedByDataRefresh()
			{
				if (Z0_Description.Length > 0)
				{
					Z0_DescriptionInfo.AddWarning("Just a dummy warning. just so that we pretend we need to access the object property for something.");
				}
			}
		}

		[ExpectNoExceptions()]
		public void TestDeletedParentBusinessObjectWillNotBeUpdatedByDataRefreshBus()
		{
			DummyBusinessObjectForDataRefreshTest dummyParent = Factory.New<DummyBusinessObjectForDataRefreshTest>();
			DummyBusinessObjectForDataRefreshTest dummyChild = Factory.New<DummyBusinessObjectForDataRefreshTest>();
			using (dummyChild.SuspendValidationTesting())
			{
				Factory.Save();

				dummyParent.RegisterEditableChildObject(dummyChild);
				dummyParent.Delete();

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				DummyBusinessObjectForDataRefreshTest dummyChildCopy = factory2.Load<DummyBusinessObjectForDataRefreshTest>(dummyChild.PK);
				dummyChildCopy.Z0_Description = "Changed";
				factory2.Save();
			}
		}

		public void TestParentCollectionIsMarkedAsIsRefreshingByDataRefreshBusWhenDeletingByDataRefreshBus()
		{
			var master = Factory.New<DummyBusinessObject>();
			Factory.Save();

			var collection = new DummyDependentBusinessObjectCollection(master, master.Factory);
			master.RegisterEditableChildObject(collection);
			collection.Load();

			bool onRemovedCalled = false;

			var dummy1 = collection.AddNew();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var dummy1OtherFactory = otherFactory.Load<DummyDependantBusinessObject>(dummy1.PK);
			dummy1OtherFactory.Delete();

			BusinessObjectCollection.ElementChangedHandler onRemovingHandler = (bizObj) =>
			{
				onRemovedCalled = true;
				AssertEquals("businessObject should be marked as IsRefreshingByDataRefreshBus", true, bizObj.IsRefreshingByDataRefreshBus);
				AssertEquals("collection should be marked as IsRefreshingByDataRefreshBus", true, collection.IsRefreshingByDataRefreshBus);
			};

			try
			{
				collection.ElementRemoving += onRemovingHandler;
				otherFactory.Save();
			}
			finally
			{
				collection.ElementRemoving -= onRemovingHandler;
			}

			AssertEquals("dummy was removed from the collection by DataRefreshBus", 0, collection.Count);
			AssertEquals("OnRemovedHandler/assertion was called", true, onRemovedCalled);
		}

		public void TestHasWarningsNotIncludingChildren()
		{
			DummyWithDependentsBusinessObject dummy = Factory.New<DummyWithDependentsBusinessObject>();
			CheckHasWarnings(dummy, false, false);

			DummyDependantBusinessObject dependent = dummy.Dependents.AddNew();
			using (dummy.SuspendValidationTesting())
			using (dependent.SuspendValidationTesting())
			{
				dummy.Dependents.Add(dependent);
				CheckHasWarnings(dummy, false, false);

				dependent.ZD1_CodeInfo.AddWarning("Code Warning");
				CheckHasWarnings(dummy, true, false);

				dummy.Z0_DecimalInfo.AddWarning("Decimal Warning");
				CheckHasWarnings(dummy, true, true);
			}
		}

		public void TestHasNotificationsWithTopLevel()
		{
			DummyBusinessObject dummyMaster = Factory.New<DummyBusinessObject>();
			Dummy.RegisterEditableChildObject(dummyMaster);

			using (dummyMaster.SuspendValidationTesting())
			{
				dummyMaster.Z0_VarCharMaxInfo.AddError("Master Error");
			}

			Assert(Dummy.HasNotifications());
			Assert(dummyMaster.HasNotifications());
			Assert(Dummy.HasErrors());
			Assert(dummyMaster.HasErrors());

			dummyMaster.IsTopLevel = true;

			Assert(!Dummy.HasNotifications());
			Assert(dummyMaster.HasNotifications());
			Assert(!Dummy.HasErrors());
			Assert(dummyMaster.HasErrors());
		}

		public void TestGetHighestSeverityNotificationType()
		{
			DummyBusinessObject dummyMaster = Factory.New<DummyBusinessObject>();
			dummyMaster.AddRowError("Master Error");
			Dummy.RegisterEditableChildObject(dummyMaster);

			AssertEquals("Error", Dummy.GetHighestSeverityNotificationType().EnumValueName);
			AssertEquals("Error", dummyMaster.GetHighestSeverityNotificationType().EnumValueName);

			dummyMaster.IsTopLevel = true;

			AssertNull(Dummy.GetHighestSeverityNotificationType());
			AssertEquals("Error", dummyMaster.GetHighestSeverityNotificationType().EnumValueName);
		}

		void CheckHasNotifications(BusinessObject businessObject, bool expectHasNotifications, bool expectHasNotificationsNotIncludingChildren)
		{
			AssertEquals("HasNotifications", expectHasNotifications, businessObject.HasNotifications());
			AssertEquals("HasNotificationsNotIncludingChildren", expectHasNotificationsNotIncludingChildren, businessObject.Notifications.HasNotifications());
		}

		void CheckHasErrors(BusinessObject businessObject, bool expectHasErrors, bool expectHasErrorsNotIncludingChildren)
		{
			AssertEquals("HasErrors", expectHasErrors, businessObject.HasErrors);
			AssertEquals("HasErrorsNotIncludingChildren", expectHasErrorsNotIncludingChildren, businessObject.Notifications.HasErrors());
		}

		void CheckHasWarnings(BusinessObject businessObject, bool expectHasWarnings, bool expectHasWarningsNotIncludingChildren)
		{
			AssertEquals("HasWarnings", expectHasWarnings, businessObject.HasWarnings);
			AssertEquals("HasWarningsNotIncludingChildren", expectHasWarningsNotIncludingChildren, businessObject.Notifications.HasWarnings());
		}

		#endregion

		#region ILinkable members

		public void TestILinkableLinkPK()
		{
			DummyBusinessObject bizo = Factory.New<DummyBusinessObject>();
			ILinkable linkableBizo = bizo;
			AssertEquals(bizo.PK, linkableBizo.LinkPK);
		}

		public void TestILinkableLinkTableName()
		{
			DummyBusinessObject bizo = Factory.New<DummyBusinessObject>();
			ILinkable linkableBizo = bizo;
			AssertEquals(bizo.TableName, linkableBizo.LinkTableName);
			bizo.Table = new ZDataTable("foo");
			AssertEquals(bizo.TableName, linkableBizo.LinkTableName);
		}

		public void TestILinkableLinkTablePrefix()
		{
			DummyBusinessObject bizo = Factory.New<DummyBusinessObject>();
			ILinkable linkableBizo = bizo;
			AssertEquals(bizo.TablePrefix, linkableBizo.LinkTablePrefix);
		}

		public void TestILinkableLinkIsInDatabase()
		{
			DummyBusinessObject bizo = Factory.New<DummyBusinessObject>();
			ILinkable linkableBizo = bizo;
			bizo.SetIsInDatabase(true);
			AssertEquals(bizo.IsInDatabase, linkableBizo.LinkIsInDatabase);
			bizo.SetIsInDatabase(false);
			AssertEquals(bizo.IsInDatabase, linkableBizo.LinkIsInDatabase);
		}

		#endregion

		#region RelatedBusinessObjectTest

		public void TestGetRelatedBusinessObject()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy4 = Factory.New<DummyBusinessObject>();

			dummy1.Z0_Guid = dummy2.PK;
			dummy3.Z0_Guid = dummy4.PK;

			AssertEquals("Dummy1.RelatedDummy", dummy2, BusinessObject.GetRelatedBizO(dummy1.Z0_GuidInfo));
			AssertEquals("Dummy3.RelatedDummy", dummy4, BusinessObject.GetRelatedBizO(dummy3.Z0_GuidInfo));
		}

		#endregion

		#region Concurrency

		[ExpectNoExceptions]
		public void TestIsValidOnDeleteConcurrency()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			BusinessObjectFactory factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			DummyBusinessObject dummy1 = factory1.NewWithValidTestData<DummyBusinessObject>();
			((ILightValidationInternals)dummy1).IsValid = false;
			factory1.Save();

			DummyBusinessObject dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
			((ILightValidationInternals)dummy2).IsValid = true;
			factory2.Save();

			dummy1.Delete();
			factory1.Save();
		}

		public void TestSetConcurrencyPolicyOnTable()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			AssertNull("Precondition: table concurrency policy is null", dummy.Table.ExtendedProperties[typeof(ConcurrencyPolicy)]);

			dummy.SetConcurrencyPolicyOnTable(ConcurrencyPolicy.Ignore);
			AssertEquals(ConcurrencyPolicy.Ignore, dummy.Table.ExtendedProperties[typeof(ConcurrencyPolicy)]);
		}

		#endregion

		#region Notifications on Deleted BusinessObject

		public void TestHasErrorsOnDeletedBusinessIsFalse()
		{
			DummyBusinessObject dummy = Factory.NewWithValidTestData<DummyBusinessObject>();

			dummy.Delete();
			dummy.AddRowError("Some error");

			Assert("Deleted BusinessObject shoud not have errors", !dummy.HasErrors());
			Assert("Notification should be stored anyway", dummy.HasNotifications());
		}

		#endregion

		#region TestRunPresaveValidationWithFetchHints

		public void TestRunPresaveValidationWithFetchHints()
		{
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				var factory = new BusinessObjectFactory();
				DummyBusinessObject dummy = factory.New<DummyBusinessObject>();
				for (int i = 0; i < 10; i++)
				{
					dummy.RegisterEditableChildObject(factory.New<DummyWithValidationFetch>());
				}

				var initialHits = factory.GetTableHitCount(DummyDependentBizoSchema.Constants.TableName);
				int initialFetchHints = factory.GetLoadedFetchHintCountForTable(DummyDependentBizoSchema.Constants.TableName);
				int expectedFetchHints = initialFetchHints + 10;

				dummy.RunPreSaveValidationWithFetchHints();

				AssertEquals("There should be only 1 new hit to DummyDependentBizo table", initialHits + 1, factory.GetTableHitCount(DummyDependentBizoSchema.Constants.TableName));
				AssertEquals(expectedFetchHints, factory.GetLoadedFetchHintCountForTable(DummyDependentBizoSchema.Constants.TableName));
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		class DummyWithValidationFetch : DummyBusinessObject
		{
			public DummyWithValidationFetch(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override void RunPreSaveValidationCore()
			{
				base.RunPreSaveValidationCore();
				Factory.Load<DummyDependantBusinessObject>(new ZQuery(DummyDependentBizoSchema.ZD1_Z0, PK));
			}

			protected override IBusinessObjectFetchStrategy GetFetchStrategy() => new DummyWithValidationFetchStrategy(this);
		}

		class DummyWithValidationFetchStrategy : BusinessObjectFetchStrategy
		{
			public DummyWithValidationFetchStrategy(BusinessObject businessObject) : base(businessObject) { }

			protected override void FetchForValidateCore()
			{
				BusinessObject.Factory.AddFetchHint(DummyDependentBizoSchema.ZD1_Z0, BusinessObject.PK);
			}
		}

		#endregion

		#region TestOnSavingForLightValidation

		public void TestOnSavingForLightValidation()
		{
			DummyForLightValidationTest dummy = Factory.New<DummyForLightValidationTest>();

			Assert("Precondition", !dummy.OnSavingWasCalled);

			Factory.Save();

			Assert("Called because not in database", dummy.OnSavingWasCalled);

			dummy.OnSavingWasCalled = false;

			((ILightValidationInternals)dummy).IsValid = !((ILightValidationInternals)dummy).IsValid;

			Assert("Precondition", !dummy.OnSavingWasCalled);
			Assert("Precondition", ((ILightValidationInternals)dummy).IsValidHasChanges);

			Factory.Save();

			Assert("Not called because only light validation changed", !dummy.OnSavingWasCalled);
			Assert("Saved to database anyway", !((ILightValidationInternals)dummy).IsValidHasChanges);

			((ILightValidationInternals)dummy).IsValid = !((ILightValidationInternals)dummy).IsValid;
			dummy.Z0_Code = "XXX";

			Assert("Precondition", !dummy.OnSavingWasCalled);
			Assert("Precondition", ((ILightValidationInternals)dummy).IsValidHasChanges);

			Factory.Save();

			Assert("Called because of other fields", dummy.OnSavingWasCalled);
			Assert("Saved to database", !((ILightValidationInternals)dummy).IsValidHasChanges);
		}

		class DummyForLightValidationTest : DummyBusinessObject
		{
			public DummyForLightValidationTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public bool OnSavingWasCalled { get; set; }

			public override void OnSaving()
			{
				base.OnSaving();
				OnSavingWasCalled = true;
			}
		}

		#endregion

		public void TestResumeValidationForAllDescendantsReallyDoes()
		{
			BusinessObject obj1 = Factory.New(typeof(DummyBusinessObject));
			BusinessObject obj2 = Factory.New(typeof(DummyBusinessObject));
			BusinessObject obj3 = Factory.New(typeof(DummyBusinessObject));
			BusinessObject obj4 = Factory.New(typeof(DummyBusinessObject));
			BusinessObject obj5 = Factory.New(typeof(DummyBusinessObject));
			DummyBusinessObjectCollection boc1 = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObjectCollection boc2 = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObjectCollection boc3 = new DummyBusinessObjectCollection(Factory);

			obj1.RegisterEditableChildObject(obj2);
			obj1.RegisterEditableChildObject(obj3);
			obj2.RegisterEditableChildObject(obj4);
			obj2.RegisterEditableChildObject(obj5);
			obj3.RegisterEditableChildObject(boc1);
			obj4.RegisterEditableChildObject(boc2);
			obj5.RegisterEditableChildObject(boc3);

			using (obj1.GetValidationSuspender())
			using (obj2.GetValidationSuspender())
			using (obj3.GetValidationSuspender())
			using (obj3.GetValidationSuspender())
			using (obj5.GetValidationSuspender())
			using (obj5.GetValidationSuspender())
			{
				boc1.SuspendValidation();
				boc3.SuspendValidation();

				using (((IBusinessObjectInternals)obj1).ResumeValidationForAllDescendantsTemporarily())
				{
					Assert("obj1 validation is resumed temporarily", !obj1.IsValidationSuspended);
					Assert("obj2 validation is resumed temporarily", !obj2.IsValidationSuspended);
					Assert("obj3 validation is resumed temporarily", !obj3.IsValidationSuspended);
					Assert("obj4 validation is resumed temporarily", !obj4.IsValidationSuspended);
					Assert("obj5 validation is resumed temporarily", !obj5.IsValidationSuspended);
					Assert("boc1 validation is resumed temporarily", !boc1.IsValidationSuspended);
					Assert("boc2 validation is resumed temporarily", !boc2.IsValidationSuspended);
					Assert("boc3 validation is resumed temporarily", !boc3.IsValidationSuspended);
				}
				Assert("obj1 validation is suspended again", obj1.IsValidationSuspended);
				Assert("obj2 validation is suspended again", obj2.IsValidationSuspended);
				Assert("obj3 validation is suspended again", obj3.IsValidationSuspended);
				Assert("obj4 validation is suspended again", obj4.IsValidationSuspended);
				Assert("obj5 validation is suspended again", obj5.IsValidationSuspended);
				Assert("boc1 validation is suspended again", boc1.IsValidationSuspended);
				Assert("boc2 validation is suspended again", boc2.IsValidationSuspended);
				Assert("boc3 validation is suspended again", boc3.IsValidationSuspended);
				AssertEquals("obj1 suspended 1 times", 1, obj1.ValidationIndex);
				AssertEquals("obj2 suspended 2 times", 2, obj2.ValidationIndex);
				AssertEquals("obj3 suspended 3 times", 3, obj3.ValidationIndex);
				AssertEquals("obj4 suspended 2 times", 2, obj4.ValidationIndex);
				AssertEquals("obj5 suspended 4 times", 4, obj5.ValidationIndex);
				AssertEquals("boc1 suspended 4 times", 4, boc1.ValidationIndex);
				AssertEquals("boc2 suspended 1 times", 2, boc2.ValidationIndex);
				AssertEquals("boc3 suspended 4 times", 5, boc3.ValidationIndex);
			}
		}

		[ExpectNoExceptions]
		public void TestLoadChildEditableObjectsForChildWithProgress()
		{
			EventHandler<BusinessObject.LoadChildEditableObjectsProgressEventArgs> handler = new EventHandler<BusinessObject.LoadChildEditableObjectsProgressEventArgs>((object sender, BusinessObject.LoadChildEditableObjectsProgressEventArgs e) => { });
			var bizo = Factory.New<DummyBusinessObject>();
			var bizo2 = Factory.New<DummyWithDependentsBusinessObject>();
			var bizo3 = bizo2.Dependents.AddNew();
			IBusiness[] children = null;
			bizo.LoadChildEditableObjectsForChild(children);
			bizo.LoadChildEditableObjectsForChild(children, handler);
			bizo2.LoadChildEditableObjectsForChild(children);
			bizo2.LoadChildEditableObjectsForChild(children, handler);
			children = Array.Empty<IBusiness>();
			bizo.LoadChildEditableObjectsForChild(children);
			bizo.LoadChildEditableObjectsForChild(children, handler);
			bizo2.LoadChildEditableObjectsForChild(children);
			bizo2.LoadChildEditableObjectsForChild(children, handler);
		}

		public void TestReaderFromStreamSource()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			var source = new ByteArrayStreamSource(new byte[] { 1, 2, 3, 4 });
			bizo.SetZ0_VarBinaryMaxSource(source);
			using (var reader = bizo.GetZ0_VarBinaryMaxReader())
			{
				Assert(reader.ContainsTheSameDataAs(source.GetStream()));
			}
		}

		public void TestReaderFromTextReaderSource()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			var source = new StringReaderSource("anything");
			bizo.SetZ0_NVarCharMaxSource(source);
			using (var reader = bizo.GetZ0_NVarCharMaxReader())
			{
				Assert(reader.ContainsTheSameDataAs(source.GetReader()));
			}
		}

		#region TestValidateWrappedInfoAndRefreshBinding

		public void TestValidateWrappedInfoAndRefreshBinding()
		{
			var dummy = Factory.New<DummyWithWrappedInfo>();

			dummy.WrappedText = "XYZ";
			AssertNoErrors(dummy.WrappedTextInfo);

			var notificationA = new Notification(NotificationType.Error, "Wrong character A");
			var notificationB = new Notification(NotificationType.Error, "Wrong character B");
			var notification0 = new Notification(NotificationType.Warning, "Digit 0");

			AssertValidateAndListChanged("Same (no) validation - list changed should not fire", dummy, "xxx", false);
			AssertValidateAndListChanged("New validation - list changed should fire", dummy, "xxxA", true, notificationA);
			AssertValidateAndListChanged("Same validation - list changed should not fire", dummy, "xxxA", false, notificationA);
			AssertValidateAndListChanged("New validation - list changed should fire", dummy, "xxxAB", true, notificationA, notificationB);
			AssertValidateAndListChanged("Same validations (different order) - list changed should not fire", dummy, "xxxBA", false, notificationA, notificationB);
			AssertValidateAndListChanged("Reduced validation - list changed should fire", dummy, "xxxB", true, notificationB);
			AssertValidateAndListChanged("Different validation - list changed should fire", dummy, "xxx0", true, notification0);
			AssertValidateAndListChanged("Same validation - list changed should not fire", dummy, "xxx0", false, notification0);
			AssertValidateAndListChanged("No validation - list changed should fire", dummy, "xxx", true);
			AssertValidateAndListChanged("Same (no) validation - list changed should not fire", dummy, "zzz", false);
		}

		public void TestValidateWrappedInfoForSubPropertyAndRefreshBinding()
		{
			var dummy = Factory.New<DummyWithWrappedInfo>();
			dummy.isRefreshed_Debug = false;

			((IBusinessObjectInternals)dummy).Validate(dummy.SubPropertyInfo);
			Assert("Should call RefreshBinding().", dummy.isRefreshed_Debug);
		}

		void AssertValidateAndListChanged(string message, DummyWithWrappedInfo dummy, string newValue, bool expectedListChanged, params INotification[] expectedNotifications)
		{
			var listChangedFired = false;
			ListChangedEventHandler listChangedHandler = (_, x_) => listChangedFired = true;

			dummy.Row[DummyBizoSchema.Constants.Z0_Description] = newValue;

			((IBindingList)dummy).ListChanged += listChangedHandler;
			try
			{
				((IBusinessObjectInternals)dummy).Validate(dummy.WrappedTextInfo);
			}
			finally
			{
				((IBindingList)dummy).ListChanged -= listChangedHandler;
			}

			AssertEquals(message, expectedListChanged, listChangedFired);

			Assert(dummy.WrappedTextInfo.Notifications.HasSameNotificationsIgnoringOrder(expectedNotifications));
		}

		class DummyWithWrappedInfo : DummyBusinessObject
		{
			public DummyWithWrappedInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ZString WrappedText
			{
				get { return WrappedWrappedText; }
				set { WrappedWrappedText = value; }
			}

			public ZPropertyInfo WrappedTextInfo => GetWrappedZPropertyInfo(nameof(WrappedText), _ => WrappedWrappedTextInfo);

			public ZString WrappedWrappedText
			{
				get { return Z0_Description; }
				set { Z0_Description = value; }
			}

			public ZPropertyInfo WrappedWrappedTextInfo => GetWrappedZPropertyInfo(nameof(WrappedWrappedTextInfo), _ => Z0_DescriptionInfo);

			public ZPropertyInfo SubPropertyInfo
			{
				get { return GetWrappedZPropertyInfo("Info+SubProperty", x => Info.SubPropertyInfo); }
			}

			protected override DummyBizoValidation GetNewValidation()
			{
				return new DummyWithWrappedInfoValidation(this);
			}

			class DummyWithWrappedInfoValidation : DummyBusinessObjectValidation
			{
				public DummyWithWrappedInfoValidation(DummyWithWrappedInfo bizO) : base(bizO) { }

				protected override void CheckZ0_Description()
				{
					base.CheckZ0_Description();

					if (Parent.Z0_Description.Contains('A'))
					{
						Parent.Z0_DescriptionInfo.AddError("Wrong character A");
					}
					if (Parent.Z0_Description.Contains('B'))
					{
						Parent.Z0_DescriptionInfo.AddError("Wrong character B");
					}
					if (Parent.Z0_Description.Contains('0'))
					{
						Parent.Z0_DescriptionInfo.AddWarning("Digit 0");
					}
				}
			}

			public WrappedInfoForSubProperty Info
			{
				get
				{
					if (fInfo == null)
					{
						fInfo = new WrappedInfoForSubProperty();
						RegisterListChangedCalledRefreshBinding(fInfo);
						fInfo.isRefreshed_Debug = false;
					}
					return fInfo;
				}
			}
			WrappedInfoForSubProperty fInfo;

			public class WrappedInfoForSubProperty : NonPersistentBusinessObject
			{
				public ZString SubProperty { get; set; }

				public ZPropertyInfo SubPropertyInfo
				{
					get { return GetZPropertyInfo(nameof(SubProperty)); }
				}
			}
		}

		#endregion

		#region IsSavedByFactory Service

		public void TestIsSavedByFactoryService()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			Assert(bizo.IsSavedByFactory);

			var service = new IsSavedByFactoryService();
			Factory.ServiceContainer.AddService<IBOIsSavedByFactoryService>(service);

			service.IsSavedByFactoryAllowed = true;
			Assert(bizo.IsSavedByFactory);

			service.IsSavedByFactoryAllowed = false;
			Assert(!bizo.IsSavedByFactory);

			Factory.ServiceContainer.RemoveService<IBOIsSavedByFactoryService>();
			Assert(bizo.IsSavedByFactory);
		}

		public void TestIsSavedByFactoryService_IsNotExcludedFromSavingByFactory()
		{
			var includedBizo = Factory.New<DummyBusinessObject>();
			var excludedBizo = Factory.New<DummyBusinessObject>();

			// with IsSavedByFactoryService attached
			var service = new IsSavedByFactoryService();
			Factory.ServiceContainer.AddService<IBOIsSavedByFactoryService>(service);
			service.AllowedBizoPKs.Add(includedBizo.PK);

			includedBizo.Z0_Date = ZDateTime.Now;
			AssertBusinessObjectIsIncludedInFactorySave(includedBizo);

			excludedBizo.Z0_Date = ZDateTime.Now;
			AssertBusinessObjectIsExcludedFromFactorySave(excludedBizo);

			// with IsSavedByFactoryService detached
			service.AllowedBizoPKs.Clear();
			Factory.ServiceContainer.RemoveService<IBOIsSavedByFactoryService>();

			excludedBizo.Z0_Date = ZDateTime.Now;
			AssertBusinessObjectIsIncludedInFactorySave(excludedBizo);
		}

		void AssertBusinessObjectIsIncludedInFactorySave(DummyBusinessObject bizo)
		{
			AssertEquals("Should not be excluded from Factory Save", true, bizo.IsNotExcludedFromSavingByFactory);
			AssertEquals("Should not be excluded from Factory Save", true, bizo.IsSavedByFactory);
			AssertEquals("HasChanges should be set", true, bizo.HasChanges);

			bizo.OnFactorySavedCalled = false;
			bizo.FactorySavedSucceded = false;

			Factory.Save();

			AssertEquals("Should have called OnFactorySaved()", true, bizo.OnFactorySavedCalled);
			AssertEquals("Should indicate Factory Save succeeded", true, bizo.FactorySavedSucceded);
			AssertEquals("HasChanges should be reset", false, bizo.HasChanges);
		}

		void AssertBusinessObjectIsExcludedFromFactorySave(DummyBusinessObject bizo)
		{
			AssertEquals("Should be excluded from Factory Save", false, bizo.IsNotExcludedFromSavingByFactory);
			AssertEquals("Should be excluded from Factory Save", false, bizo.IsSavedByFactory);
			AssertEquals("HasChanges should be set", true, bizo.HasChanges);

			bizo.OnFactorySavedCalled = false;
			bizo.FactorySavedSucceded = false;

			Factory.Save();

			AssertEquals("Should have called OnFactorySaved()", true, bizo.OnFactorySavedCalled);
			AssertEquals("Should indicate Factory Save succeeded", true, bizo.FactorySavedSucceded);
			AssertEquals("HasChanges should not be reset", true, bizo.HasChanges);
		}

		class IsSavedByFactoryService : IBOIsSavedByFactoryService
		{
			public bool IsSavedByFactoryAllowed;
			public HashSet<ZGuid> AllowedBizoPKs = new HashSet<ZGuid>();

			public bool IsBOSavedByFactory(BusinessObject businessObject)
			{
				var boPk = businessObject.PK;
				return IsSavedByFactoryAllowed || AllowedBizoPKs.Any(pk => pk.Equals(boPk));
			}
		}

		#endregion

		#region TestIsInDatabase

		public void TestIsInDatabase()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "AAA";
			AssertEquals("IsInDatabase should be false", false, dummy.IsInDatabase);
			Factory.Save();
			AssertEquals("IsInDatabase should be true", true, dummy.IsInDatabase);

			dummy.Z0_Code = "BBB";
			AssertEquals("IsInDatabase should still be true", true, dummy.IsInDatabase);
			Factory.Save();

			dummy.Delete();
			AssertEquals("IsInDatabase should still be true after deletion but before saving", true, dummy.IsInDatabase);
			Factory.Save();
			AssertEquals("IsInDatabase should be false after it's deleted and saved", false, dummy.IsInDatabase);
		}

		#endregion

		public void TestDeleteForDataRefresh()
		{
			var bizO1 = Factory.New<DummyBusinessObject>();
			var bizO2 = Factory.New<DummyBusinessObject>();
			var collection1 = new DummyBusinessObjectCollection(Factory);
			var child1 = collection1.AddNew();
			Factory.Save();

			var bizO3 = Factory.New<DummyBusinessObject>();
			var collection2 = new DummyBusinessObjectCollection(Factory);
			var child2 = collection2.AddNew();

			bizO1.RegisterEditableChildObject(bizO2);
			bizO1.RegisterEditableChildObject(bizO3);
			bizO1.RegisterEditableChildObject(collection1);
			bizO1.RegisterEditableChildObject(collection2);

			IBusiness b = bizO1;
			AssertEquals(4, b.Children.Length);

			b.DeleteForDataRefresh();

			Assert(bizO2.IsInDatabase);
			Assert(!bizO2.IsDeleted);
			Assert(!bizO3.IsInDatabase);
			Assert(bizO3.IsDeleted);

			Assert(child1.IsInDatabase);
			Assert(!child1.IsDeleted);
			Assert(!child2.IsInDatabase);
			Assert(child2.IsDeleted);
		}

		#region TestSetPropertyValueHandleClusterKeyChange

		public void TestSetPropertyValueHandleClusterKeyChange()
		{
			var master = Factory.New<DummyMasterClusterKey>();
			master.Z0_Number = 111;
			_ = master.Dependents.AddNew();
			_ = master.Dependents2.AddNew();
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var masterInAnotherFactory = anotherFactory.Load<DummyMasterClusterKey>(master.PK);
			masterInAnotherFactory.Z0_Number = 222;

			CombineAssertions(() =>
			{
				AssertEquals("child cluster key should be updated with parent before save", 222, masterInAnotherFactory.Dependents[0].ZD1_Number);
				AssertEquals("child 2 cluster key should be updated with parent before save", 222, masterInAnotherFactory.Dependents2[0].ZD1_Number);
			});
		}

		class DummyMasterClusterKey : DummyBaseBusinessObject, IClusterKeyMasterEntity
		{
			public DummyMasterClusterKey(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ChildEditable]
			public DummyDependentBusinessObjectClusterKeyCollection Dependents
			{
				get
				{
					if (fDependents == null)
					{
						fDependents = new DummyDependentBusinessObjectClusterKeyCollection(this, Factory);
						fDependents.Load();

						RegisterEditableChildObject(fDependents);
					}

					return fDependents;
				}
			}
			DummyDependentBusinessObjectClusterKeyCollection fDependents;

			[ChildEditable]
			public DummyDependentBusinessObjectClusterKeyCollection Dependents2
			{
				get
				{
					if (fDependents2 == null)
					{
						fDependents2 = new DummyDependentBusinessObjectClusterKeyCollection(this, Factory);
						fDependents2.Load();

						RegisterEditableChildObject(fDependents2);
					}

					return fDependents2;
				}
			}
			DummyDependentBusinessObjectClusterKeyCollection fDependents2;

			public ZPropertyInfoInt ClusterKeyPty => (ZPropertyInfoInt)Z0_NumberInfo;
		}

		class DummyDependentBusinessObjectClusterKeyCollection : DependentBusinessObjectCollection<DummyDependantWithClusterKeyBusinessObject, DummyBaseBusinessObject>
		{
			public DummyDependentBusinessObjectClusterKeyCollection(DummyBaseBusinessObject parent, BusinessObjectFactory factory) : base(parent, factory)
			{
			}

			public DummyDependentBusinessObjectClusterKeyCollection(DummyBaseBusinessObject parent, ZQuery filter) : base(parent, filter)
			{
			}

			public DummyDependentBusinessObjectClusterKeyCollection(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		#endregion

		public void TestIsValidConcurrencyPolicy()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummyRow = ((INeedRow)dummy).Row;
			var lightValidationColumnName = ((ILightValidationInternals)dummy).IsValidSchemaColumn.Name;

			var concurrencyPolicy = ConcurrencyInfo.Get(dummyRow, dummyRow.Table.Columns[lightValidationColumnName]);

			AssertSame("Should use default concurrency policy for new row", ConcurrencyPolicy.Default, concurrencyPolicy);

			Factory.Save(); // To ensure correct concurrency is applied to column IsValid in method EnsureLightValidationIsIgnoredForConcurrency()
			dummy.OnSaving(); // Refresh concurrency policy after row was saved to db

			concurrencyPolicy = ConcurrencyInfo.Get(dummyRow, dummyRow.Table.Columns[lightValidationColumnName]);

			AssertSame("Should use special concurrency policy for light validation field for row in db", LightValidationConcurrencyPolicy.Instance, concurrencyPolicy);

			var reloadedDummy = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyBusinessObject>(dummy.PK);
			var reloadedDummyRow = ((INeedRow)reloadedDummy).Row;

			reloadedDummy.OnSaving(); // Initialize concurrency policy

			concurrencyPolicy = ConcurrencyInfo.Get(reloadedDummyRow, reloadedDummyRow.Table.Columns[lightValidationColumnName]);

			AssertSame("Should use special concurrency policy for light validation field for row in db", LightValidationConcurrencyPolicy.Instance, concurrencyPolicy);
		}

		#region TestReportRowDeletedError_CalledFromPropertyDescriptor

		public void TestReportRowDeletedError_CalledFromPropertyDescriptor()
		{
			ErrorReporter.Clear();

			var reflectDescriptor = new KReflectPropertyDescriptor(typeof(DummyBusinessObject), typeof(DummyBusinessObject).GetProperty("Z0_Description"));
			AssertEquals("Default", BizO.Z0_Description);
			AssertEquals("Default", BizO.Z0_DescriptionInfo.Value);
			AssertEquals("Default", reflectDescriptor.GetValue(BizO));
			AssertEquals("", ErrorReporter.LastMessageReported);

			BizO.Delete();
			AssertEquals("", BizO.Z0_Description);
			AssertStartsWith("should report error", "Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			AssertEquals("", BizO.Z0_DescriptionInfo.Value);
			AssertEquals("", reflectDescriptor.GetValue(BizO));
			AssertEquals("should ignore error if called from PropertyDescriptor", "", ErrorReporter.LastMessageReported);
		}

		#endregion TestReportRowDeletedError_CalledFromPropertyDescriptor

		#region Implementation

		DummyBusinessObject BizO;
		bool EventFired;

		protected override void RunTest()
		{
			using (BizO.SuspendValidationTesting())
			{
				base.RunTest();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			BizO = Factory.New<DummyBusinessObject>();
		}

		void BusinessObjectTest_ListChanged(object sender, ListChangedEventArgs e)
		{
			EventFired = true;
		}

		class TestOverflowDummyCollection : DummyBusinessObjectCollection
		{
			public TestOverflowDummyCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override void Remove(BusinessObject elementToRemove)
			{
				RemoveAndDelete(elementToRemove);
			}
		}

		class TestOverflowDummy : DummyBusinessObject
		{
			public TestOverflowDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			int DeleteCallCount;
			public override void Delete()
			{
				DeleteCallCount++;
				if (DeleteCallCount > 2)
				{
					throw new Exception("Delete will lead to a StackOverflowException");
				}
				base.Delete();
			}
		}

		#endregion
	}

	class DummyUnsavablePersistentObjectThatRequiresRefresh : DummyBusinessObject
	{
		public DummyUnsavablePersistentObjectThatRequiresRefresh(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool IsSavedByFactory
		{
			get
			{
				return false;
			}
		}
		protected override bool IsForcedPublish
		{
			get
			{
				return true;
			}
		}
	}

	#region Test BusinessObject Changed

	sealed class TestBusinessObjectChanged : TestCaseWithDummy
	{
		[ExpectNoExceptions]
		public void TestValueChangedDefaulter()
		{
			var valueSetStrategyMock = new Mock<IValueSetStrategy>();
			var mock = Factory.NewMoq<DummyBusinessObject>();
			mock.Protected().Setup<IValueSetStrategy>("GetValueSetStrategy").Returns(valueSetStrategyMock.Object);
			valueSetStrategyMock.Setup(m => m.ValueSet(It.IsAny<ZPropertyInfo>(), It.IsAny<IZType>()));
			mock.Object.Z0_AnotherDate = ZDate.BrettsBirthday;
			mock.VerifyAll();
			mock.Object.Z0_AnotherDate = ZDate.Empty;
			mock.VerifyAll();
		}

		public void TestMultipleEventsAreFiredFromModifyingBusinessObjects()
		{
			Event1Fired = false;
			Event2Fired = false;

			Dummy.Z0_DescriptionInfo.ValueChanged += new EventHandler(Dummy_Z0_DescriptionChangedEvent1);

			Dummy.Z0_Description = "This should fire the 1st event";

			AssertEquals("Event 1 Fired", true, Event1Fired);
			AssertEquals("Event 2 Fired", false, Event2Fired);

			Dummy.Z0_DescriptionInfo.ValueChanged += new EventHandler(Dummy_Z0_DescriptionChangedEvent2);

			Event1Fired = false;
			Event2Fired = false;

			Dummy.Z0_Description = "This should also fire both events";
			AssertEquals("Event 1 Fired", true, Event1Fired);
			AssertEquals("Event 2 Fired", true, Event2Fired);
		}

		#region Test LoadChildEditableObjecst Fetch improvement
		public void TestFetchHint()
		{
			var mainObj = Factory.New<DummyBusinessObjectMain>();
			mainObj.Z0_NVarChar = "MAIN";
			for (int i = 1; i < 21; i++)
			{
				if ((i % 2) == 0)
				{
					var data1 = mainObj.Data1Collection.AddNew();
					data1.Z0_NVarChar = "D1_" + i.ToString();
					for (int j = 1; j < 5; j++)
					{
						var data2 = data1.Data2Collection.AddNew();
						data2.Z0_NVarChar = "D1_D2_" + i.ToString() + "-" + j.ToString();
						for (int k = 1; k < 4; k++)
						{
							var data3 = data2.Data1Collection.AddNew();
							data3.Z0_NVarChar = "D1_D2_D3_" + i.ToString() + "-" + j.ToString() + "-" + k.ToString();
						}
					}
				}
				else
				{
					var data2 = mainObj.Data2Collection.AddNew();
					data2.Z0_NVarChar = "D2_" + i.ToString();
					for (int j = 1; j < 5; j++)
					{
						var data1 = data2.Data1Collection.AddNew();
						data1.Z0_NVarChar = "D2_D1_" + i.ToString() + "-" + j.ToString();
						for (int k = 1; k < 4; k++)
						{
							var data3 = data1.Data2Collection.AddNew();
							data3.Z0_NVarChar = "D2_D1_D3_" + i.ToString() + "-" + j.ToString() + "-" + k.ToString();
						}
					}
				}
			}
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyBusinessObjectMain mainObj2 = newFactory.Load<DummyBusinessObjectMain>(mainObj.PK);

			AssertEquals(1, newFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));

			mainObj2.LoadChildEditableObjects();
			AssertEquals(9, newFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertEquals(10, mainObj2.Data1Collection.Count);
			foreach (DummyBusinessObject1 obj1 in mainObj2.Data1Collection)
			{
				AssertEquals(4, obj1.Data2Collection.Count);
				foreach (DummyBusinessObject2 obj3 in obj1.Data2Collection)
				{
					AssertEquals(3, obj3.Data1Collection.Count);
				}
			}
			AssertEquals(10, mainObj2.Data2Collection.Count);
			foreach (DummyBusinessObject2 obj2 in mainObj2.Data2Collection)
			{
				AssertEquals(4, obj2.Data1Collection.Count);
				foreach (DummyBusinessObject1 obj3 in obj2.Data1Collection)
				{
					AssertEquals(3, obj3.Data2Collection.Count);
				}
			}
		}

		public void TestLoadChildEditableObjectsAddFetchHintsOnlyOnce()
		{
			var mainObj = Factory.New<DummyBusinessObjectMain>();
			mainObj.Z0_NVarChar = "MAIN";
			var data1 = mainObj.Data1Collection.AddNew();
			data1.Z0_NVarChar = "D1_1";
			var data2 = data1.Data2Collection.AddNew();
			data2.Z0_NVarChar = "D1_D2_1";
			var data3 = data2.Data1Collection.AddNew();
			data3.Z0_NVarChar = "D1_D2_D3_1";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var mainObj2 = newFactory.Load<DummyBusinessObjectMain>(mainObj.PK);
			mainObj2.LoadChildEditableObjects();
			AssertEquals("No fetch hints", 0, newFactory.ActiveFetchHintsForTable(DummyBusinessObject1.Schema.TableName));
			mainObj2.LoadChildEditableObjects();
			AssertEquals("No fetch hints", 0, newFactory.ActiveFetchHintsForTable(DummyBusinessObject1.Schema.TableName));
		}

		public void TestQueryCacheLoadWithBlobs()
		{
			string message = "Hello World, How are you today?" + System.Environment.NewLine;
			StringBuilder builder = new StringBuilder(message.Length * 10);
			for (int a = 0; a < 10; a++)
			{
				builder.Append(message);
			}
			string longText = builder.ToString();
			for (int i = 1; i < 11; i++)
			{
				DummyBusinessObject data1 = Factory.New<DummyBusinessObject>();
				data1.Z0_NVarChar = "D1" + i.ToString();
				data1.Z0_VarCharMax = data1.Z0_NVarChar + longText;
			}
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ZQuery data1Query = new ZQuery(DummyBizoSchema.Z0_Code, "ZZ1");
			data1Query.ClearBlobs();
			DummyBusinessObject[] data1sWithoutBlobs = newFactory.Load<DummyBusinessObject>(data1Query);

			int dbhits = newFactory.DatabaseLoadCount;
			var tableSelects = newFactory.TableSelects;
			AssertEquals(1, tableSelects.Length);
			AssertEquals(1, tableSelects[0].Value);
			foreach (DummyBusinessObject data1 in data1sWithoutBlobs)
			{
				string text = data1.Z0_NVarChar + longText;
				DataRow row = ((IBusinessObjectInternals)data1).Row;
				AssertEquals("", row[DummyBizoSchema.Constants.Z0_VarCharMax]);
				AssertEquals(text, data1.Z0_VarCharMax);
				AssertEquals(text, row[DummyBizoSchema.Constants.Z0_VarCharMax]);
			}
			AssertEquals(data1sWithoutBlobs.Length + dbhits, newFactory.DatabaseLoadCount);
			tableSelects = newFactory.TableSelects;
			AssertEquals(1, tableSelects.Length);
			AssertEquals(1 + data1sWithoutBlobs.Length, tableSelects[0].Value);

			newFactory = new BusinessObjectFactory();
			data1Query.IncludeBlob(DummyBizoSchema.Z0_VarCharMax);
			DummyBusinessObject[] data1sWithBlobs = newFactory.Load<DummyBusinessObject>(data1Query);

			tableSelects = newFactory.TableSelects;
			AssertEquals(1, tableSelects.Length);
			AssertEquals(1, tableSelects[0].Value);
			dbhits = newFactory.DatabaseLoadCount;
			foreach (DummyBusinessObject data1 in data1sWithBlobs)
			{
				string text = data1.Z0_NVarChar + longText;
				DataRow row = ((IBusinessObjectInternals)data1).Row;
				AssertEquals(text, row[DummyBizoSchema.Constants.Z0_VarCharMax]);
				AssertEquals(text, data1.Z0_VarCharMax);
				AssertEquals(text, row[DummyBizoSchema.Constants.Z0_VarCharMax]);
			}
			AssertEquals(dbhits, newFactory.DatabaseLoadCount);
			tableSelects = newFactory.TableSelects;
			AssertEquals(1, tableSelects.Length);
			AssertEquals(1, tableSelects[0].Value);

			newFactory = new BusinessObjectFactory();
			data1Query.ClearBlobs();
			data1sWithoutBlobs = newFactory.Load<DummyBusinessObject>(data1Query);
			tableSelects = newFactory.TableSelects;
			AssertEquals(1, tableSelects.Length);
			AssertEquals(1, tableSelects[0].Value);
			data1Query.IncludeBlob(DummyBizoSchema.Z0_VarCharMax);
			data1sWithBlobs = newFactory.Load<DummyBusinessObject>(data1Query);
			tableSelects = newFactory.TableSelects;
			AssertEquals(1, tableSelects.Length);
			AssertEquals(1, tableSelects[0].Value);

			dbhits = newFactory.DatabaseLoadCount;
			foreach (DummyBusinessObject data1 in data1sWithBlobs)
			{
				string text = data1.Z0_NVarChar + longText;
				DataRow row = ((IBusinessObjectInternals)data1).Row;
				AssertEquals(text, row[DummyBizoSchema.Constants.Z0_VarCharMax]);
				AssertEquals(text, data1.Z0_VarCharMax);
				AssertEquals(text, row[DummyBizoSchema.Constants.Z0_VarCharMax]);
			}
			AssertEquals(dbhits, newFactory.DatabaseLoadCount);
			tableSelects = newFactory.TableSelects;
			AssertEquals(1, tableSelects.Length);
			AssertEquals(1, tableSelects[0].Value);
		}

		class DummyBusinessObject1 : DummyBusinessObject
		{
			public DummyBusinessObject1(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[ChildEditable]
			public DummyBusinessObject2Collection Data2Collection
			{
				get
				{
					if (data2Collection == null)
					{
						data2Collection = new DummyBusinessObject2Collection(this);
						RegisterEditableChildObject(data2Collection);
					}
					return data2Collection;
				}
			}
			public DummyBusinessObject2Collection data2Collection;

			protected override IBusinessObjectFetchStrategy GetFetchStrategy()
			{
				return new Strategy(this);
			}

			class Strategy : BusinessObjectFetchStrategy
			{
				public Strategy(DummyBusinessObject bizObj)
					: base(bizObj)
				{
				}

				protected override void FetchForLoadCore()
				{
					base.FetchForLoadCore();
					Factory.AddFetchHint(DummyBizoSchema.Z0_Guid, BusinessObject.PK);
				}
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				Z0_Code = "ZZ1";
			}
		}

		class DummyBusinessObject1Collection : BusinessObjectCollection<DummyBusinessObject1>
		{
			public DummyBusinessObject1Collection(DummyBusinessObject master)
				: base(master.Factory, GetFilter(master))
			{
				this.master = master;
			}
			readonly DummyBusinessObject master;

			static ZQuery GetFilter(DummyBusinessObject master)
			{
				ZQuery query = new ZQuery(DummyBizoSchema.Z0_Guid, master.PK);
				query.AddToFilter(DummyBizoSchema.Z0_Code, "ZZ1");
				query.FetchOnlyFromLocalCache = !master.IsInDatabase;
				return query;
			}

			protected internal override void SetCollectionRelationships(BusinessObject child)
			{
				base.SetCollectionRelationships(child);
				DummyBusinessObject1 data = (DummyBusinessObject1)child;
				data.Z0_Guid = master.PK;
			}
		}

		class DummyBusinessObject2 : DummyBusinessObject
		{
			public DummyBusinessObject2(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[ChildEditable]
			public DummyBusinessObject1Collection Data1Collection
			{
				get
				{
					if (data1Collection == null)
					{
						data1Collection = new DummyBusinessObject1Collection(this);
						data1Collection.Load();
						RegisterEditableChildObject(data1Collection);
					}
					return data1Collection;
				}
			}
			public DummyBusinessObject1Collection data1Collection;

			protected override IBusinessObjectFetchStrategy GetFetchStrategy()
			{
				return new Strategy(this);
			}

			class Strategy : BusinessObjectFetchStrategy
			{
				public Strategy(DummyBusinessObject bizObj)
					: base(bizObj)
				{
				}

				protected override void FetchForLoadCore()
				{
					base.FetchForLoadCore();
					Factory.AddFetchHint(DummyBizoSchema.Z0_Guid, BusinessObject.PK);
				}
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				Z0_Code = "ZZ2";
			}
		}

		class DummyBusinessObject2Collection : ActiveBusinessObjectCollection<DummyBusinessObject2>
		{
			public DummyBusinessObject2Collection(DummyBusinessObject master)
				: base(master.Factory, master, GetFilter(master), DummyBizoSchema.Z0_Guid)
			{
			}

			static ZQuery GetFilter(DummyBusinessObject master)
			{
				ZQuery query = new ZQuery(DummyBizoSchema.Z0_Code, "ZZ2");
				query.FetchOnlyFromLocalCache = !master.IsInDatabase;
				return query;
			}

			protected override bool MatchesFilterCore(DummyBusinessObject2 element, bool fetchOnlyFromLocalCache)
			{
				return element.Z0_Code == "ZZ2" && base.MatchesFilterCore(element, fetchOnlyFromLocalCache);
			}
		}

		class DummyBusinessObjectMain : DummyBusinessObject
		{
			public DummyBusinessObjectMain(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[ChildEditable]
			public DummyBusinessObject1Collection Data1Collection
			{
				get
				{
					if (data1Collection == null)
					{
						data1Collection = new DummyBusinessObject1Collection(this);
						data1Collection.Load();
						RegisterEditableChildObject(data1Collection);
					}
					return data1Collection;
				}
			}
			public DummyBusinessObject1Collection data1Collection;

			[ChildEditable]
			public DummyBusinessObject2Collection Data2Collection
			{
				get
				{
					if (data2Collection == null)
					{
						data2Collection = new DummyBusinessObject2Collection(this);
						RegisterEditableChildObject(data2Collection);
					}
					return data2Collection;
				}
			}
			public DummyBusinessObject2Collection data2Collection;

			protected override IBusinessObjectFetchStrategy GetFetchStrategy()
			{
				return new Strategy(this);
			}

			class Strategy : BusinessObjectFetchStrategy
			{
				public Strategy(DummyBusinessObjectMain bizObj)
					: base(bizObj)
				{
				}

				protected override void FetchForLoadCore()
				{
					base.FetchForLoadCore();
					Factory.AddFetchHint(DummyBizoSchema.Z0_Guid, BusinessObject.PK);
					Factory.AddFetchHint(typeof(DummyBusinessObject1), new ZQuery(DummyBizoSchema.Z0_Code, new ZString("ZZ1")), new ZQuery(DummyBizoSchema.Z0_Guid, BusinessObject.PK));
					Factory.AddFetchHint(typeof(DummyBusinessObject2), new ZQuery(DummyBizoSchema.Z0_Code, new ZString("ZZ2")), new ZQuery(DummyBizoSchema.Z0_Guid, BusinessObject.PK));
				}
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				Z0_Code = "ZZ!";
			}
		}
		#endregion

		#region Implementation

		bool Event1Fired;
		bool Event2Fired;

		void Dummy_Z0_DescriptionChangedEvent1(object sender, EventArgs e)
		{
			Event1Fired = true;
		}

		void Dummy_Z0_DescriptionChangedEvent2(object sender, EventArgs e)
		{
			Event2Fired = true;
		}

		#endregion
	}

	#endregion

	sealed class TestBusinessObjectChanged2 : TestCaseWithFactory
	{
		[Data.Testing.UseSnapshotProtection]
		public void TestLoadWithBlobsWithConcurrencyError()
		{
			NotificationHandler.Instance = new ZGUINotificationHandler();

			using (RunNonTransactioned())
			{
				// Create a concurrency exception
				var newFactory1 = new BusinessObjectFactory();
				var data = newFactory1.New<DummyDependantBusinessObject>();
				data.ZD1_Number = 0;
				newFactory1.Save();

				var data2 = Factory.Load<DummyDependantBusinessObject>(data.PK);
				data2.ZD1_Number = 2;

				var sql = @"UPDATE dbo.DummyDependentBizo SET ZD1_Number = 1 WHERE ZD1_PK = @PK";
				using (var command = Db.Connection.Command(sql))
				{
					command.AddParameter("@PK", SqlDbType.UniqueIdentifier, data.PK.ToGuid());
					command.ExecuteNonQuery();
				}

				// Set Stream on Bizo
				var data3 = Factory.New<DummyBusinessObject>();
				using (Stream stream = new IO.Shim.SubStreamableStream())
				{
					var encoder = new UTF8Encoding();
					byte[] inputByteArray = encoder.GetBytes("ABC");
					stream.Write(inputByteArray, 0, inputByteArray.Length);
					stream.Flush();
					var source = new StreamSource(stream);
					data3.SetZ0_VarBinaryMaxSource(source);

					// Ensure that the concurrency error occurs in the same transaction but in a different SQL query.
					for (var i = 0; i < ZSqlSaver.MaxRowsPerMultiRowInsertStatement; i++)
					{
						Factory.New<DummyBusinessObject>();
					}

					var e = AssertExceptionThrown<ZSaveConcurrencyException>(() =>
					{
						Factory.Save();
					});
					ZExceptionReporting.HandleSaveException(e);

					AssertNoExceptionThrown("Should not get a 'Stream Closed' inner exception.", () =>
					{
						Factory.Save();
					});
				}
			}
		}
	}
}
