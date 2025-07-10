using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ModuleFountainFilter : ModuleNumberFilter
	{
		#region Construction

		protected ModuleFountainFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleFountainFilter(ZString description, SchemaStringColumn filterColumn, ZString fountainPrefix)
			: this(description, filterColumn, fountainPrefix, 8)
		{
		}

		public ModuleFountainFilter(ZString description, SchemaStringColumn filterColumn, ZString fountainPrefix, int fountainPaddingLength)
			: base(description, filterColumn)
		{
			prefix = fountainPrefix;
			paddingLength = fountainPaddingLength;
		}

		public ModuleFountainFilter(ZString description, GetTextQueryWithOperator queryDelegate, ZString fountainPrefix)
			: this(description, queryDelegate, fountainPrefix, 8)
		{
		}

		public ModuleFountainFilter(ZString description, GetTextQueryWithOperator queryDelegate, ZString fountainPrefix, int fountainPaddingLength)
			: base(description, queryDelegate)
		{
			prefix = fountainPrefix;
			paddingLength = fountainPaddingLength;
		}

		readonly ZString prefix;
		readonly int paddingLength;

		#endregion

		#region Property

		public override ZString Property
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Property; }
			set
			{
				base.Property = value;
				ExpandValue();
			}
		}

		#endregion

		#region ComparisonOperator

		public override ZString ComparisonOperator
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.ComparisonOperator; }
			set
			{
				base.ComparisonOperator = value;
				ExpandValue();
			}
		}

		#endregion

		#region Expand Value

		void ExpandValue()
		{
			if (!Property.IsEmpty
				&& SqlComparisonOperator != SQLComparisonOperator.Contains
				&& SqlComparisonOperator != SQLComparisonOperator.NotContains
				&& SqlComparisonOperator != SQLComparisonOperator.EndsWith)
			{
				var number = Property.ToUpper();
				if (number.Length < prefix.Length + paddingLength && (!number.StartsWith(prefix) || prefix.IsEmpty))
				{
					Property = prefix + number.PadLeft(paddingLength, '0');
				}
			}
		}

		#endregion
	}
}
