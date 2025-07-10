using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicPayment.Common;
using Enterprise.Accounting.ElectronicPayment.Common.GlobalElectronicPayment;
using Enterprise.Environment;
using WTG.TestHelpers.Xml;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Common
{
	public class GlobalElectronicPaymentSerializerTest : TestCaseWithFactory
	{
		public void TestSerialize()
		{
			var electronicPayment = CreateGlobalElectronicPayment(Base64PayLoad);
			var serializer = new GlobalElectronicPaymentSerializer();
			var serializedEPayment = serializer.Serialize(electronicPayment);
			XmlComparison.CompareAndAssertXml(ExpectedSerializedEPayment, serializedEPayment);
		}

		public void TestDeserialize()
		{
			var serializer = new GlobalElectronicPaymentSerializer();
			var electronicPayment = serializer.Deserialize(ExpectedSerializedEPayment);
			AssertEquals("OFX", electronicPayment.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("GRT", electronicPayment.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CAU", electronicPayment.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BAU", electronicPayment.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals(false, electronicPayment.Header.ElectronicPaymentRequest.IsProductionSystem);
			AssertMultilineASCIIEquals(Base64PayLoad, electronicPayment.Payload);
		}

		GlobalElectronicPayment CreateGlobalElectronicPayment(string payLoad)
		=> new GlobalElectronicPayment
		{
			Header = new GlobalElectronicPaymentHeader()
			{
				ElectronicPaymentRequest = new GlobalElectronicPaymentHeaderElectronicPaymentRequest()
				{
					MessagingSystem = "OFX",
					MessageType = "GRT",
					CompanyCode = "CAU",
					BranchCode = "BAU",
					IsProductionSystem = Env.Instance.IsProductionSystem,
				}
			},
			Payload = payLoad
		};

		string ExpectedSerializedEPayment => FormattableString.Invariant($@"<?xml version=""1.0"" encoding=""utf-8""?>
<GlobalElectronicPayment xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicPayment"">
  <Header>
    <ElectronicPaymentRequest>
      <MessagingSystem>OFX</MessagingSystem>
      <MessageType>GRT</MessageType>
      <CompanyCode>CAU</CompanyCode>
      <BranchCode>BAU</BranchCode>
      <IsProductionSystem>false</IsProductionSystem>
    </ElectronicPaymentRequest>
  </Header>
  <Payload>{Base64PayLoad}</Payload>
</GlobalElectronicPayment>");
		string Base64PayLoad => "PFVuaXZlcnNhbFRyYW5zYWN0aW9uIHhtbG5zPSJodHRwOi8vd3d3LmNhcmdvd2lzZS5jb20vU2NoZW1hcy9Vbml2ZXJzYWwvMjAxMS8xMSIgdmVyc2lvbj0iMS4xIj4NCiAgPFRyYW5zYWN0aW9uSW5mbz4NCiAgICA8RGF0YUNvbnRleHQ+DQogICAgICA8RGF0YVNvdXJjZUNvbGxlY3Rpb24+DQogICAgICAgIDxEYXRhU291cmNlPg0KICAgICAgICAgIDxUeXBlPkFjY0VQYXltZW50UXVvdGU8L1R5cGU+DQogICAgICAgICAgPEtleT4wMDAwMTAwMDwvS2V5Pg0KICAgICAgICA8L0RhdGFTb3VyY2U+DQogICAgICA8L0RhdGFTb3VyY2VDb2xsZWN0aW9uPg0KICAgICAgPENvbXBhbnk+DQogICAgICAgIDxDb2RlPkNBVTwvQ29kZT4NCiAgICAgICAgPENvdW50cnk+DQogICAgICAgICAgPENvZGU+QVU8L0NvZGU+DQogICAgICAgICAgPE5hbWU+QXVzdHJhbGlhPC9OYW1lPg0KICAgICAgICA8L0NvdW50cnk+DQogICAgICAgIDxOYW1lPjwvTmFtZT4NCiAgICAgIDwvQ29tcGFueT4NCiAgICAgIDxEYXRhUHJvdmlkZXI+RURJREFUQ0FVPC9EYXRhUHJvdmlkZXI+DQogICAgICA8RW50ZXJwcmlzZUlEPkVESTwvRW50ZXJwcmlzZUlEPg0KICAgICAgPFNlcnZlcklEPkRBVDwvU2VydmVySUQ+DQogICAgPC9EYXRhQ29udGV4dD4NCiAgICA8QmFua0FjY291bnQ+WkhTQkNVU0Q8L0JhbmtBY2NvdW50Pg0KICAgIDxCcmFuY2g+DQogICAgICA8Q29kZT5CQVU8L0NvZGU+DQogICAgICA8TmFtZT48L05hbWU+DQogICAgPC9CcmFuY2g+DQogICAgPENoZWNrTnVtYmVyT3JQYXltZW50UmVmPjAwMDA5MjgzPC9DaGVja051bWJlck9yUGF5bWVudFJlZj4NCiAgICA8RGVzY3JpcHRpb24+UGF5aW5nIEZyZWlnaHRRdW90YSBJbnZvaWNlIDgzOTQyPC9EZXNjcmlwdGlvbj4NCiAgICA8TGVkZ2VyPkFQPC9MZWRnZXI+DQogICAgPExvY2FsQ3VycmVuY3k+DQogICAgICA8Q29kZT5VU0Q8L0NvZGU+DQogICAgICA8RGVzY3JpcHRpb24+VW5pdGVkIFN0YXRlcyBEb2xsYXI8L0Rlc2NyaXB0aW9uPg0KICAgIDwvTG9jYWxDdXJyZW5jeT4NCiAgICA8TG9jYWxUb3RhbD4wPC9Mb2NhbFRvdGFsPg0KICAgIDxPU0N1cnJlbmN5Pg0KICAgICAgPENvZGU+VVNEPC9Db2RlPg0KICAgICAgPERlc2NyaXB0aW9uPlVuaXRlZCBTdGF0ZXMgRG9sbGFyPC9EZXNjcmlwdGlvbj4NCiAgICA8L09TQ3VycmVuY3k+DQogICAgPE9TVG90YWw+MTAwMDwvT1NUb3RhbD4NCiAgICA8UGF5bWVudE9yUmVjZWlwdFR5cGU+Q0hRPC9QYXltZW50T3JSZWNlaXB0VHlwZT4NCiAgICA8UG9zdERhdGU+MjAyMS0wNC0wOFQwMDowMDowMDwvUG9zdERhdGU+DQogICAgPFRyYW5zYWN0aW9uRGF0ZT4yMDIxLTA0LTEwVDAwOjAwOjAwPC9UcmFuc2FjdGlvbkRhdGU+DQogICAgPFRyYW5zYWN0aW9uUmVmZXJlbmNlPjAwMDAxMDAwPC9UcmFuc2FjdGlvblJlZmVyZW5jZT4NCiAgICA8VHJhbnNhY3Rpb25UeXBlPlBBWTwvVHJhbnNhY3Rpb25UeXBlPg0KICA8L1RyYW5zYWN0aW9uSW5mbz4NCjwvVW5pdmVyc2FsVHJhbnNhY3Rpb24+";
	}
}
