using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using Enterprise.Integration.UniversalCopy;
using Moq;
using NUnit.Framework;
using WTG.Glow.Data.Annotations;

namespace CargoWise.UniversalCopy.Test
{
	public class CopyManagerTest : TestCase
	{
		#region TestPrepareAndFinishCopy

		public void TestPrepareAndFinishCopy()
		{
			TestObject1 source = new TestObject1();
			CopyTemplateTree copyTemplate = new CopyTemplateTree(typeof(TestObject1));
			PropertyCopyTemplateNode propertyNode =
				(PropertyCopyTemplateNode)((EntityCopyTemplateNode)copyTemplate.InnerNode).Nodes.
				Find(node => node is PropertyCopyTemplateNode && node.Name == "Code");
			propertyNode.CopyMethod = CopyMethod.Copy;

			var copyManager = new DefectiveCopyManager();
			Assert("Precondition - not in copy yet.", !copyManager.InsideCopy);

			bool testActionFired = false;
			copyManager.TestAction = () =>
			{
				Assert("Should be in copy here.", copyManager.InsideCopy);
				testActionFired = true;
			};

			AssertExceptionThrown(typeof(ApplicationException), () => copyManager.Copy(source, copyTemplate));

			Assert("Should exit from copy and FinishCopy() should complete despite excetption.", !copyManager.InsideCopy);
			Assert("TestAction should have been fired.", testActionFired);
		}

		class DefectiveCopyManager : TestCopyManager
		{
			public bool InsideCopy { get; private set; }
			public Action TestAction { get; set; }

			protected override void PrepareForCopy(object source, CopyTemplateTree copyTemplate)
			{
				base.PrepareForCopy(source, copyTemplate);
				InsideCopy = true;
			}

			protected override void FinishCopy(object source, object copy, CopyTemplateTree copyTemplate)
			{
				InsideCopy = false;
				base.FinishCopy(source, copyTemplate, copyTemplate);
			}

			protected override object GetPropertyValueCore(object source, string propertyName)
			{
				if (TestAction != null)
				{
					TestAction();
				}
				throw new ApplicationException();
			}
		}

		#endregion

		#region TestCopyProperties

		public void TestCopyProperties()
		{
			AssertCopyProperty(CopyMethod.None, "Code", o => o.Code = "abc", "xyz", o => o.Code, "def");
			AssertCopyProperty(CopyMethod.Default, "Code", o => o.Code = "abc", "xyz", o => o.Code, "123");
			AssertCopyProperty(CopyMethod.Empty, "Code", o => o.Code = "abc", "xyz", o => o.Code, string.Empty);
			AssertCopyProperty(CopyMethod.Copy, "Code", o => o.Code = "abc", "xyz", o => o.Code, "abc");
			AssertCopyProperty(CopyMethod.Value, "Code", o => o.Code = "abc", "xyz", o => o.Code, "xyz");
			AssertCopyProperty(CopyMethod.Property, "Code", o => { o.Code = "abc"; o.Text = "qwerty"; }, "Text", o => o.Code, "qwerty");
			AssertCopyProperty(CopyMethod.Macro, "Code", o => o.Code = "abc", "<SomeMacro>", o => o.Code, "Hello World!");

			AssertCopyProperty(CopyMethod.None, "Number", o => o.Number = 10, 20, o => o.Number, 3);
			AssertCopyProperty(CopyMethod.Default, "Number", o => o.Number = 10, 20, o => o.Number, 5);
			AssertCopyProperty(CopyMethod.Empty, "Number", o => o.Number = 10, 20, o => o.Number, 0);
			AssertCopyProperty(CopyMethod.Copy, "Number", o => o.Number = 10, 20, o => o.Number, 10);
			AssertCopyProperty(CopyMethod.Value, "Number", o => o.Number = 10, 20, o => o.Number, 20);
			AssertCopyProperty(CopyMethod.Property, "Number", o => { o.Number = 10; o.Text = "20"; }, "Text", o => o.Number, 20);

			AssertCopyProperty(CopyMethod.Value, "Number", o => o.Number = 10, "abc", o => o.Number, 10, "Cannot set property TestObject1.Number of type Int32 with value 'abc'.");
			AssertCopyProperty(CopyMethod.Macro, "Number", o => o.Number = 10, "<SomeMacro>", o => o.Number, 0, "Cannot set property TestObject1.Number of type Int32 with value 'Hello World!'.");
		}

		public void TestCopyProperties_CanHandleInvalidData()
		{
			AssertCopyProperty(CopyMethod.Value, "ScheduledDate", o => o.ScheduledDate = new DateTime(2019, 12, 17, 12, 36, 32), @"""Thursday, 05 December 2019 15:27:26""", o => o.ScheduledDate, null, "Cannot set property TestObject1.ScheduledDate of type DateTime with value '\"Thursday, 05 December 2019 15:27:26\"'.");
			AssertCopyProperty(CopyMethod.Value, "IsValid", o => o.IsValid = false, @"04-12-2019 10:00", o => o.IsValid, null, "Cannot set property TestObject1.IsValid of type Boolean with value '04-12-2019 10:00'.");
			AssertCopyProperty(CopyMethod.Property, "ScheduledDate", o => { o.ScheduledDate = new DateTime(2019, 12, 17, 12, 36, 32); o.Text = @"""Thursday, 05 December 2019 15:27:26"""; }, "Text", o => o.ScheduledDate, null, "Cannot set property TestObject1.ScheduledDate of type DateTime with value '\"Thursday, 05 December 2019 15:27:26\"'.");
		}

