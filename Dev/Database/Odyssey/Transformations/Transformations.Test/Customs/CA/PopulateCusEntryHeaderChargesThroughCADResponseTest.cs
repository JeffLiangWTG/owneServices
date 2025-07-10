using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(PopulateCusEntryHeaderChargesThroughCADResponse))]
	public class PopulateCusEntryHeaderChargesThroughCADResponseTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateCusEntryHeaderChargesThroughCADResponse();

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();

			TestCase(nameof(ch11), ch11);
			TestCase(nameof(ch21), ch21);
			TestCase(nameof(ch22), ch22, [("AAD", 2.1m)]);
			TestCase(nameof(ch31), ch31);
			TestCase(nameof(ch41), ch41);
			TestCase(nameof(ch51), ch51);
			TestCase(nameof(ch111), ch111);
			TestCase(nameof(ch61), ch61);
			TestCase(nameof(ch71), ch71);

			TestCase(nameof(ch81), ch81, [("CUD", 1.2m), ("GST", 2.2m), ("TOT", 3.1m)]);
			TestCase(nameof(ch91), ch91, [("CUD", 10.2m), ("GST", 20.2m), ("TOT", 30.1m)]);
			TestCase(nameof(ch101), ch101, [("GST", 0m), ("TOT", 0m)]);

			void TestCase(string key, Guid chPK, (string chargeType, decimal chargeAmount)[] expectedCharges = null)
			{
				var actualCharges = new List<(string chargeType, decimal chargeAmount)>();
				TestConnection.ExecuteReader(
					$"SELECT C1_ChargeType, C1_ChargeAmount FROM dbo.CusEntryHeaderCharges WHERE C1_CH = @chPK",
					cmd => cmd.AddParameter("@chPK", SqlDbType.UniqueIdentifier, chPK),
					reader => actualCharges.Add((reader.GetString(0), reader.GetDecimal(1)))
				);
				if (expectedCharges == null)
				{
					Assert(key, actualCharges.Count == 0);
				}
				else
				{
					AssertArrayEqualsByElements(key, expectedCharges, actualCharges.OrderBy(o => o.chargeType).ThenBy(o => o.chargeAmount).ToArray());
				}
			}
		}

		public void TestLogging()
		{
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(Db.Connection, Db.SqlDbOwnerSchema, EDIMessageSchema.Constants.TableName, "Constraint_EM_LinkTable_NoCheck"))
			{
				PrepareBasicData();
				var je1 = TestDataCreator.CreateJobDeclaration(branch, company, "1", "IMP", 1);
				var ch11 = TestDataCreator.CreateCusEntryHeader(je1, 1, "CAD", dataModel: "CA");
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch11, DateTime.Now, "RCV", "CAI", direction: "RCV", messageType: "CAD", messageSubType: "CLC", messageText: MessageTextWithoutDutyTaxFee);

				var je2 = TestDataCreator.CreateJobDeclaration(branch, company, "2", "IMP", 2);
				var ch21 = TestDataCreator.CreateCusEntryHeader(je2, 2, "CAD", dataModel: "CA");
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch21, DateTime.Now, "RCV", "CAI", direction: "RCV", messageType: "CAD", messageSubType: "CLC", messageText: MessageTextWithDutyTaxFee);
			}
			var logger = new List<string>();
			var transformation = (IOnlineTransformation)(new PopulateCusEntryHeaderChargesThroughCADResponse());
			transformation.Run(s => logger.Add(s), CancellationToken.None);
			AssertContainsExactElementsInExactOrder(new[]
				{
					"Processing completed, processed 2 entries, created 3 charges.",
					"\tCompleted: Populate CusEntryHeader charges through CAD response",
				}, logger);
		}

		protected override void PrepareTestData()
		{
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(Db.Connection, Db.SqlDbOwnerSchema, EDIMessageSchema.Constants.TableName, "Constraint_EM_LinkTable_NoCheck"))
			{
				base.PrepareTestData();
				PrepareBasicData();

				// Cases that no CusEntryHeaderCharges will be created
				// CH_DataModel != CA
				var je1 = TestDataCreator.CreateJobDeclaration(branch, company, "1", "IMP", 1);
				ch11 = TestDataCreator.CreateCusEntryHeader(je1, 1, "CAD", dataModel: "US");
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch11, DateTime.Now, "RCV", "CAI", direction: "RCV", messageType: "CAD", messageSubType: "CLO", messageText: MessageTextWithDutyTaxFee);

				// CH_MessageType != 'CAD'
				var je2 = TestDataCreator.CreateJobDeclaration(branch, company, "2", "IMP", 2);
				ch21 = TestDataCreator.CreateCusEntryHeader(je2, 2, "B3C", dataModel: "CA");
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch21, DateTime.Now, "RCV", "CAI", direction: "RCV", messageType: "CAD", messageSubType: "CLC", messageText: MessageTextWithDutyTaxFee);

				// Existing CusEntryHeaderCharges
				ch22 = TestDataCreator.CreateCusEntryHeader(je2, 2, "CAD", dataModel: "CA");
				TestDataCreator.CreateCusEntryHeaderCharges(ch22, "AAD", 2.1m, 2);
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch22, DateTime.Now, "RCV", "CAI", direction: "RCV", messageType: "CAD", messageSubType: "CLO", messageText: MessageTextWithDutyTaxFee);

				// EM_ApplicationCode != 'CAI'
				var je3 = TestDataCreator.CreateJobDeclaration(branch, company, "3", "IMP", 3);
				ch31 = TestDataCreator.CreateCusEntryHeader(je3, 3, "CAD", dataModel: "CA");
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch31, DateTime.Now, "RCV", "LCP", direction: "RCV", messageType: "CAD", messageSubType: "CLC", messageText: MessageTextWithDutyTaxFee);

				// EM_ReceiveTransmit != 'RCV'
				var je4 = TestDataCreator.CreateJobDeclaration(branch, company, "4", "IMP", 4);
				ch41 = TestDataCreator.CreateCusEntryHeader(je4, 4, "CAD", dataModel: "CA");
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch41, DateTime.Now, "RCV", "CAI", direction: "TRX", messageType: "CAD", messageSubType: "CLO", messageText: MessageTextWithDutyTaxFee);

				// EM_Status != 'RCV'
				var je5 = TestDataCreator.CreateJobDeclaration(branch, company, "5", "IMP", 5);
				ch51 = TestDataCreator.CreateCusEntryHeader(je5, 5, "CAD", dataModel: "CA");
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch51, DateTime.Now, "QUE", "CAI", direction: "RCV", messageType: "CAD", messageSubType: "CLC", messageText: MessageTextWithDutyTaxFee);

				// EM_MessageType != 'CAD'
				var je11 = TestDataCreator.CreateJobDeclaration(branch, company, "11", "IMP", 11);
				ch111 = TestDataCreator.CreateCusEntryHeader(je11, 11, "CAD", dataModel: "CA");
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch111, DateTime.Now, "RCV", "CAI", direction: "RCV", messageType: "B3C", messageSubType: "CLC", messageText: MessageTextWithDutyTaxFee);

				// EM_MessageSubType NOT IN ('CLO', 'CLC')
				var je6 = TestDataCreator.CreateJobDeclaration(branch, company, "6", "IMP", 6);
				ch61 = TestDataCreator.CreateCusEntryHeader(je6, 6, "CAD", dataModel: "CA");
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch61, DateTime.Now, "RCV", "CAI", direction: "RCV", messageType: "CAD", messageText: MessageTextWithDutyTaxFee);

				// EM_MessageText does not contain DutyTaxFee
				var je7 = TestDataCreator.CreateJobDeclaration(branch, company, "7", "IMP", 7);
				ch71 = TestDataCreator.CreateCusEntryHeader(je7, 7, "CAD", dataModel: "CA");
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch71, DateTime.Now, "RCV", "CAI", direction: "RCV", messageType: "CAD", messageSubType: "CLC", messageText: MessageTextWithoutDutyTaxFee);

				// Cases that CusEntryHeaderCharges will be created
				// EM_MessageSubType = 'CLC'
				var je8 = TestDataCreator.CreateJobDeclaration(branch, company, "8", "IMP", 8);
				ch81 = TestDataCreator.CreateCusEntryHeader(je8, 8, "CAD", dataModel: "CA");
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch81, DateTime.Now, "RCV", "CAI", direction: "RCV", messageType: "CAD", messageSubType: "CLC", messageText: MessageTextWithDutyTaxFee);

				// EM_MessageSubType = 'CLO'
				var je9 = TestDataCreator.CreateJobDeclaration(branch, company, "9", "IMP", 9);
				ch91 = TestDataCreator.CreateCusEntryHeader(je9, 9, "CAD", dataModel: "CA");
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch91, DateTime.Now, "RCV", "CAI", direction: "RCV", messageType: "CAD", messageSubType: "CLO", messageText: MessageTextWithDutyTaxFee);
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch91, DateTime.Now.AddSeconds(1), "RCV", "CAI", direction: "RCV", messageType: "CAD", messageSubType: "CLO", messageText: MessageTextWithDutyTaxFeeNewer);

				// EM_MessageText with special cases of DutyTaxFee
				var je10 = TestDataCreator.CreateJobDeclaration(branch, company, "10", "IMP", 10);
				ch101 = TestDataCreator.CreateCusEntryHeader(je10, 10, "CAD", dataModel: "CA");
				TestDataCreator.CreateEDIMessage(branch, department, Guid.Empty, ch101, DateTime.Now, "RCV", "CAI", direction: "RCV", messageType: "CAD", messageSubType: "CLO", messageText: MessageTextWithSpecialCasesDutyTaxFee);
			}
		}

		void PrepareBasicData()
		{
			company = TestDataCreator.CreateCompany("DCA", "CA", "CAD");
			branch = TestDataCreator.CreateBranch(company, "BRN", "CAXXX");
			department = TestDataCreator.CreateDepartment("DEV");
		}

		const string MessageTextWithoutDutyTaxFee = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns= ""urn:wco:datamodel:WCO:Declaration:1 "">
    <Response>
        <Declaration/>
    </Response>
