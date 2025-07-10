using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.TestDataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlWriting.Testing
{
	class XmlWriterTest : TestCaseWithFactory
	{
		public void TestCollectionContentAttibute()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var shipment = new UberShipment();
				shipment.ZZZMandatoryField = 0;
				shipment.DateCollection = new UberList<UberDate>() { Style = CollectionStyle.Retro };
				shipment.DateCollection.Add(new UberDate() { Type = UberDateType.Departure });

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UberShipment", UberShipmentWithCollectionContentAttibute.Trim(), result);
					}
				}
			}
		}

		#region UberShipmentWithCollectionContentAttibute

		const string UberShipmentWithCollectionContentAttibute = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <UberShipment>

    <ZZZMandatoryField>0</ZZZMandatoryField>

    <DateCollection Style=""Retro"">
      <Date>
        <Type>Departure</Type>
      </Date>
    </DateCollection>
  </UberShipment>
</UbiquitousShipment>
";

		#endregion

		public void TestWriteDataContext_2012_11UsingOptionalNamespaceParameter()
		{
			var dataContext = GetFilledInDataContext(UniversalXmlInfo.Namespace_2012_11);

			using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
			{
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipment.DataContext = dataContext;

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream, UniversalXmlInfo.Namespace_2012_11);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UniversalShipment", UniversalShipmentWithDataContext_2012_11.Trim(), result);
					}
				}
			}
		}

		public void TestWriteDataContext_2012_11UsingSetNamespaceForTesting()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipment.DataContext = GetFilledInDataContext();

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UniversalShipment", UniversalShipmentWithDataContext_2012_11.Trim(), result);
					}
				}
			}
		}

		#region UniversalShipmentWithDataContext_2012_11

		internal const string UniversalShipmentWithDataContext_2012_11 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>JOB01010101</Key>
        <Type>DummyBusinessObject</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>TEST DOCUMENT</DocumentName>
        <IsSystemDefined>false</IsSystemDefined>
        <Purpose Description=""Test Description"">TES</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""My Action"">ACT</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""Uber branch"">UBR</EventBranch>
        <EventDepartment Name=""Uber department"">TDE</EventDepartment>
        <EventReference>TriggerRef</EventReference>
        <EventType Description=""My Event"">EVT</EventType>
        <EventUser Name=""Awesome Top Fella"">ATF</EventUser>
        <TriggerCount>2</TriggerCount>
        <TriggerDate>2012-04-13T00:00:00.000+10:00</TriggerDate>
        <TriggerDescription>Describe me a Trigger</TriggerDescription>
        <TriggerReference>*TriggerRef*</TriggerReference>
        <TriggerType>Manual</TriggerType>
        <RecipientRoleCollection>
          <RecipientRole Description=""Broker"">BRO</RecipientRole>
        </RecipientRoleCollection>
      </Workflow>
    </DataContext>
  </Shipment>
</UniversalShipment>
";

		#endregion

		public void TestWriteDataContext_2011_11UsingOptionalNamespaceParameter()
		{
			var dataContext = GetFilledInDataContext(UniversalXmlInfo.Namespace_2011_11);

			using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
			{
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipment.DataContext = dataContext;

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream, UniversalXmlInfo.Namespace_2011_11);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UniversalShipment", UniversalShipmentWithDataContext_2011_11.Trim(), result);
					}
				}
			}
		}

		public void TestWriteDataContext_2011_11UsingSetNamespaceForTesting()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipment.DataContext = GetFilledInDataContext();

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UniversalShipment", UniversalShipmentWithDataContext_2011_11.Trim(), result);
					}
				}
			}
		}

		#region UniversalShipmentWithDataContext_2011_11

		internal const string UniversalShipmentWithDataContext_2011_11 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>DummyBusinessObject</Type>
          <Key>JOB01010101</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>ACT</Code>
        <Description>My Action</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>TEST DOCUMENT</DocumentName>
        <IsSystemDefined>false</IsSystemDefined>
        <Purpose>
          <Code>TES</Code>
          <Description>Test Description</Description>
        </Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>UBR</Code>
        <Name>Uber branch</Name>
      </EventBranch>
      <EventDepartment>
        <Code>TDE</Code>
        <Name>Uber department</Name>
      </EventDepartment>
      <EventReference>TriggerRef</EventReference>
      <EventType>
        <Code>EVT</Code>
        <Description>My Event</Description>
      </EventType>
      <EventUser>
        <Code>ATF</Code>
        <Name>Awesome Top Fella</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>2</TriggerCount>
      <TriggerDate>2012-04-13T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Describe me a Trigger</TriggerDescription>
      <TriggerReference>*TriggerRef*</TriggerReference>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>
