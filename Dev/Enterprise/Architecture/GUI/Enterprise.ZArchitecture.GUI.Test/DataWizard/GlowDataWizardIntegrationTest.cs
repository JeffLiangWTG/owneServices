using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Authentication.Primitives;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.DataTransfer;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WTG.Foundation.Http;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GlowDataWizardIntegrationTest : TestCaseWithDummy
	{
		public void TestShowImportMappingWizard_ReturnErrorWhenGlowInterfaceDoesNotExist()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var result = GlowDataWizardIntegration.ShowImportMappingWizard(typeof(DummyLogged), new TestMapping());
			AssertEquals(result, "The Advanced Data Automation Wizard is not available on this module.");
		}

		public void TestShowImportMappingWizard_ReturnErrorWhenUserIsCWSupport()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			tempContext.Dispose();
			tempContext = null;
			var result = GlowDataWizardIntegration.ShowImportMappingWizard(typeof(DummyLogged), new TestMapping());
			AssertEquals(result, "The CW1 Support login cannot be used when interacting with the Advanced Data Automation Wizard. Please login as an operational user in order to use this feature.");
		}

		public void TestShowImportMappingWizard_ReturnErrorWhenAuthorizationFailureIsSuccessDBMismatch()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ThrowsAsync(new GlowConfigurationException("Service authentication failed due to instance mismatch. Please contact your administrator."));
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);
			var result = GlowDataWizardIntegration.ShowImportMappingWizard(typeof(DummyBusinessObject), new TestMapping());
			AssertEquals("Service authentication failed due to an instance mismatch. Ensure that your Glow Services and Glow Portals settings are configured correctly. Please contact your administrator.", result);
		}

		public void TestShowImportMappingWizard_ReturnErrorWhenAuthorizationFailureInNotSuccess()
		{
			ExceptionReporterTestListener.Instance.Clear();
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ThrowsAsync(new AuthorizationFailureException(AuthenticationResult.AbnormalFailure));
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);
			var result = GlowDataWizardIntegration.ShowImportMappingWizard(typeof(DummyBusinessObject), new TestMapping());
			AssertEquals("The Advanced Data Automation Wizard is not available on this module.", result);
			AssertNotNull(ExceptionReporterTestListener.Instance[0]);
			AssertEquals("GlowDataWizardIntegration_IsImportable_Unauthorized", ExceptionReporterTestListener.Instance[0].Message);
			ErrorReporter.Clear();
		}

		public void TestShowImportMappingWizard_WhenAuthorizationFailure_WithLogonDetailsIncorrect_ShouldShowDialog()
			=> TestShowImportMappingWizardWithAuthorizationFailure(AuthenticationResult.LogonDetailsIncorrect);

		public void TestShowImportMappingWizard_WhenAuthorizationFailure_WithPasswordChangeRequired_ShouldShowDialog()
			=> TestShowImportMappingWizardWithAuthorizationFailure(AuthenticationResult.PasswordChangeRequired);

		public void TestShowImportMappingWizard_WhenAuthorizationFailure_WithLoginDisabled_ShouldShowDialog()
			=> TestShowImportMappingWizardWithAuthorizationFailure(AuthenticationResult.LoginDisabled);

		public void TestShowImportMappingWizard_WhenAuthorizationFailure_WithAccountLocked_ShouldShowDialog()
			=> TestShowImportMappingWizardWithAuthorizationFailure(AuthenticationResult.AccountLocked);

		public void TestShowImportMappingWizard_WhenAuthorizationFailure_WithContextChangeRequired_ShouldShowDialog()
			=> TestShowImportMappingWizardWithAuthorizationFailure(AuthenticationResult.ContextChangeRequired);

		void TestShowImportMappingWizardWithAuthorizationFailure(AuthenticationResult authenticationResult)
		{
			ExceptionReporterTestListener.Instance.Clear();
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var response = new HttpResponseMessage(HttpStatusCode.OK))
			using (response.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool> { { "IEntity", false } })))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ThrowsAsync(new AuthorizationFailureException(authenticationResult));

				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);

				using (var menuItem = new ZMenuItem((NoResString)"Advanced Data Automation Wizard"))
				{
					GlowDataWizardIntegration.HandleADAWMenuItemPopup(menuItem, typeof(DummyBusinessObject), _ => { }, _ => { });

					var expectedMessage = @"Changes to your credentials have occurred since the last login.
In order to use Advanced Data Automation Wizard, please logout first and login again.";
					AssertEquals(0, menuItem.MenuItems.Count);
					AssertEquals("Advanced Data Automation Wizard (Currently Unavailable)", menuItem.Caption.ToString());
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestShowImportMappingWizard_ReturnErrorWhenGlowImportableMetadataIsMissing()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			using (var response = new HttpResponseMessage())
			using (response.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool>())))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ReturnsAsync(response);
				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);
				var result = GlowDataWizardIntegration.ShowImportMappingWizard(typeof(DummyBusinessObject), new TestMapping());
				AssertEquals(result, "The Advanced Data Automation Wizard is not available on this module. If you wish to use it, please log an eRequest and WiseTech Global will look at adding this functionality.");
			}
		}

		public void TestShowImportMappingWizard_ReturnErrorWhenGlowPortalsUriRegistryItemIsEmpty()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			using (var response = new HttpResponseMessage())
			using (response.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool> { { "IDummyBizo", true } })))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ReturnsAsync(response);
				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);
				var result = GlowDataWizardIntegration.ShowImportMappingWizard(typeof(DummyBusinessObject), new TestMapping());
				AssertEquals(result, @"The Advanced Data Automation Wizard cannot be used due to missing Portals URL in the GLOW configuration.
Please contact your System Administrator.");
			}
		}

		public void TestShowImportMappingWizard_ReturnErrorWhenGlowServiceUriRegistryItemIsEmpty()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			using (var response = new HttpResponseMessage())
			using (response.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool> { { "IDummyBizo", true } })))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ReturnsAsync(response);
				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);
				var result = GlowDataWizardIntegration.ShowImportMappingWizard(typeof(DummyBusinessObject), new TestMapping());
				AssertEquals(result, @"The Advanced Data Automation Wizard cannot be used due to missing Service URL in the GLOW configuration.
Please contact your System Administrator.");
			}
		}

		public void TestShowImportMappingWizard_ShouldShowBrowserWithURLWithTokenAndType()
		{
			var testGuid = "cd762682-abca-4d41-b6a7-d43f039e94cf";
			var testMapping = new TestMapping() { PK = new Guid(testGuid) };
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			using (var response = new HttpResponseMessage())
			using (response.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool> { { "IDummyBizo", true } })))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ReturnsAsync(response);
				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);
				var result = GlowDataWizardIntegration.ShowImportMappingWizard(typeof(DummyBusinessObject), testMapping);
				AssertEquals(result, string.Empty);

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				var accessToken = queryKeyValuePairs["sso_otp"];

				AssertEquals("https", uri.Scheme);
				AssertEquals("address", uri.Host);
				AssertEquals("/GHS/" + FormFactor.Desktop, uri.AbsolutePath);
				AssertEquals("#/dataAutomation/importMappingPage/IDummyBizo/" + testGuid, uri.Fragment);

				var consumed = ObjectFactory.Get<ITokenizedAccessControl>().TryConsume(accessToken, AccessTokenTypes.LocalIdentity, out var tokenInfo);
				Assert(nameof(consumed), consumed);
				AssertEquals(Env.CurrentUserPK, tokenInfo.ParentId);
				AssertEquals(GlbStaffSchema.Constants.Prefix, tokenInfo.ParentTableCode);

				AssertNotNullOrEmpty(tokenInfo.Scope);
				var scopeObject = JObject.Parse(tokenInfo.Scope);

				// DO NOT MODIFY WITHOUT ALSO EDITING THE CORRESPONDING CODE IN GLOW
				var branchPK = new Guid((string)scopeObject["branch"]);
				AssertEquals(Env.CurrentBranchPK, branchPK);

				// DO NOT MODIFY WITHOUT ALSO EDITING THE CORRESPONDING CODE IN GLOW
				var departmentPK = new Guid((string)scopeObject["department"]);
				AssertEquals(Env.CurrentDepartmentPK, departmentPK);

				// DO NOT MODIFY WITHOUT ALSO EDITING THE CORRESPONDING CODE IN GLOW
				var isCaptiveSession = (bool)scopeObject["captive"];
				AssertEquals(true, isCaptiveSession);
			}
		}

		public void TestIsImportable_IsServiceUrlMissingWhenGlowServiceUriIsEmpty()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
			AssertEquals(IsImportableResult.ServiceUrlMissing, importable);
		}

		public void TestIsImportable_IsFalseWhenEnableAdvancedDataAutomationWizardIsFalse()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
			AssertEquals(IsImportableResult.False, importable);
		}

		public void TestIsImportable_IsNullWhenNoImportableMetadata()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			using (var response = new HttpResponseMessage())
			using (response.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool>())))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ReturnsAsync(response);
				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);
				var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
				AssertEquals(IsImportableResult.Undecided, importable);
			}
		}

		public void TestIsImportable_IsFalseWhenThirdPartyUserValidationRequired()
		{
			ExceptionReporterTestListener.Instance.Clear();
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).Throws(new AuthorizationFailureException("Blah blah blah"));
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);
			var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
			AssertEquals(IsImportableResult.Unknown, importable);
			AssertNotNull(ExceptionReporterTestListener.Instance[0]);
			AssertEquals("GlowDataWizardIntegration_IsImportable_Unauthorized", ExceptionReporterTestListener.Instance[0].Message);
			ErrorReporter.Clear();
		}

		public void TestIsImportable_IsFalseWhenThirdPartyUserValidationRequiredAndUserIsCWSupport()
		{
			ExceptionReporterTestListener.Instance.Clear();
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			tempContext.Dispose();
			tempContext = null;
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).Throws(new AuthorizationFailureException());
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);
			var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
			AssertEquals(IsImportableResult.Unknown, importable);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestIsImportable_IsFalseIfNoGlowInterfaceEvenWithForceImportableOn()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyLogged));
			AssertEquals(IsImportableResult.False, importable);
		}

		public void TestIsImportable_IsTrueIfGlowInterfaceoImportableMetadataButForceImportableOn()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
			AssertEquals(IsImportableResult.True, importable);
		}

		public void TestUserContextImportableDataDefinitionNamesUsesCacheUntilUserContextChanged()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var response = new HttpResponseMessage())
			using (response.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool> { { "IDummyBizo", false } })))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ReturnsAsync(response);
				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				clientFactoryMock.Setup(f => f.IncreaseTempUserCount()).Returns(Mock.Of<IDisposable>());
				ObjectFactory.Substitute(clientFactoryMock.Object);

				var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
				AssertEquals(IsImportableResult.False, importable);
				clientMock.Verify(c => c.GetAsync("api/datatransfer/datadefinitionnames"), Times.Once);

				importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
				AssertEquals(IsImportableResult.False, importable);
				clientMock.Verify(c => c.GetAsync("api/datatransfer/datadefinitionnames"), Times.Once);

				using (Env.SetTemporaryUserContext(new UserContext()))
				{
					importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
					AssertEquals(IsImportableResult.False, importable);
					clientMock.Verify(c => c.GetAsync("api/datatransfer/datadefinitionnames"), Times.Exactly(2));
				}
			}
		}

		public void TestIsImportable_IsTrueWhenHasImportableMetadataTrue()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var response = new HttpResponseMessage())
			using (response.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool> { { "IDummyBizo", true } })))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ReturnsAsync(response);
				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);
				var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
				AssertEquals(IsImportableResult.True, importable);
			}
		}

		public void TestIsImportable_IsFalseWhenHasImportableMetadataFalse()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var response = new HttpResponseMessage())
			using (response.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool> { { "IDummyBizo", false } })))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ReturnsAsync(response);
				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);
				var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
				AssertEquals(IsImportableResult.False, importable);
			}
		}

		public void TestIsImportable_WhenContextChangeRequired_ReturnsNull()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ThrowsAsync(new AuthorizationFailureException(AuthenticationResult.ContextChangeRequired));

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);

			var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
			AssertEquals(IsImportableResult.Unknown, importable);
		}

		public void TestIsImportable_IsUnknownAndReportWhenFailedResponseWithUnexpectedStatus()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var response = new HttpResponseMessage(HttpStatusCode.Forbidden))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ReturnsAsync(response);
				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);

				var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
				AssertEquals(IsImportableResult.Unknown, importable);
				var expectedMessageReported = $"GlowDataWizardIntegration_IsImportable failed with StatusCode: {HttpStatusCode.Forbidden}, Reason: {HttpStatusCode.Forbidden}.";
				AssertEquals(expectedMessageReported, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestIsImportable_IsUnknownAndReportWhenHttpClientExceptionWithNotExpectedStatus()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var expectedException = new GlowHttpRequestException("reportable error", HttpStatusCode.NotImplemented);
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).Throws(expectedException);
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);
			var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
			AssertEquals(IsImportableResult.Unknown, importable);
			AssertNotNull(ExceptionReporterTestListener.Instance[0]);
			AssertEquals(expectedException, ErrorReporter.LastExceptionReported);
			AssertEquals("GlowDataWizardIntegration_IsImportable", ExceptionReporterTestListener.Instance[0].Message);
			ErrorReporter.Clear();
		}

		public void TestIsImportable_IsUnknownAndNotReportWhenHttpClientExceptionWithExpectedStatus()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var clientMock = new Mock<IGlowServiceClient>();
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);

			TestIsImportable_IsUnknownAndNotReportWhenHttpClientExceptionWithExpectedStatus(clientMock, new GlowHttpRequestException("reportable error", 0));
			TestIsImportable_IsUnknownAndNotReportWhenHttpClientExceptionWithExpectedStatus(clientMock, new GlowHttpRequestException("reportable error", HttpStatusCode.BadGateway));
			TestIsImportable_IsUnknownAndNotReportWhenHttpClientExceptionWithExpectedStatus(clientMock, new GlowHttpRequestException("reportable error", HttpStatusCode.GatewayTimeout));
			TestIsImportable_IsUnknownAndNotReportWhenHttpClientExceptionWithExpectedStatus(clientMock, new GlowHttpRequestException("reportable error", HttpStatusCode.InternalServerError));
			TestIsImportable_IsUnknownAndNotReportWhenHttpClientExceptionWithExpectedStatus(clientMock, new GlowHttpRequestException("reportable error", HttpStatusCode.NotFound));
			TestIsImportable_IsUnknownAndNotReportWhenHttpClientExceptionWithExpectedStatus(clientMock, new GlowHttpRequestException("reportable error", HttpStatusCode.ServiceUnavailable));
			TestIsImportable_IsUnknownAndNotReportWhenHttpClientExceptionWithExpectedStatus(clientMock, new GlowHttpRequestException("reportable error", HttpStatusCode.Unauthorized));
			TestIsImportable_IsUnknownAndNotReportWhenHttpClientExceptionWithExpectedStatus(clientMock, new GlowHttpRequestException("reportable error", HttpStatusCode.ProxyAuthenticationRequired));
		}

		void TestIsImportable_IsUnknownAndNotReportWhenHttpClientExceptionWithExpectedStatus(Mock<IGlowServiceClient> clientMock, Exception expectedException)
		{
			clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).Callback(() => throw expectedException);

			var importable = GlowDataWizardIntegration.IsImportable(typeof(DummyBusinessObject));
			AssertEquals(IsImportableResult.Unknown, importable);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			AssertEquals(null, ErrorReporter.LastExceptionReported);
		}

		public void TestShowEmbeddedImportWizardAsync_EmbeddedImportWithCollection_EmptyUrl()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			var mappingMock = new Mock<IDataTransferMapping>();
			var result = GlowDataWizardIntegration.ShowEmbeddedImportWizard(null, mappingMock.Object);
			const string expected = @"The Advanced Data Automation Wizard cannot be used due to missing Service URL in the GLOW configuration.
