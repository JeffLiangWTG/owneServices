using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class EventDatePropertyAttributeTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var attribute = new EventDatePropertyAttribute(AutoEvents.GateInCode, EstimateActual.Estimate);
			CombineAssertions(() =>
				{
					AssertEquals(AutoEvents.GateInCode, attribute.EventType);
					AssertEquals(EstimateActual.Estimate, attribute.EstimateActual);
					AssertEquals(false, attribute.ShouldOnlyUpdateEmptyDate);
				});

			attribute = new EventDatePropertyAttribute(AutoEvents.GateInCode, EstimateActual.Actual, true);
			CombineAssertions(() =>
			{
				AssertEquals(AutoEvents.GateInCode, attribute.EventType);
				AssertEquals(EstimateActual.Actual, attribute.EstimateActual);
				AssertEquals(true, attribute.ShouldOnlyUpdateEmptyDate);
			});
		}

		public void TestFindEventDateProperty_BusinessObjectIsNull_ThrowException()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				EventDatePropertyAttribute.FindPropertyInfos(null, Events.CargoAvailable, EstimateActual.Actual);
			});
		}

		public void TestFindInheritedProperty_DoNotFindMultiple()
		{
			var bizo = Factory.New<BusinessObjectWithEventLoggedDates>();

			var propertyInfos = EventDatePropertyAttribute.FindPropertyInfos(bizo, Events.CustomisableEvent00, EstimateActual.Actual);
			AssertEquals(1, propertyInfos.Count());
		}

		public void TestFindEventDateProperty()
		{
			var businessObject = Factory.New<BusinessObjectWithEventLoggedDates>();

			var datePropertyInfos = EventDatePropertyAttribute.FindPropertyInfos(businessObject, AutoEvents.CargoAvailable, EstimateActual.Actual);
			AssertEquals("No property found for the given event type", false, datePropertyInfos.Any());

			var datePropertyInfo = EventDatePropertyAttribute.FindPropertyInfos(businessObject, AutoEvents.GateIn, EstimateActual.Estimate).FirstOrDefault();
			AssertEquals("Correct property found", businessObject.EstimatedDateInfo, datePropertyInfo.Property);
			AssertEquals("ShouldOnlyUpdateEmptyDate", false, datePropertyInfo.Attribute.ShouldOnlyUpdateEmptyDate);

			datePropertyInfo = EventDatePropertyAttribute.FindPropertyInfos(businessObject, AutoEvents.GateIn, EstimateActual.Actual).FirstOrDefault();
			AssertEquals("Correct property found", businessObject.ActualDateInfo, datePropertyInfo.Property);
			AssertEquals("ShouldOnlyUpdateEmptyDate", true, datePropertyInfo.Attribute.ShouldOnlyUpdateEmptyDate);
		}

		#region FindPropertyInfos

		public void TestFindPropertyInfos_ForEstimatedDate()
		{
			var businessObject = Factory.New<BusinessObjectWithEventLoggedDates>();
			TestFindPropertyInfos(businessObject, businessObject.EstimatedDateInfo, EstimateActual.Estimate);
		}

		public void TestFindPropertyInfos_ForActualDate()
		{
			var businessObject = Factory.New<BusinessObjectWithEventLoggedDates>();
			TestFindPropertyInfos(businessObject, businessObject.ActualDateInfo, EstimateActual.Actual);
		}

		static void TestFindPropertyInfos(BusinessObjectWithEventLoggedDates businessObject, ZPropertyInfo dateProperty, EstimateActual estimateActual)
		{
			var unrelatedDatePropertyInfos = EventDatePropertyAttribute.FindPropertyInfos(businessObject, AutoEvents.CargoAvailable, estimateActual);
			AssertEquals("Property not found for given event type", false, unrelatedDatePropertyInfos.Any());

			var datePropertyInfos = EventDatePropertyAttribute.FindPropertyInfos(businessObject, AutoEvents.GateIn, estimateActual).ToArray();
			AssertEquals("Both gate in date properties found", 2, datePropertyInfos.Length);

			var datePropertyInfo = datePropertyInfos[0];
			AssertEquals("Correct property found", dateProperty.Name, datePropertyInfo.Property.Name);

			datePropertyInfo = datePropertyInfos[1];
			AssertEquals("Other date found", "OtherDate", datePropertyInfo.Property.Name);
		}

		#endregion

		#region Test Classes

		class BusinessObjectWithEventLoggedDates : DummyEnterpriseBusinessObject
		{
			public BusinessObjectWithEventLoggedDates(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region Number

			[EventDateProperty(AutoEvents.CustomisableEvent00Code, EstimateActual.Actual)]
			[EventDateProperty(AutoEvents.CustomisableEvent00Code, EstimateActual.Actual)]
			public override ZInt Z0_Number
			{
				get { return base.Z0_Number; }
				set { base.Z0_Number = value; }
			}

			#endregion

			#region EstimatedDate

			[EventDateProperty(AutoEvents.GateInCode, EstimateActual.Estimate)]
			[EventDateProperty(AutoEvents.CallBackClientCode, EstimateActual.Estimate)] // decoy
			public ZDateTimeOffset EstimatedDate
			{
				get { return estimatedDate; }
				set
				{
					estimatedDate = value;
					Logs.CreateRecreateOrUpdateEventLog(Events.GateIn, EstimateActual.Estimate, value);
				}
			}
			ZDateTimeOffset estimatedDate;

			public ZPropertyInfo EstimatedDateInfo
			{
				get { return GetZPropertyInfo(nameof(EstimatedDate)); }
			}

			#endregion

			#region ActualDate

			[EventDateProperty(AutoEvents.CallBackClientCode, EstimateActual.Actual)] // decoy
			[EventDateProperty(AutoEvents.GateInCode, EstimateActual.Actual, true)]
			public ZDateTimeOffset ActualDate
			{
				get { return actualDate; }
				set
				{
					actualDate = value;
					Logs.CreateRecreateOrUpdateEventLog(Events.GateIn, EstimateActual.Actual, value);
				}
			}
			ZDateTimeOffset actualDate;

			public ZPropertyInfo ActualDateInfo
			{
				get { return GetZPropertyInfo(nameof(ActualDate)); }
			}

			#endregion

			#region OtherDate

			[EventDateProperty(AutoEvents.GateInCode, EstimateActual.Estimate)]
			[EventDateProperty(AutoEvents.GateInCode, EstimateActual.Actual)]
			public ZDateTime OtherDate { get; set; }

			public ZPropertyInfo OtherDateInfo
			{
				get { return GetZPropertyInfo(nameof(OtherDate)); }
			}

			#endregion
		}

		#endregion
	}
}