";

		#endregion

		static IDataContextDataObject GetFilledInDataContext(string nameSpace = null)
		{
			var dataContext = DataContextFactory.New(nameSpace ?? SchemaVersionManager.Current.Namespace);
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				EventReference = "TriggerRef",
				EventType = new CodeDescriptionPair() { Code = "EVT", Description = "My Event" },
				EventUser = new Staff() { Code = "ATF", Name = "Awesome Top Fella" },
				EventBranch = new Branch() { Code = "UBR", Name = "Uber branch" },
				EventDepartment = new Department() { Code = "TDE", Name = "Uber department" },
				ActionPurpose = new CodeDescriptionPair() { Code = "ACT", Description = "My Action" },
				TriggerDescription = "Describe me a Trigger",
				TriggerCount = 2,
				TriggerDate = new ZDateTimeOffset(2012, 4, 13),
				TriggerReference = "*TriggerRef*",
				TriggerType = TriggerType.Manual,
				RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BRO } }
			});
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "JOB01010101");

			var documentName = "TEST DOCUMENT";

			var purposeList = new CodeDescriptionPairList();
			purposeList.AddPair("TES", "Test Description");

			var isSystemDefined = false;

			dataContext.SetDocumentaryOverride(documentName, "TES", purposeList, isSystemDefined, 2, 1);

			return dataContext;
		}

		public void TestWriteAttributesHandleAllTypes()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var shipment = new UberShipment();
				shipment.ZZZMandatoryField = 0;
				shipment.Pancake = new UberPancake()
				{
					Type = "BUTTER",
					Description = "Pancake with Butter",
					Calories = 122,
					CookedIn = CookerType.FryingPan,
					Diameter = 23.45m,
					RecipeWritten = new ZDateTime(2012, 1, 4),
				};

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UberShipment", UberShipmentWithAttributesOfAllTypes.Trim(), result);
					}
				}
			}
		}

		#region UberShipmentWithAttributesOfAllTypes

		const string UberShipmentWithAttributesOfAllTypes = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <UberShipment>

    <ZZZMandatoryField>0</ZZZMandatoryField>
    <Pancake Calories=""122"" CookedIn=""FryingPan"" Description=""Pancake with Butter"" Diameter=""23.45"" RecipeWritten=""2012-01-04T00:00:00"">BUTTER</Pancake>
  </UberShipment>
</UbiquitousShipment>
";

		#endregion

		public void TestWriteHasICodeNameAndICodeDescriptionStuffDoneRight_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var shipment = new UberShipment();
				shipment.JobType = new UberCodeDescriptionPair() { Code = "FOO", Description = "Foo Fighter" };
				shipment.Origin = new UberUNLOCO() { Code = "AUSYD", Name = "Syd-en-eee" };

				shipment.ZZZMandatoryField = 0;

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UberShipment", UberShipmentWithNewFormatICodeNameAndICodeDescription.Trim(), result);
					}
				}
			}
		}

		#region UberShipmentWithNewFormatICodeNameAndICodeDescription

		const string UberShipmentWithNewFormatICodeNameAndICodeDescription = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <UberShipment>

    <ZZZMandatoryField>0</ZZZMandatoryField>
    <JobType Description=""Foo Fighter"">FOO</JobType>
    <Origin Name=""Syd-en-eee"">AUSYD</Origin>
  </UberShipment>
