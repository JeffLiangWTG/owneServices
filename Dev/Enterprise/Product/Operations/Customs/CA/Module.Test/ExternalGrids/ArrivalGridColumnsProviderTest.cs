using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using EDIMessage = Enterprise.Customs.CA.Business.EDIMessage;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class ArrivalGridColumnsProviderTest : CAShipmentGridColumnsProviderTest
	{
		[TestDate(2014, 11, 13)]
		public void TestAddColumns()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var parentConsol = Factory.NewWithValidTestData<CFSLoadListConsol>();
				var shipment1 = parentConsol.Shipments.AddNew();
				shipment1.JS_UniqueConsignRef = "S00000001";

				var shipment2 = parentConsol.Shipments.AddNew();
				shipment2.JS_UniqueConsignRef = "S00000002";
				addMessages(shipment2, CAExternalColumnsHelper.Constants.SentCode, ZDateTime.UtcNow);

				var shipment3 = parentConsol.Shipments.AddNew();
				shipment3.JS_UniqueConsignRef = "S00000003";
				addMessages(shipment3, CAExternalColumnsHelper.Constants.SentCode, ZDateTime.UtcNow.AddDays(-1));
				addMessages(shipment3, CAExternalColumnsHelper.Constants.RejectedCode, ZDateTime.UtcNow);

				var shipment4 = parentConsol.Shipments.AddNew();
				shipment4.JS_UniqueConsignRef = "S00000004";
				addMessages(shipment4, "XXX", ZDateTime.UtcNow);

				Factory.Save();

				using (var form = new MockConsolForm(parentConsol))
				{
					form.Show();

					var arrGroupName = new ResourceStringData("", "Arrival Notifications");
					AssertColumn(form.Grid, "ArrivalCertificationStatus", arrGroupName, "Arrival Certification Status", false);
					AssertColumn(form.Grid, "ArrivalCertificationDate", arrGroupName, "Arrival Certification Date", false);

					AssertColumnValue(form.Grid, "ArrivalCertificationStatus", 0, "Not Sent");
					AssertColumnValue(form.Grid, "ArrivalCertificationDate", 0, "");

					AssertColumnValue(form.Grid, "ArrivalCertificationStatus", 1, "Sent");
					AssertColumnValue(form.Grid, "ArrivalCertificationDate", 1, "13-NOV-14");

					AssertColumnValue(form.Grid, "ArrivalCertificationStatus", 2, "Rejected");
					AssertColumnValue(form.Grid, "ArrivalCertificationDate", 2, "");

					AssertColumnValue(form.Grid, "ArrivalCertificationStatus", 3, "XXX");
					AssertColumnValue(form.Grid, "ArrivalCertificationDate", 3, "");
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var parentConsol = Factory.NewWithValidTestData<CFSLoadListConsol>();

				using (var form = new MockConsolForm(parentConsol))
				{
					form.Show();

					AssertNull("Column ArrivalCertificationStatus should not be added", form.Grid.GetColumnStyle("ArrivalCertificationStatus"));
					AssertNull("Column ArrivalCertificationDate should not be added", form.Grid.GetColumnStyle("ArrivalCertificationDate"));
				}
			}
		}

		void addMessages(CFSShipment shipment, ZString status, ZDateTime createTime)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAIMP;
			message.EM_MessageType = MessageTypeList.Codes.RNSRequest;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			message.EM_Status = status;
			message.EM_SystemCreateTimeUtc = createTime;
			message.EM_LinkedObject = shipment;
		}
	}
}
