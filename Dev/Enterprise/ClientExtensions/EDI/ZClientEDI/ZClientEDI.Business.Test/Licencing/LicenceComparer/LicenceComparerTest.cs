using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Licensing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceComparerTest : TestCaseWithFactory
	{
		public void TestAreLicencesTheSame()
		{
			LegacyLicence testLicence1 = new LegacyLicence();
			LegacyLicence testLicence2 = new LegacyLicence();
			testLicence1.Core.LicenceType = LicenceTypes.Codes.PUR;
			testLicence2.Core.LicenceType = LicenceTypes.Codes.PUR;
			Assert("The licences are the same", new LicenceComparer(testLicence1, testLicence2, true).AreLicencesTheSame);

			testLicence2.Core.LicenceType = LicenceTypes.Codes.TRI;
			Assert("The licences are NOT the same (different licence type)", !new LicenceComparer(testLicence1, testLicence2, true).AreLicencesTheSame);
		}

		public void TestGetDescriptionOfDifferences()
		{
			LegacyLicence testLicence1 = new LegacyLicence();
			testLicence1.Core.LicenceType = LicenceTypes.Codes.PUR;
			testLicence1.Core.UserLimit = 10;
			testLicence1.Core.ExpiryDate = new ZDateTime(2006, 06, 10).ToDateTime();
			testLicence1.RelationshipManager.LicenceType = LicenceTypes.Codes.TRI;
			testLicence1.RelationshipManager.UserLimit = 10;
			testLicence1.RelationshipManager.ExpiryDate = new ZDateTime(2006, 06, 10).ToDateTime();
			testLicence1.RelationshipCompanyTariffsOLD.LicenceType = LicenceTypes.Codes.PUR;  // Core has all fields the same
			testLicence1.RelationshipCompanyTariffsOLD.UserLimit = 10;
			testLicence1.RelationshipCompanyTariffsOLD.ExpiryDate = new ZDateTime(2006, 06, 10).ToDateTime();
			testLicence1.WebTracker.LicenceType = LicenceTypes.Codes.NON;
			testLicence1.WebTracker.UserLimit = 0;
			testLicence1.WebTracker.ExpiryDate = new ZDateTime(2006, 06, 10).ToDateTime();

			LegacyLicence testLicence2 = new LegacyLicence();
			testLicence2.Core.LicenceType = LicenceTypes.Codes.PUR;  // Core has all fields the same
			testLicence2.Core.UserLimit = 10;
			testLicence2.Core.ExpiryDate = new ZDateTime(2006, 06, 10).ToDateTime();
			testLicence2.RelationshipManager.LicenceType = LicenceTypes.Codes.REN; //RelMan has some fields different
			testLicence2.RelationshipManager.UserLimit = 10;
			testLicence2.RelationshipManager.ExpiryDate = DateTime.MinValue;
			testLicence2.RelationshipCompanyTariffsOLD.LicenceType = LicenceTypes.Codes.PUR;  // Core has all fields the same
			testLicence2.RelationshipCompanyTariffsOLD.UserLimit = 100;
			testLicence2.RelationshipCompanyTariffsOLD.ExpiryDate = new ZDateTime(2006, 06, 10).ToDateTime();
			testLicence2.WebTracker.LicenceType = LicenceTypes.Codes.TRI;
			testLicence2.WebTracker.UserLimit = 12;
			testLicence2.WebTracker.ExpiryDate = DateTime.MinValue;

			LicenceComparer comparer = new LicenceComparer(testLicence1, testLicence2, true);
			Assert("PRE: The licences are NOT the same", !comparer.AreLicencesTheSame);

			string expectedResultWithObsoletes = string.Format(
@"MODULE: SalesMarketing
	TYPE:
		CLIENT: TRI
		PROD: REN
	EXPIRY:
		CLIENT: {0}
		PROD: (NONE)

MODULE: SalesMarketing Company Tariffs - OBSOLETE
	USER COUNT:
		CLIENT: 10
		PROD: 100

MODULE: WebTracker
	TYPE:
		CLIENT: NON
		PROD: TRI
	EXPIRY:
		CLIENT: {0}
		PROD: (NONE)
	USER COUNT:
		CLIENT: 0
		PROD: 12

", new DateTime(2006, 06, 10).ToShortDateString());

			string actual = comparer.GetDescriptionOfDifferences();
			AssertEquals("Description should have:\r\nCore not listed(all the same details)\r\nRelationshipManager showing only licence type and expiry date\r\nWebtracker shows all 3 fields\r\nOldCompanyTariffs Shown (Obsoletes visible)", expectedResultWithObsoletes, actual);

			comparer = new LicenceComparer(testLicence1, testLicence2, false);
			string expectedResultWithoutObsoletes = string.Format(
@"MODULE: SalesMarketing
	TYPE:
		CLIENT: TRI
		PROD: REN
	EXPIRY:
		CLIENT: {0}
		PROD: (NONE)

MODULE: WebTracker
	TYPE:
		CLIENT: NON
		PROD: TRI
	EXPIRY:
		CLIENT: {0}
		PROD: (NONE)
	USER COUNT:
		CLIENT: 0
		PROD: 12

", new DateTime(2006, 06, 10).ToShortDateString());

			actual = comparer.GetDescriptionOfDifferences();
			AssertEquals("Description should have:\r\nCore not listed(all the same details)\r\nRelationshipManager showing only licence type and expiry date\r\nWebtracker shows all 3 fields\r\nOldCompanyTariffs NOT Shown (Obsoletes hidden)", expectedResultWithoutObsoletes, actual);
		}
	}
}