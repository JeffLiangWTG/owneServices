using System;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	sealed class ValueObjectImportContextTest : TestCaseWithDummy
	{
		#region Constructors

		public void Test2ArgConstructor()
		{
			var interchange = new Xsd.XmlInterchange();
			var notifications = new NotificationBuffer();
			OrganisationMatching organisationMatching = new TestOrganisationMatching(Factory, interchange, notifications);

			var context = new ValueObjectImportContext(Factory, interchange, notifications);
			AssertEquals("Set in the constructor", Factory, context.Factory);
			AssertNotNull("Should create a new XmlInterchange", context.Interchange);
			AssertEquals("Should default to the OrganisationMatching class", typeof(OrganisationMatching), context.OrganisationMatching.GetType());

			context.Notify(new ErrorNotification(ErrorType.Error));
			AssertEquals("Context should be set in the constructor", true, notifications.HasErrors);
		}

		public void Test3ArgConstructor()
		{
			var interchange = new Xsd.XmlInterchange();
			var notifications = new NotificationBuffer();
			OrganisationMatching organisationMatching = new TestOrganisationMatching(Factory, interchange, notifications);

			var context = new ValueObjectImportContext(Factory, interchange, notifications);
			AssertEquals("Set in the constructor", Factory, context.Factory);
			AssertEquals("Set in the constructor", interchange, context.Interchange);
			AssertEquals("Should default to the OrganisationMatching class", typeof(OrganisationMatching), context.OrganisationMatching.GetType());

			context.Notify(new ErrorNotification(ErrorType.Error));
			AssertEquals("Context should be set in the constructor", true, notifications.HasErrors);
		}

		public void Test4ArgConstructor()
		{
			var interchange = new Xsd.XmlInterchange();
			var notifications = new NotificationBuffer();
			OrganisationMatching organisationMatching = new TestOrganisationMatching(Factory, interchange, notifications);

			var context = new ValueObjectImportContext(Factory, interchange, organisationMatching, notifications);
			AssertEquals("Set in the constructor", Factory, context.Factory);
			AssertEquals("Set in the constructor", interchange, context.Interchange);
			AssertEquals("Set in the constructor", organisationMatching, context.OrganisationMatching);

			context.Notify(new ErrorNotification(ErrorType.Error));
			AssertEquals("Context should be set in the constructor", true, notifications.HasErrors);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestArgumentNullExceptionOnFactoryNull()
		{
			var interchange = new Xsd.XmlInterchange();
			var notifications = new NotificationBuffer();
			OrganisationMatching organisationMatching = new TestOrganisationMatching(Factory, interchange, notifications);
			new ValueObjectImportContext((BusinessObjectFactoryProvider)null, Xsd.XmlInterchange.Empty, new TestOrganisationMatching(Factory, interchange, notifications), new NotificationBuffer());
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestArgumentNullExceptionOnInterchangeNull()
		{
			var interchange = new Xsd.XmlInterchange();
			var notifications = new NotificationBuffer();
			OrganisationMatching organisationMatching = new TestOrganisationMatching(Factory, interchange, notifications);
			new ValueObjectImportContext(Factory, null, new TestOrganisationMatching(Factory, interchange, notifications), new NotificationBuffer());
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestArgumentNullExceptionOnOrgMatchingNull()
		{
			new ValueObjectImportContext(Factory, Xsd.XmlInterchange.Empty, null, new NotificationBuffer());
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestArgumentNullExceptionOnNotificationsNull()
		{
			var interchange = new Xsd.XmlInterchange();
			var notifications = new NotificationBuffer();
			OrganisationMatching organisationMatching = new TestOrganisationMatching(Factory, interchange, notifications);
			new ValueObjectImportContext(Factory, Xsd.XmlInterchange.Empty, new TestOrganisationMatching(Factory, interchange, notifications), null);
		}

		#endregion

		#region Current Object XML

		public void TestCurrentObjectXML()
		{
			AssertNull(ImportContext.CurrentObjectXMLUTF8);
			ImportContext.CurrentObjectXMLUTF8 = new MemoryStream(Encoding.UTF8.GetBytes("<TEST>123</TEST>"));
			AssertEquals("XML string should not change", "<TEST>123</TEST>", new StreamReader(ImportContext.CurrentObjectXMLUTF8).ReadToEnd());
			ImportContext.CurrentObjectXMLUTF8.Position = 0;
			AssertEquals("XML string should not change", "<TEST>123</TEST>", new StreamReader(ImportContext.CurrentObjectXMLUTF8).ReadToEnd());
			ImportContext.CurrentObjectXMLUTF8 = new MemoryStream(Encoding.UTF8.GetBytes("<TEST>321</TEST>"));
			AssertEquals("XML string should not change", "<TEST>321</TEST>", new StreamReader(ImportContext.CurrentObjectXMLUTF8).ReadToEnd());
		}

		#endregion

		#region EDI Interchange

		public void TestInterchange()
		{
			AssertNull(ImportContext.EDIInterchange);
			ImportContext.EDIInterchange = Factory.New<EDIInterchange>();
			EDIInterchange cashedValue = ImportContext.EDIInterchange;
			AssertEquals("Should reference the same object", cashedValue, ImportContext.EDIInterchange);
		}

		#endregion

		#region String to Field Converter

		public void TestSetPropertyInfoValue()
		{
			ImportContext.SetPropertyInfoValue(Dummy.Z0_VarCharMaxInfo, "Text", true);
			AssertEquals("Text", Dummy.Z0_VarCharMax);

			// ImportXmlElementOnlyIfSpecified registry is TRUE by default 
			ImportContext.SetPropertyInfoValue(Dummy.Z0_VarCharMaxInfo, "", false);
			AssertEquals("Text", Dummy.Z0_VarCharMax);

			ImportContext.SetPropertyInfoValue(Dummy.Z0_VarCharMaxInfo, "", true);
			AssertEquals("", Dummy.Z0_VarCharMax);

			ImportContext.SetPropertyInfoValue(Dummy.Z0_VarCharMaxInfo, "AnotherText", true);
			AssertEquals("AnotherText", Dummy.Z0_VarCharMax);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			ImportContext.SetPropertyInfoValue(Dummy.Z0_VarCharMaxInfo, "", false);
			AssertEquals("", Dummy.Z0_VarCharMax);
		}

		public void TestSetPropertyInfoValueIfNotEmpty()
		{
			Dummy.Z0_VarCharMax = "";
			ImportContext.SetPropertyInfoValueIfValueNotEmpty(Dummy.Z0_VarCharMaxInfo, "Text");
			AssertEquals("Text", Dummy.Z0_VarCharMax);

			ImportContext.SetPropertyInfoValueIfValueNotEmpty(Dummy.Z0_VarCharMaxInfo, "");
			AssertEquals("Doesn't change as value is empty", "Text", Dummy.Z0_VarCharMax);
		}

		public void TestSetPropertyInfoValueIfNotEmpty_WithCodeMapping()
		{
			var mappingOrg = Factory.Load<OrgHeader>(ImportContext.Converter.MappingOrgPK);

			var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery());

			OrgPatternMatchOverride vesselOverride = mappingOrg.CreatePatternMatchOverrideForTest();
			vesselOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			vesselOverride.OO_LocalGuid = port.PK;
			vesselOverride.OO_ForeignCode = "Blaticus";

			Factory.Save();

			Dummy.Z0_VarCharMax = "XXX";
			ImportContext.SetPropertyInfoValueIfValueNotEmpty(Dummy.Z0_VarCharMaxInfo, "Blaticus", ForeignKeyType.PortNK);
			AssertEquals("Should map the port code correctly", port.RL_Code, Dummy.Z0_VarCharMax);

			Dummy.Z0_VarCharMax = "XXX";
			ImportContext.SetPropertyInfoValueIfValueNotEmpty(Dummy.Z0_VarCharMaxInfo, "Blaticus", ForeignKeyType.PortNK, "");
			AssertEquals("Should map the port code correctly", port.RL_Code, Dummy.Z0_VarCharMax);

			Dummy.Z0_VarCharMax = "XXX";
			ImportContext.SetPropertyInfoValueIfValueNotEmpty(Dummy.Z0_VarCharMaxInfo, "", ForeignKeyType.PortNK);
			AssertEquals("Do nothing if empty", "XXX", Dummy.Z0_VarCharMax);

			Dummy.Z0_VarCharMax = "XXX";
			ImportContext.SetPropertyInfoValueIfValueNotEmpty(Dummy.Z0_VarCharMaxInfo, "", ForeignKeyType.PortNK, "");
			AssertEquals("Do nothing if empty", "XXX", Dummy.Z0_VarCharMax);
		}

		public void TestSetPropertyInfoValue_WithMaxLengthExceeded()
		{
			AssertEquals("No warnings should exist initially for the test", false, Notifications.HasWarnings);
			ImportContext.SetPropertyInfoValue(Dummy.Z0_FK_CodeInfo, new string('x', 100), true);
			AssertEquals(new string('x', Dummy.Z0_FK_CodeInfo.MaxLength), Dummy.Z0_FK_Code);
			AssertEquals("A warning should be raised because the maximum length of the field was exceeded", true, Notifications.HasWarnings);
		}

		public void TestSetPropertyInfoValue_WithCodeMapping()
		{
			var mappingOrg = Factory.Load<OrgHeader>(ImportContext.Converter.MappingOrgPK);

			var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery());

			OrgPatternMatchOverride vesselOverride = mappingOrg.CreatePatternMatchOverrideForTest();
			vesselOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			vesselOverride.OO_LocalGuid = port.PK;
			vesselOverride.OO_ForeignCode = "Blaticus";

			Factory.Save();

			ImportContext.SetPropertyInfoValue(Dummy.Z0_VarCharMaxInfo, "Blaticus", ForeignKeyType.PortNK, "Error Context");
			AssertEquals("Should map the port code correctly", port.RL_Code, Dummy.Z0_VarCharMax);
		}

		public void TestSetPropertyInfoValue_WithADate()
		{
			ImportContext.SetPropertyInfoValue(Dummy.Z0_DateInfo, new DateTime(2005, 1, 2));
			AssertEquals("Should set the date correctly", Dummy.Z0_Date, new ZDateTime(2005, 1, 2));
			ImportContext.SetPropertyInfoValue(Dummy.Z0_DateTimeOffsetInfo, new DateTimeOffset(2005, 1, 2, 3, 4, 5, 123, TimeSpan.FromHours(8)));
			AssertEquals("Should set the date correctly", Dummy.Z0_DateTimeOffset, new DateTimeOffset(2005, 1, 2, 3, 4, 5, 123, TimeSpan.FromHours(8)));
		}

		public void TestSetPropertyInfoValue_WithAGeography()
		{
			ImportContext.SetPropertyInfoValue(Dummy.Z0_GeographyInfo, SqlGeography.STGeomFromText(new System.Data.SqlTypes.SqlChars("POINT (121 47)"), 4326));
			AssertEquals("Should set the geography correctly", Dummy.Z0_Geography, new ZGeography("POINT (121 47)"));
		}

		public void TestSetPropertyInfoValue_WithCodeMappingAndIsElementSpecifiedAndErrorContext()
		{
			var mappingOrg = Factory.Load<OrgHeader>(ImportContext.Converter.MappingOrgPK);
			var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			var org = Factory.NewWithValidTestData<OrgHeader>();

			OrgPatternMatchOverride vesselOverride = mappingOrg.CreatePatternMatchOverrideForTest();
			vesselOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			vesselOverride.OO_LocalGuid = port.PK;
			vesselOverride.OO_ForeignCode = "Blaticus";

			Factory.Save();

			org.OH_RL_NKClosestPort = "XXX";
			ImportContext.SetPropertyInfoValue(org.OH_RL_NKClosestPortInfo, "Blaticus", ForeignKeyType.PortNK, true, "");
			AssertEquals(port.RL_Code, org.OH_RL_NKClosestPort);

			org.OH_RL_NKClosestPort = "XXX";
			ImportContext.SetPropertyInfoValue(org.OH_RL_NKClosestPortInfo, "Blaticus", ForeignKeyType.PortNK, true);
			AssertEquals(port.RL_Code, org.OH_RL_NKClosestPort);

			org.OH_RL_NKClosestPort = "XXX";
			ImportContext.SetPropertyInfoValue(org.OH_RL_NKClosestPortInfo, "Blaticus", ForeignKeyType.PortNK, false, "");
			AssertEquals("XXX", org.OH_RL_NKClosestPort);

			org.OH_RL_NKClosestPort = "XXX";
			ImportContext.SetPropertyInfoValue(org.OH_RL_NKClosestPortInfo, "Blaticus", ForeignKeyType.PortNK, false);
			AssertEquals("XXX", org.OH_RL_NKClosestPort);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			org.OH_RL_NKClosestPort = "XXX";
			ImportContext.SetPropertyInfoValue(org.OH_RL_NKClosestPortInfo, "Blaticus", ForeignKeyType.PortNK, false, "");
			AssertEquals(port.RL_Code, org.OH_RL_NKClosestPort);

			org.OH_RL_NKClosestPort = "XXX";
			ImportContext.SetPropertyInfoValue(org.OH_RL_NKClosestPortInfo, "Blaticus", ForeignKeyType.PortNK, false);
			AssertEquals(port.RL_Code, org.OH_RL_NKClosestPort);
		}

		public void TestSetPropertyInfoValueIfNotEmpty_WithADate()
		{
			Dummy.Z0_Date = ZDateTime.Empty;
			ImportContext.SetPropertyInfoValueIfValueNotEmpty(Dummy.Z0_DateInfo, (ZDateTime)new DateTime(2005, 1, 2));
			AssertEquals("Should set the date correctly", Dummy.Z0_Date, new ZDateTime(2005, 1, 2));

			ImportContext.SetPropertyInfoValueIfValueNotEmpty(Dummy.Z0_DateInfo, ZDateTime.Empty);
			AssertEquals("Should NOT set the date as value is empty", Dummy.Z0_Date, new ZDateTime(2005, 1, 2));

			Dummy.Z0_DateTimeOffset = ZDateTimeOffset.Empty;
			ImportContext.SetPropertyInfoValueIfValueNotEmpty(Dummy.Z0_DateTimeOffsetInfo, (ZDateTimeOffset)new DateTimeOffset(2005, 1, 2, 3, 4, 5, 123, TimeSpan.FromHours(8)));
			AssertEquals("Should set the date correctly", Dummy.Z0_DateTimeOffset, new ZDateTimeOffset(2005, 1, 2, 3, 4, 5, 123, TimeSpan.FromHours(8)));

			ImportContext.SetPropertyInfoValueIfValueNotEmpty(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Empty);
			AssertEquals("Should NOT set the date as value is empty", Dummy.Z0_DateTimeOffset, new ZDateTimeOffset(2005, 1, 2, 3, 4, 5, 123, TimeSpan.FromHours(8)));
		}

		public void TestSetPropertyInfoValueIfNotEmpty_WithAGeography()
		{
			Dummy.Z0_Geography = ZGeography.Empty;
			ImportContext.SetPropertyInfoValueIfValueNotEmpty(Dummy.Z0_GeographyInfo, (ZGeography)SqlGeography.STGeomFromText(new System.Data.SqlTypes.SqlChars("POINT (-122 47)"), 4326));
			AssertEquals("Should set the geography correctly", Dummy.Z0_Geography, new ZGeography("POINT (-122 47)"));

			ImportContext.SetPropertyInfoValueIfValueNotEmpty(Dummy.Z0_DateInfo, ZGeography.Empty);
			AssertEquals("Should NOT set the geography as value is empty", Dummy.Z0_Geography, new ZGeography("POINT (-122 47)"));
		}

		public void TestSetPropertyInfoValue_Decimal()
		{
			ImportContext.SetPropertyInfoValue(Dummy.Z0_AnotherDecimalInfo, 777.666m, DummyBizoSchema.Z0_AnotherDecimal);
			AssertEquals(777.666m, Dummy.Z0_AnotherDecimal);
		}

		public void TestConvertRawStringToZType()
		{
			var orgExample = Factory.New<OrgPatternMatchOverride>();
			orgExample.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Country;
			orgExample.OO_ForeignCode = "India";
			orgExample.OO_LocalGuid = Core.Constants.CountryGuids.India;
			orgExample.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			Factory.Save();

			IZType country = ImportContext.ConvertRawStringToZTypeValue<ZString>("India", ForeignKeyType.CountryNK);
			AssertEquals("ZString country code should be india", Core.Constants.CountryCodes.India, (ZString)country);

			country = ImportContext.ConvertRawStringToZTypeValue<ZGuid>("India", ForeignKeyType.CountryNK);
			AssertEquals("ZGuid country should be set to orgExample's country guid which is India", Core.Constants.CountryGuids.India, (ZGuid)country);

			string port = "INBOM";
			var convertedPort = ImportContext.ConvertRawStringToZTypeValue<ZString>(port, ForeignKeyType.PortNK);
			AssertEquals("Converted Port should match the input port", "INBOM", convertedPort);

			var vesselName = ImportContext.ConvertRawStringToZTypeValue<ZInt>("456789", ForeignKeyType.VesselNameNK);
			AssertEquals("Vessel name should be set to this number", 456789, vesselName);
		}

		#endregion

		#region INotifications

		public void TestLastNotificationMessage()
		{
			ImportContext.Notify(new InfoNotification("Test Message"));
			ImportContext.Notify(new InfoNotification("Test Message1"));
			AssertEquals("Test Message1", ImportContext.LastNotificationMessage);
		}

		public void TestNotify()
		{
			AssertEquals("There should be no notifications on the inner INotifications initially", false, Notifications.HasErrors);
			ImportContext.Notify(new ErrorNotification(ErrorType.Error));
			AssertEquals("There should be notifications on the inner INotifications now", true, Notifications.HasErrors);
		}

		public void TestQueryUser()
		{
			var notifications = new TestNotificationSubscriberQueryUser();
			var context = new ValueObjectImportContext(Factory, notifications);

			context.QueryUser(new QueryUserYesNoEventArgs("message", true));
			AssertNotNull("The user should have been queried", notifications.LastQueryUser);
		}

		public void TestNotificationHasErrors()
		{
			AssertEquals("There should be no notifications on the inner INotifications initially", false, ImportContext.NotificationsHasErrors);
			ImportContext.Notify(new ErrorNotification(ErrorType.Error));
			AssertEquals("There should be notifications on the inner INotifications now", true, ImportContext.NotificationsHasErrors);
			Notifications.Clear();
			AssertEquals("There should be no notifications on the inner INotifications initially", false, ImportContext.NotificationsHasErrors);
		}

		public void TestSetPropertyInfoValueWithinMaxLength()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = ZString.Empty;
			var valueToSet = "A".PadRight(dummy.Z0_CodeInfo.MaxLength + 1, 'A');

			AssertNoExceptionThrown(delegate
			{ ImportContext.SetPropertyInfoValueWithinMaxLengthAndWarn(dummy.Z0_CodeInfo, valueToSet); });
			AssertEquals("value set", "A".PadRight(dummy.Z0_CodeInfo.MaxLength, 'A'), dummy.Z0_Code);
			AssertContains("exceeds the max length that is allowed for the field,", ImportContext.LastNotificationMessage);
		}

		public void TestNotificationsContaionsNotifictionType()
		{
			AssertEquals("There should be no Error notifications initially", false, ImportContext.NotificationsContainsNotifictionType(ErrorType.Error));
			AssertEquals("There should be no DataErrorPreventSave notifications initially", false, ImportContext.NotificationsContainsNotifictionType(ErrorType.DataErrorPreventSave));

			ImportContext.Notify(new ErrorNotification(ErrorType.Error));

			AssertEquals("There should be Error notifications now", true, ImportContext.NotificationsContainsNotifictionType(ErrorType.Error));
			AssertEquals("There should be no DataErrorPreventSave notifications yet", false, ImportContext.NotificationsContainsNotifictionType(ErrorType.DataErrorPreventSave));

			ImportContext.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave));

			AssertEquals("There should be Error notifications now", true, ImportContext.NotificationsContainsNotifictionType(ErrorType.Error));
			AssertEquals("There should be DataErrorPreventSave notifications now", true, ImportContext.NotificationsContainsNotifictionType(ErrorType.DataErrorPreventSave));

			Notifications.Clear();

			AssertEquals("There should be no Error notifications finally", false, ImportContext.NotificationsContainsNotifictionType(ErrorType.Error));
			AssertEquals("There should be no DataErrorPreventSave notifications finally", false, ImportContext.NotificationsContainsNotifictionType(ErrorType.DataErrorPreventSave));
		}

		class TestNotificationSubscriberQueryUser : INotifications, INotificationSubscriberQueryUser
		{
			public void Add(INotification @event)
			{
				throw new NotSupportedException();
			}

			public IQueryUserEventArgs LastQueryUser;
			public void QueryUser(IQueryUserEventArgs e)
			{
				LastQueryUser = e;
			}
		}

		#endregion

		#region Implementation

		NotificationBuffer Notifications;
		ValueObjectImportContext ImportContext;

		class TestOrganisationMatching : OrganisationMatching
		{
			public TestOrganisationMatching(BusinessObjectFactory factory, Xsd.XmlInterchange interchange, INotifications notifications)
				: base(new BusinessObjectFactoryProvider(factory), interchange, notifications)
			{
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Notifications = new NotificationBuffer();
			ImportContext = new ValueObjectImportContext(Factory, Notifications);
		}

		#endregion
	}
}