</UbiquitousShipment>
";

		#endregion

		public void TestWriteXMLDoesNotRemoveEmptyElements()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var shipment = new UberShipment();
				shipment.ZZZMandatoryField = 2;
				shipment.DateCollection = new UberList<UberDate>();
				shipment.DateCollection.Add(new UberDate { Value = new ZDateTime() });
				shipment.DateCollection.Add(new UberDate { Value = new ZDateTime() });
				shipment.ContainerCollection = new UberList<UberContainer>();
				shipment.ContainerCollection.Add(new UberContainer() { ContainerNumber = "123", ContainerType = new UberContainerType() });
				shipment.ContainerCollection.Add(new UberContainer() { ContainerNumber = "" });

				//removeEmptyElementsRegistryItem = false;
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UberShipment", UberShipmentWithEmptyElements.Trim(), result);
					}
				}
			}
		}

		public void TestWriteXMLRemovesEmptyElements()
		{
			var shipment = new UberShipment();
			shipment.ZZZMandatoryField = 2;
			shipment.DateCollection = new UberList<UberDate>();
			shipment.DateCollection.Add(new UberDate { Value = new ZDateTime() });
			shipment.DateCollection.Add(new UberDate { Value = new ZDateTime() });
			shipment.ContainerCollection = new UberList<UberContainer>();
			shipment.ContainerCollection.Add(new UberContainer() { ContainerNumber = "123", ContainerType = new UberContainerType() });
			shipment.ContainerCollection.Add(new UberContainer() { ContainerNumber = "" });

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var writer = new XmlWriter();
				writer.RemoveEmptyElements = true;
				writer.WriteXML(shipment, stream);
				using (var reader = new StreamReader(stream))
				{
					string result = reader.ReadToEnd();
					AssertMultilineASCIIEquals("Serialized UberShipment", UberShipmentWithEmptyElementsIgnored.Trim(), result);
				}
			}
		}

		#region UberShipmentWithEmptyElementsIgnored

		const string UberShipmentWithEmptyElements = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <UberShipment>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <ContainerCollection>
      <Container>
        <ContainerNumber>123</ContainerNumber>
        <ContainerType>
        </ContainerType>
      </Container>
      <Container>
        <ContainerNumber></ContainerNumber>
      </Container>
    </ContainerCollection>
    <DateCollection>
      <Date>
        <Value></Value>
      </Date>
      <Date>
        <Value></Value>
      </Date>
    </DateCollection>
  </UberShipment>
</UbiquitousShipment>";

		const string UberShipmentWithEmptyElementsIgnored = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <UberShipment>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <ContainerCollection>
      <Container>
        <ContainerNumber>123</ContainerNumber>
      </Container>
    </ContainerCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestWriteXMLWithNullElementsInList()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var shipment = new UberShipment();
				shipment.ZZZMandatoryField = 2;
				shipment.DateCollection = new UberList<UberDate>();
				shipment.DateCollection.Add(new UberDate { Value = new ZDateTime(2011, 1, 1) });
				shipment.DateCollection.Add(null);
				shipment.DateCollection.Add(null);
				shipment.DateCollection.Add(new UberDate { Value = new ZDateTime(2011, 1, 2) });

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UberShipment", UberShipmentWithNullElementsIgnored.Trim(), result);
					}
				}
			}
		}

		#region UberShipmentWithNullElementsIgnored

		const string UberShipmentWithNullElementsIgnored = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <UberShipment>

    <ZZZMandatoryField>2</ZZZMandatoryField>

    <DateCollection>
      <Date>
        <Value>2011-01-01T00:00:00</Value>
      </Date>
      <Date>
        <Value>2011-01-02T00:00:00</Value>
      </Date>
    </DateCollection>
  </UberShipment>
</UbiquitousShipment>
";

		#endregion

		public void TestWriteXMLWithDescriptionAttributes()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var shipment = new UberShipment();

				shipment.CandidateKeyField = 1001;
				shipment.ReferenceData = "MyReference";
				shipment.ZZZMandatoryField = 2;
				shipment.PackageCount = 10;
				shipment.UberMediumField = new UberCodeDescriptionPairAlwaysAttributes() { Code = "HELLO HELLO HELLO", Description = @"He <can't> &stop saying ""hello""! This description is pointless, but it must be here." };

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UberShipment", UberShipmentWithDescriptionAttributeApplied.Trim(), result);
					}
				}
			}
		}

		#region UberShipmentWithDescriptionAttributeApplied

		const string UberShipmentWithDescriptionAttributeApplied = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <UberShipment>
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <PackageCount>10</PackageCount>
    <UberMediumField Description=""He &lt;can't&gt; &amp;stop saying &quot;hello&quot;! This description "">HELLO HELLO HELLO</UberMediumField>
  </UberShipment>
