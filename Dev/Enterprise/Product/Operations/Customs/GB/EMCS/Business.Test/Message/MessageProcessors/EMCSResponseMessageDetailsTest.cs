using System;
using Enterprise.Customs.GB.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class EMCSResponseMessageDetailsTest : TestCase
	{
		public void TestResponseDetails()
		{
			AssertEquals(16, responseMessageDetails.ResponseMessages.Count);
			// Update to reflect new number of possible responses
		}

		public void Test704()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE704), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie704uk.Ie704Type), typeof(IE704MessageProcessor));
		}

		public void Test801()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE801), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801.Ie801Type), typeof(IE801MessageProcessor));
		}

		public void Test802()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE802), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie802.Ie802Type), typeof(IE802MessageProcessor));
		}

		public void Test803()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE803), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie803.Ie803Type), typeof(IE803MessageProcessor));
		}

		public void Test807()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE807), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie807.Ie807Type), typeof(IE807MessageProcessor));
		}

		public void Test810()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE810), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie810.Ie810Type), typeof(IE810MessageProcessor));
		}

		public void Test813()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE813), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813.Ie813Type), typeof(IE813MessageProcessor));
		}

		public void Test818()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE818), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie818.Ie818Type), typeof(IE818MessageProcessor));
		}

		public void Test819()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE819), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie819.Ie819Type), typeof(IE819MessageProcessor));
		}

		public void Test829()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE829), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie829.Ie829Type), typeof(IE829MessageProcessor));
		}

		public void Test837()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE837), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie837.Ie837Type), typeof(IE837MessageProcessor));
		}

		public void Test839()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE839), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie839.Ie839Type), typeof(IE839MessageProcessor));
		}

		public void Test840()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE840), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie840.Ie840Type), typeof(IE840MessageProcessor));
		}

		public void Test871()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE871), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie871.Ie871Type), typeof(IE871MessageProcessor));
		}

		public void Test881()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE881), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie881.Ie881Type), typeof(IE881MessageProcessor));
		}

		public void Test905()
		{
			AssertResponseDetail(responseMessageDetails.GetResponseMessage(EMCSGBIncomingMessageTypeList.Codes.IE905), typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie905.Ie905Type), typeof(IE905MessageProcessor));
		}

		public void TestGetResponseDetail_InvalidMessageType()
		{
			AssertNull(responseMessageDetails.GetResponseMessage("!@#"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			responseMessageDetails = EMCSResponseMessageDetails.Instance;
		}

		void AssertResponseDetail(EMCSResponseDetail? detail, Type xmlObjectType, Type processorType)
		{
			AssertNotNull(detail);
			var detailValue = detail.Value;
			CombineAssertions(() =>
			{
				AssertEquals(nameof(detailValue.XmlObjectType), xmlObjectType, detailValue.XmlObjectType);
				AssertEquals(nameof(detailValue.ProcessorType), processorType, detailValue.ProcessorType);
			});
		}

		EMCSResponseMessageDetails responseMessageDetails;
	}
}
