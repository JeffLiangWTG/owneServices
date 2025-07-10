using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	public class SimilarIncidentSearchOptionsTest : TestCase
	{
		public void TestRestrictionModeNone()
		{
			// Arrange
			var options = new SimilarIncidentSearchOptions()
			{
				FromTime = null,
				ToTime = null,
			};

			// Act
			var restrictionMode = options.GetRestriction();

			// Assert
			AssertEquals(SimilarIncidentSearchOptionsRestriction.None, restrictionMode);
		}

		public void TestRestrictionModeNoneToUtcNow()
		{
			// Arrange
			var options = new SimilarIncidentSearchOptions()
			{
				FromTime = null,
				ToTime = DateTime.UtcNow,
			};

			// Act
			var restrictionMode = options.GetRestriction();

			// Assert
			AssertEquals(SimilarIncidentSearchOptionsRestriction.None, restrictionMode);
		}

		public void TestRestrictionModeNoneFromLongTimeAgo()
		{
			// Arrange
			var options = new SimilarIncidentSearchOptions()
			{
				FromTime = ZDateTime.BrettsBirthday.ToDateTime(),
				ToTime = null,
			};

			// Act
			var restrictionMode = options.GetRestriction();

			// Assert
			AssertEquals(SimilarIncidentSearchOptionsRestriction.None, restrictionMode);
		}

		public void TestRestrictionModeDateFrom()
		{
			// Arrange
			var options = new SimilarIncidentSearchOptions()
			{
				FromTime = DateTime.UtcNow.AddYears(-3),
				ToTime = null,
			};

			// Act
			var restrictionMode = options.GetRestriction();

			// Assert
			AssertEquals(SimilarIncidentSearchOptionsRestriction.Date, restrictionMode);
		}

		public void TestRestrictionModeDateTo()
		{
			// Arrange
			var options = new SimilarIncidentSearchOptions()
			{
				FromTime = null,
				ToTime = DateTime.UtcNow.AddDays(-7),
			};

			// Act
			var restrictionMode = options.GetRestriction();

			// Assert
			AssertEquals(SimilarIncidentSearchOptionsRestriction.Date, restrictionMode);
		}

		public void TestRestrictionModeDateFullCompany()
		{
			// Arrange
			var options = new SimilarIncidentSearchOptions()
			{
				CompanyGuid = ZGuid.BrettsGuid,
			};

			// Act
			var restrictionMode = options.GetRestriction();

			// Assert
			AssertEquals(SimilarIncidentSearchOptionsRestriction.Full, restrictionMode);
		}

		public void TestRestrictionModeDateFullProduct()
		{
			// Arrange
			var options = new SimilarIncidentSearchOptions()
			{
				Product = "Product",
			};

			// Act
			var restrictionMode = options.GetRestriction();

			// Assert
			AssertEquals(SimilarIncidentSearchOptionsRestriction.Full, restrictionMode);
		}

		public void TestRestrictionModeDateFullProductArea()
		{
			// Arrange
			var options = new SimilarIncidentSearchOptions()
			{
				ProductArea = "ProductArea",
			};

			// Act
			var restrictionMode = options.GetRestriction();

			// Assert
			AssertEquals(SimilarIncidentSearchOptionsRestriction.Full, restrictionMode);
		}

		public void TestRestrictionModeDateFullStatusOpen()
		{
			// Arrange
			var options = new SimilarIncidentSearchOptions()
			{
				IncidentStatus = IncidentStatus.Open,
			};

			// Act
			var restrictionMode = options.GetRestriction();

			// Assert
			AssertEquals(SimilarIncidentSearchOptionsRestriction.Full, restrictionMode);
		}

		public void TestRestrictionModeDateFullStatusClosed()
		{
			// Arrange
			var options = new SimilarIncidentSearchOptions()
			{
				IncidentStatus = IncidentStatus.Closed,
			};

			// Act
			var restrictionMode = options.GetRestriction();

			// Assert
			AssertEquals(SimilarIncidentSearchOptionsRestriction.Full, restrictionMode);
		}
	}
}