		void AssertCopyProperty(CopyMethod copyMethod, string propertyName, Action<TestObject1> initializeObject, object configurationValue, Func<TestObject1, object> getActualValue, object expectedValue, string expectedError = null)
		{
			TestObject1 source = new TestObject1();
			initializeObject(source);

			CopyTemplateTree copyTemplate = new CopyTemplateTree(typeof(TestObject1));

			PropertyCopyTemplateNode propertyNode =
				(PropertyCopyTemplateNode)((EntityCopyTemplateNode)copyTemplate.InnerNode).Nodes.
				Find(node => node is PropertyCopyTemplateNode && node.Name == propertyName);
			propertyNode.CopyMethod = copyMethod;
			propertyNode.Value = configurationValue;

			TestCopyManager copyManager = new TestCopyManager();
			var copyResult = copyManager.Copy(source, copyTemplate);
			var target = copyResult.Object;

			if (target != null)
			{
				AssertEquals(expectedValue, getActualValue((TestObject1)target));
			}
			else
			{
				AssertNotNull(copyResult.ErrorMessage);
				AssertEquals(expectedError, copyResult.ErrorMessage);
			}

			if (copyMethod == CopyMethod.None)
			{
				propertyNode.CopyMethod = CopyMethod.Value;
				copyResult = copyManager.Copy(source, copyTemplate);
				target = copyResult.Object;
				if (target != null)
				{
					AssertNotEquals(expectedValue, getActualValue((TestObject1)target));
				}
				else
				{
					AssertNotNull(copyResult.ErrorMessage);
					AssertEquals(expectedError, copyResult.ErrorMessage);
				}
			}
		}

		[TableNameProvider("TestObject1")]
		interface ITestObject1 { }

		class TestObject1 : ITestObject1
		{
			[DefaultValue("123")]
			public string Code
			{
				get { return code; }
				set { code = value; }
			}
			string code = "def";

			[DefaultValue("Hello")]
			public string Text
			{
				get { return text; }
				set { text = value; }
			}
			string text = "World";

			[DefaultValue(5)]
			public int Number
			{
				get { return number; }
				set { number = value; }
			}
			int number = 3;

			public DateTime ScheduledDate { get; set; }

			public bool IsValid { get; set; }
		}

		#endregion

		#region TestCopyPropertiesOnDataRow

		public void TestCopyPropertiesOnDataRow()
		{
			var testDate = new DateTime(2016, 10, 25, 14, 44, 17);
			AssertCopyPropertyOnDataRow(CopyMethod.Copy, typeof(DateTime), false, testDate, null, testDate);
			AssertCopyPropertyOnDataRow(CopyMethod.Copy, typeof(DateTime), true, testDate, null, testDate);
			AssertCopyPropertyOnDataRow(CopyMethod.Empty, typeof(DateTime), false, testDate, null, new DateTime());
			AssertCopyPropertyOnDataRow(CopyMethod.Empty, typeof(DateTime), true, testDate, null, DBNull.Value);

			const int TestNumber = 123;
			AssertCopyPropertyOnDataRow(CopyMethod.Copy, typeof(int), false, TestNumber, null, TestNumber);
			AssertCopyPropertyOnDataRow(CopyMethod.Copy, typeof(int), true, TestNumber, null, TestNumber);
			AssertCopyPropertyOnDataRow(CopyMethod.Empty, typeof(int), false, TestNumber, null, 0);
			AssertCopyPropertyOnDataRow(CopyMethod.Empty, typeof(int), true, TestNumber, null, DBNull.Value);
		}

		public void AssertCopyPropertyOnDataRow(CopyMethod copyMethod, Type columnDataType, bool isNullable, object sourceValue, object configurationValue, object expectedValue)
		{
			const string ColumnName = "C1";

			var table = new DataTable();
			var column = table.Columns.Add(ColumnName);
			column.DataType = columnDataType;
			column.AllowDBNull = isNullable;
			column.DefaultValue =
				isNullable
					? DBNull.Value
					: columnDataType == typeof(string)
						? string.Empty
						: Activator.CreateInstance(columnDataType);

			var sourceRow = table.NewRow();
			table.Rows.Add(sourceRow);
			sourceRow[ColumnName] = sourceValue;

			var copyTemplate = new CopyTemplateTree();
			var entityNode = new EntityCopyTemplateNode();
			copyTemplate.InnerNode = entityNode;

			var propertyNode = new PropertyCopyTemplateNode();
			propertyNode.Name = ColumnName;
			propertyNode.PropertyType = columnDataType.Name;
			propertyNode.CopyMethod = copyMethod;
			propertyNode.Value = configurationValue;
			entityNode.Nodes.Add(propertyNode);

			var copyManager = new TestCopyManager();
			var target = copyManager.Copy(sourceRow, copyTemplate).Object;

			AssertNotNull(target);
			Assert(target.GetType().Name, target is DataRow);
			AssertEquals(expectedValue, ((DataRow)target)[ColumnName]);
		}