</DocumentMetaData>";

		const string MessageTextWithDutyTaxFee = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
  <Response>
    <Declaration>
      <DutyTaxFee>
        <TypeCode>CUD</TypeCode>
        <Payment>
          <TaxAssessedAmount currencyID=""CAD"">1.1</TaxAssessedAmount>
          <PaymentAmount currencyID=""CAD"">1.2</PaymentAmount>
        </Payment>
      </DutyTaxFee>
      <DutyTaxFee>
        <TypeCode>GST</TypeCode>
        <Payment>
          <TaxAssessedAmount currencyID=""CAD"">2.1</TaxAssessedAmount>
          <PaymentAmount currencyID=""CAD"">2.2</PaymentAmount>
        </Payment>
      </DutyTaxFee>
      <DutyTaxFee>
        <AdValoremTaxBaseAmount currencyID=""CAD"">3.1</AdValoremTaxBaseAmount>
        <TypeCode>TOT</TypeCode>
        <Payment>
          <TaxAssessedAmount currencyID=""CAD"">3.2</TaxAssessedAmount>
          <DueDateTime>
            <DateTimeString>20220526</DateTimeString>
          </DueDateTime>
          <PaymentAmount currencyID=""CAD"">3.3</PaymentAmount>
        </Payment>
      </DutyTaxFee>
      <GoodsShipment>
        <GovernmentAgencyGoodsItem>
          <Commodity>
            <DutyTaxFee>
              <TypeCode>TOT</TypeCode>
              <DutyTaxFeeAssessmentBasis>
                <AdValoremTaxBaseAmount currencyID=""CAD"">30.1</AdValoremTaxBaseAmount>
              </DutyTaxFeeAssessmentBasis>
              <DutyTaxFeeAssessmentBasis>
                <AdValoremTaxBaseAmount currencyID=""CAD"">30.2</AdValoremTaxBaseAmount>
                <RateNumeric>1.00000000</RateNumeric>
              </DutyTaxFeeAssessmentBasis>
              <Payment>
                <TaxAssessedAmount currencyID=""CAD"">30.3</TaxAssessedAmount>
                <PaymentAmount currencyID=""CAD"">30.4</PaymentAmount>
              </Payment>
            </DutyTaxFee>
          </Commodity>
        </GovernmentAgencyGoodsItem>
      </GoodsShipment>
    </Declaration>
  </Response>
