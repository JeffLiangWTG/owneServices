using System.IO;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;
using ATLASVersion10_1 = CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(AtlasInboundEDIMessage<IDataProvider>))]
	sealed class AtlasInboundEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", ApplicationCodes.DECustomsAtlasSystem, message.EM_ApplicationCode);
			});
		}

		public void TestValidMessageTemporaryStorage()
		{
			var applicationReference = nameof(ATLASVersion10_1.SCCANE);

			message.EM_MessageText = $"<DECustomsData><LogbookTime>2019-02-25T16:29:00.2347048+02:00</LogbookTime><CustomsData><{applicationReference} /></CustomsData></DECustomsData>";
			message.EM_ApplicationReference = applicationReference;
			CombineAssertions(() =>
			{
				var dataProvider = message.DataProvider;
				AssertEquals("Is DataProvider", true, typeof(IDataProvider).IsAssignableFrom(dataProvider.GetType()));
				AssertSame("DataProvider is cached", message.DataProvider, dataProvider);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidMessageNcts()
		{
			message.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, TestHelper.BusinessTestDirectory, @"EDIMessage\ATLAS\TestFiles\TestE_TRQ_STA_Message.txt"));
			message.EM_ApplicationReference = nameof(ATLASVersion10_1.DETQSC);
			CombineAssertions(() =>
			{
				var dataProvider = message.DataProvider;
				AssertEquals("Is DataProvider", true, typeof(IDataProvider).IsAssignableFrom(dataProvider.GetType()));
				AssertSame("DataProvider is cached", message.DataProvider, dataProvider);
			});
		}

		public void TestEM_LinkedObject_CusTempStorageRegHeader()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			message.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.ConfirmationForTemporaryStorage;
			message.EM_LinkTable = regHeader.TableName;
			message.EM_LinkUniqueID = regHeader.PK;
			AssertEquals(regHeader, message.EM_LinkedObject);
		}

		public void TestEM_LinkedObject_CusTempStorageDec()
		{
			var message = Factory.New<AtlasEDIMessage>();
			var cusTempStorageDec = CUSPRLCusTempStorageDec.New(Factory.New<CusTempStorageJobHeader>());
			message.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.ConfirmationForTemporaryStorage;
			message.EM_LinkTable = cusTempStorageDec.TableName;
			message.EM_LinkUniqueID = cusTempStorageDec.PK;
			AssertEquals(cusTempStorageDec, message.EM_LinkedObject);
		}

		public void TestEM_LinkedObject_NctsHeader()
		{
			var nctsHeader = Factory.New<Integration.Customs.DE.ICusInBondHeader>();
			message.EM_MessageSubType = NctsMessageSubTypeList.Codes.DestinationMessage;
			message.EM_LinkTable = nctsHeader.TableName;
			message.EM_LinkUniqueID = nctsHeader.PK;
			AssertEquals(nctsHeader, message.EM_LinkedObject);
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<AtlasInboundEDIMessage<IDataProvider>>();
		}
		AtlasInboundEDIMessage<IDataProvider> message;
	}
}