</UbiquitousShipment>
";

		#endregion

		public void TestWriteXMLWithCultureInvariantDecimals()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var currentCulture = Thread.CurrentThread.CurrentCulture;
				var currentUICulture = Thread.CurrentThread.CurrentUICulture;

				try
				{
					Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
					Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");

					var shipment = new UberShipment();
					shipment.UberDecimal = 43342.332m;

					using (var stream = (SubStreamableStream)new MemoryStream())
					{
						ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

						using (var reader = new StreamReader(stream))
						{
							string result = reader.ReadToEnd();
							AssertMultilineASCIIEquals("Serialized UberShipment", @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <UberShipment>

    <UberDecimal>43342.332</UberDecimal>
  </UberShipment>
</UbiquitousShipment>
					".Trim(), result);
						}
					}
				}
				finally
				{
					Thread.CurrentThread.CurrentCulture = currentCulture;
					Thread.CurrentThread.CurrentUICulture = currentUICulture;
				}
			}
		}

		public void TestWriteXML()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var shipment = new UberShipment();

				shipment.CandidateKeyField = 1001;
				shipment.ReferenceData = "MyReference";
				shipment.ZZZMandatoryField = 2;

				shipment.UberBigField = @"Some really big block of text.
2 > 1 & 3 < 4
I mean it should have at least 3 or 4 lines.
Enough to know better.";
				shipment.UberBoolean = true;
				shipment.UberByteField = 14;
				shipment.UberDate = new ZDate(2017, 11, 27);
				shipment.UberDateTime = new ZDateTime(2010, 2, 12);
				shipment.UberDecimal = 12.23;
				shipment.UberInteger = 44;
				shipment.UberLongInt = 99;
				shipment.UberMediumField = new UberCodeDescriptionPairAlwaysAttributes() { Code = "A bit longer, but not too long." };
				shipment.UberShortIntField = 9;
				shipment.UberSmallField = "SHORTFIELDTRUNCATED";
				shipment.UberDuration = new TimeSpan(3, 2, 1);

				shipment.Origin = new UberUNLOCO() { Code = "AUSYD", Name = "Syd-er-nee" };
				shipment.Destination = new UberUNLOCO() { Code = "NZAKL", Name = "Doorklund" };

				shipment.PackageCount = 12;
				shipment.PackageUnit = new UberPackingUnit() { Code = "PK" };

				shipment.Weight = 44;
				shipment.WeightUnit = new UberUnitOfWeight() { Code = "KG", Description = "Kilograms" };

				shipment.Volume = 33;
				shipment.VolumeUnit = new UberUnitOfVolume() { Code = "M3", Description = "Cubic Meters" };

				shipment.InnerRelation = new UberInnerRelatedObject() { ExtraBoolean = false, ExtraDecimal = 1.224, ExtraField = "hOOters" };

				var dates = shipment.DateCollection = new UberList<UberDate>() { Style = CollectionStyle.Classic };
				dates.Add(new UberDate() { Type = UberDateType.Pickup, IsEstimate = false, Value = new ZDateTime(2010, 2, 1) });
				dates.Add(new UberDate() { Type = UberDateType.Departure, Value = new ZDateTime(2010, 2, 4) });
				dates.Add(new UberDate() { Type = UberDateType.Pack, IsEstimate = true, Value = ZDateTime.Empty });

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UberShipment", TargetUberShipmentXML.Trim(), result);
					}
				}
			}
		}

		#region TargetUberShipmentXML

		const string TargetUberShipmentXML = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <UberShipment>
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination>
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin>
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>
    <UberBigField>Some really big block of text.
