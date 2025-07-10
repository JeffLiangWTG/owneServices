using System;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace CargoWise.Types.Tests
{
	public class ZGeographyTest : IZTypeTest
	{
		#region IZTypeTest Overrides

		public override void TestEquals()
		{
			base.TestEquals();

			Assert(ZGeography.Equals(ZGeography.Empty, ZGeography.Empty));
			Assert(ZGeography.Equals(ZGeography.CreatePoint(1, 1), ZGeography.CreatePoint(1, 1)));
			Assert(!ZGeography.Equals(ZGeography.CreatePoint(1, 1), ZGeography.CreatePoint(2, 2)));

			Assert(ZGeography.Equals(SqlGeography.STPointFromText(new SqlChars("POINT EMPTY"), 4326), SqlGeography.STPointFromText(new SqlChars("POINT EMPTY"), 4326)));
			Assert(ZGeography.Equals(SqlGeography.STPointFromText(new SqlChars("POINT (1 1)"), 4326), SqlGeography.STPointFromText(new SqlChars("POINT (1 1)"), 4326)));
			Assert(!ZGeography.Equals(SqlGeography.STPointFromText(new SqlChars("POINT (1 1)"), 4326), SqlGeography.STPointFromText(new SqlChars("POINT (2 2)"), 4326)));

			Assert(ZGeography.Equals(SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT EMPTY")), 4326), SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT EMPTY")), 4326)));
			Assert(ZGeography.Equals(SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT (1 1)")), 4326), SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT (1 1)")), 4326)));
			Assert(!ZGeography.Equals(SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT (1 1)")), 4326), SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT (2 2)")), 4326)));

			Assert(ZGeography.Equals(ZGeography.CreatePoint(1, 1), SqlGeography.STPointFromText(new SqlChars("POINT (1 1)"), 4326)));
			Assert(!ZGeography.Equals(ZGeography.CreatePoint(1, 1), SqlGeography.STPointFromText(new SqlChars("POINT (2 2)"), 4326)));

			Assert(ZGeography.Equals(SqlGeography.STPointFromText(new SqlChars("POINT (1 1)"), 4326), SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT (1 1)")), 4326)));
			Assert(!ZGeography.Equals(SqlGeography.STPointFromText(new SqlChars("POINT (1 1)"), 4326), SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT (2 2)")), 4326)));

			Assert(ZGeography.Equals(SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT (1 1)")), 4326), ZGeography.CreatePoint(1, 1)));
			Assert(!ZGeography.Equals(SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT (1 1)")), 4326), ZGeography.CreatePoint(2, 2)));
		}

		public override void TestCompareTo()
		{
			foreach (object value in AllValues)
			{
				bool wasThrown = false;

				try
				{
					NewZ(value).CompareTo(value);
				}
				catch (NotSupportedException)
				{
					wasThrown = true;
				}

				Assert(MessageForValue(value), wasThrown);
			}
		}

		public override void TestToString()
		{
			foreach (object value in ValidValues)
			{
				if (value != null)
				{
					if (value is SqlGeography)
					{
						AssertEquals(MessageForValue(value), ((SqlGeography)value).AsTextZM().ToSqlString().ToString(), NewZ(value).ToString()); //AsTextZM is like STAsText but also includes elevation
					}
				}
			}
		}

		protected override IZType NewZ(object value)
		{
			return new ZGeography(value);
		}

		protected override object[] ValidValues { get; } = new object[]
		{
			SqlGeography.Parse("POINT (-122 47)"),
			SqlGeography.STPointFromText(new SqlChars("POINT (-122 47)"), ZGeography.SridGps),
			new ZGeography("POINT (-122 47)"),
			"-122 47",
			"-122,47",
			"-122.3 47.6",
			"-122.3,47.6",
			new ZString("POINT (-122 47)"),
			"POINT (-122 47)",
			new ZGeography("POINT (-122 47)").AsBinary(),
			new ZBlob(new ZGeography("POINT (-122 47)").AsBinary())
		};

		protected override object[] InvalidValues { get; } = new object[]
		{
			ZGeography.Invalid,
		};

		protected override object[] EmptyValues { get; } = new object[]
		{
			string.Empty,
			ZGeography.Empty,
			SqlGeography.Parse("POINT EMPTY"),
			"POINT EMPTY",
			SqlGeography.Parse("POINT EMPTY").Serialize(),
			new ZBlob(SqlGeography.Parse("POINT EMPTY").Serialize().Buffer),
			SqlGeography.Parse("POLYGON EMPTY"),
			"POLYGON EMPTY",
			SqlGeography.Parse("POLYGON EMPTY").Serialize().Buffer,
			new ZBlob(SqlGeography.Parse("POLYGON EMPTY").Serialize().Buffer),
		};

		protected override object[] UnsupportedValues { get; } = new object[]
		{
			new object(),
			0.0,
			0M,
			"POINT()",
			ZDateTime.Empty,
			true,
			false
		};

		ZGeography[] ZGeographyTypes { get; } = new ZGeography[] {
			ZGeography.Invalid,
			ZGeography.Empty,
			ZGeography.CreatePoint(151.1928512, -33.916385)
		};

		#endregion

		#region Properties

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestEmptyProperties()
		{
			Assert("Empty should be valid", ZGeography.Empty.IsValid);
			Assert("Empty should be isEmpty", ZGeography.Empty.IsEmpty);
		}

		public void TestInvalidProperties()
		{
			Assert("Invalid should be invalid", !ZGeography.Invalid.IsValid);
			Assert("Invalid should not be empty", !ZGeography.Invalid.IsEmpty);
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestValueHasCorrectCoordinateSystem()
		{
			foreach (object value in AllValues)
			{
				var instance = NewZ(value);
				AssertEquals(MessageForValue(value), ValueIsValid(value), instance.IsValid);
				if (instance.IsValid)
				{
					AssertEquals(ZGeography.SridGps, ((ZGeography)instance).CoordinateSystemId);
				}
			}
		}

		public void TestZGeographyEquality()
		{
			for (int i = 0; i < ZGeographyTypes.Length; i++)
			{
				for (int j = 0; j < ZGeographyTypes.Length; j++)
				{
					if (i == j)
					{
						AssertEquals(ZGeographyTypes[i], ZGeographyTypes[j]);
					}
					else
					{
						AssertNotEquals(ZGeographyTypes[i], ZGeographyTypes[j]);
					}
				}
			}
		}

		public void TestZGeographyThrowExceptionWhenCreatingWithDBNull()
		{
			AssertExceptionThrown<ZTypeValueException>(() =>
			{
				new ZGeography(DBNull.Value);
			});
		}

		public void TestZGeographyAsSerialisation()
		{
			foreach (var value in ZGeographyTypes)
			{
				ThrowWhenInvalid(value, x => x.AsBinary());
				ThrowWhenInvalid(value, x => x.AsText());
			}
		}

		void ThrowWhenInvalid<T>(ZGeography geography, Func<ZGeography, T> func)
		{
			try
			{
				AssertEquals(geography, new ZGeography(func(geography)));
			}
			catch (OperationOnInvalidZGeographyException e)
			{
				if (geography.IsValid)
				{
					Assert($"A Valid Geography {{{geography}}} is not meant to throw {e}", false);
				}
			}
		}

		protected override bool ValueIsValid(object value)
		{
			try
			{
				if (value is IZType)
				{
					//Clean the constructor
					return ((IZType)value).IsValid;
				}
				return ArrayContainsValue(ValidValues, value) || ArrayContainsValue(EmptyValues, value);
			}
			catch (Exception)
			{
				return false;
			}
		}

		public override void TestGetValue()
		{
			foreach (object value in AllValues)
			{
				string message = MessageForValue(value) + " ";
				IZType z = NewZ(value);

				object actualFalse = ((IZTypeInternals)z).GetValueForLogicalDataLayer(false);
				object actualTrue = ((IZTypeInternals)z).GetValueForLogicalDataLayer(true);

				if (!z.IsValid && z.IsEmpty)
				{
					AssertEquals(message + "GetValue(false) type when invalid", UsualValueType, actualFalse.GetType());
					AssertEquals(message + "GetValue(true) when invalid", DBNull.Value, actualTrue);
				}
				else if (ValueIsUsualType(value))
				{
					Assert(message + "GetValue(false) when not invalid", IsEqual(value, actualFalse));
					Assert(message + "GetValue(true) when not invalid", IsEqual(value, actualTrue));
				}
				else
				{
					Assert(message + " when it is a valid parse", IsEqual(actualFalse, actualTrue));
				}
			}
		}

		bool IsEqual(object lhs, object rhs)
		{
			if (lhs.GetType() == typeof(SqlGeography) && rhs.GetType() == typeof(SqlGeography))
			{
				return ((SqlGeography)lhs).STEquals((SqlGeography)rhs).Value;
			}
			return lhs.Equals(rhs);
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestCreatePolygon_CoordinateText_CorrectInput()
		{
			var csLongLat = "1 1,3 3,1 3,1 1";
			var polygon = ZGeography.CreatePolygon(csLongLat);

			AssertNotNull(polygon);

			AssertEquals("Start point is correct", "POINT (1 1)", polygon.StartPoint.ToString());
			AssertEquals("End point is correct", "POINT (1 1)", polygon.EndPoint.ToString());

			AssertEquals("Point count is correct", 4, polygon.PointCount);

			var p = (SqlGeography)polygon;
			AssertEquals("First point should be in polygon.", "Longitude = 1, Latitude = 1", $"Longitude = {p.STPointN(1).Long.Value}, Latitude = {p.STPointN(1).Lat.Value}");
			AssertEquals("Second point should be in polygon", "Longitude = 3, Latitude = 3", $"Longitude = {p.STPointN(2).Long.Value}, Latitude = {p.STPointN(2).Lat.Value}");
			AssertEquals("Third point should be in polygon", "Longitude = 1, Latitude = 3", $"Longitude = {p.STPointN(3).Long.Value}, Latitude = {p.STPointN(3).Lat.Value}");
			AssertEquals("Last point should be in polygon", "Longitude = 1, Latitude = 1", $"Longitude = {p.STPointN(4).Long.Value}, Latitude = {p.STPointN(4).Lat.Value}");
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestCreatePolygon_CoordinateText_AllowMultiPolygon_CorrectInput()
		{
			var multiPolygon = ZGeography.Empty;
			Assert(!multiPolygon.IsMultiPolygon);

			AssertNoExceptionThrown("No exception should be thrown since multi polygon is allowed.", () =>
			{
				multiPolygon = ZGeography.CreatePolygon("0 0,5 5,0 5,5 0,0 0", true);
			});
			Assert(multiPolygon.IsMultiPolygon);
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestCreatePolygon_CoordinateText_IncorrectInput()
		{
			var csLongLat = "1 1,-3 -3,1 1";
			AssertExceptionThrown("Should throw exception on not enough input points", typeof(ZTypeValueException), () => ZGeography.CreatePolygon(csLongLat));

			csLongLat = "1 1,3 3,3 1,2 1";
			AssertExceptionThrown("Should throw exception on Start point not matching End Point", typeof(ZTypeValueException), () => ZGeography.CreatePolygon(csLongLat));

			csLongLat = "1 1,3 3,3 3,1 1";
			AssertExceptionThrown("Should throw exception on it is not a valid polygon", typeof(ZTypeValueException), () => ZGeography.CreatePolygon(csLongLat));

			csLongLat = "0 0,5 5,0 5,5 0,0 0";
			AssertExceptionThrown("Should throw exception on creating a multi polygon", typeof(ZTypeValueException), () => ZGeography.CreatePolygon(csLongLat));
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestCreatePolygon_PointArray_CorrectInput()
		{
			var point1 = ZGeography.CreatePoint(0, 0);
			var point2 = ZGeography.CreatePoint(2, 0);
			var point3 = ZGeography.CreatePoint(2, 2);

			var polygon1 = ZGeography.CreatePolygon(point1, point2, point3);
			var polygon2 = ZGeography.CreatePolygon(point1, point2, point3, point1);

			AssertEquals("Valid polygon: different head and tail point.", "POLYGON ((0 0, 2 0, 2 2, 0 0))", polygon1.AsText());
			AssertEquals("Valid polygon: same head and tail point.", "POLYGON ((0 0, 2 0, 2 2, 0 0))", polygon2.AsText());
			AssertEquals("Same polygon since auto closing the polygon.", polygon1.AsText(), polygon2.AsText());
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestCreatePolygon_PointArray_IncorrectInput()
		{
			var point1 = ZGeography.CreatePoint(0, 0);
			var point2 = ZGeography.CreatePoint(2, 0);
			var point3 = ZGeography.CreatePoint(2, 2);
			var point4 = ZGeography.CreatePoint(0, 2);
			var polygon = ZGeography.CreatePolygon(point1, point2, point3, point4, point1);

			AssertExceptionThrown("Exception thrown on null point array.", typeof(ZTypeValueException), () =>
			{
				ZGeography[] array = null;
				ZGeography.CreatePolygon(array);
			});

			AssertExceptionThrown("Exception thrown on point array length is 0.", typeof(ZTypeValueException), () =>
			{
				ZGeography.CreatePolygon(Array.Empty<ZGeography>());
			});

			AssertExceptionThrown("Exception thrown on point array length is less than 3", typeof(ZTypeValueException), () =>
			{
				ZGeography.CreatePolygon(point1, point2);
			});

			AssertExceptionThrown("Exception thrown on point array length is less than 4 when head and tail point are same.", typeof(ZTypeValueException), () =>
			{
				ZGeography.CreatePolygon(point1, point2, point1);
			});

			AssertExceptionThrown("Exception thrown on point array contains empty point.", typeof(ZTypeValueException), () =>
			{
				ZGeography.CreatePolygon(point1, point2, point3, ZGeography.Empty);
			});

			AssertExceptionThrown("Exception thrown on point array contains non-point geography instances.", typeof(ZTypeValueException), () =>
			{
				ZGeography.CreatePolygon(point1, polygon, point2, point3);
			});

			AssertExceptionThrown("Exception thrown on point array is not a valid polygon.", typeof(ZTypeValueException), () =>
			{
				ZGeography.CreatePolygon(point1, point2, point2, point1);
			});
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestIntersects()
		{
			var csLongLat = "3 3,1 3,1 1,3 1,3 3";
			var polygon = ZGeography.CreatePolygon(csLongLat);
			var testPointInside = ZGeography.CreatePoint("2,2");
			var testPointOutside = ZGeography.CreatePoint("72,72");

			AssertNotNull(polygon);
			AssertNotNull(testPointInside);
			AssertNotNull(testPointOutside);
			AssertEquals("Point 2,2 should be in the polygon", polygon.Intersects(testPointInside), true);
			AssertEquals("Point 72,72 should NOT be in the polygon", polygon.Intersects(testPointOutside), false);
			AssertEquals("Point 2,2 should NOT be in the polygon after ReorientObject()", polygon.ReorientObject().Intersects(testPointInside), false);
			AssertEquals("Point 72,72 should be in the polygon after ReorientObject()", polygon.ReorientObject().Intersects(testPointOutside), true);
		}

		public void TestParseSafe()
		{
			var validPoint = ZGeography.ParseSafe("2,2", ZGeography.Empty);
			var invalidPoint = ZGeography.ParseSafe("500,500", ZGeography.Invalid);

			AssertNotNull(validPoint);
			AssertNotNull(invalidPoint);
			AssertEquals("Long/Lat of 2,2 should be valid.", true, validPoint.IsValid);
			AssertEquals("Long/Lat of 500,500 is not valid.", false, invalidPoint.IsValid);
			AssertEquals("Invalid Long/Lat should be set to our default.", invalidPoint, ZGeography.Invalid);
		}

		public void TestCreatePointDouble()
		{
			// Arrange
			const double lon = -122.3;
			const double lat = 47.6;
			const double alt = 53.4;

			// Act
			var result1 = ZGeography.CreatePoint(lon, lat);
			var result2 = ZGeography.CreatePoint(lon, lat, alt);

			// Assert
			AssertEquals(lon, result1.Longitude);
			AssertEquals(lat, result1.Latitude);
			AssertNull(result1.Elevation);
			AssertEquals(lon, result2.Longitude);
			AssertEquals(lat, result2.Latitude);
			AssertEquals(alt, result2.Elevation);
			AssertEquals("POINT (-122.3 47.6)", result1.ToString());
			AssertEquals("POINT (-122.3 47.6 53.4)", result2.ToString());
		}

		public void TestCreatePointDouble_WithNormalization()
		{
			// Arrange
			const double lon = -181;
			const double lat = 91;

			// Act
			var result = ZGeography.CreatePoint(lon, lat);

			// Assert
			AssertEquals(179d, result.Longitude);
			AssertEquals(89d, result.Latitude);
			AssertNull(result.Elevation);
		}

		#endregion

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestIsPoint()
		{
			Assert(ZGeography.Empty.IsPoint); //we use 'POINT EMPTY' for empty value.

			var point = ZGeography.CreatePoint(1, 2);
			Assert("Normal case.", point.IsPoint);

			var polygon = ZGeography.CreatePolygon("0 0,0 2,2 2,2 0,0 0");
			Assert("Polygon is not a point.", !polygon.IsPoint);
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestIsPolygon()
		{
			var polygon = ZGeography.CreatePolygon("0 0,0 2,2 2,2 0,0 0");
			Assert("Normal case.", polygon.IsPolygon);

			var point = ZGeography.CreatePoint(1, 2);
			Assert("Point is not a polygon", !point.IsPolygon);
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestIsMultiPolygon()
		{
			var multiPolygon = new ZGeography("MULTIPOLYGON(((0 0,2 0,2 2,0 2,0 0)),((10 10,12 10,12 12,10 12,10 10)))");
			var polygon = ZGeography.CreatePolygon("0 0,0 2,2 2,2 0,0 0");
			var point = ZGeography.CreatePoint(1, 2);

			Assert("A multi-polygon.", multiPolygon.IsMultiPolygon);
			Assert("Polygon is not multi-polygon.", !polygon.IsMultiPolygon);
			Assert("Point is not multi-polygon.", !point.IsMultiPolygon);
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestInvalidBinaryFormat()
		{
			AssertExceptionThrown(typeof(ArgumentException), "Do not pass any byte[]/ZBlob/SqlBytes into ZGeography constructor except for ones created by geography.STAsBinary() (on SQL Server) or ZGeography.AsBinary() (on C# side), please.",
				() => new ZGeography(HexStringToByteArray("0x010400000000000000")));
		}

		#region OGC/Extend Methods

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestSTUnion()
		{
			var polygon1 = ZGeography.CreatePolygon("0 0,2 0,2 2,0 2,0 0");
			var polygon2 = ZGeography.CreatePolygon("0 0,1 0,1 1,0 1,0 0");
			var result = polygon1.STUnion(polygon2);

			AssertEquals("POLYGON ((0 2, 0 1, 0 0, 1 0, 2 0, 2 2, 0 2))", result.AsText());
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestSTContains()
		{
			var point1 = ZGeography.CreatePoint(1, 1);
			var point2 = ZGeography.CreatePoint(3, 3);
			var polygon1 = ZGeography.CreatePolygon("0 0,2 0,2 2,0 2,0 0");
			var polygon2 = ZGeography.CreatePolygon("0 0,0 2,2 2,2 0,0 0");

			Assert("Point 1 in expected polygon.", polygon1.STContains(point1));
			Assert("Point 2 not in expected polygon.", !polygon1.STContains(point2));
			AssertEquals("Polygon 1 and 2 are same polygon but have different point sequence.", polygon2, polygon1.ReorientObject());
			Assert("Point 1 not in expected polygon since the polygon is inside-out.", !polygon2.STContains(point1));
			Assert("Point 2 in expected polygon since the polygon is inside-out.", polygon2.STContains(point2));
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestSTIsValid()
		{
			var invalidPolygon = new ZGeography(SqlGeography.STPolyFromText(new SqlChars("POLYGON((0 0,0 0,0 0,0 0))"), 4326));
			var validPolygon = ZGeography.CreatePolygon("0 0,2 0,2 2,0 2,0 0");

			Assert("Invalid polygon.", !invalidPolygon.STIsValid());
			Assert("Valid polygon", validPolygon.STIsValid());
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestSTNumPoints()
		{
			var point = ZGeography.CreatePoint(1, 1);
			var polygon = ZGeography.CreatePolygon("0 0,2 0,2 2,0 2,0 0");

			AssertEquals("Point number of Point", 1, point.STNumPoints());
			AssertEquals("Point number of Polygon", 5, polygon.STNumPoints());
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestEnvelopeAngle()
		{
			var polygon1 = ZGeography.CreatePolygon("0 0,2 0,2 2,0 2,0 0");
			var polygon2 = ZGeography.CreatePolygon("0 0,0 2,2 2,2 0,0 0");

			AssertLessThan("Polygon 1 has envelope angle less than 90 since it does not cover more than a hemisphere.", polygon1.EnvelopeAngle(), 90);
			AssertEquals("Polygon 2 is a reoriented version of polygon 1.", polygon2, polygon1.ReorientObject());
			AssertGreaterThanOrEqualTo("Polygon 2 has envelope angle greater than 90 since it is a reoriented version of polygon 1 and covers more than a hemisphere.", polygon2.EnvelopeAngle(), 90);
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestReorientObject()
		{
			var polygon = ZGeography.CreatePolygon("0 0,0 2,2 2,2 0,0 0");

			AssertEquals("The polygon is reoriented.", "POLYGON ((0 0, 2 0, 2 2, 0 2, 0 0))", polygon.ReorientObject().AsText());
			AssertEquals("Double reoriented polygon equals the original.", polygon, polygon.ReorientObject().ReorientObject());
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestMakeValid()
		{
			var invalidPolygon = new ZGeography(SqlGeography.STPolyFromText(new SqlChars("POLYGON((0 0,0 0,0 0,0 0))"), 4326));
			Assert("Invalid polygon created.", !invalidPolygon.STIsValid());

			var validValue = invalidPolygon.MakeValid();

			Assert("A valid geography value is created.", validValue.STIsValid());
			AssertEquals("The invalid polygon equals to a point after MakeValid().", ZGeography.CreatePoint(0, 0), validValue);
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestGetValueWithInHemisphere()
		{
			var polygon1 = ZGeography.CreatePolygon("0 0,2 0,2 2,0 2,0 0");
			var polygon2 = ZGeography.CreatePolygon("0 0,0 2,2 2,2 0,0 0");

			AssertEquals("A polygon not covering more than a hemisphere will create a exact copy.", polygon1, polygon1.GetValueWithInHemisphere());
			AssertEquals("A polygon covering more than a hemisphere will create a reoriented version.", polygon2, polygon2.GetValueWithInHemisphere().ReorientObject());
		}

		#endregion

		#region Utils

		public void TestIsGeographyValue()
		{
			Assert(ZGeography.IsGeographyValue(ZGeography.CreatePoint(1, 1)));
			Assert(ZGeography.IsGeographyValue(SqlGeography.STPointFromText(new SqlChars("POINT (1 1)"), 4326)));
			Assert(ZGeography.IsGeographyValue(SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT EMPTY")), 4326)));

			Assert(!ZGeography.IsGeographyValue(null));
			Assert(!ZGeography.IsGeographyValue(new object()));
		}

		public void TestAsText()
		{
			AssertEquals("POINT EMPTY", ZGeography.Empty.AsText());
			AssertEquals("POINT EMPTY", ZGeography.AsText(ZGeography.Empty));
			AssertEquals("POINT (1 1)", ZGeography.AsText(ZGeography.CreatePoint(1, 1)));
			AssertEquals("POINT EMPTY", ZGeography.AsText(SqlGeography.STPointFromText(new SqlChars("POINT EMPTY"), 4326)));
			AssertEquals("POINT (1 1)", ZGeography.AsText(SqlGeography.STPointFromText(new SqlChars("POINT (1 1)"), 4326)));
			AssertEquals("POINT EMPTY", ZGeography.AsText(SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT EMPTY")), 4326)));
			AssertEquals("POINT (1 1)", ZGeography.AsText(SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT (1 1)")), 4326)));

			//test that elevation survives round trip
			AssertEquals("POINT (-152.22 58.88 10.5)", ZGeography.AsText(SqlGeography.STPointFromText(new SqlChars("POINT (-152.22 58.88 10.5)"), 4326)));

			AssertExceptionThrown(typeof(InvalidOperationException), () => ZGeography.AsText(null));
			AssertExceptionThrown(typeof(InvalidOperationException), () => ZGeography.AsText(new object()));
		}

		public void TestAsGml()
		{
			AssertEquals("<?xml version=\"1.0\" encoding=\"utf-8\"?><Point xmlns=\"http://www.opengis.net/gml\"><pos /></Point>", ZGeography.Empty.AsGml());
			AssertEquals("<?xml version=\"1.0\" encoding=\"utf-8\"?><Point xmlns=\"http://www.opengis.net/gml\"><pos>1 1</pos></Point>", ZGeography.CreatePoint(1, 1).AsGml());
		}

		public void TestSpatialTypeName()
		{
			AssertEquals("Point", ZGeography.Empty.SpatialTypeName);
			AssertEquals("Point", ZGeography.CreatePoint(1, 1).SpatialTypeName);
			AssertEquals("Polygon", ZGeography.CreatePolygon("0 0,0 2,2 2,2 0,0 0").SpatialTypeName);
			AssertEquals("MultiPolygon", new ZGeography("MULTIPOLYGON(((0 0,2 0,2 2,0 2,0 0)),((10 10,12 10,12 12,10 12,10 10)))").SpatialTypeName);
		}

		public void TestGetSqlGeographyValue()
		{
			var empty = SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT EMPTY")), 4326);
			var point = SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT (1 1)")), 4326);

			AssertSqlGeography(empty, ZGeography.GetSqlGeographyValue(empty));
			AssertSqlGeography(point, ZGeography.GetSqlGeographyValue(point));
			AssertSqlGeography(empty, ZGeography.GetSqlGeographyValue(SqlGeography.STPointFromText(new SqlChars("POINT EMPTY"), 4326)));
			AssertSqlGeography(point, ZGeography.GetSqlGeographyValue(SqlGeography.STPointFromText(new SqlChars("POINT (1 1)"), 4326)));
			AssertSqlGeography(empty, ZGeography.GetSqlGeographyValue(ZGeography.Empty));
			AssertSqlGeography(point, ZGeography.GetSqlGeographyValue(ZGeography.CreatePoint(1, 1)));

			AssertExceptionThrown(typeof(OperationOnInvalidZGeographyException), () => ZGeography.GetSqlGeographyValue(ZGeography.Invalid));
			AssertExceptionThrown(typeof(OperationOnInvalidZGeographyException), () => ZGeography.GetSqlGeographyValue(new object()));
			AssertExceptionThrown(typeof(OperationOnInvalidZGeographyException), () => ZGeography.GetSqlGeographyValue(null));

			string AsText(SqlGeography value)
			{
				return value.STAsText().ToSqlString().ToString();
			}

			void AssertSqlGeography(SqlGeography expected, SqlGeography actual)
			{
				AssertEquals(AsText(expected), AsText(ZGeography.GetSqlGeographyValue(actual)));
				AssertEquals(expected.STSrid.Value, actual.STSrid.Value);
			}
		}

		// https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-arra
		public static byte[] HexStringToByteArray(string hex)
		{
			if (hex[0] == '0' && hex[1] == 'x')
			{
				hex = hex.Substring(2);
			}

			if (hex.Length % 2 == 1)
			{
				throw new ArgumentException("The binary key cannot have an odd number of digits");
			}

			byte[] arr = new byte[hex.Length >> 1];

			for (int i = 0; i < hex.Length >> 1; ++i)
			{
				arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
			}

			return arr;
		}

		public static int GetHexVal(char hex)
		{
			int val = hex;
			//For uppercase A-F letters:
			return val - (val < 58 ? 48 : 55);
			//For lowercase a-f letters:
			//return val - (val < 58 ? 48 : 87);
			//Or the two combined, but a bit slower:
			//return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
		}

		#endregion
	}
}
