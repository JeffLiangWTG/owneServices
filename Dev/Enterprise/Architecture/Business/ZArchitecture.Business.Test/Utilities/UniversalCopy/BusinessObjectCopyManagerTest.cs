using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWise.UniversalCopy.Interfaces;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Utilities.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.ZArchitecture.Business.UniversalCopy.Testing
{
	sealed class BusinessObjectCopyManagerTest : TestCaseWithDummy
	{
		public void TestDateTimeIsCopiedCorrectlyIfCopyMethodIsMacroThatReturnEmpty()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var copyTemplate = new CopyTemplateTree(shipment.GetType());
			var propertyNode = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)copyTemplate.InnerNode).Nodes.Find(node => node is PropertyCopyTemplateNode && node.Name == nameof(shipment.JS_E_ARV));
			propertyNode.CopyMethod = CopyMethod.Macro;
			propertyNode.Value = string.Empty;

			var copyManager = new BusinessObjectCopyManager();
			var target = copyManager.Copy(shipment, copyTemplate).Object as Forwarding.IForwardingShipment;
			AssertEquals(ZDateTime.Empty, target.JS_E_ARV);
		}

		#region TestGetCollectionComponentPropertyInfo_UniversalCopyAssociateElement

		public void TestGetCollectionComponentPropertyInfo_UniversalCopyAssociateElement()
		{
			var config1 = new CopyTreeConfigurationForTest();
			_ = new CopyTemplateTree(typeof(IRoot), typeof(MyFirstTree), copyTreeConfiguration: config1);

			var config2 = new CopyTreeConfigurationForTest();
			_ = new CopyTemplateTree(typeof(IRoot), typeof(MySecondTree), copyTreeConfiguration: config2);

			AssertEquals(typeof(ICollection<MyIOC>), config1.CollectionType);
			AssertNull(config2.CollectionType);
		}

		class CopyTreeConfigurationForTest : BusinessObjectCopyManager.CopyTreeConfigurationImplementation, ICopyTreeConfiguration
		{
			public Type CollectionType;

			Type ICopyTreeConfiguration.GetCollectionElementTypeFromCollectionType(Type collectionType)
			{
				CollectionType = collectionType;
				return null;
			}
		}

		[WTG.Glow.Data.Annotations.CollectionRelationProperty("OCs", "OC_RT", "OC")]
		interface IRoot
		{
			ICollection<IOC> OCs { get; }
		}

		interface IOC
		{
		}

		[UniversalCopyAssociateElement("OCs", "MyOCs")]
		class MyFirstTree
		{
			public ICollection<MyIOC> MyOCs { get; }
		}

		class MyIOC
		{
		}

		class MySecondTree
		{
			public ICollection<MyIOC> MyOCs { get; }
		}

		#endregion

		public void TestCreateNewEntityFrom()
		{
			var newDummy = new BusinessObjectCopyManagerForTest().CreateNewEntityFromCoreForTesting(null, Dummy, null, null) as BusinessObject;

			AssertNotNull(newDummy);
			AssertNotEquals(Dummy.PK, newDummy.PK);
			AssertSame("New object should be in same factory as original", Dummy.Factory, newDummy.Factory);
			Assert("New object should not be saved", !newDummy.IsInDatabase);
			Assert("New object should be ready for saving", newDummy.IsSavedByFactory);
		}

		public void TestCreateNewEntityFromCustomized()
		{
			var newDummy = new BusinessObjectCopyManagerForTest().CreateNewEntityFromCoreForTesting(null, Factory.New<UniversalCopyDummy>(), null, null) as BusinessObject;
			AssertEquals("Customized", (newDummy as UniversalCopyDummy).Z0_Description);
		}

		public void TestGetEntityPKCore()
		{
			AssertEquals(Dummy.PK, new BusinessObjectCopyManagerForTest().GetEntityPkCoreForTesting(Dummy));
		}

		public void TestGetEntityCodeCore()
		{
			Dummy.Z0_Code = "ABC";
			AssertEquals("ABC", new BusinessObjectCopyManagerForTest().GetEntityCodeCoreForTesting(Dummy));
		}

		public void TestSetEntityRelationship_CustomRelatedEntity_GetsRelatedEntityObjectFromCustomBehaviour()
		{
			// Arrange
			var targetEntity = Factory.New<DummyBusinessObject>();
			var relatedEntity = Factory.New<DummyBusinessObject>();
			var customRelatedEntity = Factory.New<DummyBusinessObject>();
			var relatedEntityCopyTemplateNode = new RelatedEntityCopyTemplateNode()
			{
				Name = "RelatedPropertyName",
				RelatedPropertyName = "Z0_Guid",
				RelatedEntityTableName = relatedEntity.TableName
			};

			var mockUniversalCopyCustomRelatedEntity = new Mock<IUniversalCopyCustomRelatedEntity>();
			mockUniversalCopyCustomRelatedEntity.Setup(entity => entity.RelatedPropertyName).Returns("Z0_Guid");
			mockUniversalCopyCustomRelatedEntity.Setup(entity => entity.GetOriginalRelatedEntityFromRelatedEntityObject(
				It.IsAny<RelatedEntityCopyTemplateNode>(), It.IsAny<object>(), It.IsAny<Func<object, string, object>>()))
				.Returns(customRelatedEntity);

			var mockDifferentUniversalCopyCustomRelatedEntity = new Mock<IUniversalCopyCustomRelatedEntity>();
			mockDifferentUniversalCopyCustomRelatedEntity.Setup(entity => entity.RelatedPropertyName).Returns("Different");
			mockDifferentUniversalCopyCustomRelatedEntity.Setup(entity => entity.GetOriginalRelatedEntityFromRelatedEntityObject(
				It.IsAny<RelatedEntityCopyTemplateNode>(), It.IsAny<object>(), It.IsAny<Func<object, string, object>>()));

			// Act
			using (ObjectFactory.Substitute("UniversalCopyCustomRelatedEntitiesList", new ListObject()
				{
					mockUniversalCopyCustomRelatedEntity.Object,
					mockDifferentUniversalCopyCustomRelatedEntity.Object
				}))
			{
				var copyManager = new BusinessObjectCopyManagerForTest();
				copyManager.SetEntityRelationshipForTesting(targetEntity, relatedEntity, relatedEntityCopyTemplateNode);
			}

			// Assert
			mockUniversalCopyCustomRelatedEntity.Verify(entity => entity.GetOriginalRelatedEntityFromRelatedEntityObject(
				relatedEntityCopyTemplateNode, relatedEntity, It.IsAny<Func<object, string, object>>()), Times.Once);
			mockDifferentUniversalCopyCustomRelatedEntity.Verify(entity => entity.GetOriginalRelatedEntityFromRelatedEntityObject(
				It.IsAny<RelatedEntityCopyTemplateNode>(), It.IsAny<object>(), It.IsAny<Func<object, string, object>>()), Times.Never);

			AssertEquals("Assert target entity related to custom related entity", customRelatedEntity.PK, targetEntity.Z0_Guid);
		}

		public void TestSetPropertyValueCore()
		{
			var copyManager = new BusinessObjectCopyManagerForTest();
			AssertSetPropertyValueCore(copyManager, DummyBizoSchema.Constants.Z0_Description, new ZString("Hello World!"), "Hello World!"); // Convertion to ZType will be done in CopyManager.SetPropertyValue()
			AssertSetPropertyValueCore(copyManager, DummyBizoSchema.Constants.Z0_Number, new ZInt(123), 123);
			AssertSetPropertyValueCore(copyManager, DummyBizoSchema.Constants.Z0_Code, new ZString("ABCDEFGH"), "ABCDE");
		}

		void AssertSetPropertyValueCore(BusinessObjectCopyManagerForTest copyManager, string propertyName, object value, object expectedValue)
		{
			copyManager.SetPropertyValueCoreForTesting(Dummy, propertyName, value);
			AssertEquals(expectedValue, Dummy[propertyName]);
		}

		#region TestSetPropertyValueWithNotificationsHandling

		public void TestSetPropertyValueWithNotificationsHandling()
		{
			var copyManager = new BusinessObjectCopyManagerForTest();
			copyManager.AlwaysCopyViaPropertyForTest = true;

			AssertExceptionThrown<UniversalCopyAbortException>("Cannot set property DummyBusinessObject.Z0_Number of type ZInt with value 'abc'.", () => copyManager.SetPropertyValueForTesting(Dummy, DummyBizoSchema.Constants.Z0_Number, "abc"));

			var testNotification = new TestNotifications();
			copyManager.Notifications = testNotification;

			AssertNoExceptionThrown(() => copyManager.SetPropertyValueForTesting(Dummy, DummyBizoSchema.Constants.Z0_Number, "abc"));
			AssertNotNull(testNotification.LastNotification);
			AssertEquals("Cannot set property DummyBusinessObject.Z0_Number of type ZInt with value 'abc'.", testNotification.LastNotification.Message);
		}

		class TestNotifications : INotifications
		{
			public void Add(INotification notification)
			{
				LastNotification = notification;
			}

			public INotification LastNotification { get; set; }
		}

		#endregion

		#region TestSetDataPropertyValue

		public void TestSetDataPropertyValue()
		{
			var copyManager = new BusinessObjectCopyManagerForTest();
			var dummy = Factory.New<DummyWithProperty>();

			copyManager.SetDataPropertyValueForTesting(dummy, DummyBizoSchema.Constants.Z0_Number, 5);
			AssertEquals(5, dummy.Z0_Number);
			Assert("Property setter on bizo should not have been accessed", !dummy.propertySetterAccessed);

			copyManager.SetPropertyValueForTesting(dummy, DummyBizoSchema.Constants.Z0_Number, 7);
			AssertEquals(7, dummy.Z0_Number);
			Assert("Property setter on bizo should have been accessed", dummy.propertySetterAccessed);
		}

		class DummyWithProperty : DummyBusinessObject
		{
			public DummyWithProperty(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public override ZInt Z0_Number
			{
				get { return base.Z0_Number; }
				set
				{
					base.Z0_Number = value;
					propertySetterAccessed = true;
				}
			}

			internal bool propertySetterAccessed;
		}

		#endregion

		public void TestProcessMacrosCore()
		{
			var copyManager = new BusinessObjectCopyManagerForTest();
			Dummy.Z0_Code = "XYZ";
			AssertEquals("XYZ - EDI", copyManager.ProcessMacrosCoreForTesting("<Z0_Code> - <CompanyCode>", new object[] { Dummy }));
		}

		public void TestGetRelatedEntityFromDb()
		{
			var relatedDummy = new BusinessObjectFactory().New<DummyBusinessObject>();
			relatedDummy.Z0_Code = "XYZ";
			relatedDummy.Factory.Save();

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Guid = relatedDummy.PK;

			var copyManager = new BusinessObjectCopyManagerForTest();

			var loadedRelatedElement = copyManager.GetRelatedEntityFromDbForTesting(dummy,
				new RelatedEntityCopyTemplateNode
				{
					Name = "NoName",
					RelatedPropertyName = "Z0_Guid",
					RelatedEntityTableName = relatedDummy.TableName
				});

			AssertNotNull(loadedRelatedElement);

			ZGuid loadedPK =
				loadedRelatedElement is BusinessObject
					? ((BusinessObject)loadedRelatedElement).PK
					: loadedRelatedElement is DataRow
						? new ZGuid(((DataRow)loadedRelatedElement)[DummyBizoSchema.Constants.PK])
						: ZGuid.Invalid;

			AssertEquals(relatedDummy.PK, loadedPK);
		}

		public void TestGetRelatedEntityFromDb_CustomRelatedEntity_GetsRelatedEntityFKFromCustomBehaviour()
		{
			// Arrange
			var relatedDummy = Factory.New<DummyBusinessObject>();
			relatedDummy.Z0_Code = "XYZ";

			var anotherDummy = Factory.New<DummyBusinessObject>();
			anotherDummy.Z0_Code = "ABC";

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Guid = relatedDummy.PK;

			anotherDummy.Factory.Save();

			var mockUniversalCopyCustomRelatedEntity = new Mock<IUniversalCopyCustomRelatedEntity>();
			mockUniversalCopyCustomRelatedEntity.Setup(entity => entity.RelatedPropertyName).Returns("Z0_Guid");
			mockUniversalCopyCustomRelatedEntity.Setup(entity => entity.GetRelatedEntityObjectFK(
					It.IsAny<BusinessObjectFactory>(), It.IsAny<RelatedEntityCopyTemplateNode>(), It.IsAny<ZGuid>()))
				.Returns(anotherDummy.PK);

			object loadedRelatedElement;

			// Act
			using (ObjectFactory.Substitute("UniversalCopyCustomRelatedEntitiesList", new ListObject() { mockUniversalCopyCustomRelatedEntity.Object }))
			{
				var copyManager = new BusinessObjectCopyManagerForTest();
				loadedRelatedElement = copyManager.GetRelatedEntityFromDbForTesting(dummy,
					new RelatedEntityCopyTemplateNode
					{
						Name = "NoName",
						RelatedPropertyName = "Z0_Guid",
						RelatedEntityTableName = relatedDummy.TableName
					});
			}

			// Assert
			AssertNotNull(loadedRelatedElement);

			var loadedPK =
				loadedRelatedElement is BusinessObject
					? ((BusinessObject)loadedRelatedElement).PK
					: loadedRelatedElement is DataRow
						? new ZGuid(((DataRow)loadedRelatedElement)[DummyBizoSchema.Constants.PK])
						: ZGuid.Invalid;

			AssertEquals(anotherDummy.PK, loadedPK);
		}

		#region TestPreCopyEntity TestPostCopyEntity

		public void TestPostCopyEntity()
		{
			var dummy = Factory.New<DummyWithCollection>();
			Assert(!dummy.FinishCopyCalled);
			AssertEquals(0, dummy.AllDummies1.Count);
			AssertEquals(0, dummy.AllDummies2.Count);

			new BusinessObjectCopyManagerForTest().PostCopyEntityForTesting(dummy, dummy, new EntityCopyTemplateNode());
			Assert(dummy.FinishCopyCalled);
			Assert("Should load legacy collection", dummy.AllDummies1.Count > 0);
			AssertEquals("Should not load non-registered child collection", 0, dummy.AllDummies2.Count);
		}

		public void TestPreCopyEntity()
		{
			var dummy = Factory.New<DummyWithCollection>();
			Assert(!dummy.StartCopyCalled);

			new BusinessObjectCopyManagerForTest().PreCopyEntityForTesting(dummy, dummy, new EntityCopyTemplateNode());
			Assert(dummy.StartCopyCalled);
		}

		[UniversalCopyWithExtendedEntities(StartCopyMethod = "StartCopy", FinishCopyMethod = "FinishCopy")]
		class DummyWithCollection : DummyBusinessObject
		{
			public DummyWithCollection(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public DummyBusinessObjectCollection AllDummies1
			{
				get
				{
					if (allDummies1 == null)
					{
						allDummies1 = new DummyBusinessObjectCollection(Factory);
						RegisterEditableChildObject(allDummies1);
					}
					return allDummies1;
				}
			}
			DummyBusinessObjectCollection allDummies1;

			public DummyBusinessObjectCollection AllDummies2
			{
				get
				{
					if (allDummies2 == null)
					{
						allDummies2 = new DummyBusinessObjectCollection(Factory);
					}
					return allDummies2;
				}
			}
			DummyBusinessObjectCollection allDummies2;

			protected void FinishCopy()
			{
				FinishCopyCalled = true;
			}

			protected void StartCopy()
			{
				StartCopyCalled = true;
			}

			public bool FinishCopyCalled { get; private set; }

			public bool StartCopyCalled { get; private set; }
		}

		#endregion

		#region TestPrepareAndFinishCopy

		public void TestPrepareAndFinishCopy()
		{
			var copyManager = new BusinessObjectCopyManagerForTest();

			AssertNull(Dummy.Factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>());
			copyManager.PrepareForCopyForTesting(Dummy, null);
			AssertNotNull(Dummy.Factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>());

			bool onCopyFinishedActionFired = false;
			Dummy.Factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>().AddOnCopyFinishedAction(() => onCopyFinishedActionFired = true);
			Assert(!onCopyFinishedActionFired);

			copyManager.FinishCopyForTesting(Dummy, null, null);
			AssertNull(Dummy.Factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>());
			Assert(onCopyFinishedActionFired);
		}

		public void TestReloadAllCollectionsOnFinishCopy()
		{
			var otherFactory = new BusinessObjectFactory();
			var otherDummy = (DummyBusinessObject)otherFactory.ImportFromAnotherFactory(Dummy);

			otherDummy.Collection.Load();
			otherDummy.Collection.RemoveAndDeleteAll();
			otherDummy.RegisterEditableChildObject(otherDummy.Collection);

			Assert(otherDummy.Collection.IsLoaded);
			AssertEquals(0, otherDummy.Collection.Count);

			otherFactory.ImportFromAnotherFactory(Dummy.Collection.AddNew());
			otherFactory.ImportFromAnotherFactory(Dummy.Collection.AddNew());

			AssertEquals(0, otherDummy.Collection.Count);

			new BusinessObjectCopyManagerForTest().FinishCopyForTesting(Dummy, otherDummy, null);

			AssertEquals("Collection should be reloaded with elements in memory", 2, otherDummy.Collection.Count);
		}

		[NUnit.Framework.TestDate(2019, 04, 02, 0, 0, 0)]
		public void TestLogOnFinishCopy()
		{
			var now = ZDateTime.Now;

			var copyTemplateTree = new CopyTemplateTree();
			copyTemplateTree.ConfigurationName = "Template For Test";

			var copy = Factory.New<UniversalCopyDummy>();
			Assert("Shoud not have the UCP event.", !copy.GetLogs().HasLogWith(c => c.SL_SE_NKEvent == AutoEvents.UniversalCopyCode));

			new BusinessObjectCopyManagerForTest().FinishCopyForTesting(Dummy, copy, copyTemplateTree);

			var log = copy.GetLogs().Find(c => c.SL_SE_NKEvent == AutoEvents.UniversalCopyCode).First();
			AssertEquals("SL_EventTime", now, log.SL_EventTime);
			AssertEquals("SL_Reference", $"{Dummy.HumanReadableName}|Template For Test", log.SL_Reference);

			copy = Factory.New<UniversalCopyDummy>();
			new BusinessObjectCopyManagerForTest().FinishCopyForTesting(Dummy, copy, null);

			log = copy.GetLogs().Find(c => c.SL_SE_NKEvent == AutoEvents.UniversalCopyCode).First();
			AssertEquals("SL_EventTime", now, log.SL_EventTime);
			AssertEquals("SL_Reference", $"{Dummy.HumanReadableName}|", log.SL_Reference);
		}

		public void TestCopyBMNCNShapeInfoOnFinishCopy()
		{
			// Arrange
			BusinessObjectCopyManagerForTest copyManager;
			var copy = Factory.New<UniversalCopyDummy>();

			var mockBMNCNShapeCopier = new Mock<IUniversalCopyCustomFinishCopyAction>();
			mockBMNCNShapeCopier.Setup(m => m.FinishCopyAction(It.IsAny<Dictionary<object, object>>())).Callback<Dictionary<object, object>>(entities => entities.Add(Factory.New<UniversalCopyDummy>(), Factory.New<UniversalCopyDummy>()));

			// Act
			using (ObjectFactory.Substitute("UniversalCopyCustomFinishCopyActionsList", new ListObject() { mockBMNCNShapeCopier.Object }))
			{
				copyManager = new BusinessObjectCopyManagerForTest();
				copyManager.CopiedEntities = new Dictionary<object, object>() { { Dummy, copy } };
				copyManager.FinishCopyForTesting(Dummy, copy, null);
			}

			// Assert
			AssertEquals("Assert extra copied elements added to copiedEntities", 2, copyManager.CopiedEntities.Count);
			mockBMNCNShapeCopier.Verify(m => m.FinishCopyAction(copyManager.CopiedEntities), Times.Once);
		}

		public void TestCopyRelatedProcessHeaderLinksOnFinishCopy()
		{
			// Arrange
			BusinessObjectCopyManagerForTest copyManager;
			var copy = Factory.New<UniversalCopyDummy>();

			var mockProcessHeaderLinkCopier = new Mock<IUniversalCopyCustomFinishCopyAction>();
			mockProcessHeaderLinkCopier.Setup(m => m.FinishCopyAction(It.IsAny<Dictionary<object, object>>())).Callback<Dictionary<object, object>>(entities => entities.Add(Factory.New<UniversalCopyDummy>(), Factory.New<UniversalCopyDummy>()));

			// Act
			using (ObjectFactory.Substitute("UniversalCopyCustomFinishCopyActionsList", new ListObject() { mockProcessHeaderLinkCopier.Object }))
			{
				copyManager = new BusinessObjectCopyManagerForTest();
				copyManager.CopiedEntities = new Dictionary<object, object>() { { Dummy, copy } };
				copyManager.FinishCopyForTesting(Dummy, copy, null);
			}

			// Assert
			AssertEquals("Assert extra copied elements added to copiedEntities", 2, copyManager.CopiedEntities.Count);
			mockProcessHeaderLinkCopier.Verify(m => m.FinishCopyAction(copyManager.CopiedEntities), Times.Once);
		}

		#endregion

		#region Test Get Collection

		public void TestGetCollectionCore()
		{
			var dummy = Factory.New<DummyWithDependentsBusinessObject>();
			dummy.Dependents.AddNew();
			dummy.Dependents.AddNew();

			var copyManager = new BusinessObjectCopyManagerForTest();

			var collectionNode = GetCollectionCopyTemplateNode("SomeMissingCollectionProperty", DummyDependentBizoSchema.Constants.ZD1_Z0, DummyDependentBizoSchema.Constants.TableName);
			var collection = copyManager.GetCollectionCoreForTesting(dummy, collectionNode);
			AssertNull(collection);

			collectionNode.Name = "Dependents";
			collection = copyManager.GetCollectionCoreForTesting(dummy, collectionNode);
			AssertSame(dummy.Dependents, collection);
		}

		public void TestGetCollectionCoreForLogsOrNotes()
		{
			var dummy = Factory.New<ZArchitecture.Business.Testing.DummyBizOWithRelatedNotes>();
			dummy.Notes.AddNew();
			dummy.Notes.AddNew();

			var copyManager = new BusinessObjectCopyManagerForTest();

			var collectionNode = GetCollectionCopyTemplateNode("Notes", DummyDependentBizoSchema.Constants.ZD1_Z0, DummyDependentBizoSchema.Constants.TableName);
			var collection = copyManager.GetCollectionCoreForTesting(dummy, collectionNode);
			AssertSame(dummy.Notes.ElementsInternal, collection);
		}

		public void TestGetCollectionFromDb()
		{
			var dummy = Factory.New<DummyWithDependentsBusinessObject>();
			dummy.Dependents.AddNew();
			dummy.Dependents.AddNew();

			var copyManager = new BusinessObjectCopyManagerForTest(Factory);

			var collectionNode = GetCollectionCopyTemplateNode("SomeMissingCollectionProperty", DummyDependentBizoSchema.Constants.ZD1_Z0, DummyDependentBizoSchema.Constants.TableName);
			var collection = copyManager.GetCollectionFromDbForTesting(dummy, collectionNode);
			AssertNotNull(collection);
			AssertEquals(2, collection.Cast<object>().Count());

			var collectionFromRow = copyManager.GetCollectionFromDbForTesting(((INeedRow)dummy).Row, collectionNode);
			AssertNotNull(collectionFromRow);
			AssertEquals(2, collectionFromRow.Cast<object>().Count());
		}

		public void TestGetCollectionFromDbWithParentTableCode()
		{
			var parrentDummy = Factory.New<DummyBusinessObject>();

			var note1 = Factory.New<StmNote>();
			note1.ST_ParentID = parrentDummy.PK;
			note1.ST_NoteType = DummyBizoSchema.Constants.Prefix;
			note1.ST_Table = DummyBizoSchema.Constants.TableName;
			note1.ST_Description = "1";

			var note2 = Factory.New<StmNote>();
			note2.ST_ParentID = parrentDummy.PK;
			note2.ST_NoteType = DummyBizoSchema.Constants.Prefix;
			note2.ST_Table = "DifferentTable";
			note2.ST_Description = "2";

			var note3 = Factory.New<StmNote>();
			note3.ST_ParentID = parrentDummy.PK;
			note3.ST_NoteType = "XXX";
			note3.ST_Table = "DifferentTable";
			note3.ST_Description = "3";

			var copyManager = new BusinessObjectCopyManagerForTest(Factory);
			var collectionNode = GetCollectionCopyTemplateNode("SomeMissingCollectionProperty", StmNoteSchema.Constants.ST_ParentID, StmNoteSchema.Constants.TableName);

			collectionNode.ItemParentTablePropertyName = StmNoteSchema.Constants.ST_Table;
			var collection = copyManager.GetCollectionFromDbForTesting(parrentDummy, collectionNode);

			AssertNotNull(collection);
			AssertEquals(1, collection.Cast<object>().Count());
			var noteCollection = collection.OfType<StmNote>().OrderBy(note => note.ST_Description).ToArray();
			AssertEquals(1, noteCollection.Length);
			AssertEquals(note1.PK, noteCollection[0].PK);
		}

		CollectionCopyTemplateNode GetCollectionCopyTemplateNode(string collectionName, string itemPropertyName, string itemTableName)
		{
			var collectionNode = new CollectionCopyTemplateNode();
			collectionNode.Name = collectionName;
			collectionNode.ItemPropertyName = itemPropertyName;
			collectionNode.ItemsTableName = itemTableName;
			return collectionNode;
		}

		#region TestGetSpecialCollection

		public void TestGetSpecialCollection_NullItemsTableName()
		{
			var dummy = Factory.New<DummyWithServices>();
			var copyManager = new BusinessObjectCopyManagerForTest(Factory);
			var collectionNode = GetCollectionCopyTemplateNode("AdditionalServices", DummyBizoSchema.Constants.Z0_Guid, null);

			AssertNoExceptionThrown(() => copyManager.GetCollectionCoreForTesting(dummy, collectionNode));
		}

		public void TestGetSpecialCollection()
		{
			var dummy = Factory.New<DummyWithServices>();
			var copyManager = new BusinessObjectCopyManagerForTest(Factory);
			var collectionNode = GetCollectionCopyTemplateNode("AdditionalServices", DummyBizoSchema.Constants.Z0_Guid, JobServiceSchema.Constants.TableName);

			var collection = copyManager.GetCollectionCoreForTesting(dummy, collectionNode);
			AssertNotNull(collection);
			AssertEquals(typeof(DummyJobServices), collection.GetType());
		}

		class DummyWithServices : DummyBusinessObject
		{
			public DummyWithServices(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public BusinessObjectCollection<DummyJobService> Services
			{
				get { return new DummyJobServices(Factory); }
			}
		}

		class DummyJobService : DummyBusinessObject, IJobService
		{
			public DummyJobService(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		class DummyJobServices : BusinessObjectCollection<DummyJobService>
		{
			public DummyJobServices(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion

		#endregion

		#region TestPrepareTargetCollection

		public void TestPrepareTargetCollection()
		{
			var copyManager = new BusinessObjectCopyManagerForTest(Factory);
			var collectionNode = GetCollectionCopyTemplateNode("ClearableCollection", DummyBizoSchema.Constants.Z0_Guid, DummyBizoSchema.Constants.TableName);

			var dummy = Factory.New<DummyWithClearableCollection>();
			dummy.ClearableCollection.AddNew().Z0_Code = "1";
			Factory.Save();

			dummy.ClearableCollection.AddNew().Z0_Code = "2";

			var itemCreateByCopyManager = dummy.ClearableCollection.AddNew();
			itemCreateByCopyManager.Z0_Code = "3";
			copyManager.CreateByMeObjects.Add(itemCreateByCopyManager);

			AssertEquals("Precondition", 3, dummy.ClearableCollection.Count);

			copyManager.PrepareTargetCollectionForTesting(dummy, collectionNode);
			AssertEquals("All elements should remain intact", 3, dummy.ClearableCollection.Count);

			dummy.UseClearableAddresses = true;
			AssertEquals("All elements still should remain intact", 3, dummy.ClearableCollection.Count);

			copyManager.PrepareTargetCollectionForTesting(dummy, collectionNode);
			AssertEquals("New elements not created by copy manager should be deleted", 2, dummy.ClearableCollection.Count);
			AssertEquals("In-db element should remain", "1", dummy.ClearableCollection[0].Z0_Code);
			AssertEquals("Created by copy manager element should remain", "3", dummy.ClearableCollection[1].Z0_Code);
		}

		class DummyWithClearableCollection : DummyBusinessObject
		{
			public DummyWithClearableCollection(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public bool UseClearableAddresses
			{
				get { return useClearableAddresses; }
				set
				{
					if (value != useClearableAddresses && clearableCollection != null)
					{
						var oldCollection = clearableCollection;
						clearableCollection = null;
						useClearableAddresses = value;

						ClearableCollection.AddRange(oldCollection);
					}
					else
					{
						useClearableAddresses = value;
					}
				}
			}
			bool useClearableAddresses;

			public DummyBusinessObjectCollection ClearableCollection
			{
				get
				{
					if (clearableCollection == null)
					{
						clearableCollection = UseClearableAddresses
							? new DummyBusinessObjectClearableCollection(Factory)
							: new DummyBusinessObjectCollection(Factory);
					}
					return clearableCollection;
				}
			}
			DummyBusinessObjectCollection clearableCollection;
		}

		[UniversalCopyClearCollectionOnCopy]
		class DummyBusinessObjectClearableCollection : DummyBusinessObjectCollection
		{
			public DummyBusinessObjectClearableCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion

		#region TestFinishTargetCollection

		public void TestFinishTargetCollection()
		{
			var copyManager = new BusinessObjectCopyManagerForTest(Factory);
			var collectionNode = GetCollectionCopyTemplateNode("ClearableCollection", DummyBizoSchema.Constants.Z0_Guid, DummyBizoSchema.Constants.TableName);

			var dummy = Factory.New<DummyWithClearableCollection2>();

			AssertEquals("Precondition", 1, dummy.ClearableCollection.Count);

			var initialElement = dummy.ClearableCollection[0];
			copyManager.FinishTargetCollectionForTesting(dummy, collectionNode);

			Assert("Initial element should be deleted", initialElement.IsDeleted);
			AssertEquals("Collection should contain at least 1 element", 1, dummy.ClearableCollection.Count);
			AssertNotEquals(initialElement.PK, dummy.ClearableCollection[0].PK);

			initialElement = dummy.ClearableCollection[0];
			initialElement.Z0_Code = "1";
			var copiedElement = dummy.ClearableCollection.AddNew();
			copiedElement.Z0_Code = "2";
			copyManager.CreateByMeObjects.Add(copiedElement);
			copyManager.FinishTargetCollectionForTesting(dummy, collectionNode);

			Assert("Initial element should be deleted", initialElement.IsDeleted);
			Assert("Copied element should remain", !copiedElement.IsDeleted);
			AssertEquals("1 element should remain in collection", 1, dummy.ClearableCollection.Count);
			AssertSame(copiedElement, dummy.ClearableCollection[0]);
		}

		class DummyWithClearableCollection2 : DummyBusinessObject
		{
			public DummyWithClearableCollection2(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public DummyBusinessObjectClearableCollection2 ClearableCollection
			{
				get
				{
					if (clearableCollection == null)
					{
						clearableCollection = new DummyBusinessObjectClearableCollection2(Factory);
						clearableCollection.AddNew();
					}
					return clearableCollection;
				}
			}
			DummyBusinessObjectClearableCollection2 clearableCollection;
		}

		[UniversalCopyClearCollectionOnCopy(ClearAfterAllElementsWereCopied = true)]
		class DummyBusinessObjectClearableCollection2 : DummyBusinessObjectCollection
		{
			public DummyBusinessObjectClearableCollection2(BusinessObjectFactory factory) : base(factory) { }

			public override void Remove(BusinessObject elementToRemove)
			{
				base.Remove(elementToRemove);

				if (Count == 0)
				{
					AddNew();
				}
			}
		}

		#endregion

		#region Test Filter Collection

		public void TestFilterCollectionCore()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.AddNew().Z0_Code = "A01";
			collection.AddNew().Z0_Code = "B02";
			collection.AddNew().Z0_Code = "A03";
			collection.AddNew().Z0_Code = "B04";

			var copyManager = new BusinessObjectCopyManagerForTest();
			copyManager.GetFilterBusinessObjectMethod = (node, path) => new DummyFilterBizo(Factory);

			var filteredCollection = copyManager.FilterCollectionCoreForTesting(collection, null, null);
			AssertNotNull(filteredCollection);
			AssertEquals(2, filteredCollection.Cast<object>().Count());
			foreach (DummyBusinessObject element in filteredCollection)
			{
				Assert(element.Z0_Code.StartsWith("A"));
			}
		}

		public void TestFilterCollectionCoreWithMandatoryExpressionFilter()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.AddNew().Z0_Code = "A01";
			collection.AddNew().Z0_Code = "B02";
			collection.AddNew().Z0_Code = "A03";
			collection.AddNew().Z0_Code = "B04";

			var collectionCopyTemplateNode = new CollectionCopyTemplateNode
			{
				Filter = new EntityFilter
				{
					FilterTypeId = EntityFilterTypeIds.MandatoryExpressionFilter,
					FilterData = DummyBizoSchema.Constants.Z0_Code + " like 'B%'"
				}
			};

			var copyManager = new BusinessObjectCopyManagerForTest();
			copyManager.GetFilterBusinessObjectMethod = (node, path) => new DummyFilterBizo(Factory); // Should be ignored due to mandatory expression filter

			var filteredCollection = copyManager.FilterCollectionCoreForTesting(collection, collectionCopyTemplateNode, null);
			AssertNotNull(filteredCollection);
			AssertEquals(2, filteredCollection.Cast<object>().Count());
			foreach (DummyBusinessObject element in filteredCollection)
			{
				Assert(element.Z0_Code.StartsWith("B"));
			}
		}

		class DummyFilterBizo : FilterBusinessObject
		{
			public DummyFilterBizo(BusinessObjectFactory factory) : base(factory, new DataTable().NewRow()) { }

			public override ZQuery Filter
			{
				get { return new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "A"); }
			}

			protected override void SetPKAndDefaults()
			{
				// Do nothing
			}
		}

		#endregion

		#region TestSetCollecitonRelationship

		public void TestSetCollectionRelationship()
		{
			var dummy = Factory.New<DummyWithChildCollection>();

			AssertSetCollectionRelationship(dummy, "ChildCollection", dummy.Child.PK, "ABC");
			AssertSetCollectionRelationship(dummy, "InternalCollection", dummy.PK, "ABC");
			AssertSetCollectionRelationship(dummy, "SimpleCollection", dummy.PK, "Z0");
		}

		void AssertSetCollectionRelationship(object targetEntity, string collectionName, ZGuid expectedFK, ZString expectedParentTableCode)
		{
			var item = Factory.New<DummyDependantBusinessObject>();

			var childCollectionNode = GetCollectionCopyTemplateNode(collectionName, DummyDependentBizoSchema.Constants.ZD1_Z0, DummyDependentBizoSchema.Constants.TableName);
			childCollectionNode.ItemParentTablePropertyName = DummyDependentBizoSchema.Constants.ZD1_NumberUnitCode;

			new BusinessObjectCopyManagerForTest().SetCollectionRelationshipForTesting(targetEntity, item, childCollectionNode);

			AssertEquals(expectedFK, item.ZD1_Z0);
			AssertEquals(expectedParentTableCode, item.ZD1_NumberUnitCode);
		}

		class DummyWithChildCollection : DummyBusinessObject
		{
			public DummyWithChildCollection(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public object ChildCollection
			{
				get { return Child.InternalCollection; }
			}

			public DummyWithChildCollection Child
			{
				get { return child ?? (child = Factory.New<DummyWithChildCollection>()); }
			}
			DummyWithChildCollection child;

			public DependentDummyCollection InternalCollection
			{
				get { return internalCollection ?? (internalCollection = new DependentDummyCollection(this)); }
			}
			DependentDummyCollection internalCollection;

			public SimpleDependentDummyCollection SimpleCollection
			{
				get { return simpleCollection ?? (simpleCollection = new SimpleDependentDummyCollection(Factory)); }
			}
			SimpleDependentDummyCollection simpleCollection;
		}

		class DependentDummyCollection : DependentBusinessObjectCollection<DummyDependantBusinessObject, DummyWithChildCollection>
		{
			public DependentDummyCollection(DummyWithChildCollection master) : base(master) { }

			protected override SchemaGuidColumn FKSchemaColumnInDependent
			{
				get { return DummyDependentBizoSchema.ZD1_Z0; }
			}

			protected override void SetCollectionRelationships(BusinessObject child)
			{
				base.SetCollectionRelationships(child);

				((DummyDependantBusinessObject)child).ZD1_NumberUnitCode = "ABC";
				((DummyDependantBusinessObject)child).ZD1_Z0 = Master.PK;
			}
		}

		class SimpleDependentDummyCollection : BusinessObjectCollection<DummyDependantBusinessObject>
		{
			public SimpleDependentDummyCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion

		#region Test DocData

		public void TestDocData_AddExtendedPropertiesToEntityNode()
		{
			AssertDocData_AddExtendedPropertiesToEntityNode("JobConsol", true);
			AssertDocData_AddExtendedPropertiesToEntityNode("JobShipment", true);
			AssertDocData_AddExtendedPropertiesToEntityNode("JobDeclaration", true);
			AssertDocData_AddExtendedPropertiesToEntityNode("Whatever", false);
			AssertDocData_AddExtendedPropertiesToEntityNode("jobconsol", false);
		}

		void AssertDocData_AddExtendedPropertiesToEntityNode(string entityName, bool expectDocData)
		{
			var entityNode = new EntityCopyTemplateNode { Name = entityName };
			BusinessObjectCopyManager.AddExtendedPropertiesToEntityNode(entityNode, null, null, null, null);
			if (expectDocData)
			{
				AssertEquals(1, entityNode.Nodes.Count);
				AssertEquals(BusinessObjectCopyManager.DocDataEntityName, entityNode.Nodes[0].Name);
			}
			else
			{
				AssertEquals(0, entityNode.Nodes.Count);
			}
		}

		public void TestGetDocDataRelatedEntity()
		{
			var shipment = Factory.New<ICommonShipment>();

			var docData = Factory.New<StmNote>();
			docData.ST_ParentID = shipment.PK;
			docData.ST_Table = "JobShipment";
			docData.ST_NoteType = "DOC";
			docData.ST_Description = ZString.Empty;
			docData.ST_NoteData = new ZBlob(System.Text.ASCIIEncoding.ASCII.GetBytes("<NewDataSet/>"));

			Factory.Save();

			var reloadedShipment = new BusinessObjectFactory().Load<ICommonShipment>(shipment.PK);

			var copyManager = new BusinessObjectCopyManager();
			var loadedDocData = copyManager.GetDocDataRelatedEntity(reloadedShipment);

			AssertNotNull(loadedDocData);
			AssertEquals("DocumentNote", loadedDocData.GetType().Name);
			AssertEquals(docData.PK, ((BusinessObject)loadedDocData).PK);
		}

		public void TestDocData_Copy()
		{
			var shipment = Factory.New<ICommonShipment>();

			var docData = Factory.New<StmNote>();
			docData.ST_ParentID = shipment.PK;
			docData.ST_NoteType = "DOC";
			docData.ST_Description = ZString.Empty;
			docData.ST_NoteDataAsText = "abc";

			var otherNote = Factory.New<StmNote>();
			otherNote.ST_ParentID = shipment.PK;
			otherNote.ST_NoteType = "XYZ";

			var copyTemplateTree = new CopyTemplateTree();
			var entityNode = new EntityCopyTemplateNode { Name = "JobShipment" };
			copyTemplateTree.InnerNode = entityNode;
			BusinessObjectCopyManager.AddExtendedPropertiesToEntityNode(entityNode, null, null, null, null);
			((RelatedEntityCopyTemplateNode)entityNode.Nodes[0]).CopyMethod = RelatedEntityCopyMethod.Copy;

			var copyManager = new BusinessObjectCopyManager();

			AssertEquals(docData.PK, ((BusinessObject)copyManager.GetDocDataRelatedEntity(shipment)).PK);

			var copiedShipment = copyManager.Copy(shipment, copyTemplateTree).Object as ICommonShipment;
			AssertNotNull("Should create shipment's copy", copiedShipment);

			var copiedDocData = copyManager.GetDocDataRelatedEntity(copiedShipment) as StmNote;
			AssertNotNull(copiedDocData);
			AssertEquals("abc", copiedDocData.ST_NoteDataAsText);
			AssertNotEquals(docData.PK, copiedDocData.PK);
			AssertSame("MainBusinessObject should be initialized with copied shipment", copiedShipment, ((IDocumentNote)copiedDocData).MainBusinessObject);
		}

		#endregion

		#region Test JobHeader

		public void TestJobHeader_AddExtendedPropertiesToEntityNode_ShouldNotHaveJobHeader()
		{
			var entityNode = new EntityCopyTemplateNode { Name = "Dummy" };
			var preProcessedTypes = new Dictionary<Type, EntityCopyTemplateNode>();

			BusinessObjectCopyManager.AddExtendedPropertiesToEntityNode(entityNode, null, typeof(DummyBusinessObject), preProcessedTypes, null);
			AssertEquals(0, entityNode.Nodes.Count);

			BusinessObjectCopyManager.AddExtendedPropertiesToEntityNode(entityNode, null, typeof(DummyJobHeaderParent), preProcessedTypes, null);
			AssertEquals(0, entityNode.Nodes.Count);
		}

		public void TestNodeIsIgnoringParentProperty()
		{
			var preProcessedTypes = new Dictionary<Type, EntityCopyTemplateNode>();
			BusinessObjectToCopyTemplateReflectionHelper.GetEntityCopyTemplateNode(typeof(IJobHeader), BusinessObjectCopyManager.JobHeaderGlowInterfaceType, preProcessedTypes, null);

			var entityCopyTemplateNode = (TemplateCopyTemplateNode)BusinessObjectToCopyTemplateReflectionHelper.GetEntityCopyTemplateNode(typeof(IJobHeader), BusinessObjectCopyManager.JobHeaderGlowInterfaceType, preProcessedTypes, null,
					JobHeaderSchema.Constants.JH_ParentID, JobHeaderSchema.Constants.JH_ParentTableCode, JobHeaderSchema.Constants.JH_IsValid);

			var nodes = ((EntityCopyTemplateNode)entityCopyTemplateNode.TemplateNode).Nodes.Where(node => node.Name.Equals(JobHeaderSchema.Constants.JH_ParentID));
			AssertEquals(0, nodes.Count());
		}

		class DummyJobHeaderParent : DummyBusinessObject, IJobHeaderParentCore
		{
			public DummyJobHeaderParent(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public string JobNumber { get; set; }
		}

		public void TestDeveloperExceptionAfterIgnoringJobHeader()
		{
			var dummy = Factory.New<DummyJobHeaderParent>();
			var copyManager = new BusinessObjectCopyManagerForTest();

			var collectionNode = GetCollectionCopyTemplateNode("JobHeaderCollectionProperty", JobHeaderSchema.Constants.JH_GB, JobHeaderSchema.Constants.TableName);
			var collection = copyManager.GetCollectionFromDbForTesting(dummy, collectionNode);

			AssertNull(collection);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		#endregion

		#region TestReflectExtendedPropertiesIfNeeded_Collection

		public void TestReflectExtendedPropertiesIfNeeded_Collection()
		{
			var entityNode = new EntityCopyTemplateNode();

			BusinessObjectCopyManager.ReflectExtendedPropertiesIfNeeded(entityNode, null, typeof(DummyBusinessObject), null, null);
			AssertEquals("Nothing reflected as there is no attributes specified", 0, entityNode.Nodes.Count);

			BusinessObjectCopyManager.ReflectExtendedPropertiesIfNeeded(entityNode, null, typeof(DummyWithExtraCollections1), null, null);
			AssertEquals("Nothing reflected as there is no attributes specified", 0, entityNode.Nodes.Count);

			BusinessObjectCopyManager.ReflectExtendedPropertiesIfNeeded(entityNode, null, typeof(DummyWithExtraCollections2), null, null);
			AssertEquals("2 collection nodes should be reflected", 2, entityNode.Nodes.Count);

			var nodes = entityNode.Nodes.OrderBy(node => node.Name).ToArray();

			AssertEquals(typeof(CollectionCopyTemplateNode), nodes[0].GetType());
			AssertEquals("Collection1", nodes[0].Name);
			AssertEquals("Table1", ((CollectionCopyTemplateNode)nodes[0]).ItemsTableName);
			AssertEquals("FK1", ((CollectionCopyTemplateNode)nodes[0]).ItemPropertyName);
			AssertEquals("", ((CollectionCopyTemplateNode)nodes[0]).ItemParentTablePropertyName);

			AssertEquals(typeof(CollectionCopyTemplateNode), nodes[1].GetType());
			AssertEquals("Collection2", nodes[1].Name);
			AssertEquals("Table2", ((CollectionCopyTemplateNode)nodes[1]).ItemsTableName);
			AssertEquals("FK2", ((CollectionCopyTemplateNode)nodes[1]).ItemPropertyName);
			AssertEquals("ParentTableKey2", ((CollectionCopyTemplateNode)nodes[1]).ItemParentTablePropertyName);
		}

		class DummyWithExtraCollections1 : DummyBusinessObject
		{
			public DummyWithExtraCollections1(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[UniversalCopyCollectionEntity("Table1", "FK1")]
			public DummyBusinessObjectCollection Collection1 { get; set; }

			[UniversalCopyCollectionEntity("Table2", "FK2", "ParentTableKey2")]
			public DummyBusinessObjectCollection Collection2 { get; set; }
		}

		[UniversalCopyWithExtendedEntities]
		class DummyWithExtraCollections2 : DummyWithExtraCollections1
		{
			public DummyWithExtraCollections2(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		#endregion

		#region TestReflectExtendedPropertiesIfNeeded_RelatedEntity

		public void TestReflectExtendedPropertiesIfNeeded_RelatedEntity()
		{
			var entityNode = new EntityCopyTemplateNode();

			BusinessObjectCopyManager.ReflectExtendedPropertiesIfNeeded(entityNode, null, typeof(DummyWithExtraRelatedEntity), null, null);
			AssertEquals("1 related entity added", 1, entityNode.Nodes.Count);
			AssertEquals(typeof(RelatedEntityCopyTemplateNode), entityNode.Nodes[0].GetType());
			AssertEquals("RelatedEnity", entityNode.Nodes[0].Name);

			((RelatedEntityCopyTemplateNode)entityNode.Nodes[0]).CopyMethod = RelatedEntityCopyMethod.Copy;
			var copyTemplateTree = new CopyTemplateTree { InnerNode = entityNode };

			var sourceDummy = Factory.New<DummyWithExtraRelatedEntity>();
			AssertNull("Precondition", sourceDummy.RelatedEnity);

			var targetDummy = new BusinessObjectCopyManager().Copy(sourceDummy, copyTemplateTree).Object as DummyWithExtraRelatedEntity;
			AssertNotNull(targetDummy);
			AssertNull("Should not cretate new entity from nothing", targetDummy.RelatedEnity);

			sourceDummy.CreateRelatedEnity();
			AssertNotNull(sourceDummy.RelatedEnity);

			targetDummy = new BusinessObjectCopyManager().Copy(sourceDummy, copyTemplateTree).Object as DummyWithExtraRelatedEntity;
			AssertNotNull(targetDummy);
			AssertNotNull(targetDummy.RelatedEnity);
			AssertEquals("Autocreated", targetDummy.RelatedEnity.Z0_Description);
		}

		public void TestRelatedEntityCopy()
		{
			var entityNode = new EntityCopyTemplateNode();
			BusinessObjectCopyManager.ReflectExtendedPropertiesIfNeeded(entityNode, null, typeof(DummyWithExtraRelatedEntity3), null, null);

			foreach (var node in entityNode.Nodes)
			{
				((RelatedEntityCopyTemplateNode)node).CopyMethod = RelatedEntityCopyMethod.Copy;
			}

			var copyTemplateTree = new CopyTemplateTree { InnerNode = entityNode };

			var sourceDummy = Factory.New<DummyWithExtraRelatedEntity3>();

			var targetDummy = new BusinessObjectCopyManager().Copy(sourceDummy, copyTemplateTree).Object as DummyWithExtraRelatedEntity3;
			AssertNotNull(targetDummy);
			AssertNotNull(targetDummy.RelatedEntity);
			AssertEquals("Should be saved by factory", targetDummy.RelatedEntity.Z0_Description);
		}

		public void TestReflectExtendedPropertiesIfNeeded_RelatedEntity2()
		{
			var entityNode = new EntityCopyTemplateNode();

			BusinessObjectCopyManager.ReflectExtendedPropertiesIfNeeded(entityNode, null, typeof(DummyWithExtraRelatedEntity2), null, null);
			AssertEquals("3 related entities added", 3, entityNode.Nodes.Count);

			var nodes = entityNode.Nodes.OrderBy(node => node.Name).Cast<RelatedEntityCopyTemplateNode>().ToArray();

			AssertRelatedEntityCopyTemplateNode(nodes[0], "RelatedEntity1", "Property1", "Table1", true, false, false);
			AssertRelatedEntityCopyTemplateNode(nodes[1], "RelatedEntity2", "Property2", "Table2", false, true, false);
			AssertRelatedEntityCopyTemplateNode(nodes[2], "RelatedEntity3", "Property3", "Table3", false, false, true);
		}

		public void TestReflectExtendedPropertiesIfNeeded_RelatedEntityExcludePropertyWhichHasUniversalCopyIgnoreBusinessObjectAttribute()
		{
			var entityNode = new EntityCopyTemplateNode();

			BusinessObjectCopyManager.ReflectExtendedPropertiesIfNeeded(entityNode, null, typeof(DummyWithExtraRelatedEntity4), null, null);
			AssertEquals("0 related entities added", 0, entityNode.Nodes.Count);
		}

		void AssertRelatedEntityCopyTemplateNode(RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode, string name,
			string relatedPropertyName, string relatedEntityTableName,
			bool disableCopyMethodCopy, bool disableCopyMethodLink, bool allowCopyMethodLinkCopiedWhenLinkIsDisabled)
		{
			AssertEquals(name, relatedEntityCopyTemplateNode.Name);
			AssertEquals(relatedPropertyName, relatedEntityCopyTemplateNode.RelatedPropertyName);
			AssertEquals(relatedEntityTableName, relatedEntityCopyTemplateNode.RelatedEntityTableName);
			AssertEquals(disableCopyMethodCopy, relatedEntityCopyTemplateNode.DisableCopyMethodCopy);
			AssertEquals(disableCopyMethodLink, relatedEntityCopyTemplateNode.DisableCopyMethodLink);
			AssertEquals(allowCopyMethodLinkCopiedWhenLinkIsDisabled, relatedEntityCopyTemplateNode.AllowCopyMethodLinkCopiedWhenLinkIsDisabled);
		}

		[UniversalCopyWithExtendedEntities]
		class DummyWithExtraRelatedEntity : DummyBusinessObject
		{
			public DummyWithExtraRelatedEntity(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[UniversalCopyRelatedEntity(CreationMethodName = "CreateRelatedEnity")]
			public DummyBusinessObject RelatedEnity { get; private set; }

			public void CreateRelatedEnity()
			{
				RelatedEnity = Factory.New<DummyBusinessObject>();
				RelatedEnity.Z0_Description = "Autocreated";
			}

			public void MakeRelatedEnitySavedByFactory()
			{
				Z0_Description = "Should be saved by factory";
			}
		}

		[UniversalCopyWithExtendedEntities]
		class DummyWithExtraRelatedEntity2 : DummyBusinessObject
		{
			public DummyWithExtraRelatedEntity2(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[UniversalCopyRelatedEntity(RelatedPropertyName = "Property1", RelatedEntityTableName = "Table1",
				DisableCopyMethodCopy = true, DisableCopyMethodLink = false, AllowCopyMethodLinkCopiedWhenLinkIsDisabled = false)]
			public DummyBusinessObject RelatedEntity1 { get; set; }

			[UniversalCopyRelatedEntity(RelatedPropertyName = "Property2", RelatedEntityTableName = "Table2",
				DisableCopyMethodCopy = false, DisableCopyMethodLink = true, AllowCopyMethodLinkCopiedWhenLinkIsDisabled = false)]
			public DummyBusinessObject RelatedEntity2 { get; set; }

			[UniversalCopyRelatedEntity(RelatedPropertyName = "Property3", RelatedEntityTableName = "Table3",
				DisableCopyMethodCopy = false, DisableCopyMethodLink = false, AllowCopyMethodLinkCopiedWhenLinkIsDisabled = true)]
			public DummyBusinessObject RelatedEntity3 { get; set; }
		}

		[UniversalCopyWithExtendedEntities]
		class DummyWithExtraRelatedEntity3 : DummyBusinessObject
		{
			public DummyWithExtraRelatedEntity3(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[UniversalCopyRelatedEntity(DisableCopyMethodCopy = false, DisableCopyMethodLink = true, MakeRelatedEntitySavedByFactoryMethod = "MakeRelatedEnitySavedByFactory")]
			public DummyWithExtraRelatedEntity RelatedEntity
			{
				get
				{
					if (relatedEntity == null)
					{
						relatedEntity = Factory.New<DummyWithExtraRelatedEntity>();
					}
					return relatedEntity;
				}
			}

			DummyWithExtraRelatedEntity relatedEntity;
		}

		[UniversalCopyWithExtendedEntities]
		class DummyWithExtraRelatedEntity4 : DummyBusinessObject
		{
			public DummyWithExtraRelatedEntity4(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[UniversalCopyRelatedEntity]
			public DummyIgnoreUSC RelatedEntity
			{
				get
				{
					if (relatedEntity == null)
					{
						relatedEntity = Factory.New<DummyIgnoreUSC>();
					}
					return relatedEntity;
				}
			}

			DummyIgnoreUSC relatedEntity;
		}

		[UniversalCopyIgnoreBusinessObject]
		class DummyIgnoreUSC : DummyBusinessObject
		{
			public DummyIgnoreUSC(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		#endregion

		#region TestReflectExtendedPropertiesIfNeeded_ExtraProperty

		public void TestReflectExtendedPropertiesIfNeeded_ExtraProperty()
		{
			var entityNode = new EntityCopyTemplateNode();

			BusinessObjectCopyManager.ReflectExtendedPropertiesIfNeeded(entityNode, null, typeof(DummyWithExtraProperty), null, null);
			AssertEquals("1 property added", 1, entityNode.Nodes.Count);
			AssertEquals("ExtraProperty1", entityNode.Nodes[0].Name);
			AssertEquals("DateTime", ((PropertyCopyTemplateNode)entityNode.Nodes[0]).PropertyType);
		}

		[UniversalCopyWithExtendedEntities]
		class DummyWithExtraProperty : DummyBusinessObject
		{
			public DummyWithExtraProperty(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[UniversalCopyExtraProperty]
			public ZDateTime ExtraProperty1 { get; set; }

			public ZDateTime ExtraProperty2 { get; set; }
		}

		#endregion

		#region TestReflectExtendedPropertiesIfNeeded_SplitCollection

		public void TestReflectExtendedPropertiesIfNeeded_SplitCollection()
		{
			var entityNode = new EntityCopyTemplateNode();

			BusinessObjectCopyManager.ReflectExtendedPropertiesIfNeeded(entityNode, null, typeof(DummyWithSplitCollection), null, null);
			AssertEquals("2 nodes should be reflected", 2, entityNode.Nodes.Count);

			var nodes = entityNode.Nodes.OrderBy(node => node.Name).ToArray();

			AssertEquals(typeof(CollectionCopyTemplateNode), nodes[0].GetType());
			AssertEquals("Collection1", nodes[0].Name);
			Assert(string.IsNullOrEmpty(nodes[0].Description));
			AssertEquals("Table", ((CollectionCopyTemplateNode)nodes[0]).ItemsTableName);
			AssertEquals("FK", ((CollectionCopyTemplateNode)nodes[0]).ItemPropertyName);
			AssertEquals("PK", ((CollectionCopyTemplateNode)nodes[0]).ItemParentTablePropertyName);
			AssertNull(((CollectionCopyTemplateNode)nodes[0]).Filter);

			AssertEquals(typeof(CollectionCopyTemplateNode), nodes[1].GetType());
			AssertEquals("Collection1", nodes[1].Name);
			AssertEquals("As", nodes[1].Description);
			AssertEquals("Table", ((CollectionCopyTemplateNode)nodes[1]).ItemsTableName);
			AssertEquals("FK", ((CollectionCopyTemplateNode)nodes[1]).ItemPropertyName);
			AssertEquals("PK", ((CollectionCopyTemplateNode)nodes[1]).ItemParentTablePropertyName);
			AssertNotNull(((CollectionCopyTemplateNode)nodes[1]).Filter);
			AssertEquals(EntityFilterTypeIds.MandatoryExpressionFilter, ((CollectionCopyTemplateNode)nodes[1]).Filter.FilterTypeId);
			AssertEquals("Code like 'A%'", ((CollectionCopyTemplateNode)nodes[1]).Filter.FilterData);
		}

		[UniversalCopyWithExtendedEntities]
		class DummyWithSplitCollection : DummyBusinessObject
		{
			public DummyWithSplitCollection(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[UniversalCopyCollectionEntity("Table", "FK", "PK")]
			[UniversalCopySplitCollection("As", "Code like 'A%'")]
			public DummyBusinessObjectCollection Collection1 { get; set; }
		}

		#endregion

		#region TestReflectExtendedPropertiesIfNeeded_CopyExtraMetadata

		public void TestReflectExtendedPropertiesIfNeeded_RelatedCopyTemplateNode()
		{
			var source = Factory.New<DummyWithRelatedCopyTemplateNodedata>();
			source.Z0_Description = "A";
			source.Z0_Code = "B";

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_Description, CopyMethod = CopyMethod.Copy, CustomCopyTemplateNode = "GetDescriptionCopyTemplateNode" });
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var copyManager = new BusinessObjectCopyManager();
			copyManager.AlwaysCopyViaPropertyForTest = true;

			var target = (DummyWithRelatedCopyTemplateNodedata)copyManager.Copy(source, copyTree).Object;

			AssertEquals("B", target.Z0_Code);
			AssertNotEquals("A", target.Z0_Description);
		}

		[UniversalCopyWithExtendedEntities]
		class DummyWithRelatedCopyTemplateNodedata : DummyBusinessObject
		{
			public DummyWithRelatedCopyTemplateNodedata(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[UniversalCopyExtraProperty(CustomCopyTemplateNode = nameof(GetDescriptionCopyTemplateNode))]
			public override ZString Z0_Description { get => base.Z0_Description; set => base.Z0_Description = value; }

			CopyTemplateNode GetDescriptionCopyTemplateNode()
			{
				return new PropertyCopyTemplateNode { Name = "Z0_Code", CopyMethod = CopyMethod.Copy };
			}
		}

		#endregion

		#region TestReflectExtendedPropertiesIfNeeded_CopyExtraMetadata

		public void TestReflectExtendedPropertiesIfNeeded_CopyExtraMetadata()
		{
			var entityNode = new EntityCopyTemplateNode();

			var codeNode = new PropertyCopyTemplateNode { Name = "Z0_Code" };
			var guidNode = new PropertyCopyTemplateNode { Name = "Z0_Guid" };
			var otherNode = new PropertyCopyTemplateNode { Name = "Z0_Number" };
			var descriptionNode = new PropertyCopyTemplateNode { Name = "Z0_Description" };
			entityNode.Nodes.AddRange(new CopyTemplateNode[] { codeNode, guidNode, otherNode, descriptionNode });

			BusinessObjectCopyManager.ReflectExtendedPropertiesIfNeeded(entityNode, null, typeof(DummyWithExtraMetadata), null, null);

			Assert(codeNode.IsMandatory);
			Assert(string.IsNullOrEmpty(codeNode.Description));

			Assert(!guidNode.IsMandatory);
			AssertEquals("Some Guid", guidNode.Description);

			Assert(!otherNode.IsMandatory);
			Assert(string.IsNullOrEmpty(otherNode.Description));

			AssertEquals(-1, descriptionNode.Priority);
		}

		[UniversalCopyWithExtendedEntities]
		class DummyWithExtraMetadata : DummyBusinessObject
		{
			public DummyWithExtraMetadata(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[UniversalCopyExtraMetadata(IsMandatory = true)]
			public override ZString Z0_Code
			{
				get { return base.Z0_Code; }
				set { base.Z0_Code = value; }
			}

			[UniversalCopyExtraMetadata(HumanReadableName = "Some Guid")]
			public override ZGuid Z0_Guid
			{
				get { return base.Z0_Guid; }
				set { base.Z0_Guid = value; }
			}

			[UniversalCopyExtraMetadata(Priority = -1)]
			public override ZString Z0_Description { get => base.Z0_Description; set => base.Z0_Description = value; }
		}

		#endregion

		#region TestSubstituteChildrenTypes

		public void TestSubstituteChildrenTypes()
		{
			var copyTreeConfiguration = BusinessObjectCopyManager.CopyTreeConfiguration;
			copyTreeConfiguration.PreInitializeEntity(new EntityCopyTemplateNode(), null, typeof(DummyWithChildTypeSubstitution));

			var entityNode = new EntityCopyTemplateNode();
			copyTreeConfiguration.AdditionalEntityInitialization(entityNode, null, typeof(DummyBusinessObject), null, null);

			AssertEquals("Should add 1 property from DummyWithExtraProperty", 1, entityNode.Nodes.Count);
			AssertEquals("ExtraProperty1", entityNode.Nodes[0].Name);
			AssertEquals("DateTime", ((PropertyCopyTemplateNode)entityNode.Nodes[0]).PropertyType);
		}

		[UniversalCopyChildrenSubstituteType(typeof(DummyBusinessObject), typeof(DummyWithExtraProperty))]
		class DummyWithChildTypeSubstitution : DummyBusinessObject
		{
			public DummyWithChildTypeSubstitution(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		#endregion

		#region TestCopyRequiredProperties

		public void TestCopyRequiredProperties()
		{
			var source = Factory.New<DummyWithRequiredProperties>();
			source.Z0_Number = 111;
			source.Z0_AnotherNumber = 222;
			source.Z0_Decimal = 333m;

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_Decimal, CopyMethod = CopyMethod.Copy });
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var copyManager = new BusinessObjectCopyManager();
			copyManager.AlwaysCopyViaPropertyForTest = true;

			var target = (DummyWithRequiredProperties)copyManager.Copy(source, copyTree).Object;

			AssertEquals(111, target.Z0_Number);
			AssertEquals(222, target.Z0_AnotherNumber);
			AssertEquals(333m, target.Z0_Decimal);

			AssertEquals(3, target.UpdatedProperties.Count);
			AssertEquals(DummyBizoSchema.Constants.Z0_AnotherNumber, target.UpdatedProperties[0]);
			AssertEquals(DummyBizoSchema.Constants.Z0_Decimal, target.UpdatedProperties[1]);
			AssertEquals(DummyBizoSchema.Constants.Z0_Number, target.UpdatedProperties[2]);
		}

		public void TestCopyRequiredPropertiesDoesNotAccessPropertySetter()
		{
			var copyManager = new BusinessObjectCopyManager();
			var sourceDummy = Factory.New<DummyWithRequiredProperties>();
			sourceDummy.Z0_Number = 101;
			sourceDummy.Z0_AnotherNumber = 868;

			var targetDummy = Factory.New<DummyWithRequiredProperties>();

			copyManager.CopyRequiredProperties(sourceDummy, targetDummy, UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning);
			AssertEquals(sourceDummy.Z0_AnotherNumber, targetDummy.Z0_AnotherNumber);
			Assert("Target Z0_AnotherNumber.Setter should not be called", !targetDummy.Z0_AnotherNumberSetterAccessed);

			copyManager.CopyRequiredProperties(sourceDummy, targetDummy, UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheEnd);
			AssertEquals(sourceDummy.Z0_Number, targetDummy.Z0_Number);
			Assert("Target Z0_Number.Setter should not be called", !targetDummy.Z0_NumberSetterAccessed);
		}

		class DummyWithLoggedProperties : DummyBusinessObject
		{
			public DummyWithLoggedProperties(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public List<string> UpdatedProperties
			{
				get { return updatedProperties; }
			}
			readonly List<string> updatedProperties = new List<string>();

			public override ZInt Z0_Number
			{
				get { return base.Z0_Number; }
				set
				{
					base.Z0_Number = value;
					updatedProperties.Add(DummyBizoSchema.Constants.Z0_Number);
				}
			}

			public override ZInt Z0_AnotherNumber
			{
				get { return base.Z0_AnotherNumber; }
				set
				{
					base.Z0_AnotherNumber = value;
					updatedProperties.Add(DummyBizoSchema.Constants.Z0_AnotherNumber);
				}
			}

			public override ZDecimal Z0_Decimal
			{
				get { return base.Z0_Decimal; }
				set
				{
					base.Z0_Decimal = value;
					updatedProperties.Add(DummyBizoSchema.Constants.Z0_Decimal);
				}
			}
		}

		[UniversalCopyWithExtendedEntities]
		class DummyWithRequiredProperties : DummyWithLoggedProperties
		{
			internal bool Z0_AnotherNumberSetterAccessed;
			internal bool Z0_NumberSetterAccessed;

			public DummyWithRequiredProperties(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheEnd)]
			public override ZInt Z0_Number
			{
				get { return base.Z0_Number; }
				set
				{
					base.Z0_Number = value;
					Z0_NumberSetterAccessed = true;
				}
			}

			[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
			public override ZInt Z0_AnotherNumber
			{
				get { return base.Z0_AnotherNumber; }
				set
				{
					base.Z0_AnotherNumber = value;
					Z0_AnotherNumberSetterAccessed = true;
				}
			}
		}

		#endregion

		#region TestEnsureElementsOrder

		public void TestTopologicalSort()
		{
			var copyManager = new BusinessObjectCopyManager();
			AssertTopologicalSort("A,B,C", "A-B,B-C,A-C");
			AssertTopologicalSort("A,B,C", "A-C,B-C,A-B");
			AssertTopologicalSort("A,B,C", "B-C,A-B,A-C");
			AssertTopologicalSort("A,B,C", "B-C,A-B");
			AssertTopologicalSort("", "B-A,A-B");

			void AssertTopologicalSort(string expected, string prerequisites)
			{
				var pre = prerequisites.Split(',').Select(p => (p.Split('-')[0], p.Split('-')[1]));
				var res = string.Join(",", copyManager.TopologicalSort(pre));
				AssertEquals(expected, res);
			}
		}

		public void TestSplitCollectionElementOrder()
		{
			AssertEquals(-1,
			new CopyManager.TemplateNodeCopyOrderComparer().Compare(new CollectionCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_Guid, Filter = new EntityFilter(), IsSplitCollection = true, Description = null },
			new CollectionCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_Guid, Filter = new EntityFilter(), IsSplitCollection = false, Description = "Filter" })
			);
		}

		public void TestEnsureElementsOrder()
		{
			var source = Factory.New<DummyWithOrderedProperties>();
			source.Z0_Number = 111;
			source.Z0_AnotherNumber = 222;
			source.Z0_Decimal = 333m;

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_Number, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_AnotherNumber, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_Decimal, CopyMethod = CopyMethod.Copy });
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var copyManager = new BusinessObjectCopyManager { AlwaysCopyViaPropertyForTest = true };

			var target = (DummyWithOrderedProperties)copyManager.Copy(source, copyTree).Object;

			AssertEquals(111, target.Z0_Number);
			AssertEquals(222, target.Z0_AnotherNumber);
			AssertEquals(333m, target.Z0_Decimal);

			AssertEquals(3, target.UpdatedProperties.Count);
			AssertEquals(DummyBizoSchema.Constants.Z0_AnotherNumber, target.UpdatedProperties[0]);
			AssertEquals(DummyBizoSchema.Constants.Z0_Decimal, target.UpdatedProperties[1]);
			AssertEquals(DummyBizoSchema.Constants.Z0_Number, target.UpdatedProperties[2]);
		}

		[UniversalCopyWithExtendedEntities]
		[UniversalCopyElementsOrder(DummyBizoSchema.Constants.Z0_AnotherNumber, DummyBizoSchema.Constants.Z0_Decimal)]
		[UniversalCopyElementsOrder(DummyBizoSchema.Constants.Z0_Decimal, DummyBizoSchema.Constants.Z0_Number)]
		[UniversalCopyElementsOrder(DummyBizoSchema.Constants.Z0_AnotherNumber, DummyBizoSchema.Constants.Z0_Number)]
		class DummyWithOrderedProperties : DummyWithLoggedProperties
		{
			public DummyWithOrderedProperties(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		public void TestEnsureElementsOrder2()
		{
			var source = Factory.New<DummyWithOrderedProperties2>();
			source.Z0_Number = 111;
			source.Z0_AnotherNumber = 222;
			source.Z0_Decimal = 333m;

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_Decimal, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_AnotherNumber, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_Number, CopyMethod = CopyMethod.Copy });
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var copyManager = new BusinessObjectCopyManager { AlwaysCopyViaPropertyForTest = true };

			var target = (DummyWithOrderedProperties2)copyManager.Copy(source, copyTree).Object;

			AssertEquals(111, target.Z0_Number);
			AssertEquals(222, target.Z0_AnotherNumber);
			AssertEquals(333m, target.Z0_Decimal);

			AssertEquals(3, target.UpdatedProperties.Count);
			AssertEquals(DummyBizoSchema.Constants.Z0_Number, target.UpdatedProperties[0]);
			AssertEquals(DummyBizoSchema.Constants.Z0_AnotherNumber, target.UpdatedProperties[1]);
			AssertEquals(DummyBizoSchema.Constants.Z0_Decimal, target.UpdatedProperties[2]);
		}

		[UniversalCopyWithExtendedEntities]
		[UniversalCopyElementsOrder(DummyBizoSchema.Constants.Z0_AnotherNumber, DummyBizoSchema.Constants.Z0_Decimal)]
		[UniversalCopyElementsOrder(DummyBizoSchema.Constants.Z0_Number, DummyBizoSchema.Constants.Z0_AnotherNumber)]
		class DummyWithOrderedProperties2 : DummyWithLoggedProperties
		{
			public DummyWithOrderedProperties2(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		#endregion

		#region Test copy element in active collection triggers HasChangesChanged

		public void TestCopyElementInActiveCollectionTriggersHasChangesChanged()
		{
			var dummy = Factory.New<DummyWithActiveCollection>();

			var element1 = Factory.New<DummyBusinessObject>();
			element1.Z0_Number = 100;

			Factory.Save();

			AssertEquals(1, dummy.ActiveCollection.Count);
			AssertSame(element1, dummy.ActiveCollection[0]);
			Assert(!dummy.HasChanges);

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_Number, CopyMethod = CopyMethod.Copy });
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var element2 = (DummyBusinessObject)new BusinessObjectCopyManager().Copy(element1, copyTree).Object;

			AssertEquals(2, dummy.ActiveCollection.Count);
			AssertSame(element1, dummy.ActiveCollection[0]);
			AssertSame(element2, dummy.ActiveCollection[1]);
			Assert("New element should have changes", element2.HasChanges);
			Assert("Collection should have changes", ((IBusinessObjectState)dummy.ActiveCollection).HasChanges);
			Assert("Parent object should have changes", dummy.HasChanges);
		}

		class DummyWithActiveCollection : DummyBusinessObject
		{
			public DummyWithActiveCollection(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ActiveBusinessObjectCollection<DummyBusinessObject> ActiveCollection
			{
				get
				{
					if (activeCollection == null)
					{
						activeCollection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
						activeCollection.AdditionalFilter = new ZQuery(DummyBizoSchema.Z0_Number, 100);
						RegisterEditableChildObject(activeCollection);
					}
					return activeCollection;
				}
			}
			ActiveBusinessObjectCollection<DummyBusinessObject> activeCollection;
		}

		#endregion

		#region TestCopyBlobProperty

		public void TestCopyBlobProperty()
		{
			var data = new byte[2048];
			new Random().NextBytes(data);
			data[0] = 0x50; // To prevent packing data and making it smaller then 1024 bytes in db
			data[1] = 0x5A;
			Dummy.Z0_VarBinaryMax = data;
			Dummy.Factory.Save();

			var dummyInOtherFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyBusinessObject>(Dummy.PK);
			Assert("BLOB should not be loaded yet", LazyLoading.LoadRequired(((INeedRow)dummyInOtherFactory).Row[DummyBizoSchema.Constants.Z0_VarBinaryMax]));

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_VarBinaryMax, CopyMethod = CopyMethod.Copy });
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var dummyCopy = (DummyBusinessObject)new BusinessObjectCopyManager().Copy(dummyInOtherFactory, copyTree).Object;

			Assert("BLOB should have been loaded", !LazyLoading.LoadRequired(((INeedRow)dummyInOtherFactory).Row[DummyBizoSchema.Constants.Z0_VarBinaryMax]));
			AssertEquals(data, (byte[])dummyCopy.Z0_VarBinaryMax);
		}

		public void TestCopyBlobPropertyOnBizoDeletedInDb()
		{
			var data = new byte[2048];
			new Random().NextBytes(data);
			data[0] = 0x50; // To prevent packing data and making it smaller then 1024 bytes in db
			data[1] = 0x5A;
			Dummy.Z0_VarBinaryMax = data;
			Dummy.Factory.Save();

			var dummyInOtherFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyBusinessObject>(Dummy.PK);
			Assert("BLOB should not be loaded yet", LazyLoading.LoadRequired(((INeedRow)dummyInOtherFactory).Row[DummyBizoSchema.Constants.Z0_VarBinaryMax]));

			Dummy.Delete();
			Dummy.Factory.Save();

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_VarBinaryMax, CopyMethod = CopyMethod.Copy });
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var dummyCopy = (DummyBusinessObject)new BusinessObjectCopyManager().Copy(dummyInOtherFactory, copyTree).Object;

			Assert("Blob field should remain empty", LazyLoading.LoadRequired(((INeedRow)dummyInOtherFactory).Row[DummyBizoSchema.Constants.Z0_VarBinaryMax]));
			AssertEquals("Lazy-loaded marker should be copied", LazyLoading.BinaryPlaceholder, dummyCopy.Z0_VarBinaryMax);
			AssertEquals("Whilst you were working another user has deleted some information you are attempting to copy and copy result can be incomplete. Please cancel this copy, refresh information you are copying, and retry the copy action.",
				UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
		}

		#endregion

		#region Test Copy ZAddress Property

		public void TestZAddressOrgPKShouldBeUpdatedAfterUniversalCopy()
		{
			var department = EnvProxy.Instance.CurrentDepartment;
			department.GetType().GetProperty(GlbDepartmentSchema.Constants.GE_Import).SetValue(department, (ZBool)true);
			var consol = Factory.New<Forwarding.IForwardingConsol>();

			var orgHeader = Factory.Load<IOrgHeader>(EnvProxy.Instance.CurrentCompany.OrganisationPK);
			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;

			var copyTemplate = new CopyTemplateTree(consol.GetType());
			var propertyNode = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)copyTemplate.InnerNode).Nodes.Find(node => node is PropertyCopyTemplateNode && node.Name == nameof(consol.JK_OA_CreditorAddress));
			propertyNode.CopyMethod = CopyMethod.Copy;
			propertyNode.Value = string.Empty;

			var copyManager = new BusinessObjectCopyManager();
			var target = copyManager.Copy(consol, copyTemplate).Object as Forwarding.IForwardingConsol;
			var zAddressProperty = target.GetType().GetProperty($"{JobConsolSchema.Constants.JK_OA_CreditorAddress}{ZAddress.Schema.BindingSuffix}");
			var creditorAddress = (ZAddress)zAddressProperty.GetValue(target);
			AssertEquals("Address FK(ZGuid) should be copied.", consol.JK_OA_CreditorAddress, target.JK_OA_CreditorAddress);
			AssertEquals("ZAddress.AddressFK should be copied.", consol.JK_OA_CreditorAddress, creditorAddress.AddressFK);
			AssertEquals("ZAddress.OrgPK should be updated after universal copy", EnvProxy.Instance.CurrentCompany.OrganisationPK, creditorAddress.OrgPK);
		}

		#endregion

		#region Test Classes

		[UniversalCopyInstanceType(InstanceType = typeof(UniversalCopyDummy), CreationMethod = "NewForUniversalCopy")]
		class UniversalCopyDummy : DummyBusinessObject, IStmALogParent
		{
			public UniversalCopyDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected object NewForUniversalCopy(BusinessObjectFactory factory, object parentEntity)
			{
				var dummy = factory.New<UniversalCopyDummy>();
				dummy.Z0_Description = "Customized";
				return dummy;
			}

			#region IStmALogParent

			bool IStmALogParent.IsDeleted => IsDeleted;

			ZGuid IStmALogParent.LogsParentPK => PK;

			string IStmALogParent.LogsParentTableName => DummyBusinessObject.Schema.TableName;

			BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents => Array.Empty<BusinessObject>();

			bool IStmALogParent.DeferFiringWorkflow => false;

			Logs IStmALogProvider.Logs => logs ?? (logs = new Logs(this));
			Logs logs;

			BusinessObjectFactory IStmALogProvider.LogsFactory => Factory;

			void IStmALogParent.ProcessLog(IStmALog log) { }

			#endregion
		}

		class BusinessObjectCopyManagerForTest : BusinessObjectCopyManager
		{
			public BusinessObjectCopyManagerForTest(BusinessObjectFactory defaultFactory = null) : base(defaultFactory)
			{
			}

			protected override bool WasCreatedByMe(object entity)
			{
				return CreateByMeObjects.Contains(entity);
			}

			public List<BusinessObject> CreateByMeObjects
			{
				get { return createByMeObjects ?? (createByMeObjects = new List<BusinessObject>()); }
			}
			List<BusinessObject> createByMeObjects;

			public Dictionary<object, object> CopiedEntities
			{
				get => copiedEntities;
				set => copiedEntities = value;
			}

			public object CreateNewEntityFromCoreForTesting(object parentEntity, object sourceEntity, CopyTemplateNode copyTemplateNode, string propertyName) => CreateNewEntityFromCore(parentEntity, sourceEntity, copyTemplateNode, propertyName);
			public object GetEntityPkCoreForTesting(object entity) => GetEntityPKCore(entity);
			public string GetEntityCodeCoreForTesting(object entity) => GetEntityCodeCore(entity);
			public void SetEntityRelationshipForTesting(object targetEntity, object relatedEntity, RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode) => SetEntityRelationship(targetEntity, relatedEntity, relatedEntityCopyTemplateNode);
			public string ProcessMacrosCoreForTesting(string value, IEnumerable<object> rootEntities) => ProcessMacrosCore(value, rootEntities);
			public void SetPropertyValueCoreForTesting(object target, string propertyName, object value) => SetPropertyValueCore(target, propertyName, value);
			public void SetPropertyValueForTesting(object target, string propertyName, object value) => SetPropertyValue(target, propertyName, value);
			public void SetDataPropertyValueForTesting(object target, string propertyName, object value) => SetDataPropertyValue(target, propertyName, value);
			public object GetRelatedEntityFromDbForTesting(object sourceEntity, RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode) => GetRelatedEntityFromDb(sourceEntity, relatedEntityCopyTemplateNode);
			public void PostCopyEntityForTesting(object sourceEntity, object targetEntity, CopyTemplateNode copyTemplateNode) => PostCopyEntity(sourceEntity, targetEntity, copyTemplateNode);
			public void PreCopyEntityForTesting(object sourceEntity, object targetEntity, CopyTemplateNode copyTemplateNode) => PreCopyEntity(sourceEntity, targetEntity, copyTemplateNode);
			public void PrepareForCopyForTesting(object source, CopyTemplateTree copyTemplate) => PrepareForCopy(source, copyTemplate);
			public void FinishCopyForTesting(object source, object copy, CopyTemplateTree copyTemplate) => FinishCopy(source, copy, copyTemplate);
			public IEnumerable GetCollectionCoreForTesting(object sourceEntity, CollectionCopyTemplateNode collectionCopyTemplateNode) => GetCollectionCore(sourceEntity, collectionCopyTemplateNode);
			public IEnumerable GetCollectionFromDbForTesting(object sourceEntity, CollectionCopyTemplateNode collectionCopyTemplateNode) => GetCollectionFromDb(sourceEntity, collectionCopyTemplateNode);
			public void PrepareTargetCollectionForTesting(object targetEntity, CollectionCopyTemplateNode collectionCopyTemplateNode) => PrepareTargetCollection(targetEntity, collectionCopyTemplateNode);
			public void FinishTargetCollectionForTesting(object targetEntity, CollectionCopyTemplateNode collectionCopyTemplateNode) => FinishTargetCollection(targetEntity, collectionCopyTemplateNode);
			public IEnumerable FilterCollectionCoreForTesting(IEnumerable collection, CollectionCopyTemplateNode collectionCopyTemplateNode, IEnumerable<string> path) => FilterCollectionCore(collection, collectionCopyTemplateNode, path);
			public void SetCollectionRelationshipForTesting(object targetEntity, object collectionItem, CollectionCopyTemplateNode collectionCopyTemplateNode) => SetCollectionRelationship(targetEntity, collectionItem, collectionCopyTemplateNode);
		}

		#endregion
	}
}
