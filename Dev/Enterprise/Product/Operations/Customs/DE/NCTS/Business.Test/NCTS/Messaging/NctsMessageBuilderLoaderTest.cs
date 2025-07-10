using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsMessageBuilderLoaderTest : TestCaseWithFactory
	{
		public void TestStatusRequest()
		{
			var messageVersionRegistryCollection = GetMessageVersionRegistryCollection(ATLASVersionNumberList.Codes._101);
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertType<CargoWise.Customs.DE.MessageContracts.NCTS.ATLASVersion10_1.TRQQUEMessageBuilder>(NctsMessageBuilderLoader.Instance.GetStatusRequestMessageBuilder(new Mock<IStatusRequestHeader>().Object));
			}
		}

		public void TestDepartureDeclaration()
		{
			var messageVersionRegistryCollection =  GetMessageVersionRegistryCollection(ATLASVersionNumberList.Codes._101);
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertOutboundMessageDetails<CargoWise.Customs.DE.MessageContracts.NCTS.ATLASVersion10_1.DEPDATMessageBuilder, DEPDATMessageHeaderProvider>(NctsMessageBuilderLoader.Instance.GetMessageDetails(NctsMessageTypeList.Codes.DEPDAT));
			}
		}

		public void TestArrivalDeclaration()
		{
			var messageVersionRegistryCollection =  GetMessageVersionRegistryCollection(ATLASVersionNumberList.Codes._101);
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertOutboundMessageDetails<CargoWise.Customs.DE.MessageContracts.NCTS.ATLASVersion10_1.DESNOTMessageBuilder, DESNOTMessageHeaderProvider>(NctsMessageBuilderLoader.Instance.GetMessageDetails(NctsMessageTypeList.Codes.DESNOT));
			}
		}

		public void TestUnloadingRemarks()
		{
			var messageVersionRegistryCollection =  GetMessageVersionRegistryCollection(ATLASVersionNumberList.Codes._101);
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertOutboundMessageDetails<CargoWise.Customs.DE.MessageContracts.NCTS.ATLASVersion10_1.DESREMMessageBuilder, DESREMMessageHeaderProvider>(NctsMessageBuilderLoader.Instance.GetMessageDetails(NctsMessageTypeList.Codes.DESREM));
			}
		}

		public void TestGuaranteeAccessCode()
		{
			var messageVersionRegistryCollection = GetMessageVersionRegistryCollection(ATLASVersionNumberList.Codes._101);
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertOutboundMessageDetails<CargoWise.Customs.DE.MessageContracts.NCTS.ATLASVersion10_1.GUACODMessageBuilder, GUACODMessageHeaderProvider>(NctsMessageBuilderLoader.Instance.GetMessageDetails(NctsMessageTypeList.Codes.GUACOD));
			}
		}

		public void TestZDeveloperErrorForInvalidMessageBuilderRequest()
		{
			ErrorReporter.Clear();
			CombineAssertions(() =>
			{
				foreach (var atlasVersionNumber in new ATLASVersionNumberList().GetAllCodes())
				{
					var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = atlasVersionNumber } };
					var validationErrorMessage = DECustomsDataRegistry.Instance.CustomsMessageVersion.GetValidationErrorMessage(messageVersionRegistryCollection, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					if (validationErrorMessage.Contains("Enter a valid Version."))
					{
						continue;
					}
					using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
					{
						NctsMessageBuilderLoader.Instance.GetMessageDetails("INVALID");
					}
					AssertEquals($"ATLASVersion {atlasVersionNumber}", $"Invalid DE NCTS Message Builder for code: INVALID requested for ATLAS Version {atlasVersionNumber}", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			});
		}

		static MessageVersionRegistryCollection GetMessageVersionRegistryCollection(string version)
		{
			return new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = version } };
		}

		void AssertOutboundMessageDetails<TMessageBuilder, TDataProvier>(OutboundMessageDetails outboundMessageDetails)
		{
			CombineAssertions(() =>
			{
				AssertEquals(typeof(TMessageBuilder), outboundMessageDetails.MessageBuilderType);
				AssertEquals(typeof(TDataProvier), outboundMessageDetails.ProviderType);
			});
		}
	}
}
