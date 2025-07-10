using System.Globalization;
using Enterprise.Core;

namespace Enterprise.ZArchitecture
{
	using System;
	using CargoWise.Types;

	public struct Quantity : IQuantity
	{
		/// <summary>
		///		Creates a new quantity instance representing some amount (weight, volume, money, distance, etc.) with some unit.
		/// </summary>
		/// <param name="source">
		///		The source of the amount. For example, shipment, container, consol, packline, etc.
		/// </param>
		/// <param name="reference">
		///		The unique number of the <paramref name="source"/>. For example, shipment number, container number, etc.
		/// </param>
		/// <param name="description">
		///		A free text description typically shown after the quantity text representation.
		/// </param>
		/// <param name="label">
		///		A free text label typically shown before the quantity text representation.
		/// </param>
		/// <exception cref="ArgumentException">
		///		Thrown when <paramref name="unit"/> is not specified as the quantity cannot be without unit.
		/// </exception>
		public Quantity(ZDecimal amount, ZString unit, string source = "", string reference = "", string description = "", string label = "")
		{
			if (string.IsNullOrWhiteSpace(unit))
			{
				throw new ArgumentException("The unit cannot be empty", nameof(unit));
			}

			isNotEmpty = true;
			Amount = amount;
			Unit = unit;
			Source = source;
			Reference = reference;
			Description = description;
			Label = label;
		}

		public bool IsEmpty => !isNotEmpty;
		readonly bool isNotEmpty;

		public ZBool IsValid => true;

		public ZDecimal Amount { get; }

		public ZString Unit { get; }

		public ZUnitType? UnitType
		{
			get
			{
				if (Constants.Weight.ContainsCode(Unit))
				{
					return ZUnitType.Weight;
				}

				if (Constants.Volume.ContainsCode(Unit))
				{
					return ZUnitType.Volume;
				}

				if (Constants.Length.ContainsCode(Unit))
				{
					return ZUnitType.Length;
				}

				return null;
			}
		}

		// TODO: Somebody needs to refactor this Quantity from having all these
		// separate strings, into having perhaps an "Attributes' field which
		// given an ID will point to an object, probably a string. This is 
		// because the Quantity doesnt care about these strings.At all.
		// It's the user to this Quantity that wants to store a value so that
		// the quantity can take it with it wherever the quantity travels to.

		/// <summary>
		/// The source of the amount. For example, shipment, container, consol, packline, etc.
		/// </summary>
		public ZString Source { get; }
		/// <summary>
		/// The unique number of the <paramref name="source"/>. For example, shipment number, container number, etc.
		/// </summary>
		public ZString Reference { get; }
		/// <summary>
		/// A free text description typically shown after the quantity text representation.
		/// </summary>
		public ZString Description { get; private set; }
		/// <summary>
		/// A free text label typically shown before the quantity text representation.
		/// </summary>
		public ZString Label { get; }

		public override string ToString()
		{
			return IsEmpty ? string.Empty : string.Format(CultureInfo.CurrentCulture, "{0} {1}", Amount, Unit);
		}

		public string ToString(int decimals)
		{
			return IsEmpty ? string.Empty : string.Format(CultureInfo.CurrentCulture, "{0} {1}", Amount.ToString(decimals), Unit);
		}

		public static bool operator ==(Quantity q1, Quantity q2) => q1.Equals(q2);
		public static bool operator !=(Quantity q1, Quantity q2) => !q1.Equals(q2);

		public static Quantity Empty(string description)
		{
			var result = default(Quantity);
			result.Description = description;
			return result;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Quantity))
			{
				return false;
			}

			var other = (Quantity)obj;
			return Amount.Equals(other.Amount) && Unit.Equals(other.Unit) && Reference.Equals(other.Reference);
		}

		public override int GetHashCode()
		{
			return Amount.GetHashCode();
		}
	}
}
