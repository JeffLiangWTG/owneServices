using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Utils;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using NUnit.Framework;

#pragma warning disable SA1312        // Variable names should begin with lower-case letter
#pragma warning disable SA1313        // Parameter names should begin with lower-case letter

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(PopulateJE_ValuationDateForCADDeclarations))]
	public sealed class PopulateJE_ValuationDateForCADDeclarationsTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateJE_ValuationDateForCADDeclarations(batchSize: 1);

		protected override void PrepareTestData()
		{
			Guid CH_PK, EM_PK;

			JE_PK1 = CreateJobDeclaration("IMP");
			CH_PK = CreateCusEntryHeader(JE_PK1, "CAD");
			EM_PK = CreateEDIMessage(CH_PK, GetCADResponseMessage("20250131"));

			JE_PK2 = CreateJobDeclaration("LVS");
			CH_PK = CreateCusEntryHeader(JE_PK2, "CAD");
			EM_PK = CreateEDIMessage(CH_PK, GetCADResponseMessage("20261201"));

			JE_PK4 = CreateJobDeclaration("EXP");
			CH_PK = CreateCusEntryHeader(JE_PK4, "CAD");
			EM_PK = CreateEDIMessage(CH_PK, GetCADResponseMessage("20250101"));

			JE_PK5 = CreateJobDeclaration("IMP");
			CH_PK = CreateCusEntryHeader(JE_PK5, "CAD");
			EM_PK = CreateEDIMessage(CH_PK, GetCADResponseMessage("1"));

			JE_PK6 = CreateJobDeclaration("IMP");
			CH_PK = CreateCusEntryHeader(JE_PK6, "CAD");
			EM_PK = CreateEDIMessage(CH_PK, GetCADResponseMessage(""));

			JE_PK7 = CreateJobDeclaration("IMP");
			CH_PK = CreateCusEntryHeader(JE_PK7, "CAD");
			EM_PK = CreateEDIMessage(CH_PK, @"<DocumentMetaData xmlns=""urn: wco:datamodel: WCO:Declaration: 1""></DocumentMetaData>");

			JE_PK8 = CreateJobDeclaration("IMP");
			CH_PK = CreateCusEntryHeader(JE_PK8, "CAD");
			EM_PK = CreateEDIMessage(CH_PK, "1");

			JE_PK9 = CreateJobDeclaration("IMP");
			CH_PK = CreateCusEntryHeader(JE_PK9, "CAD");
			EM_PK = CreateEDIMessage(CH_PK, "");

			JE_PK10 = CreateJobDeclaration("IMP");
			CH_PK = CreateCusEntryHeader(JE_PK10, "REL");
			EM_PK = CreateEDIMessage(CH_PK, GetCADResponseMessage("20250101"));

			JE_PK11 = CreateJobDeclaration("IMP");
			CH_PK = CreateCusEntryHeader(JE_PK1, "CAD");
			EM_PK = CreateEDIMessage(CH_PK, GetCADResponseMessage("20250127"), DateTime.UtcNow.AddDays(-3));
			EM_PK = CreateEDIMessage(CH_PK, GetCADResponseMessage("20250201"), DateTime.UtcNow.AddDays(-2));
			EM_PK = CreateEDIMessage(CH_PK, GetCADResponseMessage("20250205"), DateTime.UtcNow.AddDays(-1));
		}

		protected override void AssertTransformationResults()
		{
			AssertValuationDate(JE_PK1, new DateTime(2025, 01, 31));
			AssertValuationDate(JE_PK2, new DateTime(2026, 12, 01));
			AssertValuationDateIsNull(JE_PK4);
			AssertValuationDateIsNull(JE_PK5);
			AssertValuationDateIsNull(JE_PK6);
			AssertValuationDateIsNull(JE_PK7);
			AssertValuationDateIsNull(JE_PK8);
			AssertValuationDateIsNull(JE_PK9);
			AssertValuationDateIsNull(JE_PK10);
			AssertValuationDate(JE_PK11, new DateTime(2025, 02, 05));

			AssertNull(PopulateJE_ValuationDateForCADDeclarations.ClusterKeyWatermark.Select());
		}

		public void TestOnlinePostUpgradeVerbosity()
		{
			PrepareTestData();
			var actual = new List<string>();
			var transformation = new PopulateJE_ValuationDateForCADDeclarations(batchSize: 3);
			((IOnlineTransformation)transformation).Run(actual.Add, CancellationToken.None);
			CombineAssertions(() =>
			{
				AssertEquals(4, actual.Count);
				Assert(actual[0].StartsWith("Failed to parse XML:"));
				Assert(actual[1].StartsWith("Failed to parse XML:"));
				AssertEquals("Processing completed, processed 10 jobs, updated 3 records.", actual[2]);
				AssertEquals("\tCompleted: Populate JE_ValuationDate for CAD entries", actual[3]);
			});
		}

		void AssertValuationDate(Guid JE_PK, DateTime expected)
		{
			var valuationDate = GetValuationDate(JE_PK);
			AssertEquals(expected.Date, valuationDate.Date);
		}

		void AssertValuationDateIsNull(Guid JE_PK)
		{
			AssertExceptionThrown<ExecuteScalarReturnedNullException>(() => GetValuationDate(JE_PK));
		}

		DateTime GetValuationDate(Guid JE_PK)
			=> Db.Connection.ExecuteScalar<DateTime>("SELECT JE_ValuationDate FROM dbo.JobDeclaration WHERE JE_PK = @pk", x => x.AddParameter("@pk", SqlDbType.UniqueIdentifier, JE_PK));

		Guid CreateJobDeclaration(string JE_MessageType)
		{
			var JE_PK = Guid.NewGuid();
			clusterKey++;
			data.CreateDeclaration(JE_PK, $"B{clusterKey:00000000}", clusterKey, GB_PK, GC_PK, dataModel: "CA", messageType: JE_MessageType);
			return JE_PK;
		}

		Guid CreateCusEntryHeader(Guid JE_PK, string CH_MessageType)
			=> data.CreateCusEntryHeader("CA", JE_PK, clusterKey, CH_MessageType);

		Guid CreateEDIMessage(Guid EM_LinkUniqueID, string EM_MessageText)
			=> CreateEDIMessage(EM_LinkUniqueID, EM_MessageText, DateTime.UtcNow.AddDays(-1));

		Guid CreateEDIMessage(Guid EM_LinkUniqueID, string EM_MessageText, DateTime EM_SystemCreateTimeUtc)
			=> data.CreateEDIMessage(GB_PK, GE_PK, "RCV", "CAI", "RCV", "CAD", "1", EM_LinkUniqueID, "CusEntryHeader", EM_MessageText, EM_SystemCreateTimeUtc, messageSubType: "CLO");

		protected override void SetUp()
		{
			base.SetUp();
			data = new TransformationTestDataCreator();
			GC_PK = data.CreateCompany(Guid.NewGuid(), "C", "CA", "CAD");
			GB_PK = data.CreateBranch("B", "001", GC_PK);
			GE_PK = data.CreateGlbDepartment("D");
		}

		TransformationTestDataCreator data;
		Guid GC_PK, GB_PK, GE_PK, JE_PK1, JE_PK2, JE_PK4, JE_PK5, JE_PK6, JE_PK7, JE_PK8, JE_PK9, JE_PK10, JE_PK11;
		int clusterKey;

		string GetCADResponseMessage(string dateTimeString) => @$"
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
	<CommunicationMetaData>
		<ApplicationReferenceID>1020700350233200001091</ApplicationReferenceID>
		<Recipient>
			<ID>822066668RM0002</ID>
		</Recipient>
	</CommunicationMetaData>
	<Response>
		<IssueDateTime>
			<DateTimeString>20220926071241</DateTimeString>
		</IssueDateTime>
		<Declaration>
			<AcceptanceDateTime>
				<DateTimeString>20250304071044</DateTimeString>
			</AcceptanceDateTime>
			<FunctionCode>9</FunctionCode>
			<FunctionalReferenceID>B00224912</FunctionalReferenceID>
			<ID>10207003502332</ID>
			<LanguageCode>EN</LanguageCode>
			<TypeCode>30</TypeCode>
			<VersionID>00001</VersionID>
			<AdditionalInformation>
				<StatementCode>30-2</StatementCode>
				<StatementTypeCode>STC</StatementTypeCode>
			</AdditionalInformation>
			<BorderTransportMeans>
				<ModeCode>02</ModeCode>
			</BorderTransportMeans>
			<Declarant>
				<ID>102891009RM0004</ID>
			</Declarant>
			<DutyTaxFee>
				<TypeCode>CUD</TypeCode>
				<Payment>
					<TaxAssessedAmount currencyID=""CAD"">1950.00</TaxAssessedAmount>
					<PaymentAmount currencyID=""CAD"">1950.00</PaymentAmount>
				</Payment>
			</DutyTaxFee>
			<DutyTaxFee>
				<TypeCode>GST</TypeCode>
				<Payment>
					<TaxAssessedAmount currencyID=""CAD"">1597.50</TaxAssessedAmount>
					<PaymentAmount currencyID=""CAD"">1597.50</PaymentAmount>
				</Payment>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount currencyID=""CAD"">30000.00</AdValoremTaxBaseAmount>
				<TypeCode>TOT</TypeCode>
				<Payment>
					<TaxAssessedAmount currencyID=""CAD"">3547.50</TaxAssessedAmount>
					<DueDateTime>
						<DateTimeString>{dateTimeString}</DateTimeString>
					</DueDateTime>
					<PaymentAmount currencyID=""CAD"">3547.50</PaymentAmount>
				</Payment>
			</DutyTaxFee>
			<Importer>
				<ID>793914086RM0001</ID>
			</Importer>
			<PreviousDocument>
				<ID>13284006000241</ID>
				<TypeCode>632</TypeCode>
			</PreviousDocument>
			<ReleaseLocation>
				<ID>0453</ID>
				<Warehouse>
					<ID>2705</ID>
					<TypeCode>18</TypeCode>
					<RoleCode>ST</RoleCode>
				</Warehouse>
				<Warehouse>
					<ID>5261</ID>
					<TypeCode>18</TypeCode>
					<RoleCode>SF</RoleCode>
				</Warehouse>
			</ReleaseLocation>
			<Status>
				<ReleaseDateTime>
					<DateTimeString>20220926</DateTimeString>
				</ReleaseDateTime>
			</Status>
			<GoodsShipment>
				<SequenceNumeric>00001</SequenceNumeric>
				<Invoice>
					<AmountAmount currencyID=""CAD"">30000.00</AmountAmount>
					<ID>DCTCE7INV4</ID>
					<TypeCode>380</TypeCode>
				</Invoice>
				<Seller>
					<Name>HENKEL CORP.</Name>
					<Address>
						<CityName>ENOREE</CityName>
						<CountryCode>US</CountryCode>
						<CountrySubDivisionCode>SC</CountrySubDivisionCode>
						<Line>14351 HIGHWAY 221 WOODRUFF PLANT</Line>
						<PostcodeID>29335</PostcodeID>
					</Address>
					<Communication>
						<ID>+1 864-969-6679</ID>
					</Communication>
				</Seller>
				<GovernmentAgencyGoodsItem>
					<Commodity>
						<ExitDateTime>
							<DateTimeString>20220926</DateTimeString>
						</ExitDateTime>
						<SequenceNumeric>00001</SequenceNumeric>
						<Description>WATERS, INCLUDING NATURAL OR ARTIFICIAL FLAVOURING</Description>
						<CountQuantity unitCode=""LTR"">300.000</CountQuantity>
						<AdditionalInformation>
							<LimitDateTime>
								<DateTimeString>20221226</DateTimeString>
							</LimitDateTime>
							<StatementTypeCode>TLE</StatementTypeCode>
						</AdditionalInformation>
						<AdditionalInformation>
							<LimitDateTime>
								<DateTimeString>20220926</DateTimeString>
							</LimitDateTime>
							<StatementTypeCode>TLS</StatementTypeCode>
						</AdditionalInformation>
						<AdditionalInformation>
							<StatementCode>4</StatementCode>
							<StatementTypeCode>TLT</StatementTypeCode>
						</AdditionalInformation>
						<AdditionalInformation>
							<StatementCode>013</StatementCode>
							<StatementTypeCode>VDC</StatementTypeCode>
						</AdditionalInformation>
						<Classification>
							<ID>2201900000</ID>
							<BindingTariffReferenceID>10</BindingTariffReferenceID>
						</Classification>
						<ExportCountry>
							<CountryCode>US</CountryCode>
							<RegionID>NY</RegionID>
						</ExportCountry>
						<Origin>
							<CountryCode>US</CountryCode>
							<RegionID>NY</RegionID>
						</Origin>
						<PreviousDocument>
							<LineNumeric>00001</LineNumeric>
							<TypeCode>632</TypeCode>
						</PreviousDocument>
						<DutyTaxFee>
							<DutyRegimeCode>002</DutyRegimeCode>
							<TypeCode>CUD</TypeCode>
							<AdditionalInformation>
								<StatementDescription>6.5%</StatementDescription>
								<StatementTypeCode>AAF</StatementTypeCode>
							</AdditionalInformation>
							<Payment>
								<TaxAssessedAmount currencyID=""CAD"">1950.00</TaxAssessedAmount>
								<PaymentAmount currencyID=""CAD"">1950.00</PaymentAmount>
							</Payment>
							<Rate>
								<TariffClassSpecificationCode>N</TariffClassSpecificationCode>
								<AdvaloremTaxBaseRateNumeric>6.50000</AdvaloremTaxBaseRateNumeric>
							</Rate>
						</DutyTaxFee>
						<DutyTaxFee>
							<TypeCode>GST</TypeCode>
							<AdditionalInformation>
								<StatementDescription>5.00%</StatementDescription>
								<StatementTypeCode>AAF</StatementTypeCode>
							</AdditionalInformation>
							<Payment>
								<TaxAssessedAmount currencyID=""CAD"">1597.50</TaxAssessedAmount>
								<PaymentAmount currencyID=""CAD"">1597.50</PaymentAmount>
							</Payment>
							<Rate>
								<AdvaloremTaxBaseRateNumeric>5.00000</AdvaloremTaxBaseRateNumeric>
							</Rate>
						</DutyTaxFee>
						<DutyTaxFee>
							<TypeCode>TOT</TypeCode>
							<DutyTaxFeeAssessmentBasis>
								<AdValoremTaxBaseAmount currencyID=""CAD"">30000.00</AdValoremTaxBaseAmount>
							</DutyTaxFeeAssessmentBasis>
							<DutyTaxFeeAssessmentBasis>
								<AdValoremTaxBaseAmount currencyID=""CAD"">30000.00</AdValoremTaxBaseAmount>
								<RateNumeric>1.00000000</RateNumeric>
							</DutyTaxFeeAssessmentBasis>
							<Payment>
								<TaxAssessedAmount currencyID=""CAD"">3547.50</TaxAssessedAmount>
								<PaymentAmount currencyID=""CAD"">3547.50</PaymentAmount>
							</Payment>
						</DutyTaxFee>
						<DutyTaxFee>
							<TypeCode>VFT</TypeCode>
							<Payment>
								<TaxAssessedAmount currencyID=""CAD"">31950.00</TaxAssessedAmount>
							</Payment>
						</DutyTaxFee>
					</Commodity>
				</GovernmentAgencyGoodsItem>
			</GoodsShipment>
		</Declaration>
		<Status>
			<NameCode>39</NameCode>
		</Status>
	</Response>
</DocumentMetaData>
";
	}
}
