using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(TRQQUEProvider))]
	class TRQQUEProviderTest : StatusRequestHeaderProviderAbstractTest<TRQQUEProvider>
	{
		public override void TestInterchangeRecipientID()
		{
			var currentValue = DECustomsDataRegistry.Instance.ExportStatusRequestRecipient.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			currentValue.Cast<ExportStatusRequestRecipientRegistry>().Single(x => x.SystemCode == ExportStatusRequestRecipientRegistry.AtlasSystemCode).MessageRecipient = "DE001234";
			using (DECustomsDataRegistry.Instance.ExportStatusRequestRecipient.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, currentValue))
			{
				AssertEquals("DE001234", provider.InterchangeRecipientID);
			}
		}

		public override void TestPartyType()
		{
			CombineAssertions(() =>
			{
				statusRequest.Role = ExportStatusRequestNCTSRoleList.Codes.Consignor;
				AssertEquals("Consignor", PartyType.NCTSConsignor, GetProvider().PartyType);
				statusRequest.Role = ExportStatusRequestNCTSRoleList.Codes.Consignee;
				AssertEquals("Consignee", PartyType.NCTSConsignee, GetProvider().PartyType);
				statusRequest.Role = ExportStatusRequestNCTSRoleList.Codes.Principal;
				AssertEquals("Principal", PartyType.NCTSProcedureOwner, GetProvider().PartyType);
				statusRequest.Role = ExportStatusRequestNCTSRoleList.Codes.AuthorizedConsignee;
				AssertEquals("AuthorizedConsignee", PartyType.NCTSAuthorisedConsignee, GetProvider().PartyType);
				statusRequest.Role = ExportStatusRequestNCTSRoleList.Codes.Representative;
				AssertEquals("Representative", PartyType.NCTSRepresentative, GetProvider().PartyType);
				statusRequest.Role = ZString.Empty;
				AssertEquals("Other", PartyType.Unknown, GetProvider().PartyType);
			});
		}

		protected override IStatusRequestHeader GetProvider() => new TRQQUEProvider(statusRequest);
	}
}
