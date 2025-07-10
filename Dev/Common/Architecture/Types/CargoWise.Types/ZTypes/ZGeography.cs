using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.Common;
using Microsoft.SqlServer.Types;

namespace CargoWise.Types
{
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes"), TypeConverter(typeof(ZGeographyTypeConverter))]
	[DebuggerDisplay("{DebuggerDisplay}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZGeography : IZType, IZTypeInternals
	{
		public const int SridGps = 4326;
		public const int Srid2dPlane = 3857; // https://stackoverflow.com/questions/15776161/coordinatesystemid-on-SqlGeography
		const int InvalidCoordinateSystem = 4995; //Used to annotate if a ZGeography 'Point Empty' is a valid Empty, or Invalid. It's a coordinate system from 1860, so no-one should be using it

		[XmlIgnore]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Debugging")]
		public string DebuggerDisplay
		{
			get { return (IsValid ? ToString() : "Invalid ") + (IsEmpty ? "Empty" : string.Empty); }
		}

		//[DebuggerStepThrough]
		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public ZGeography(object value)
		{
			if (value == null)
			{
				//Since so much of the damn code relies on this, DBNull is an invalid empty
				this = Invalid;
				isNotEmpty = false;
			}
			else if (value is byte[] v)
			{
				this = new ZGeography(v);
			}
			else if (value is ZBlob blob)
			{
				this = new ZGeography(blob);
			}
			else if (value is SqlGeography geography2)
			{
				this = new ZGeography(geography2);
			}
			else if (value is ZGeography geography)
			{
				this = geography;
			}
			else if (value is SqlBytes sbytes)
			{
				this = new ZGeography(sbytes);
			}
			else if (value is string string1)
			{
				this = new ZGeography(string1);
			}
			else if (value is ZString @string)
			{
				this = new ZGeography(@string);
			}
			else if (value is DBNull)
			{
				throw new ZTypeValueException(typeof(ZGeography), value);
			}
			else
			{
				TypeConverter converter = ZGeographyTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					var converted = converter.ConvertFrom(value);
					this = (ZGeography)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZGeography), value);
				}
			}
		}

		//[DebuggerStepThrough]
		public ZGeography(SqlGeography value)
		{
			fValue = value;
			if (value == null)
			{
				this = Empty;
			}
			else if (value.STIsEmpty())
			{
				this = value.STSrid == InvalidCoordinateSystem ? Invalid : Empty;
			}
			else
			{
				fValue = value;
				isNotValid = false;
				isNotEmpty = true;
			}
		}

