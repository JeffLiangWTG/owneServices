using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXQQUEProvider))]
	class EXQQUEProviderTest : StatusRequestHeaderProviderAbstractTest<EXQQUEProvider>
	{
		public override void TestInterchangeRecipientID()
		{
			var currentValue = DECustomsDataRegistry.Instance.ExportStatusRequestRecipient.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			currentValue.Cast<ExportStatusRequestRecipientRegistry>().Single(x => x.SystemCode == ExportStatusRequestRecipientRegistry.AESSystemCode).MessageRecipient = "DE001234";
			using (DECustomsDataRegistry.Instance.ExportStatusRequestRecipient.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, currentValue))
			{
				AssertEquals("DE001234", provider.InterchangeRecipientID);
			}
		}

		public override void TestPartyType()
		{
			CombineAssertions(() =>
			{
				statusRequest.Role = ExportStatusRequestAESRoleList.Codes.Declarant;
				AssertEquals("Declarant", PartyType.AESDeclarant, GetProvider().PartyType);
				statusRequest.Role = ExportStatusRequestAESRoleList.Codes.Exporter;
				AssertEquals("Exporter", PartyType.AESExporter, GetProvider().PartyType);
				statusRequest.Role = ExportStatusRequestAESRoleList.Codes.Representative;
				AssertEquals("Representative", PartyType.AESRepresentative, GetProvider().PartyType);
				statusRequest.Role = ExportStatusRequestAESRoleList.Codes.Subcontractor;
				AssertEquals("Subcontractor", PartyType.AESContractor, GetProvider().PartyType);
				statusRequest.Role = ZString.Empty;
				AssertEquals("Other", PartyType.Unknown, GetProvider().PartyType);
			});
		}

		protected override IStatusRequestHeader GetProvider() => new EXQQUEProvider(statusRequest);
	}
}