		#endregion

		#region TestSetPropertyValueOnRowWithMaxLength

		public void TestSetPropertyValueOnRowWithMaxLength()
		{
			var dataTable = new DataTable();
			var column = dataTable.Columns.Add("Code", typeof(string));
			column.MaxLength = 3;
			var row = dataTable.NewRow();
			dataTable.Rows.Add(row);
			row[0] = "";

			AssertEquals("", row[0]);

			var copyManager = new TestCopyManager();
			copyManager.SetPropertyValueExposed(row, "Code", "ABCDE");

			AssertEquals("Trimmed value should be set without exceptions", "ABC", row[0]);
		}

		#endregion

		#region TestSetPropertyValue_SourceValueIsDBNullAndAllowDBNull

		public void TestSetPropertyValue_SourceValueIsDBNullAndAllowDBNull()
		{
			CombineAssertions(() =>
			{
				AssertSetPropertyValue_SourceValueIsDBNullAndAllowDBNull(typeof(string));
				AssertSetPropertyValue_SourceValueIsDBNullAndAllowDBNull(typeof(DateTime));
				AssertSetPropertyValue_SourceValueIsDBNullAndAllowDBNull(typeof(decimal));
				AssertSetPropertyValue_SourceValueIsDBNullAndAllowDBNull(typeof(int));
				AssertSetPropertyValue_SourceValueIsDBNullAndAllowDBNull(typeof(bool));
			});
		}

		public void AssertSetPropertyValue_SourceValueIsDBNullAndAllowDBNull(Type columnDataType)
		{
			const string ColumnName = "C1";

			var table = new DataTable();
			var column = table.Columns.Add(ColumnName);
			column.DataType = columnDataType;
			column.AllowDBNull = true;

			var sourceRow = table.NewRow();
			table.Rows.Add(sourceRow);
			sourceRow[ColumnName] = DBNull.Value;

			var copyTemplate = new CopyTemplateTree();
			var entityNode = new EntityCopyTemplateNode();
			copyTemplate.InnerNode = entityNode;

			var propertyNode = new PropertyCopyTemplateNode();
			propertyNode.Name = ColumnName;
			propertyNode.PropertyType = columnDataType.Name;
			propertyNode.CopyMethod = CopyMethod.Copy;
			entityNode.Nodes.Add(propertyNode);

			var copyManager = new TestCopyManager();
			var target = copyManager.Copy(sourceRow, copyTemplate).Object as DataRow;
			AssertEquals(columnDataType.Name, DBNull.Value, target[ColumnName]);
		}

		#endregion

		#region TestSetPropertyValueOnRowWithNoNullAllowed

		public void TestSetPropertyValueOnRowWithNoNullAllowed()
		{
			var dataTable = new DataTable();
			var column = dataTable.Columns.Add("Code", typeof(string));
			column.AllowDBNull = false;
			column.DefaultValue = "";
			var row = dataTable.NewRow();
			dataTable.Rows.Add(row);

			AssertEquals("Precondition", "", row[0]);

			var copyManager = new TestCopyManager();
			AssertNoExceptionThrown(() => copyManager.SetPropertyValueExposed(row, "Code", null));
			AssertEquals("Initial value should remain", "", row[0]);
		}

		#endregion

		#region TestErrorMessageIsPopuplatedWhenCopyProducesNull

