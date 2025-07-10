using System;
using System.Data;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectTestCase : TestCaseWithFactory
	{
		public void TestOnFactorySaveDoesNotCallHasChangesUnnecessarily()
		{
			var mock = Factory.New<DummyBusinessObjectHasChangesTest>();
			AssertNoExceptionThrown(() =>
			{
				mock.CanThrowHasChanges = true;
				mock.OnFactorySavingInternal();
			});
		}

		#region Test Support Class - TestOnFactorySaveDoesNotCallHasChangesUnnecessarily()
		class DummyBusinessObjectHasChangesTest : DummyBusinessObject
		{
			public DummyBusinessObjectHasChangesTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool CanThrowHasChanges { get; set; }

			public override bool HasChanges { get => CanThrowHasChanges ? throw new NotImplementedException() : base.HasChanges; set => base.HasChanges = value; }
		}
		#endregion

		public void TestAddInfoEnableSynchronizationIsCalledOnNewAndLoaded()
		{
			var bizObj = Factory.New<DummyBusinessObjectWithIAddInfoWithSyncPropertySupporter>();
			AssertEquals("Called from SetDefaultValues", 1, bizObj.AddInfoForTesting.EnableSynchronizationCalledCount);
			bizObj = Factory.Load<DummyBusinessObjectWithIAddInfoWithSyncPropertySupporter>(bizObj.PK);
			AssertEquals("No extra called", 1, bizObj.AddInfoForTesting.EnableSynchronizationCalledCount);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			bizObj = newFactory.Load<DummyBusinessObjectWithIAddInfoWithSyncPropertySupporter>(bizObj.PK);
			AssertEquals("Called from OnLoaded", 1, bizObj.AddInfoForTesting.EnableSynchronizationCalledCount);
			bizObj = newFactory.Load<DummyBusinessObjectWithIAddInfoWithSyncPropertySupporter>(bizObj.PK);
			AssertEquals("No extra called", 1, bizObj.AddInfoForTesting.EnableSynchronizationCalledCount);
		}

		class DummyBusinessObjectWithIAddInfoWithSyncPropertySupporter : DummyBusinessObject, IAddInfoWithSyncPropertySupporter
		{
			public DummyBusinessObjectWithIAddInfoWithSyncPropertySupporter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public AddInfo AddInfoForTesting => addInfoForTesting ?? (addInfoForTesting = new AddInfo(Z0_XmlInfo));
			AddInfo addInfoForTesting;

			IAddInfoWithSyncProperty IAddInfoWithSyncPropertySupporter.AddInfo => AddInfoForTesting;

			public class AddInfo : IAddInfoWithSyncProperty
			{
				public AddInfo(ZPropertyInfo addInfoProperty)
				{
					this.AddInfoProperty = addInfoProperty;
				}

				public int EnableSynchronizationCalledCount;

				public ZPropertyInfo AddInfoProperty { get; private set; }

				public void EnableSynchronization()
				{
					EnableSynchronizationCalledCount++;
				}

				IZType IAddInfoWithSyncProperty.GetAddInfoValue(IZType data, Type addInfoValueType) => data;
			}
		}

		public void TestAddInfoChildSupporterBizObjCreateChild()
		{
			var newFactory = new BusinessObjectFactory();
			var supporter = newFactory.New<DummyBizObjWithAddInfoChildSupporter>();
			var children = newFactory.Load<DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter>(new ZQuery(DummyBizoSchema.Z0_Guid, supporter.PK));
			AssertEquals(1, children.Length);
			var child = children[0];
			AssertSame(child, supporter.AddInfoChild);
			AssertEquals("child.HasChanges", false, child.HasChanges);
			AssertEquals("supporter.HasChanges", false, supporter.HasChanges);

			newFactory.Save();
			newFactory = new BusinessObjectFactory();
			AssertEquals(true, newFactory.ExistsInDatabase(DummyBusinessObject.Schema.TableName, new ZQuery(DummyBizoSchema.PK, child.PK)));
		}

		public void TestAddInfoChildSupporterDeleteChild()
		{
			var supporter = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			var child = supporter.AddInfoChild;
			supporter.OnBeforeSuccessfulDelete += (o, e) => { _ = supporter.AddInfoChild; };
			supporter.Delete();
			AssertEquals(true, child.IsDeleted);

			var children = Factory.Load<DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter>(new ZQuery(DummyBizoSchema.Z0_Guid, supporter.PK));
			AssertEquals("AddInfoChild should not recreate on deleting", 0, children.Length);
		}

		public void TestAddInfoChildSupporterMarkAsNeedingValidation()
		{
			var supporter = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			var child = supporter.AddInfoChild;
			supporter.RunPreSaveValidation();
			Assert(!supporter.ShouldValidateOnSave);
			Assert(!child.ShouldValidateOnSave);
			supporter.MarkAsNeedingValidation();
			Assert(supporter.ShouldValidateOnSave);
			Assert(child.ShouldValidateOnSave);
		}

		public void TestAddInfoChildSupporterClone()
		{
			var supporter = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			var child = supporter.AddInfoChild;
			supporter.Clone();
			child.Z0_Description = "Set!";
			child.Z0_Decimal = 5;
			var clone = (DummyBizObjWithAddInfoChildSupporter)supporter.Clone();
			AssertEquals("Values copied", "Set!", clone.AddInfoChild.Z0_Description);
			AssertEquals("Values copied", (ZDecimal)5, clone.AddInfoChild.Z0_Decimal);
			AssertEquals("IsInDatabase", false, clone.AddInfoChild.IsInDatabase);
			AssertEquals("HasChanges", false, clone.AddInfoChild.HasChanges);
		}

		public void TestHasChangesChangedEventArgs()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var child = Factory.New<DummyBusinessObject>();
			dummy.RegisterEditableChildObject(child);
			var objectToChange = child;
			dummy.HasChangesChanged += HasChangesChangedHandler;
			child.HasChanges = true;

			void HasChangesChangedHandler(object sender, HasChangesChangedEventArgs e)
			{
				AssertEquals(objectToChange, e.ObjectThatWasChanged);
			}
		}

		public void TestChangingMasterClusterKeyWillUpdateChildrenClusterKey()
		{
			var company = (IGlbCompany)EnvProxy.Instance.CurrentCompany;
			using (company.TemporarilySetCountry("TW"))
			{
				var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				var declaration2 = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				((BusinessObject)declaration).LoadChildEditableObjects();
				var topInvoiceQuery = new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, declaration.PK);
				topInvoiceQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.True);
				topInvoiceQuery.FetchOnlyFromLocalCache = true;
				var topInvoice = Factory.LoadTop1<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceGroupHeader>(topInvoiceQuery);
				var invoice = Factory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
				invoice.JZ_JE = declaration.PK;
				invoice.JZ_JZ_GroupInvoiceFK = topInvoice.PK;
				var invoiceLine = Factory.New<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>();
				invoiceLine.JI_JZ = invoice.PK;
				var invoiceLineRef = Factory.New<Enterprise.Integration.Customs.Shared.IJobComInvLineRefs>();
				invoiceLineRef.JG_JI = invoiceLine.PK;
				invoiceLineRef.JG_ReferenceType = "CHAS";
				invoiceLineRef.JG_ReferenceNumber = "ABC123";
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				invoice = newFactory.Load<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>(invoice.PK);
				var newClusterKey = declaration2.JE_ClusterKey;
				invoice.JZ_ClusterKey = declaration2.JE_ClusterKey;
				((BusinessObject)invoice).LoadChildEditableObjects();
				invoiceLine = newFactory.Load<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>(invoiceLine.PK);
				AssertEquals("Invoice Line ClusterKey should have been changed", newClusterKey, invoiceLine.JI_ClusterKey);
				invoiceLineRef = newFactory.Load<Enterprise.Integration.Customs.Shared.IJobComInvLineRefs>(invoiceLineRef.PK);
				AssertEquals("Invoice Line Ref ClusterKey should have been changed", newClusterKey, invoiceLineRef.JG_ClusterKey);

				invoiceLine.JI_ClusterKey = 23424234;
				AssertEquals("Invoice Line Ref should not have changed as Invoice Line is not a master", newClusterKey, invoiceLineRef.JG_ClusterKey);
			}
		}

		[ExpectNoExceptions]
		public void TestWhatMakesAZDateTimeOffset()
		{
			var dummy = Factory.New<DummyBaseBusinessObject>();
			dummy.Z0_DateTimeOffset = new ZDateTimeOffset(ZDateTime.Now, TimeSpan.FromMinutes(0));
			Assert(ZDateTimeOffset.IsValidOffset(TimeSpan.FromMinutes(0)));
			Factory.Save();
			dummy.Z0_DateTimeOffset = new ZDateTimeOffset(ZDateTime.Now, TimeSpan.FromMinutes(10));
			Assert(ZDateTimeOffset.IsValidOffset(TimeSpan.FromMinutes(10)));
			Factory.Save();
			dummy.Z0_DateTimeOffset = new ZDateTimeOffset(ZDateTime.Now, TimeSpan.FromMinutes(15));
			Assert(ZDateTimeOffset.IsValidOffset(TimeSpan.FromMinutes(15)));
			Factory.Save();
			dummy.Z0_DateTimeOffset = new ZDateTimeOffset(ZDateTime.Now, TimeSpan.FromMinutes(30));
			Assert(ZDateTimeOffset.IsValidOffset(TimeSpan.FromMinutes(30)));
			Factory.Save();
			dummy.Z0_DateTimeOffset = new ZDateTimeOffset(ZDateTime.Now, TimeSpan.FromMinutes(60));
			Assert(ZDateTimeOffset.IsValidOffset(TimeSpan.FromMinutes(60)));
			Factory.Save();
			dummy.Z0_DateTimeOffset = new ZDateTimeOffset(ZDateTime.Now, TimeSpan.FromMinutes(1));
			Assert(ZDateTimeOffset.IsValidOffset(TimeSpan.FromMinutes(1)));
			Factory.Save();

			Assert(!ZDateTimeOffset.IsValidOffset(TimeSpan.FromSeconds(1)));
			Assert(!ZDateTimeOffset.IsValidOffset(TimeSpan.FromMilliseconds(1)));
		}

		public void TestDeletingBusinessObjectsViaDataRefresh_DoesNotCauseConcurrencyError()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();
			var otherFactory = Factory.CreateNewFactory();
			var otherDummy = otherFactory.Load<DummyBusinessObject>(dummy.PK);
			dummy.Delete();
			Factory.Save();
			AssertNoExceptionThrown("No concurrency error should occur", () => otherFactory.Save());
		}

		public void TestDeletingBusinessObjectsViaDeleteForDataRefresh_DoesNotCauseConcurrencyError()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();
			var otherFactory = Factory.CreateNewFactory();
			otherFactory.RefreshEnabled = false;
			var otherDummy = otherFactory.Load<DummyBusinessObject>(dummy.PK);
			dummy.Delete();
			Factory.Save();
			((IBusiness)otherDummy).DeleteForDataRefresh();
			AssertNoExceptionThrown("No concurrency error should occur", () => otherFactory.Save());
		}

		public void TestCopyValuesFrom_NormalClone()
		{
			AssertCopyIsIdentical(new BusinessObjectCloneArgs(Enumerable.Empty<string>(), performRowCopyWithoutTriggeringValidationAndSetter: false));
		}

		public void TestCopyValuesFrom_DataRowCopy()
		{
			AssertCopyIsIdentical(new BusinessObjectCloneArgs(Enumerable.Empty<string>(), performRowCopyWithoutTriggeringValidationAndSetter: true));
		}

		public void TestCopyDeciderGetsToHaveTurn_AtCheckingBlobColumns()
		{
			var dummy = CreatePopulatedDummy();
			Factory.Save();
			var factoryForLoad = new BusinessObjectFactory();
			var loadedDummy = factoryForLoad.Load<DummyBusinessObject>(dummy.PK);
			var args = new BusinessObjectCloneArgs(factoryForLoad, Enumerable.Empty<string>(), typeof(DummyBusinessObject), true, (bizo1, bizo2, column) => column.ColumnName != DummyBizoSchema.Z0_VarBinaryMax.Name);
			var clonedDummy = (DummyBusinessObject)loadedDummy.Clone(args);
			factoryForLoad.Save();

			AssertNotEquals("Shouldn't have copied this lazy row", loadedDummy.Z0_VarBinaryMax.ToUTF8(), clonedDummy.Z0_VarBinaryMax.ToUTF8());
			AssertEquals("But should have copied this one.", loadedDummy.Z0_Xml, clonedDummy.Z0_Xml);
		}

		void AssertCopyIsIdentical(BusinessObjectCloneArgs args)
		{
			var dummy = CreatePopulatedDummy();

			Factory.Save();

			var factoryForLoad = new BusinessObjectFactory();
			var loadedDummy = factoryForLoad.Load<DummyBusinessObject>(dummy.PK);
			var clonedDummy = loadedDummy.Clone(args);

			factoryForLoad.Save();

			AssertIsLoaded(DummyBizoSchema.Z0_VarBinaryMax, clonedDummy);
			AssertIsLoaded(DummyBizoSchema.Z0_NVarCharMax, clonedDummy);
			AssertIsLoaded(DummyBizoSchema.Z0_Xml, clonedDummy);

			CombineAssertions(() =>
			{
				foreach (var column in DummyBizoSchema.All)
				{
					if (loadedDummy.FindPropertyInfo(column.Name) != null)
					{
						AssertColumnEqual(loadedDummy, clonedDummy, column);
					}
				}
			});
		}

		static void AssertColumnEqual(DummyBusinessObject loadedDummy, BusinessObject clonedDummy, SchemaColumn column)
		{
			var loaded = loadedDummy[column];
			var cloned = clonedDummy[column];
			if (typeof(Array).IsAssignableFrom(loaded.GetType()))
			{
				AssertArrayEqualsByElements((object[])loaded, (object[])cloned);
			}
			else if (loaded is ZBlob blob1 && cloned is ZBlob blob2)
			{
				AssertArrayEqualsByElements(column.Name, (byte[])blob1, (byte[])blob2);
			}
			else
			{
				AssertEquals(column.Name, loaded, cloned);
			}
		}

		void AssertIsLoaded(SchemaColumn lazyColumn, BusinessObject clonedDummy)
		{
			Assert("The property must be loaded since you cannot lazily stream blobs from the database (since the row could be deleted at any time): " + clonedDummy.Row[lazyColumn.Name].ToString(), !LazyLoading.LoadRequired(clonedDummy.Row[lazyColumn.Name]));
		}

		DummyBusinessObject CreatePopulatedDummy()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			foreach (var column in DummyBizoSchema.All)
			{
				GenerateTestDataForColumn(dummy, column);
			}
			return dummy;
		}

		ZBlob GetBigBlob(int length)
		{
			var random = new Random();
			var bigByteArray = Enumerable.Range(0, 10000).Select(i => (byte)(random.Next() % 254)).ToArray();
			return new ZBlob(bigByteArray);
		}

		void GenerateTestDataForColumn(BusinessObject bizo, SchemaColumn column)
		{
			if (bizo.FindPropertyInfo(column.Name) == null)
			{
			}
			else if (column is SchemaPKColumn col)
			{
			}
			else if (column is SchemaXmlColumn xmlCol)
			{
				var random = new Random();
				bizo[column] = $"<IMXML>{string.Join("", Enumerable.Range(0, 10000).Select(i => random.Next(0, 9).ToString()))}</IMXML>";
			}
			else if (column is SchemaBinaryColumn binaryCol)
			{
				bizo[column] = GetBigBlob(binaryCol.MaxLength);
			}
			else if (column is SchemaDateTimeColumn dateTimeCol)
			{
				bizo[column] = ZDateTime.Now;
			}
			else if (column is SchemaShortColumn shortCol)
			{
				bizo[column] = new ZShort(12);
			}
			else if (column is SchemaIntColumn intCol)
			{
				bizo[column] = new ZInt(25);
			}
			else if (column is SchemaLongColumn longCol)
			{
				bizo[column] = new ZLong(34);
			}
			else if (column is SchemaGuidColumn guidCol)
			{
				bizo[column] = ZGuid.NewZGuid();
			}
			else if (column is SchemaGeographyColumn geographyCol)
			{
				bizo[column] = new ZGeography("POINT(-122.3 47.6)");
			}
			else if (column is SchemaDecimalColumn decimalCol)
			{
				bizo[column] = new ZDecimal(0.131d);
			}
			else if (column is SchemaDateTimeOffsetColumn dateTimeOffsetCol)
			{
				bizo[column] = ZDateTimeOffset.Now;
			}
			else if (column is SchemaDateColumn dateCol)
			{
				bizo[column] = ZDate.BrettsBirthday;
			}
			else if (column is SchemaTimeColumn timeCol)
			{
				bizo[column] = new ZTime(1,2);
			}
			else if (column is SchemaStringColumn stringCol)
			{
				var maxLength = bizo.GetZPropertyInfo(column.Name).MaxLength;
				bizo[column] = new ZString(GetBigBlob(maxLength).ToUTF8()).SubstringSafe(0, maxLength);
			}
			else if (column is SchemaByteColumn byteCol)
			{
				bizo[column] = new ZByte(3);
			}
			else if (column is SchemaBoolColumn boolCol)
			{
				bizo[column] = ZBool.True;
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}
