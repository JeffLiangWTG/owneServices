using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class NctsMessageHelperTest : TestCaseWithFactory
	{
		public void TestLocateLocateLinkedObjectByEdiInterchange()
		{
			Factory.Save();
			var sessionGUID = ZGuid.BrettsGuid;
			var ediMessageBadStatus = Factory.New<NCTSMessage>();
			ediMessageBadStatus.EM_Status = "FAL";
			var ediMessageOld = Factory.New<NCTSMessage>();
			ediMessageOld.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			var ediMessage = Factory.New<NCTSMessage>();
			ediMessage.EM_Status = "SNT";
			ediMessage.EM_LinkTable = nctsHeader.TableName;
			ediMessage.EM_LinkUniqueID = nctsHeader.PK;
			var interchangeOutgoing = Factory.New<BECInterchange>();
			interchangeOutgoing.EI_SessionGUID = sessionGUID;
			interchangeOutgoing.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchangeOutgoing.EI_Status = LogicalStatusList.Codes.Sent;
			ediMessage.EM_EI = interchangeOutgoing.PK;
			var interchangeIncoming = Factory.New<BECInterchange>();
			interchangeIncoming.EI_SessionGUID = sessionGUID;
			interchangeIncoming.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			AssertEquals(nctsHeader.PK, NctsMessageHelper.LocateLinkedObjectByEdiInterchange(interchangeIncoming).PK);
		}

		public void TestLocateHeaderByLRNOrMRN()
		{
			entry.BM_PaperlessInbondNum = "22045281480600000001";
			Extensions.CreateMovementReferenceNumber(entry.Header, "22BE000000000012J1");
			Factory.Save();

			CombineAssertions(() =>
			{
				mockProvider.Setup(m => m.LRN).Returns(string.Empty);
				mockProvider.Setup(m => m.MRN).Returns(string.Empty);
				AssertNull("Empty", NctsMessageHelper.LocateHeaderByLRNOrMRN(new BusinessObjectFactory(), mockProvider.Object));

				mockProvider.Setup(m => m.LRN).Returns(string.Empty);
				mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");
				AssertEquals("By MRN", entry.Header.PK, NctsMessageHelper.LocateHeaderByLRNOrMRN(new BusinessObjectFactory(), mockProvider.Object).PK);

				mockProvider.Setup(m => m.LRN).Returns("123");
				mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");
				AssertEquals("By MRN", entry.Header.PK, NctsMessageHelper.LocateHeaderByLRNOrMRN(new BusinessObjectFactory(), mockProvider.Object).PK);

				mockProvider.Setup(m => m.MRN).Returns(string.Empty);
				mockProvider.Setup(m => m.LRN).Returns("22045281480600000001");
				AssertEquals("By LRN", entry.Header.PK, NctsMessageHelper.LocateHeaderByLRNOrMRN(new BusinessObjectFactory(), mockProvider.Object).PK);

				mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");
				mockProvider.Setup(m => m.LRN).Returns("22045281480600000001");
				AssertEquals("By LRN", entry.Header.PK, NctsMessageHelper.LocateHeaderByLRNOrMRN(new BusinessObjectFactory(), mockProvider.Object).PK);
			});
		}

		public void TestLocateHeaderByLRNOrMRNFallbackInterchange()
		{
			entry.BM_PaperlessInbondNum = "22045281480600000001";
			Extensions.CreateMovementReferenceNumber(entry.Header, "22BE000000000012J1");
			Factory.Save();

			var sessionGUID = ZGuid.BrettsGuid;
			var ediMessageBadStatus = Factory.New<NCTSMessage>();
			ediMessageBadStatus.EM_Status = "FAL";
			var ediMessageOld = Factory.New<NCTSMessage>();
			ediMessageOld.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			var ediMessage = Factory.New<NCTSMessage>();
			ediMessage.EM_Status = "SNT";
			ediMessage.EM_LinkTable = nctsHeader.TableName;
			ediMessage.EM_LinkUniqueID = nctsHeader.PK;
			var interchangeOutgoing = Factory.New<BECInterchange>();
			interchangeOutgoing.EI_SessionGUID = sessionGUID;
			interchangeOutgoing.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchangeOutgoing.EI_Status = LogicalStatusList.Codes.Sent;
			ediMessage.EM_EI = interchangeOutgoing.PK;
			var interchangeIncoming = Factory.New<BECInterchange>();
			interchangeIncoming.EI_SessionGUID = sessionGUID;
			interchangeIncoming.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			CombineAssertions(() =>
			{
				mockProvider.Setup(m => m.LRN).Returns(string.Empty);
				mockProvider.Setup(m => m.MRN).Returns(string.Empty);
				AssertEquals("By Interchange", entry.Header.PK, NctsMessageHelper.LocateHeaderByLRNOrMRNFallbackInterchange(new BusinessObjectFactory(), mockProvider.Object, interchangeIncoming).PK);

				interchangeIncoming.EI_SessionGUID = new ZGuid();
				mockProvider.Setup(m => m.LRN).Returns(string.Empty);
				mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");
				AssertEquals("By MRN", entry.Header.PK, NctsMessageHelper.LocateHeaderByLRNOrMRNFallbackInterchange(new BusinessObjectFactory(), mockProvider.Object, interchangeIncoming).PK);

				mockProvider.Setup(m => m.LRN).Returns("123");
				mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");
				AssertEquals("By MRN", entry.Header.PK, NctsMessageHelper.LocateHeaderByLRNOrMRNFallbackInterchange(new BusinessObjectFactory(), mockProvider.Object, interchangeIncoming).PK);

				mockProvider.Setup(m => m.MRN).Returns(string.Empty);
				mockProvider.Setup(m => m.LRN).Returns("22045281480600000001");
				AssertEquals("By LRN", entry.Header.PK, NctsMessageHelper.LocateHeaderByLRNOrMRNFallbackInterchange(new BusinessObjectFactory(), mockProvider.Object, interchangeIncoming).PK);

				mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");
				mockProvider.Setup(m => m.LRN).Returns("22045281480600000001");
				AssertEquals("By LRN", entry.Header.PK, NctsMessageHelper.LocateHeaderByLRNOrMRNFallbackInterchange(new BusinessObjectFactory(), mockProvider.Object, interchangeIncoming).PK);
			});
		}

		public void TestLocateHeaderByMRN()
		{
			Extensions.CreateMovementReferenceNumber(entry.Header, "22BE000000000012J1");
			Factory.Save();

			CombineAssertions(() =>
			{
				mockProvider.Setup(m => m.MRN).Returns(string.Empty);
				AssertNull("Empty", NctsMessageHelper.LocateHeaderByMRN(new BusinessObjectFactory(), mockProvider.Object));
				mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");
				AssertNull("By MRN", NctsMessageHelper.LocateHeaderByMRN(new BusinessObjectFactory(), mockProvider.Object, null, null, NctsMovementType.Codes.Departure));
				mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");
				AssertEquals("By MRN", entry.Header.PK, NctsMessageHelper.LocateHeaderByMRN(new BusinessObjectFactory(), mockProvider.Object, null, null, NctsMovementType.Codes.Arrival).PK);
			});
		}

		public void TestLocateHeaderByMRN_WrongCountry()
		{
			var headerForOtherCountry = Factory.New<NctsHeader>();
			var mrnEntryNumberForOtherCountry = CusEntryNumber.LoadOrCreate(headerForOtherCountry, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
			Factory.Save();

			mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");
			AssertNull("By MRN - Wrong Country", NctsMessageHelper.LocateHeaderByMRN(new BusinessObjectFactory(), mockProvider.Object, null, null, NctsMovementType.Codes.Arrival));
		}

		public void TestLocateHeaderByMRNUsingMovementHeader()
		{
			Extensions.CreateMovementReferenceNumber(entry.Header.ArrivalMovementHeader, "22BE000000000012J1");
			Factory.Save();

			CombineAssertions(() =>
			{
				mockProvider.Setup(m => m.MRN).Returns(string.Empty);
				AssertNull("Empty", NctsMessageHelper.LocateHeaderByMRN(new BusinessObjectFactory(), mockProvider.Object));
				mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");
				AssertEquals("By MRN", entry.Header.PK, NctsMessageHelper.LocateHeaderByMRN(new BusinessObjectFactory(), mockProvider.Object, null, null, NctsMovementType.Codes.Arrival).PK);
			});
		}

		public void TestLocateHeaderByMRNWithSubApplicationCode()
		{
			Extensions.CreateMovementReferenceNumber(entry.Header, "22BE000000000012J1");
			entry.BM_SubApplicationCode = NctsTypeOfAdditionalDeclarationList.Codes.A;
			Factory.Save();
			mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");

			CombineAssertions(() =>
			{
				AssertNull("Invalid SubApplicationCode", NctsMessageHelper.LocateHeaderByMRN(new BusinessObjectFactory(), mockProvider.Object, NctsTypeOfAdditionalDeclarationList.Codes.D));
				AssertEquals("Valid SubApplicationCode", entry.Header.PK, NctsMessageHelper.LocateHeaderByMRN(new BusinessObjectFactory(), mockProvider.Object, NctsTypeOfAdditionalDeclarationList.Codes.A).PK);
			});
		}

		public void TestLocateHeaderByMRNWithMessageStatusArray()
		{
			string[] messageStatussesArray = new string[2] { LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Acknowledged };
			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.SetMovementType(NctsMovementType.Codes.Arrival);
			var entry2 = nctsHeader2.ArrivalMovementHeader;
			nctsHeader2.EffectiveMessageStatus = LogicalStatusList.Codes.Invalid;
			entry.Header.EffectiveMessageStatus = LogicalStatusList.Codes.Invalid;
			Factory.Save();

			Extensions.CreateMovementReferenceNumber(entry.Header, "22BE000000000012J1");
			Extensions.CreateMovementReferenceNumber(entry2.Header, "22BE000000000012J1");
			mockProvider.Setup(m => m.MRN).Returns("22BE000000000012J1");

			CombineAssertions(() =>
			{
				AssertNull("Invalid Message Status", NctsMessageHelper.LocateHeaderByMRN(new BusinessObjectFactory(), mockProvider.Object, messageStatusArray: messageStatussesArray));
				entry.Header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
				Factory.Save();
				AssertEquals("Valid Message Status entry1", entry.Header.PK, NctsMessageHelper.LocateHeaderByMRN(new BusinessObjectFactory(), mockProvider.Object, messageStatusArray: messageStatussesArray).PK);
				entry.Header.EffectiveMessageStatus = LogicalStatusList.Codes.Invalid;
				nctsHeader2.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
				Factory.Save();
				AssertEquals("Valid Message Status entry2", nctsHeader2.PK, NctsMessageHelper.LocateHeaderByMRN(new BusinessObjectFactory(), mockProvider.Object, messageStatusArray: messageStatussesArray).PK);
			});
		}

		public void TestLocateHeaderByLRN()
		{
			entry.BM_PaperlessInbondNum = "22045281480600000001";
			Factory.Save();

			CombineAssertions(() =>
			{
				mockProvider.Setup(m => m.LRN).Returns(string.Empty);
				AssertNull("Empty", NctsMessageHelper.LocateHeaderByLRN(new BusinessObjectFactory(), mockProvider.Object));
				mockProvider.Setup(m => m.LRN).Returns("22045281480600000001");
				AssertEquals("By LRN", entry.Header.PK, NctsMessageHelper.LocateHeaderByLRN(new BusinessObjectFactory(), mockProvider.Object).PK);
			});
		}

		public void TestLocateHeaderByLRNWithSubApplicationCode()
		{
			entry.BM_PaperlessInbondNum = "22045281480600000001";
			entry.BM_SubApplicationCode = NctsTypeOfAdditionalDeclarationList.Codes.A;
			Factory.Save();
			mockProvider.Setup(m => m.LRN).Returns("22045281480600000001");

			CombineAssertions(() =>
			{
				AssertNull("Invalid SubApplicationCode", NctsMessageHelper.LocateHeaderByLRN(new BusinessObjectFactory(), mockProvider.Object, NctsTypeOfAdditionalDeclarationList.Codes.D));
				AssertEquals("Valid SubApplicationCode", entry.Header.PK, NctsMessageHelper.LocateHeaderByLRN(new BusinessObjectFactory(), mockProvider.Object, NctsTypeOfAdditionalDeclarationList.Codes.A).PK);
			});
		}

		public void TestRetrievePackageBySequencesForArrivalDeclaration()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = nctsHeader.Bills.AddNew();
			bill.MovementDetail.B9_SeqNo = "1";
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_DeclarationGoodsItemNumber = 1;
			var package = goodsItem.Packages.AddNew();
			package.B5_SequenceNumber = 1;
			CombineAssertions(() =>
			{
				AssertEquals("Package should be found", package, NctsMessageHelper.RetrievePackageBySequencesForArrivalDeclaration(nctsHeader, 1, 1, 1));
				AssertEquals("Wrong house consignment sequence", null, NctsMessageHelper.RetrievePackageBySequencesForArrivalDeclaration(nctsHeader, 2, 1, 1));
				AssertEquals("Wrong consignment item sequence", null, NctsMessageHelper.RetrievePackageBySequencesForArrivalDeclaration(nctsHeader, 1, 2, 1));
				AssertEquals("Wrong package sequence", null, NctsMessageHelper.RetrievePackageBySequencesForArrivalDeclaration(nctsHeader, 1, 1, 2));
			});
		}

		public void TestRequestTADFromCustoms()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			NctsMessageHelper.RequestTADFromCustoms(nctsHeader);

			var query = new ZQuery();
			query.AddToFilter(new ZQuery(EDIMessageSchema.EM_MessageType, "NCT"));
			query.AddToFilter(new ZQuery(EDIMessageSchema.EM_MessageSubType, "TAD"));

			AssertNotNull(Factory.LoadTop1<EDIMessage>(query));
		}

		public void TestGetUtcDateTimeToLocalBEBranchString()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Non Utc", ZString.Empty, NctsMessageHelper.GetUtcDateTimeToLocalBEBranchString(new DateTime(2022, 6, 1, 12, 34, 56, DateTimeKind.Unspecified)));
				AssertEquals("Valid", "01-06-2022 14:34:56 UTC+2", NctsMessageHelper.GetUtcDateTimeToLocalBEBranchString(new DateTime(2022, 6, 1, 12, 34, 56, DateTimeKind.Utc)));
				AssertEquals("Invalid", ZString.Empty, NctsMessageHelper.GetUtcDateTimeToLocalBEBranchString(new DateTime(0001, 01, 01, 00, 00, 00, DateTimeKind.Utc)));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider = new Mock<IInboundProvider>();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			entry = nctsHeader.ArrivalMovementHeader;
		}

		Mock<IInboundProvider> mockProvider;
		NctsHeader nctsHeader;
		NctsArrivalMovementHeader entry;
	}
}
