using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	[TestedType(typeof(TP5MessageSendingObject))]
	public class TP5MessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => new TP5MessageSendingObject(null));
		}

		public void TestMessageOwner()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			objectToSend = new TP5MessageSendingObject(nctsHeader);
			objectToSend.MessageType = TP5MessageTypeList.Codes.CC015C;
			AssertEquals("MessagesOwner is equal to NctsHeader when NCTS is not departure", nctsHeader, objectToSend.MessagesOwner);

			nctsHeader = Factory.New<FR.Business.NCTS.NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			objectToSend = new TP5MessageSendingObject(nctsHeader);
			objectToSend.MessageType = TP5MessageTypeList.Codes.CC015C;
			AssertEquals("MessagesOwner is equal to departure movement of NctsHeader when NCTS is departure", nctsHeader.MovementHeader, objectToSend.MessagesOwner);

			nctsHeader = Factory.New<FR.Business.NCTS.NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
			objectToSend = new TP5MessageSendingObject(nctsHeader);
			objectToSend.MessageType = TP5MessageTypeList.Codes.CC015C;
			AssertEquals("MessagesOwner is equal to departure movement of NctsHeader when NCTS is departure", nctsHeader.MovementHeader, objectToSend.MessagesOwner);
		}

		public void TestMessageType()
		{
			AssertEquals(TP5MessageTypeList.Codes.CC015C, objectToSend.MessageType);
		}

		public void TestDataSource()
		{
			AssertEquals(nctsHeader, objectToSend.DataSource);
		}

		public void TestJustificationReadOnly()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			objectToSend = new TP5MessageSendingObject(nctsHeader);

			objectToSend.MessageType = TP5MessageTypeList.Codes.CC013C;
			AssertEquals(false, objectToSend.JustificationInfo.ReadOnly);

			objectToSend.MessageType = TP5MessageTypeList.Codes.CC014C;
			AssertEquals(false, objectToSend.JustificationInfo.ReadOnly);

			objectToSend.MessageType = TP5MessageTypeList.Codes.CC015C;
			AssertEquals(true, objectToSend.JustificationInfo.ReadOnly);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			objectToSend = new TP5MessageSendingObject(nctsHeader);

			objectToSend.MessageType = TP5MessageTypeList.Codes.CC013C;
			AssertEquals(false, objectToSend.JustificationInfo.ReadOnly);

			objectToSend.MessageType = TP5MessageTypeList.Codes.CC014C;
			AssertEquals(false, objectToSend.JustificationInfo.ReadOnly);

			objectToSend.MessageType = TP5MessageTypeList.Codes.CC015C;
			AssertEquals(true, objectToSend.JustificationInfo.ReadOnly);
		}

		public void TestJustificationCodeReadOnly()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			objectToSend = new TP5MessageSendingObject(nctsHeader);

			objectToSend.MessageType = TP5MessageTypeList.Codes.CC013C;
			AssertEquals(true, objectToSend.JustificationCodeInfo.ReadOnly);

			objectToSend.MessageType = TP5MessageTypeList.Codes.CC014C;
			AssertEquals(false, objectToSend.JustificationCodeInfo.ReadOnly);

			objectToSend.MessageType = TP5MessageTypeList.Codes.CC015C;
			AssertEquals(true, objectToSend.JustificationCodeInfo.ReadOnly);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			objectToSend = new TP5MessageSendingObject(nctsHeader);

			objectToSend.MessageType = TP5MessageTypeList.Codes.CC013C;
			AssertEquals(false, objectToSend.JustificationCodeInfo.ReadOnly);

			objectToSend.MessageType = TP5MessageTypeList.Codes.CC014C;
			AssertEquals(false, objectToSend.JustificationCodeInfo.ReadOnly);

			objectToSend.MessageType = TP5MessageTypeList.Codes.CC015C;
			AssertEquals(false, objectToSend.JustificationCodeInfo.ReadOnly);
		}

		public void TestMessageSendingActionLookups()
		{
			AssertType<TP5MessageSendingObjectLookups>(objectToSend.Lookups);
		}

		public void TestQueryIdentifierDefaultValue()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();

			guarantee.PW_BondType = "2";
			objectToSend = new TP5MessageSendingObject(nctsHeader);
			AssertEquals("Default value of QueryIdentifier should be PW_BondType when PW_BondType is 2 or 4", "2", objectToSend.QueryIdentifier);

			guarantee.PW_BondType = "4";
			objectToSend = new TP5MessageSendingObject(nctsHeader);
			AssertEquals("Default value of QueryIdentifier should be PW_BondType when PW_BondType is 2 or 4", "4", objectToSend.QueryIdentifier);

			guarantee.PW_BondType = "3";
			objectToSend = new TP5MessageSendingObject(nctsHeader);
			AssertEquals("Default value of QueryIdentifier should be empty when PW_BondType is not 2 or 4", ZString.Empty, objectToSend.QueryIdentifier);
		}

		public void TestPeriodReadOnly()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			objectToSend = new TP5MessageSendingObject(nctsHeader);

			string[] periodEditableBondTypeList = { "0", "1", "9" };
			string[] periodEditableQueryIdentifierList = { "1", "3" };

			CombineAssertions("Period should be editable only when PW_BondType is 0,1,9 and QueryIdentifier is 1,3", () =>
			{
				foreach (var bondType in periodEditableBondTypeList)
				{
					foreach (var queryIdentifier in periodEditableQueryIdentifierList)
					{
						guarantee.PW_BondType = bondType;
						objectToSend.QueryIdentifier = queryIdentifier;
						AssertEquals("PeriodFrom should be editable when PW_BondType is 0,1,9 and QueryIdentifier is 1,3", false, objectToSend.PeriodFromInfo.ReadOnly);
						AssertEquals("PeriodTo should be editable when PW_BondType is 0,1,9 and QueryIdentifier is 1,3", false, objectToSend.PeriodToInfo.ReadOnly);
					}
				}

				guarantee.PW_BondType = "2";
				AssertEquals("PeriodFrom should be readonly when PW_BondType is not 0,1,9", true, objectToSend.PeriodFromInfo.ReadOnly);
				AssertEquals("PeriodTo should be readonly when PW_BondType is not 0,1,9", true, objectToSend.PeriodToInfo.ReadOnly);

				guarantee.PW_BondType = "0";
				objectToSend.QueryIdentifier = "2";
				AssertEquals("PeriodFrom should be readonly when QueryIdentifier is not 1,3", true, objectToSend.PeriodFromInfo.ReadOnly);
				AssertEquals("PeriodTo should be readonly when QueryIdentifier is not 1,3", true, objectToSend.PeriodToInfo.ReadOnly);
			});
		}

		public void TestPeriod_WhenQueryIdentifierChange()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			objectToSend = new TP5MessageSendingObject(nctsHeader);

			guarantee.PW_BondType = "0";
			objectToSend.QueryIdentifier = "1";
			objectToSend.PeriodFrom = new ZDateTime(2025, 1, 6);
			objectToSend.PeriodTo = new ZDateTime(2025, 1, 7);

			objectToSend.QueryIdentifier = "2";
			AssertEquals("PeriodFrom should be cleared when the change in QueryIdentifier causes the Period to become ReadOnly.", true, objectToSend.PeriodFrom.IsEmpty);
			AssertEquals("PeriodTo should be cleared when the change in QueryIdentifier causes the Period to become ReadOnly.", true, objectToSend.PeriodTo.IsEmpty);

			guarantee.PW_BondType = "0";
			objectToSend.QueryIdentifier = "1";
			objectToSend.PeriodFrom = new ZDateTime(2025, 1, 6);
			objectToSend.PeriodTo = new ZDateTime(2025, 1, 7);

			objectToSend.QueryIdentifier = "3";
			AssertEquals("PeriodFrom should not be cleared when the change in QueryIdentifier does not cause the Period to become ReadOnly.", false, objectToSend.PeriodFrom.IsEmpty);
			AssertEquals("PeriodTo should not be cleared when the change in QueryIdentifier does not cause the Period to become ReadOnly.", false, objectToSend.PeriodTo.IsEmpty);
		}

		public void TestRequesterId()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			objectToSend = new TP5MessageSendingObject(nctsHeader);
			AssertEquals("RequesterId should be empty when both Representative and Principal are empty.", ZString.Empty, objectToSend.RequesterId);

			nctsHeader.Principal.OrganisationPK = GetOrganisationPKWithEORICode("0001");
			AssertEquals("RequesterId should be Principal EORI when only Principal is filled.", "FR0001", objectToSend.RequesterId);

			nctsHeader.MovementHeader.Representative.OrganisationPK = GetOrganisationPKWithEORICode("0002");
			AssertEquals("RequesterId should be Representative EORI when both Representative and Principal are filled.", "FR0002", objectToSend.RequesterId);
		}

		ZGuid GetOrganisationPKWithEORICode(string code)
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			orgCusCode.OK_CustomsRegNo = code;
			return orgHeader.PK;
		}

		public void TestRequesterRoleDefaultValue()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			objectToSend = new TP5MessageSendingObject(nctsHeader);

			AssertEquals("Default value of RequesterRole should be 1", "1", objectToSend.RequesterRole);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<FR.Business.NCTS.NctsHeader>();
			objectToSend = new TP5MessageSendingObject(nctsHeader);
			objectToSend.MessageType = TP5MessageTypeList.Codes.CC015C;
		}

		protected override BusinessObject GetNewBusinessObject() => objectToSend;

		FR.Business.NCTS.NctsHeader nctsHeader;
		TP5MessageSendingObject objectToSend;
	}
}
