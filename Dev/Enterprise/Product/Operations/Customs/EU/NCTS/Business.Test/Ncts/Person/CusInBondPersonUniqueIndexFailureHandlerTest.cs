using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class CusInBondPersonUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CusInBondPersonUniqueIndexFailureHandler(null));
		}

		public void TestHandledUniqueIndexNames()
		{
			AssertEquals(CusInBondPersonSchema.Constants.Indexes.NR_UX__CP_BH_Header_CP_Type, new CusInBondPersonUniqueIndexFailureHandler(Factory.New<CusInBondPerson>()).HandledUniqueIndexNames.Single());
		}

		public void TestNotifyUserAndAttemptToResolve_NoChanges()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			var cusInBondPerson = CreateCusInBondPerson(header, "AAA", "111111", "111@abc.com");
			Factory.Save();

			var anotherUserFactory = new BusinessObjectFactory();
			var headerInAnotherFactory = anotherUserFactory.Load<NctsHeader>(header.PK);
			var cusInBondPersonInAnotherFactory = CreateCusInBondPerson(headerInAnotherFactory, string.Empty, string.Empty, string.Empty);
			cusInBondPersonInAnotherFactory.HasChanges = false;

			AssertUniqueIndexFailureResolved(anotherUserFactory, header, cusInBondPerson, cusInBondPersonInAnotherFactory, "Original Person Details used", "AAA", "111111", "111@abc.com");
		}

		public void TestNotifyUserAndAttemptToResolve_HasChanges()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			var cusInBondPerson = CreateCusInBondPerson(header, "AAA", "111111", "111@abc.com");
			Factory.Save();

			var anotherUserFactory = new BusinessObjectFactory();
			var headerInAnotherFactory = anotherUserFactory.Load<NctsHeader>(header.PK);
			var cusInBondPersonInAnotherFactory = CreateCusInBondPerson(headerInAnotherFactory, "BBB", "222222", "222@abc.com");
			AssertUniqueIndexFailureResolved(anotherUserFactory, header, cusInBondPerson, cusInBondPersonInAnotherFactory, "Second Person Details used", "BBB", "222222", "222@abc.com");
		}

		CusInBondPerson CreateCusInBondPerson(NctsHeader header, string fullName, string phone, string email)
		{
			var result = header.Factory.New<CusInBondPerson>();
			result.CP_BH_Header = header.PK;
			result.CP_Type = CusInBondPerson.LocationContactType;
			result.CP_FullName = fullName;
			result.CP_Phone = phone;
			result.CP_Email = email;
			return result;
		}

		void AssertUniqueIndexFailureResolved(BusinessObjectFactory anotherUserFactory, NctsHeader originalHeader, CusInBondPerson orginalPerson, CusInBondPerson anotherUserPerson,
			string assertionMessage, string expectedName, string expectedPhone, string expectedEmail)
		{
			CombineAssertions(assertionMessage, () =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				try
				{
					anotherUserFactory.Save();
					Fail("First save should not have succeeded.");
				}
				catch (Exception ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				AssertEquals("Message to notify user",
					$@"While you were working, Goods Location at Departure Contact Info has already been entered on '{originalHeader.HumanReadableName}' by another user ({orginalPerson.CP_SystemLastEditUser} @ {orginalPerson.CP_SystemLastEditTimeUtc.ToSmallDateTimeFloor()}). Your changes have been merged, please review your changes and save again.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("CusInBondPersonInAnotherFactory should be deleted", true, anotherUserPerson.IsDeleted);

				anotherUserFactory.Save();

				var query = new ZQuery(CusInBondPersonSchema.CP_BH_Header, originalHeader.PK);
				query.AddToFilter(CusInBondPersonSchema.CP_Type, orginalPerson.CP_Type);
				var reloadedCusInBondPersonInAnotherFactory = anotherUserFactory.Load<CusInBondPerson>(query).Single();
				AssertEquals("CusInBondPerson.PK", orginalPerson.PK, reloadedCusInBondPersonInAnotherFactory.PK);
				AssertEquals("CP_FullName", expectedName, reloadedCusInBondPersonInAnotherFactory.CP_FullName);
				AssertEquals("CP_Phone", expectedPhone, reloadedCusInBondPersonInAnotherFactory.CP_Phone);
				AssertEquals("CP_Email", expectedEmail, reloadedCusInBondPersonInAnotherFactory.CP_Email);
			});
		}
	}
}