2 &gt; 1 &amp; 3 &lt; 4
I mean it should have at least 3 or 4 lines.
Enough to know better.</UberBigField>
    <UberBoolean>true</UberBoolean>
    <UberByteField>14</UberByteField>
    <UberDate>2017-11-27</UberDate>
    <UberDateTime>2010-02-12T00:00:00</UberDateTime>
    <UberDecimal>12.23</UberDecimal>
    <UberDuration>PT3H2M1S</UberDuration>
    <UberInteger>44</UberInteger>
    <UberLongInt>99</UberLongInt>
    <UberMediumField>A bit longer, but not too long.</UberMediumField>
    <UberShortIntField>9</UberShortIntField>
    <UberSmallField>SHORTFIELD</UberSmallField>
    <Volume>33</Volume>
    <VolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </VolumeUnit>
    <Weight>44</Weight>
    <WeightUnit Description=""Kilograms"">KG</WeightUnit>

    <InnerRelation>
      <ExtraBoolean>false</ExtraBoolean>
      <ExtraDecimal>1.224</ExtraDecimal>
      <ExtraField>hOOters</ExtraField>
    </InnerRelation>

    <DateCollection Style=""Classic"">
      <Date>
        <Type>Pickup</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2010-02-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <Value>2010-02-04T00:00:00</Value>
      </Date>
      <Date>
        <Type>Pack</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>
  </UberShipment>
</UbiquitousShipment>
    ";

		#endregion

		public void TestWriteElementRemoveInvalidXml()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipment.DataContext = GetFilledInDataContext();

				shipment.QuoteNumber = "Contains\x1DInvalid";

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertContains("Should contain element without invalid character", "<QuoteNumber>ContainsInvalid</QuoteNumber>", result);
					}
				}
			}
		}

		#region Writing with Override

		#region DummyOverrideProvider

		sealed class DummyOverrideProvider : IDataOverrideProvider
		{
			readonly Dictionary<IDataObject, DataObjectState> dataObjectStates = new Dictionary<IDataObject, DataObjectState>();
			readonly Dictionary<IDataObject, IList<IPropertyOverride>> propertyOverrides = new Dictionary<IDataObject, IList<IPropertyOverride>>();

			public void SetDataObjectState(IDataObject dataObject, DataObjectState state)
			{
				dataObjectStates[dataObject] = state;
			}

			public DataObjectState GetDataObjectState(IDataObject dataObject)
			{
				DataObjectState result;

				return dataObjectStates.TryGetValue(dataObject, out result)
					? result
					: DataObjectState.Default;
			}

			public void AddPropertyOverride(IDataObject dataObject, string name, IZType value)
			{
				IList<IPropertyOverride> overrides;

				if (!propertyOverrides.TryGetValue(dataObject, out overrides))
				{
					overrides = new List<IPropertyOverride>();
					propertyOverrides[dataObject] = overrides;
				}

				overrides.Add(new PropertyOverride
				{
					Name = name,
					Value = value
				});
			}

			public IEnumerable<IPropertyOverride> GetPropertyOverrides(IDataObject dataObject)
			{
				IList<IPropertyOverride> overrides;

				return propertyOverrides.TryGetValue(dataObject, out overrides)
					? overrides
					: Enumerable.Empty<IPropertyOverride>();
			}
		}

		sealed class PropertyOverride : IPropertyOverride
		{
			public string Name { get; set; }
			public IZType Value { get; set; }
		}

		#endregion

		void AssertXml(string expectedXml, IDataObject shipment, IDataOverrideProvider overrideProvider, string ns = UniversalXmlInfo.Namespace_2011_11)
		{
			using (SchemaVersionManager.SetNamespaceForTesting(ns))
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream, null, overrideProvider);

				using (var reader = new StreamReader(stream))
				{
					string actualXml = reader.ReadToEnd();
					AssertMultilineASCIIEquals("", expectedXml, actualXml);
				}
			}
		}

		public void TestSimpleOverride()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.PortOfDischarge = new UNLOCO { Code = "USLAX", Name = "Los Angeles" };
			shipment.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			shipment.PortOfOrigin = new UNLOCO { Code = "GBLON", Name = "London" };

			var overrideProvider = new DummyOverrideProvider();
			overrideProvider.AddPropertyOverride(shipment.PortOfLoading, nameof(shipment.PortOfLoading.Code), new ZString("AUXXX"));
			overrideProvider.AddPropertyOverride(shipment.PortOfLoading, nameof(shipment.PortOfLoading.Name), new ZString("X Land"));
			overrideProvider.AddPropertyOverride(shipment.PortOfOrigin, nameof(shipment.PortOfLoading.Name), new ZString("Super London"));

			const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>

    <PortOfDischarge>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
      <DocumentaryOverride>
        <Code>AUXXX</Code>
        <Name>X Land</Name>
      </DocumentaryOverride>
    </PortOfLoading>
    <PortOfOrigin>
      <Code>GBLON</Code>
      <Name>London</Name>
      <DocumentaryOverride>
        <Name>Super London</Name>
      </DocumentaryOverride>
    </PortOfOrigin>
  </Shipment>
