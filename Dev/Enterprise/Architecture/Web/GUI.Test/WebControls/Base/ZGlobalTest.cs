using System;
using System.IO;
using System.Xml;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	public abstract class ZGlobalTest : TransactionedTestCase
	{
		#region setup

		protected virtual ZGlobal GetNewZGlobalForTesting()
		{
			return new ZGlobalForTesting();
		}

		#endregion

		[ExpectNoExceptions("Failed to reflect Enterprise.Initialisation.Initialiser.InitialiseAfterDbConnection method to Initialise Excel for Web")]
		public void TestInitialiseExcelForWeb()
		{
			ZGlobal global = GetNewZGlobalForTesting();
			ZDateTime dateTest = ZDateTime.Now;
			//TODO: Make AppDomain
			//Global.InitialiseExcelEngine();
		}

		#region Web.Config test

		protected abstract string WebConfigPath { get; }
		protected abstract int NumberOfLocations { get; }

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRequiredApplicationSettings()
		{
			string webConfigFileName = Path.Combine(BaseSourcePath, WebConfigPath);
			XmlDocument doc = new XmlDocument();
			XmlTextReader reader = new XmlTextReader(webConfigFileName);
			try
			{
				doc.Load(reader);

				XmlNodeList list = doc.GetElementsByTagName("configuration");
				AssertEquals("Should contain confuguration section", 1, list.Count);
				AssertEquals("Should contain data", true, list[0].HasChildNodes);

				AssertEquals("Should contain appSettings section", 1, list[0].SelectNodes("/configuration/appSettings").Count);

				AssertEquals("Should contain DatabaseName entry", ExpectedDatabaseName, list[0].SelectSingleNode("/configuration/appSettings/add[@key='DatabaseName']").Attributes["value"].Value);
				AssertEquals("Should contain ServerName entry", ExpectedServerName, list[0].SelectSingleNode("/configuration/appSettings/add[@key='ServerName']").Attributes["value"].Value);
				AssertEquals("Should contain CompanyName entry", "WiseTech Global", list[0].SelectSingleNode("/configuration/appSettings/add[@key='CompanyName']").Attributes["value"].Value);
				AssertEquals("Should contain HomePageURL entry", ExpectedHomePageURL, list[0].SelectSingleNode("/configuration/appSettings/add[@key='HomePageURL']").Attributes["value"].Value);
				AssertEquals("Should contain Branch entry", ExpectedBranch, list[0].SelectSingleNode("/configuration/appSettings/add[@key='Branch']").Attributes["value"].Value);
				AssertEquals("Should contain DBAdminEmail entry", ExpectedDBAdminEmail, list[0].SelectSingleNode("/configuration/appSettings/add[@key='DBAdminEmail']").Attributes["value"].Value);
				AssertCustomApplicationSettings(list[0]);
			}
			finally
			{
				reader.Close();
			}
		}

		protected virtual void AssertCustomApplicationSettings(XmlNode node)
		{
		}

		#region Expected Application Settings

		protected virtual ZString ExpectedDatabaseName
		{
			get { return "Odyssey"; }
		}

		protected virtual ZString ExpectedServerName
		{
			get { return "dbserver"; }
		}

		protected virtual ZString ExpectedBranch
		{
			get { return "SYD"; }
		}

		protected virtual ZString ExpectedDBAdminEmail
		{
			get { return "TrackErr@cargowise.com"; }
		}

		protected virtual ZString ExpectedHomePageURL
		{
			get { return "http://www.wisetechglobal.com/"; }
		}
		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWebConfig()
		{
			string webConfigFileName = Path.Combine(BaseSourcePath, WebConfigPath);
			XmlDocument doc = new XmlDocument();
			XmlTextReader reader = new XmlTextReader(webConfigFileName);
			try
			{
				doc.Load(reader);

				XmlNodeList list = doc.GetElementsByTagName("configuration");
				AssertEquals("Should contain confuguration section", 1, list.Count);
				AssertEquals("Should contain data", true, list[0].HasChildNodes);

				AssertEquals("Should contain appSettings section", 1, list[0].SelectNodes("/configuration/appSettings").Count);
				AssertEquals("Should contain system.web section", 1, list[0].SelectNodes("/configuration/location[not(@path)]/system.web").Count);

				if (NumberOfLocations > 0)
				{
					AssertEquals("Should contain location section", NumberOfLocations, list[0].SelectNodes("/configuration/location[@path]").Count);
					AssertLocations(list[0].SelectNodes("/configuration/location[@path]"));
				}
			}
			finally
			{
				reader.Close();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMainSystemWebSectionIsNotInherited()
		{
			string webConfigFileName = Path.Combine(BaseSourcePath, WebConfigPath);
			AssertEquals("Actual web.config exists", true, File.Exists(webConfigFileName));

			XmlDocument doc = new XmlDocument();
			doc.Load(webConfigFileName);

			XmlNode mainSystemWebSection = doc.DocumentElement.SelectSingleNode("location[@inheritInChildApplications='false']/system.web");
			AssertNotNull("The main system.web section must exist and must be inside a <location inheritInChildApplications=\"false\"> tag.", mainSystemWebSection);

			CheckAllSystemWebNodesRecursively(doc.DocumentElement);
		}

		void CheckAllSystemWebNodesRecursively(XmlNode parentNode)
		{
			foreach (XmlNode node in parentNode.ChildNodes)
			{
				if (node.Name == "system.web")
				{
					XmlNode parentLocationNode = FindParentNodeRecursively(node, "location");

					if (parentLocationNode == null)
					{
						Fail(String.Format("The tag <{0}> is outside a <location> tag. All <system.web> nodes must be inside a <location> tag.", node.Name));
					}
					else
					{
						if (parentLocationNode.Attributes["path"] == null &&
							(parentLocationNode.Attributes["inheritInChildApplications"] == null
							|| !parentLocationNode.Attributes["inheritInChildApplications"].Value.Equals("false", StringComparison.CurrentCultureIgnoreCase)))
						{
							Fail(String.Format("Every <location> tag must either specify a path or have the inheritInChildApplications attribute set to false.", node.Name));
						}
					}
				}

				CheckAllSystemWebNodesRecursively(node);
			}
		}

		XmlNode FindParentNodeRecursively(XmlNode childNode, string name)
		{
			XmlNode parentNode = null;

			while (childNode.ParentNode != null)
			{
				if (childNode.ParentNode.Name == name)
				{
					parentNode = childNode.ParentNode;
					break;
				}
				else
				{
					childNode = childNode.ParentNode;
				}
			}

			return parentNode;
		}

		public abstract void AssertLocations(XmlNodeList locations);

		#endregion
	}
}
