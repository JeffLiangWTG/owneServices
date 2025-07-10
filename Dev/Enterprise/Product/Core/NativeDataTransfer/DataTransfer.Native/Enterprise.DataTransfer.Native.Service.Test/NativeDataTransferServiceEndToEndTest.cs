using System;
using System.ServiceModel;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Service.Client;
using Enterprise.DataTransfer.Native.Service.TestData;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Service
{
	[WithNativeDataTransferService]
	class NativeDataTransferServiceEndToEndTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public void TestNativeDataTransferRetrieveOrgHeaders()
		{
			var nativeDataTransferService = WithNativeDataTransferServiceAttribute.NativeDataTransferService;
			var remoteAddress = nativeDataTransferService.GetUrl(NativeDataService);

			CreateOrgHeader("TestOrg1", "TestOrg12", "TestAltOrg1", "TestAltOrg2");
			CombineAssertions(() =>
			{
				foreach (var testRetrieveFile in TestDataHelper.TestRetrieveOrgHeaders)
				{
					TestRetrieve(testRetrieveFile);
				}
			});

			void CreateOrgHeader(params string[] orgHeaderCodes)
			{
				foreach (var orgHeaderCode in orgHeaderCodes)
				{
					var orgHeader = Factory.New<OrgHeader>();
					orgHeader.OH_Code = orgHeaderCode;
				}

				Factory.Save();
			}

			void TestRetrieve(string xml, Action<string> assertServiceResponse = null)
			{
				var address = new EndpointAddress(remoteAddress);
				var binding = new WSHttpBinding(SecurityMode.None);

				string response;
				using (var client = new EnterpriseNativeDataServiceClient(binding, address))
				{
					response = client.Retrieve(XElement.Parse(xml))?.ToString();
					client.Close();
				}

				var failureMessage = $@"Requested:
{xml};
Responded:
{response}";
				Assert(response, !string.IsNullOrEmpty(response));
				Assert(failureMessage, response.Contains("<Status>Accepted</Status>"));

				assertServiceResponse?.Invoke(response);
			}
		}

		const string NativeDataService = "EnterpriseNativeDataService.svc";
	}
}
