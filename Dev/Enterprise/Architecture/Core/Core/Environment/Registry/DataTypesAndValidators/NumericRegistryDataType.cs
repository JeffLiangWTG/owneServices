using System;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class NumericRegistryDataType<T> : RegistryDataType<T>
	{
		protected NumericRegistryDataType(string code, T defaultValue)
			: base(code, defaultValue)
		{
		}

		protected NumericRegistryDataType(string code, T defaultValue, double lowerBound, double upperBound)
			: base(code, defaultValue)
		{
			LowerBound = lowerBound;
			UpperBound = upperBound;
		}

		public double LowerBound
		{
			get { return lowerBound; }
			set { lowerBound = value; }
		}

		public double UpperBound
		{
			get { return upperBound; }
			set { upperBound = value; }
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new NumericRegistryEditorInfo(0);
		}

		protected override void ValidateCore(IRegistryItem registryItem, T proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			double value = Convert.ToDouble(proposedValue);

			if (value < LowerBound)
			{
				throw new RegistryValidationException(Res.GetString("7cf86ac9-3e01-4ace-a537-132475e58818", "Value must be greater than or equal to the minimum ({0})", LowerBound));
			}

			if (value > UpperBound)
			{
				throw new RegistryValidationException(Res.GetString("af1c41d7-11a9-4221-ae69-1174628246a8", "Value must be less than or equal to the maximum ({0})", UpperBound));
			}
		}

		protected override bool ValuesAreEqualCore(T a, T b)
		{
			return Equals(a, b);
		}

		double lowerBound = double.MinValue;
		double upperBound = double.MaxValue;
	}
}
