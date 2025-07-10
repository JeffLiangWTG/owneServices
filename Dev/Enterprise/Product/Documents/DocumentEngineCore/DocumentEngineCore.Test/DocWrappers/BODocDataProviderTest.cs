using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class BODocDataProviderTest : TestCaseWithFactory
	{
		#region TestGetEventLastDatetime

		public void TestGetEventLastDatetime()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			dummy.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2011, 07, 15, 10, 54, 00));
			dummy.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2011, 08, 31, 13, 39, 00));
			dummy.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2011, 01, 02, 18, 00, 00));
			dummy.Logs.AddNew(Events.Departure, new ZDateTimeOffset(2011, 06, 20, 10, 00, 00));
			dummy.Logs.AddNew(Events.Departure, new ZDateTimeOffset(2011, 08, 30, 08, 30, 00));

			AssertEquals(new ZDateTime(2011, 08, 31, 13, 39, 00), BODocDataProvider.GetEventLastDateTime(dummy, Events.ArrivalCode));
			AssertEquals(new ZDateTime(2011, 08, 30, 08, 30, 00), BODocDataProvider.GetEventLastDateTime(dummy, Events.DepartureCode));
			AssertEquals(ZDateTime.Empty, BODocDataProvider.GetEventLastDateTime(dummy, Events.DocumentDeliveredCode));
		}

		public void TestGetEventLastDatetime_WithCancelledAndEstimateEvents()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			var log = dummy.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2011, 07, 15, 10, 54, 00));
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_IsEstimate = true;
			}
			log = dummy.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2011, 08, 31, 13, 39, 00));
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.Cancel();
			}
			dummy.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2011, 01, 02, 18, 00, 00));
			dummy.Logs.AddNew(Events.Departure, new ZDateTimeOffset(2011, 06, 20, 10, 00, 00));
			log = dummy.Logs.AddNew(Events.Departure, new ZDateTimeOffset(2011, 08, 30, 08, 30, 00));
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_IsEstimate = true;
				log.Cancel();
			}

			AssertEquals(new ZDateTime(2011, 01, 02, 18, 00, 00), BODocDataProvider.GetEventLastDateTime(dummy, Events.ArrivalCode));
			AssertEquals(new ZDateTime(2011, 06, 20, 10, 00, 00), BODocDataProvider.GetEventLastDateTime(dummy, Events.DepartureCode));
			AssertEquals(ZDateTime.Empty, BODocDataProvider.GetEventLastDateTime(dummy, Events.DocumentDeliveredCode));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGetEventLastDatetime_WithNullCode()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			var x = BODocDataProvider.GetEventLastDateTime(dummy, null);
			AssertEquals(x, ZDateTime.Empty);
		}

		public void TestGetEventLastDatetime_WithInvalidEventCodes()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			AssertExceptionThrown(typeof(InvalidDocumentWrapperParameterException), "No Event Code was entered.", () => BODocDataProvider.GetEventLastDateTime(dummy, string.Empty));
			AssertExceptionThrown(typeof(InvalidDocumentWrapperParameterException), "Invalid Event Code (XXXX) was entered.", () => BODocDataProvider.GetEventLastDateTime(dummy, "XXXX"));
		}

		#endregion

		public void TestGet()
		{
			AssertEquals(
				"The actual business object is returned when IBODocDataProvider is implemented directly on the object",
				typeof(TestBODocDataProvider), BODocDataProvider.Get(new TestBODocDataProvider()).GetType());
		}

		public void TestGetDefault()
		{
			AssertEquals(
				"A default instance of BODocDataProvider returned",
				typeof(BODocDataProvider), BODocDataProvider.Get(Dummy).GetType());
		}

		public void TestGetBusinessObject()
		{
			TestBODocDataProvider docDataProviderBizo = new TestBODocDataProvider();
			AssertEquals("GetBusinessObject when IBODocDataProvider implemented on bizo", docDataProviderBizo, BODocDataProvider.GetBusinessObject(docDataProviderBizo));
			AssertEquals("GetBusinessObject when default IBODocDataProvider used", Dummy, BODocDataProvider.GetBusinessObject(BODocDataProvider.Get(Dummy)));
		}

		public void TestIsBODocDataProviderOnInterfaces()
		{
			Assert(BODocDataProvider.IsBODocDataProvider(typeof(Integration.Customs.IBaseJobDeclaration)));
			Assert(!BODocDataProvider.IsBODocDataProvider(typeof(DummyInterface)));
		}

		#region Test Classes

		class TestBODocDataProvider : NonPersistentBusinessObject, IBODocDataProvider
		{
			#region IBODocDataProvider Members

			public ZString DocTypeCode
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public DocWrapperCopyInfo AdditionalCopyInfo
			{
				get { throw new NotImplementedException(); }
			}

			public void SetAdditionalCopyInfo(DocWrapperCopyInfo additionalCopyInfo)
			{
				throw new NotImplementedException();
			}

			public BusinessObject BusinessObjectToLogAgainst
			{
				get { throw new NotImplementedException(); }
			}

			public string[] ImageNamesToRemove
			{
				get { throw new NotImplementedException(); }
			}

			public void SetDocWrapperContext(Dictionary<string, object> constants)
			{
				throw new NotImplementedException();
			}

			public BusinessObject ParentBusinessObject
			{
				get { throw new NotImplementedException(); }
			}

			public ZString Format(ZString formatString)
			{
				throw new NotImplementedException();
			}

			public ZString GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue)
			{
				throw new NotImplementedException();
			}

			public IZType GetCustomField(string fieldName, string typeName)
			{
				throw new NotImplementedException();
			}

			public string GetCustomFieldCodeDescription(string fieldName, string typeName)
			{
				throw new NotImplementedException();
			}

			public ZDateTime GetEventLastDateTime(string eventCode)
			{
				throw new NotImplementedException();
			}

			#endregion
		}

		interface DummyInterface { }

		#endregion

		#region Implementation

		DummyEnterpriseBusinessObject Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyEnterpriseBusinessObject>()); }
		}
		DummyEnterpriseBusinessObject dummy;

		#endregion
	}
}