</UniversalShipment>";

			AssertXml(expectedXml, shipment, overrideProvider);

			const string expectedXmlVersion2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>

    <PortOfDischarge Name=""Los Angeles"">USLAX</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <PortOfOrigin Name=""London"">GBLON</PortOfOrigin>
    <DocumentaryOverride>
      <PortOfLoading Name=""X Land"">AUXXX</PortOfLoading>
      <PortOfOrigin Name=""Super London"">GBLON</PortOfOrigin>
    </DocumentaryOverride>
  </Shipment>
</UniversalShipment>";

			AssertXml(expectedXmlVersion2, shipment, overrideProvider, UniversalXmlInfo.Namespace_2012_11);
		}

		public void TestComplexOverride()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.JobCosting.Currency = new Currency();
			shipment.JobCosting.Currency.Code = "GCS";
			shipment.JobCosting.Currency.Description = "Galactic Credit Standard";

			var overrideProvider = new DummyOverrideProvider();
			overrideProvider.SetDataObjectState(shipment.JobCosting.Currency, DataObjectState.Added);

			const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>

    <JobCosting>
      <Currency>
        <DocumentaryOverride Type=""Addition"">
          <Code>GCS</Code>
          <Description>Galactic Credit Standard</Description>
        </DocumentaryOverride>
      </Currency>
    </JobCosting>
  </Shipment>
</UniversalShipment>";

			AssertXml(expectedXml, shipment, overrideProvider);

			const string expectedXmlVersion2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>

    <JobCosting>
      <Currency Description=""Galactic Credit Standard"">GCS</Currency>
    </JobCosting>
  </Shipment>
</UniversalShipment>";

			AssertXml(expectedXmlVersion2, shipment, overrideProvider, UniversalXmlInfo.Namespace_2012_11);
		}

		public void TestFlattenedOverride_DescriptionOnly()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.PortOfOrigin = new UNLOCO { Code = "SWEND", Name = "Endor" };

			var overrideProvider = new DummyOverrideProvider();
			overrideProvider.AddPropertyOverride(shipment.PortOfOrigin, nameof(shipment.PortOfOrigin.Name), new ZString("Farbenkugel"));

			const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>

    <PortOfOrigin Name=""Endor"">SWEND</PortOfOrigin>
    <DocumentaryOverride>
      <PortOfOrigin Name=""Farbenkugel"">SWEND</PortOfOrigin>
    </DocumentaryOverride>
  </Shipment>
</UniversalShipment>";

			AssertXml(expectedXml, shipment, overrideProvider, UniversalXmlInfo.Namespace_2012_11);
		}

		public void TestCustomFieldsOverride()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var overrideProvider = new DummyOverrideProvider();
			overrideProvider.AddPropertyOverride(shipment, "ABC", new ZString("Blah blah blah"));
			overrideProvider.AddPropertyOverride(shipment, "XYZ", new ZString("Rah & rah & rah"));

			const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>

    <DocumentaryOverride>
      <ABC>Blah blah blah</ABC>
      <XYZ>Rah &amp; rah &amp; rah</XYZ>
    </DocumentaryOverride>
  </Shipment>
