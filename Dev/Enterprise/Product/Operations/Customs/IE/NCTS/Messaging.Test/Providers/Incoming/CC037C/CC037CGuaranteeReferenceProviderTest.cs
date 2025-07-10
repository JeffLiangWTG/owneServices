using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC037CGuaranteeReferenceProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("GuaranteeReferenceType missing", () => new CC037CGuaranteeReferenceProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber", 1, provider.SequenceNumber);
		}

		public void TestGuaranteeAmount()
		{
			AssertEquals("GRN", "12GRNCC055C012345A678901", provider.GRN);
		}

		public void TestGuaranteeMonitoringCode()
		{
			AssertEquals("GuaranteeMonitoringCode", "7", provider.GuaranteeMonitoringCode);
		}

		public void TestNullGuaranteeQuery()
		{
			CombineAssertions(() =>
			{
				var emptyProvider = GetEmptyProvider();
				AssertEquals("Null value should return empty", ZString.Empty, emptyProvider.QueryIdentifier);
				AssertEquals("Null value should return empty", ZDate.Empty, emptyProvider.QueryPeriodFromDate);
				AssertEquals("Null value should return empty", ZDate.Empty, emptyProvider.QueryPeriodToDate);
			});
		}

		public void TestQueryIdentifier()
		{
			AssertEquals("QueryIdentifier", "1", provider.QueryIdentifier);
		}

		public void TestQueryPeriodFromDate()
		{
			AssertEquals("QueryPeriodFromDate", new ZDate(2023, 01, 31), provider.QueryPeriodFromDate);
		}

		public void TestQueryPeriodToDate()
		{
			AssertEquals("QueryPeriodToDate", new ZDate(2023, 02, 28), provider.QueryPeriodToDate);
		}

		public void TestUsages()
		{
			AssertEquals("Owner is empty", 0, GetEmptyProvider().Usages.Count);

			var usages1 = provider.Usages;
			AssertType<CC037CUsageProvider>(usages1.ElementAt(0));
			var usages2 = provider.Usages;
			AssertSame("Is cached", usages1, usages2);
		}

		public void TestGuarantor()
		{
			AssertEquals("Guarantor is null", null, GetEmptyProvider().Guarantor);

			var guarantor1 = provider.Guarantor;
			AssertType<CC037GuarantorProvider>(guarantor1);
			var guarantor2 = provider.Guarantor;
			AssertSame("Is cached", guarantor1, guarantor2);
		}

		public void TestExposure()
		{
			AssertEquals("Exposure is null", null, GetEmptyProvider().Exposure);

			var exposure1 = provider.Exposure;
			AssertType<CC037CExposureProvider>(exposure1);
			var exposure2 = provider.Exposure;
			AssertSame("Is cached", exposure1, exposure2);
		}

		public void TestComprehensiveGuarantee()
		{
			AssertEquals("ComprehensiveGuarantee is null", null, GetEmptyProvider().ComprehensiveGuarantee);

			var comprehensiveGuarantee1 = provider.ComprehensiveGuarantee;
			AssertType<CC037CComprehensiveGuaranteeProvider>(comprehensiveGuarantee1);
			var comprehensiveGuarantee2 = provider.ComprehensiveGuarantee;
			AssertSame("Is cached", comprehensiveGuarantee1, comprehensiveGuarantee2);
		}

		public void TestIndividualGuaranteeByGuarantor()
		{
			AssertEquals("IndividualGuaranteeByGuarantor is null", null, GetEmptyProvider().IndividualGuaranteeByGuarantor);

			var individualGuaranteeByGuarantor1 = provider.IndividualGuaranteeByGuarantor;
			AssertType<CC037CIndividualGuaranteeByGuarantorProvider>(individualGuaranteeByGuarantor1);
			var individualGuaranteeByGuarantor2 = provider.IndividualGuaranteeByGuarantor;
			AssertSame("Is cached", individualGuaranteeByGuarantor1, individualGuaranteeByGuarantor2);
		}

		public void TestIndividualGuaranteeVoucher()
		{
			AssertEquals("IndividualGuaranteeVoucher is null", null, GetEmptyProvider().IndividualGuaranteeVoucher);

			var individualGuaranteeVoucher1 = provider.IndividualGuaranteeVoucher;
			AssertType<CC037CIndividualGuaranteeVoucherProvider>(individualGuaranteeVoucher1);
			var individualGuaranteeVoucher2 = provider.IndividualGuaranteeVoucher;
			AssertSame("Is cached", individualGuaranteeVoucher1, individualGuaranteeVoucher2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC037CGuaranteeReferenceProvider(new GuaranteeReferenceType07
			{
				SequenceNumber = "1",
				Grn = "12GRNCC055C012345A678901",
				AcceptanceDateAndTime = new DateTime(2023, 01, 31, 10, 22, 11),
				GuaranteeMonitoringCode = "7",
				GuaranteeQuery = new GuaranteeQueryType
				{
					QueryIdentifier = "1",
					PeriodFromDate = new DateTime(2023, 01, 31, 10, 22, 11),
					PeriodToDate = new DateTime(2023, 02, 28, 9, 8, 7),
				},
				Owner = new OwnerType02
				{
					IdentificationNumber = "IN002",
					Name = "Guarantor 1",
					Address = new AddressType10
					{
						City = "CITY",
						Country = "ES",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					},
				},
				Usage = new Collection<UsageType>
						{
							new UsageType
							{
								SequenceNumber = "1",
								Mrn = "19MRNCC055C0123456",
								CoveredAmount = 10,
								Currency = "QWE",
								LockDate = new DateTime(2023, 02, 21, 1, 2, 7),
								ArrivalDateAndTime = new DateTime(2023, 02, 5, 3, 2, 2),
								ReleaseDate = new DateTime(2023, 02, 19, 2, 3, 6),
							},
							new UsageType
							{
								SequenceNumber = "2",
								Mrn = "20MRNCC055C0123456",
								CoveredAmount = 9,
								Currency = "POI",
								LockDate = new DateTime(2023, 03, 15, 1, 2, 7),
								ArrivalDateAndTime = new DateTime(2023, 02, 15, 4, 3, 3),
								ReleaseDate = new DateTime(2023, 04, 9, 2, 3, 6),
							}
						},
				Exposure = new ExposureType
				{
					Exposure = 7,
					ExposureCounter = "1",
					Balance = 4,
					Currency = "YGV"
				},
				Guarantor = new GuarantorType01
				{
					IdentificationNumber = "LKJ321",
					Name = "Guarantor 1",
					Address = new AddressType13
					{
						City = "CITY",
						Country = CountryCodesCustomsOfficeLists.Ie,
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					},
					ContactPerson = new ContactPersonType01
					{
						Name = "Contact Person 1",
						PhoneNumber = "+123456789",
						EMailAddress = "tes@email.com"
					}
				},
				ComprehensiveGuarantee = new ComprehensiveGuaranteeType
				{
					ReferenceAmount = 7,
					PercentageOfReferenceAmount = "45",
					GuaranteeAmount = 12,
					Currency = "USD",
					NumberOfCertificates = "3",
					ValidityStartDate = new DateTime(2023, 02, 11, 2, 3, 6),
					ValidityEndDate = new DateTime(2023, 02, 15, 2, 4, 6),
					InvalidityReasonCode = "AB8",
					InvalidityReasonText = "Some reason text",
					LiabilityLiberationDate = new DateTime(2023, 05, 11, 2, 4, 6),
					RestrictedUseForSuspendedGoods = Flag.Item1,
					ValidityLimitation = new Collection<ValidityLimitationType>
							{
								new ValidityLimitationType
								{
									SequenceNumber = "1",
									GuaranteeNotValidIn = "AB"
								},
								new ValidityLimitationType
								{
									SequenceNumber = "2",
									GuaranteeNotValidIn = "DE"
								}
							}
				},
				IndividualGuaranteeByGuarantor = new IndividualGuaranteeByGuarantorType
				{
					GuaranteeAmount = 8,
					Currency = "EUR",
					CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
					{
						ReferenceNumber = "FD123TYU",
					},
					CustomsOfficeOfDestination = new CustomsOfficeOfDestinationType02
					{
						ReferenceNumber = "UJ321YHN",
					}
				},
				IndividualGuaranteeVoucher = new IndividualGuaranteeVoucherType
				{
					IssueDate = new DateTime(2023, 01, 24, 2, 4, 6),
					ExpiryDate = new DateTime(2023, 01, 28, 2, 4, 8),
					CopyGiven = Flag.Item1,
					TirCarnet = Flag.Item1,
					VoucherAmount = 999,
					Currency = "AUD"
				}
			});
		}
		CC037CGuaranteeReferenceProvider provider;

		CC037CGuaranteeReferenceProvider GetEmptyProvider()
		{
			return new CC037CGuaranteeReferenceProvider(new GuaranteeReferenceType07
			{
				GuaranteeQuery = new GuaranteeQueryType { },
			});
		}
	}
}
