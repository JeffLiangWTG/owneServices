using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class BusinessObjectDataProviderTest : TestCaseWithFactory
	{
		public void TestFindColumnWhenHasDataSourcePrefix()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org1.PK;

			((BusinessObject)shipment).SetUserDefinedValue("shipment 1", ZBool.True);
			((BusinessObject)shipment).SetUserDefinedValue("shipment 2", new ZString("Lots Of Ice"));
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_OH_Importer = org2.PK;
			((BusinessObject)declaration).SetUserDefinedValue("declaration 1", ZBool.True);
			((BusinessObject)declaration).SetUserDefinedValue("declaration 2", new ZString("Lots Of Ice"));

			var shipmentBO = shipment as BusinessObject;
			var declarationBO = declaration as BusinessObject;
			var dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(shipmentBO), BODocDataProvider.Get(declarationBO)), null);

			AssertNotNull("org1", dataProvider.FindColumn("Consignee.OH_Code"));
			AssertNotNull("org2", dataProvider.FindColumn("_DataSource.JobDeclaration.Consignee.OH_Code"));
			AssertNotNull("shipment 1", dataProvider.FindColumn("GetCustomField(shipment 1)"));
			AssertNotNull("declaration 1", dataProvider.FindColumn("_DataSource.JobDeclaration.GetCustomField(declaration 1)"));

			AssertNotNull("Whatever Custom Field existed in any type, it will create a new MethodInfoChainLink, see GetCustomFieldFunctionExtractor.AddMethodInfoChainLinks", dataProvider.FindColumn("GetCustomField(XX)"));
			AssertNotNull("Whatever Custom Field existed in the type, it will create a new MethodInfoChainLink", dataProvider.FindColumn("_DataSource.JobDeclaration.GetCustomField(shipment 1)"));

			AssertNull("Empty DataSource", dataProvider.FindColumn("_DataSource"));
			AssertNull("DataSourceType XX Not Found", dataProvider.FindColumn("_DataSource.XX"));
			AssertNull("Field XX Not Found", dataProvider.FindColumn("_DataSource.JobDeclaration.XX"));

			dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(shipmentBO)), null);
			AssertNull("DataSourceType JobDeclaration Not Found", dataProvider.FindColumn("_DataSource.JobDeclaration.GetCustomField(declaration 1)"));
		}

		public void TestGetColumnValueShouldReturnCorrectWhenHasDataSourcePrefix()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org1.PK;
			shipment.JS_HouseBill = "JS";
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_OH_Importer = org2.PK;
			declaration.JE_HouseBill = "JE";

			var shipmentBO = shipment as BusinessObject;
			var declarationBO = declaration as BusinessObject;
			var dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(shipmentBO), BODocDataProvider.Get(declarationBO)), null);

			AssertEquals("org1", dataProvider.GetColumnValue(null, 0, "Consignee.OH_Code"));
			AssertEquals("org2", dataProvider.GetColumnValue(null, 0, "_DataSource.JobDeclaration.Consignee.OH_Code"));
			AssertEquals("JS", dataProvider.GetColumnValue(null, 0, "JS_HouseBill"));
			AssertEquals("JE", dataProvider.GetColumnValue(null, 0, "JE_HouseBill"));

			AssertEquals("no error for GetCustomField and return empty - see GetFieldValueFromMethodInfoChain", "", dataProvider.GetColumnValue(null, 0, "GetCustomField(XX)"));
			AssertEquals("no error for GetCustomField and return empty", "", dataProvider.GetColumnValue(null, 0, "_DataSource.JobDeclaration.GetCustomField(XX)"));

			AssertExceptionThrown<FieldNotFoundException>("Empty DataSource",
MacroDataSource.EmptyDataSourceTypeError,
() => dataProvider.GetColumnValue(null, 0, "_DataSource"));

			AssertExceptionThrown<FieldNotFoundException>("DataSourceType XX Not Found",
MacroDataSource.GetDataSourceTypeNotFoundMessage("XX"),
() => dataProvider.GetColumnValue(null, 0, "_DataSource.XX"));

			AssertExceptionThrown<FieldNotFoundException>("Field Not Found",
"Field <XX> not found on DataSource Type [JobDeclaration].",
() => dataProvider.GetColumnValue(null, 0, "_DataSource.JobDeclaration.XX"));

			dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(shipmentBO)), null);
			AssertExceptionThrown<FieldNotFoundException>("DataSourceType JobDeclaration Not Found",
MacroDataSource.GetDataSourceTypeNotFoundMessage("JobDeclaration"),
() => dataProvider.GetColumnValue(null, 0, "_DataSource.JobDeclaration.Consignee.OH_Code"));
		}

		public void TestGetTimeTablesOfNullAddressWontThrowException()
		{
			var wrapperCreator = ObjectFactory.Get<IDocFreightWrapperCreator>();
			var wrapper = wrapperCreator.CreateFreightWrapper(Factory.New<Forwarding.IForwardingShipment>() as BusinessObject, Factory);

			var dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(wrapper as BusinessObject)), null);
			AssertNoExceptionThrown("Exception \"Error reflecting property [get_Timetables] from parent type [Enterprise.DocumentWrappers.GenericWrappers.AddressWrapper] with property identifier [PickupCFSAddress.Timetables.Address.AdditionalInfos.OAI_AdditionalInfo] : Cannot add to a collection with a NoResultRelationship\" should not be thrown.",
				() => dataProvider.GetColumnValue(null, 0, "PickupCFSAddress.Timetables.Address.AdditionalInfos.OAI_AdditionalInfo"));
		}

		public void TestGetColumnValueShouldReturnNullWhenResultIsBusinessObjectAndItIsNull()
		{
			var topLevelDataSource = Factory.New<ExceptionThrownBusinessObject>();
			var dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(topLevelDataSource)), null);
			var columnValue = dataProvider.GetColumnValue(null, 0, "DummyExceptionThrownBusinessObject.Z0_Code");
			AssertNull(columnValue);
		}

		public void TestExceptionMessageShouldContainPropertyIdentifierWhenGettingColumnValue()
		{
			var topLevelDataSource = Factory.New<ExceptionThrownBusinessObject>();
			var dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(topLevelDataSource)), null);
			AssertExceptionThrown<InvalidOperationException>("Exception message should contain whole property identifier",
				"Error reflecting property [get_Z0_Description] from parent type [Enterprise.DocumentEngine.DataProviders.Testing.BusinessObjectDataProviderTest+ExceptionThrownBusinessObject] with property identifier [DummyNormalExceptionThrownBusinessObject.Z0_Description] : Operation is not valid due to the current state of the object.",
				() => dataProvider.GetColumnValue(null, 0, "DummyNormalExceptionThrownBusinessObject.Z0_Description"));
		}

		class ExceptionThrownBusinessObject : DummyBusinessObject
		{
			public ExceptionThrownBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ExceptionThrownBusinessObject DummyExceptionThrownBusinessObject => Factory.GetNull<ExceptionThrownBusinessObject>();

			public ExceptionThrownBusinessObject DummyNormalExceptionThrownBusinessObject
			{
				get
				{
					var result = Factory.New<ExceptionThrownBusinessObject>();
					result.throwException = true;
					return result;
				}
			}

			public override ZString Z0_Code
			{
				get
				{
					if (this == null)
					{
						throw new InvalidOperationException();
					}
					return base.Z0_Code;
				}
				set => base.Z0_Code = value;
			}

			bool throwException;
			public override ZString Z0_Description
			{
				get
				{
					if (throwException)
					{
						throw new InvalidOperationException();
					}

					return base.Z0_Description;
				}
				set => base.Z0_Description = value;
			}
		}

		public void TestGetColumnValueForBodyDataSourceReturnsZBoolWhenOverriddenWithBool()
		{
			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();
			topLevelDataSource.Z0_Bool = ZBool.True;

			VisualiserDataSet dataSet = new VisualiserDataSet();
			dataSet.Tables.Add("Collection");
			dataSet.Tables["Collection"].Columns.Add("Z0_Bool", typeof(bool));
			dataSet.Tables["Collection"].Rows.Add(true);

			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(topLevelDataSource)), dataSet);
			VisualiserDataSource dataSource = new VisualiserDataSource(dataSet, "Collection");

			object columnValue = dataProvider.GetColumnValue(dataSource, 0, "Z0_Bool");
			AssertEquals("Pre-condition: Value should be a ZBool.", typeof(ZBool), columnValue.GetType());
			AssertEquals("Pre-condition: Value should be ZBool.True.", ZBool.True, columnValue);
		}

		public void TestInvalidDataRowColumnOnSubDocWrapper()
		{
			string field = "SingleChild.StringArray";
			string tableIdentifier = VisualiserDataSet.GetTableName(field);

			VisualiserDataSet overridingDataSet = new VisualiserDataSet();
			DataTable overridingDataTable = overridingDataSet.Tables.Add(tableIdentifier);

			DataRow row = overridingDataTable.NewRow();
			overridingDataTable.Rows.Add(row);

			DocumentWrapperForTesting topLevelBO = new DocumentWrapperForTesting("Blaticus");
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(topLevelBO)), overridingDataSet);

			IDataRowSource dataRowSource = null;
			AssertExceptionThrown(typeof(FieldNotFoundException), () => dataRowSource = dataProvider.GetDataRowSource(field));
		}

		public void TestDeserialisingZStringArrayOnSubDocWrapper()
		{
			string field = "SingleChild.StringArray";
			string tableIdentifier = VisualiserDataSet.GetTableName(field);
			string columnIdentifier = VisualiserDataSet.GetColumnName(field, "SingleChild");

			VisualiserDataSet overridingDataSet = new VisualiserDataSet();
			DataTable overridingDataTable = overridingDataSet.Tables.Add(tableIdentifier);
			overridingDataTable.Columns.Add(columnIdentifier);

			DataRow row = overridingDataTable.NewRow();
			row[columnIdentifier] = "Blah";
			overridingDataTable.Rows.Add(row);

			DocumentWrapperForTesting topLevelBO = new DocumentWrapperForTesting("Blaticus");
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(topLevelBO)), overridingDataSet);

			IDataRowSource dataRowSource = null;
			AssertNoExceptionThrown(() => dataRowSource = dataProvider.GetDataRowSource(field));
			AssertEquals("Blah", dataProvider.GetColumnValue(dataRowSource, 0, field).ToString());
		}

		public void TestGetDataRowSourceWithDummyCollection()
		{
			VisualiserDataSet overridingDataSet = new VisualiserDataSet();
			DataTable overridingDataTable = overridingDataSet.Tables.Add(OneRowDataSource.TableIdentifier);
			DocumentWrapperForTesting topLevelBO = new DocumentWrapperForTesting("Blaticus");
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(topLevelBO)), overridingDataSet);
			Assert("Table: " + OneRowDataSource.TableIdentifier + " Should give a IDataRowSource of type: [" + typeof(OneRowDataSource).ToString() + "]", dataProvider.GetDataRowSource(OneRowDataSource.TableIdentifier).GetType() == typeof(OneRowDataSource));
		}

		public void TestGetDataSource()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocumentWrapperForTesting("Main")), new VisualiserDataSet());
			IDataRowSource children = dataProvider.GetDataRowSource("Children");
			AssertEquals(3, children.RowCount);
			children = dataProvider.GetDataRowSource("Children", false, -1);
			AssertEquals(3, children.RowCount);
		}

		public void TestGetDataSourceWithStringArray()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocumentWrapperForTesting("Main")), new VisualiserDataSet());
			IDataRowSource children = dataProvider.GetDataRowSource("StringArray");
			AssertEquals(4, children.RowCount);
		}

		public void TestGetNonExistingDataSource()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocumentWrapperForTesting("Main")), new VisualiserDataSet());
			AssertExceptionThrown(typeof(FieldNotFoundException), () => dataProvider.GetDataRowSource("NonExistingChild"));
		}

		public void TestGetColumnValue()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocumentWrapperForTesting("Main")), new VisualiserDataSet());
			IDataRowSource children = dataProvider.GetDataRowSource("Children");
			AssertEquals("Child 0", dataProvider.GetColumnValue(children, 0, "Children.TestString"));
			AssertEquals("Child 1", dataProvider.GetColumnValue(children, 1, "Children.TestString"));
			AssertEquals("Child 2", dataProvider.GetColumnValue(children, 2, "Children.TestString"));
			AssertEquals("Simple", dataProvider.GetColumnValue(null, 0, "SimpleClass.ActualValue"));

			AssertEquals("Child 0", dataProvider.GetColumnValue(null, 0, "Children[1].TestString"));
			AssertEquals("Child 1", dataProvider.GetColumnValue(null, 0, "Children[2].TestString"));
			AssertEquals("Child 2", dataProvider.GetColumnValue(null, 0, "Children[3].TestString"));

			AssertEquals("TestBusinessObject", dataProvider.GetColumnValue(null, 0, "Children[2]"));
		}

		public void TestGetColumnValueOnChildCollectionWhereCollectionHasNoFactoryOnlyConstructor()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(new BusinessObjectForTesting("Main"))), new VisualiserDataSet());
			IDataRowSource children = dataProvider.GetDataRowSource("CollectionWithNoFactoryOnlyConstructor");
			AssertEquals("Child 0", dataProvider.GetColumnValue(children, 0, "CollectionWithNoFactoryOnlyConstructor.TestString"));
			AssertEquals("Child 1", dataProvider.GetColumnValue(children, 1, "CollectionWithNoFactoryOnlyConstructor.TestString"));
			AssertEquals("Child 2", dataProvider.GetColumnValue(children, 2, "CollectionWithNoFactoryOnlyConstructor.TestString"));
			AssertEquals(null, dataProvider.GetColumnValue(children, 3, "CollectionWithNoFactoryOnlyConstructor.TestString"));

			AssertEquals("Child 0", dataProvider.GetColumnValue(null, 0, "CollectionWithNoFactoryOnlyConstructor[1].TestString"));
			AssertEquals("Child 1", dataProvider.GetColumnValue(null, 0, "CollectionWithNoFactoryOnlyConstructor[2].TestString"));
			AssertEquals("Child 2", dataProvider.GetColumnValue(null, 0, "CollectionWithNoFactoryOnlyConstructor[3].TestString"));
			AssertEquals(null, dataProvider.GetColumnValue(null, 0, "CollectionWithNoFactoryOnlyConstructor[4].TestString"));

			AssertEquals("Child 1", dataProvider.GetColumnValue(null, 0, "CollectionWithNoFactoryOnlyConstructor[2]"));
		}

		public void TestGetColumnValueWithOverride()
		{
			VisualiserDataSet overridingDS = new VisualiserDataSet();
			overridingDS.MainTable.Columns.Add("SimpleClassActualValue");
			overridingDS.MainRow["SimpleClassActualValue"] = "Not so simple!";
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocumentWrapperForTesting("Main")), overridingDS);
			AssertEquals("Not so simple!", dataProvider.GetColumnValue(null, 0, "SimpleClass.ActualValue"));
		}

		public void TestGetColumnValueFromCollectionWithOverride()
		{
			VisualiserDataSet overridingDS = new VisualiserDataSet();
			string tblName = VisualiserDataSet.GetTableName("Children");
			overridingDS.Tables.Add(tblName);
			overridingDS.Tables[tblName].Columns.Add("TestString");
			overridingDS.Tables[tblName].Rows.Add(new object[] { "Child Override 0" });
			overridingDS.Tables[tblName].Rows.Add(new object[] { "Child Override 1" });
			overridingDS.Tables[tblName].Rows.Add(new object[] { "Child Override 2" });

			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocumentWrapperForTesting("Main")), overridingDS);
			IDataRowSource children = dataProvider.GetDataRowSource("Children");
			AssertEquals("Child Override 0", dataProvider.GetColumnValue(children, 0, "Children.TestString"));
			AssertEquals("Child Override 1", dataProvider.GetColumnValue(children, 1, "Children.TestString"));
			AssertEquals("Child Override 2", dataProvider.GetColumnValue(children, 2, "Children.TestString"));
		}

		public void TestGetColumnValueUsingFormatString()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new PlainVanillaClass()), new VisualiserDataSet());
			AssertEquals("Foobar.Format(\"{Something}\")", "Hi", dataProvider.GetColumnValue(null, 0, "Foobar.Format(\"{Something}\")"));
			AssertEquals("Foobar.Foobar.Format(\"{Something}\")", null, dataProvider.GetColumnValue(null, 0, "Foobar.Foobar.Format(\"{Something}\")"));

			AssertEquals("Foobar.Something.Format(\"{Something}\") - Format on a type that doesn't support it should be ignored."
				, "Hi", dataProvider.GetColumnValue(null, 0, "Foobar.Format(\"{Something}\")"));

			AssertEquals("Foobar.Format(\"\")"
				, "", dataProvider.GetColumnValue(null, 0, "Foobar.Format(\"\")"));

			AssertEquals("Foobar.Something.Format(\"\") - Format on a type that doesn't support it should be ignored."
				, "Hi", dataProvider.GetColumnValue(null, 0, "Foobar.Format(\"{Something}\")"));
		}

		public void TestExceptionIsThrownWhenColumnIsInvalid()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocumentWrapperForTesting("Main")), new VisualiserDataSet());
			IDataRowSource children = dataProvider.GetDataRowSource("Children");
			AssertExceptionThrown(typeof(FieldNotFoundException), () => dataProvider.GetColumnValue(children, 0, "Children.Blah"));
		}

		public void TestGetNullCollection()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocumentWrapperForTesting("Main")), new VisualiserDataSet());
			IDataRowSource children = dataProvider.GetDataRowSource("NullChild");
			AssertEquals(0, children.RowCount);
		}

		public void TestGetColumnValueIndexOutOfRange()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocumentWrapperForTesting("Main")), new VisualiserDataSet());
			IDataRowSource emptyCollection = dataProvider.GetDataRowSource("EmptyCollection");
			AssertEquals("Should return null if index is out of range", null, dataProvider.GetColumnValue(emptyCollection, 0, "EmptyCollection.TestString"));
		}

		public void TestGetColumnValueFromStringArray()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocumentWrapperForTesting("Main")), new VisualiserDataSet());
			IDataRowSource children = dataProvider.GetDataRowSource("StringArray");
			AssertEquals("str1", dataProvider.GetColumnValue(children, 0, "StringArray.Text"));
			AssertEquals("str2", dataProvider.GetColumnValue(children, 1, "StringArray.Text"));
			AssertEquals("str3", dataProvider.GetColumnValue(children, 2, "StringArray.Text"));
			AssertEquals("str4", dataProvider.GetColumnValue(children, 3, "StringArray.Text"));
		}

		public void TestGetColumnValueFromMultipleSources()
		{
			var source1 = new Source1();
			var source2 = new Source2();

			DataProviderList providers = new DataProviderList(source1, source2);
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(providers, new VisualiserDataSet());

			AssertEquals("This is property one.", dataProvider.GetColumnValue(null, 0, "PropertyOne"));
			AssertEquals("This is property two.", dataProvider.GetColumnValue(null, 0, "PropertyTwo"));
			AssertExceptionThrown(typeof(FieldNotFoundException), () => dataProvider.GetColumnValue(null, 0, "Test"));
		}

		public void TestGetColumnValue_WithPropertySetOnAllSources_ReturnsFirstValue()
		{
			// Arrange
			var expectedResult = string.Empty;

			var source1 = Factory.New<DummyBusinessObject>();
			source1.Z0_Description = expectedResult;

			var source2 = Factory.New<DummyBusinessObject>();
			source2.Z0_Description = "123";

			var providers = new DataProviderList(BODocDataProvider.Get(source1), BODocDataProvider.Get(source2));
			var dataProvider = new BusinessObjectDataProvider(providers, new VisualiserDataSet());

			// Act
			var result = dataProvider.GetColumnValue(null, 0, nameof(DummyBusinessObject.Z0_Description));

			// Assert
			AssertEquals(expectedResult, result);
		}

		public void TestGetColumnValue_WithMultipleSourcesSetCustomFieldOnLastSource_ReturnsLastValue()
		{
			// Arrange
			var expectedResult = new ZInt(123);
			var booking = Factory.New<Enterprise.Integration.TransportBooking.IDtbBookingConsolidation>() as BusinessObject;

			var shipment = Factory.New<Forwarding.IForwardingShipment>() as BusinessObject;
			shipment.SetUserDefinedValue("Field1", expectedResult);

			var providers = new DataProviderList(BODocDataProvider.Get(booking), BODocDataProvider.Get(shipment));
			var dataProvider = new BusinessObjectDataProvider(providers, new VisualiserDataSet());

			// Act
			var result = dataProvider.GetColumnValue(null, 0, "GetCustomField(Field1)");

			// Assert
			AssertEquals(expectedResult, result);
		}

		[ExpectNoExceptions]
		public void TestMultipleBusinessObjectsShouldNotFailFromSingleProvider()
		{
			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var note = Factory.New<StmNote>();
			note.ST_Description = "Special Instructions";
			note.ST_NoteText = "It's me Luigi";
			shipment.GetNotes().Add(note);

			var freightWrapper = ObjectFactory.Get<IDocFreightWrapperCreator>().CreateFreightWrapper(shipment, Factory);

			var providers = new DataProviderList(BODocDataProvider.Get((BusinessObject)freightWrapper), BODocDataProvider.Get(shipment));
			var dataProvider = new BusinessObjectDataProvider(providers, new VisualiserDataSet());

			var result = dataProvider.GetColumnValue(null, 0, @"Notes.Find(\""{Description}\""==\""Special Instructions\"").Text");

			AssertEquals(note.ST_NoteText, result);
		}

		public void TestMultipleBusinessObjectsFailsReturnFirstException()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();

			var providers = new DataProviderList(BODocDataProvider.Get(consol), BODocDataProvider.Get(shipment));
			var dataProvider = new BusinessObjectDataProvider(providers, new VisualiserDataSet());

			var exceptionThrown = false;

			try
			{
				dataProvider.GetColumnValue(null, 0, @"Notes.Find(\""{Description}\""==\""Special Instructions\"").Text");
			}
			catch (DataProviderException ex)
			{
				AssertEquals(ex.ChainLinks[0].MethodInfo.DeclaringType, consol.GetType());
				exceptionThrown = true;
			}

			Assert(exceptionThrown);
		}

		class Source1 : DocumentWrapper
		{
			public ZString PropertyOne
			{
				get { return "This is property one."; }
			}
		}

		class Source2 : DocumentWrapper
		{
			public ZString PropertyTwo
			{
				get { return "This is property two."; }
			}
		}

		public void TestDoesColumnExist()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocumentWrapperForTesting("Main")), new VisualiserDataSet());
			AssertEquals(true, dataProvider.DoesColumnExist("TestString"));
			AssertEquals(false, dataProvider.DoesColumnExist("Value"));
		}

		public void TestNullInChainReturnsNull()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocFoobar()), new VisualiserDataSet());
			AssertEquals("Property on main object", "Hi", dataProvider.GetColumnValue(null, 0, "Something"));
			AssertEquals("Property on other object which is null", null, dataProvider.GetColumnValue(null, 0, "FoobarSomething"));
		}

		public void TestStringArray()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocFoobar()), new VisualiserDataSet());
			AssertEquals("Property value with Index 0", "A", dataProvider.GetColumnValue(null, 0, "Blah[0]"));
		}

		public void TestGetDataSourceWithObsoleteAttribute()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new DocumentWrapperForTesting("Main")), new VisualiserDataSet());
			AssertEquals("Property on main object", "Obsolete", dataProvider.GetColumnValue(null, 0, "ObsoleteString"));
		}

		public void TestGettingDataFromPlainClass()
		{
			BusinessObjectDataProvider dataProvider = new BusinessObjectDataProvider(new DataProviderList(new PlainVanillaClass()), new VisualiserDataSet());
			AssertEquals("Property on main object", "Hi, this is the plain one.", dataProvider.GetColumnValue(null, 0, "Something"));
			AssertEquals("Property on main object", "Hi", dataProvider.GetColumnValue(null, 0, "Foobar.Something"));
			AssertEquals("Property on other object which is null", null, dataProvider.GetColumnValue(null, 0, "FoobarFoobarSomething"));
		}

		class ConcreteNonPersistentBusinessObject : NonPersistentBusinessObject
		{ }

		class PlainVanillaClass : IBODocDataProvider
		{
			public ZString Something
			{
				get
				{
					return "Hi, this is the plain one.";
				}
			}

			public DocFoobar Foobar
			{
				get
				{
					return new DocFoobar();
				}
			}

			#region IBODocDataProvider Members

			DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo
			{
				get { return null; }
			}

			BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst
			{
				get { return null; }
			}

			ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue)
			{
				return ZString.Empty;
			}

			string[] IBODocDataProvider.ImageNamesToRemove
			{
				get { return null; }
			}

			BusinessObject IBODocDataProvider.ParentBusinessObject
			{
				get { return null; }
			}

			void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants)
			{
			}

			IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName)
			{
				return ZString.Empty;
			}

			string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName)
			{
				return string.Empty;
			}

			ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode)
			{
				return ZDateTime.Empty;
			}

			#endregion
		}

		class DocFoobar : DocumentWrapper
		{
			public DocFoobar()
				: base(new ConcreteNonPersistentBusinessObject(), new BusinessObjectFactory())
			{ }

			public override string ToString()
			{
				return Something;
			}

			public ZString Something
			{
				get
				{
					return "Hi";
				}
			}

			public DocFoobar Foobar
			{
				get
				{
					return null;
				}
			}

			public ZString[] Blah => new ZString[] { "A", "B", "C" };
		}

		[ExpectNoExceptions]
		public void TestGetColumnValueOfStringArrayDataSource_RespectsArraySize()
		{
			var dataProvider = new BusinessObjectDataProvider(new DataProviderList(new PlainVanillaClass()), null);
			var bodySectionDataSource = new ZStringArrayDataSource("Something", new ZString[] { "One", "Two", "Three" });
			dataProvider.GetColumnValue(bodySectionDataSource, 3, "Something");
		}

		public void TestGetColumnValueWithOnlyForCurrentSource()
		{
			var topLevelDataSource = Factory.New<DummyBusinessObject>();
			topLevelDataSource.Z0_Bool = ZBool.True;

			var dataSet = new VisualiserDataSet();
			dataSet.Tables.Add("Collection");
			dataSet.Tables["Collection"].Columns.Add("Z1_Bool", typeof(bool));
			dataSet.Tables["Collection"].Rows.Add(true);

			var dataProvider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(topLevelDataSource)), dataSet);
			var dataSource = new VisualiserDataSource(dataSet, "Collection");

			var columnValue = dataProvider.GetColumnValue(dataSource, 0, "Z0_Bool");
			AssertEquals("Value should be ZBool.True.", ZBool.True, columnValue);

			columnValue = dataProvider.GetColumnValue(dataSource, 0, "Z0_Bool", true);
			AssertNull("Value should be null", columnValue);
		}
	}
}
