using System;
using System.Security.Cryptography.X509Certificates;
using CargoWise.ActiveDirectory.TestFramework;
using Microsoft.Web.Administration;
using WTG.DevTools.TestFramework;
using WTG.StaticAnalysis.Annotation;

[assembly:UsesConstants(typeof(TestConstants))]
namespace Enterprise.ZArchitecture.Web.Shared.Test
{
		public class WebApplicationInstaller : ExecuteOrInvokeViaAppManager
		{
			public static void ExecuteOrInvoke(
				string deploymentPath,
				int port,
				int tlsPort,
				byte[] tlsCertificateHash,
				string siteName,
				string appPoolName,
				bool enableWindowsAuthentication,
				string rapidFailProtectionInterval = "00:05:00",
				int rapidFailProtectionMaxCrashes = 5)
			{
				new WebApplicationInstaller
				{
					deploymentPath = deploymentPath,
					port = port,
					tlsPort = tlsPort,
					tlsCertificateHash = tlsCertificateHash,
					siteName = siteName,
					appPoolName = appPoolName,
					enableWindowsAuthentication = enableWindowsAuthentication,
					rapidFailProtectionInterval = rapidFailProtectionInterval,
					rapidFailProtectionMaxCrashes = rapidFailProtectionMaxCrashes
				}.ExecuteOrInvoke();
			}

			public override void Execute()
			{
				using (var serverManager = new ServerManager())
				{
					var appPool = serverManager.ApplicationPools.Add(appPoolName);
					appPool.ProcessModel.IdentityType = ProcessModelIdentityType.SpecificUser;
					appPool.ProcessModel.UserName = TestConstants.ADTestUserAccount.NameWithDomain;
					appPool.ProcessModel.Password = TestConstants.ADTestUserAccount.Password;
					appPool.ProcessModel.LoadUserProfile = false;

					var site = serverManager.Sites.Add(siteName, deploymentPath, port);
					site.Bindings.RemoveAt(0);
					site.Bindings.Add($"*:{port}:localhost", "http");
					site.ApplicationDefaults.ApplicationPoolName = appPool.Name;

					if (tlsPort > 0 && tlsCertificateHash != null)
					{
						site.Bindings.Add($"*:{tlsPort}:localhost", tlsCertificateHash, nameof(StoreName.My));
					}

					if (enableWindowsAuthentication)
					{
						serverManager
							.GetApplicationHostConfiguration()
							.GetSection("system.webServer/security/authentication/windowsAuthentication", site.Name)["enabled"] = true;
					}

					ConfigureFastFailureProtection(
						serverManager.GetApplicationHostConfiguration(),
						appPoolName,
						rapidFailProtectionInterval,
						rapidFailProtectionMaxCrashes);

					serverManager.CommitChanges();
				}
			}

			static void ConfigureFastFailureProtection(Configuration hostConfig, string appPoolName, string rapidFailProtectionInterval, int rapidFailProtectionMaxCrashes)
			{
				var applicationPoolsSection = hostConfig.GetSection("system.applicationHost/applicationPools");
				var applicationPoolsCollection = applicationPoolsSection.GetCollection();
				var addElement = FindElement(applicationPoolsCollection, "add", "name", appPoolName)
					?? throw new InvalidOperationException($@"Add element not found for appPool: {appPoolName}");

				var failureElement = addElement.GetChildElement("failure");
				failureElement["rapidFailProtection"] = true;
				failureElement["rapidFailProtectionInterval"] = TimeSpan.Parse(rapidFailProtectionInterval);
				failureElement["rapidFailProtectionMaxCrashes"] = rapidFailProtectionMaxCrashes;
			}

			static ConfigurationElement FindElement(ConfigurationElementCollection collection, string elementTagName, params string[] keyValues)
			{
				foreach (var element in collection)
				{
					if (!string.Equals(element.ElementTagName, elementTagName, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					bool matchFound = true;
					for (int i = 0; i < keyValues.Length; i += 2)
					{
						object attributeValue = element.GetAttributeValue(keyValues[i]);
						string value = null;
						if (attributeValue != null)
						{
							value = attributeValue.ToString();
						}

						if (string.Equals(value, keyValues[i + 1], StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}

						matchFound = false;
						break;
					}

					if (matchFound)
					{
						return element;
					}
				}

				return null;
			}

			protected override object[] GetState()
			{
				return new object[]
				{
					deploymentPath,
					port,
					tlsPort,
					tlsCertificateHash,
					siteName,
					appPoolName,
					enableWindowsAuthentication,
					rapidFailProtectionInterval,
					rapidFailProtectionMaxCrashes
				};
			}

			protected override void ReadState(object[] state)
			{
				deploymentPath = (string)state[0];
				port = (int)state[1];
				tlsPort = (int)state[2];
				tlsCertificateHash = (byte[])state[3];
				siteName = (string)state[4];
				appPoolName = (string)state[5];
				enableWindowsAuthentication = (bool)state[6];
				rapidFailProtectionInterval = (string)state[7];
				rapidFailProtectionMaxCrashes = (int)state[8];
			}

			string deploymentPath;
			int port;
			int tlsPort;
			byte[] tlsCertificateHash;
			string siteName;
			string appPoolName;
			bool enableWindowsAuthentication;
			string rapidFailProtectionInterval;
			int rapidFailProtectionMaxCrashes;
		}
}
