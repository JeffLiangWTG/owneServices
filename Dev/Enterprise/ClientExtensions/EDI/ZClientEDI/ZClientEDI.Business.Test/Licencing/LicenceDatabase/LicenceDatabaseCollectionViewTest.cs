using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceDatabaseCollectionView))]
	public class LicenceDatabaseCollectionViewTest : BusinessObjectCollectionViewTestCase<LicenceDatabaseCollectionView>
	{
		protected override LicenceDatabaseCollectionView GetCollectionToTest()
		{
			return new LicenceDatabaseCollectionView(new LicenceDatabaseNonDependentCollection(Factory));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<LicenceDatabase>();
		}
	}

	[TestedType(typeof(ActiveOrAllLicenceDatabaseCollection))]
	public class ActiveOrAllLicenceDatabaseCollectionTest : BusinessObjectCollectionViewTestCase<ActiveOrAllLicenceDatabaseCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(ActiveOrAllLicenceDatabaseCollection);
		}

		protected override ActiveOrAllLicenceDatabaseCollection GetCollectionToTest()
		{
			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			LicenceCompany licCompany = enterprise.Companies.AddNew();
			return enterprise.Companies[0].ActiveOrAllLicDatabases;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			LicenceEnterprise licEnterprise = Factory.New<LicenceEnterprise>();
			LicenceDatabaseCollection licDatabases = new LicenceDatabaseCollection(licEnterprise, Factory);

			licDatabases.AddNew();

			return licDatabases[0];
		}
	}
}