		public void TestErrorMessageIsPopuplatedWhenCopyProducesNull()
		{
			// Arrange
			var source = new TestObject1();
			var copyTemplate = new CopyTemplateTree(typeof(TestObject1));
			var propertyNode = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)copyTemplate.InnerNode).Nodes.Find(node => node is PropertyCopyTemplateNode && node.Name == "Code");
			propertyNode.CopyMethod = CopyMethod.Default;
			propertyNode.Value = "abc";
			var copyManager = new TestCopyManager(true);
			// Act
			var copyResult = copyManager.Copy(source, copyTemplate);
			var target = copyResult.Object;
			var errorMessage = copyResult.ErrorMessage;
			// Assert
			AssertNull(target);
			AssertContains("Template", errorMessage);
			AssertContains("Path", errorMessage);
		}

		#endregion

		#region TestCopyRelatedEntity

		public void TestCopyRelatedEntity()
		{
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.None, RelatedEntityCopyMethod.None, RelatedEntityCopyMethod.None);
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.None, RelatedEntityCopyMethod.Link, RelatedEntityCopyMethod.Link);
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.None, RelatedEntityCopyMethod.Copy, RelatedEntityCopyMethod.Copy);
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.None, RelatedEntityCopyMethod.LinkCopied, RelatedEntityCopyMethod.Link);

			AssertCopyRelatedEntity(RelatedEntityCopyMethod.Copy, RelatedEntityCopyMethod.None, RelatedEntityCopyMethod.None);
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.Copy, RelatedEntityCopyMethod.Link, RelatedEntityCopyMethod.Link);
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.Copy, RelatedEntityCopyMethod.Copy, RelatedEntityCopyMethod.LinkCopied);
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.Copy, RelatedEntityCopyMethod.LinkCopied, RelatedEntityCopyMethod.LinkCopied);

			AssertCopyRelatedEntity(RelatedEntityCopyMethod.Link, RelatedEntityCopyMethod.None, RelatedEntityCopyMethod.None);
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.Link, RelatedEntityCopyMethod.Link, RelatedEntityCopyMethod.Link);
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.Link, RelatedEntityCopyMethod.Copy, RelatedEntityCopyMethod.Copy);
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.Link, RelatedEntityCopyMethod.LinkCopied, RelatedEntityCopyMethod.Link);
		}

		public void TestCopyRelatedEntityWithMandatoryCheck()
		{
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.None, RelatedEntityCopyMethod.None, RelatedEntityCopyMethod.Copy, true);
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.None, RelatedEntityCopyMethod.Copy, RelatedEntityCopyMethod.Copy, true);
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.Copy, RelatedEntityCopyMethod.None, RelatedEntityCopyMethod.Copy, true);
			AssertCopyRelatedEntity(RelatedEntityCopyMethod.Copy, RelatedEntityCopyMethod.Copy, RelatedEntityCopyMethod.LinkCopied, true);
		}

		void AssertCopyRelatedEntity(RelatedEntityCopyMethod copyMethod1, RelatedEntityCopyMethod copyMethod2, RelatedEntityCopyMethod expectedActualCopyMethod2, bool checkMandatory = false)
		{
			TestObject2 source = new TestObject2();
			TestObject2Related relatedEntity = new TestObject2Related();

			source.RelatedObject1 = relatedEntity;
			source.FK1 = relatedEntity.PK;

			source.RelatedObject2 = relatedEntity;
			source.FK2 = relatedEntity.PK;

			CopyTemplateTree copyTemplate = new CopyTemplateTree(typeof(TestObject2));
			foreach (CopyTemplateNode node in ((EntityCopyTemplateNode)copyTemplate.InnerNode).Nodes)
			{
				var relatedNode = node as RelatedEntityCopyTemplateNode;
				if (relatedNode != null)
				{
					if (checkMandatory)
					{
						relatedNode.IsMandatory = true;
					}

					switch (relatedNode.RelatedPropertyName)
					{
						case "FK1":
							relatedNode.CopyMethod = copyMethod1;
							break;
						case "FK2":
							relatedNode.CopyMethod = copyMethod2;
							break;
					}

					var templateNode = relatedNode.InnerNode as TemplateCopyTemplateNode;
					if (templateNode != null)
					{
						templateNode.FindTemplateAndInitializeInnerNode(copyTemplate);
					}
				}
			}

			TestCopyManager copyManager = new TestCopyManager();

			if (checkMandatory)
			{
				if (copyMethod1 == RelatedEntityCopyMethod.None)
				{
					AssertEquals("Mandatory Check", "Related entity RelatedObject1 is mandatory to be copied. Please modify the template you are using.", copyManager.Copy(source, copyTemplate).ErrorMessage);
					return;
				}

				if (copyMethod1 != RelatedEntityCopyMethod.None && copyMethod2 == RelatedEntityCopyMethod.None)
				{
					AssertEquals("Mandatory Check", "Related entity RelatedObject2 is mandatory to be copied. Please modify the template you are using.", copyManager.Copy(source, copyTemplate).ErrorMessage);
					return;
				}
			}

			var target = (TestObject2)copyManager.Copy(source, copyTemplate).Object;
			var linkableEntity = target.RelatedObject1;

			AssertCopyiedEntity(copyMethod1, relatedEntity, null, target.RelatedObject1);
			AssertCopyiedEntity(expectedActualCopyMethod2, relatedEntity, linkableEntity, target.RelatedObject2);
		}

		void AssertCopyiedEntity(RelatedEntityCopyMethod copyMethod, TestEntity sourceEntity, TestEntity linkableEntity, TestEntity targetEntity)
		{
			switch (copyMethod)
			{
				case RelatedEntityCopyMethod.None:
					AssertNull(targetEntity);
					break;
				case RelatedEntityCopyMethod.Copy:
					AssertNotNull(targetEntity);
					AssertNotEquals(sourceEntity.PK, targetEntity.PK);
					Assert(linkableEntity == null || linkableEntity.PK != targetEntity.PK);
					break;
				case RelatedEntityCopyMethod.Link:
					AssertNotNull(targetEntity);
					AssertEquals(sourceEntity.PK, targetEntity.PK);
					break;
				case RelatedEntityCopyMethod.LinkCopied:
					AssertNotNull(targetEntity);
					AssertEquals(linkableEntity.PK, targetEntity.PK);
					break;
			}
		}

		[TableNameProvider("TestObject2")]
		interface ITestObject2 { }

		class TestObject2 : TestEntity, ITestObject2
		{
			public Guid FK1 { get; set; }

			[RelationProperty(nameof(FK1))]
			public TestObject2Related RelatedObject1 { get; set; }

			public Guid FK2 { get; set; }

			[RelationProperty(nameof(FK2))]
			public TestObject2Related RelatedObject2 { get; set; }
		}

		[TableNameProvider("TestObject2Related")]
		interface ITestObject2Related { }

		class TestObject2Related : TestEntity, ITestObject2Related
		{
		}

		#endregion

		#region TestCopyRelatedEntityWithUnsyncModel

		public void TestCopyRelatedEntityWithUnsyncModel()
		{
			TestObject2Actual source = new TestObject2Actual();
			source.FK1 = Guid.NewGuid();
			source.FK2 = Guid.NewGuid();

			CopyTemplateTree copyTemplate = new CopyTemplateTree(typeof(TestObject2));
			foreach (CopyTemplateNode node in ((EntityCopyTemplateNode)copyTemplate.InnerNode).Nodes)
			{
				var relatedNode = node as RelatedEntityCopyTemplateNode;
				if (relatedNode != null)
				{
					switch (relatedNode.RelatedPropertyName)
					{
						case "FK1":
							relatedNode.CopyMethod = RelatedEntityCopyMethod.Link;
							break;
						case "FK2":
							relatedNode.CopyMethod = RelatedEntityCopyMethod.Copy;
							break;
					}

					var templateNode = relatedNode.InnerNode as TemplateCopyTemplateNode;
					if (templateNode != null)
					{
						templateNode.FindTemplateAndInitializeInnerNode(copyTemplate);
					}
				}
			}

			TestCopyManager copyManager = new TestCopyManager();
			TestObject2Actual target = (TestObject2Actual)copyManager.Copy(source, copyTemplate).Object;

			AssertEquals("Copy FK for entity to be linked.", source.FK1, target.FK1);
			AssertEquals("Do nothing for entity to be copied.", Guid.Empty, target.FK2);
		}

		class TestObject2Actual : TestEntity
		{
			public Guid FK1 { get; set; }
			public Guid FK2 { get; set; }
		}

		#endregion

		#region TestCopyCollection

		public void TestCopyCollection()
		{
			TestObject3 source = new TestObject3();
			source.Collection.Add(new TestObject3Item { FK = source.PK });
			source.Collection.Add(new TestObject3Item { FK = source.PK });
			source.Collection.Add(new TestObject3Item { FK = source.PK });

			CopyTemplateTree copyTemplate = new CopyTemplateTree(typeof(TestObject3));
			TestCopyManager copyManager = new TestCopyManager();
			foreach (CopyTemplateNode node in ((EntityCopyTemplateNode)copyTemplate.InnerNode).Nodes)
			{
				CollectionCopyTemplateNode collectionNode = node as CollectionCopyTemplateNode;
				if (collectionNode != null)
				{
					collectionNode.CopyMethod = CollectionCopyMethod.All;
				}
			}

			var target = (TestObject3)copyManager.Copy(source, copyTemplate).Object;

			AssertNotEquals(source.PK, target.PK);

			AssertEquals(3, target.Collection.Count);

			Assert("Items in target collection should be new", source.Collection.All(item => item.PK != target.Collection[0].PK));
			Assert("Items in target collection should be new", source.Collection.All(item => item.PK != target.Collection[1].PK));
			Assert("Items in target collection should be new", source.Collection.All(item => item.PK != target.Collection[2].PK));

			AssertEquals("Correct relationship should be set", target.PK, target.Collection[0].FK);
			AssertEquals("Correct relationship should be set", target.PK, target.Collection[1].FK);
			AssertEquals("Correct relationship should be set", target.PK, target.Collection[2].FK);

			AssertNotEquals("All items should be different instances", target.Collection[0].PK, target.Collection[1].PK);
			AssertNotEquals("All items should be different instances", target.Collection[0].PK, target.Collection[2].PK);
			AssertNotEquals("All items should be different instances", target.Collection[1].PK, target.Collection[2].PK);
		}

		public void TestCopyCollectionWithFilter()
		{
			TestObject3 source = new TestObject3();
			source.Collection.Add(new TestObject3Item { FK = source.PK });
			source.Collection.Add(new TestObject3Item { FK = source.PK });
			source.Collection.Add(new TestObject3Item { FK = source.PK });

			CopyTemplateTree copyTemplate = new CopyTemplateTree(typeof(TestObject3));
			foreach (CopyTemplateNode node in ((EntityCopyTemplateNode)copyTemplate.InnerNode).Nodes)
			{
				CollectionCopyTemplateNode collectionNode = node as CollectionCopyTemplateNode;
				if (collectionNode != null)
				{
					collectionNode.CopyMethod = CollectionCopyMethod.Filter;
					collectionNode.Filter = new EntityFilter();
				}
			}

			TestCopyManager copyManager = new TestCopyManager();
			var target = (TestObject3)copyManager.Copy(source, copyTemplate).Object;

			AssertEquals(2, target.Collection.Count);
			AssertNotEquals(source.Collection[0].PK, target.Collection[0].PK);
			AssertNotEquals(source.Collection[2].PK, target.Collection[1].PK);
		}

		public void TestCopyCollectionWithFilterBeforeGeneralCollection()
		{
			var copyTemplate = GetCopyTemplate(CollectionCopyMethod.Filter);

			var source = new TestObject3();
			source.Collection.Add(new TestObject3Item { Text = "A" }); // Will be filtered
			source.Collection.Add(new TestObject3Item { Text = "B" });
			source.Collection.Add(new TestObject3Item { Text = "C" }); // Will be filtered
			source.Collection.Add(new TestObject3Item { Text = "D" });

			TestCopyManager copyManager = new TestCopyManager();
			var target = (TestObject3)copyManager.Copy(source, copyTemplate).Object;

			AssertEquals(4, target.Collection.Count);
			AssertEquals("Should use filtered collection first", "XYZ", target.Collection[0].Text);
			AssertEquals("Should use filtered collection first", "XYZ", target.Collection[1].Text);
			AssertEquals("Rest elements should be copied with general collection", "B", target.Collection[2].Text);
			AssertEquals("Rest elements should be copied with general collection", "D", target.Collection[3].Text);
		}

		public void TestCopyCollectionWithDeniedFilterBeforeGeneralCollection()
		{
			var copyTemplate = GetCopyTemplate(CollectionCopyMethod.None);

			var source = new TestObject3();
			var test = new TestObject3Item { Text = "A" };
			source.Collection.Add(new TestObject3Item { Text = "A" }); // Will be filtered
			source.Collection.Add(new TestObject3Item { Text = "B" });
			source.Collection.Add(new TestObject3Item { Text = "C" }); // Will be filtered
			source.Collection.Add(new TestObject3Item { Text = "D" });

			TestCopyManager copyManager = new TestCopyManager();
			var target = (TestObject3)copyManager.Copy(source, copyTemplate).Object;

			AssertEquals("Filtered denied elements are ignored", 2, target.Collection.Count);
			AssertEquals("Rest elements should be copied with general collection", "B", target.Collection[0].Text);
			AssertEquals("Rest elements should be copied with general collection", "D", target.Collection[1].Text);
		}

		public void TestCopyCollectionWithDeniedFilterBeforeGeneralCollectionHasSameFilter()
		{
			var copyTemplate = GetCopyTemplate(CollectionCopyMethod.None);

			var source = new TestObject3();
			var test = new TestObject3Item { Text = "A" };
			source.Collection.Add(test); // Will be filtered
			source.Collection.Add(new TestObject3Item { Text = "B" });
			source.Collection.Add(test); // Will be filtered
			source.Collection.Add(new TestObject3Item { Text = "D" });

			TestCopyManager copyManager = new TestCopyManager();
			var target = (TestObject3)copyManager.Copy(source, copyTemplate).Object;

			AssertEquals("Filtered denied elements are ignored", 2, target.Collection.Count);
			AssertEquals("Rest elements should be copied with general collection", "B", target.Collection[0].Text);
			AssertEquals("Rest elements should be copied with general collection", "D", target.Collection[1].Text);
		}

		public void TestCopyCollection_CollectionTypeHasSort_SortsCollection()
		{
			// Arrange
			var source = new TestObject3();
			var item1 = new TestObject3Item { FK = source.PK, Text = "1" };
			var item2 = new TestObject3Item { FK = source.PK, Text = "2" };
			var item3 = new TestObject3Item { FK = source.PK, Text = "3" };
			source.Collection.Add(item1);
			source.Collection.Add(item2);
			source.Collection.Add(item3);

			var entityNode = new EntityCopyTemplateNode { Name = "TestObject3" };
			var copyTemplate = new CopyTemplateTree { Name = "TestObject3", InnerNode = entityNode };
			var itemEntityNode1 = new EntityCopyTemplateNode { Name = "TestObject3Item" };
			itemEntityNode1.Nodes.Add(new PropertyCopyTemplateNode { Name = "Text", CopyMethod = CopyMethod.Copy });
			var collectionNode1 = new CollectionCopyTemplateNode
			{
				Name = "Collection",
				CopyMethod = CollectionCopyMethod.All,
				ItemsTableName = "TestObject3Item",
				ItemPropertyName = "FK",
				ItemParentTablePropertyName = "PK",
				InnerNode = itemEntityNode1
			};
			entityNode.Nodes.Add(collectionNode1);

			var copyManager = new TestCopyManager();

			var mockUniversalCopyTypeCollectionSorter = new Mock<IUniversalCopyTypeCollectionSorter>();
			mockUniversalCopyTypeCollectionSorter.Setup(s => s.ItemsTableName).Returns("TestObject3Item");
			mockUniversalCopyTypeCollectionSorter.Setup(s => s.GetSortedCollection(It.IsAny<IEnumerable>())).Returns(new[] { item2, item3, item1 });

			// Act
			TestObject3 target;
			using (ObjectFactory.Substitute("UniversalCopyTypeCollectionSortersList", new ListObject() { mockUniversalCopyTypeCollectionSorter.Object }))
			{
				target = (TestObject3)copyManager.Copy(source, copyTemplate).Object;
			}

			// Assert
			AssertEquals(3, target.Collection.Count);

			AssertEquals("Items in target collection should be in correct order", "2", target.Collection[0].Text);
			AssertEquals("Items in target collection should be in correct order", "3", target.Collection[1].Text);
			AssertEquals("Items in target collection should be in correct order", "1", target.Collection[2].Text);
		}

		CopyTemplateTree GetCopyTemplate(CollectionCopyMethod copyMethod)
		{
			var entityNode = new EntityCopyTemplateNode { Name = "TestObject3" };
			var copyTemplate = new CopyTemplateTree { Name = "TestObject3", InnerNode = entityNode };

			var itemEntityNode1 = new EntityCopyTemplateNode { Name = "TestObject3Item" };
			itemEntityNode1.Nodes.Add(new PropertyCopyTemplateNode { Name = "Text", CopyMethod = CopyMethod.Copy });
			var collectionNode1 = new CollectionCopyTemplateNode
			{
				Name = "Collection",
				CopyMethod = CollectionCopyMethod.All,
				ItemsTableName = "TestObject3Item",
				ItemPropertyName = "FK",
				ItemParentTablePropertyName = "PK",
				InnerNode = itemEntityNode1
			};
			entityNode.Nodes.Add(collectionNode1);

			var itemEntityNode2 = new EntityCopyTemplateNode { Name = "TestObject3Item" };
			itemEntityNode2.Nodes.Add(new PropertyCopyTemplateNode { Name = "Text", CopyMethod = CopyMethod.Value, Value = "XYZ" });
			var collectionNode2 = new CollectionCopyTemplateNode
			{
				Name = "Collection",
				CopyMethod = copyMethod,
				ItemsTableName = "TestObject3Item",
				ItemPropertyName = "FK",
				ItemParentTablePropertyName = "PK",
				InnerNode = itemEntityNode2,
				Filter = new EntityFilter
				{
					FilterTypeId = "Some filter type",
					FilterData = "Some filter data"
				}
			};
			entityNode.Nodes.Add(collectionNode2);

			return copyTemplate;
		}

		[CollectionRelationProperty("Collection", "FK", "TestObject3Item")]
		class TestObject3 : TestEntity
		{
			public List<TestObject3Item> Collection
			{
				get { return collection ?? (collection = new List<TestObject3Item>()); }
			}
			List<TestObject3Item> collection;
		}

		class TestObject3Item : TestEntity
		{
			public Guid FK { get; set; }
			public string Text { get; set; }
		}

		#endregion

		#region TestCopyTemplateNodeCompare

		public void TestCopyTemplateNodeCompareOnNull()
		{
			var comparer = new CopyManager.TemplateNodeCopyOrderComparer();
			AssertEquals("Null nodes are uncomparable", 0, comparer.Compare(null, null));
		}

		public void TestCopyTemplateNodeCompareOnType()
		{
			var comparer = new CopyManager.TemplateNodeCopyOrderComparer();
			AssertEquals("PropertyCopyTemplateNode comes first", -1, comparer.Compare(new PropertyCopyTemplateNode(), new RelatedEntityCopyTemplateNode()));
			AssertEquals("PropertyCopyTemplateNode comes first", 1, comparer.Compare(new RelatedEntityCopyTemplateNode(), new PropertyCopyTemplateNode()));
			AssertEquals("PropertyCopyTemplateNode comes first", 0, comparer.Compare(new RelatedEntityCopyTemplateNode(), new RelatedEntityCopyTemplateNode()));
		}

		public void TestCopyTemplateNodeCompareOnName()
		{
			var comparer = new CopyManager.TemplateNodeCopyOrderComparer();
			Func<string, PropertyCopyTemplateNode> makeNode = (name) => new PropertyCopyTemplateNode() { Name = name };
			AssertEquals("A node comes before B", -1, comparer.Compare(makeNode("A"), makeNode("B")));
			AssertEquals("A node comes before B", 1, comparer.Compare(makeNode("B"), makeNode("A")));
			AssertEquals("A node comes before B", 0, comparer.Compare(makeNode("A"), makeNode("A")));
		}

		public void TestCollectionCopyTemplateNodeCompareOnFilter()
		{
			var comparer = new CopyManager.TemplateNodeCopyOrderComparer();
			Func<bool, CollectionCopyTemplateNode> makeNode = (filtered) => new CollectionCopyTemplateNode() { Filter = filtered ? new EntityFilter() : null };
			AssertEquals("Collection node with filter goes first", -1, comparer.Compare(makeNode(true), makeNode(false)));
			AssertEquals("Collection node with filter goes first", 1, comparer.Compare(makeNode(false), makeNode(true)));
			AssertEquals("Collection node with filter goes first", 0, comparer.Compare(makeNode(true), makeNode(true)));
		}

		public void TestCollectionCopyTemplateNodeCompareOnOrder()
		{
			var comparer = new CopyManager.TemplateNodeCopyOrderComparer();
			Func<int, CollectionCopyTemplateNode> makeNode = (order) => new CollectionCopyTemplateNode() { Filter = new EntityFilter(), Order = order };
			AssertEquals("Collection node with lowest order goes first", -1, comparer.Compare(makeNode(1), makeNode(2)));
			AssertEquals("Collection node with lowest order goes first", 1, comparer.Compare(makeNode(2), makeNode(1)));
			AssertEquals("Collection node with lowest order goes first", 0, comparer.Compare(makeNode(3), makeNode(3)));
		}

		public void TestCopyTemplateNodeCompareOverall()
		{
			var prop0 = new CollectionCopyTemplateNode() { Filter = null };
			var prop1 = new CollectionCopyTemplateNode() { Filter = new EntityFilter(), Order = 5 };
			var prop2 = new CollectionCopyTemplateNode() { Filter = new EntityFilter() };
			var prop3 = new PropertyCopyTemplateNode() { Name = "B" };
			var prop4 = new PropertyCopyTemplateNode() { Name = "A" };
			var prop5 = new PropertyCopyTemplateNode();
			var nodes = new List<CopyTemplateNode>() { prop0, prop1, prop2, prop3, prop4, prop5 };
			nodes.Sort(new CopyManager.TemplateNodeCopyOrderComparer());
			AssertEquals("Overall sort back to front", prop5, nodes[0]);
			AssertEquals("Overall sort back to front", prop4, nodes[1]);
			AssertEquals("Overall sort back to front", prop3, nodes[2]);
			AssertEquals("Overall sort back to front", prop2, nodes[3]);
			AssertEquals("Overall sort back to front", prop1, nodes[4]);
			AssertEquals("Overall sort back to front", prop0, nodes[5]);
		}

		public void TestCopyTemplateNodeCompareOnPriority()
		{
			var comparer = new CopyManager.TemplateNodeCopyOrderComparer();
			Func<int, PropertyCopyTemplateNode> makeNode = priority => new PropertyCopyTemplateNode { Priority = priority };
			AssertEquals(-1, comparer.Compare(makeNode(-1), makeNode(0)));
			AssertEquals(1, comparer.Compare(makeNode(0), makeNode(-1)));
			AssertEquals(0, comparer.Compare(makeNode(0), makeNode(0)));
		}

		#endregion

		#region TestClasses

		class TestCopyManager : ReflectionCopyManagerBase
		{
			public TestCopyManager(bool isNewEntityNull = false)
			{
				this.isNewEntityNull = isNewEntityNull;
			}

			protected override object GetRelatedEntityFromDb(object sourceEntity, RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode)
			{
				return null;
			}

			protected override object CreateNewEntityFromCore(object parentEntity, object sourceEntity, CopyTemplateNode copyTemplateNode, string propertyName)
			{
				return isNewEntityNull ? null : Activator.CreateInstance(sourceEntity.GetType());
			}

			protected override object GetEntityPKCore(object entity)
			{
				return (entity as IHavePK)?.PK ?? Guid.Empty;
			}

			protected override string GetEntityCodeCore(object entity)
			{
				return ((IHaveCode)entity).Code;
			}

			protected override IEnumerable GetCollectionFromDb(object sourceEntity, CollectionCopyTemplateNode collectionCopyTemplateNode)
			{
				return null;
			}

			protected override IEnumerable FilterCollectionCore(IEnumerable collection, CollectionCopyTemplateNode collectionCopyTemplateNode, IEnumerable<string> path)
			{
				bool flag = true;
				foreach (var item in collection)
				{
					if (flag)
					{
						yield return item;
					}
					flag = !flag;
				}
			}

			protected override void SetCollectionRelationship(object targetEntity, object collectionItem, CollectionCopyTemplateNode collectionCopyTemplateNode)
			{
				base.SetCollectionRelationship(targetEntity, collectionItem, collectionCopyTemplateNode);

				var collection = GetPropertyValue(targetEntity, collectionCopyTemplateNode.Name) as IList;
				if (collection != null)
				{
					collection.Add(collectionItem);
				}
			}

			protected override void SetCollectionItemParentTableProperty(object targetEntity, object collectionItem, CollectionCopyTemplateNode collectionCopyTemplateNode)
			{
			}

			protected override string ProcessMacrosCore(string value, IEnumerable<object> rootEntities)
			{
				if (value.StartsWith("<") && value.EndsWith(">"))
				{
					return "Hello World!";
				}
				return value;
			}

			public void SetPropertyValueExposed(object target, string propertyName, object value)
			{
				SetPropertyValue(target, propertyName, value);
			}

			readonly bool isNewEntityNull;
		}

		interface IHavePK
		{
			Guid PK { get; }
		}

		interface IHaveCode
		{
			string Code { get; }
		}

		public class TestEntity : IHavePK, IHaveCode
		{
			public Guid PK
			{
				get { return pk; }
			}
			readonly Guid pk = Guid.NewGuid();

			public string Code
			{
				get { return PK.ToString(); }
			}
		}

		#endregion
	}
}
