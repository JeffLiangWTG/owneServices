using System.Globalization;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebServicesConfig))]
	public class WebServicesConfigTest : RegistryBusinessObjectTemplateTestCase<WebServicesConfig>
	{
		public void TestReadOnly()
		{
			var nonSupportStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, User.SupportUserName));
			using (Env.SetTemporaryUserContext(nonSupportStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var config = GetBusinessObjectToClone();
				AssertEquals(true, config.NameInfo.ReadOnly);
				AssertEquals(false, config.IsEnabledInfo.ReadOnly);
				AssertEquals(false, config.IsCustomURLInfo.ReadOnly);
				AssertEquals(true, config.URLInfo.ReadOnly);
				AssertEquals(false, config.IsAutoManagedInfo.ReadOnly);
				AssertEquals(false, config.NumberOfServerClustersInfo.ReadOnly);

				config.IsAutoManaged = false;

				AssertEquals(true, config.NameInfo.ReadOnly);
				AssertEquals(true, config.IsEnabledInfo.ReadOnly);
				AssertEquals(true, config.IsCustomURLInfo.ReadOnly);
				AssertEquals(true, config.URLInfo.ReadOnly);
				AssertEquals(false, config.IsAutoManagedInfo.ReadOnly);
				AssertEquals(false, config.NumberOfServerClustersInfo.ReadOnly);
			}
		}

		public void TestWebServiceTeardownWarningPopup()
		{
			var config = GetBusinessObjectToClone();
			config.IsEnabled = true;
			config.RunPreSaveValidation();
			AssertNoWarnings(config.IsEnabledInfo);

			config.IsEnabled = false;
			AssertHasWarnings(config.IsEnabledInfo);
		}

		public void TestCustomUrlValidationChecks()
		{
			var config = GetBusinessObjectToClone();
			config.IsCustomURL = true;
			config.RunPreSaveValidation();
			AssertHasErrors(config.URLInfo);

			config.URL = "YR6TST.webprint.wisegrid.net";
			AssertNoErrors(config.URLInfo);

			config.URL = "GRHTST.webprint.wisegrid.net";
			AssertHasWarnings(config.URLInfo);
		}

		public void TestIsAutoManagedInformationTooltip()
		{
			var config = GetBusinessObjectToClone();
			config.IsCustomURL = true;
			config.RunPreSaveValidation();
			AssertHasErrors(config.URLInfo);

			config.URL = "YR6TST.webprint.wisegrid.net";
			AssertNoErrors(config.URLInfo);

			config.URL = "GRHTST.webprint.wisegrid.net";
			AssertHasWarnings(config.URLInfo);
		}

		public void TestXmlSerialization()
		{
			var config = GetBusinessObjectToSerialise();
			string xml;
			var serializer = ZXmlSerializer.New(typeof(WebServicesConfig));
			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				serializer.Serialize(writer, config);
				xml = writer.ToString();
			}

			using (var reader = new StringReader(xml))
			{
				var clone = (WebServicesConfig)serializer.Deserialize(reader);
				AssertEquals(false, clone.IsEnabled);
				AssertEquals(true, clone.IsAutoManaged);
				AssertEquals(false, clone.IsCustomURL);
				AssertEquals(string.Empty, clone.URL);
				AssertEquals((short)1, clone.NumberOfServerClusters);
			}
		}

		public void TestCodeAlive()
		{
			AssertNotNull("CodeAlive", typeof(WebServicesConfig.Schema));
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WebServicesConfig();
		}

		protected override WebServicesConfig GetBusinessObjectToClone()
		{
			return (WebServicesConfig)GetNewBusinessObject();
		}

		protected override WebServicesConfig GetBusinessObjectToSerialise()
		{
			return (WebServicesConfig)GetNewBusinessObject();
		}

		#endregion
	}
}