		//[DebuggerStepThrough]
		public ZGeography(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				this = Empty;
			}
			else
			{
				try
				{
					fValue = CreatePointHelper(value);
				}
				catch (Exception e) when (e is ZTypeValueException || e is TargetInvocationException || e is FormatException)
				{
					try
					{
						fValue = SqlGeography.Parse(value);
					}
					catch (Exception ex) when (ex is TargetInvocationException || ex is FormatException)
					{
						throw new ZTypeValueException(typeof(ZGeography), value);
					}
				}
				if (fValue.STIsEmpty())
				{
					this = Empty;
				}
				else
				{
					isNotValid = false;
					isNotEmpty = true;
				}
			}
		}

		public ZGeography(SqlBytes value) : this(value.Buffer)
		{
		}

		//[DebuggerStepThrough]
		public ZGeography(byte[] value)
		{
			if (value == null || value.Length == 0)
			{
				this = Empty;
			}
			else
			{
				try
				{
					fValue = SqlGeography.Deserialize(new SqlBytes(value));
				}
				catch (ArgumentException)
				{
					throw new ArgumentException("Do not pass any byte[]/ZBlob/SqlBytes into ZGeography constructor except for ones created by geography.STAsBinary() (on SQL Server) or ZGeography.AsBinary() (on C# side), please.");
				}
				catch (FormatException)
				{
					throw new ArgumentException("Do not pass any byte[]/ZBlob/SqlBytes into ZGeography constructor except for ones created by geography.STAsBinary() (on SQL Server) or ZGeography.AsBinary() (on C# side), please.");
				}
				catch (TargetInvocationException)
				{
					throw new ArgumentException("Do not pass any byte[]/ZBlob/SqlBytes into ZGeography constructor except for ones created by geography.STAsBinary() (on SQL Server) or ZGeography.AsBinary() (on C# side), please.");
				}
				if (fValue.STIsEmpty())
				{
					this = Empty;
				}
				else
				{
					isNotValid = false;
					isNotEmpty = true;
				}
			}
		}

		//[DebuggerStepThrough]
		public ZGeography(ZBlob value)
		{
			this = new ZGeography(value.XmlSerializedValue);
		}

		//Helper for Empty/Invalid
		//[DebuggerStepThrough]
		ZGeography(bool isEmpty)
		{
			fValue = isEmpty ? EmptySqlGeography : InvalidSqlGeography;
			isNotValid = !isEmpty;
			isNotEmpty = !isEmpty;
		}

		/// <summary>
		/// Create a ZGeography point based on longitude and latitude.
		/// </summary>
		/// <param name="longitude">Longitude (degrees)</param>
		/// <param name="latitude">Latitude (degrees)</param>
		/// <param name="altitudeMetres">Altitude (metres)</param>
		/// <returns></returns>
		public static ZGeography CreatePoint(double longitude, double latitude, double? altitudeMetres = null)
		{
			return new ZGeography(CreateSqlGeographyPoint(longitude, latitude, altitudeMetres));
		}

		/// <summary>
		///Create a ZGeography point based on longitude and latitude.
		/// </summary>
		/// <param name="value">
		/// String should be a longitude (deg) then a latitude (deg) and optionaly an altitude (m), all single comma or space delimited.
		/// -121.516153,45.710030
		/// -121.516153 45.710030
		/// -121.516153,45.710030,53.654
		/// -121.516153 45.710030 53.654
		/// </param>
		/// <returns></returns>
		public static ZGeography CreatePoint(string value)
		{
			Argument.NotNullOrEmpty(value, nameof(value));
			return new ZGeography(CreatePointHelper(value));
		}

		static SqlGeography CreatePointHelper(string value)
		{
			Argument.NotNullOrEmpty(value, nameof(value));
			var tokens = value.Split(',', ' ');
			switch (tokens.Length)
			{
				case 2:
					return CreateSqlGeographyPoint(tokens[0], tokens[1], null);
				case 3:
					return CreateSqlGeographyPoint(tokens[0], tokens[1], tokens[2]);
			}

			throw new ZTypeValueException(typeof(ZGeography), value);
		}

		static SqlGeography CreateSqlGeographyPoint(object longitude, object latitude, object altitudeMetres)
		{
			Argument.NotNull(longitude, nameof(longitude));
			Argument.NotNull(latitude, nameof(latitude));

			if (longitude is double @double)
			{
				longitude = NormalizeLongitudeDegree(@double);
			}

			if (latitude is double double1)
			{
				latitude = NormalizeLatitudeDegree(double1);
			}

			var text = altitudeMetres == null
				? FormattableString.Invariant($"POINT({longitude} {latitude})")
				: FormattableString.Invariant($"POINT({longitude} {latitude} {altitudeMetres})");

			// 4326 is most common coordinate system used by GPS/Maps
			var result = SqlGeography.STPointFromText(new SqlChars(text), SridGps);
			return result;
		}

		public static double NormalizeLatitudeDegree(double latitude)
		{
			while (latitude > 90 || latitude < -90)
			{
				latitude = latitude > 0 ? 180 - latitude : -180 - latitude;
			}

			return latitude;
		}

		public static ZDecimal NormalizeLatitudeDegree(ZDecimal latitude)
		{
			return NormalizeLatitudeDegree((double)latitude);
		}

		public static double NormalizeLongitudeDegree(double longitude)
		{
			while (longitude > 180 || longitude < -180)
			{
				longitude = longitude > 0 ? longitude - 360 : longitude + 360;
			}

			return longitude;
		}

		public static ZDecimal NormalizeLongitudeDegree(ZDecimal longitude)
		{
			return NormalizeLongitudeDegree((double)longitude);
		}

		/// <summary>
		///Create a ZGeography Polygon based on a comma delimited longitude and latitude string.
		/// </summary>
		/// <param name="csLongLatPoints">
		/// String should be longitude then latitude pairs, separated by commas.
		/// !Provide first long/lat pair twice at start and end of the string to complete polygon.!
		/// !Point order should follow the left-hand rule.!
		/// "-121.516153 45.710030,-123.526153 45.810030,-121.518153 45.715030,-121.516153 45.710030"
		/// </param>
		/// <param name="allowMultiPolygon">
		/// If allow a Multi Polygon is created as the result.
		/// </param>
		/// <returns></returns>
		public static ZGeography CreatePolygon(string csLongLatPoints, bool allowMultiPolygon = false)
		{
			Argument.NotNullOrEmpty(csLongLatPoints, nameof(csLongLatPoints));
			var result = new ZGeography(CreateCreatePolygonHelper(csLongLatPoints)).MakeValid();
			if (result.IsPolygon || (allowMultiPolygon && result.IsMultiPolygon))
			{
				return result;
			}
			else
			{
				throw new ZTypeValueException(string.Format(CultureInfo.InvariantCulture, "Cannot initialise a CargoWise.Types.ZGeography since given string does not represent a valid polygon or multi polygon."));
			}
		}

		public static ZGeography CreatePolygon(params ZGeography[] points)
		{
			if (points == null || points.Length <= 2)
			{
				throw new ZTypeValueException(string.Format(CultureInfo.InvariantCulture, "Cannot initialise a CargoWise.Types.ZGeography with a null point array or a point array with insufficient points."));
			}

			var headPoint = points[0];
			var tailPoint = points[points.Length - 1];
			if (headPoint != tailPoint ? points.Length < 3 : points.Length < 4)
			{
				throw new ZTypeValueException(string.Format(CultureInfo.InvariantCulture, @"Cannot initialise a CargoWise.Types.ZGeography with given point array.
The minimum allowed length of points is
- 4 when the first point equals to the last point
- 3 otherwise."));
			}

			var textBuilder = new ZStringBuilder();

			foreach (var point in points)
			{
				if (!point.IsValid || point.IsEmpty || !point.IsPoint)
				{
					throw new ZTypeValueException(string.Format(CultureInfo.InvariantCulture, "Cannot initialise a CargoWise.Types.ZGeography since an invalid point found in the given array."));
				}

				textBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0} {1}", point.Longitude, point.Latitude));
			}

			if (headPoint != tailPoint)
			{
				textBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0} {1}", headPoint.Longitude, headPoint.Latitude));
			}

			var result = CreatePolygon(textBuilder.ToStringWithDelimiterBetweenAppends(",")).MakeValid();
			if (!result.IsPolygon)
			{
				throw new ZTypeValueException(string.Format(CultureInfo.InvariantCulture, "Cannot initialise a CargoWise.Types.ZGeography since given point array does not represent a valid polygon."));
			}

			return result;
		}

		static SqlGeography CreateCreatePolygonHelper(string csLongLatPoints)
		{
			Argument.NotNullOrEmpty(csLongLatPoints, nameof(csLongLatPoints));

			var tokens = csLongLatPoints.Split(',');
			if (tokens.Length < 4) //minimum input point number for polygon is 4 for a triangle (Start, 2nd, 3rd, Start)
			{
				throw new ZTypeValueException(string.Format(CultureInfo.InvariantCulture, "Cannot initialise a CargoWise.Types.ZGeography with {0}. The minimum allowed number of points is 4.", csLongLatPoints));
			}
			if (tokens.First() != tokens.Last())
			{
				throw new ZTypeValueException(string.Format(CultureInfo.InvariantCulture, "Cannot initialise a CargoWise.Types.ZGeography with {0}. The first point must match the last.", csLongLatPoints));
			}

			var polygonInWellKnownFormat = string.Format(CultureInfo.InvariantCulture, "POLYGON(({0}))", csLongLatPoints);
			var result = SqlGeography.STPolyFromText(new SqlChars(polygonInWellKnownFormat), SridGps);
			return result;
		}

		#region True/False Constants

		/// <summary>
		/// The empty (but valid) ZGeography.
		/// </summary>
		public static readonly ZGeography Empty = new ZGeography(true);

		/// <summary>
		/// The invalid (but not empty) ZGeography.
		/// </summary>
		public static readonly ZGeography Invalid = new ZGeography(false);

		#endregion

		#region Object Overrides

		/// <summary>
		/// Is this instance equal to the specified object?
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		//[DebuggerStepThrough]
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public override bool Equals(object obj)
		{
			try
			{
				return Equals(this, obj);
			}
			catch
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Invalid string conversion")]
		public override string ToString()
		{
			if (IsEmpty)
			{
				return string.Empty;
			}
			else if (!IsValid)
			{
				return "<Invalid>";
			}
			return fValue.AsTextZM().ToSqlString().ToString();
		}

		#endregion

		#region Casting

		//[DebuggerStepThrough]
		public static implicit operator ZGeography(SqlGeography value)
		{
			return new ZGeography(value);
		}

		//[DebuggerStepThrough]
		public static implicit operator SqlGeography(ZGeography value)
		{
			return value.fValue;
		}

		#endregion

		#region Operator Overloads

		#region ZGeography + ZGeography

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]//, System.Diagnostics.DebuggerStepThrough]
		public static bool operator ==(ZGeography lhs, ZGeography rhs)
		{
			bool result = false;

			if (lhs.IsValid && rhs.IsValid && !lhs.IsEmpty && !rhs.IsEmpty)
			{
				result = (lhs.fValue.STEquals(rhs.fValue)).Value;
			}
			//Invalid Empty unfortunately needs to == Empty, hence this code
			else if (lhs.IsEmpty && rhs.IsEmpty)
			{
				result = true;
			}
			else if (lhs.IsEmpty == rhs.IsEmpty && lhs.IsValid == rhs.IsValid)
			{
				result = true;
			}

			return result;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZGeography lhs, ZGeography rhs) => !(lhs == rhs);

		#endregion

		#endregion

		#region TryParse / ParseSafe

		public static bool TryParse(string value, out ZGeography result)
		{
			bool success = false;

			if (string.IsNullOrEmpty(value))
			{
				result = Empty;
				success = true;
			}
			else
			{
				try
				{
					result = new ZGeography(value);
					success = result != Invalid;
				}
				catch (ZTypeValueException)
				{
					result = Invalid;
				}
			}
			return success;
		}

		public static ZGeography ParseSafe(ZString value, ZGeography defaultValue)
		{
			return TryParse(value, out var parsed) ? parsed : defaultValue;
		}

		#endregion

		#region SqlGeography members

		public double? Distance(SqlGeography other)
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			Argument.NotNull(other, nameof(other));
			var result = fValue.STDistance(other);
			return result.IsNull ? null : result.Value;
		}

		public double? Distance(ZGeography other)
		{
			if (!(IsValid && !IsEmpty))
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			if (!(other.IsValid && !other.IsEmpty))
			{
				throw new ArgumentException("Cannot operate on an invalid SqlGeography.", nameof(other));
			}

			var result = fValue.STDistance(other);
			return result.IsNull ? null : result.Value;
		}

		public bool SpatialEquals(SqlGeography other)
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			Argument.NotNull(other, nameof(other));
			return fValue.STEquals(other).Value;
		}

		public bool SpatialEquals(ZGeography other)
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			if (!other.IsValid)
			{
				throw new ArgumentException("Cannot operate on an invalid SqlGeography.", nameof(other));
			}

			return fValue.STEquals(other).Value;
		}

		public bool Intersects(ZGeography other)
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			if (!(other.IsValid && !other.IsEmpty))
			{
				throw new ArgumentException("Cannot operate on an invalid SqlGeography.", nameof(other));
			}

			return fValue.STIntersects(other).Value;
		}

		public byte[] AsBinary()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			var result = fValue.Serialize().Buffer;
			return result;
		}

		public string AsGml()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			return fValue.AsGml().Value;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Empty string conversion")]
		public string AsText()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}
			if (IsEmpty)
			{
				return "POINT EMPTY";
			}

			return fValue.AsTextZM().ToSqlString().ToString();
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public double? Area
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				var result = fValue.STArea();
				return result.IsNull ? null : result.Value;
			}
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public int CoordinateSystemId
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				return fValue.STSrid.Value;
			}
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public int Dimension
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				return fValue.STDimension().Value;
			}
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public int? NumCurves
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				var result = fValue.STNumCurves();
				return result.IsNull ? null : result.Value;
			}
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public int? NumGeometries
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				var result = fValue.STNumGeometries();
				return result.IsNull ? null : result.Value;
			}
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public double? Elevation
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				var result = fValue.Z;
				return result.IsNull ? null : result.Value;
			}
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public SqlGeography EndPoint
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				return fValue.STEndPoint();
			}
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public bool? IsClosed
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				var result = fValue.STIsClosed();
				return result.IsNull ? null : result.Value;
			}
		}

		//Not copying IsEmpty because that has a different meaning for us!

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public double? Latitude
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				var result = fValue.Lat;
				return result.IsNull ? null : result.Value;
			}
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public double? Length
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				var result = fValue.STLength();
				return result.IsNull ? null : result.Value;
			}
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public double? Longitude
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				var result = fValue.Long;
				return result.IsNull ? null : result.Value;
			}
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public double? Measure
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				var result = fValue.M;
				return result.IsNull ? null : result.Value;
			}
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public int? PointCount
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				var result = fValue.STNumPoints();
				return result.IsNull ? null : result.Value;
			}
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public string SpatialTypeName
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				//surprisingly, only DBGeography implements this. we have to make our own hack version and hope it works.
				var str = this.fValue.AsTextZM().ToSqlString().ToString();
				foreach (var type in SpatialType.All)
				{
					if (str.StartsWith(type.ToUpperInvariant()))
					{
						return type;
					}
				}
				throw new InvalidOperationException(string.Format("Expected this geography to have a type and it didn't: {0}", str));
			}
		}

		[XmlIgnore]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public SqlGeography StartPoint
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				return fValue.STStartPoint();
			}
		}

		[XmlIgnore]
		public bool IsPoint
		{
			get
			{
				return CheckIsSameSpatialType(SpatialType.Point);
			}
		}

		[XmlIgnore]
		public bool IsPolygon
		{
			get
			{
				return CheckIsSameSpatialType(SpatialType.Polygon);
			}
		}

		[XmlIgnore]
		public bool IsMultiPolygon
		{
			get
			{
				return CheckIsSameSpatialType(SpatialType.MultiPolygon);
			}
		}

		#endregion

		#region OGC/Extended Methods

		public ZGeography STUnion(ZGeography value)
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			if (!value.IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			return new ZGeography(SqlGeographyValue.STUnion(value.SqlGeographyValue).MakeValid().STAsText().ToSqlString().ToString());
		}

		public bool STContains(ZGeography value)
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			if (!value.IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			return SqlGeographyValue.STContains(value.SqlGeographyValue).IsTrue;
		}

		public bool STIsValid()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			return SqlGeographyValue.STIsValid().IsTrue;
		}

		public int STNumPoints()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			return SqlGeographyValue.STNumPoints().Value;
		}

		public double EnvelopeAngle()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			return SqlGeographyValue.EnvelopeAngle().Value;
		}

		public ZGeography ReorientObject()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			return new ZGeography(SqlGeographyValue.ReorientObject().STAsText().ToSqlString().ToString());
		}

		public ZGeography MakeValid()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
			}

			return new ZGeography(SqlGeographyValue.MakeValid().STAsText().ToSqlString().ToString());
		}

		public ZGeography GetValueWithInHemisphere()
		{
			return EnvelopeAngle() < 90 ? this : ReorientObject();
		}

		SqlGeography SqlGeographyValue
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				return GetSqlGeographyValue(this);
			}
		}

		#endregion

		#region XmlSerializedValue

		[XmlText]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public SqlGeography XmlSerializedValue
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				return this;
			}
			set { this = value; }
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			throw new NotSupportedException("IComparable.CompareTo() cannot be used on ZGeographys!");
		}

		#endregion

		#region IZTypeInternals Members

		object IZTypeInternals.GetValueForLogicalDataLayer(bool isNullable)
		{
			return (!IsValid && IsEmpty) && isNullable ? DBNull.Value : fValue;
		}

		#endregion

		#region IZType Members

		[XmlIgnore]
		public ZDataType DataType => ZDataType.NonNumeric;

		[XmlIgnore]
		public Type BaseDataType => typeof(SqlGeography);

		[XmlIgnore]
		public bool IsEmpty { get => !isNotEmpty; }

		[XmlIgnore]
		public bool IsValid { get => !isNotValid; }

		[XmlIgnore]
		public bool IsDefault => this == Empty;

		[XmlIgnore]
		public IZType Default => Empty;

		//Required because XML serialisation assumes that the default values in the struct are set to the right values by default (doesn't use the IZType Default)
		//In this case, the default ZGeography is Valid, and is Empty
		readonly bool isNotEmpty;
		readonly bool isNotValid;
		#endregion

		#region Utils

		public new static bool Equals(object value1, object value2)
		{
			return GetZValue(value1) == GetZValue(value2);

			ZGeography GetZValue(object value)
			{
				if (value is ZGeography geography)
				{
					return geography;
				}
				else if (value is SqlGeography)
				{
					return new ZGeography(value);
				}
				else if (value is SqlGeography sqlValue)
				{
					return new ZGeography(sqlValue.STAsText().ToSqlString().ToString());
				}

				throw new InvalidOperationException("Only ZGeography, SqlGeography and SqlGeography are supported.");
			}
		}

		public static bool IsGeographyValue(object value)
		{
			return value != null && (
				value is ZGeography
				|| value is SqlGeography
				|| value is SqlGeography);
		}

		public static string AsText(object value)
		{
			if (value is ZGeography zValue)
			{
				return zValue.AsText();
			}
			else if (value is SqlGeography dbValue)
			{
				return AsText(new ZGeography(dbValue));
			}
			else if (value is SqlGeography sqlValue)
			{
				return sqlValue.STAsText().ToSqlString().ToString();
			}

			throw new InvalidOperationException("Only ZGeography, SqlGeography and SqlGeography are supported.");
		}

		public static SqlGeography GetSqlGeographyValue(object value)
		{
			if (value is SqlGeography sqlValue)
			{
				return SqlGeography.STGeomFromText(sqlValue.STAsText(), sqlValue.STSrid.Value);
			}
			else if (value is ZGeography zValue)
			{
				if (!zValue.IsValid)
				{
					throw new OperationOnInvalidZGeographyException("Cannot operate on an invalid SqlGeography.");
				}

				return SqlGeography.STGeomFromText(new SqlChars(new SqlString(zValue.AsText())), zValue.Value.STSrid.Value);
			}

			throw new OperationOnInvalidZGeographyException("Only support SqlGeography, SqlGeography and ZGeography");
		}

		#endregion

		#region Implementation

		[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Avoid extra allocations for performance reasons")]
		readonly SqlGeography fValue;

		SqlGeography Value
		{
			get
			{
				if (IsValid && fValue == null)
				{
					return EmptySqlGeography;
				}

				return fValue;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sql character")]
		public static SqlGeography EmptySqlGeography => SqlGeography.STPointFromText(new SqlChars("POINT EMPTY"), SridGps);
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sql character")]
		public static SqlGeography EmptyPolygonSqlGeography => SqlGeography.STPointFromText(new SqlChars("POLYGON EMPTY"), SridGps);
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sql character")]
		public static SqlGeography InvalidSqlGeography => SqlGeography.STPointFromText(new SqlChars("POINT EMPTY"), InvalidCoordinateSystem);

		bool CheckIsSameSpatialType(string target)
		{
			return SpatialTypeName.Equals(target, StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Type strings")]
		public static class SpatialType
		{
			public const string Point = "Point";
			public const string LineString = "LineString";
			public const string CircularString = "CircularString";
			public const string CompoundCurve = "CompoundCurve";
			public const string Polygon = "Polygon";
			public const string CurvePolygon = "CurvePolygon";
			public const string MultiPoint = "MultiPoint";
			public const string MultiLineString = "MultiLineString";
			public const string MultiPolygon = "MultiPolygon";
			public const string GeometryCollection = "GeometryCollection";

			public static IEnumerable<string> All
			{
				get
				{
					yield return Point;
					yield return LineString;
					yield return CircularString;
					yield return CompoundCurve;
					yield return Polygon;
					yield return CurvePolygon;
					yield return MultiPoint;
					yield return MultiLineString;
					yield return MultiPolygon;
					yield return GeometryCollection;
				}
			}
		}

		public class SpatialTypeComparer : IEqualityComparer<string>
		{
			public bool Equals(string x, string y)
			{
				return x.Equals(y, StringComparison.OrdinalIgnoreCase);
			}

			public int GetHashCode(string obj)
			{
				return obj.GetHashCode();
			}
		}
	}

	[Serializable]
	public sealed class OperationOnInvalidZGeographyException : OperationOnInvalidZTypeException
	{
		public OperationOnInvalidZGeographyException()
		{
		}

		public OperationOnInvalidZGeographyException(string message)
			: base(message)
		{
		}

		public OperationOnInvalidZGeographyException(string message, Exception ex)
			: base(message, ex)
		{
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		OperationOnInvalidZGeographyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