</DocumentMetaData>
";

		const string MessageTextWithDutyTaxFeeNewer = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
  <Response>
    <Declaration>
      <DutyTaxFee>
        <TypeCode>CUD</TypeCode>
        <Payment>
          <TaxAssessedAmount currencyID=""CAD"">10.1</TaxAssessedAmount>
          <PaymentAmount currencyID=""CAD"">10.2</PaymentAmount>
        </Payment>
      </DutyTaxFee>
      <DutyTaxFee>
        <TypeCode>GST</TypeCode>
        <Payment>
          <TaxAssessedAmount currencyID=""CAD"">20.1</TaxAssessedAmount>
          <PaymentAmount currencyID=""CAD"">20.2</PaymentAmount>
        </Payment>
      </DutyTaxFee>
      <DutyTaxFee>
        <AdValoremTaxBaseAmount currencyID=""CAD"">30.1</AdValoremTaxBaseAmount>
        <TypeCode>TOT</TypeCode>
        <Payment>
          <TaxAssessedAmount currencyID=""CAD"">30.2</TaxAssessedAmount>
          <DueDateTime>
            <DateTimeString>20220526</DateTimeString>
          </DueDateTime>
          <PaymentAmount currencyID=""CAD"">30.3</PaymentAmount>
        </Payment>
      </DutyTaxFee>
      <GoodsShipment>
        <GovernmentAgencyGoodsItem>
          <Commodity>
            <DutyTaxFee>
              <TypeCode>TOT</TypeCode>
              <DutyTaxFeeAssessmentBasis>
                <AdValoremTaxBaseAmount currencyID=""CAD"">300.1</AdValoremTaxBaseAmount>
              </DutyTaxFeeAssessmentBasis>
              <DutyTaxFeeAssessmentBasis>
                <AdValoremTaxBaseAmount currencyID=""CAD"">300.2</AdValoremTaxBaseAmount>
                <RateNumeric>1.00000000</RateNumeric>
              </DutyTaxFeeAssessmentBasis>
              <Payment>
                <TaxAssessedAmount currencyID=""CAD"">300.3</TaxAssessedAmount>
                <PaymentAmount currencyID=""CAD"">300.4</PaymentAmount>
              </Payment>
            </DutyTaxFee>
          </Commodity>
        </GovernmentAgencyGoodsItem>
      </GoodsShipment>
    </Declaration>
  </Response>
