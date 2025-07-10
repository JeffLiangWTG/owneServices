using System;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(CADMessageManager))]
	sealed class CADMessageManagerTest : CAMessageManagerTestCase
	{
		public class CADMessageManagerForTest : CADMessageManager
		{
			public CADMessageManagerForTest(CADMessageWrapper wrapper)
				: base(wrapper, new TestMessageInstructionUserNotification())
			{
			}

			public bool PreCheck4CreditOKToSendChecking_Exposed(JobDeclaration declaration)
			{
				return PreCheck4CreditOKToSendChecking(declaration);
			}

			public bool CanSendThisMessage_Exposed(MessageSubTypes actionCode, out ZString messageText)
			{
				return CanSendThisMessage(actionCode, out messageText);
			}

			public IMessageBuilder GetMessageBuilder_Exposed(MessageSubTypes actionCode)
			{
				return GetMessageBuilder(actionCode);
			}

			public ZString GetAdditionalWarningsMessage_Exposed(MessageSubTypes actionCode)
			{
				return GetAdditionalWarningsMessage(actionCode);
			}

			public void SetNextAnswer(bool answer)
			{
				Notification.NextAnswer = answer;
			}

			public void PopulateMessage_Exposed(MessageSubTypes actionCode)
			{
				PopulateMessage(actionCode);
			}

			public void RunAdditionalEDIMessageModification_Exposed(Enterprise.Messaging.Business.EDIMessage message)
			{
				RunAdditionalEDIMessageModification(new Enterprise.Messaging.Business.EDIMessage[] { message });
			}

			public string LastMessage
			{
				get { return Notification.LastMessage; }
			}

			public TestMessageInstructionUserNotification Notification
			{
				get { return ((TestMessageInstructionUserNotification)notification); }
			}

			public ZString LastNotification(MessageSubTypes actionCodeToSend)
			{
				ShowQueuedForSending(actionCodeToSend);
				return ((TestMessageInstructionUserNotification)notification).LastMessage;
			}
		}

		public void TestGetAdditionalWarningsMessage()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var importer = Factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Importer;
				declaration.ImporterAddInfo.ZO_PreventWarningOnSendingB3 = false;
				declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
				var manager = new CADMessageManagerForTest((CADMessageWrapper)dataWrapper);
				AssertEquals(@"The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
Are you sure you wish to continue and send the CAD message at this time?", manager.GetAdditionalWarningsMessage_Exposed(MessageSubTypes.Create));
			}
		}

		public void TestRunAdditionalEDIMessageModification_CorrectWithOldMessageSent()
		{
			#region Testing Data
			var outgoingMessageOld = entryHeader.Messages.AddNew();
			outgoingMessageOld.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessageOld.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			outgoingMessageOld.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			outgoingMessageOld.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			outgoingMessageOld.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
  <ResponsibleAgencyName>CBSA</ResponsibleAgencyName>
  <AgencyAssignedCustomizationCode>CAD</AgencyAssignedCustomizationCode>
  <AgencyAssignedCustomizationVersionCode>001</AgencyAssignedCustomizationVersionCode>
  <FunctionalDefinition>CAD-IN</FunctionalDefinition>
  <CommunicationMetaData>
    <ApplicationReferenceID>1020700022087000001001</ApplicationReferenceID>
    <Sender>
      <ID>822066668RM0002</ID>
      <RoleCode>HN</RoleCode>
    </Sender>
    <Sender>
      <ID>864015219RM0003</ID>
      <RoleCode>AK</RoleCode>
    </Sender>
  </CommunicationMetaData>
  <Declaration>
    <FunctionCode>9</FunctionCode>
    <FunctionalReferenceID>B100000066</FunctionalReferenceID>
    <ID>10207000220870</ID>
    <LanguageCode>EN</LanguageCode>
    <TypeCode>AB</TypeCode>
    <TotalGrossMassMeasure>1</TotalGrossMassMeasure>
    <BorderTransportMeans>
      <ModeCode>01</ModeCode>
    </BorderTransportMeans>
    <Consignment>
      <Freight>
        <RateAmount>1</RateAmount>
      </Freight>
    </Consignment>
    <Declarant>
      <ID>864015219RM0003</ID>
    </Declarant>
    <ReleaseLocation>
      <ID>0497</ID>
    </ReleaseLocation>
    <Status>
      <ReleaseDateTime>
        <DateTimeString>20241201</DateTimeString>
      </ReleaseDateTime>
    </Status>
    <GoodsShipment>
      <ExitDateTime>
        <DateTimeString>20250603</DateTimeString>
      </ExitDateTime>
      <SequenceNumeric>1</SequenceNumeric>
      <ExportCountry>
        <CountryCode />
      </ExportCountry>
      <Invoice>
        <ID>123</ID>
        <TypeCode>380</TypeCode>
      </Invoice>
      <Seller>
        <Name />
        <Address />
      </Seller>
      <GovernmentAgencyGoodsItem>
        <Commodity>
          <SequenceNumeric>1</SequenceNumeric>
          <Description>Pretend old builder and doesn't change this time, should remain old - 1</Description>
          <CountQuantity>0</CountQuantity>
          <Classification>
            <ID>7210900000</ID>
            <BindingTariffReferenceID>10</BindingTariffReferenceID>
          </Classification>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""CAD"">100</ItemChargeAmount>
          </InvoiceLine>
          <DutyTaxFee>
            <TypeCode>VFT</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""CAD"">135.00</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>CUD</TypeCode>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>TOT</TypeCode>
            <DutyTaxFeeAssessmentBasis>
              <AdValoremTaxBaseAmount currencyID=""CAD"">100</AdValoremTaxBaseAmount>
            </DutyTaxFeeAssessmentBasis>
          </DutyTaxFee>
        </Commodity>
        <Commodity>
          <SequenceNumeric>2</SequenceNumeric>
          <Description>FLAT-ROLLED PRODUCTS OF IRON OR NON-ALLOY STEEL, OF A WIDTH OF 600 MM OR MORE, CLAD, PLATED OR COATED. - OTHER</Description>
          <CountQuantity>0</CountQuantity>
          <Classification>
            <ID>7210900000</ID>
            <BindingTariffReferenceID>10</BindingTariffReferenceID>
          </Classification>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""CAD"">200</ItemChargeAmount>
          </InvoiceLine>
          <DutyTaxFee>
            <TypeCode>VFT</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""CAD"">270.00</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>CUD</TypeCode>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>TOT</TypeCode>
            <DutyTaxFeeAssessmentBasis>
              <AdValoremTaxBaseAmount currencyID=""CAD"">200</AdValoremTaxBaseAmount>
            </DutyTaxFeeAssessmentBasis>
          </DutyTaxFee>
        </Commodity>
        <Commodity>
          <SequenceNumeric>3</SequenceNumeric>
          <Description>Pretend old builder and doesn't change this time, should remain old - 3</Description>
          <CountQuantity>0</CountQuantity>
          <Classification>
            <ID>7210900000</ID>
            <BindingTariffReferenceID>10</BindingTariffReferenceID>
          </Classification>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""CAD"">3300</ItemChargeAmount>
          </InvoiceLine>
          <DutyTaxFee>
            <TypeCode>VFT</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""CAD"">4455.00</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>CUD</TypeCode>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>TOT</TypeCode>
            <DutyTaxFeeAssessmentBasis>
              <AdValoremTaxBaseAmount currencyID=""CAD"">3300</AdValoremTaxBaseAmount>
            </DutyTaxFeeAssessmentBasis>
          </DutyTaxFee>
        </Commodity>
      </GovernmentAgencyGoodsItem>
    </GoodsShipment>
  </Declaration>
</DocumentMetaData>
";

			var outgoingMessageNew = entryHeader.Messages.AddNew();
			outgoingMessageNew.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessageNew.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			outgoingMessageNew.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			outgoingMessageNew.EM_MessageSubType = MessageSubTypeCodes.Codes.Change;
			outgoingMessageNew.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetaData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
  <ResponsibleAgencyName>CBSA</ResponsibleAgencyName>
  <AgencyAssignedCustomizationCode>CAD</AgencyAssignedCustomizationCode>
  <AgencyAssignedCustomizationVersionCode>001</AgencyAssignedCustomizationVersionCode>
  <FunctionalDefinition>CAD-IN</FunctionalDefinition>
  <CommunicationMetaData>
    <ApplicationReferenceID>1020700022087000001002</ApplicationReferenceID>
    <Sender>
      <ID>822066668RM0002</ID>
      <RoleCode>HN</RoleCode>
    </Sender>
    <Sender>
      <ID>864015219RM0003</ID>
      <RoleCode>AK</RoleCode>
    </Sender>
  </CommunicationMetaData>
  <Declaration>
    <FunctionCode>4</FunctionCode>
    <FunctionalReferenceID>B100000066</FunctionalReferenceID>
    <ID>10207000220870</ID>
    <LanguageCode>EN</LanguageCode>
    <TypeCode>AB</TypeCode>
    <VersionID>00001</VersionID>
    <TotalGrossMassMeasure>1</TotalGrossMassMeasure>
    <Amendment>
      <ChangeReasonCode>001</ChangeReasonCode>
      <AdditionalInformation>
        <StatementDescription>DEDWE</StatementDescription>
        <StatementTypeCode>CHG</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>2</StatementCode>
        <StatementTypeCode>APC</StatementTypeCode>
      </AdditionalInformation>
      <Pointer>
        <Location>DocumentMetaData/Declaration/GoodsShipment[1]/GovernmentAgencyGoodsItem/Commodity[2]</Location>
      </Pointer>
    </Amendment>
    <BorderTransportMeans>
      <ModeCode>01</ModeCode>
    </BorderTransportMeans>
    <Consignment>
      <Freight>
        <RateAmount>1</RateAmount>
      </Freight>
    </Consignment>
    <Declarant>
      <ID>864015219RM0003</ID>
    </Declarant>
    <ReleaseLocation>
      <ID>0497</ID>
    </ReleaseLocation>
    <Status>
      <ReleaseDateTime>
        <DateTimeString>20241231</DateTimeString>
      </ReleaseDateTime>
    </Status>
    <GoodsShipment>
      <SequenceNumeric>1</SequenceNumeric>
      <ExportCountry>
        <CountryCode />
      </ExportCountry>
      <Invoice>
        <ID>123</ID>
        <TypeCode>380</TypeCode>
      </Invoice>
      <Seller>
        <Name />
        <Address />
      </Seller>
      <GovernmentAgencyGoodsItem>
        <Commodity>
          <ExitDateTime>
            <DateTimeString>20250603</DateTimeString>
          </ExitDateTime>
          <SequenceNumeric>1</SequenceNumeric>
          <Description>FLAT-ROLLED PRODUCTS OF IRON OR NON-ALLOY STEEL, OF A WIDTH OF 600 MM OR MORE, CLAD, PLATED OR COATED. - OTHER</Description>
          <CountQuantity>0</CountQuantity>
          <Classification>
            <ID>7210900000</ID>
            <BindingTariffReferenceID>10</BindingTariffReferenceID>
          </Classification>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""CAD"">100</ItemChargeAmount>
          </InvoiceLine>
          <DutyTaxFee>
            <TypeCode>VFT</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""CAD"">135.00</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>CUD</TypeCode>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>TOT</TypeCode>
            <DutyTaxFeeAssessmentBasis>
              <AdValoremTaxBaseAmount currencyID=""CAD"">100</AdValoremTaxBaseAmount>
            </DutyTaxFeeAssessmentBasis>
          </DutyTaxFee>
        </Commodity>
        <Commodity>
          <ExitDateTime>
            <DateTimeString>20250603</DateTimeString>
          </ExitDateTime>
          <SequenceNumeric>2</SequenceNumeric>
          <Description>FLAT-ROLLED PRODUCTS OF IRON OR NON-ALLOY STEEL, OF A WIDTH OF 600 MM OR MORE, CLAD, PLATED OR COATED. - OTHER</Description>
          <CountQuantity>0</CountQuantity>
          <Classification>
            <ID>7210900000</ID>
            <BindingTariffReferenceID>10</BindingTariffReferenceID>
          </Classification>
          <DutyTaxFee>
            <TypeCode>VFT</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""CAD"">270.00</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>CUD</TypeCode>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>TOT</TypeCode>
            <DutyTaxFeeAssessmentBasis>
              <AdValoremTaxBaseAmount currencyID=""CAD"">200</AdValoremTaxBaseAmount>
            </DutyTaxFeeAssessmentBasis>
          </DutyTaxFee>
        </Commodity>
        <Commodity>
          <ExitDateTime>
            <DateTimeString>20250603</DateTimeString>
          </ExitDateTime>
          <SequenceNumeric>3</SequenceNumeric>
          <Description>FLAT-ROLLED PRODUCTS OF IRON OR NON-ALLOY STEEL, OF A WIDTH OF 600 MM OR MORE, CLAD, PLATED OR COATED. - OTHER</Description>
          <CountQuantity>0</CountQuantity>
          <Classification>
            <ID>7210900000</ID>
            <BindingTariffReferenceID>10</BindingTariffReferenceID>
          </Classification>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""CAD"">3300</ItemChargeAmount>
          </InvoiceLine>
          <DutyTaxFee>
            <TypeCode>VFT</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""CAD"">4455.00</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>CUD</TypeCode>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>TOT</TypeCode>
            <DutyTaxFeeAssessmentBasis>
              <AdValoremTaxBaseAmount currencyID=""CAD"">3300</AdValoremTaxBaseAmount>
            </DutyTaxFeeAssessmentBasis>
          </DutyTaxFee>
        </Commodity>
      </GovernmentAgencyGoodsItem>
    </GoodsShipment>
  </Declaration>
</DocumentMetaData>
";
			var expectedMessageTextNew = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetaData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
  <ResponsibleAgencyName>CBSA</ResponsibleAgencyName>
  <AgencyAssignedCustomizationCode>CAD</AgencyAssignedCustomizationCode>
  <AgencyAssignedCustomizationVersionCode>001</AgencyAssignedCustomizationVersionCode>
  <FunctionalDefinition>CAD-IN</FunctionalDefinition>
  <CommunicationMetaData>
    <ApplicationReferenceID>1020700022087000001002</ApplicationReferenceID>
    <Sender>
      <ID>822066668RM0002</ID>
      <RoleCode>HN</RoleCode>
    </Sender>
    <Sender>
      <ID>864015219RM0003</ID>
      <RoleCode>AK</RoleCode>
    </Sender>
  </CommunicationMetaData>
  <Declaration>
    <FunctionCode>4</FunctionCode>
    <FunctionalReferenceID>B100000066</FunctionalReferenceID>
    <ID>10207000220870</ID>
    <LanguageCode>EN</LanguageCode>
    <TypeCode>AB</TypeCode>
    <VersionID>00001</VersionID>
    <TotalGrossMassMeasure>1</TotalGrossMassMeasure>
    <Amendment>
      <ChangeReasonCode>001</ChangeReasonCode>
      <AdditionalInformation>
        <StatementDescription>DEDWE</StatementDescription>
        <StatementTypeCode>CHG</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>2</StatementCode>
        <StatementTypeCode>APC</StatementTypeCode>
      </AdditionalInformation>
      <Pointer>
        <Location>DocumentMetaData/Declaration/GoodsShipment[1]/GovernmentAgencyGoodsItem/Commodity[2]</Location>
      </Pointer>
    </Amendment>
    <BorderTransportMeans>
      <ModeCode>01</ModeCode>
    </BorderTransportMeans>
    <Consignment>
      <Freight>
        <RateAmount>1</RateAmount>
      </Freight>
    </Consignment>
    <Declarant>
      <ID>864015219RM0003</ID>
    </Declarant>
    <ReleaseLocation>
      <ID>0497</ID>
    </ReleaseLocation>
    <Status>
      <ReleaseDateTime>
        <DateTimeString>20241201</DateTimeString>
      </ReleaseDateTime>
    </Status>
    <GoodsShipment>
      <SequenceNumeric>1</SequenceNumeric>
      <ExportCountry>
        <CountryCode />
      </ExportCountry>
      <Invoice>
        <ID>123</ID>
        <TypeCode>380</TypeCode>
      </Invoice>
      <Seller>
        <Name />
        <Address />
      </Seller>
      <GovernmentAgencyGoodsItem>
        <Commodity>
          <ExitDateTime>
            <DateTimeString>20250603</DateTimeString>
          </ExitDateTime>
          <SequenceNumeric>1</SequenceNumeric>
          <Description>Pretend old builder and doesn't change this time, should remain old - 1</Description>
          <CountQuantity>0</CountQuantity>
          <Classification>
            <ID>7210900000</ID>
            <BindingTariffReferenceID>10</BindingTariffReferenceID>
          </Classification>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""CAD"">100</ItemChargeAmount>
          </InvoiceLine>
          <DutyTaxFee>
            <TypeCode>VFT</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""CAD"">135.00</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>CUD</TypeCode>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>TOT</TypeCode>
            <DutyTaxFeeAssessmentBasis>
              <AdValoremTaxBaseAmount currencyID=""CAD"">100</AdValoremTaxBaseAmount>
            </DutyTaxFeeAssessmentBasis>
          </DutyTaxFee>
        </Commodity>
        <Commodity>
          <ExitDateTime>
            <DateTimeString>20250603</DateTimeString>
          </ExitDateTime>
          <SequenceNumeric>2</SequenceNumeric>
          <Description>FLAT-ROLLED PRODUCTS OF IRON OR NON-ALLOY STEEL, OF A WIDTH OF 600 MM OR MORE, CLAD, PLATED OR COATED. - OTHER</Description>
          <CountQuantity>0</CountQuantity>
          <Classification>
            <ID>7210900000</ID>
            <BindingTariffReferenceID>10</BindingTariffReferenceID>
          </Classification>
          <DutyTaxFee>
            <TypeCode>VFT</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""CAD"">270.00</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>CUD</TypeCode>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>TOT</TypeCode>
            <DutyTaxFeeAssessmentBasis>
              <AdValoremTaxBaseAmount currencyID=""CAD"">200</AdValoremTaxBaseAmount>
            </DutyTaxFeeAssessmentBasis>
          </DutyTaxFee>
        </Commodity>
        <Commodity>
          <ExitDateTime>
            <DateTimeString>20250603</DateTimeString>
          </ExitDateTime>
          <SequenceNumeric>3</SequenceNumeric>
          <Description>Pretend old builder and doesn't change this time, should remain old - 3</Description>
          <CountQuantity>0</CountQuantity>
          <Classification>
            <ID>7210900000</ID>
            <BindingTariffReferenceID>10</BindingTariffReferenceID>
          </Classification>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""CAD"">3300</ItemChargeAmount>
          </InvoiceLine>
          <DutyTaxFee>
            <TypeCode>VFT</TypeCode>
            <Payment>
              <TaxAssessedAmount currencyID=""CAD"">4455.00</TaxAssessedAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>CUD</TypeCode>
          </DutyTaxFee>
          <DutyTaxFee>
            <TypeCode>TOT</TypeCode>
            <DutyTaxFeeAssessmentBasis>
              <AdValoremTaxBaseAmount currencyID=""CAD"">3300</AdValoremTaxBaseAmount>
            </DutyTaxFeeAssessmentBasis>
          </DutyTaxFee>
        </Commodity>
      </GovernmentAgencyGoodsItem>
    </GoodsShipment>
  </Declaration>
</DocumentMetaData>";
			#endregion

			var actionWrapper = new CADCorrectionMessageSendingActionWrapper(entryHeader);
			var actions = new CADCorrectionMessageSendingActionCollection(actionWrapper);
			var action1 = actions.AddNew();
			action1.InvoiceLineSequence = 2;
			action1.InvoiceSequence = 1;
			var wrapper = new CADMessageWrapper(entryHeader, actions);
			var manager = new CADMessageManagerForTest(wrapper);
			manager.RunAdditionalEDIMessageModification_Exposed(outgoingMessageNew);
			AssertEquals(expectedMessageTextNew, outgoingMessageNew.EM_MessageText);
		}

		[TestDate(2012, 7, 22)]
		public override void TestCanSendThisMessage()
		{
			using (ZArchitecture.Environment.Globals.SetIsWinzorForTest(true))
			using (ZArchitecture.Environment.Globals.SetIsUserInteractiveForTest(true))
			{
				Env.Security.CAB3MsgSend.IsAllowed = false;
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, string.Empty);
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.CL_CustomsValue = 1m;
				entryLine.ConfirmedFees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);

				var manager = new CADMessageManagerForTest((CADMessageWrapper)dataWrapper);
				ZString messageText;
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("MessageText", "Job not yet saved, Please save before sending.", messageText);

				Factory.Save();
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				Assert(messageText, messageText.Contains("You do not have the appropriate security rights to run this function."));

				Env.Security.CAB3MsgSend.IsAllowed = true;
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("MessageText", @"The Network Client ID is not configured,
in the registry for Company - EDI, Branch - BNE. Please contact your System Administrator.", messageText);

				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
				declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
				Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CBSABO, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
				{
					Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
					AssertEquals("MessageText", "CBSA is currently in the CARM Blackout period where no CAD entries may be sent. Entry cannot be sent at this time.", manager.LastMessage);
				}

				declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;
				var importer = Factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				declaration.JE_OH_Importer = importer.PK;
				declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
				declaration.ImporterAddInfo.ZO_IsGSTDirectAutoRated = true;
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2012, 7, 15);
				declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
				Factory.Save();
				Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CBSABO, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
				{
					Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
					AssertEquals("MessageText", "CBSA is currently in the CARM Blackout period where no CAD entries may be sent. Entry cannot be sent at this time.", manager.LastMessage);
				}

				manager.SetNextAnswer(true);
				declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
				Factory.Save();
				Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CBSABO, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
				{
					Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
					AssertEquals("MessageText", "CBSA is currently in the CARM Blackout period where no CAD entries may be sent. Entry cannot be sent at this time.", manager.LastMessage);
				}

				declaration.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
				Factory.Save();
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("MessageText", "CAD messages may not be sent when CAD Entry Type is 'No B3 required'", manager.LastMessage);

				declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
				declaration.JE_OH_Importer = importer.PK;
				declaration.Importer.CompanyData.OB_AROnCreditHold = true;
				declaration.Importer.CompanyData.OB_IsDebtor = true;
				declaration.HasChanges = false;
				declaration.Importer.HasChanges = false;
				Factory.Save();

				Assert("Not OK to send message", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("Submit message with credit restriction canceled.", messageText);

				declaration.JE_EntryStatus = "MAN";
				Factory.Save();
				Assert("Not OK to send message", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("Submit message with credit restriction canceled.", messageText);

				declaration.B3EntryHeader.CH_EntryStatus = CADEntryStatusList.Codes.Approved;
				declaration.Importer.CompanyData.OB_IsDebtor = false;
				declaration.Importer.CompanyData.OB_AROnCreditHold = false;
				declaration.HasChanges = false;
				declaration.Importer.HasChanges = false;
				Factory.Save();
				Assert("Can send CAD messages when CAD Entry Status is Clear", manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));

				declaration.B3EntryHeader.CH_EntryStatus = ZString.Empty;
				declaration.Importer.CompanyData.OB_IsDebtor = false;
				declaration.Importer.CompanyData.OB_AROnCreditHold = false;
				declaration.HasChanges = false;
				declaration.Importer.HasChanges = false;
				Factory.Save();

				declaration.TransactionNumber.AccountSecurityCode = "12345";
				declaration.TransactionNumber.SequentialNumber = "00006789";
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_DeclarationReference = "B00001002";
				declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration2.TransactionNumber.AccountSecurityCode = "54321";
				declaration2.TransactionNumber.SequentialNumber = "00006789";
				var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				entryHeader2.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
				var declaration3 = Factory.New<JobDeclaration>();
				declaration3.JE_DeclarationReference = "B00001003";
				declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration3.TransactionNumber.AccountSecurityCode = "54339";
				declaration3.TransactionNumber.SequentialNumber = "00006789";
				var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
				entryHeader3.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				Factory.Save();
				manager.SetNextAnswer(false);
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("MessageText", "There is another job B00001002 having the same transaction sequence number waiting for response. A CAD response from Customs does not contain an ASEC number and system might not be able to identify a correct originating job. It is recommended that you wait until other jobs are responded. Are you sure you wish to continue?", manager.LastMessage);

				entryHeader3.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
				Factory.Save();
				manager.SetNextAnswer(false);
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("MessageText", "There are other jobs B00001002,B00001003 having the same transaction sequence number waiting for response. A CAD response from Customs does not contain an ASEC number and system might not be able to identify a correct originating job. It is recommended that you wait until other jobs are responded. Are you sure you wish to continue?", manager.LastMessage);

				entryHeader2.CH_Status = ZString.Empty;
				entryHeader3.CH_Status = ZString.Empty;
				Factory.Save();

				Assert("OK to send message", manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CBSABO, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
				{
					Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
					AssertEquals("MessageText", "CBSA is currently in the CARM Blackout period where no CAD entries may be sent. Entry cannot be sent at this time.", manager.LastMessage);
				}

				entryHeader2 = null;
				entryHeader3 = null;
				Factory.Save();

				declaration2.B3EntryHeader.Delete();
				declaration3.B3EntryHeader.Delete();
				Assert("OK to send message", manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CBSABO, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
				{
					Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
					AssertEquals("MessageText", "CBSA is currently in the CARM Blackout period where no CAD entries may be sent. Entry cannot be sent at this time.", manager.LastMessage);
				}

				var outgoingMessagesCount = declaration.SentCADMessageCount;
				for (var i = outgoingMessagesCount; i < 999; i++)
				{
					var outgoingMessage = entryHeader.Messages.AddNew();
					outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
					outgoingMessage.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				}
				Factory.Save();

				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertEquals("MessageText", "This declaration has already sent 999 CAD messages.\nPlease create a new declaration.", manager.LastMessage);
			}
		}

		public override void TestGetMessageBuilder()
		{
			var manager = new CADMessageManagerForTest((CADMessageWrapper)dataWrapper);
			AssertEquals(typeof(CADMessageBuilder), manager.GetMessageBuilder_Exposed(MessageSubTypes.Create).GetType());
		}

		public override void TestMessageFriendlyName()
		{
			AssertEquals("CAD Message for Declaration", GetMessageManager().MessageFriendlyName);
		}

		public void TestIsCreditCheckRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = entry.MergedLines.AddNew();
			entryLine.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.CUD, 1m);
			entryLine.CL_CommoditySequence = 1;
			entryLine.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.GST, 2m);
			entryLine.CL_CommoditySequence = 2;
			Assert(entryLine.DutyFeeChangedSinceLastResponse);

			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);
			Factory.Save();
			Assert(entryLine.DutyFeeChangedSinceLastResponse);

			var manager = new CADMessageManagerForTest(new CADMessageWrapper(entry));
			AssertEquals(true, manager.PreCheck4CreditOKToSendChecking_Exposed(declaration));

			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 1m);
			Factory.Save();
			Assert(entryLine.DutyFeeChangedSinceLastResponse);

			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);
			Factory.Save();
			Assert(!entryLine.DutyFeeChangedSinceLastResponse);

			var manager2 = new CADMessageManagerForTest(new CADMessageWrapper(entry));
			AssertEquals(false, manager2.PreCheck4CreditOKToSendChecking_Exposed(declaration));
		}

		public override void TestPopulateMessages()
		{
			var manager = new CADMessageManagerForTest((CADMessageWrapper)dataWrapper);
			manager.PopulateMessage_Exposed(MessageSubTypes.Create);
			AssertEquals("1 message", 1, entryHeader.Messages.Count);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.CommercialAccountingDeclaration, entryHeader.Messages[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubTypeCodes.Codes.Original, entryHeader.Messages[0].EM_MessageSubType);
			AssertEquals("Message status", MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
		}

		protected override void AssertCanSendThisMessage(CusEntryHeader entryHeader, ZString expectedMessage)
		{
			Assert(true);
		}

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			return new CADMessageWrapper(entryHeader);
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			return new CADMessageManager((CADMessageWrapper)dataWrapper, new TestUserNotification());
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			base.SetUp();
		}

		protected override void AssertResetDeclaration(CAMessageManager manager, Action resetDeclaration, bool securityAllowed)
		{
			Assert(true);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
	}
}
