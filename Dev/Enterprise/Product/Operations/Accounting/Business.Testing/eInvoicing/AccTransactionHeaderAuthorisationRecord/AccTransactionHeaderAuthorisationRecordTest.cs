using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.JobInvoicing.Testing.ExchangeRateTest;
using AuthRecordConstants = Enterprise.Accounting.Integration.DataTransferConstants.AccTransactionHeaderAuthorisationRecord;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	[TestedType(typeof(AccTransactionHeaderAuthorisationRecord))]
	public class AccTransactionHeaderAuthorisationRecordTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var record = base.GetNewBusinessObject() as AccTransactionHeaderAuthorisationRecord;
			record.AHF_Counter = "FJI";
			return record;
		}

		public void TestAuthorisationRecordUniqueIndexFailureHandler()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var arInvoice = (ARInvoice)objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "0001", objectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, objectCreator.Debtor, objectCreator.CC1.PK);
			arInvoice.AH_TransactionReference = "REF001";
			Factory.Save();

			var record = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			PopulateProperties(record);
			Factory.Save();

			var duplicateRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			PopulateProperties(duplicateRecord);

			var failureHandler = duplicateRecord.UniqueIndexFailureHandler_ForTestOnly;
			AssertNotNull(failureHandler);

			AssertExceptionThrown("ZSaveException should be thrown", typeof(ZSaveException), () =>
			{
				Factory.Save();
			});

			var notification = new NotificationHandlerForTest();
			failureHandler.NotifyUserAndAttemptToResolve(notification, failureHandler.HandledUniqueIndexNames.Single(x => x == AccTransactionHeaderAuthorisationRecordSchema.Constants.Indexes.NR_UC__AHF_ParentId_AHF_ParentTableCode_AHF_RecordType));
			AssertContains("An authorization record for this transaction already exists in database.", notification.Message);
			AssertContains("Property values of the existing record", notification.Message);
			AssertContains("Property values of the record to be saved", notification.Message);
			Assert("Error should be reported", notification.ReportErrorCount == 1 && notification.ReportInformationCount == 0);

			void PopulateProperties(AccTransactionHeaderAuthorisationRecord authorisationRecord)
			{
				authorisationRecord.AHF_ParentId = arInvoice.PK;
				authorisationRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
				authorisationRecord.AHF_RecordType = AccTransactionHeaderAuthorisationRecordTypes.Fiji;
			}
		}

		public void TestDefaultExpectEmptyAndNullableFieldsNotPlaceholders()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();

			AssertEquals(ZString.Empty, authRecord.AHF_Counter);
			AssertEquals(ZString.Empty, authRecord.AHF_IDType);
			AssertEquals(ZString.Empty, authRecord.AHF_Number);
			AssertEquals(ZDateTimeOffset.Empty, authRecord.AHF_DateTime);
		}

		#region TrySetXXXOnce()

		public void TestSetNumberOnce()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			AssertSetXXXOnceString(authRecord, authRecord.GetType().GetProperty(nameof(AccTransactionHeaderAuthorisationRecord.AHF_Number)), authRecord.TrySetNumberOnce, legacyNullPlaceholderValue: AuthRecordConstants.NullPlaceholderForNVarchar);
		}

		public void TestSetCounterOnce()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			AssertSetXXXOnceString(authRecord, authRecord.GetType().GetProperty(nameof(AccTransactionHeaderAuthorisationRecord.AHF_Counter)), authRecord.TrySetCounterOnce, legacyNullPlaceholderValue: AuthRecordConstants.NullPlaceholderForNVarchar);
		}

		public void TestSetIDTypeOnce()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			AssertSetXXXOnceString3(authRecord, authRecord.GetType().GetProperty(nameof(AccTransactionHeaderAuthorisationRecord.AHF_IDType)), authRecord.TrySetIDTypeOnce, legacyNullPlaceholderValue: AuthRecordConstants.NullPlaceholderForChar3);
		}

		public void TestSetIDNumberOnce()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			AssertSetXXXOnceString(authRecord, authRecord.GetType().GetProperty(nameof(AccTransactionHeaderAuthorisationRecord.AHF_IDNumber)), authRecord.TrySetIDNumberOnce);
		}

		public void TestSetDateTimeOnce()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			AssertSetXXXOnceDateTimeOffset(authRecord, authRecord.GetType().GetProperty(nameof(AccTransactionHeaderAuthorisationRecord.AHF_DateTime)), authRecord.TrySetDateTimeOnce, legacyNullPlaceholderValue: AuthRecordConstants.NullPlaceholderForDateTime);
		}

		public void TestSetVerificationUrlOnce()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			AssertSetXXXOnceString(authRecord, authRecord.GetType().GetProperty(nameof(AccTransactionHeaderAuthorisationRecord.AHF_VerificationUrl)), authRecord.TrySetVerificationUrlOnce);
		}

		public void TestSetPublicKeyOnce()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			AssertSetXXXOnceBlob(authRecord, authRecord.GetType().GetProperty(nameof(AccTransactionHeaderAuthorisationRecord.AHF_PublicKey)), authRecord.TrySetPublicKeyOnce);
		}

		public void TestSetAuthorisationDataOnce()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			AssertSetXXXOnceBlob(authRecord, authRecord.GetType().GetProperty(nameof(AccTransactionHeaderAuthorisationRecord.AHF_AuthorisationData)), authRecord.TrySetAuthorisationDataOnce);
		}

		public void TestSetITransactionHashOnce()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			AssertSetXXXOnceBlob(authRecord, authRecord.GetType().GetProperty(nameof(AccTransactionHeaderAuthorisationRecord.AHF_ITransactionHash)), authRecord.TrySetITransactionHashOnce);
		}

		public void TestSetIssuerCertificateIdentifierOnce()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			AssertSetXXXOnceString(authRecord, authRecord.GetType().GetProperty(nameof(AccTransactionHeaderAuthorisationRecord.AHF_IssuerCertificateIdentifier)), authRecord.TrySetIssuerCertificateIdentifierOnce);
		}

		public void TestSetIssuerAuthorizationDataOnce()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			AssertSetXXXOnceBlob(authRecord, authRecord.GetType().GetProperty(nameof(AccTransactionHeaderAuthorisationRecord.AHF_IssuerAuthorizationData)), authRecord.TrySetIssuerAuthorizationDataOnce);
		}

		public void TestSetDebtorNumberOnce()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			AssertSetXXXOnceString(authRecord, authRecord.GetType().GetProperty(nameof(AccTransactionHeaderAuthorisationRecord.AHF_DebtorNumber)), authRecord.TrySetDebtorNumberOnce);
		}

		public void TestSetPlaceOfIssueOnce()
		{
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			AssertSetXXXOnceString3(authRecord, authRecord.GetType().GetProperty(nameof(AccTransactionHeaderAuthorisationRecord.AHF_PlaceOfIssue)), authRecord.TrySetPlaceOfIssueOnce);
		}

		static void AssertSetXXXOnceString(AccTransactionHeaderAuthorisationRecord authRecord, PropertyInfo property, Func<ZString, bool> trySetterFunc, ZString? legacyNullPlaceholderValue = null)
		{
			property.SetValue(authRecord, ZString.Empty);

			Assert("Can set when field is empty", trySetterFunc("something"));
			AssertEquals("something", property.GetValue(authRecord));

			if (legacyNullPlaceholderValue.HasValue)
			{
				property.SetValue(authRecord, legacyNullPlaceholderValue);

				Assert("Can set when field is legacy null placeholder", trySetterFunc("something else"));
				AssertEquals("something else", property.GetValue(authRecord));
			}

			property.SetValue(authRecord, (ZString)"another_value");

			Assert("Can NOT set when field was previously set", !trySetterFunc("something else"));
			AssertEquals("another_value", property.GetValue(authRecord));

			Assert("Can set when new value is same as current", trySetterFunc("another_value"));
			AssertEquals("another_value", property.GetValue(authRecord));
		}

		static void AssertSetXXXOnceString3(AccTransactionHeaderAuthorisationRecord authRecord, PropertyInfo property, Func<ZString, bool> trySetterFunc, ZString? legacyNullPlaceholderValue = null)
		{
			property.SetValue(authRecord, ZString.Empty);

			Assert("Can set when field is empty", trySetterFunc("ABC"));
			AssertEquals("ABC", property.GetValue(authRecord));

			if (legacyNullPlaceholderValue.HasValue)
			{
				property.SetValue(authRecord, legacyNullPlaceholderValue);

				Assert("Can set when field is legacy null placeholder", trySetterFunc("XYZ"));
				AssertEquals("XYZ", property.GetValue(authRecord));
			}

			property.SetValue(authRecord, (ZString)"QQQ");

			Assert("Can NOT set when field was previously set", !trySetterFunc("GGG"));
			AssertEquals("QQQ", property.GetValue(authRecord));

			Assert("Can set when new value is same as current", trySetterFunc("QQQ"));
			AssertEquals("QQQ", property.GetValue(authRecord));
		}

		static void AssertSetXXXOnceDateTimeOffset(AccTransactionHeaderAuthorisationRecord authRecord, PropertyInfo property, Func<ZDateTimeOffset, bool> trySetterFunc, ZDateTimeOffset? legacyNullPlaceholderValue = null)
		{
			var someValue = ZDateTimeOffset.Today;
			var someOtherValue = ZDateTimeOffset.Today.AddMinutes(22);

			property.SetValue(authRecord, ZDateTimeOffset.Empty);

			Assert("Can set when field is null", trySetterFunc(someValue));
			AssertEquals(someValue, property.GetValue(authRecord));

			if(legacyNullPlaceholderValue.HasValue)
			{
				property.SetValue(authRecord, legacyNullPlaceholderValue);

				Assert("Can set when field is legacy null placeholder", trySetterFunc(someOtherValue));
				AssertEquals(someOtherValue, property.GetValue(authRecord));
			}

			property.SetValue(authRecord, someValue);

			Assert("Can NOT set when field was previously set", !trySetterFunc(someOtherValue));
			AssertEquals(someValue, property.GetValue(authRecord));

			Assert("Can set when new value is same as current", trySetterFunc(someValue));
			AssertEquals(someValue, property.GetValue(authRecord));
		}

		static void AssertSetXXXOnceBlob(AccTransactionHeaderAuthorisationRecord authRecord, PropertyInfo property, Func<ZBlob, bool> trySetterFunc)
		{
			property.SetValue(authRecord, ZBlob.Empty);
			Assert("Can set when field is null", trySetterFunc(ZBlob.FromAscii("something")));
			AssertEquals(ZBlob.FromAscii("something"), property.GetValue(authRecord));

			property.SetValue(authRecord, ZBlob.FromAscii("another_value"));
			Assert("Can NOT set when field was previously set", !trySetterFunc(ZBlob.FromAscii("something else")));
			AssertEquals(ZBlob.FromAscii("another_value"), property.GetValue(authRecord));

			Assert("Can set when new value is same as current", trySetterFunc(ZBlob.FromAscii("another_value")));
			AssertEquals(ZBlob.FromAscii("another_value"), property.GetValue(authRecord));
		}

		#endregion

		public void TestIsAHF_NumberNullOrEmpty_True()
		{
			var invoice = new TestObjectCreator(Factory).CreateInvoice(typeof(ARInvoice));
			var authRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authRecord.AHF_ParentId = invoice.PK;
			authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;

			authRecord.AHF_Number = "";
			Assert(authRecord.IsAHF_NumberNullOrEmpty());

			authRecord.AHF_Number = AuthRecordConstants.NullPlaceholderForNVarchar;
			Assert("Legacy null placeholder strings should be treated as NullOrEmpty", authRecord.IsAHF_NumberNullOrEmpty());
		}

		public void TestIsAHF_NumberNullOrEmpty_False()
		{
			var invoice = new TestObjectCreator(Factory).CreateInvoice(typeof(ARInvoice));
			var authRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authRecord.AHF_ParentId = invoice.PK;
			authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;

			authRecord.AHF_Number = "Number";
			Assert(!authRecord.IsAHF_NumberNullOrEmpty());
		}

		public void TestIsAHF_DateTimeNullOrEmpty_True()
		{
			var invoice = new TestObjectCreator(Factory).CreateInvoice(typeof(ARInvoice));
			var authRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authRecord.AHF_ParentId = invoice.PK;
			authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;

			authRecord.AHF_DateTime = ZDateTimeOffset.Empty;
			Assert(authRecord.IsAHF_DateTimeNullOrEmpty());

			authRecord.AHF_DateTime = AuthRecordConstants.NullPlaceholderForDateTime;
			Assert("Legacy null placeholder datetime strings should be treated as NullOrEmpty", authRecord.IsAHF_DateTimeNullOrEmpty());
		}

		public void TestIsAHF_DateTimeNullOrEmpty_False()
		{
			var invoice = new TestObjectCreator(Factory).CreateInvoice(typeof(ARInvoice));
			var authRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authRecord.AHF_ParentId = invoice.PK;
			authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;

			authRecord.AHF_DateTime = ZDateTimeOffset.Now;
			Assert(!authRecord.IsAHF_NumberNullOrEmpty());
		}
	}
}
