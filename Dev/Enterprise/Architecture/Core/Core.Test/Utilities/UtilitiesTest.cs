using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
#if NETFRAMEWORK
using Microsoft.JScript.Vsa;
#endif
using NUnit.Framework;
using static Enterprise.ZArchitecture.Core.Utilities.ExpressionEvaluator;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class UtilitiesTest : TransactionedTestCase
	{
		public void TestIsValidGuid()
		{
			object testObject = new object();
			Guid testGuid = Guid.NewGuid();
			Guid testEmptyGuid = NullType.Guid;

			AssertEquals("Not a Guid", false, Utilities.IsValidGuid(testObject));
			AssertEquals("Empty Guid", false, Utilities.IsValidGuid(testEmptyGuid));
			AssertEquals("Valid Guid", true, Utilities.IsValidGuid(testGuid));
			AssertEquals("InvalidSelectionGuid", false, Utilities.IsValidGuid(Utilities.InvalidSelectionGuid));
		}

		public void TestGetLocalIPAddress()
		{
			AssertEquals("IP4 address should contain 3 periods", "...",
			Utilities.GetLocalIPAddress()
				.Replace("0", "")
				.Replace("1", "")
				.Replace("2", "")
				.Replace("3", "")
				.Replace("4", "")
				.Replace("5", "")
				.Replace("6", "")
				.Replace("7", "")
				.Replace("8", "")
				.Replace("9", ""));
		}

		public void TestGetLocalIpAddressReturnsRoutableIp()
		{
			Assert("Should return routable IP Address", Utilities.IsRoutableIp(Utilities.GetLocalIPAddress()));
		}

		public void TestGetFirstRoutableIpAddressOrEmpty()
		{
			var addresses = new System.Net.IPAddress[5];
			AssertEquals("First nonroutable IP should be empty", string.Empty, Utilities.GetFirstRoutableIpAddressOrEmpty(addresses));

			addresses[0] = new System.Net.IPAddress(new byte[] { 169, 254, 1, 2 });
			addresses[1] = new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 });
			addresses[2] = new System.Net.IPAddress(new byte[] { 192, 168, 1, 2 });
			addresses[3] = new System.Net.IPAddress(new byte[] { 192, 168, 1, 101 });
			addresses[4] = new System.Net.IPAddress(new byte[] { 192, 168, 1, 202 });

			AssertEquals("First nonroutable IP should be '192.168.1.2'", "192.168.1.2", Utilities.GetFirstRoutableIpAddressOrEmpty(addresses));
		}

		public void TestIsLoopbackAddress()
		{
			Assert("'127.0.0.1' is a loopback address", Utilities.IsLoopbackAddress("127.0.0.1"));
			Assert("'::1' is a loopback address", Utilities.IsLoopbackAddress("::1"));
			Assert("'192.168.1.101' is not a loopback address", !Utilities.IsLoopbackAddress("192.168.1.101"));
		}

		public void TestIsRoutableIp()
		{
			Assert("'192.168.1.101' is routable IP", Utilities.IsRoutableIp("192.168.1.101"));
			Assert("169.254.1.2 is not routable IP", !Utilities.IsRoutableIp("169.254.1.2"));
		}

		public void TestGetFieldFromTable()
		{
			Guid expectedPK = (Guid)Db.Connection.ExecuteScalar("SELECT RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = @code", cmd => cmd.AddParameterBasedOnDbColumn("@code", "AUSYD", RefUNLOCOSchema.RL_Code));   // This is O so can't use Z

			AssertEquals("Text Filter", expectedPK, Utilities.GetFieldFromTable("RL_PK", "RefUNLOCO", RefUNLOCOSchema.RL_Code, "AUSYD"));
			AssertEquals("Guid Filter", "AUSYD", Utilities.GetFieldFromTable("RL_Code", "RefUNLOCO", RefUNLOCOSchema.PK, expectedPK));
		}

		public void TestGetFormatString()
		{
			AssertEquals("{0,3:#0.00}", Utilities.GetFormatString(2));
			AssertEquals("{0,3:#0}", Utilities.GetFormatString(0));
			AssertEquals("{0,3:#0.000}", Utilities.GetFormatString(3));
		}

		public void TestGetCodeFromNameAndUNLOCO()
		{
			AssertEquals("EAGLESYD", Utilities.GetCodeFromNameAndUNLOCO("Eagle", SydGuid, ""));
			AssertEquals("EAGLESYD", Utilities.GetCodeFromNameAndUNLOCO("Eagle LTD", SydGuid, ""));
			AssertEquals("ALIBRNDNNSYD", Utilities.GetCodeFromNameAndUNLOCO("Ali Brendon International LTD", SydGuid, ""));
			AssertEquals("ALIBRENDNSYD", Utilities.GetCodeFromNameAndUNLOCO("Ali Brendon PTY", SydGuid, ""));
			AssertEquals("AAISHPPNGSYD", Utilities.GetCodeFromNameAndUNLOCO("A.A.I Shipping PTY", SydGuid, ""));
		}

		public void TestGetStringFromObject()
		{
			AssertEquals("", Utilities.GetStringFromObject(null));
			AssertEquals("", Utilities.GetStringFromObject(""));
			AssertEquals("1", Utilities.GetStringFromObject(1));
			AssertEquals("blah", Utilities.GetStringFromObject("blah"));
		}

		public void TestGetGuidFromObject()
		{
			Guid testGuid = Guid.NewGuid();
			AssertEquals(NullType.Guid, Utilities.GetGuidFromObject(null));
			AssertEquals(NullType.Guid, Utilities.GetGuidFromObject(5));
			AssertEquals(NullType.Guid, Utilities.GetGuidFromObject("test string"));
			AssertEquals(testGuid, Utilities.GetGuidFromObject(testGuid.ToString()));
			AssertEquals(testGuid, Utilities.GetGuidFromObject(testGuid));
		}

		public void TestIsObjectAValidDate()
		{
			AssertEquals(false, Utilities.IsObjectAValidDate(null));
			AssertEquals(false, Utilities.IsObjectAValidDate(DBNull.Value));
			AssertEquals(false, Utilities.IsObjectAValidDate("asdf"));
			AssertEquals(true, Utilities.IsObjectAValidDate(EnvProxy.Instance.Time.CurrentLocalDateTime));
		}

		public void TestConvertToDecimal()
		{
			AssertEquals(0M, Utilities.ConvertToDecimal(null));
			AssertEquals(0M, Utilities.ConvertToDecimal(""));
			AssertEquals(0M, Utilities.ConvertToDecimal("This is not a double"));
			AssertEquals(23.5M, Utilities.ConvertToDecimal("23.5"));
			AssertEquals(0M, Utilities.ConvertToDecimal(DBNull.Value));
			AssertEquals(23.5M, Utilities.ConvertToDecimal(23.5));
		}

		public void TestConvertToDateTime()
		{
			DateTime testDate = new DateTime(2002, 12, 16);
			AssertEquals(testDate, Utilities.ConvertToDateTime(testDate));
			AssertEquals(NullType.DateTime, Utilities.ConvertToDateTime(DBNull.Value));
			AssertEquals(testDate, Utilities.ConvertToDateTime("16Dec2002"));
			AssertEquals(NullType.DateTime, Utilities.ConvertToDateTime(""));
		}

		public void TestConvertToInt32()
		{
			AssertEquals("Null", 0, Utilities.ConvertToInt32(null));
			AssertEquals("DBNull", 0, Utilities.ConvertToInt32(DBNull.Value));
			AssertEquals(23, Utilities.ConvertToInt32(23));
			AssertEquals(0, Utilities.ConvertToInt32("123-245"));
		}

		public void TestConvertToInt32TwoArg()
		{
			AssertEquals("Parsable string", 42, Utilities.ConvertToInt32("42", -123));
			AssertEquals("Parsable string", -123, Utilities.ConvertToInt32("crap", -123));
		}

		public void TestFormatNumber()
		{
			// With default (currently -- Aussie-based) culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				AssertEquals("0", Utilities.FormatNumber(0, 0));
				AssertEquals("0.00", Utilities.FormatNumber(0, 2));
				AssertEquals("3.000", Utilities.FormatNumber(3m, 3));
				AssertEquals("2.35", Utilities.FormatNumber(2.3456, 2));
				AssertEquals("2.300", Utilities.FormatNumber(2.3d, 3));
				AssertEquals("2000.300", Utilities.FormatNumber(2000.3d, 3));
			}

			// With French culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("fr-FR")))
			{
				AssertEquals("0", Utilities.FormatNumber(0, 0));
				AssertEquals("0.00", Utilities.FormatNumber(0, 2));
				AssertEquals("3.000", Utilities.FormatNumber(3m, 3));
				AssertEquals("2.35", Utilities.FormatNumber(2.3456, 2));
				AssertEquals("2.300", Utilities.FormatNumber(2.3d, 3));
				AssertEquals("2000.300", Utilities.FormatNumber(2000.3d, 3));
			}
		}

		public void TestFormatNumberCustomCulture()
		{
			CultureInfo testCulture = CultureInfo.CreateSpecificCulture("fr-FR");
			AssertEquals("0", Utilities.FormatNumber(0, 0, testCulture));
			AssertEquals("0,00", Utilities.FormatNumber(0, 2, testCulture));
			AssertEquals("3,000", Utilities.FormatNumber(3m, 3, testCulture));
			AssertEquals("2,35", Utilities.FormatNumber(2.3456, 2, testCulture));
			AssertEquals("2,300", Utilities.FormatNumber(2.3d, 3, testCulture));
			AssertEquals("2000,300", Utilities.FormatNumber(2000.3d, 3, testCulture));
		}

		public void TestFormatNumberNational()
		{
			// With default (currently -- Aussie-based) culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				AssertEquals("0", Utilities.FormatNumberNational(0, 0));
				AssertEquals("0.00", Utilities.FormatNumberNational(0, 2));
				AssertEquals("3.000", Utilities.FormatNumberNational(3m, 3));
				AssertEquals("2.35", Utilities.FormatNumberNational(2.3456, 2));
				AssertEquals("2.300", Utilities.FormatNumberNational(2.3d, 3));
				AssertEquals("2000.300", Utilities.FormatNumberNational(2000.3d, 3));
			}

			// With French culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("fr-FR")))
			{
				AssertEquals("0", Utilities.FormatNumberNational(0, 0));
				AssertEquals("0,00", Utilities.FormatNumberNational(0, 2));
				AssertEquals("3,000", Utilities.FormatNumberNational(3m, 3));
				AssertEquals("2,35", Utilities.FormatNumberNational(2.3456, 2));
				AssertEquals("2,300", Utilities.FormatNumberNational(2.3d, 3));
				AssertEquals("2000,300", Utilities.FormatNumberNational(2000.3d, 3));
			}
		}

		public void TestFormatNumberWithGroupSeparators()
		{
			// With default (currently -- Aussie-based) culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				AssertEquals("2,000.300", Utilities.FormatNumberWithGroupSeparators(2000.3d, 3));
			}

			// With French culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("fr-FR")))
			{
				AssertEquals("2,000.300", Utilities.FormatNumberWithGroupSeparators(2000.3d, 3));
			}
		}

		public void TestFormatNumberWithGroupSeparatorsCustomCulture()
		{
			AssertEquals("2 000,300", Utilities.FormatNumberWithGroupSeparators(2000.3d, 3, CultureInfo.CreateSpecificCulture("fr-FR")));
		}

		public void TestFormatNumberNationalWithGroupSeparators()
		{
			// With default (currently -- Aussie-based) culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			{
				AssertEquals("2,000.300", Utilities.FormatNumberNationalWithGroupSeparators(2000.3d, 3));
			}

			// With French culture
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("fr-FR")))
			{
				AssertEquals("2 000,300", Utilities.FormatNumberNationalWithGroupSeparators(2000.3d, 3));
			}
		}

		public void TestFormatNumberFromZDecimal()
		{
			ZDecimal testValue = 0m;
			AssertEquals("0", Utilities.FormatNumber(testValue, 0));
			AssertEquals("0.00", Utilities.FormatNumber(testValue, 2));
			testValue = 3m;
			AssertEquals("3.000", Utilities.FormatNumber(testValue, 3));
			testValue = 2.3456m;
			AssertEquals("2.35", Utilities.FormatNumber(testValue, 2));
			testValue = 2.3d;
			AssertEquals("2.300", Utilities.FormatNumber(testValue, 3));
		}

		public void TestFormatNumberFromZInt()
		{
			ZInt testValue = 0;
			AssertEquals("0", Utilities.FormatNumberFromZInt(testValue, 0));
			AssertEquals("0.00", Utilities.FormatNumberFromZInt(testValue, 2));
			testValue = 3;
			AssertEquals("3.000", Utilities.FormatNumberFromZInt(testValue, 3));
		}

		public void TestRoundDown()
		{
			AssertEquals("Rounding Normal Number", 3.4m, Utilities.Round(3.44m, 1));
		}

		public void TestRoundAtMidway()
		{
			AssertEquals("Rounding Normal Number", 3.5m, Utilities.Round(3.45m, 1));
			AssertEquals("Rounding Normal Number", 3.4m, Utilities.Round(3.35m, 1));
			AssertEquals("Rounding Normal Number", 3.15m, Utilities.Round(3.145m, 2));
			AssertEquals("Rounding Normal Number", 3.14m, Utilities.Round(3.135m, 2));
		}

		public void TestRoundUp()
		{
			AssertEquals("Rounding Normal Number", 3.5m, Utilities.Round(3.46m, 1));
		}

		public void TestRoundChopTwoDPOff()
		{
			AssertEquals("Rounding", 3.5m, Utilities.Round(3.5499m, 1));
		}

		public void TestRoundDownNegatives()
		{
			AssertEquals("Rounding Normal Number", -3.4m, Utilities.Round(-3.44m, 1));
		}

		public void TestRoundNegativesAtMidway()
		{
			AssertEquals("Rounding Normal Number", -3.5m, Utilities.Round(-3.45m, 1));
			AssertEquals("Rounding Normal Number", -3.4m, Utilities.Round(-3.35m, 1));
			AssertEquals("Rounding Normal Number", -3.15m, Utilities.Round(-3.145m, 2));
			AssertEquals("Rounding Normal Number", -3.14m, Utilities.Round(-3.135m, 2));
		}

		public void TestRoundNegativesUp()
		{
			AssertEquals("Rounding Normal Number", -3.5m, Utilities.Round(-3.46m, 1));
		}

		public void TestRoundChopTwoDPOffNegatives()
		{
			AssertEquals("Rounding", -3.5m, Utilities.Round(-3.5499m, 1));
		}

		public void TestRoundByParamRoundingFactor()
		{
			var testData = new Dictionary<ZString, (decimal inputValue, decimal expectedValue)[]>()
				{
					{ Utilities.RoundingTypes.NoRounding, new [] { (0.49m, 0.49m), (0.50m, 0.50m), (0.51m, 0.51m) } },
					{
						Utilities.RoundingTypes.Bankers,
						new []
						{
							(0.49m, 0), (0.50m, 0m), (0.51m, 1m),
							(1.49m, 1m), (1.50m, 2m), (1.51m, 2m),
							(2.49m, 2m), (2.50m, 2m), (2.51m, 3m),
						}
					},
					{
						Utilities.RoundingTypes.UpTo1,
						new []
						{
							(0.49m, 1m), (0.50m, 1m), (0.51m, 1m),
							(1.49m, 2m), (1.50m, 2m), (1.51m, 2m),
						}
					},
					{
						Utilities.RoundingTypes.UpToHalf,
						new []
						{
							(0.49m, 0.5m), (0.50m, 0.50m), (0.51m, 1m),
							(1.49m, 1.5m), (1.50m, 1.50m), (1.51m, 2m),
						}
					},
					{
						Utilities.RoundingTypes.UpTo1IfLessThanOne,
						new []
						{
							(0.49m, 1m), (0.50m, 1m), (0.51m, 1m),
							(1.49m, 1.49m), (1.50m, 1.50m), (1.51m, 1.51m),
						}
					},
					{
						Utilities.RoundingTypes.Custom, // with rounding factor 0.72
						new []
						{
							(0.49m, 0.72m), (0.50m, 0.72m), (0.51m, 0.72m),
							(1.49m, 2.16m), (2.16m, 2.16m), (2.160001m, 2.88m),
						}
					},
				};

			var roundingFactor = 0.72m;

			foreach (var roundingType in testData.Keys)
			{
				foreach(var testcase in testData[roundingType])
				{
					AssertEquals(testcase.expectedValue, Utilities.Round(testcase.inputValue, roundingType, roundingFactor));
				}
			}
		}

		public void TestCurrentHostMachineName()
		{
			AssertNotNull("Current Host", Utilities.GetCurrentHostMachineName());
		}

		public void TestServerMachineName()
		{
			AssertNotNull("Server Machine Name", Utilities.GetServerMachineName());
		}

		public void TestIsByteArrayEqual()
		{
			byte[] byteArray1 = new byte[] { 0, 1, 2, 3, 4, 5 };
			byte[] byteArray2 = new byte[] { 6, 7, 8, 9, 10 };

			Assert("IsByteArrayEqual should be true", Utilities.IsByteArrayEqual(null, null));
			Assert("IsByteArrayEqual should be false", !Utilities.IsByteArrayEqual(byteArray1, null));
			Assert("IsByteArrayEqual should be false", !Utilities.IsByteArrayEqual(byteArray2, null));
			Assert("IsByteArrayEqual should be false", !Utilities.IsByteArrayEqual(null, byteArray1));
			Assert("IsByteArrayEqual should be false", !Utilities.IsByteArrayEqual(null, byteArray2));
			Assert("IsByteArrayEqual should be true", Utilities.IsByteArrayEqual(byteArray1, byteArray1));
			Assert("IsByteArrayEqual should be true", Utilities.IsByteArrayEqual(byteArray2, byteArray2));
			Assert("IsByteArrayEqual should be true", !Utilities.IsByteArrayEqual(byteArray1, byteArray2));
			Assert("IsByteArrayEqual should be true", !Utilities.IsByteArrayEqual(byteArray2, byteArray1));
		}

		public void TestIsImageEqual()
		{
			Image image1 = new Bitmap(10, 10);
			Image image2 = new Bitmap(20, 20);

			Assert("IsImageEqual should be true", Utilities.IsImageEqual(image1, image1));
			Assert("IsImageEqual should be true", Utilities.IsImageEqual(image2, image2));
			Assert("IsImageEqual should be false", !Utilities.IsImageEqual(image1, image2));
			Assert("IsImageEqual should be false with no exception", !Utilities.IsImageEqual(image1, null));
		}

		public void TestGetImageContentsHashCode()
		{
			var image1 = new Bitmap(1, 1);
			var image2 = new Bitmap(1, 1);
			AssertEquals(Utilities.GetImageContentsHashCode(image1), Utilities.GetImageContentsHashCode(image2));
		}

		public void TestIsConnectionAndPortChecked()
		{
			var portRegistry = new RawDataRegistry.PhysicalServerRegistryItem("TestPort", (NoResString)"Test Category", (NoResString)"Test Server Port", (NoResString)"Test Server Port", RegistryDataTypes.IntType, RegistryOptions.Default, 110);
			var listProvider = new CodeDescriptionPairListProvider(() => new SecureConnectionTypes());

			var pop3SecureConnection = new CodePairWithAdditionalEventRegistryItem("TestConnection", (NoResString)"Test Category", (NoResString)"Test Connection", (NoResString)"Test Hint", listProvider, false, true, new ComboBoxRegistryEditorInfo(listProvider),
				(registryItem) =>
				{
					return Utilities.IsConnectionAndPortChecked(registryItem, portRegistry);
				}, RegistryStorageFlags.System, RegistryOptions.Default, SecureConnectionTypes.None, false);

			Assert("Pre-condition: IsConnectionAndPortChecked should be false", !Utilities.IsConnectionAndPortChecked(pop3SecureConnection, portRegistry));

			((IRegistryItemInternals)pop3SecureConnection).SetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty, SecureConnectionTypes.SSL);
			Assert("IsConnectionAndPortChecked should be true", Utilities.IsConnectionAndPortChecked(pop3SecureConnection, portRegistry));
			AssertEquals("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text, "You have selected to use a secure connection, please confirm that the port number selected is desired > Test Server Port");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			((IRegistryItemInternals)pop3SecureConnection).SetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty, SecureConnectionTypes.None);
			portRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 465);
			Assert("IsConnectionAndPortChecked should be false", !Utilities.IsConnectionAndPortChecked(pop3SecureConnection, portRegistry));

			((IRegistryItemInternals)pop3SecureConnection).SetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty, SecureConnectionTypes.None);
			Assert("IsConnectionAndPortChecked should still be false", !Utilities.IsConnectionAndPortChecked(pop3SecureConnection, portRegistry));
		}

		public void TestIsProtocolAndPortChecked()
		{
			var portRegistry = new RawDataRegistry.PhysicalServerRegistryItem("TestPort", (NoResString)"Test Category", (NoResString)"Test Server Port", (NoResString)"Test Server Port", RegistryDataTypes.IntType, RegistryOptions.Default, 110);
			var listProvider = new CodeDescriptionPairListProvider(() => new MailRetrievalProtocols());

			var mailRetrievalProtocol = new CodePairWithAdditionalEventRegistryItem("TestProtocol", (NoResString)"Test Category", (NoResString)"Test Protocol", (NoResString)"Test Hint", listProvider, false, true, new ComboBoxRegistryEditorInfo(listProvider),
				(registryItem) =>
				{
					return Utilities.IsProtocolAndPortChecked(registryItem, portRegistry);
				}, RegistryStorageFlags.System, RegistryOptions.Default, MailRetrievalProtocols.POP3, false);

			Assert("Pre-condition: IsProtocolAndPortChecked should be false", !Utilities.IsProtocolAndPortChecked(mailRetrievalProtocol, portRegistry));

			((IRegistryItemInternals)mailRetrievalProtocol).SetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty, MailRetrievalProtocols.IMAP);
			Assert("IsProtocolAndPortChecked should be true", Utilities.IsProtocolAndPortChecked(mailRetrievalProtocol, portRegistry));
			AssertEquals("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text, "Please confirm that this protocol matches the selected port > Test Server Port");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			((IRegistryItemInternals)mailRetrievalProtocol).SetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty, MailRetrievalProtocols.POP3);
			Assert("IsProtocolAndPortChecked should be false", !Utilities.IsProtocolAndPortChecked(mailRetrievalProtocol, portRegistry));

			portRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 143);
			Assert("IsProtocolAndPortChecked should be true", Utilities.IsProtocolAndPortChecked(mailRetrievalProtocol, portRegistry));
			AssertEquals("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text, "Please confirm that this protocol matches the selected port > Test Server Port");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			portRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)portRegistry.DefaultValue);
			((IRegistryItemInternals)mailRetrievalProtocol).SetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty, MailRetrievalProtocols.POP3);
			Assert("IsProtocolAndPortChecked should be false", !Utilities.IsProtocolAndPortChecked(mailRetrievalProtocol, portRegistry));
		}

		public void TestIsImageEqualByPixels()
		{
			// Three byte arrays created from identical 256-colour images, one saved as 24 bit bmp, the other as a 256 colour one, and the third compressed to GIF.
			// The test cannot assert the equality of images that are different due to compressionsal losses.  e.g. if we started with a skin tone and compressed to gif or 256 bmp, they'd probably not be the same by pixel.

			byte[] image256Colour = new byte[] { 66, 77, 182, 11, 0, 0, 0, 0, 0, 0, 54, 4, 0, 0, 40, 0, 0, 0, 45, 0, 0, 0, 40, 0, 0, 0, 1, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 196, 14, 0, 0, 196, 14, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 255, 0, 0, 128, 255, 0, 128, 0, 255, 0, 128, 128, 255, 128, 0, 0, 255, 128, 0, 128, 255, 128, 128, 0, 255, 192, 192, 192, 255, 192, 220, 192, 255, 240, 202, 166, 255, 0, 32, 64, 255, 0, 32, 96, 255, 0, 32, 128, 255, 0, 32, 160, 255, 0, 32, 192, 255, 0, 32, 224, 255, 0, 64, 0, 255, 0, 64, 32, 255, 0, 64, 64, 255, 0, 64, 96, 255, 0, 64, 128, 255, 0, 64, 160, 255, 0, 64, 192, 255, 0, 64, 224, 255, 0, 96, 0, 255, 0, 96, 32, 255, 0, 96, 64, 255, 0, 96, 96, 255, 0, 96, 128, 255, 0, 96, 160, 255, 0, 96, 192, 255, 0, 96, 224, 255, 0, 128, 0, 255, 0, 128, 32, 255, 0, 128, 64, 255, 0, 128, 96, 255, 0, 128, 128, 255, 0, 128, 160, 255, 0, 128, 192, 255, 0, 128, 224, 255, 0, 160, 0, 255, 0, 160, 32, 255, 0, 160, 64, 255, 0, 160, 96, 255, 0, 160, 128, 255, 0, 160, 160, 255, 0, 160, 192, 255, 0, 160, 224, 255, 0, 192, 0, 255, 0, 192, 32, 255, 0, 192, 64, 255, 0, 192, 96, 255, 0, 192, 128, 255, 0, 192, 160, 255, 0, 192, 192, 255, 0, 192, 224, 255, 0, 224, 0, 255, 0, 224, 32, 255, 0, 224, 64, 255, 0, 224, 96, 255, 0, 224, 128, 255, 0, 224, 160, 255, 0, 224, 192, 255, 0, 224, 224, 255, 64, 0, 0, 255, 64, 0, 32, 255, 64, 0, 64, 255, 64, 0, 96, 255, 64, 0, 128, 255, 64, 0, 160, 255, 64, 0, 192, 255, 64, 0, 224, 255, 64, 32, 0, 255, 64, 32, 32, 255, 64, 32, 64, 255, 64, 32, 96, 255, 64, 32, 128, 255, 64, 32, 160, 255, 64, 32, 192, 255, 64, 32, 224, 255, 64, 64, 0, 255, 64, 64, 32, 255, 64, 64, 64, 255, 64, 64, 96, 255, 64, 64, 128, 255, 64, 64, 160, 255, 64, 64, 192, 255, 64, 64, 224, 255, 64, 96, 0, 255, 64, 96, 32, 255, 64, 96, 64, 255, 64, 96, 96, 255, 64, 96, 128, 255, 64, 96, 160, 255, 64, 96, 192, 255, 64, 96, 224, 255, 64, 128, 0, 255, 64, 128, 32, 255, 64, 128, 64, 255, 64, 128, 96, 255, 64, 128, 128, 255, 64, 128, 160, 255, 64, 128, 192, 255, 64, 128, 224, 255, 64, 160, 0, 255, 64, 160, 32, 255, 64, 160, 64, 255, 64, 160, 96, 255, 64, 160, 128, 255, 64, 160, 160, 255, 64, 160, 192, 255, 64, 160, 224, 255, 64, 192, 0, 255, 64, 192, 32, 255, 64, 192, 64, 255, 64, 192, 96, 255, 64, 192, 128, 255, 64, 192, 160, 255, 64, 192, 192, 255, 64, 192, 224, 255, 64, 224, 0, 255, 64, 224, 32, 255, 64, 224, 64, 255, 64, 224, 96, 255, 64, 224, 128, 255, 64, 224, 160, 255, 64, 224, 192, 255, 64, 224, 224, 255, 128, 0, 0, 255, 128, 0, 32, 255, 128, 0, 64, 255, 128, 0, 96, 255, 128, 0, 128, 255, 128, 0, 160, 255, 128, 0, 192, 255, 128, 0, 224, 255, 128, 32, 0, 255, 128, 32, 32, 255, 128, 32, 64, 255, 128, 32, 96, 255, 128, 32, 128, 255, 128, 32, 160, 255, 128, 32, 192, 255, 128, 32, 224, 255, 128, 64, 0, 255, 128, 64, 32, 255, 128, 64, 64, 255, 128, 64, 96, 255, 128, 64, 128, 255, 128, 64, 160, 255, 128, 64, 192, 255, 128, 64, 224, 255, 128, 96, 0, 255, 128, 96, 32, 255, 128, 96, 64, 255, 128, 96, 96, 255, 128, 96, 128, 255, 128, 96, 160, 255, 128, 96, 192, 255, 128, 96, 224, 255, 128, 128, 0, 255, 128, 128, 32, 255, 128, 128, 64, 255, 128, 128, 96, 255, 128, 128, 128, 255, 128, 128, 160, 255, 128, 128, 192, 255, 128, 128, 224, 255, 128, 160, 0, 255, 128, 160, 32, 255, 128, 160, 64, 255, 128, 160, 96, 255, 128, 160, 128, 255, 128, 160, 160, 255, 128, 160, 192, 255, 128, 160, 224, 255, 128, 192, 0, 255, 128, 192, 32, 255, 128, 192, 64, 255, 128, 192, 96, 255, 128, 192, 128, 255, 128, 192, 160, 255, 128, 192, 192, 255, 128, 192, 224, 255, 128, 224, 0, 255, 128, 224, 32, 255, 128, 224, 64, 255, 128, 224, 96, 255, 128, 224, 128, 255, 128, 224, 160, 255, 128, 224, 192, 255, 128, 224, 224, 255, 192, 0, 0, 255, 192, 0, 32, 255, 192, 0, 64, 255, 192, 0, 96, 255, 192, 0, 128, 255, 192, 0, 160, 255, 192, 0, 192, 255, 192, 0, 224, 255, 192, 32, 0, 255, 192, 32, 32, 255, 192, 32, 64, 255, 192, 32, 96, 255, 192, 32, 128, 255, 192, 32, 160, 255, 192, 32, 192, 255, 192, 32, 224, 255, 192, 64, 0, 255, 192, 64, 32, 255, 192, 64, 64, 255, 192, 64, 96, 255, 192, 64, 128, 255, 192, 64, 160, 255, 192, 64, 192, 255, 192, 64, 224, 255, 192, 96, 0, 255, 192, 96, 32, 255, 192, 96, 64, 255, 192, 96, 96, 255, 192, 96, 128, 255, 192, 96, 160, 255, 192, 96, 192, 255, 192, 96, 224, 255, 192, 128, 0, 255, 192, 128, 32, 255, 192, 128, 64, 255, 192, 128, 96, 255, 192, 128, 128, 255, 192, 128, 160, 255, 192, 128, 192, 255, 192, 128, 224, 255, 192, 160, 0, 255, 192, 160, 32, 255, 192, 160, 64, 255, 192, 160, 96, 255, 192, 160, 128, 255, 192, 160, 160, 255, 192, 160, 192, 255, 192, 160, 224, 255, 192, 192, 0, 255, 192, 192, 32, 255, 192, 192, 64, 255, 192, 192, 96, 255, 192, 192, 128, 255, 192, 192, 160, 255, 240, 251, 255, 255, 164, 160, 160, 255, 128, 128, 128, 255, 0, 0, 255, 255, 0, 255, 0, 255, 0, 255, 255, 255, 255, 0, 0, 255, 255, 0, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 250, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 249, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 227, 227, 255 };
			byte[] image24Bit = new byte[] { 66, 77, 118, 21, 0, 0, 0, 0, 0, 0, 54, 0, 0, 0, 40, 0, 0, 0, 45, 0, 0, 0, 40, 0, 0, 0, 1, 0, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 196, 14, 0, 0, 196, 14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0 };
			byte[] imageFromGif = new byte[] { 71, 73, 70, 56, 57, 97, 45, 0, 40, 0, 247, 0, 0, 0, 0, 0, 128, 0, 0, 0, 128, 0, 128, 128, 0, 0, 0, 128, 128, 0, 128, 0, 128, 128, 128, 128, 128, 192, 192, 192, 255, 0, 0, 0, 255, 0, 255, 255, 0, 0, 0, 255, 255, 0, 255, 0, 255, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 51, 0, 0, 102, 0, 0, 153, 0, 0, 204, 0, 0, 255, 0, 51, 0, 0, 51, 51, 0, 51, 102, 0, 51, 153, 0, 51, 204, 0, 51, 255, 0, 102, 0, 0, 102, 51, 0, 102, 102, 0, 102, 153, 0, 102, 204, 0, 102, 255, 0, 153, 0, 0, 153, 51, 0, 153, 102, 0, 153, 153, 0, 153, 204, 0, 153, 255, 0, 204, 0, 0, 204, 51, 0, 204, 102, 0, 204, 153, 0, 204, 204, 0, 204, 255, 0, 255, 0, 0, 255, 51, 0, 255, 102, 0, 255, 153, 0, 255, 204, 0, 255, 255, 51, 0, 0, 51, 0, 51, 51, 0, 102, 51, 0, 153, 51, 0, 204, 51, 0, 255, 51, 51, 0, 51, 51, 51, 51, 51, 102, 51, 51, 153, 51, 51, 204, 51, 51, 255, 51, 102, 0, 51, 102, 51, 51, 102, 102, 51, 102, 153, 51, 102, 204, 51, 102, 255, 51, 153, 0, 51, 153, 51, 51, 153, 102, 51, 153, 153, 51, 153, 204, 51, 153, 255, 51, 204, 0, 51, 204, 51, 51, 204, 102, 51, 204, 153, 51, 204, 204, 51, 204, 255, 51, 255, 0, 51, 255, 51, 51, 255, 102, 51, 255, 153, 51, 255, 204, 51, 255, 255, 102, 0, 0, 102, 0, 51, 102, 0, 102, 102, 0, 153, 102, 0, 204, 102, 0, 255, 102, 51, 0, 102, 51, 51, 102, 51, 102, 102, 51, 153, 102, 51, 204, 102, 51, 255, 102, 102, 0, 102, 102, 51, 102, 102, 102, 102, 102, 153, 102, 102, 204, 102, 102, 255, 102, 153, 0, 102, 153, 51, 102, 153, 102, 102, 153, 153, 102, 153, 204, 102, 153, 255, 102, 204, 0, 102, 204, 51, 102, 204, 102, 102, 204, 153, 102, 204, 204, 102, 204, 255, 102, 255, 0, 102, 255, 51, 102, 255, 102, 102, 255, 153, 102, 255, 204, 102, 255, 255, 153, 0, 0, 153, 0, 51, 153, 0, 102, 153, 0, 153, 153, 0, 204, 153, 0, 255, 153, 51, 0, 153, 51, 51, 153, 51, 102, 153, 51, 153, 153, 51, 204, 153, 51, 255, 153, 102, 0, 153, 102, 51, 153, 102, 102, 153, 102, 153, 153, 102, 204, 153, 102, 255, 153, 153, 0, 153, 153, 51, 153, 153, 102, 153, 153, 153, 153, 153, 204, 153, 153, 255, 153, 204, 0, 153, 204, 51, 153, 204, 102, 153, 204, 153, 153, 204, 204, 153, 204, 255, 153, 255, 0, 153, 255, 51, 153, 255, 102, 153, 255, 153, 153, 255, 204, 153, 255, 255, 204, 0, 0, 204, 0, 51, 204, 0, 102, 204, 0, 153, 204, 0, 204, 204, 0, 255, 204, 51, 0, 204, 51, 51, 204, 51, 102, 204, 51, 153, 204, 51, 204, 204, 51, 255, 204, 102, 0, 204, 102, 51, 204, 102, 102, 204, 102, 153, 204, 102, 204, 204, 102, 255, 204, 153, 0, 204, 153, 51, 204, 153, 102, 204, 153, 153, 204, 153, 204, 204, 153, 255, 204, 204, 0, 204, 204, 51, 204, 204, 102, 204, 204, 153, 204, 204, 204, 204, 204, 255, 204, 255, 0, 204, 255, 51, 204, 255, 102, 204, 255, 153, 204, 255, 204, 204, 255, 255, 255, 0, 0, 255, 0, 51, 255, 0, 102, 255, 0, 153, 255, 0, 204, 255, 0, 255, 255, 51, 0, 255, 51, 51, 255, 51, 102, 255, 51, 153, 255, 51, 204, 255, 51, 255, 255, 102, 0, 255, 102, 51, 255, 102, 102, 255, 102, 153, 255, 102, 204, 255, 102, 255, 255, 153, 0, 255, 153, 51, 255, 153, 102, 255, 153, 153, 255, 153, 204, 255, 153, 255, 255, 204, 0, 255, 204, 51, 255, 204, 102, 255, 204, 153, 255, 204, 204, 255, 204, 255, 255, 255, 0, 255, 255, 51, 255, 255, 102, 255, 255, 153, 255, 255, 204, 255, 255, 255, 33, 249, 4, 1, 0, 0, 16, 0, 44, 0, 0, 0, 0, 45, 0, 40, 0, 0, 8, 133, 0, 255, 9, 28, 72, 176, 160, 193, 131, 8, 19, 42, 92, 200, 176, 161, 195, 135, 16, 35, 74, 156, 72, 177, 34, 65, 110, 24, 51, 106, 212, 104, 49, 225, 198, 143, 25, 59, 34, 4, 9, 82, 228, 65, 146, 31, 77, 26, 68, 185, 81, 101, 65, 150, 28, 93, 14, 132, 25, 82, 166, 64, 154, 24, 109, 222, 196, 169, 243, 31, 78, 110, 61, 127, 6, 229, 169, 83, 104, 207, 163, 72, 147, 42, 93, 202, 180, 169, 211, 167, 80, 163, 74, 173, 104, 164, 170, 213, 171, 86, 143, 98, 221, 90, 85, 43, 87, 172, 94, 191, 102, 237, 41, 246, 106, 216, 178, 103, 197, 166, 253, 186, 150, 107, 219, 173, 111, 193, 146, 45, 91, 53, 32, 0, 59 };
			MemoryStream ms1 = new MemoryStream(image256Colour);
			MemoryStream ms2 = new MemoryStream(image24Bit);
			MemoryStream ms3 = new MemoryStream(imageFromGif);

			Image image1 = Image.FromStream(ms1);
			Image image2 = Image.FromStream(ms2);
			Image image3 = Image.FromStream(ms3);

			Assert("IsImageEqual should be true - same image, different pixels", Utilities.IsImageEqual(image1, image2));
			Assert("Same image, pixel by pixel, despite different byte arrays", Utilities.IsImageEqual(image1, image3));
			Assert("Same image, pixel by pixel, despite different byte arrays", Utilities.IsImageEqual(image2, image3));
		}

		public void TestFixNewLinesForEnvironment()
		{
			AssertEquals("FixNewLinesForEnvironment(\"abc\\r\\nde\")", "abc\r\nde", Utilities.FixNewLinesForEnvironment("abc\r\nde"));
			AssertEquals("FixNewLinesForEnvironment(\"abc\\nde\")", "abc\r\nde", Utilities.FixNewLinesForEnvironment("abc\nde"));
			AssertEquals("FixNewLinesForEnvironment(\"abcde\")", "abcde", Utilities.FixNewLinesForEnvironment("abcde"));
			AssertEquals("FixNewLinesForEnvironment(\"abcde\\n\")", "abcde\r\n", Utilities.FixNewLinesForEnvironment("abcde\n"));
			AssertEquals("FixNewLinesForEnvironment(\"\\nabcde\\n\")", "\r\nabcde\r\n", Utilities.FixNewLinesForEnvironment("\nabcde\n"));
			AssertEquals("FixNewLinesForEnvironment(\"\\nab\\r\\ncde\\n\")", "\r\nab\r\ncde\r\n", Utilities.FixNewLinesForEnvironment("\nab\r\ncde\n"));
		}

		public void TestFormatNumberFromZLongNational()
		{
			CombineAssertions(() =>
			{
				var testZlongValue = new ZLong(123);
				using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
				{
					AssertEquals("Aussie Decimals structure", "123.00", Utilities.FormatNumberFromZLongNational(testZlongValue, 2));
				}

				using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("es-ES")))
				{
					AssertEquals("Spanish Decimals structure", "123,00", Utilities.FormatNumberFromZLongNational(testZlongValue, 2));
				}
			});
		}

		#region ToFriendlyTimeString

		public void TestToFriendlyTimeString()
		{
			AssertEquals(string.Empty, TimeSpan.FromSeconds(0).ToFriendlyTimeString());
			AssertEquals(string.Empty, TimeSpan.FromSeconds(1).ToFriendlyTimeString());

			AssertEquals("1 minute", TimeSpan.FromMinutes(1).ToFriendlyTimeString());
			AssertEquals("1 minute", TimeSpan.FromMinutes(1).ToFriendlyTimeString());
			AssertEquals("1 minute", TimeSpan.FromSeconds(65).ToFriendlyTimeString());
			AssertEquals("1.08 minutes", TimeSpan.FromSeconds(65).ToFriendlyTimeString(FriendlyMinutesDisplay.ShowMinutesDecimalPlaces));
			AssertEquals("1 minute and 5 seconds", TimeSpan.FromSeconds(65).ToFriendlyTimeString(FriendlyMinutesDisplay.ShowSeconds));

			AssertEquals("2 minutes", TimeSpan.FromSeconds(110).ToFriendlyTimeString());
			AssertEquals("2 minutes", TimeSpan.FromMinutes(2).ToFriendlyTimeString());
			AssertEquals("2 minutes", TimeSpan.FromMinutes(2).ToFriendlyTimeString(FriendlyMinutesDisplay.ShowSeconds));
			AssertEquals("2 minutes and 1 second", TimeSpan.FromSeconds(121).ToFriendlyTimeString(FriendlyMinutesDisplay.ShowSeconds));
			AssertEquals("2 minutes", TimeSpan.FromMinutes(2).ToFriendlyTimeString());

			AssertEquals("1 hour", TimeSpan.FromHours(1).ToFriendlyTimeString());
			AssertEquals("1 hour", TimeSpan.FromHours(1).ToFriendlyTimeString());
			AssertEquals("1.08 hours", TimeSpan.FromMinutes(65).ToFriendlyTimeString());
			AssertEquals("2 hours", TimeSpan.FromHours(2).ToFriendlyTimeString());
			AssertEquals("2 hours", TimeSpan.FromHours(2).ToFriendlyTimeString());
			AssertEquals("2.5 hours", TimeSpan.FromHours(2.5).ToFriendlyTimeString());

			AssertEquals("1 day", TimeSpan.FromDays(1).ToFriendlyTimeString());
			AssertEquals("2 days", TimeSpan.FromDays(2).ToFriendlyTimeString());

			AssertEquals("1 week", TimeSpan.FromDays(8).ToFriendlyTimeString());
			AssertEquals("2 weeks", TimeSpan.FromDays(15).ToFriendlyTimeString());

			AssertEquals("1 month", TimeSpan.FromDays(32).ToFriendlyTimeString());
			AssertEquals("2 months", TimeSpan.FromDays(64).ToFriendlyTimeString());

			AssertEquals("1 year", TimeSpan.FromDays(380).ToFriendlyTimeString());
			AssertEquals("2 years", TimeSpan.FromDays(800).ToFriendlyTimeString());

			AssertEquals("-1 minute", TimeSpan.FromMinutes(-1).ToFriendlyTimeString());
			AssertEquals("-2 minutes", TimeSpan.FromMinutes(-2).ToFriendlyTimeString());

			AssertEquals("-1 hour", TimeSpan.FromHours(-1).ToFriendlyTimeString());
			AssertEquals("-2 hours", TimeSpan.FromHours(-2).ToFriendlyTimeString());

			AssertEquals("-1 day", TimeSpan.FromDays(-1).ToFriendlyTimeString());
			AssertEquals("-2 days", TimeSpan.FromDays(-2).ToFriendlyTimeString());

			AssertEquals("-1 week", TimeSpan.FromDays(-8).ToFriendlyTimeString());
			AssertEquals("-2 weeks", TimeSpan.FromDays(-15).ToFriendlyTimeString());

			AssertEquals("-1 month", TimeSpan.FromDays(-32).ToFriendlyTimeString());
			AssertEquals("-2 months", TimeSpan.FromDays(-64).ToFriendlyTimeString());

			AssertEquals("-1 year", TimeSpan.FromDays(-380).ToFriendlyTimeString());
			AssertEquals("-2 years", TimeSpan.FromDays(-800).ToFriendlyTimeString());
		}

		#endregion

		#region TestGenerateSecurityToken

		[TestDate(2011, 05, 30, 12, 30, 00)]
		public void TestGenerateSecurityToken()
		{
			string token1 = Utilities.GenerateSecurityToken("JON", "John Smith", "BRC", "DEP");
			string token2 = Utilities.GenerateSecurityToken("JON", "John Smith", "BRC", "DEP");
			AssertSecurityToken(token1, "JON", "John Smith", "BRC", "DEP");
			AssertEquals(token1, token2);

			string token3 = Utilities.GenerateSecurityToken("KAR", "Karen Black");
			AssertSecurityToken(token3, "KAR", "Karen Black");
		}
		void AssertSecurityToken(string securityToken, string username, string password, string branchCode = "", string departmentCode = "")
		{
			TwoWayEncoder encrypter = TwoWayEncoder.NewWithStandardInitialisationVector();
			DateTime now = DateTime.Now; // used to generate security token

			string expectedResult = encrypter.Encrypt(username + password + branchCode + departmentCode +
				now.Year.ToString() + now.Month.ToString() + now.Day.ToString() + now.Hour.ToString());

			AssertEquals(expectedResult, securityToken);
		}

		#endregion

		#region Expression Evaluator

		public void TestDontRunWhileLoopsForever()
		{
			Utilities.ExpressionEvaluator.EvaluateJS("while(true) { /* Javascript goes brrrrr */ }");
			Assert(true);
		}

		public void TestSimpleExpressionsJS()
		{
			AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS("1 == 1").Right);
			AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS("2 == 1 + 1").Right);
			AssertEquals(false, Utilities.ExpressionEvaluator.EvaluateJS("3 < 2").Right);
			AssertEquals(false, Utilities.ExpressionEvaluator.EvaluateJS("1 == 3").Right);
			AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS("1 != 2 && 3 < 4").Right);
			AssertEquals(false, Utilities.ExpressionEvaluator.EvaluateJS("1 < 1").Right);

			AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS("   1 == 1").Right);
			AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS("1 == 1   ").Right);
			AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS("   1 == 1   ").Right);
		}

		public void TestStringExpressionsJS()
		{
			AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS("\"\\n\" != \"\"").Right);
			AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS("\"\\r\" != \"\"").Right);
			AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS("\"\nDesc \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nB \n\nB \n\nMarks \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nB \n\nB \n\" != \"\"").Right);

			AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS("\"").IsLeft);
			AssertEquals(Utilities.ExpressionEvaluator.ErrorCode.InvalidExpression, Utilities.ExpressionEvaluator.EvaluateJS("\"").Left);
		}

		public void TestEmptyExpressionsJS()
		{
			Assert(Utilities.ExpressionEvaluator.EvaluateJS(string.Empty).IsLeft);
			AssertEquals(Utilities.ExpressionEvaluator.ErrorCode.EmptyExpression,
				Utilities.ExpressionEvaluator.EvaluateJS(string.Empty).Left);

			Assert(Utilities.ExpressionEvaluator.EvaluateJS(null).IsLeft);
			AssertEquals(Utilities.ExpressionEvaluator.ErrorCode.EmptyExpression,
				Utilities.ExpressionEvaluator.EvaluateJS(null).Left);

			Assert(Utilities.ExpressionEvaluator.EvaluateJS(new string(' ', 3)).IsLeft);
			AssertEquals(Utilities.ExpressionEvaluator.ErrorCode.EmptyExpression,
				Utilities.ExpressionEvaluator.EvaluateJS(new string(' ', 3)).Left);
		}

		public void TestEscapeQuotesWhenUsingJSEvaluation()
		{
			const string expr = "\"A\" Line Shipping\" == \"A\" Line Shipping\"";

			var res = Utilities.ExpressionEvaluator.EvaluateJS(expr);

			AssertEquals($"expression {expr} has been evaluated sucessfully", true, res.IsRight);
			AssertEquals($"expression {expr} result", true, res.Right);
		}

		public void TestInvalidMacroExpressionThrowsErrorJS()
		{
			var result = Utilities.ExpressionEvaluator.EvaluateJS("\"a\" = \"\"");

			AssertEquals(true, result.IsLeft);
			AssertEquals(Utilities.ExpressionEvaluator.ErrorCode.NotTrueFalseExpression, result.Left);
		}

		public void TestNewJSIsFaster()
		{
			var expr = "1 == 1";
			int count = 100000;
			var sw = new Stopwatch();
			sw.Start();
			for (int i = 0; i < count; i++)
			{
				AssertEquals(true, StringToObjectJS_OLD(expr).Right);
			}
			sw.Stop();
			var r1 = sw.ElapsedTicks;

			sw.Restart();
			for (int i = 0; i < count; i++)
			{
				AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS(expr).Right);
			}
			sw.Stop();
			var r2 = sw.ElapsedMilliseconds;
			AssertGreaterThan(r1, r2);
		}

		public void TestMultiThreading()
		{
			var expr = "1 == 1";
			var sw = new Stopwatch();
			Parallel.ForEach(Enumerable.Range(0, 100000), a => AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS(expr).Right));
			Assert(true);
		}

		public void TestNoLeaking()
		{
			var expr = @"(function (a) { if (typeof x !== 'undefined') { x = 2; } else { x = 1; } return x == 1 })(1)";
			AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS(expr).Right);
			AssertEquals(true, Utilities.ExpressionEvaluator.EvaluateJS(expr).Right);
		}

		static Either<ErrorCode, object> StringToObjectJS_OLD(string expression, bool isRetry = false)
		{
#if NETFRAMEWORK
#pragma warning disable 0618
			var engine = VsaEngine.CreateEngine();
#pragma warning restore 0618

			expression = expression.Replace("\n", "\\n");
			expression = expression.Replace("\r", "\\r");
			try
			{
#pragma warning disable 0618
				return Microsoft.JScript.Eval.JScriptEvaluate(expression, engine);
#pragma warning restore 0618
			}
			catch (IndexOutOfRangeException) when (!isRetry)
			{
				//Sometimes JScriptEvaluate fails with IndexOutOfRangeException even when the expression looks all good. The chance of occuring this issue is 
				//pretty low e.g. this could happen 0 to 2 times when rendering 21000 documents. Doing the evaluation a second time can fix this issue.
#pragma warning disable 0618
				return StringToObjectJS_OLD(expression, true);
#pragma warning restore 0618
			}
			catch (Microsoft.JScript.JScriptException)
			{
				return ErrorCode.JScriptEngineError;
			}
#else
			throw new NotImplementedException("Microsoft.JScript is not supported on .net core. Is this test still required? If so, perhaps look at ClearScript? https://github.com/microsoft/ClearScript has Microsoft.JScript engine.");
#endif
		}

		#endregion

		public void TestIsSubclassOfRawGeneric()
		{
			AssertEquals("IsSubclassOfRawGeneric - GenericClass<>", true, typeof(Test).IsSubclassOfRawGeneric(typeof(GenericClass<>)));
			AssertEquals("IsSubclassOfRawGeneric - Typle<>", false, typeof(Test).IsSubclassOfRawGeneric(typeof(Tuple<>)));
		}

		class GenericClass<T>
		{
		}

		class Test : GenericClass<object>
		{
		}

		#region Implementation

		Guid SydGuid;

		protected override void SetUp()
		{
			base.SetUp();

			Db.Connection.ExecuteNonQuery("update dbo.RefUNLOCO Set " + RefUNLOCOSchema.Constants.RL_RW + " = 'B957B026-C0B9-41A1-960C-BAC376AF14BE' where RL_CODE = 'AUBDB'"); // This is O so can't use Z
			Db.Connection.ExecuteNonQuery("update dbo.RefUNLOCO Set " + RefUNLOCOSchema.Constants.RL_RW + " = 'CE2FEFCA-7CAC-49B8-9DFF-7096CBC1F4A3' where RL_CODE = 'AUSYD'"); // This is O so can't use Z

			SydGuid = (Guid)Db.Connection.ExecuteScalar("select RL_PK from dbo.RefUNLOCO where RL_CODE = 'AUSYD'");   // This is O so can't use Z
		}

		#endregion
	}
}