</DocumentMetaData>
";

		const string MessageTextWithSpecialCasesDutyTaxFee = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
  <Response>
    <Declaration>
      <!--TypeCode is Missing -->
      <DutyTaxFee>
        <AdValoremTaxBaseAmount currencyID=""CAD"">1.1</AdValoremTaxBaseAmount>
        <Payment>
          <TaxAssessedAmount currencyID=""CAD"">1.2</TaxAssessedAmount>
          <PaymentAmount currencyID=""CAD"">1.3</PaymentAmount>
        </Payment>
      </DutyTaxFee>
      <!--TypeCode is empty -->
      <DutyTaxFee>
        <TypeCode></TypeCode>
        <Payment>
          <TaxAssessedAmount currencyID=""CAD"">4.1</TaxAssessedAmount>
          <PaymentAmount currencyID=""CAD"">4.2</PaymentAmount>
        </Payment>
      </DutyTaxFee>
      <!--Non-TOT charge, PaymentAmount is Missing -->
      <DutyTaxFee>
        <TypeCode>GST</TypeCode>
        <Payment>
          <TaxAssessedAmount currencyID=""CAD"">2.1</TaxAssessedAmount>
        </Payment>
      </DutyTaxFee>
      <!--TOT charge, AdValoremTaxBaseAmount is Missing -->
      <DutyTaxFee>
        <TypeCode>TOT</TypeCode>
        <Payment>
          <TaxAssessedAmount currencyID=""CAD"">3.2</TaxAssessedAmount>
          <DueDateTime>
            <DateTimeString>20220526</DateTimeString>
          </DueDateTime>
          <PaymentAmount currencyID=""CAD"">3.3</PaymentAmount>
        </Payment>
      </DutyTaxFee>
      <!--Invalid charge type-->
      <DutyTaxFee>
        <TypeCode>NOTALLOWED</TypeCode>
        <Payment>
          <TaxAssessedAmount currencyID=""CAD"">1.2</TaxAssessedAmount>
          <PaymentAmount currencyID=""CAD"">1.3</PaymentAmount>
        </Payment>
      </DutyTaxFee>
    </Declaration>
  </Response>
</DocumentMetaData>
";

		Guid company, branch, department, ch11, ch21, ch22, ch31, ch41, ch51, ch61, ch71, ch81, ch91, ch101, ch111;
	}
}