Please contact your System Administrator.";
			AssertEquals(expected, result);
		}

		public void TestShowEmbeddedImportWizardAsync_ReturnErrorWhenUserIsCWSupport()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			tempContext.Dispose();
			tempContext = null;
			var result = GlowDataWizardIntegration.ShowEmbeddedImportWizard(null, new Mock<IDataTransferMapping>().Object);
			const string expected = "The CW1 Support login cannot be used when interacting with the Advanced Data Automation Wizard. Please login as an operational user in order to use this feature.";
			AssertEquals(expected, result);
		}

		public void TestShowEmbeddedImportWizard_DisplayUserFriendlyErrorOn413()
		{
			using (var response = new HttpResponseMessage() { StatusCode = HttpStatusCode.RequestEntityTooLarge })
			using (response.Content = new StringContent("413.1 - Request Entity Too Large"))
			{
				TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
					SetupClientMockWithSampleDataResponse(response),
					string.Empty,
					string.Empty,
					"Start getting data rows\n" +
					"Tried to get sample data but the request was unsuccessful.\n" +
					"The file uploaded is too large and exceeds the size supported. Please select a smaller file and try again.\n" +
					"Import has finished with errors.\n");
			}
		}

		Mock<IGlowServiceClient> SetupClientMockWithSampleDataResponse(HttpResponseMessage expectedResponse, Mock<IGlowServiceClient> clientMock = null)
		{
			var mock = clientMock ?? new Mock<IGlowServiceClient>();
			mock
				.Setup(c => c.PostAsync("api/datatransfer/sampledata?separator=%2C&sheetName=fooSheet&processFullStream=true", It.IsAny<HttpContent>()))
				.ReturnsAsync(expectedResponse);
			return mock;
		}

		public void TestShowEmbeddedImportWizardAsync_ChildGridImport()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;
			var expectedLog = "Start getting data rows\nFinish getting data rows\n" +
									"Start getting mapping data model\nFinish getting mapping data model\n" +
									"Start populating rows\nFinish populating rows\n" +
									"Data has been imported successfully.\n";
			TestShowEmbeddedImportWizardAsync_ChildGridImport(null, expectedLog);
		}

		public void TestShowEmbeddedImportWizardAsync_ChildGridImport_AdditionalAction()
		{
			var func = new Func<string, INotifications, bool>((fileName, notification) =>
			{
				notification.Add(CargoWise.EntityFramework.NotificationType.Information, "Additional Log Message");
				return true;
			});

			var expectedLog = "Start getting data rows\nFinish getting data rows\n" +
									"Start getting mapping data model\nFinish getting mapping data model\n" +
									"Start populating rows\nFinish populating rows\n" +
									"Additional Log Message\n" +
									"Data has been imported successfully.\n";
			TestShowEmbeddedImportWizardAsync_ChildGridImport(func, expectedLog);
		}

		void TestShowEmbeddedImportWizardAsync_ChildGridImport(Func<string, INotifications, bool> additionalAction, string expectedLog)
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var tempFilePath = string.Empty;
			try
			{
				tempFilePath = CreateFileWithData("data");
				var businessEntity = Factory.New<DummyBusinessObject>();
				Factory.Save();

				var filter = new ZQuery(DummyDependentBizoSchema.ZD1_Z0, businessEntity.PK);
				var collection = new TestCollection<DummyDependantBusinessObject>(filter);
				var mappingPk = Guid.NewGuid();
				var mappingMock = new Mock<IDataTransferMapping>();
				mappingMock.Setup(m => m.Name).Returns("Test mapping");
				mappingMock.Setup(m => m.ContextModule).Returns("IDummyBizo");
				mappingMock.Setup(m => m.PK).Returns(mappingPk);
				mappingMock.Setup(m => m.Delimiter).Returns(",");
				mappingMock.Setup(m => m.SheetName).Returns("fooSheet");
				mappingMock.Setup(m => m.StartingRow).Returns(1);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;

				var sampleData = new SampleData();

				sampleData.DelimitedRows.Add(new SampleDataLine(0, new[] { "Col1", "Col2", "Col3" }));
				sampleData.DelimitedRows.Add(new SampleDataLine(1, new[] { "AAA", "True", "3.45" }));
				sampleData.DelimitedRows.Add(new SampleDataLine(2, new[] { "BBB, CCC", "False", "1.00001" }));
				var sampleDataResponse = new HttpResponseMessage();
				using (sampleDataResponse.Content = new StringContent(JsonConvert.SerializeObject(sampleData)))
				{
					var clientMock = SetupClientMockWithSampleDataResponse(sampleDataResponse);
					var importPreviewHeader0 = new ImportPreviewHeader(string.Empty, new[] { "Header1", "Header2", "Header3" });
					var importPreviewLine0 = new ImportPreviewLine(string.Empty, 0, new[] {
						new ImportPreviewLineDetails("AAA", 0),
						new ImportPreviewLineDetails("True", 1),
						new ImportPreviewLineDetails("2", 2)
					});

					var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "Header1", "Header2", "Header3" });
					var importPreviewHeader2 = new ImportPreviewHeader("ChildCol", new[] { "ChildCol.Header11", "ChildCol.Header22" });

					var importPreviewLine1 = new ImportPreviewLine(string.Empty, 0, new[] {
						new ImportPreviewLineDetails("AAA", 0),
						new ImportPreviewLineDetails("True", 1),
						new ImportPreviewLineDetails("2", 2)
					});
					importPreviewLine1.ChildLines.Add(new ImportPreviewLine("ChildCol", 0, new[] { new ImportPreviewLineDetails("data1", 0), new ImportPreviewLineDetails("12-06-2017", 1) }));
					importPreviewLine1.ChildLines.Add(new ImportPreviewLine("ChildCol", 1, new[] { new ImportPreviewLineDetails("data2", 0), new ImportPreviewLineDetails("24-06-2017", 1) }));

					var dataRows = new[] { new ImportPreview(new[] { importPreviewHeader0 }, importPreviewLine0), new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2 }, importPreviewLine1) };

					using (var response = new HttpResponseMessage())
					using (response.Content = new StringContent(JsonConvert.SerializeObject(dataRows)))
					{
						var previewContentExpected = new[] { new SampleDataLine(1, null, "AAA,True,3.45"), new SampleDataLine(2, null, "\"BBB, CCC\",False,1.00001") };
						clientMock.Setup(c => c.PostAsJsonAsync($"api/datatransfer/createpreview?mappingPK={mappingPk}", It.IsAny<IEnumerable<SampleDataLine>>())).Returns(Task.FromResult(response));

						var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
						clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);

						var mappingDataModel = new MappingDataModel(new MappingDataDefinition[]
						{
								new MappingDataDefinition("Parent", "PV")
										.AddRelation("ChildFK", "Child", "PV_ChildFK")
						});

						using (var response1 = new HttpResponseMessage())
						using (response1.Content = new StringContent(JsonConvert.SerializeObject(mappingDataModel)))
						{
							clientMock.Setup(c => c.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(response1));

							var collectionImporterMock = new Mock<IGlowCollectionImporter>();

							collectionImporterMock.Setup(i => i.PopulateFromDataRows(collection, It.IsAny<ImportPreview[]>(), It.IsAny<MappingDataModel>(), It.IsAny<GlowLog>(), It.IsAny<IProgressReporter>())).Returns(true);

							ObjectFactory.Substitute(clientFactoryMock.Object);
							ObjectFactory.Substitute(collectionImporterMock.Object);

							using (var form = new ZForm())
							{
								var actualLogs = string.Empty;
								ZFormModaliser.ShowDialogsInTest = true;
								ZFormModaliser.SetDelegateToCallOnFormShown(showForm =>
								{
									if (showForm is GlowDataWizardImportForm)
									{
										var glowDataWizardForm = showForm as GlowDataWizardImportForm;
										actualLogs = glowDataWizardForm.Controls[0].Text;
									}
								});
								ZFormModaliser.SetApplicationActiveForm(form);
								var result = GlowDataWizardIntegration.ShowEmbeddedImportWizard(collection, mappingMock.Object, additionalAction);
								ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(form);
								AssertEquals(result, string.Empty);
								AssertEquals(expectedLog, actualLogs);
							}
						}
					}
				}
			}
			finally
			{
				ObjectFactory.DisposeSubstitutions();
				OpenedFormCache.GetInstance().CloseAllCachedForms();
				DeleteIfExists(tempFilePath);
				progressReporter.Dispose();
			}
		}

		public void TestShowEmbeddedImportWizardAsync_EmbeddedImportWithMapping_EmptyUrl()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			var mappingMock = new Mock<IDataTransferMapping>();
			var result = GlowDataWizardIntegration.ShowEmbeddedImportWizard(mappingMock.Object, null);
			AssertEquals(@"The Advanced Data Automation Wizard cannot be used due to missing Service URL in the GLOW configuration.
Please contact your System Administrator.", result);
		}

		public void TestShowEmbeddedImportWizardAsync_EmbeddedImportWithCollection_UserIsCWSupport()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			tempContext.Dispose();
			tempContext = null;
			var result = GlowDataWizardIntegration.ShowEmbeddedImportWizard(new Mock<IDataTransferMapping>().Object, null);
			AssertEquals("The CW1 Support login cannot be used when interacting with the Advanced Data Automation Wizard. Please login as an operational user in order to use this feature.", result);
		}

		public void TestShowEmbeddedImportWizardAsync_ImportService()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var tempFilePath = string.Empty;
			try
			{
				tempFilePath = CreateFileWithData("data");
				var mappingPk = Guid.NewGuid();
				var mappingMock = new Mock<IDataTransferMapping>();
				mappingMock.Setup(m => m.Name).Returns("Test mapping");
				mappingMock.Setup(m => m.ContextModule).Returns("IDummyBizo");
				mappingMock.Setup(m => m.PK).Returns(mappingPk);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;
				var importerMock = new Mock<IGlowServiceImporter>();
				ObjectFactory.Substitute(importerMock.Object);
				var externalLog = new GlowLog();
				var importHaveBeenCalled = false;
				importerMock
					.Setup(i => i.StartImportAsync(new Uri("https://address/"), mappingMock.Object, It.IsAny<MultipartContent>(), It.IsAny<GlowLog>()))
					.Callback((Uri uri, IDataTransferMapping info, MultipartContent content, GlowLog internalLog) =>
					{
						importHaveBeenCalled = true;

						AssertEquals(content.Single().Headers.ContentDisposition.FileName, "\"" + Path.GetFileName(tempFilePath) + "\"");
						AssertEquals(content.Single().Headers.ContentDisposition.DispositionType, "form-data");
						AssertEquals(content.Single().Headers.ContentDisposition.Name, "\"Files[]\"");
						internalLog.AppendLog(LogType.Error, "error message", 10);
						internalLog.AppendLog(LogType.Info, "info message", 90);
						externalLog = internalLog;
					});

				using (var form = new ZForm())
				{
					ZFormModaliser.SetApplicationActiveForm(form);
					var result = GlowDataWizardIntegration.ShowEmbeddedImportWizard(mappingMock.Object, () => { });
					ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(form);
				}

				Assert(importHaveBeenCalled);
				AssertEquals(2, externalLog.CountWithoutVerbose);

				AssertEquals(LogType.Error, externalLog.GetLogType(0));
				AssertEquals("error message", externalLog.GetLogMessage(0));

				AssertEquals(LogType.Info, externalLog.GetLogType(1));
				AssertEquals("info message", externalLog.GetLogMessage(1));
			}
			finally
			{
				ObjectFactory.DisposeSubstitutions();
				OpenedFormCache.GetInstance().CloseAllCachedForms();
				DeleteIfExists(tempFilePath);
				progressReporter.Dispose();
			}
		}

		public void TestShowEmbeddedImportWizardAsync_SucceedWithCaseMismatchFileExtension()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var tempFilePath = string.Empty;
			try
			{
				tempFilePath = CreateFileWithData("data", "CsV");
				var mappingPk = Guid.NewGuid();
				var mappingMock = new Mock<IDataTransferMapping>();
				mappingMock.Setup(m => m.Name).Returns("Test mapping");
				mappingMock.Setup(m => m.ContextModule).Returns("IDummyBizo");
				mappingMock.Setup(m => m.PK).Returns(mappingPk);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;
				var importerMock = new Mock<IGlowServiceImporter>();
				ObjectFactory.Substitute(importerMock.Object);
				var externalLog = new GlowLog();
				var importHaveBeenCalled = false;
				importerMock
					.Setup(i => i.StartImportAsync(new Uri("https://address/"), mappingMock.Object, It.IsAny<MultipartContent>(), It.IsAny<GlowLog>()))
					.Callback((Uri uri, IDataTransferMapping info, MultipartContent content, GlowLog internalLog) =>
					{
						importHaveBeenCalled = true;

						AssertEquals(content.Single().Headers.ContentDisposition.FileName, "\"" + Path.GetFileName(tempFilePath) + "\"");
						AssertEquals(content.Single().Headers.ContentDisposition.DispositionType, "form-data");
						AssertEquals(content.Single().Headers.ContentDisposition.Name, "\"Files[]\"");
						internalLog.AppendLog(LogType.Error, "error message", 10);
						internalLog.AppendLog(LogType.Info, "info message", 90);
						externalLog = internalLog;
					});

				using (var form = new ZForm())
				{
					ZFormModaliser.SetApplicationActiveForm(form);
					var result = GlowDataWizardIntegration.ShowEmbeddedImportWizard(mappingMock.Object, () => { });
					ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(form);
				}

				Assert(importHaveBeenCalled);
				AssertEquals(2, externalLog.CountWithoutVerbose);

				AssertEquals(LogType.Error, externalLog.GetLogType(0));
				AssertEquals("error message", externalLog.GetLogMessage(0));

				AssertEquals(LogType.Info, externalLog.GetLogType(1));
				AssertEquals("info message", externalLog.GetLogMessage(1));
			}
			finally
			{
				ObjectFactory.DisposeSubstitutions();
				OpenedFormCache.GetInstance().CloseAllCachedForms();
				DeleteIfExists(tempFilePath);
				progressReporter.Dispose();
			}
		}

		void TestShowEmbeddedImportWizardAsync_WhenGetDataRows(
			Mock<IGlowServiceClient> mockClient,
			string expectedKeyReported,
			string expectedMessageReported,
			string expectedActualLog)
		{
			ExceptionReporterTestListener.Instance.Clear();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var tempFilePath = string.Empty;
			try
			{
				tempFilePath = CreateFileWithData("data");
				var businessEntity = Factory.New<DummyBusinessObject>();
				Factory.Save();

				var filter = new ZQuery(DummyDependentBizoSchema.ZD1_Z0, businessEntity.PK);
				var collection = new TestCollection<DummyDependantBusinessObject>(filter);
				var mappingPk = new Guid("6b713ce5-a8d0-42eb-9a7a-b151324ab9a7");
				var mappingMock = new Mock<IDataTransferMapping>();
				mappingMock.Setup(m => m.Name).Returns("Test mapping");
				mappingMock.Setup(m => m.ContextModule).Returns("IDummyBizo");
				mappingMock.Setup(m => m.PK).Returns(mappingPk);
				mappingMock.Setup(m => m.Delimiter).Returns(",");
				mappingMock.Setup(m => m.SheetName).Returns("fooSheet");
				mappingMock.Setup(m => m.StartingRow).Returns(1);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;

				using (var sampleDataResponse = new HttpResponseMessage())
				using (sampleDataResponse.Content = new StringContent(JsonConvert.SerializeObject(new SampleData())))
				{
					SetupClientMockWithSampleDataResponse(sampleDataResponse, mockClient);

					var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
					clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(mockClient.Object);

					var log = new GlowLog();

					var collectionImporterMock = new Mock<IGlowCollectionImporter>();
					collectionImporterMock.Setup(i => i.PopulateFromDataRows(collection, It.IsAny<ImportPreview[]>(), It.IsAny<MappingDataModel>(), It.IsAny<GlowLog>(), progressReporter))
						.Returns(true);

					ObjectFactory.Substitute(clientFactoryMock.Object);
					ObjectFactory.Substitute(collectionImporterMock.Object);

					using (var form = new ZForm())
					{
						var actualLogs = string.Empty;
						ZFormModaliser.ShowDialogsInTest = true;
						ZFormModaliser.SetDelegateToCallOnFormShown(showForm =>
						{
							if (showForm is GlowDataWizardImportForm)
							{
								var glowDataWizardForm = showForm as GlowDataWizardImportForm;
								actualLogs = glowDataWizardForm.Controls[0].Text;
							}
						});

						ZFormModaliser.SetApplicationActiveForm(form);
						var result = GlowDataWizardIntegration.ShowEmbeddedImportWizard(collection, mappingMock.Object);
						var actualKeyReported = ErrorReporter.LastKeyReported;
						var actualMessageReported = ErrorReporter.LastMessageReported;
						ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(form);
						AssertEquals(string.Empty, result);
						AssertEquals(expectedKeyReported, actualKeyReported);
						AssertEquals(expectedMessageReported, actualMessageReported);
						AssertEquals(expectedActualLog, actualLogs);
					}
				}
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
				ErrorReporter.Clear();
				ObjectFactory.DisposeSubstitutions();
				OpenedFormCache.GetInstance().CloseAllCachedForms();
				DeleteIfExists(tempFilePath);
				progressReporter.Dispose();
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetDataRows_FailsWithNullDataRows()
		{
			using (var response = new HttpResponseMessage())
			using (response.Content = new StringContent(JsonConvert.SerializeObject(null)))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.PostAsJsonAsync($"api/datatransfer/createpreview?mappingPK=6b713ce5-a8d0-42eb-9a7a-b151324ab9a7", It.IsAny<IEnumerable<SampleDataLine>>()))
					.ReturnsAsync(response);

				TestShowEmbeddedImportWizardAsync_WhenGetDataRows(
					clientMock,
					string.Empty,
					string.Empty,
					"Start getting data rows\n" +
					"Tried to get data rows but returned null.\n" +
					"Post Request details: baseUri: https://address/, mappingPK: 6b713ce5-a8d0-42eb-9a7a-b151324ab9a7, Post Response details: StatusCode: OK, Content: null\n" +
					"Import has finished with errors.\n");
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetDataRows_FailsWithServerTransientError()
		{
			using (var response = new HttpResponseMessage() { StatusCode = HttpStatusCode.InternalServerError })
			using (response.Content = new StringContent("Some Error occured"))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.PostAsJsonAsync($"api/datatransfer/createpreview?mappingPK=6b713ce5-a8d0-42eb-9a7a-b151324ab9a7", It.IsAny<IEnumerable<SampleDataLine>>()))
					.ReturnsAsync(response);

				TestShowEmbeddedImportWizardAsync_WhenGetDataRows(
					clientMock,
					string.Empty,
					string.Empty,
					"Start getting data rows\n" +
					"Tried to get data rows but the request was unsuccessful.\n" +
					"Post Request details: baseUri: https://address/, mappingPK: 6b713ce5-a8d0-42eb-9a7a-b151324ab9a7, Post Response details: StatusCode: InternalServerError, Content: Some Error occured\n" +
					"Import has finished with errors.\n");
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetDataRows_FailsWithGlowHttpRequestExceptionWithTransientStatus()
		{
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.PostAsJsonAsync($"api/datatransfer/createpreview?mappingPK=6b713ce5-a8d0-42eb-9a7a-b151324ab9a7", It.IsAny<IEnumerable<SampleDataLine>>()))
				.ThrowsAsync(new GlowHttpRequestException("Network exception", HttpStatusCode.BadGateway));

			TestShowEmbeddedImportWizardAsync_WhenGetDataRows(clientMock, string.Empty, string.Empty, "Start getting data rows\n" +
				"Network exception (Code: 502)\n" +
				"Import has finished with errors.\n");
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetDataRows_FailsWithGlowHttpRequestExceptionWithNotTransientStatus()
		{
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.PostAsJsonAsync($"api/datatransfer/createpreview?mappingPK=6b713ce5-a8d0-42eb-9a7a-b151324ab9a7", It.IsAny<IEnumerable<SampleDataLine>>()))
				.ThrowsAsync(new GlowHttpRequestException("Network exception", HttpStatusCode.BadRequest));

			TestShowEmbeddedImportWizardAsync_WhenGetDataRows(
				clientMock,
				"GlowImportWizard_FailedImport",
				"Start getting data rows\r\n" +
				"Network exception (Code: 400)\r\n" +
				"An error report of the problem has been sent to CargoWise.",
				"Start getting data rows\n" +
				"Network exception (Code: 400)\n" +
				"An error report of the problem has been sent to CargoWise.\n");
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetDataRows_FailsWithServerNotTransientError()
		{
			using (var response = new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest })
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.PostAsJsonAsync($"api/datatransfer/createpreview?mappingPK=6b713ce5-a8d0-42eb-9a7a-b151324ab9a7", It.IsAny<IEnumerable<SampleDataLine>>()))
					.ReturnsAsync(response);

				TestShowEmbeddedImportWizardAsync_WhenGetDataRows(
					clientMock,
					"GlowImportWizard_FailedImport",
					"Start getting data rows\r\n" +
					"Tried to get data rows but the request was unsuccessful.\r\n" +
					"Post Request details: baseUri: https://address/, mappingPK: 6b713ce5-a8d0-42eb-9a7a-b151324ab9a7, Post Response details: StatusCode: BadRequest, Content: \r\n" +
					"An error report of the problem has been sent to CargoWise.",
					"Start getting data rows\n" +
					"Tried to get data rows but the request was unsuccessful.\n" +
					"Post Request details: baseUri: https://address/, mappingPK: 6b713ce5-a8d0-42eb-9a7a-b151324ab9a7, Post Response details: StatusCode: BadRequest, Content: \n" +
					"An error report of the problem has been sent to CargoWise.\n");
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetDataRows_FailsWithRandomException_ItShouldBubbleUp()
		{
			ExceptionReporterTestListener.Instance.Clear();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var tempFilePath = string.Empty;
			try
			{
				tempFilePath = CreateFileWithData("data");
				var businessEntity = Factory.New<DummyBusinessObject>();
				Factory.Save();

				var filter = new ZQuery(DummyDependentBizoSchema.ZD1_Z0, businessEntity.PK);
				var collection = new TestCollection<DummyDependantBusinessObject>(filter);
				var mappingPk = Guid.NewGuid();
				var mappingMock = new Mock<IDataTransferMapping>();
				mappingMock.Setup(m => m.Name).Returns("Test mapping");
				mappingMock.Setup(m => m.ContextModule).Returns("IDummyBizo");
				mappingMock.Setup(m => m.PK).Returns(mappingPk);
				mappingMock.Setup(m => m.Delimiter).Returns(",");
				mappingMock.Setup(m => m.SheetName).Returns("fooSheet");
				mappingMock.Setup(m => m.StartingRow).Returns(1);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;

				using (var sampleDataResponse = new HttpResponseMessage())
				using (sampleDataResponse.Content = new StringContent(JsonConvert.SerializeObject(new SampleData())))
				{
					var clientMock = SetupClientMockWithSampleDataResponse(sampleDataResponse);
					clientMock.Setup(c => c.PostAsJsonAsync($"api/datatransfer/createpreview?mappingPK={mappingPk}", It.IsAny<IEnumerable<SampleDataLine>>()))
						.ThrowsAsync(new ArgumentNullException("Argument"));

					var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
					clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);

					var log = new GlowLog();

					var collectionImporterMock = new Mock<IGlowCollectionImporter>();
					collectionImporterMock.Setup(i => i.PopulateFromDataRows(collection, It.IsAny<ImportPreview[]>(), It.IsAny<MappingDataModel>(), It.IsAny<GlowLog>(), progressReporter))
						.Returns(true);

					ObjectFactory.Substitute(clientFactoryMock.Object);
					ObjectFactory.Substitute(collectionImporterMock.Object);

					using (var form = new ZForm())
					{
						var actualLogs = string.Empty;
						ZFormModaliser.ShowDialogsInTest = true;
						ZFormModaliser.SetDelegateToCallOnFormShown(showForm =>
						{
							if (showForm is GlowDataWizardImportForm)
							{
								var glowDataWizardForm = showForm as GlowDataWizardImportForm;
								actualLogs = glowDataWizardForm.Controls[0].Text;
							}
						});

						ZFormModaliser.SetApplicationActiveForm(form);
						AssertExceptionThrown<ArgumentNullException>("Argument", () => GlowDataWizardIntegration.ShowEmbeddedImportWizard(collection, mappingMock.Object));
						AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
						AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
						AssertEquals(string.Empty, actualLogs);
						ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(form);
					}
				}
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
				ErrorReporter.Clear();
				ObjectFactory.DisposeSubstitutions();
				OpenedFormCache.GetInstance().CloseAllCachedForms();
				DeleteIfExists(tempFilePath);
				progressReporter.Dispose();
			}
		}

		void TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
			Mock<IGlowServiceClient> mockClient,
			string expectedKeyReported,
			string expectedMessageReported,
			string expectedActualLog,
			string fileContents = "data",
			Mock<IDataTransferMapping> mapping = null)
		{
			ExceptionReporterTestListener.Instance.Clear();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var tempFilePath = string.Empty;
			try
			{
				tempFilePath = CreateFileWithData(fileContents);
				var businessEntity = Factory.New<DummyBusinessObject>();
				Factory.Save();

				var filter = new ZQuery(DummyDependentBizoSchema.ZD1_Z0, businessEntity.PK);
				var collection = new TestCollection<DummyDependantBusinessObject>(filter);
				var mappingPk = new Guid("6b713ce5-a8d0-42eb-9a7a-b151324ab9a7");
				var mappingMock = mapping ?? new Mock<IDataTransferMapping>();
				if (mapping == null)
				{
					mappingMock.Setup(m => m.Name).Returns("Test mapping");
					mappingMock.Setup(m => m.ContextModule).Returns("IDummyBizo");
					mappingMock.Setup(m => m.PK).Returns(mappingPk);
					mappingMock.Setup(m => m.Delimiter).Returns(",");
					mappingMock.Setup(m => m.SheetName).Returns("fooSheet");
					mappingMock.Setup(m => m.StartingRow).Returns(1);
				}

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;

				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(mockClient.Object);

				var log = new GlowLog();

				var collectionImporterMock = new Mock<IGlowCollectionImporter>();
				collectionImporterMock.Setup(i => i.PopulateFromDataRows(collection, It.IsAny<ImportPreview[]>(), It.IsAny<MappingDataModel>(), It.IsAny<GlowLog>(), progressReporter))
					.Returns(true);

				ObjectFactory.Substitute(clientFactoryMock.Object);
				ObjectFactory.Substitute(collectionImporterMock.Object);

				using (var form = new ZForm())
				{
					var actualLogs = string.Empty;
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(showForm =>
					{
						if (showForm is GlowDataWizardImportForm)
						{
							var glowDataWizardForm = showForm as GlowDataWizardImportForm;
							actualLogs = glowDataWizardForm.Controls[0].Text;
						}
					});

					ZFormModaliser.SetApplicationActiveForm(form);
					var result = GlowDataWizardIntegration.ShowEmbeddedImportWizard(collection, mappingMock.Object);
					var actualKeyReported = ErrorReporter.LastKeyReported;
					var actualMessageReported = ErrorReporter.LastMessageReported;
					ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(form);
					AssertEquals(string.Empty, result);
					AssertEquals(expectedKeyReported, actualKeyReported);
					AssertEquals(expectedMessageReported, actualMessageReported);
					AssertEquals(expectedActualLog, actualLogs);
				}
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
				ErrorReporter.Clear();
				ObjectFactory.DisposeSubstitutions();
				OpenedFormCache.GetInstance().CloseAllCachedForms();
				DeleteIfExists(tempFilePath);
				progressReporter.Dispose();
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithNullSampleData()
		{
			using (var response = new HttpResponseMessage())
			using (response.Content = new StringContent(JsonConvert.SerializeObject(null)))
			{
				TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
					SetupClientMockWithSampleDataResponse(response),
					string.Empty,
					string.Empty,
					"Start getting data rows\n" +
					"Tried to get sample data but was null.\n" +
					"Post Request details: baseUri: https://address/, separator: ',', sheetName: 'fooSheet', Post Response details: StatusCode: OK, Content: null\n" +
					"Import has finished with errors.\n");
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithEmptyFile()
		{
			var responseObject = new ProblemDetails() { Type = ProblemType.FileIsEmpty, Detail = "The selected file contains no data." };
			using (var response = new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest })
			using (response.Content = new StringContent(JsonConvert.SerializeObject(responseObject), Encoding.UTF8, "application/problem+json"))
			{
				TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
					SetupClientMockWithSampleDataResponse(response),
					string.Empty,
					string.Empty,
					"Start getting data rows\n" +
					"Tried to get sample data but the request was unsuccessful.\n" +
					"Error when processing sample data\n" +
					"The selected file contains no data.\n" +
					"Import has finished with errors.\n",
					fileContents: "\n");
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithStrictOOXMLExcelFileNotSupported()
		{
			var responseObject = new ProblemDetails() { Type = ProblemType.StrictOOXMLExcelFileNotSupported, Detail = "This detail should not be used" };
			using (var response = new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest })
			using (response.Content = new StringContent(JsonConvert.SerializeObject(responseObject), Encoding.UTF8, "application/problem+json"))
			{
				TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
					SetupClientMockWithSampleDataResponse(response),
					string.Empty,
					string.Empty,
					"Start getting data rows\n" +
					"Tried to get sample data but the request was unsuccessful.\n" +
					"Error when processing sample data\n" +
					"Your import data file is in an unsupported Excel format. Please save your file in 'Excel Workbook (*.xlsx)' format and then try again.\n" +
					"Import has finished with errors.\n",
					fileContents: "\n");
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithZeroSizeFile()
		{
			using (var sampleDataResponse = new HttpResponseMessage())
			using (sampleDataResponse.Content = new StringContent(JsonConvert.SerializeObject(new SampleData() { Error = "The selected file contains no data." })))
			{
				TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
					SetupClientMockWithSampleDataResponse(sampleDataResponse),
					string.Empty,
					string.Empty,
					"The selected file is empty.\n" +
					"Import has finished with errors.\n",
					fileContents: string.Empty);
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithBadData()
		{
			using (var sampleDataResponse = new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest })
			{
				var responseObject = new ProblemDetails() { Type = ProblemType.BadData, Detail = "Please fix the file and try again. File contains bad data: 'ugly data'", Extensions = new Dictionary<string, object>() { { "badData", "'ugly data'" } } };
				using (sampleDataResponse.Content = new StringContent(JsonConvert.SerializeObject(responseObject), Encoding.UTF8, "application/problem+json"))
				{
					TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
						SetupClientMockWithSampleDataResponse(sampleDataResponse),
						string.Empty,
						string.Empty,
						"Start getting data rows\n" +
						"Tried to get sample data but the request was unsuccessful.\n" +
						"Error when processing sample data\n" +
						"Please fix the file and try again. File contains bad data: 'ugly data'\n" +
						"Import has finished with errors.\n");
				}
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithFileTypeNotSupported()
		{
			using (var sampleDataResponse = new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest })
			{
				var responseJson = "{\"type\":\"https://glow.wisetechglobal.com/data-import/file-type-not-supported\",\"status\":400,\"detail\":\"the detail\",\"fileType\":\".pdf\",\"supportedFileTypes\":\".xls, .xlsx, .csv, .txt\"}";
				using (sampleDataResponse.Content = new StringContent(responseJson, Encoding.UTF8, "application/problem+json"))
				{
					TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
						SetupClientMockWithSampleDataResponse(sampleDataResponse),
						string.Empty,
						string.Empty,
						"Start getting data rows\n" +
						"Tried to get sample data but the request was unsuccessful.\n" +
						"Error when processing sample data\n" +
						"File with extension \".pdf\" is not supported. Supported file types are: .xls, .xlsx, .csv, .txt\n" +
						"Import has finished with errors.\n");
				}
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithInvalidExcelSheet()
		{
			using (var sampleDataResponse = new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest })
			{
				var responseJson = "{\"type\":\"https://glow.wisetechglobal.com/data-import/file-has-invalid-excel-sheet\",\"status\":400,\"detail\":\"the detail\",\"sheetName\":\"fooSheet\"}";
				using (sampleDataResponse.Content = new StringContent(responseJson, Encoding.UTF8, "application/problem+json"))
				{
					TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
						SetupClientMockWithSampleDataResponse(sampleDataResponse),
						string.Empty,
						string.Empty,
						"Start getting data rows\n" +
						"Tried to get sample data but the request was unsuccessful.\n" +
						"Error when processing sample data\n" +
						"No Worksheet with name 'fooSheet' was found in file. Please make sure the file and mapping settings are correct.\n" +
						"Import has finished with errors.\n");
				}
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenSampleData_WithPipeDelimiter_ShouldRequestCorrectly()
			=> TestSampleDataRequestWithCorrectUri("|", "PipeSheetName", "api/datatransfer/sampledata?separator=%7C&sheetName=PipeSheetName&processFullStream=true");

		public void TestShowEmbeddedImportWizardAsync_WhenSampleData_WithColonDelimiter_ShouldRequestCorrectly()
			=> TestSampleDataRequestWithCorrectUri(":", "ColonSheetName", "api/datatransfer/sampledata?separator=%3A&sheetName=ColonSheetName&processFullStream=true");

		public void TestShowEmbeddedImportWizardAsync_WhenSampleData_WithSemicolonDelimiter_ShouldRequestCorrectly()
			=> TestSampleDataRequestWithCorrectUri(";", "SemicolonSheetName", "api/datatransfer/sampledata?separator=%3B&sheetName=SemicolonSheetName&processFullStream=true");

		public void TestShowEmbeddedImportWizardAsync_WhenSampleData_WithTildeDelimiter_ShouldRequestCorrectly()
			=> TestSampleDataRequestWithCorrectUri("~", "TildeSheetName", "api/datatransfer/sampledata?separator=%7E&sheetName=TildeSheetName&processFullStream=true");

		public void TestShowEmbeddedImportWizardAsync_WhenSampleData_WithSpaceDelimiter_ShouldRequestCorrectly()
			=> TestSampleDataRequestWithCorrectUri(" ", "SpaceSheetName", "api/datatransfer/sampledata?separator=+&sheetName=SpaceSheetName&processFullStream=true");

		public void TestShowEmbeddedImportWizardAsync_WhenSampleData_WithTabDelimiter_ShouldRequestCorrectly()
			=> TestSampleDataRequestWithCorrectUri("	", "TabSheetName", "api/datatransfer/sampledata?separator=%09&sheetName=TabSheetName&processFullStream=true");

		public void TestShowEmbeddedImportWizardAsync_WhenSampleData_GivenNoSheetName_ShouldRequestCorrectly()
			=> TestSampleDataRequestWithCorrectUri(",", "", "api/datatransfer/sampledata?separator=%2C&processFullStream=true");

		public void TestShowEmbeddedImportWizardAsync_WhenSampleData_GivenSheetNameWithSpaces_ShouldRequestCorrectly()
			=> TestSampleDataRequestWithCorrectUri(",", "Sheet name with spaces", "api/datatransfer/sampledata?separator=%2C&sheetName=Sheet+name+with+spaces&processFullStream=true");

		void TestSampleDataRequestWithCorrectUri(string delimiter, string sheetName, string expectedUri)
		{
			var mappingMock = new Mock<IDataTransferMapping>();
			mappingMock.Setup(m => m.PK).Returns(new Guid("6b713ce5-a8d0-42eb-9a7a-b151324ab9a7"));
			mappingMock.Setup(m => m.Name).Returns("Test mapping");
			mappingMock.Setup(m => m.ContextModule).Returns("IDummyBizo");
			mappingMock.Setup(m => m.Delimiter).Returns(delimiter);
			mappingMock.Setup(m => m.SheetName).Returns(sheetName);
			mappingMock.Setup(m => m.StartingRow).Returns(1);

			var previewData = new StringContent(JsonConvert.SerializeObject(new[]
			{
				new ImportPreview(Enumerable.Empty<ImportPreviewHeader>(), new ImportPreviewLine("", 0, Enumerable.Empty<ImportPreviewLineDetails>()))
			}));

			using (var sampleDataResponse = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK })
			using (sampleDataResponse.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool> { { "IDummyBizo", false } })))
			using (var previewDataResponse = new HttpResponseMessage())
			using (previewDataResponse.Content = previewData)
			using (var mappingDataResponse = new HttpResponseMessage())
			using (mappingDataResponse.Content = new StringContent(JsonConvert.SerializeObject(null)))
			{
				var actualUri = string.Empty;
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock
					.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
					.Callback<string, HttpContent>((uri, content) =>
					{
						actualUri = uri;
					})
					.ReturnsAsync(sampleDataResponse);
				clientMock
					.Setup(c => c.PostAsJsonAsync(It.IsAny<string>(), It.IsAny<IEnumerable<SampleDataLine>>()))
					.Returns(Task.FromResult(previewDataResponse));
				clientMock.Setup(c => c.GetAsync(It.IsAny<string>()))
					.ReturnsAsync(mappingDataResponse);

				TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
					clientMock,
					string.Empty,
					string.Empty,
					"Start getting data rows\n" +
					"Finish getting data rows\n" +
					"Start getting mapping data model\n" +
					"Tried to get mapping data model but returned null.\n" +
					"Get Request details: baseUri: https://address/, mappingPK: 6b713ce5-a8d0-42eb-9a7a-b151324ab9a7, dataDefinitionName: IDummyDependentBizo, Get Response details: StatusCode: OK, Content: null\n" +
					"Import has finished with errors.\n",
					$"data1{delimiter}data2{delimiter}data3",
					mappingMock);

				AssertEquals(expectedUri, actualUri);
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithMissingSeparatorError()
		{
			TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithoutReportCore(ProblemType.MissingSeparatorError, "A separator must be provided.");
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithFileIsEncrypted()
		{
			TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithoutReportCore(ProblemType.FileIsEncrypted, "The selected Excel file is encrypted with a password and cannot be read. Please remove the password before importing.");
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithFileIsInvalidExcel()
		{
			TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithoutReportCore(ProblemType.FileIsInvalidExcel, "The selected Excel file is invalid or corrupted.");
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithFileTooOld()
		{
			TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithoutReportCore(ProblemType.FileTooOld, "The selected Excel file version is not supported. Please select an Excel file with version 97/2000/XP/2003 or superior.");
		}

		void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithoutReportCore(string type, string message)
		{
			using (var sampleDataResponse = new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest })
			{
				var responseObject = new ProblemDetails() { Type = type };
				using (sampleDataResponse.Content = new StringContent(JsonConvert.SerializeObject(responseObject), Encoding.UTF8, "application/problem+json"))
				{
					TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
						SetupClientMockWithSampleDataResponse(sampleDataResponse),
						string.Empty,
						string.Empty,
						"Start getting data rows\n" +
						"Tried to get sample data but the request was unsuccessful.\n" +
						"Error when processing sample data\n" +
						$"{message}\n" +
						"Import has finished with errors.\n");
				}
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithMissingContentError()
		{
			TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsAndReportsCore(ProblemType.MissingContentError, "Request must have multipart content.");
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithUnhandledProblemType()
		{
			TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsAndReportsCore("UnhandledType", "Some unhandled problem message");
		}

		void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsAndReportsCore(string type, string message)
		{
			using (var sampleDataResponse = new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest })
			{
				var responseObject = new ProblemDetails() { Type = type, Detail = message };
				using (sampleDataResponse.Content = new StringContent(JsonConvert.SerializeObject(responseObject), Encoding.UTF8, "application/problem+json"))
				{
					TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
						SetupClientMockWithSampleDataResponse(sampleDataResponse),
						"GlowImportWizard_FailedImport",
						"Start getting data rows\r\n" +
						"Tried to get sample data but the request was unsuccessful.\r\n" +
						"Error when processing sample data\r\n" +
						$"{message}\r\n" +
						"An error report of the problem has been sent to CargoWise.",
						"Start getting data rows\n" +
						"Tried to get sample data but the request was unsuccessful.\n" +
						"Error when processing sample data\n" +
						$"{message}\n" +
						"An error report of the problem has been sent to CargoWise.\n");
				}
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithServerTransientError()
		{
			using (var response = new HttpResponseMessage() { StatusCode = HttpStatusCode.InternalServerError })
			using (response.Content = new StringContent("Some Error occured"))
			{
				TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
					SetupClientMockWithSampleDataResponse(response),
					string.Empty,
					string.Empty,
					"Start getting data rows\n" +
					"Tried to get sample data but the request was unsuccessful.\n" +
					"Post Request details: baseUri: https://address/, separator: ',', sheetName: 'fooSheet', Post Response details: StatusCode: InternalServerError, Content: Some Error occured\n" +
					"Import has finished with errors.\n");
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithGlowHttpRequestExceptionWithTransientStatus()
		{
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.PostAsync("api/datatransfer/sampledata?separator=%2C&sheetName=fooSheet&processFullStream=true", It.IsAny<HttpContent>()))
				.ThrowsAsync(new GlowHttpRequestException("Network exception", HttpStatusCode.BadGateway));

			TestShowEmbeddedImportWizardAsync_WhenGetImportContent(clientMock, string.Empty, string.Empty, "Start getting data rows\n" +
				"Network exception (Code: 502)\n" +
				"Import has finished with errors.\n");
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithGlowHttpRequestExceptionWithNotTransientStatus()
		{
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.PostAsync("api/datatransfer/sampledata?separator=%2C&sheetName=fooSheet&processFullStream=true", It.IsAny<HttpContent>()))
				.ThrowsAsync(new GlowHttpRequestException("Network exception", HttpStatusCode.BadRequest));

			TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
				clientMock,
				"GlowImportWizard_FailedImport",
				"Start getting data rows\r\n" +
				"Network exception (Code: 400)\r\n" +
				"An error report of the problem has been sent to CargoWise.",
				"Start getting data rows\n" +
				"Network exception (Code: 400)\n" +
				"An error report of the problem has been sent to CargoWise.\n");
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithAuthorizationFailureException_LogonDetailsIncorrect()
			=> TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithAuthorizationFailureException(AuthenticationResult.LogonDetailsIncorrect);

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithAuthorizationFailureException_PasswordChangeRequired()
			=> TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithAuthorizationFailureException(AuthenticationResult.PasswordChangeRequired);

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithAuthorizationFailureException_LoginDisabled()
			=> TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithAuthorizationFailureException(AuthenticationResult.LoginDisabled);

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithAuthorizationFailureException_AccountLocked()
			=> TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithAuthorizationFailureException(AuthenticationResult.AccountLocked);

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithAuthorizationFailureException_ContextChangeRequired()
			=> TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithAuthorizationFailureException(AuthenticationResult.ContextChangeRequired);

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithAuthorizationFailureException_WithReportableResult()
			=> TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithAuthorizationFailureException(AuthenticationResult.WebAccessNotEnabled, shouldReport: true);

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithAuthorizationFailureException(
			AuthenticationResult authenticationResult,
			bool shouldReport = false)
		{
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock
				.Setup(c => c.PostAsync($"api/datatransfer/sampledata?separator=%2C&processFullStream=true", It.IsAny<HttpContent>()))
				.ThrowsAsync(new AuthorizationFailureException(authenticationResult));

			ExceptionReporterTestListener.Instance.Clear();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var tempFilePath = string.Empty;
			try
			{
				tempFilePath = CreateFileWithData("Some data");
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock
					.Setup(f => f.Create(new Uri("https://address/")))
					.Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);

				using (var form = new ZForm())
				{
					var actualLogs = string.Empty;
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(showForm =>
					{
						if (showForm is GlowDataWizardImportForm)
						{
							var glowDataWizardForm = showForm as GlowDataWizardImportForm;
							actualLogs = glowDataWizardForm.Controls[0].Text;
						}
					});
					ZFormModaliser.SetApplicationActiveForm(form);

					var mappingMock = new Mock<IDataTransferMapping>();
					mappingMock.Setup(m => m.Delimiter).Returns(",");

					var result = GlowDataWizardIntegration.ShowEmbeddedImportWizard(new Mock<IBusinessObjectCollection>().Object, mappingMock.Object);

					var expectedMessage = @"Changes to your credentials have occurred since the last login.
In order to use Advanced Data Automation Wizard, please logout first and login again.";
					AssertEquals(string.Empty, result);
					AssertEquals(shouldReport ? "GlowImportWizard_FailedImport" : string.Empty, ErrorReporter.LastKeyReported);
					AssertEquals(shouldReport ? null : expectedMessage, ((UnitTestUserNotification)Globals.Message).LastMessage.Text);

					var authenticationResultString = Enum.GetName(typeof(AuthenticationResult), authenticationResult);
					var logResultString = shouldReport ? "An error report of the problem has been sent to CargoWise." : "Import has finished with errors.";
					var expectedMessageReported = $"Start getting data rowsUnexpected authentication result: {authenticationResultString}{logResultString}";
					AssertEquals(shouldReport ? expectedMessageReported : string.Empty, ErrorReporter.LastMessageReported.Replace("\n", "").Replace("\r", ""));
					AssertEquals(expectedMessageReported, actualLogs.Replace("\n", "").Replace("\r", ""));

					ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(form);
				}
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
				ErrorReporter.Clear();
				ObjectFactory.DisposeSubstitutions();
				OpenedFormCache.GetInstance().CloseAllCachedForms();
				DeleteIfExists(tempFilePath);
				progressReporter.Dispose();
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithServerNotTransientError()
		{
			using (var response = new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest })
			{
				TestShowEmbeddedImportWizardAsync_WhenGetImportContent(
					SetupClientMockWithSampleDataResponse(response),
					"GlowImportWizard_FailedImport",
					"Start getting data rows\r\n" +
					"Tried to get sample data but the request was unsuccessful.\r\n" +
					"Post Request details: baseUri: https://address/, separator: ',', sheetName: 'fooSheet', Post Response details: StatusCode: BadRequest, Content: \r\n" +
					"An error report of the problem has been sent to CargoWise.",
					"Start getting data rows\n" +
					"Tried to get sample data but the request was unsuccessful.\n" +
					"Post Request details: baseUri: https://address/, separator: ',', sheetName: 'fooSheet', Post Response details: StatusCode: BadRequest, Content: \n" +
					"An error report of the problem has been sent to CargoWise.\n");
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetImportContent_FailsWithRandomException_ItShouldBubbleUp()
		{
			ExceptionReporterTestListener.Instance.Clear();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var tempFilePath = string.Empty;
			try
			{
				tempFilePath = CreateFileWithData("data");
				var businessEntity = Factory.New<DummyBusinessObject>();
				Factory.Save();

				var filter = new ZQuery(DummyDependentBizoSchema.ZD1_Z0, businessEntity.PK);
				var collection = new TestCollection<DummyDependantBusinessObject>(filter);
				var mappingPk = Guid.NewGuid();
				var mappingMock = new Mock<IDataTransferMapping>();
				mappingMock.Setup(m => m.Name).Returns("Test mapping");
				mappingMock.Setup(m => m.ContextModule).Returns("IDummyBizo");
				mappingMock.Setup(m => m.PK).Returns(mappingPk);
				mappingMock.Setup(m => m.Delimiter).Returns(",");
				mappingMock.Setup(m => m.StartingRow).Returns(1);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;

				var clientMock = new Mock<IGlowServiceClient>();

				clientMock.Setup(c => c.PostAsync($"api/datatransfer/sampledata?separator=%2C&processFullStream=true", It.IsAny<HttpContent>()))
					.ThrowsAsync(new ArgumentNullException("Argument"));

				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);

				var collectionImporterMock = new Mock<IGlowCollectionImporter>();
				var log = new GlowLog();

				collectionImporterMock.Setup(i => i.PopulateFromDataRows(collection, It.IsAny<ImportPreview[]>(), It.IsAny<MappingDataModel>(), It.IsAny<GlowLog>(), progressReporter))
					.Returns(true);

				ObjectFactory.Substitute(clientFactoryMock.Object);
				ObjectFactory.Substitute(collectionImporterMock.Object);

				using (var form = new ZForm())
				{
					var actualLogs = string.Empty;
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(showForm =>
					{
						if (showForm is GlowDataWizardImportForm)
						{
							var glowDataWizardForm = showForm as GlowDataWizardImportForm;
							actualLogs = glowDataWizardForm.Controls[0].Text;
						}
					});

					ZFormModaliser.SetApplicationActiveForm(form);
					AssertExceptionThrown<ArgumentNullException>("Argument", () => GlowDataWizardIntegration.ShowEmbeddedImportWizard(collection, mappingMock.Object));
					AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
					AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
					AssertEquals(string.Empty, actualLogs);
					ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(form);
				}
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
				ErrorReporter.Clear();
				ObjectFactory.DisposeSubstitutions();
				OpenedFormCache.GetInstance().CloseAllCachedForms();
				DeleteIfExists(tempFilePath);
				progressReporter.Dispose();
			}
		}

		void TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel(Mock<IGlowServiceClient> mockClient, string expectedKeyReported, string expectedMessageReported, string expectedActualLog)
		{
			ExceptionReporterTestListener.Instance.Clear();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var tempFilePath = string.Empty;
			try
			{
				tempFilePath = CreateFileWithData("data");
				var businessEntity = Factory.New<DummyBusinessObject>();
				Factory.Save();

				var filter = new ZQuery(DummyDependentBizoSchema.ZD1_Z0, businessEntity.PK);
				var collection = new TestCollection<DummyDependantBusinessObject>(filter);
				var mappingPk = new Guid("6b713ce5-a8d0-42eb-9a7a-b151324ab9a7");
				var mappingMock = new Mock<IDataTransferMapping>();
				mappingMock.Setup(m => m.Name).Returns("Test mapping");
				mappingMock.Setup(m => m.ContextModule).Returns("IDummyBizo");
				mappingMock.Setup(m => m.PK).Returns(mappingPk);
				mappingMock.Setup(m => m.Delimiter).Returns(",");
				mappingMock.Setup(m => m.StartingRow).Returns(1);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;

				using (var sampleDataResponse = new HttpResponseMessage())
				using (sampleDataResponse.Content = new StringContent(JsonConvert.SerializeObject(new SampleData())))
				using (var response = new HttpResponseMessage())
				using (response.Content = new StringContent(JsonConvert.SerializeObject(new[] { new ImportPreview(Enumerable.Empty<ImportPreviewHeader>(), new ImportPreviewLine("", 0, Enumerable.Empty<ImportPreviewLineDetails>())) })))
				{
					mockClient.Setup(c => c.PostAsync($"api/datatransfer/sampledata?separator=%2C&processFullStream=true", It.IsAny<HttpContent>()))
						.Returns(Task.FromResult(sampleDataResponse));
					mockClient.Setup(c => c.PostAsJsonAsync($"api/datatransfer/createpreview?mappingPK={mappingPk}", It.IsAny<IEnumerable<SampleDataLine>>()))
						.Returns(Task.FromResult(response));

					var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
					clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(mockClient.Object);

					var collectionImporterMock = new Mock<IGlowCollectionImporter>();
					var log = new GlowLog();

					collectionImporterMock.Setup(i => i.PopulateFromDataRows(collection, It.IsAny<ImportPreview[]>(), It.IsAny<MappingDataModel>(), It.IsAny<GlowLog>(), progressReporter))
						.Returns(true);

					ObjectFactory.Substitute(clientFactoryMock.Object);
					ObjectFactory.Substitute(collectionImporterMock.Object);

					using (var form = new ZForm())
					{
						var actualLogs = string.Empty;
						ZFormModaliser.ShowDialogsInTest = true;
						ZFormModaliser.SetDelegateToCallOnFormShown(showForm =>
						{
							if (showForm is GlowDataWizardImportForm)
							{
								var glowDataWizardForm = showForm as GlowDataWizardImportForm;
								actualLogs = glowDataWizardForm.Controls[0].Text;
							}
						});

						ZFormModaliser.SetApplicationActiveForm(form);
						var result = GlowDataWizardIntegration.ShowEmbeddedImportWizard(collection, mappingMock.Object);
						AssertEquals(string.Empty, result);
						var actualKeyReported = ErrorReporter.LastKeyReported;
						var actualMessageReported = ErrorReporter.LastMessageReported;
						ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(form);
						AssertEquals(string.Empty, result);
						AssertEquals(expectedKeyReported, actualKeyReported);
						AssertEquals(expectedMessageReported, actualMessageReported);
						AssertEquals(expectedActualLog, actualLogs);
					}
				}
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
				ErrorReporter.Clear();
				ObjectFactory.DisposeSubstitutions();
				OpenedFormCache.GetInstance().CloseAllCachedForms();
				DeleteIfExists(tempFilePath);
				progressReporter.Dispose();
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel_FailsWithNullMappingDataModel()
		{
			using (var response = new HttpResponseMessage())
			using (response.Content = new StringContent(JsonConvert.SerializeObject(null)))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync(It.IsAny<string>()))
					.ReturnsAsync(response);

				TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel(
					clientMock,
					string.Empty,
					string.Empty,
					"Start getting data rows\n" +
					"Finish getting data rows\n" +
					"Start getting mapping data model\n" +
					"Tried to get mapping data model but returned null.\n" +
					"Get Request details: baseUri: https://address/, mappingPK: 6b713ce5-a8d0-42eb-9a7a-b151324ab9a7, dataDefinitionName: IDummyDependentBizo, Get Response details: StatusCode: OK, Content: null\n" +
					"Import has finished with errors.\n");
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel_FailsWithServerTransientError()
		{
			using (var response = new HttpResponseMessage() { StatusCode = HttpStatusCode.InternalServerError })
			using (response.Content = new StringContent("Some Error occured"))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync(It.IsAny<string>()))
					.ReturnsAsync(response);

				TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel(
					clientMock,
					string.Empty,
					string.Empty,
					"Start getting data rows\n" +
					"Finish getting data rows\n" +
					"Start getting mapping data model\n" +
					"Tried to get mapping data model but the request was unsuccessful.\n" +
					"Get Request details: baseUri: https://address/, mappingPK: 6b713ce5-a8d0-42eb-9a7a-b151324ab9a7, dataDefinitionName: IDummyDependentBizo, Get Response details: StatusCode: InternalServerError, Content: Some Error occured\n" +
					"Import has finished with errors.\n");
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel_FailsWithGlowHttpRequestExceptionWithTransientStatus()
		{
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.GetAsync(It.IsAny<string>()))
				.ThrowsAsync(new GlowHttpRequestException("Network exception", HttpStatusCode.BadGateway));

			TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel(clientMock, string.Empty, string.Empty, "Start getting data rows\n" +
				"Finish getting data rows\n" +
				"Start getting mapping data model\n" +
				"Network exception (Code: 502)\n" +
				"Import has finished with errors.\n");
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel_FailsWithGlowHttpRequestExceptionWithNotTransientStatus()
		{
			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.GetAsync(It.IsAny<string>()))
				.ThrowsAsync(new GlowHttpRequestException("Network exception", HttpStatusCode.BadRequest));

			TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel(
				clientMock,
				"GlowImportWizard_FailedImport",
				"Start getting data rows\r\n" +
				"Finish getting data rows\r\n" +
				"Start getting mapping data model\r\n" +
				"Network exception (Code: 400)\r\n" +
				"An error report of the problem has been sent to CargoWise.",
				"Start getting data rows\n" +
				"Finish getting data rows\n" +
				"Start getting mapping data model\n" +
				"Network exception (Code: 400)\n" +
				"An error report of the problem has been sent to CargoWise.\n");
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel_FailsWithServerNotTransientError()
		{
			using (var response = new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest })
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync(It.IsAny<string>()))
					.ReturnsAsync(response);

				TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel(
					clientMock,
					"GlowImportWizard_FailedImport",
					"Start getting data rows\r\n" +
					"Finish getting data rows\r\n" +
					"Start getting mapping data model\r\n" +
					"Tried to get mapping data model but the request was unsuccessful.\r\n" +
					"Get Request details: baseUri: https://address/, mappingPK: 6b713ce5-a8d0-42eb-9a7a-b151324ab9a7, dataDefinitionName: IDummyDependentBizo, Get Response details: StatusCode: BadRequest, Content: \r\n" +
					"An error report of the problem has been sent to CargoWise.",
					"Start getting data rows\n" +
					"Finish getting data rows\n" +
					"Start getting mapping data model\n" +
					"Tried to get mapping data model but the request was unsuccessful.\n" +
					"Get Request details: baseUri: https://address/, mappingPK: 6b713ce5-a8d0-42eb-9a7a-b151324ab9a7, dataDefinitionName: IDummyDependentBizo, Get Response details: StatusCode: BadRequest, Content: \n" +
					"An error report of the problem has been sent to CargoWise.\n");
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel_FailsWithRandomException_ItShouldBubbleUp()
		{
			ExceptionReporterTestListener.Instance.Clear();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var tempFilePath = string.Empty;
			try
			{
				tempFilePath = CreateFileWithData("data");
				var businessEntity = Factory.New<DummyBusinessObject>();
				Factory.Save();

				var filter = new ZQuery(DummyDependentBizoSchema.ZD1_Z0, businessEntity.PK);
				var collection = new TestCollection<DummyDependantBusinessObject>(filter);
				var mappingPk = Guid.NewGuid();
				var mappingMock = new Mock<IDataTransferMapping>();
				mappingMock.Setup(m => m.Name).Returns("Test mapping");
				mappingMock.Setup(m => m.ContextModule).Returns("IDummyBizo");
				mappingMock.Setup(m => m.PK).Returns(mappingPk);
				mappingMock.Setup(m => m.Delimiter).Returns(",");
				mappingMock.Setup(m => m.StartingRow).Returns(1);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;

				var clientMock = new Mock<IGlowServiceClient>();

				using (var sampleDataResponse = new HttpResponseMessage())
				using (sampleDataResponse.Content = new StringContent(JsonConvert.SerializeObject(new SampleData())))
				using (var response = new HttpResponseMessage())
				using (response.Content = new StringContent(JsonConvert.SerializeObject(new[] { new ImportPreview(Enumerable.Empty<ImportPreviewHeader>(), new ImportPreviewLine("", 0, Enumerable.Empty<ImportPreviewLineDetails>())) })))
				{
					clientMock.Setup(c => c.PostAsync($"api/datatransfer/sampledata?separator=%2C&processFullStream=true", It.IsAny<HttpContent>()))
						.Returns(Task.FromResult(sampleDataResponse));
					clientMock.Setup(c => c.PostAsJsonAsync($"api/datatransfer/createpreview?mappingPK={mappingPk}", It.IsAny<IEnumerable<SampleDataLine>>()))
						.Returns(Task.FromResult(response));
					clientMock.Setup(c => c.GetAsync(It.IsAny<string>()))
						.ThrowsAsync(new ArgumentNullException("Argument"));

					var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
					clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);

					var log = new GlowLog();

					var collectionImporterMock = new Mock<IGlowCollectionImporter>();
					collectionImporterMock.Setup(i => i.PopulateFromDataRows(collection, It.IsAny<ImportPreview[]>(), It.IsAny<MappingDataModel>(), It.IsAny<GlowLog>(), progressReporter))
						.Returns(true);

					ObjectFactory.Substitute(clientFactoryMock.Object);
					ObjectFactory.Substitute(collectionImporterMock.Object);

					using (var form = new ZForm())
					{
						var actualLogs = string.Empty;
						ZFormModaliser.ShowDialogsInTest = true;
						ZFormModaliser.SetDelegateToCallOnFormShown(showForm =>
						{
							if (showForm is GlowDataWizardImportForm)
							{
								var glowDataWizardForm = showForm as GlowDataWizardImportForm;
								actualLogs = glowDataWizardForm.Controls[0].Text;
							}
						});

						ZFormModaliser.SetApplicationActiveForm(form);
						AssertExceptionThrown<ArgumentNullException>("Argument", () => GlowDataWizardIntegration.ShowEmbeddedImportWizard(collection, mappingMock.Object));
						AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
						AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
						AssertEquals(string.Empty, actualLogs);
						ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(form);
					}
				}
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
				ErrorReporter.Clear();
				ObjectFactory.DisposeSubstitutions();
				OpenedFormCache.GetInstance().CloseAllCachedForms();
				DeleteIfExists(tempFilePath);
				progressReporter.Dispose();
			}
		}

		public void TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel_FailsWithProblem_InvalidMappingTargetColumn()
		{
			var extensions = new Dictionary<string, object>();
			extensions.Add("entity", "IJobShipment");
			extensions.Add("path", "FooRelated");
			var responseObject = new ProblemDetails() { Type = ProblemType.InvalidMappingTargetColumn, Detail = "Invalid Target.", Extensions = extensions };
			using (var response = new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest })
			using (response.Content = new StringContent(JsonConvert.SerializeObject(responseObject), Encoding.UTF8, "application/problem+json"))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock
					.Setup(c => c.GetAsync("api/datatransfer/MappingDataModel?dataDefinitionName=IDummyDependentBizo&mappingPK=6b713ce5-a8d0-42eb-9a7a-b151324ab9a7"))
					.ReturnsAsync(response);

				TestShowEmbeddedImportWizardAsync_WhenGetMappingDataModel(
					clientMock,
					string.Empty,
					string.Empty,
					"Start getting data rows\n" +
					"Finish getting data rows\n" +
					"Start getting mapping data model\n" +
					"Shipment does not have an associated property called \"FooRelated\"\n" +
					"Import has finished with errors.\n");
			}
		}

		static string CreateFileWithData(string fileContents, string fileExtension = "csv")
		{
			var tempFilePath = Temp.GetTempFileNameWithExtension(fileExtension);
			if (!string.IsNullOrEmpty(fileContents))
			{
				var writer = new StreamWriter(new FileStream(tempFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 4096));
				writer.Write(fileContents);
				writer.Close();
			}

			return tempFilePath;
		}

		public void TestHandleADAWMenuItemPopup_ImportableIsFalse()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();

			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var response = new HttpResponseMessage(HttpStatusCode.OK))
			using (response.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool> { { "IDummyBizo", false } })))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ReturnsAsync(response);

				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);

				using (var parentMenuItem = new ZMenuItem())
				using (var siblingMenuItem = new ZMenuItem())
				using (var menuItem = new ZMenuItem((NoResString)"Advanced Data Automation Wizard"))
				{
					parentMenuItem.MenuItems.AddRange(new[] { siblingMenuItem, menuItem });

					GlowDataWizardIntegration.HandleADAWMenuItemPopup(menuItem, typeof(DummyBusinessObject), _ => { }, _ => { });

					AssertEquals(0, menuItem.MenuItems.Count);
					AssertEquals("Advanced Data Automation Wizard (Unavailable)", menuItem.Caption.ToString());
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

					menuItem.PerformClick();
					AssertEquals("Importing is not available for this data type.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHandleADAWMenuItemPopup_CWSupportUser()
		{
			tempContext.Dispose();
			tempContext = null;

			using (var menuItem = new ZMenuItem((NoResString)"Advanced Data Automation Wizard"))
			{
				GlowDataWizardIntegration.HandleADAWMenuItemPopup(menuItem, typeof(DummyBusinessObject), _ => { }, _ => { });

				AssertEquals(0, menuItem.MenuItems.Count);
				AssertEquals("Advanced Data Automation Wizard (Unavailable for CW1 Support)", menuItem.Caption.ToString());
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

				var expectedMessage = @"The CW1 Support login cannot be used when interacting with the Advanced Data Automation Wizard. Please login as an operational user in order to use this feature.";
				menuItem.PerformClick();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleADAWMenuItemPopup_MetadataIsMissing()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();

			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var response = new HttpResponseMessage(HttpStatusCode.OK))
			using (response.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, bool> { { "IEntity", false } })))
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).ReturnsAsync(response);

				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);

				using (var menuItem = new ZMenuItem((NoResString)"Advanced Data Automation Wizard"))
				{
					GlowDataWizardIntegration.HandleADAWMenuItemPopup(menuItem, typeof(DummyBusinessObject), _ => { }, _ => { });

					AssertEquals(0, menuItem.MenuItems.Count);
					AssertEquals("Advanced Data Automation Wizard (Currently Unsupported)", menuItem.Caption.ToString());
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

					var expectedMessage = @"Importing is currently not available for this data type. 
Please raise a CR9 for the relevant Product for this grid with examples of the data to be imported (you can press the F1 key to open a new eRequest).";
					menuItem.PerformClick();
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHandleADAWMenuItemPopup_ServiceIsNotConfigured()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();

			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var clientMock = new Mock<IGlowServiceClient>();
			clientMock.Setup(c => c.GetAsync("api/datatransfer/datadefinitionnames")).Throws(new GlowHttpRequestException("some error", HttpStatusCode.ServiceUnavailable));

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);

			using (var menuItem = new ZMenuItem((NoResString)"Advanced Data Automation Wizard"))
			{
				GlowDataWizardIntegration.HandleADAWMenuItemPopup(menuItem, typeof(DummyBusinessObject), _ => { }, _ => { });

				AssertEquals(0, menuItem.MenuItems.Count);
				AssertEquals("Advanced Data Automation Wizard (Currently Unavailable)", menuItem.Caption.ToString());
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

				var expectedMessage = @"Advanced Data Automation Wizard is currently not available.
Please contact your CW1 administrator to check that configuration is correct or contact CW1 Support if this error persists.";
				menuItem.PerformClick();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleADAWMenuItemPopup_ManageMappingsMenuWithNoImportMappings()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();

			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var menuItem = new ZMenuItem())
			{
				var mockDelegate = new Mock<Action<IDataTransferMapping>>();
				GlowDataWizardIntegration.HandleADAWMenuItemPopup(menuItem, typeof(DummyBusinessObject), _ => { }, mockDelegate.Object);

				AssertEquals(1, menuItem.MenuItems.Count);

				var manageMappingsMenuItem = menuItem.MenuItems[0];
				AssertEquals(manageMappingsMenuItem.Text, "Manage Mappings");

				var manageMappingMenuItems = manageMappingsMenuItem.MenuItems;
				AssertEquals(manageMappingMenuItems.Count, 1);
				AssertEquals(manageMappingMenuItems[0].Text, "New Mapping");

				manageMappingMenuItems[0].PerformClick();
				mockDelegate.Verify(d => d(It.IsAny<IDataTransferMapping>()), Times.Once);
				mockDelegate.Verify(d => d(null));
			}
		}

		public void TestHandleADAWMenuItemPopup_ManageMappingsMenuWithImportMappings()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();

			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var mapping1 = Factory.New<StmModuleFilter>();
			mapping1.S9_ModuleID = "GLOWDataImportV2_IDummyBizo";
			mapping1.S9_FilterName = "test mapping 1";
			mapping1.S9_RelatedEntityID = Guid.Empty;

			var mapping2 = Factory.New<StmModuleFilter>();
			mapping2.S9_ModuleID = "GLOWDataImportV2_IDummyBizo";
			mapping2.S9_FilterName = "test mapping 2";
			mapping2.S9_RelatedEntityID = Guid.Empty;

			Factory.Save();

			var parserMock = new Mock<IDataTransferMappingParser>();
			parserMock.Setup(p => p.FromModuleFilterInfo(Moq.It.IsAny<IModuleFilterInfo>())).Returns((IModuleFilterInfo filterInfo) =>
			{
				var mappingMock = new Mock<IDataTransferMapping>();
				mappingMock.Setup(m => m.Name).Returns(filterInfo.FilterName);
				mappingMock.Setup(m => m.PK).Returns(filterInfo.PK);
				return mappingMock.Object;
			});
			ObjectFactory.Substitute(parserMock.Object);

			using (var menuItem = new ZMenuItem())
			{
				var mockDelegate = new Mock<Action<IDataTransferMapping>>();
				GlowDataWizardIntegration.HandleADAWMenuItemPopup(menuItem, typeof(DummyBusinessObject), _ => { }, mockDelegate.Object);
				AssertEquals(2, menuItem.MenuItems.Count);

				var manageMappingsMenuItem = menuItem.MenuItems[1];
				AssertEquals("Manage Mappings", manageMappingsMenuItem.Text);
				AssertEquals(3, manageMappingsMenuItem.MenuItems.Count);
				AssertEquals("New Mapping", manageMappingsMenuItem.MenuItems[0].Text);
				AssertNotNull(manageMappingsMenuItem.MenuItems.FindByText("test mapping 1"));
				AssertNotNull(manageMappingsMenuItem.MenuItems.FindByText("test mapping 2"));

				manageMappingsMenuItem.MenuItems.FindByText("test mapping 1").PerformClick();
				mockDelegate.Verify(d => d(It.Is<IDataTransferMapping>(x => x.PK == mapping1.PK)));
			}
		}

		public void TestHandleADAWMenuItemPopup_ImportFileMenu()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var mapping1 = Factory.New<StmModuleFilter>();
			mapping1.S9_ModuleID = "GLOWDataImportV2_IDummyBizo";
			mapping1.S9_FilterName = "test mapping 1";
			mapping1.S9_RelatedEntityID = Guid.Empty;

			var mapping2 = Factory.New<StmModuleFilter>();
			mapping2.S9_ModuleID = "GLOWDataImportV2_IDummyBizo";
			mapping2.S9_FilterName = "test mapping 2";
			mapping2.S9_RelatedEntityID = Guid.Empty;

			Factory.Save();

			var parserMock = new Mock<IDataTransferMappingParser>();
			parserMock.Setup(p => p.FromModuleFilterInfo(Moq.It.IsAny<IModuleFilterInfo>())).Returns((IModuleFilterInfo filterInfo) =>
			{
				var mappingMock = new Mock<IDataTransferMapping>();
				mappingMock.Setup(m => m.Name).Returns(filterInfo.FilterName);
				mappingMock.Setup(m => m.PK).Returns(filterInfo.PK);
				return mappingMock.Object;
			});
			ObjectFactory.Substitute(parserMock.Object);

			using (var menuItem = new ZMenuItem())
			{
				var mockDelegate = new Mock<Action<IDataTransferMapping>>();
				GlowDataWizardIntegration.HandleADAWMenuItemPopup(menuItem, typeof(DummyBusinessObject), mockDelegate.Object, _ => { });
				AssertEquals(2, menuItem.MenuItems.Count);

				var importMenuItem = menuItem.MenuItems[0];
				AssertEquals("Import File Using", importMenuItem.Text);
				AssertEquals(2, importMenuItem.MenuItems.Count);
				AssertNotNull(importMenuItem.MenuItems.FindByText("test mapping 1"));
				AssertNotNull(importMenuItem.MenuItems.FindByText("test mapping 2"));

				importMenuItem.MenuItems.FindByText("test mapping 1").PerformClick();
				mockDelegate.Verify(d => d(It.Is<IDataTransferMapping>(x => x.PK == mapping1.PK)));
			}
		}

		public void TestHandleADAWMenuItemPopup_CalledTwice()
		{
			GlowDataWizardIntegration.ResetImportableDataDefinitionNames();

			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var mapping1 = Factory.New<StmModuleFilter>();
			mapping1.S9_ModuleID = "GLOWDataImportV2_IDummyBizo";
			mapping1.S9_FilterName = "test mapping 1";
			mapping1.S9_RelatedEntityID = Guid.Empty;

			var mapping2 = Factory.New<StmModuleFilter>();
			mapping2.S9_ModuleID = "GLOWDataImportV2_IDummyBizo";
			mapping2.S9_FilterName = "test mapping 2";
			mapping2.S9_RelatedEntityID = Guid.Empty;

			Factory.Save();

			var parserMock = new Mock<IDataTransferMappingParser>();
			parserMock.Setup(p => p.FromModuleFilterInfo(Moq.It.IsAny<IModuleFilterInfo>())).Returns((IModuleFilterInfo filterInfo) =>
			{
				var mappingMock = new Mock<IDataTransferMapping>();
				mappingMock.Setup(m => m.Name).Returns(filterInfo.FilterName);
				mappingMock.Setup(m => m.PK).Returns(filterInfo.PK);
				return mappingMock.Object;
			});
			ObjectFactory.Substitute(parserMock.Object);

			using (var menuItem = new ZMenuItem())
			{
				GlowDataWizardIntegration.HandleADAWMenuItemPopup(menuItem, typeof(DummyBusinessObject), _ => { }, _ => { });
				AssertEquals(2, menuItem.MenuItems.Count);

				var manageMappingsMenuItem = menuItem.MenuItems[1];
				AssertEquals(3, manageMappingsMenuItem.MenuItems.Count);
				AssertNotNull(manageMappingsMenuItem.MenuItems.FindByText("test mapping 1"));
				AssertNotNull(manageMappingsMenuItem.MenuItems.FindByText("test mapping 2"));

				mapping1.Delete();
				Factory.Save();

				GlowDataWizardIntegration.HandleADAWMenuItemPopup(menuItem, typeof(DummyBusinessObject), _ => { }, _ => { });
				AssertEquals(2, menuItem.MenuItems.Count);

				manageMappingsMenuItem = menuItem.MenuItems[1];
				AssertEquals(2, manageMappingsMenuItem.MenuItems.Count);
				AssertNotNull(manageMappingsMenuItem.MenuItems.FindByText("test mapping 2"));
			}
		}

		IDisposable tempContext;
		IProgressReporter progressReporter;
		ZForm form;

		IProgressReporterProvider GetProgressFromProvider()
		{
			return new DefaultProgressReporterProvider(form);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var tempStaff = Factory.New<IGlbStaff>();
			Factory.Save();
			tempContext = Env.SetTemporaryUserContext(tempStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK);
			form = new ZForm();
			progressReporter = GetProgressFromProvider().CreateProgressReporter("Processed", 100);
		}

		protected override void TearDown()
		{
			base.TearDown();
			tempContext?.Dispose();
			ObjectFactory.DisposeSubstitutions();
			progressReporter.Dispose();
			form.Dispose();
		}

		class TestCollection<T> : BusinessObjectCollection<T> where T : BusinessObject
		{
			public TestCollection() : base(new BusinessObjectFactory())
			{
			}

			public TestCollection(ZQuery filter) : base(new BusinessObjectFactory())
			{
				this.filter = filter;
			}

			readonly ZQuery filter = ZQuery.NoResultQuery;

			protected override ZQuery CreateRelationshipFilter()
			{
				return filter;
			}
		}

		class TestMapping : IDataTransferMapping
		{
			public Guid PK { get; set; }
			public string Name { get; set; }
			public string ContextModule { get; set; }
			public bool IsDefault { get; set; }
			public Guid OwnerPK { get; set; }
			public short StartingRow { get; set; }
			public string Delimiter { get; set; }
			public string SheetName { get; set; }
			public string ParentCollection { get; set; }
			public bool IncludesHeaders { get; set; }
			public bool IncludesInactiveWhenMatching { get; set; }
			public IReadOnlyCollection<IMappingTable> MappingTables { get; set; }
		}
	}
}
