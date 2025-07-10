using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Business.Update.Testing
{
	class OrgAddressAdditionalInfoInterceptorTest : TestCaseWithFactory
	{
		#region AdditionalInfo

		public void TestImportXml_DuplicateAdditionalInfoCase1()
		{
			var address = CreateAddress(null, null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(false, address.AdditionalInfos.Any());
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Walker", "Walker", false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXml_DuplicateAdditionalInfoCase2()
		{
			var address = CreateAddress(null, null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(false, address.AdditionalInfos.Any());
			});

			ProcessNativeXml_AdditionalInfo(address, "Walker", "Walker", "Walker", false);
			CombineAssertions(() =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasNoValue_Case1()
		{
			var address = CreateAddress(null, null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(false, address.AdditionalInfos.Any());
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Peter", null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasNoValue_Case2()
		{
			var address = CreateAddress(null, null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(false, address.AdditionalInfos.Any());
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Peter", "Walker", false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasNoValue_Case3()
		{
			var address = CreateAddress(null, null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(false, address.AdditionalInfos.Any());
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Walker", null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasNoValue_Case4()
		{
			var address = CreateAddress(null, null);
			CombineAssertions("Precondition: ", () =>
			{ 
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(false, address.AdditionalInfos.Any());
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Walker", "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasNoValue_Case5()
		{
			var address = CreateAddress(null, null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(false, address.AdditionalInfos.Any());
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", null, "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasNoValue_Case6()
		{
			var address = CreateAddress(null, null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(false, address.AdditionalInfos.Any());
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", null, null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasNoValue_Case7()
		{
			var address = CreateAddress(null, null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(false, address.AdditionalInfos.Any());
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Peter", null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasNoValue_Case8()
		{
			var address = CreateAddress(null, null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(false, address.AdditionalInfos.Any());
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Walker", "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasNoValue_Case9()
		{
			var address = CreateAddress(null, null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(false, address.AdditionalInfos.Any());
			});

			ProcessNativeXml_AdditionalInfo(address, null, null, "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasNoValue_Case10()
		{
			var address = CreateAddress(null, null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(false, address.AdditionalInfos.Any());
			});

			ProcessNativeXml_AdditionalInfo(address, null, null, null, false);
			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(false, address.AdditionalInfos.Any());
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXml_Case1()
		{
			var address = CreateAddress("Peter", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Peter", null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXml_Case2()
		{
			var address = CreateAddress("Peter", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Peter", "Walker", false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXml_Case3()
		{
			var address = CreateAddress("Peter", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Walker", null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXml_Case4()
		{
			var address = CreateAddress("Peter", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Walker", "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXml_Case5()
		{
			var address = CreateAddress("Peter", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", null, "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXml_Case6()
		{
			var address = CreateAddress("Peter", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", null, null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXml_Case7()
		{
			var address = CreateAddress("Peter", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Peter", null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXml_Case8()
		{
			var address = CreateAddress("Peter", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Walker", null, true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXml_Case9()
		{
			var address = CreateAddress("Peter", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Walker", "Peter", true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXml_Case10()
		{
			var address = CreateAddress("Peter", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, null, "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXml_Case11()
		{
			var address = CreateAddress("Peter", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, null, null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXmlAndNonPrimaryAdditionalInfo_Case1()
		{
			var address = CreateAddress("Peter", "Walker");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Peter", null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXmlAndNonPrimaryAdditionalInfo_Case2()
		{
			var address = CreateAddress("Peter", "Walker");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Peter", "Walker", false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXmlAndNonPrimaryAdditionalInfo_Case3()
		{
			var address = CreateAddress("Peter", "Walker");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Walker", null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXmlAndNonPrimaryAdditionalInfo_Case4()
		{
			var address = CreateAddress("Peter", "Walker");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Walker", "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXmlAndNonPrimaryAdditionalInfo_Case5()
		{
			var address = CreateAddress("Peter", "Walker");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", null, "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXmlAndNonPrimaryAdditionalInfo_Case6()
		{
			var address = CreateAddress("Peter", "Walker");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", null, null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXmlAndNonPrimaryAdditionalInfo_Case7()
		{
			var address = CreateAddress("Peter", "Walker");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Peter", null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXmlAndNonPrimaryAdditionalInfo_Case8()
		{
			var address = CreateAddress("Peter", "Walker");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Walker", null, true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXmlAndNonPrimaryAdditionalInfo_Case9()
		{
			var address = CreateAddress("Peter", "Walker");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Walker", "Peter", true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXmlAndNonPrimaryAdditionalInfo_Case10()
		{
			var address = CreateAddress("Peter", "Walker");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, null, "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(3, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoSameAsXmlAndNonPrimaryAdditionalInfo_Case11()
		{
			var address = CreateAddress("Peter", "Walker");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, null, null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Peter", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXmlAndNonPrimaryAdditionalInfo_Case1()
		{
			var address = CreateAddress("Walker", "Peter");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Peter", null, true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXmlAndNonPrimaryAdditionalInfo_Case2()
		{
			var address = CreateAddress("Walker", "Peter");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Peter", "Walker", true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXmlAndNonPrimaryAdditionalInfo_Case3()
		{
			var address = CreateAddress("Walker", "Peter");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Walker", null, true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXmlAndNonPrimaryAdditionalInfo_Case4()
		{
			var address = CreateAddress("Walker", "Peter");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Walker", "Peter", true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXmlAndNonPrimaryAdditionalInfo_Case5()
		{
			var address = CreateAddress("Walker", "Peter");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", null, "Peter", true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXmlAndNonPrimaryAdditionalInfo_Case6()
		{
			var address = CreateAddress("Walker", "Peter");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", null, null, true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXmlAndNonPrimaryAdditionalInfo_Case7()
		{
			var address = CreateAddress("Walker", "Peter");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Peter", null, true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXmlAndNonPrimaryAdditionalInfo_Case8()
		{
			var address = CreateAddress("Walker", "Peter");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Walker", null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXmlAndNonPrimaryAdditionalInfo_Case9()
		{
			var address = CreateAddress("Walker", "Peter");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Walker", "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXmlAndNonPrimaryAdditionalInfo_Case10()
		{
			var address = CreateAddress("Walker", "Peter");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, null, "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXmlAndNonPrimaryAdditionalInfo_Case11()
		{
			var address = CreateAddress("Walker", "Peter");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, null, null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXml_Case1()
		{
			var address = CreateAddress("Walker", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Peter", null, true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXml_Case2()
		{
			var address = CreateAddress("Walker", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Peter", "Walker", true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXml_Case3()
		{
			var address = CreateAddress("Walker", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Walker", null, true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXml_Case4()
		{
			var address = CreateAddress("Walker", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", "Walker", "Peter", true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXml_Case5()
		{
			var address = CreateAddress("Walker", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", null, "Peter", true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXml_Case6()
		{
			var address = CreateAddress("Walker", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, "Peter", null, null, true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXml_Case7()
		{
			var address = CreateAddress("Walker", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Peter", null, true);
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXml_Case8()
		{
			var address = CreateAddress("Walker", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Walker", null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXml_Case9()
		{
			var address = CreateAddress("Walker", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, "Walker", "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXml_Case10()
		{
			var address = CreateAddress("Walker", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, null, "Peter", false);
			CombineAssertions(() =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Peter" && !a.OAI_IsPrimary));
			});
		}

		public void TestImportXmlWhenDbHasPrimaryAdditionalInfoConflictWithXml_Case11()
		{
			var address = CreateAddress("Walker", null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});

			ProcessNativeXml_AdditionalInfo(address, null, null, null, false);
			CombineAssertions(() =>
			{
				AssertEquals("Walker", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos.Any(a => a.OAI_AdditionalInfo == "Walker" && a.OAI_IsPrimary));
			});
		}
		#endregion

		#region TranslatedAddressAdditionalInfo
		public void TestImportXml_TranslatedAdditionalInfo_Case0()
		{
			var address = InitializeDatabase_TranslatedAdditionalInfo(SharedConstants.Languages.English, "A");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals(SharedConstants.Languages.English, address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("A", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(SharedConstants.Languages.English, address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("A", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			ProcessNativeXml_TranslatedAdditionalInfo(address, SharedConstants.Languages.English, "B", SharedConstants.Languages.English, "B");
			CombineAssertions(() =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals(SharedConstants.Languages.English, address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("B", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(SharedConstants.Languages.English, address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("B", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});
		}

		public void TestImportXml_TranslatedAdditionalInfo_Case1()
		{
			var address = InitializeDatabase_TranslatedAdditionalInfo(SharedConstants.Languages.English, "A");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals(SharedConstants.Languages.English, address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("A", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(SharedConstants.Languages.English, address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("A", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			ProcessNativeXml_TranslatedAdditionalInfo(address, SharedConstants.Languages.English, "B", SharedConstants.Languages.ChineseSimplified, "A");
			CombineAssertions(() =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals(SharedConstants.Languages.English, address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("B", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(SharedConstants.Languages.English, address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("B", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});
		}

		public void TestImportXml_TranslatedAdditionalInfo_Case2()
		{
			var address = InitializeDatabase_TranslatedAdditionalInfo(SharedConstants.Languages.ChineseSimplified, "A");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals(SharedConstants.Languages.ChineseSimplified, address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("A", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(SharedConstants.Languages.ChineseSimplified, address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("A", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			ProcessNativeXml_TranslatedAdditionalInfo(address, SharedConstants.Languages.English, "A", SharedConstants.Languages.EnglishBritish, "A");
			CombineAssertions(() =>
			{
				AssertEquals(2, address.TranslatedAddresses.Count);
				AssertEquals(true, address.TranslatedAddresses.Any(t => t.OTA_Language == SharedConstants.Languages.ChineseSimplified && t.OTA_AdditionalAddressInformation == "A"));
				AssertEquals(true, address.TranslatedAddresses.Any(t => t.OTA_Language == SharedConstants.Languages.English && t.OTA_AdditionalAddressInformation == "A"));
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(2, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].TranslatedInfos.Any(t => t.OTI_Language == SharedConstants.Languages.ChineseSimplified && t.OTI_AdditionalInfo == "A"));
				AssertEquals(true, address.AdditionalInfos[0].TranslatedInfos.Any(t => t.OTI_Language == SharedConstants.Languages.English && t.OTI_AdditionalInfo == "A"));
			});
		}

		public void TestImportXml_TranslatedAdditionalInfo_Case3()
		{
			var address = InitializeDatabase_TranslatedAdditionalInfo(SharedConstants.Languages.ChineseSimplified, "A");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals(SharedConstants.Languages.ChineseSimplified, address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("A", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(SharedConstants.Languages.ChineseSimplified, address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("A", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			ProcessNativeXml_TranslatedAdditionalInfo(address, SharedConstants.Languages.English, null, SharedConstants.Languages.English, "A");
			CombineAssertions(() =>
			{
				AssertEquals(2, address.TranslatedAddresses.Count);
				AssertEquals(true, address.TranslatedAddresses.Any(t => t.OTA_Language == SharedConstants.Languages.ChineseSimplified && t.OTA_AdditionalAddressInformation == "A"));
				AssertEquals(true, address.TranslatedAddresses.Any(t => t.OTA_Language == SharedConstants.Languages.English && t.OTA_AdditionalAddressInformation == "A"));
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(2, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].TranslatedInfos.Any(t => t.OTI_Language == SharedConstants.Languages.ChineseSimplified && t.OTI_AdditionalInfo == "A"));
				AssertEquals(true, address.AdditionalInfos[0].TranslatedInfos.Any(t => t.OTI_Language == SharedConstants.Languages.English && t.OTI_AdditionalInfo == "A"));
			});
		}

		public void TestImportXml_TranslatedAdditionalInfo_Case4()
		{
			var address = InitializeDatabase_TranslatedAdditionalInfo(SharedConstants.Languages.English, "A");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals(SharedConstants.Languages.English, address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("A", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(SharedConstants.Languages.English, address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("A", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			ProcessNativeXml_TranslatedAdditionalInfo(address, SharedConstants.Languages.English, null, SharedConstants.Languages.ChineseSimplified, null);
			CombineAssertions(() =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals(SharedConstants.Languages.English, address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("A", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(SharedConstants.Languages.English, address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("A", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});
		}

		public void TestImportXml_TranslatedAdditionalInfo_Case5()
		{
			var address = InitializeDatabase_TranslatedAdditionalInfo(SharedConstants.Languages.English, "A");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals(SharedConstants.Languages.English, address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("A", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(SharedConstants.Languages.English, address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("A", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			ProcessNativeXml_TranslatedAdditionalInfo(address, SharedConstants.Languages.English, null, null, null);
			CombineAssertions(() =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals(SharedConstants.Languages.English, address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("A", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(SharedConstants.Languages.English, address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("A", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});
		}

		public void TestImportXml_TranslatedAdditionalInfo_Case6()
		{
			var address = InitializeDatabase_TranslatedAdditionalInfo(null, null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(false, address.TranslatedAddresses.Any());
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(false, address.AdditionalInfos[0].TranslatedInfos.Any());
			});

			ProcessNativeXml_TranslatedAdditionalInfo(address, SharedConstants.Languages.English, "A", null, null);
			CombineAssertions(() =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals(SharedConstants.Languages.English, address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("A", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(SharedConstants.Languages.English, address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("A", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});
		}

		public void TestImportXml_TranslatedAdditionalInfo_Case7()
		{
			var address = InitializeDatabase_TranslatedAdditionalInfo(SharedConstants.Languages.English, "A");
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals(SharedConstants.Languages.English, address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("A", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(SharedConstants.Languages.English, address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("A", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			ProcessNativeXml_TranslatedAdditionalInfo(address, null, null, null, null);
			CombineAssertions(() =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals(SharedConstants.Languages.English, address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("A", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals(SharedConstants.Languages.English, address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("A", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});
		}

		public void TestImportXml_TranslatedAdditionalInfo_Case8()
		{
			var address = InitializeDatabase_TranslatedAdditionalInfo(null, null);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(false, address.TranslatedAddresses.Any());
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(false, address.AdditionalInfos[0].TranslatedInfos.Any());
			});

			ProcessNativeXml_TranslatedAdditionalInfo(address, null, null, null, null);
			CombineAssertions(() =>
			{
				AssertEquals(false, address.TranslatedAddresses.Any());
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(false, address.AdditionalInfos[0].TranslatedInfos.Any());
			});
		}

		public void TestArgumentException1()
		{
			var address = InitializeDatabase_TranslatedAdditionalInfo(null, null);
			var importXml = $@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
	<OwnerCode>EDICUS</OwnerCode>
	<EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
	<Organization version=""2.0"">
	  <OrgHeader Action=""MERGE"">
		<PK>{address.Header.PK}</PK>
		<Code>{address.Header.OH_Code}</Code>
		<IsActive>true</IsActive>
		<FullName>{address.Header.OH_FullName}</FullName>
		<OrgAddressCollection>
			<OrgAddress Action=""MERGE"">
				<PK>{address.PK}</PK>
				<Code>{address.OA_Code}</Code>
				<AdditionalAddressInformation>A</AdditionalAddressInformation>
				<OrgAddressAdditionalInfoCollection>
					<OrgAddressAdditionalInfo Action=""MERGE"">
						<PK>bd6d3478-09fa-4929-8341-567ad82e7772</PK>
						<IsPrimary>true</IsPrimary>
						<AdditionalInfo> </AdditionalInfo>
					</OrgAddressAdditionalInfo>
				</OrgAddressAdditionalInfoCollection>
			</OrgAddress>
		</OrgAddressCollection>
		<ClosestPort TableName=""RefUNLOCO"">
		  <Code>AUSYD</Code>
		  <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
		</ClosestPort>
	  </OrgHeader>
	</Organization>
  </Body>
</Native>";

			importHandler.Import(new MemoryStream(Encoding.Default.GetBytes(importXml)));
			var importLog = string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray());
			AssertContains("Error occurred trying to import file. Please fix the error and try importing the file again.\r\nNo insert/update action performed.", importLog);
		}

		public void TestArgumentException2()
		{
			var address = InitializeDatabase_TranslatedAdditionalInfo(null, null);
			var importXml = $@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
	<OwnerCode>EDICUS</OwnerCode>
	<EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
	<Organization version=""2.0"">
	  <OrgHeader Action=""MERGE"">
		<PK>{address.Header.PK}</PK>
		<Code>{address.Header.OH_Code}</Code>
		<IsActive>true</IsActive>
		<FullName>{address.Header.OH_FullName}</FullName>
		<OrgAddressCollection>
			<OrgAddress Action=""MERGE"">
				<PK>{address.PK}</PK>
				<Code>{address.OA_Code}</Code>
				<AdditionalAddressInformation>A</AdditionalAddressInformation>
				<OrgAddressAdditionalInfoCollection>
					<OrgAddressAdditionalInfo Action=""MERGE"">
						<PK>bd6d3478-09fa-4929-8341-567ad82e7772</PK>
						<IsPrimary>true</IsPrimary>
						<AdditionalInfo>A</AdditionalInfo>
							<OrgTranslatedAddressAdditionalInfoCollection>
								<OrgTranslatedAddressAdditionalInfo Action=""MERGE"">
									<PK>be4ec2ea-280d-4f1c-b003-1df977290634</PK>
									<Language>EN-US</Language>
									<AdditionalInfo> </AdditionalInfo>
								</OrgTranslatedAddressAdditionalInfo>
							</OrgTranslatedAddressAdditionalInfoCollection>
					</OrgAddressAdditionalInfo>
				</OrgAddressAdditionalInfoCollection>
			</OrgAddress>
		</OrgAddressCollection>
		<ClosestPort TableName=""RefUNLOCO"">
		  <Code>AUSYD</Code>
		  <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
		</ClosestPort>
	  </OrgHeader>
	</Organization>
  </Body>
</Native>";

			importHandler.Import(new MemoryStream(Encoding.Default.GetBytes(importXml)));
			var importLog = string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray());
			AssertContains("Error occurred trying to import file. Please fix the error and try importing the file again.\r\nNo insert/update action performed.", importLog);
		}

		public void TestArgumentException3()
		{
			var address = InitializeDatabase_TranslatedAdditionalInfo(null, null);
			var importXml = $@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
	<OwnerCode>EDICUS</OwnerCode>
	<EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
	<Organization version=""2.0"">
	  <OrgHeader Action=""MERGE"">
		<PK>{address.Header.PK}</PK>
		<Code>{address.Header.OH_Code}</Code>
		<IsActive>true</IsActive>
		<FullName>{address.Header.OH_FullName}</FullName>
		<OrgAddressCollection>
			<OrgAddress Action=""MERGE"">
				<PK>{address.PK}</PK>
				<Code>{address.OA_Code}</Code>
				<AdditionalAddressInformation>A</AdditionalAddressInformation>
				<OrgAddressAdditionalInfoCollection>
					<OrgAddressAdditionalInfo Action=""MERGE"">
						<PK>bd6d3478-09fa-4929-8341-567ad82e7772</PK>
						<IsPrimary>true</IsPrimary>
						<AdditionalInfo>ABC</AdditionalInfo>
					</OrgAddressAdditionalInfo>
					<OrgAddressAdditionalInfo Action=""MERGE"">
						<PK>316cdb32-3a16-4380-901c-6049d03778b0</PK>
						<IsPrimary>True</IsPrimary>
						<AdditionalInfo>DEF</AdditionalInfo>
					</OrgAddressAdditionalInfo>
				</OrgAddressAdditionalInfoCollection>
			</OrgAddress>
		</OrgAddressCollection>
		<ClosestPort TableName=""RefUNLOCO"">
		  <Code>AUSYD</Code>
		  <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
		</ClosestPort>
	  </OrgHeader>
	</Organization>
  </Body>
</Native>";

			importHandler.Import(new MemoryStream(Encoding.Default.GetBytes(importXml)));
			var importLog = string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray());
			AssertContains("Error occurred trying to import file. Please fix the error and try importing the file again.\r\nNo insert/update action performed.", importLog);
		}

		public void TestArgumentException4()
		{
			var address = InitializeDatabase_TranslatedAdditionalInfo(null, null);
			var importXml = $@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
	<OwnerCode>EDICUS</OwnerCode>
	<EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
	<Organization version=""2.0"">
	  <OrgHeader Action=""MERGE"">
		<PK>{address.Header.PK}</PK>
		<Code>{address.Header.OH_Code}</Code>
		<IsActive>true</IsActive>
		<FullName>{address.Header.OH_FullName}</FullName>
		<OrgAddressCollection>
			<OrgAddress Action=""MERGE"">
				<PK>{address.PK}</PK>
				<Code>{address.OA_Code}</Code>
				<AdditionalAddressInformation>A</AdditionalAddressInformation>
				<OrgAddressAdditionalInfoCollection>
					<OrgAddressAdditionalInfo Action=""MERGE"">
						<PK>bd6d3478-09fa-4929-8341-567ad82e7772</PK>
						<IsPrimary>true</IsPrimary>
						<AdditionalInfo>A</AdditionalInfo>
							<OrgTranslatedAddressAdditionalInfoCollection>
								<OrgTranslatedAddressAdditionalInfo Action=""MERGE"">
									<PK>be4ec2ea-280d-4f1c-b003-1df977290634</PK>
									<Language>EN-US</Language>
									<AdditionalInfo>B</AdditionalInfo>
								</OrgTranslatedAddressAdditionalInfo>
								<OrgTranslatedAddressAdditionalInfo Action=""MERGE"">
									<PK>c29c74fd-7f89-4ea2-8af6-64add261148e</PK>
									<Language>EN-US</Language>
									<AdditionalInfo>C</AdditionalInfo>
								</OrgTranslatedAddressAdditionalInfo>
							</OrgTranslatedAddressAdditionalInfoCollection>
					</OrgAddressAdditionalInfo>
				</OrgAddressAdditionalInfoCollection>
			</OrgAddress>
		</OrgAddressCollection>
		<ClosestPort TableName=""RefUNLOCO"">
		  <Code>AUSYD</Code>
		  <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
		</ClosestPort>
	  </OrgHeader>
	</Organization>
  </Body>
</Native>";

			importHandler.Import(new MemoryStream(Encoding.Default.GetBytes(importXml)));
			var importLog = string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray());
			AssertContains("Error occurred trying to import file. Please fix the error and try importing the file again.\r\nNo insert/update action performed.", importLog);
		}

		#endregion

		OrgAddress CreateAddress(string additionalInfo_Main, string additionalInfo_NotMain)
		{
			var header = CreateOrganization();
			var address = header.MainAddress;
			InitializeDatabase_AdditionalInfo(address, additionalInfo_Main, additionalInfo_NotMain);

			return address;
		}

		void InitializeDatabase_AdditionalInfo(OrgAddress address, string additionalInfo_Main, string additionalInfo_NotMain)
		{
			address.OA_AdditionalAddressInformation = additionalInfo_Main ?? string.Empty;

			if (additionalInfo_NotMain != null)
			{
				var additionalInfo2 = address.AdditionalInfos.AddNew();
				additionalInfo2.OAI_AdditionalInfo = additionalInfo_NotMain;
				additionalInfo2.OAI_IsPrimary = false;
			}

			Factory.Save();
		}

		OrgAddress InitializeDatabase_TranslatedAdditionalInfo(string language, string additionalInfo)
		{
			var address = CreateAddress("Peter", null);

			if (language != null && additionalInfo != null)
			{
				var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
				translatedAddress.OTA_OA = address.PK;
				translatedAddress.OTA_Address1 = "AAA";
				translatedAddress.OTA_AdditionalAddressInformation = additionalInfo;
				translatedAddress.OTA_Language = language;

				Factory.Save();
			}

			return address;
		}

		void ProcessNativeXml_AdditionalInfo(OrgAddress address, string additionalAddressInformation, string additionalInfo_Main, string additionalInfo_NotMain, bool errorOccurred)
		{
			var additionalAddressInformation_xml = additionalAddressInformation == null ?
				string.Empty : $@"<AdditionalAddressInformation>{additionalAddressInformation}</AdditionalAddressInformation>";

			var additionalInfo_Main_Xml = additionalInfo_Main == null ?
				string.Empty : $@"<OrgAddressAdditionalInfo Action=""MERGE"">
								<PK>bd6d3478-09fa-4929-8341-567ad82e7772</PK>
								<IsPrimary>true</IsPrimary>
								<AdditionalInfo>{additionalInfo_Main}</AdditionalInfo>
					</OrgAddressAdditionalInfo>";

			var additionalInfo_NotMain_Xml = additionalInfo_NotMain == null ?
				string.Empty : System.Environment.NewLine +
					$@"<OrgAddressAdditionalInfo Action=""MERGE"">
								<PK>641ef428-9d53-4835-b6b3-174063547a59</PK>
								<IsPrimary>false</IsPrimary>
								<AdditionalInfo>{additionalInfo_NotMain}</AdditionalInfo>
								</OrgAddressAdditionalInfo>";

			var additionalInfoCollectionNode1 = additionalInfo_Main == null && additionalInfo_NotMain == null ? string.Empty : "<OrgAddressAdditionalInfoCollection>";
			var additionalInfoCollectionNode2 = additionalInfo_Main == null && additionalInfo_NotMain == null ? string.Empty : "</OrgAddressAdditionalInfoCollection>";

			var importXml = $@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
	<OwnerCode>EDICUS</OwnerCode>
	<EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
	<Organization version=""2.0"">
	  <OrgHeader Action=""MERGE"">
		<PK>{address.Header.PK}</PK>
		<Code>{address.Header.OH_Code}</Code>
		<IsActive>true</IsActive>
		<FullName>{address.Header.OH_FullName}</FullName>
		<OrgAddressCollection>
			<OrgAddress Action=""MERGE"">
				<PK>{address.PK}</PK>
				<Code>{address.OA_Code}</Code>
				{additionalAddressInformation_xml}
				{additionalInfoCollectionNode1}
					{additionalInfo_Main_Xml}{additionalInfo_NotMain_Xml}
				{additionalInfoCollectionNode2}
			</OrgAddress>
		</OrgAddressCollection>
		<ClosestPort TableName=""RefUNLOCO"">
		  <Code>AUSYD</Code>
		  <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
		</ClosestPort>
	  </OrgHeader>
	</Organization>
  </Body>
</Native>";

			importHandler.Import(new MemoryStream(Encoding.Default.GetBytes(importXml)));
			var importLog = string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray());
			var errorMessage = "Error occurred trying to import file. Please fix the error and try importing the file again.\r\nNo insert/update action performed.";
			if (errorOccurred)
			{
				AssertEquals(errorMessage, importLog);
			}
			else
			{
				AssertNotEquals(errorMessage, importLog);
				address.Reload();
				address.AdditionalInfos.RefreshFromDb();
			}
		}

		void ProcessNativeXml_TranslatedAdditionalInfo(OrgAddress address, string oldLanguage, string oldAdditionalInfo, string newLanguage, string newAdditionalInfo)
		{
			var new_Language_Xml = newLanguage == null ? string.Empty : $"<Language>{newLanguage}</Language>";
			var new_AdditionalInfo_Xml = newAdditionalInfo == null ? string.Empty : $@"
					<AdditionalInfo>{newAdditionalInfo}</AdditionalInfo>";

			var translatedAdditionalInfo_Xml = newLanguage == null && newAdditionalInfo == null ?
				string.Empty :
				$@"<OrgTranslatedAddressAdditionalInfoCollection>
                  <OrgTranslatedAddressAdditionalInfo Action=""MERGE"">
                    <PK>be4ec2ea-280d-4f1c-b003-1df977290634</PK>
                    {new_Language_Xml}{new_AdditionalInfo_Xml}
                  </OrgTranslatedAddressAdditionalInfo>
                </OrgTranslatedAddressAdditionalInfoCollection>";

			var old_Language_Xml = oldLanguage == null ? string.Empty : $"<Language>{oldLanguage}</Language>";
			var old_AdditionalInfo_Xml = oldAdditionalInfo == null ? string.Empty : $@"
				<AdditionalAddressInformation>{oldAdditionalInfo}</AdditionalAddressInformation>";
			var translatedAddress_Xml = oldLanguage == null && oldAdditionalInfo == null ?
				string.Empty :
				$@"<OrgTranslatedAddressCollection>
              <OrgTranslatedAddress Action=""MERGE"">
                <PK>7ac5f224-c9f6-4eca-a8c6-274567334aa6</PK>
                <Address1>AAA</Address1>
                <Address2></Address2>
                <City></City>
                <PostCode></PostCode>
                <AddressMap></AddressMap>
                <CompanyName></CompanyName>
                <State></State>
                {old_Language_Xml}{old_AdditionalInfo_Xml}
              </OrgTranslatedAddress>
            </OrgTranslatedAddressCollection>";

			var importXml = $@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
	<OwnerCode>EDICUS</OwnerCode>
	<EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
	<Organization version=""2.0"">
	  <OrgHeader Action=""MERGE"">
		<PK>{address.Header.PK}</PK>
		<Code>{address.Header.OH_Code}</Code>
		<IsActive>true</IsActive>
		<FullName>{address.Header.OH_FullName}</FullName>
		<OrgAddressCollection>
			<OrgAddress Action=""MERGE"">
				<PK>{address.PK}</PK>
				<Code>{address.OA_Code}</Code>
				<AdditionalAddressInformation>Peter</AdditionalAddressInformation>
				<OrgAddressAdditionalInfoCollection>
					<OrgAddressAdditionalInfo Action=""MERGE"">
						<PK>bd6d3478-09fa-4929-8341-567ad82e7772</PK>
						<IsPrimary>true</IsPrimary>
						<AdditionalInfo>Peter</AdditionalInfo>
						{translatedAdditionalInfo_Xml}
					</OrgAddressAdditionalInfo>
				</OrgAddressAdditionalInfoCollection>
				{translatedAddress_Xml}
			</OrgAddress>
		</OrgAddressCollection>
		<ClosestPort TableName=""RefUNLOCO"">
		  <Code>AUSYD</Code>
		  <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
		</ClosestPort>
	  </OrgHeader>
	</Organization>
  </Body>
</Native>";

			importHandler.Import(new MemoryStream(Encoding.Default.GetBytes(importXml)));
			address.Reload();
			address.TranslatedAddresses?.RefreshFromDb();
			address.AdditionalInfos?.RefreshFromDb();
			address.AdditionalInfos?.ForEach(a => a.TranslatedInfos?.RefreshFromDb());
		}

		#region Implementation

		AncillaryImportServices sessionServices;
		ImportHandler importHandler;

		OrgHeader CreateOrganization()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Tesla Automotive";
			header.OH_RL_NKClosestPort = "AUSYD";

			var address = header.MainAddress;
			address.OA_Address1 = "BOURKE STREET";

			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();

			sessionServices = new AncillaryImportServices();
			importHandler = new ImportHandler(sessionServices);
		}

		#endregion
	}
}
