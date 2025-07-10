using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	public class OrganisationLineTest : TestCase
	{
		public void TestPhone()
		{
			OrganisationLineTestClass organisationLineTestClass = new OrganisationLineTestClass("US4196AU9639000626              DA15V04FXKC7400000        0000A15V04DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    000                                                                                000           ");
			organisationLineTestClass.fUnformattedPhoneNumber = "963766";
			AssertEquals("963766", organisationLineTestClass.Phone);
			organisationLineTestClass.fUnformattedPhoneNumber = "001161963766";
			AssertEquals("963766", organisationLineTestClass.Phone);
			organisationLineTestClass.fUnformattedPhoneNumber = "00116963766";
			AssertEquals("963766", organisationLineTestClass.Phone);
			organisationLineTestClass.fUnformattedPhoneNumber = "0011963766";
			AssertEquals("963766", organisationLineTestClass.Phone);
			organisationLineTestClass.fUnformattedPhoneNumber = "963700116166";
			AssertEquals("963700116166", organisationLineTestClass.Phone);
			organisationLineTestClass.fUnformattedPhoneNumber = "000000000";
			AssertEquals("", organisationLineTestClass.Phone);
			organisationLineTestClass.fUnformattedPhoneNumber = "0000000001";
			AssertEquals("0000000001", organisationLineTestClass.Phone);
		}

		public void TestFax()
		{
			OrganisationLineTestClass organisationLineTestClass = new OrganisationLineTestClass("US4196AU9639000626              DA15V04FXKC7400000        0000A15V04DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    000                                                                                000           ");
			organisationLineTestClass.fUnformattedFaxNumber = "9763766";
			AssertEquals("9763766", organisationLineTestClass.Fax);
			organisationLineTestClass.fUnformattedFaxNumber = "0011619763766";
			AssertEquals("9763766", organisationLineTestClass.Fax);
			organisationLineTestClass.fUnformattedFaxNumber = "00116963766";
			AssertEquals("963766", organisationLineTestClass.Fax);
			organisationLineTestClass.fUnformattedFaxNumber = "0011963766";
			AssertEquals("963766", organisationLineTestClass.Fax);
			organisationLineTestClass.fUnformattedFaxNumber = "963700116166";
			AssertEquals("963700116166", organisationLineTestClass.Fax);
			organisationLineTestClass.fUnformattedFaxNumber = "000000000";
			AssertEquals("", organisationLineTestClass.Fax);
			organisationLineTestClass.fUnformattedFaxNumber = "0000000001";
			AssertEquals("0000000001", organisationLineTestClass.Fax);
		}

		public void TestAccountNumber()
		{
			OrganisationLineTestClass organisationLineTestClass = new OrganisationLineTestClass("US4196AU9639000626              DA15V04FXKC7400000        0000A15V04DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    000                                                                                000           ");
			organisationLineTestClass.fUnformattedAccountNumber = "0003432";
			AssertEquals("0003432", organisationLineTestClass.AccountNumber);
			organisationLineTestClass.fUnformattedAccountNumber = "00003432";
			AssertEquals("3432", organisationLineTestClass.AccountNumber);
			organisationLineTestClass.fUnformattedAccountNumber = "08SEPA00";
			AssertEquals("08SEPA00", organisationLineTestClass.AccountNumber);
		}

		class OrganisationLineTestClass : _OrganisationLine
		{
			public OrganisationLineTestClass(string value) : base(value)
			{
			}

			public string fUnformattedAccountNumber;
			protected override string UnformattedAccountNumber
			{
				get
				{
					return fUnformattedAccountNumber;
				}
			}

			public override string City
			{
				get
				{
					return null;
				}
			}

			public override string ContactName
			{
				get
				{
					return null;
				}
			}

			public override string Country
			{
				get
				{
					return null;
				}
			}

			public override string Name
			{
				get
				{
					return null;
				}
			}

			public override string PostCode
			{
				get
				{
					return null;
				}
			}

			public override string State
			{
				get
				{
					return null;
				}
			}

			public override string Street1
			{
				get
				{
					return null;
				}
			}

			public override string Street2
			{
				get
				{
					return null;
				}
			}

			public string fUnformattedPhoneNumber;
			protected override string UnformattedPhoneNumber
			{
				get
				{
					return fUnformattedPhoneNumber;
				}
			}

			public string fUnformattedFaxNumber;
			protected override string UnformattedFaxNumber
			{
				get
				{
					return fUnformattedFaxNumber;
				}
			}
		}
	}
}
