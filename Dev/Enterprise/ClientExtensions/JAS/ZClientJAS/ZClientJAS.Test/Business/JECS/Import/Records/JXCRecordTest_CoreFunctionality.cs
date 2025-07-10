using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Utilities;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class JXCRecordTest_CoreFunctionality : JXCRecordTestCase
	{
		public void TestFields()
		{
			JXCRecord record = GetNewRecord("TEST", "THIS;IS;The;RECord;Content");
			PropertyInfo fieldsProperty = record.GetType().GetProperty("Fields", BindingFlags.NonPublic | BindingFlags.Instance);
			JASCsvLine fields = (JASCsvLine)fieldsProperty.GetValue(record, null);
			AssertEquals("THIS", fields.GetFieldValue(0));
			AssertEquals("IS", fields.GetFieldValue(1));
			AssertEquals("The", fields.GetFieldValue(2));
			AssertEquals("RECord", fields.GetFieldValue(3));
			AssertEquals("Content", fields.GetFieldValue(4));
		}

		public void TestFindOrCreateTempOrganisation()
		{
			OrgHeader insertedOrg = InsertOrganisationForTest();
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Xsd.Organisation orgValue = new Xsd.Organisation();
			orgValue.OwnerCode = "TEST";
			orgValue.OrganisationDetails.Name = "ORGNAME";
			AssertEquals(insertedOrg.OH_Code, Record.FindOrCreateTempOrganisation(orgValue, OrganisationTypes.None, dummy, new NotificationBuffer()).OH_Code);
		}

		public void TestFindOrCreateTempOrganisation_NameDoesNotContainAnyLetters()
		{
			Xsd.Organisation orgValue = new Xsd.Organisation();
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			orgValue.OwnerCode = "TEST";
			AssertNull("Should not try to match / create if organisation name does not contain any letters", Record.FindOrCreateTempOrganisation(orgValue, OrganisationTypes.None, dummy, new NotificationBuffer()));
		}

		public void TestFindOrCreateTempOrganisationPK()
		{
			OrgHeader insertedOrg = InsertOrganisationForTest();
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Xsd.Organisation orgValue = new Xsd.Organisation();
			orgValue.OwnerCode = "TEST";
			orgValue.OrganisationDetails.Name = "ORGNAME";
			AssertEquals(insertedOrg.PK, Record.FindOrCreateTempOrganisationPK(orgValue, OrganisationTypes.None, dummy, new NotificationBuffer()));
		}

		public void TestFindOrganisation()
		{
			OrgHeader insertedOrg = InsertOrganisationForTest();
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Xsd.Organisation orgValue = new Xsd.Organisation();
			orgValue.OwnerCode = "TEST";
			orgValue.OrganisationDetails.Name = "ORGNAME";
			AssertEquals(insertedOrg.OH_Code, Record.FindOrganisation(orgValue, OrganisationTypes.None, dummy, new NotificationBuffer()).OH_Code);
		}

		public void TestBaseGetOrganisationMatching()
		{
			AssertEquals(typeof(OrganisationMatching), Record.BaseGetOrganisationMatching(new BusinessObjectFactoryProvider(Factory), null).GetType());
		}

		public void TestConvertJASWeightUnit()
		{
			AssertEquals(Core.Constants.Weight.Kilograms, Record.ConvertJASWeightUnit("K"));
			AssertEquals(Core.Constants.Weight.Kilograms, Record.ConvertJASWeightUnit("k"));
			AssertEquals(Core.Constants.Weight.Pounds, Record.ConvertJASWeightUnit("L"));
			AssertEquals(Core.Constants.Weight.Pounds, Record.ConvertJASWeightUnit("l"));
		}

		public void TestGetUNLOCOFromOfficeCode()
		{
			AssertEquals("Should not match any OrgHeader", "", Record.GetUNLOCOFromOfficeCode(Factory, "AUCOR"));
			JASOrgHeader orgHeader = Factory.NewWithValidTestData<JASOrgHeader>();
			orgHeader.OfficeCode = "AUCOR";
			orgHeader.OH_RL_NKClosestPort = "ITMIL";
			Factory.Save();
			AssertEquals("ITMIL", Record.GetUNLOCOFromOfficeCode(Factory, "AUCOR"));
		}

		public void TestIsDefaultUnmatchedOrg()
		{
			JASOrgHeader orgHeader1 = Factory.New<JASOrgHeader>();
			Assert("Not a default Unmatched Organisation", !Record.IsDefaultUnmatchedOrg(orgHeader1));
			JASOrgHeader orgHeader2 = Factory.Load<JASOrgHeader>(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);
			Assert(Record.IsDefaultUnmatchedOrg(orgHeader2));
		}

		public void TestNotifyAttachingNewShipmentToConsol()
		{
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			Record.NotifyAttachingNewShipmentToConsol(consol, notificationBuffer);
			AssertEquals(1, notificationBuffer.Events.Length);
			AssertEquals("Attaching new shipment to " + consol.HumanReadableName, ((InfoNotification)notificationBuffer.Events[0]).AdditionalInfo);
		}

		[ExpectNoExceptions]
		public void TestNotifyAttachingNewShipmentToConsol_NullParam()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			Record.NotifyAttachingNewShipmentToConsol(consol, null);
		}

		public void TestNotifyAttachingExistingShipmentToConsol()
		{
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			Record.NotifyAttachingExistingShipmentToConsol(consol, shipment, notificationBuffer);
			AssertEquals(1, notificationBuffer.Events.Length);
			AssertEquals("Attaching " + shipment.HumanReadableName + " to " + consol.HumanReadableName, ((InfoNotification)notificationBuffer.Events[0]).AdditionalInfo);
		}

		[ExpectNoExceptions]
		public void TestNotifyAttachingExistingShipmentToConsol_NullParam()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			Record.NotifyAttachingExistingShipmentToConsol(consol, shipment, null);
		}

		#region Implementation
		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new JXCRecordForTest(lineType, lineContent);
		}

		JXCRecordForTest Record
		{
			get
			{
				if (fRecord == null)
				{
					fRecord = new JXCRecordForTest("", "");
				}

				return fRecord;
			}
		}

		OrgHeader InsertOrganisationForTest()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_Code = "__TEST__";
			return result;
		}

		JXCRecordForTest fRecord;
		#region JXCRecordForTest
		class JXCRecordForTest : JXCRecord
		{
			public JXCRecordForTest(ZString lineType, ZString lineContent) : base(lineType, lineContent)
			{
			}

			public new JASOrgHeader FindOrCreateTempOrganisation(Xsd.Organisation organisation, OrganisationTypes organisationType, BusinessObject sourceObject, INotifications notificationSubscriber)
			{
				return base.FindOrCreateTempOrganisation(organisation, organisationType, sourceObject, notificationSubscriber);
			}

			public new ZGuid FindOrCreateTempOrganisationPK(Xsd.Organisation organisation, OrganisationTypes organisationType, BusinessObject sourceObject, INotifications notificationSubscriber)
			{
				return base.FindOrCreateTempOrganisationPK(organisation, organisationType, sourceObject, notificationSubscriber);
			}

			public new JASOrgHeader FindOrganisation(Xsd.Organisation organisation, OrganisationTypes organisationType, BusinessObject sourceObject, INotifications notificationSubscriber)
			{
				return base.FindOrganisation(organisation, organisationType, sourceObject, notificationSubscriber);
			}

			public OrganisationMatching BaseGetOrganisationMatching(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
			{
				return base.GetOrganisationMatching(factoryProvider, notificationSubscriber);
			}

			public new ZString ConvertJASWeightUnit(ZString jASWeightUnit)
			{
				return base.ConvertJASWeightUnit(jASWeightUnit);
			}

			public new ZString GetUNLOCOFromOfficeCode(BusinessObjectFactory factory, ZString officeCode)
			{
				return base.GetUNLOCOFromOfficeCode(factory, officeCode);
			}

			public new bool IsDefaultUnmatchedOrg(JASOrgHeader orgHeader)
			{
				return base.IsDefaultUnmatchedOrg(orgHeader);
			}

			public new void NotifyAttachingNewShipmentToConsol(JASForwardingConsol consol, INotifications notificationSubscriber)
			{
				base.NotifyAttachingNewShipmentToConsol(consol, notificationSubscriber);
			}

			public new void NotifyAttachingExistingShipmentToConsol(JASForwardingConsol consol, JASForwardingShipment shipment, INotifications notificationSubscriber)
			{
				base.NotifyAttachingExistingShipmentToConsol(consol, shipment, notificationSubscriber);
			}

			protected override OrganisationMatching GetOrganisationMatching(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
			{
				return new OrganisationMatchingForTest(factoryProvider, Xsd.XmlInterchange.Empty, notificationSubscriber);
			}
		}

		#endregion
		#region OrganisationMatchingForTest
		class OrganisationMatchingForTest : OrganisationMatching
		{
			public OrganisationMatchingForTest(BusinessObjectFactoryProvider factoryProvider, Xsd.XmlInterchange interchange, INotifications notificationSubscriber) : base(factoryProvider, interchange, notificationSubscriber)
			{
			}

			public override OrgMatchingResult Match(IValueObject matchingCriteria, OrganisationTypes orgTypes, bool createTemporaryOrUnmatchOrgIfNoMatchFound)
			{
				ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, "__TEST__");
				OrgHeader orgHeader = FactoryProvider.Current.LoadTop1<OrgHeader>(filter);
				return new OrgMatchingResult(orgHeader, (orgHeader != null), false, null);
			}
		}
		#endregion
		#endregion
	}
}
