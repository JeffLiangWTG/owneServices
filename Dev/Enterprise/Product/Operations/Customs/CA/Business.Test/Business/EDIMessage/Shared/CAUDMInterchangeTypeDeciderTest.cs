using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAUDMInterchangeTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				BatchProcessorUtilities.ResetValidACIBranchesForTesting();
				CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
				CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
				CACustomsDataRegistry.Instance.TransmissionSite.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");
				CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "12345");
				CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PASSWORD");
				LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);

				EDIMessage CreateMessage(string messageType, string applicationCode)
				{
					var message = Factory.New<EDIMessage>();
					message.EM_MessageType = messageType;
					message.EM_ApplicationCode = applicationCode;
					message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
					message.EM_MessageText = "Test Message";
					message.EM_MessageData = new ZBlob(new byte[] { 0x00, 0x01, 0xFF, 0x01 });
					message.EM_HeldUntilDate = ZDateTime.Now.AddDays(-1);

					return message;
				}

				var message1 = CreateMessage(MessageTypeList.Codes.IntegratedImportDeclaration, EDIMessage.ApplicationCodes.CAIMP);
				var message2 = CreateMessage(MessageTypeList.Codes.IntegratedImportDeclaration, EDIMessage.ApplicationCodes.CAIMP);
				var message3 = CreateMessage(MessageTypeList.Codes.EDIRelease, EDIMessage.ApplicationCodes.CAIMP);
				var message4 = CreateMessage(MessageTypeList.Codes.IntegratedImportDeclaration, EDIMessage.ApplicationCodes.CAIMP);

				Factory.Save();

				var sender = new Sender();
				sender.ExecuteBatchForDebug();

				Factory.Save();

				var typeDecider = new CAUDMInterchangeTypeDecider();

				message1.Reload();
				message2.Reload();
				message3.Reload();
				message4.Reload();

				var row1 = ((IBusinessObjectInternals)message1.Interchange).Row;
				row1[EDIInterchangeSchema.Constants.EI_To] = "CACustomsTest";
				AssertEquals("Should is CAUniversalXMLInterchange.", typeof(CAUniversalXMLInterchange), typeDecider.GetTypeForLoad(row1, NewFactory()));

				var row2 = ((IBusinessObjectInternals)message2.Interchange).Row;
				row2[EDIInterchangeSchema.Constants.EI_To] = "CACustoms";
				AssertEquals("Should is CAUniversalXMLInterchange.", typeof(CAUniversalXMLInterchange), typeDecider.GetTypeForLoad(row2, NewFactory()));

				var row3 = ((IBusinessObjectInternals)message3.Interchange).Row;
				row3[EDIInterchangeSchema.Constants.EI_To] = "CACustoms";
				AssertEquals("Should is null", null, typeDecider.GetTypeForLoad(row3, NewFactory()));

				var row4 = ((IBusinessObjectInternals)message4.Interchange).Row;
				row4[EDIInterchangeSchema.Constants.EI_To] = "TestID";
				AssertEquals("Should is null", null, typeDecider.GetTypeForLoad(row4, NewFactory()));
			}
		}
	}
}
