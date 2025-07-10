using System;
using System.Collections.Generic;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.WebLauncher;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Tools.Testing
{
	sealed class WebUrlLauncherValidationTest : TestCase
	{
		public void TestWebUrlLaunchValidator_KnownSchema_DoesNotThrow()
		{
			var validator = new WebUrlLaunchValidator();
			AssertNoExceptionThrown(() => { validator.ValidateUrl("hTtP://asd.a.b"); });
			AssertNoExceptionThrown(() => { validator.ValidateUrl("HtTpS://asd.a.b"); });
			AssertNoExceptionThrown(() => { validator.ValidateUrl("mailto:a@b.c"); });
			AssertNoExceptionThrown(() => { validator.ValidateUrl("tel:+61 51 21-36"); });
			AssertNoExceptionThrown(() => { validator.ValidateUrl("callto:+61 51 21-36"); });
			AssertNoExceptionThrown(() => { validator.ValidateUrl("vsnet:+61 51 21-36"); });
			AssertNoExceptionThrown(() => { validator.ValidateUrl("HtTpS://a"); });
			AssertNoExceptionThrown(() => { validator.ValidateUrl("edient:Command=ShowEditForm&ControllerID=WorkItem&BusinessEntityPK=c3055c73-83a5-4ba2-9c8c-b012b4042338&Domain=wtg.zone&Instance=ediProd&Hash=%2bvgeWwj9T%2fte8lXQ%2fMNMiQ%2fYQaeLizFva"); });
		}

		public void TestWebUrlLaunchValidator_httpUriWithoutProtocol_DoesNotThrow()
		{
			var validator = new WebUrlLaunchValidator();
			AssertNoExceptionThrown(() => { validator.ValidateUrl("www.google.com"); });
			AssertNoExceptionThrown(() => { validator.ValidateUrl("eqt5g4fuenphqinx.onion"); });
			AssertNoExceptionThrown(() => { validator.ValidateUrl("asdasda"); });
		}

		[UseSnapshotProtection]
		public void TestWebUrlLaunchValidator_InvalidUri_Throws()
		{
			TestWebUrlLaunchValidator_InvalidUri_Throws(null);
			TestWebUrlLaunchValidator_InvalidUri_Throws(string.Empty);
			TestWebUrlLaunchValidator_InvalidUri_Throws("uuuuuu^$&*(u");
			TestWebUrlLaunchValidator_InvalidUri_Throws("&YHhs.COMs&/OHBT");

			void TestWebUrlLaunchValidator_InvalidUri_Throws(string inputUri)
			{
				// Arrange
				using (RawDataRegistry.Instance.ShowWarningPopupBeforeLaunchingURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var userPrompterMock = new Mock<IWebUrlValidationUserPrompter>();
					userPrompterMock
						.Setup(x => x.GetUserConfirmation(It.IsAny<string>()))
						.Returns(false);

					var validator = new WebUrlLaunchValidator(userPrompterMock.Object);

					// Act
					var exception = AssertExceptionThrown<WebUrlValidationException>(() => { validator.ValidateUrl(inputUri); });

					// Assert
					AssertContains($"user chose not to open url.", exception.Message);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestValidateUrlThrowWebUrlValidationExceptionIfUserNotAllowToOpenUrl()
		{
			TestValidateUrlThrowWebUrlValidationExceptionIfUserNotAllowToOpenUrl("file://1");
			TestValidateUrlThrowWebUrlValidationExceptionIfUserNotAllowToOpenUrl("\\\\RemoteFile\\abc");
			unknownProtocolUris.ForEach(x => TestValidateUrlThrowWebUrlValidationExceptionIfUserNotAllowToOpenUrl(x));

			void TestValidateUrlThrowWebUrlValidationExceptionIfUserNotAllowToOpenUrl(string filePath)
			{
				// Arrange
				var userPrompterMock = new Mock<IWebUrlValidationUserPrompter>();
				userPrompterMock
					.Setup(x => x.GetUserConfirmation(It.IsAny<string>()))
					.Returns(false);
				var validator = new WebUrlLaunchValidator(userPrompterMock.Object);

				using (RawDataRegistry.Instance.ShowWarningPopupBeforeLaunchingURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					var exception = AssertExceptionThrown<WebUrlValidationException>((() => { validator.ValidateUrl(filePath); }));

					// Assert
					userPrompterMock.Verify(x => x.GetUserConfirmation(It.IsAny<string>()), Times.Once);
					AssertEquals("user chose not to open url.", exception.Message);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestValidateUrlTriggersUserPrompterOnProtocal()
		{
			TestValidateUrlTriggersUserPrompterOnProtocal("file://1");
			TestValidateUrlTriggersUserPrompterOnProtocal("\\\\RemoteFile\\abc");
			unknownProtocolUris.ForEach(x => TestValidateUrlTriggersUserPrompterOnProtocal(x));

			void TestValidateUrlTriggersUserPrompterOnProtocal(string filePath)
			{
				// Arrange
				var userPrompterMock = new Mock<IWebUrlValidationUserPrompter>();
				userPrompterMock
					.Setup(x => x.GetUserConfirmation(It.IsAny<string>()))
					.Returns(true);
				var validator = new WebUrlLaunchValidator(userPrompterMock.Object);
				using (RawDataRegistry.Instance.ShowWarningPopupBeforeLaunchingURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					AssertNoExceptionThrown((() => { validator.ValidateUrl(filePath); }));

					// Assert
					userPrompterMock.Verify(x => x.GetUserConfirmation(It.IsAny<string>()), Times.Once);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestValidateUrlIfShowWarningPopupBeforeLaunchingURLIsFalse()
		{
			TestValidateUrlIfShowWarningPopupBeforeLaunchingURLIsFalse("file://1");
			TestValidateUrlIfShowWarningPopupBeforeLaunchingURLIsFalse("\\\\RemoteFile\\abc");
			unknownProtocolUris.ForEach(x => TestValidateUrlIfShowWarningPopupBeforeLaunchingURLIsFalse(x));

			void TestValidateUrlIfShowWarningPopupBeforeLaunchingURLIsFalse(string filePath)
			{
				// Arrange
				var userPrompterMock = new Mock<IWebUrlValidationUserPrompter>();
				var validator = new WebUrlLaunchValidator(userPrompterMock.Object);

				using (RawDataRegistry.Instance.ShowWarningPopupBeforeLaunchingURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					// Act
					AssertNoExceptionThrown(() => { validator.ValidateUrl(filePath); });

					// Assert
					userPrompterMock.Verify(x => x.GetUserConfirmation(It.IsAny<string>()), Times.Never);
				}
			}
		}

		readonly List<string> unknownProtocolUris = new List<string>
			{
				"admin:/",
				"tcp://1",
				"app://1",
				"javascript://1",
				"jdbc://1",
				"ms-excel://1",
				"web+abc://1",
				"https1:asdasd"
			};
	}
}
