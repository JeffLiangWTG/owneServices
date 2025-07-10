using System;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[TestedType(typeof(UpdateJobCO2eStatusFromTEUStatus))]
	sealed class UpdateJobCO2eStatusFromTEUStatusTest : DataTransformationTestCase
	{
		Guid refContainerId = Guid.NewGuid();
		Guid refContainerEmptyTEUId = Guid.NewGuid();
		Guid shipment1JobCO2eId = Guid.NewGuid();
		Guid shipment2JobCO2eId = Guid.NewGuid();
		Guid shipment3JobCO2eId = Guid.NewGuid();
		Guid shipment4JobCO2eId = Guid.NewGuid();
		Guid shipment5JobCO2eId = Guid.NewGuid();
		Guid shipment6JobCO2eId = Guid.NewGuid();
		Guid shipment7JobCO2eId = Guid.NewGuid();
		Guid shipment8JobCO2eId = Guid.NewGuid();
		Guid shipment9JobCO2eId = Guid.NewGuid();
		Guid consol1JobCO2eId = Guid.NewGuid();
		Guid consol2JobCO2eId = Guid.NewGuid();
		Guid consol3JobCO2eId = Guid.NewGuid();
		Guid consol4JobCO2eId = Guid.NewGuid();
		Guid consol5JobCO2eId = Guid.NewGuid();
		Guid ooq1JobCO2eId = Guid.NewGuid();
		Guid ooq2JobCO2eId = Guid.NewGuid();
		Guid ooq3JobCO2eId = Guid.NewGuid();
		Guid bwq1JobCO2eId = Guid.NewGuid();
		Guid bwq2JobCO2eId = Guid.NewGuid();
		Guid bwq3JobCO2eId = Guid.NewGuid();
		Guid bwq4JobCO2eId = Guid.NewGuid();
		Guid bwq5JobCO2eId = Guid.NewGuid();
		Guid bwq6JobCO2eId = Guid.NewGuid();
		Guid quickBooking1JobCO2eId = Guid.NewGuid();
		Guid quickBooking2JobCO2eId = Guid.NewGuid();
		Guid leg1JobCO2eId = Guid.NewGuid();
		Guid leg2JobCO2eId = Guid.NewGuid();
		Guid leg3JobCO2eId = Guid.NewGuid();
		Guid leg4JobCO2eId = Guid.NewGuid();
		Guid leg5JobCO2eId = Guid.NewGuid();
		Guid leg6JobCO2eId = Guid.NewGuid();
		Guid leg7JobCO2eId = Guid.NewGuid();
		Guid leg8JobCO2eId = Guid.NewGuid();

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateJobCO2eStatusFromTEUStatus();
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, JobCO2eSchema.Constants.TableName, "JCO_TEUStatus", "char(3)");

			CreateRefContainer(refContainerId, "11GP", 1, "SEA");
			CreateRefContainer(refContainerEmptyTEUId, "99GP", 0, "SEA");

			CreateShipment1();
			CreateShipment2();
			CreateShipment3();
			CreateShipment4();
			CreateShipment5();
			CreateShipment6();
			CreateShipment7();
			CreateShipment8();
			CreateShipment9();
			CreateConsol1();
			CreateConsol2();
			CreateConsol3();
			CreateConsol4();
			CreateConsol5();
			CreateOneOffQuote1();
			CreateOneOffQuote2();
			CreateOneOffQuote3();
			CreateBookingWithQuote1();
			CreateBookingWithQuote2();
			CreateBookingWithQuote3();
			CreateBookingWithQuote4();
			CreateBookingWithQuote5();
			CreateBookingWithQuote6();
			CreateQuickBooking1();
			CreateQuickBooking2();
			CreateJobCO2e(leg1JobCO2eId, Guid.NewGuid(), "JX", "CUR", "PEN");
			CreateJobCO2e(leg2JobCO2eId, Guid.NewGuid(), "JX", "CUR", "REJ");
			CreateJobCO2e(leg3JobCO2eId, Guid.NewGuid(), "JX", "CUR", "NCU");
			CreateJobCO2e(leg4JobCO2eId, Guid.NewGuid(), "JX", "NCU", "CUR");
			CreateJobCO2e(leg5JobCO2eId, Guid.NewGuid(), "JX", "PEN", "CUR");
			CreateJobCO2e(leg6JobCO2eId, Guid.NewGuid(), "JX", "REJ", "CUR");
			CreateJobCO2e(leg7JobCO2eId, Guid.NewGuid(), "JX", "CUR", "NON");
			CreateJobCO2e(leg8JobCO2eId, Guid.NewGuid(), "JX", "NON", "CUR");
		}

		protected override void AssertTransformationResults()
		{
			AssertStatus(shipment1JobCO2eId, nameof(shipment1JobCO2eId), "CUR");
			AssertStatus(shipment2JobCO2eId, nameof(shipment2JobCO2eId), "CUR");
			AssertStatus(shipment3JobCO2eId, nameof(shipment3JobCO2eId), "NON", "missing a packpivot, not all packlines are all packed");
			AssertStatus(shipment4JobCO2eId, nameof(shipment4JobCO2eId), "NON", "no packline");
			AssertStatus(shipment5JobCO2eId, nameof(shipment5JobCO2eId), "NON", "packline weight is 0");
			AssertStatus(shipment6JobCO2eId, nameof(shipment6JobCO2eId), "NON", "shipment weight is not 0");
			AssertStatus(shipment7JobCO2eId, nameof(shipment7JobCO2eId), "NON", "no consol/container");
			AssertStatus(shipment8JobCO2eId, nameof(shipment8JobCO2eId), "CUR");
			AssertStatus(shipment9JobCO2eId, nameof(shipment9JobCO2eId), "NON", "missing a packpivot, not all packlines are all packed");

			AssertStatus(consol1JobCO2eId, nameof(consol1JobCO2eId), "CUR");
			AssertStatus(consol2JobCO2eId, nameof(consol2JobCO2eId), "CUR");
			AssertStatus(consol3JobCO2eId, nameof(consol3JobCO2eId), "NON", "shipment weight is not 0");
			AssertStatus(consol4JobCO2eId, nameof(consol4JobCO2eId), "NON", "teu is 0");
			AssertStatus(consol5JobCO2eId, nameof(consol5JobCO2eId), "NON", "shipment weight is not 0");

			AssertStatus(ooq1JobCO2eId, nameof(ooq1JobCO2eId), "CUR");
			AssertStatus(ooq2JobCO2eId, nameof(ooq2JobCO2eId), "NON", "teu is 0");
			AssertStatus(ooq3JobCO2eId, nameof(ooq3JobCO2eId), "CUR");

			AssertStatus(bwq1JobCO2eId, nameof(bwq1JobCO2eId), "CUR");
			AssertStatus(bwq2JobCO2eId, nameof(bwq2JobCO2eId), "CUR");
			AssertStatus(bwq3JobCO2eId, nameof(bwq3JobCO2eId), "NON", "teu is 0");
			AssertStatus(bwq4JobCO2eId, nameof(bwq4JobCO2eId), "NON", "weight is not 0");
			AssertStatus(bwq5JobCO2eId, nameof(bwq5JobCO2eId), "NON", "no container");
			AssertStatus(bwq6JobCO2eId, nameof(bwq6JobCO2eId), "NON", "no container");

			AssertStatus(quickBooking1JobCO2eId, nameof(quickBooking1JobCO2eId), "CUR");
			AssertStatus(quickBooking2JobCO2eId, nameof(quickBooking2JobCO2eId), "CUR");

			AssertStatus(leg1JobCO2eId, nameof(leg1JobCO2eId), "NCU");
			AssertStatus(leg2JobCO2eId, nameof(leg2JobCO2eId), "NCU");
			AssertStatus(leg3JobCO2eId, nameof(leg3JobCO2eId), "NCU");
			AssertStatus(leg4JobCO2eId, nameof(leg4JobCO2eId), "NCU");
			AssertStatus(leg5JobCO2eId, nameof(leg5JobCO2eId), "NCU");
			AssertStatus(leg6JobCO2eId, nameof(leg6JobCO2eId), "NCU");
			AssertStatus(leg7JobCO2eId, nameof(leg7JobCO2eId), "CUR");
			AssertStatus(leg8JobCO2eId, nameof(leg8JobCO2eId), "CUR");
		}

		void CreateShipment1()
		{
			var shipment1Id = Guid.NewGuid();
			var consol1Id = Guid.NewGuid();
			var consol2Id = Guid.NewGuid();
			var container1Id = Guid.NewGuid();
			var container2Id = Guid.NewGuid();
			var packLineId = Guid.NewGuid();
			CreateShipment(shipment1Id, "SEA", "FCL", 100);
			CreateConsol(consol1Id, "FCL", "SEA", 50);
			CreateContainer(container1Id, refContainerId, consol1Id);
			CreateConsol(consol2Id, "FCL", "SEA", 50);
			CreateContainer(container2Id, refContainerId, consol2Id);
			CreateJobConShipLink(consol1Id, shipment1Id);
			CreateJobConShipLink(consol2Id, shipment1Id);
			CreatePackLine(packLineId, shipment1Id, 100);
			CreatePackPivot(container1Id, packLineId);
			CreatePackPivot(container2Id, packLineId);
			CreateJobCO2e(shipment1JobCO2eId, shipment1Id, "JS", "NON", "CUR");
		}

		void CreateShipment2()
		{
			var shipment1Id = Guid.NewGuid();
			var consol1Id = Guid.NewGuid();
			var container1Id = Guid.NewGuid();
			CreateShipment(shipment1Id, "ROA", "FCL", 0);
			CreateConsol(consol1Id, "FCL", "ROA", 0);
			CreateJobConShipLink(consol1Id, shipment1Id);
			CreateContainer(container1Id, refContainerId, consol1Id);
			CreateJobCO2e(shipment2JobCO2eId, shipment1Id, "JS", "NON", "CUR");
		}

		void CreateShipment3()
		{
			var shipment1Id = Guid.NewGuid();
			var consol1Id = Guid.NewGuid();
			var consol2Id = Guid.NewGuid();
			var container1Id = Guid.NewGuid();
			var container2Id = Guid.NewGuid();
			var packLineId = Guid.NewGuid();
			CreateShipment(shipment1Id, "SEA", "FCL", 100);
			CreateConsol(consol1Id, "FCL", "SEA", 50);
			CreateContainer(container1Id, refContainerId, consol1Id);
			CreateConsol(consol2Id, "FCL", "SEA", 50);
			CreateContainer(container2Id, refContainerId, consol2Id);
			CreateJobConShipLink(consol1Id, shipment1Id);
			CreateJobConShipLink(consol2Id, shipment1Id);
			CreatePackLine(packLineId, shipment1Id, 100);
			CreatePackPivot(container1Id, packLineId);
			CreateJobCO2e(shipment3JobCO2eId, shipment1Id, "JS", "NON", "CUR");
		}

		void CreateShipment4()
		{
			var shipment1Id = Guid.NewGuid();
			var consol1Id = Guid.NewGuid();
			var container1Id = Guid.NewGuid();
			CreateShipment(shipment1Id, "SEA", "FCL", 0);
			CreateConsol(consol1Id, "FCL", "ROA", 0);
			CreateJobConShipLink(consol1Id, shipment1Id);
			CreateContainer(container1Id, refContainerId, consol1Id);
			CreateJobCO2e(shipment4JobCO2eId, shipment1Id, "JS", "NON", "CUR");
		}

		void CreateShipment5()
		{
			var shipment1Id = Guid.NewGuid();
			var consol1Id = Guid.NewGuid();
			var container1Id = Guid.NewGuid();
			var packLineId = Guid.NewGuid();
			CreateShipment(shipment1Id, "SEA", "FCL", 100);
			CreateConsol(consol1Id, "FCL", "SEA", 50);
			CreateContainer(container1Id, refContainerId, consol1Id);
			CreateJobConShipLink(consol1Id, shipment1Id);
			CreatePackLine(packLineId, shipment1Id, 0);
			CreatePackPivot(container1Id, packLineId);
			CreateJobCO2e(shipment5JobCO2eId, shipment1Id, "JS", "NON", "CUR");
		}

		void CreateShipment6()
		{
			var shipment1Id = Guid.NewGuid();
			var consol1Id = Guid.NewGuid();
			var container1Id = Guid.NewGuid();
			CreateShipment(shipment1Id, "ROA", "FCL", 100);
			CreateConsol(consol1Id, "FCL", "ROA", 0);
			CreateJobConShipLink(consol1Id, shipment1Id);
			CreateContainer(container1Id, refContainerId, consol1Id);
			CreateJobCO2e(shipment6JobCO2eId, shipment1Id, "JS", "NON", "CUR");
		}

		void CreateShipment7()
		{
			var shipment1Id = Guid.NewGuid();
			CreateShipment(shipment1Id, "SEA", "FCL", 100);
			CreateJobCO2e(shipment7JobCO2eId, shipment1Id, "JS", "NON", "CUR");
		}

		void CreateShipment8()
		{
			var shipment1Id = Guid.NewGuid();
			var consol1Id = Guid.NewGuid();
			var container1Id = Guid.NewGuid();
			var container2Id = Guid.NewGuid();
			var packLineId = Guid.NewGuid();
			CreateShipment(shipment1Id, "SEA", "FCL", 100);
			CreateConsol(consol1Id, "FCL", "SEA", 50);
			CreateContainer(container1Id, refContainerId, consol1Id);
			CreateContainer(container2Id, refContainerId, consol1Id);
			CreateJobConShipLink(consol1Id, shipment1Id);
			CreatePackLine(packLineId, shipment1Id, 100);
			CreatePackPivot(container1Id, packLineId);
			CreateJobCO2e(shipment8JobCO2eId, shipment1Id, "JS", "NON", "CUR");
		}

		void CreateShipment9()
		{
			var shipment1Id = Guid.NewGuid();
			var consol1Id = Guid.NewGuid();
			var container1Id = Guid.NewGuid();
			var container2Id = Guid.NewGuid();
			var consol2Id = Guid.NewGuid();
			var container3Id = Guid.NewGuid();
			var container4Id = Guid.NewGuid();
			var packLine1Id = Guid.NewGuid();
			var packLine2Id = Guid.NewGuid();
			CreateShipment(shipment1Id, "SEA", "FCL", 100);
			CreateConsol(consol1Id, "FCL", "SEA", 50);
			CreateContainer(container1Id, refContainerId, consol1Id);
			CreateContainer(container2Id, refContainerId, consol1Id);
			CreateJobConShipLink(consol1Id, shipment1Id);
			CreateConsol(consol2Id, "FCL", "SEA", 50);
			CreateContainer(container3Id, refContainerId, consol2Id);
			CreateContainer(container4Id, refContainerId, consol2Id);
			CreateJobConShipLink(consol2Id, shipment1Id);
			CreatePackLine(packLine1Id, shipment1Id, 100);
			CreatePackLine(packLine2Id, shipment1Id, 100);
			CreatePackPivot(container1Id, packLine1Id);
			CreatePackPivot(container2Id, packLine2Id);
			CreatePackPivot(container3Id, packLine1Id);
			CreateJobCO2e(shipment9JobCO2eId, shipment1Id, "JS", "NON", "CUR");
		}

		void CreateConsol1()
		{
			var consol1Id = Guid.NewGuid();
			var container1Id = Guid.NewGuid();
			CreateConsol(consol1Id, "FCL", "SEA", 0);
			CreateContainer(container1Id, refContainerId, consol1Id);
			CreateJobCO2e(consol1JobCO2eId, consol1Id, "JK", "NON", "CUR");
		}

		void CreateConsol2()
		{
			var consol1Id = Guid.NewGuid();
			var shipment1Id = Guid.NewGuid();
			var container1Id = Guid.NewGuid();
			CreateConsol(consol1Id, "FCL", "ROA", 0);
			CreateContainer(container1Id, refContainerId, consol1Id);
			CreateShipment(shipment1Id, "ROA", "FCL", 0);
			CreateJobConShipLink(consol1Id, shipment1Id);
			CreateJobCO2e(consol2JobCO2eId, consol1Id, "JK", "NON", "CUR");
		}

		void CreateConsol3()
		{
			var consol1Id = Guid.NewGuid();
			var shipment1Id = Guid.NewGuid();
			var container1Id = Guid.NewGuid();
			CreateConsol(consol1Id, "FCL", "ROA", 0);
			CreateContainer(container1Id, refContainerId, consol1Id);
			CreateShipment(shipment1Id, "ROA", "FCL", 100);
			CreateJobConShipLink(consol1Id, shipment1Id);
			CreateJobCO2e(consol3JobCO2eId, consol1Id, "JK", "NON", "CUR");
		}

		void CreateConsol4()
		{
			var consol1Id = Guid.NewGuid();
			var container1Id = Guid.NewGuid();
			CreateConsol(consol1Id, "FCL", "SEA", 1000);
			CreateContainer(container1Id, refContainerEmptyTEUId, consol1Id);
			CreateJobCO2e(consol4JobCO2eId, consol1Id, "JK", "NON", "CUR");
		}

		void CreateConsol5()
		{
			var consol1Id = Guid.NewGuid();
			var shipment1Id = Guid.NewGuid();
			var shipment2Id = Guid.NewGuid();
			var container1Id = Guid.NewGuid();
			CreateConsol(consol1Id, "FCL", "ROA", 0);
			CreateContainer(container1Id, refContainerId, consol1Id);
			CreateShipment(shipment1Id, "ROA", "FCL", 0);
			CreateShipment(shipment2Id, "ROA", "FCL", 50);
			CreateJobConShipLink(consol1Id, shipment1Id);
			CreateJobConShipLink(consol1Id, shipment2Id);
			CreateJobCO2e(consol5JobCO2eId, consol1Id, "JK", "NON", "CUR");
		}

		void CreateOneOffQuote1()
		{
			var ratingHeaderId = Guid.NewGuid();
			var rateOneOffShipmentId = Guid.NewGuid();
			CreateRatingHeader(ratingHeaderId, "ABC");
			CreateRateOneOffShipment(rateOneOffShipmentId, ratingHeaderId, "FCL", "SEA", 100);
			CreateRateOneOffContainer(Guid.NewGuid(), rateOneOffShipmentId, refContainerId, 1);
			CreateJobCO2e(ooq1JobCO2eId, ratingHeaderId, "VB", "NON", "CUR");
		}

		void CreateOneOffQuote2()
		{
			var ratingHeaderId = Guid.NewGuid();
			var rateOneOffShipmentId = Guid.NewGuid();
			CreateRatingHeader(ratingHeaderId, "BCD");
			CreateRateOneOffShipment(rateOneOffShipmentId, ratingHeaderId, "FCL", "SEA", 100);
			CreateRateOneOffContainer(Guid.NewGuid(), rateOneOffShipmentId, refContainerEmptyTEUId, 1);
			CreateJobCO2e(ooq2JobCO2eId, ratingHeaderId, "VB", "NON", "CUR");
		}

		void CreateOneOffQuote3()
		{
			var ratingHeaderId = Guid.NewGuid();
			var rateOneOffShipmentId = Guid.NewGuid();
			CreateRatingHeader(ratingHeaderId, "CDE");
			CreateRateOneOffShipment(rateOneOffShipmentId, ratingHeaderId, "FCL", "ROA", 0);
			CreateRateOneOffContainer(Guid.NewGuid(), rateOneOffShipmentId, refContainerId, 1);
			CreateJobCO2e(ooq3JobCO2eId, ratingHeaderId, "VB", "NON", "CUR");
		}

		void CreateBookingWithQuote1()
		{
			var ratingHeaderId = Guid.NewGuid();
			var bookingId = Guid.NewGuid();
			CreateRatingHeader(ratingHeaderId, "DEF");
			CreateShipment(bookingId, "SEA", "FCL", 100, true, false, ratingHeaderId);
			CreateContainer(Guid.NewGuid(), refContainerId, Guid.Empty, bookingId);
			CreateJobCO2e(bwq1JobCO2eId, ratingHeaderId, "VB", "NON", "CUR");
		}

		void CreateBookingWithQuote2()
		{
			var ratingHeaderId = Guid.NewGuid();
			var bookingId = Guid.NewGuid();
			CreateRatingHeader(ratingHeaderId, "GHI");
			CreateShipment(bookingId, "ROA", "FCL", 0, true, false, ratingHeaderId);
			CreateContainer(Guid.NewGuid(), refContainerId, Guid.Empty, bookingId);
			CreateJobCO2e(bwq2JobCO2eId, ratingHeaderId, "VB", "NON", "CUR");
		}

		void CreateBookingWithQuote3()
		{
			var ratingHeaderId = Guid.NewGuid();
			var bookingId = Guid.NewGuid();
			CreateRatingHeader(ratingHeaderId, "QWE");
			CreateShipment(bookingId, "SEA", "FCL", 100, true, false, ratingHeaderId);
			CreateContainer(Guid.NewGuid(), refContainerEmptyTEUId, Guid.Empty, bookingId);
			CreateJobCO2e(bwq3JobCO2eId, ratingHeaderId, "VB", "NON", "CUR");
		}

		void CreateBookingWithQuote4()
		{
			var ratingHeaderId = Guid.NewGuid();
			var bookingId = Guid.NewGuid();
			CreateRatingHeader(ratingHeaderId, "ASD");
			CreateShipment(bookingId, "RAI", "FCL", 100, true, false, ratingHeaderId);
			CreateContainer(Guid.NewGuid(), refContainerId, Guid.Empty, bookingId);
			CreateJobCO2e(bwq4JobCO2eId, ratingHeaderId, "VB", "NON", "CUR");
		}

		void CreateBookingWithQuote5()
		{
			var ratingHeaderId = Guid.NewGuid();
			var bookingId = Guid.NewGuid();
			CreateRatingHeader(ratingHeaderId, "RTY");
			CreateShipment(bookingId, "SEA", "FCL", 100, true, false, ratingHeaderId);
			CreateJobCO2e(bwq5JobCO2eId, ratingHeaderId, "VB", "NON", "CUR");
		}

		void CreateBookingWithQuote6()
		{
			var ratingHeaderId = Guid.NewGuid();
			var bookingId = Guid.NewGuid();
			CreateRatingHeader(ratingHeaderId, "UIO");
			CreateShipment(bookingId, "ROA", "FCL", 0, true, false, ratingHeaderId);
			CreateJobCO2e(bwq6JobCO2eId, ratingHeaderId, "VB", "NON", "CUR");
		}

		void CreateQuickBooking1()
		{
			var bookingId = Guid.NewGuid();
			CreateShipment(bookingId, "SEA", "FCL", 100, isBooking: true, isForwardRegistered: false);
			CreateContainer(Guid.NewGuid(), refContainerId, Guid.Empty, bookingId);
			CreateJobCO2e(quickBooking1JobCO2eId, bookingId, "VB", "NON", "CUR");
		}

		void CreateQuickBooking2()
		{
			var bookingId = Guid.NewGuid();
			CreateShipment(bookingId, "ROA", "FCL", 0, isBooking: true, isForwardRegistered: false);
			CreateContainer(Guid.NewGuid(), refContainerId, Guid.Empty, bookingId);
			CreateJobCO2e(quickBooking2JobCO2eId, bookingId, "VB", "NON", "CUR");
		}

		void CreateRefContainer(Guid id, string code, int teu, string shippingMode)
		{
			TestConnection.ExecuteNonQuery($@"
INSERT INTO [RefContainer] (RC_PK, RC_Code, RC_TEU, RC_ShippingMode, RC_SystemCreateTimeUtc, RC_SystemCreateUser, RC_SystemLastEditTimeUtc, RC_SystemLastEditUser)
VALUES ('{id}', '{code}', {teu}, '{shippingMode}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
");
		}

		int shipmentCounter = 1;

		void CreateShipment(Guid id, string transportMode, string packingMode, int actualWeight, bool isBooking = false, bool isForwardRegistered = true, Guid quote = default)
		{
			var uniqueConsignRef = $"S{shipmentCounter:D7}";
			shipmentCounter++;
			var quoteValue = quote == default ? "NULL" : $"'{quote}'";
			TestConnection.ExecuteNonQuery($@"
INSERT INTO [JobShipment] (JS_PK, JS_UniqueConsignRef, JS_IsForwardRegistered, JS_IsBooking, JS_TransportMode, JS_PackingMode, JS_ActualWeight, JS_TH_OneTimeQuote, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser)
VALUES ('{id}', '{uniqueConsignRef}', {(isForwardRegistered ? 1 : 0)}, {(isBooking ? 1 : 0)}, '{transportMode}', '{packingMode}', {actualWeight}, {quoteValue}, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
");
		}

		int consolCounter = 1;

		void CreateConsol(Guid id, string consolMode, string transportMode, int totalShipmentActWeightCheck)
		{
			string uniqueConsignRef = $"C{consolCounter:D7}";
			consolCounter++;
			TestConnection.ExecuteNonQuery($@"
INSERT INTO [JobConsol] (JK_PK, JK_UniqueConsignRef, JK_ConsolMode, JK_TransportMode, JK_TotalShipmentActWeightCheck, JK_SystemCreateTimeUtc, JK_SystemCreateUser, JK_SystemLastEditTimeUtc, JK_SystemLastEditUser)
VALUES ('{id}', '{uniqueConsignRef}', '{consolMode}', '{transportMode}', {totalShipmentActWeightCheck}, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
");
		}

		void CreateContainer(Guid id, Guid refContainerId, Guid consolId, Guid booking = default)
		{
			var bookingValue = booking == default ? "NULL" : $"'{booking}'";
			var consolValue = consolId == default ? "NULL" : $"'{consolId}'";
			TestConnection.ExecuteNonQuery($@"
INSERT INTO [JobContainer] (JC_PK, JC_RC, JC_JK, JC_JS_FCLBookingOnlyLink, JC_SystemCreateTimeUtc, JC_SystemCreateUser, JC_SystemLastEditTimeUtc, JC_SystemLastEditUser)
VALUES ('{id}', '{refContainerId}', {consolValue}, {bookingValue}, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
");
		}

		void CreateJobConShipLink(Guid consolId, Guid shipmentId)
		{
			TestConnection.ExecuteNonQuery($@"
INSERT INTO [JobConShipLink] (JN_PK, JN_JK, JN_JS, JN_SystemCreateTimeUtc, JN_SystemCreateUser, JN_SystemLastEditTimeUtc, JN_SystemLastEditUser)
VALUES (NEWID(), '{consolId}', '{shipmentId}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
");
		}

		void CreatePackLine(Guid id, Guid shipmentId, int actualWeight)
		{
			TestConnection.ExecuteNonQuery($@"
INSERT INTO [JobPackLines] (JL_PK, JL_JS, JL_ActualWeight, JL_SystemCreateTimeUtc, JL_SystemCreateUser, JL_SystemLastEditTimeUtc, JL_SystemLastEditUser)
VALUES ('{id}', '{shipmentId}', {actualWeight}, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
");
		}

		void CreatePackPivot(Guid containerId, Guid packLineId)
		{
			TestConnection.ExecuteNonQuery($@"
INSERT INTO [JobContainerPackPivot] (J6_PK, J6_JC, J6_JL, J6_SystemCreateTimeUtc, J6_SystemCreateUser, J6_SystemLastEditTimeUtc, J6_SystemLastEditUser)
VALUES (NEWID(), '{containerId}', '{packLineId}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
");
		}

		readonly TransformationTestDataCreator helper = new TransformationTestDataCreator();

		void CreateRatingHeader(Guid id, string org)
		{
			var orgId = helper.CreateOrg(org);
			TestConnection.ExecuteNonQuery($@"
INSERT INTO [RatingHeader] (TH_PK, TH_OH, TH_RateType, TH_QuoteDate, TH_SystemCreateTimeUtc, TH_SystemCreateUser, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser)
VALUES ('{id}', '{orgId}', 'QTE', GetUtcDate(), GetUtcDate(), '~BP', GetUtcDate(), '~BP');
");
		}

		void CreateRateOneOffShipment(Guid id, Guid ratingHeaderId, string containerMode, string transportMode, int actualWeight)
		{
			TestConnection.ExecuteNonQuery($@"
INSERT INTO [RateOneOffShipment] (TT_PK, TT_TH, TT_ContainerMode, TT_TransportMode, TT_ActualWeight, TT_SystemCreateTimeUtc, TT_SystemCreateUser, TT_SystemLastEditTimeUtc, TT_SystemLastEditUser)
VALUES ('{id}', '{ratingHeaderId}', '{containerMode}', '{transportMode}', '{actualWeight}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
");
		}

		void CreateRateOneOffContainer(Guid id, Guid rateOneOffShipmentId, Guid refContainerId, int containerCount)
		{
			TestConnection.ExecuteNonQuery($@"
INSERT INTO [RateOneOffContainers] (TC_PK, TC_TT, TC_RC, TC_ContainerCount, TC_SystemCreateTimeUtc, TC_SystemCreateUser, TC_SystemLastEditTimeUtc, TC_SystemLastEditUser)
VALUES ('{id}', '{rateOneOffShipmentId}', '{refContainerId}', '{containerCount}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
");
		}

		void CreateJobCO2e(Guid id, Guid parentId, string parentTableCode, string status, string teuStatus)
		{
			TestConnection.ExecuteNonQuery($@"
INSERT INTO [JobCO2e] (JCO_PK, JCO_ParentID, JCO_ParentTableCode, JCO_Status, JCO_TEUStatus, JCO_SystemCreateTimeUtc, JCO_SystemCreateUser, JCO_SystemLastEditTimeUtc, JCO_SystemLastEditUser)
VALUES ('{id}', '{parentId}', '{parentTableCode}', '{status}', '{teuStatus}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
");
		}

		void AssertStatus(Guid pk, string name, string status, string message = "")
		{
			AssertEquals($"{(string.IsNullOrEmpty(message) ? name : name + ":" + message)}", status, TestConnection.ExecuteScalar<string>($@"
SELECT JCO_Status from JobCO2e WHERE JCO_PK = '{pk}'
"));
		}
	}
}