</UniversalShipment>";

			AssertXml(expectedXml, shipment, overrideProvider);
		}

		public void TestCollectionOverride()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = AddressTypes.Importer,
				CompanyName = "Planet Express"
			};

			var removedAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = AddressTypes.ResponsibleParty,
				CompanyName = "Father Gascoigne"
			};

			var addedOrgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CompanyName = "Pretzel Wagon"
			};

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				orgAddress,
				removedAddress,
				addedOrgAddress
			});

			var overrideProvider = new DummyOverrideProvider();
			overrideProvider.AddPropertyOverride(orgAddress, nameof(orgAddress.CompanyName), new ZString("Globex Corp"));
			overrideProvider.SetDataObjectState(addedOrgAddress, DataObjectState.Added);
			overrideProvider.SetDataObjectState(removedAddress, DataObjectState.Removed);

			const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>


    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Importer</AddressType>
        <CompanyName>Planet Express</CompanyName>
        <DocumentaryOverride>
          <CompanyName>Globex Corp</CompanyName>
        </DocumentaryOverride>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ResponsibleParty</AddressType>
        <CompanyName>Father Gascoigne</CompanyName>
        <DocumentaryOverride Type=""Removed""></DocumentaryOverride>
      </OrganizationAddress>
      <OrganizationAddress>
        <DocumentaryOverride Type=""Addition"">
          <CompanyName>Pretzel Wagon</CompanyName>
        </DocumentaryOverride>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			AssertXml(expectedXml, shipment, overrideProvider);
		}

		public void TestAddedCollectionElementWithCustomizedFields()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var addedOrgAddress = new OrganizationAddress { CompanyName = "Flash Delirium" };
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				addedOrgAddress
			});

			var overrideProvider = new DummyOverrideProvider();
			overrideProvider.AddPropertyOverride(addedOrgAddress, "ABC", new ZString("Blah blah blah"));
			overrideProvider.SetDataObjectState(addedOrgAddress, DataObjectState.Added);

			const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>


    <OrganizationAddressCollection>
      <OrganizationAddress>
        <DocumentaryOverride Type=""Addition"">
          <CompanyName>Flash Delirium</CompanyName>
          <ABC>Blah blah blah</ABC>
        </DocumentaryOverride>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			AssertXml(expectedXml, shipment, overrideProvider);
		}

		#endregion

		string GetTriggerDateValue(string xml)
		{
			var doc = XDocument.Parse(xml);
			var nameSpace = doc.Root.Name.Namespace;
			return doc.Root.Element(nameSpace + "Shipment").Element(nameSpace + "DataContext").Element(nameSpace + "Workflow").Element(nameSpace + "TriggerDate").Value;
		}

		public void TestWriteXMLContainZDateTimeOffset()
		{
			var dataContext = GetFilledInDataContext(UniversalXmlInfo.Namespace_2012_11);

			using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
			{
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipment.DataContext = dataContext;

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream, UniversalXmlInfo.Namespace_2012_11);

					using (var reader = new StreamReader(stream))
					{
						var result = reader.ReadToEnd();
						AssertEquals("TriggerDate should contain the offset", "2012-04-13T00:00:00.000+10:00", GetTriggerDateValue(result));
					}
				}
			}
		}

		[TestDate(2024, 1, 5, 12, 0, 0)]
		public void TestWriteUniversalEventContainZDateTimeOffset()
		{
			UniversalEvent ev = new UniversalEvent()
			{
				EventType = AutoEvents.CustomisableEvent00.Code,
				EventTime = ZDateTimeOffset.Now,
			};
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				ObjectFactory.Get<IXmlWriter>().WriteXML(ev, stream);

				using (var reader = new StreamReader(stream))
				{
					string result = reader.ReadToEnd();
					AssertMultilineASCIIEquals("Serialized UniversalEvent", @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>

    <EventTime>2024-01-05T12:00:00.000+00:00</EventTime>
    <EventType>Z00</EventType>
  </Event>
</UniversalEvent>", result);
				}
			}
		}
	}
}

