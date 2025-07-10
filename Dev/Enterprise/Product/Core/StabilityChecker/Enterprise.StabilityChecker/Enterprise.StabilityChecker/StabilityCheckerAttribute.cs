using System;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Definitions;

namespace Enterprise.StabilityChecker
{
	[XmlSerializerAssembly("Enterprise.StabilityChecker.XmlSerializers")]
	[Serializable]
	public sealed class StabilityCheckerAttribute : AssemblyMetaDataAttributeWithType, IEquatable<StabilityCheckerAttribute>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="description"></param>
		/// <param name="category"></param>
		/// <param name="stabilityCheckerType">An implementation of IStabilityChecker</param>
		public StabilityCheckerAttribute(string description, string category, Type stabilityCheckerType)
			: base(stabilityCheckerType)
		{
			Argument.NotNullOrEmpty(description, "description");
			Argument.NotNullOrEmpty(category, "category");
			if (stabilityCheckerType != null)
			{
				if (!typeof(IStabilityChecker).IsAssignableFrom(stabilityCheckerType))
				{
					throw new ArgumentException("Type does not implement IStabilityChecker", nameof(stabilityCheckerType));
				}
				if (stabilityCheckerType.GetConstructor(Array.Empty<Type>()) == null)
				{
					throw new ArgumentException("Parameterless constructor could not be found", nameof(stabilityCheckerType));
				}
			}
			Description = description;
			Category = category;
		}

		public StabilityCheckerAttribute()
		{ }

		/// <summary>
		/// A brief description of the stability check that will be performed
		/// </summary>
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		/// A grouping description that allows similar checks to be displayed together
		/// </summary>
		public string Category
		{
			get;
			set;
		}

		#region Equality members

		public bool Equals(StabilityCheckerAttribute other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other) && (Description, Category).Equals((other.Description, other.Category));
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is StabilityCheckerAttribute other && Equals(other);
		}

		public override int GetHashCode() => (base.GetHashCode(), Description, Category).GetHashCode();

		#endregion
	}
}
