using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DecimalArrayRegistryDataType : RegistryDataType<decimal[]>
	{
		public DecimalArrayRegistryDataType()
			: base(RegistryDataTypes.Codes.DecimalArray, Array.Empty<decimal>())
		{
			DecimalPlaces = 2;
		}

		/// <summary>
		/// The number of decimal places allowed for each value. The default is 2.
		/// </summary>
		public int DecimalPlaces
		{
			get { return decimalPlaces; }
			set { decimalPlaces = value; }
		}

		/// <summary>
		/// The smallest number allowed for each value. This is not set by default.
		/// </summary>
		public decimal? LowerBound
		{
			get { return lowerBound; }
			set { lowerBound = value; }
		}

		/// <summary>
		/// The biggest number allowed for each value. This is not set by default.
		/// </summary>
		public decimal? UpperBound
		{
			get { return upperBound; }
			set { upperBound = value; }
		}

		protected override bool HasDefaultEditorInfoCore
		{
			get { return false; }
		}

		protected override decimal[] DeserialiseCore(byte[] value)
		{
			List<decimal> result = new List<decimal>();
			string valueAsString = Encoding.Unicode.GetString(value);

			if (!string.IsNullOrEmpty(valueAsString))
			{
				foreach (string part in valueAsString.Split(','))
				{
					decimal number;
					if (decimal.TryParse(part.Trim(), out number))
					{
						result.Add(number);
					}
				}
			}

			return result.ToArray();
		}

		protected override byte[] SerialiseCore(decimal[] value)
		{
			byte[] result = Array.Empty<byte>();

			if (value != null)
			{
				if (value.Length > 0)
				{
					StringBuilder builder = new StringBuilder();
					foreach (decimal number in value)
					{
						if (builder.Length > 0)
						{
							builder.Append(',');
						}
						builder.Append(number.ToString());
					}
					result = Encoding.Unicode.GetBytes(builder.ToString());
				}
			}

			return result;
		}

		protected override void ValidateCore(IRegistryItem registryItem, decimal[] proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			foreach (decimal number in proposedValue)
			{
				if (LowerBound.HasValue && (number < LowerBound.Value))
				{
					throw new RegistryValidationException((NoResString)"Values must be greater than or equal to " + LowerBound.Value + (NoResString)".");
				}
				else if (UpperBound.HasValue && (number > UpperBound.Value))
				{
					throw new RegistryValidationException((NoResString)"Values must be less than or equal to " + UpperBound.Value + (NoResString)".");
				}
				else
				{
					string[] numberParts = number.ToString().Split('.');
					if ((numberParts.Length == 2) && (numberParts[1].TrimEnd('0').Length > DecimalPlaces))
					{
						string places = (DecimalPlaces == 1) ? Res.GetString("d1618de5-e5ef-4ba1-9275-7573382b3bf5", "place") : Res.GetString("3dd6b359-22b7-489a-9f1d-0fe14ad68ebe", "places");
						throw new RegistryValidationException(string.Format((NoResString)"Values cannot have more than {0} decimal {1}.", DecimalPlaces, places));
					}
				}
			}
		}

		protected override decimal[] CloneValue(decimal[] value)
		{
			return (decimal[])value.Clone();
		}

		decimal? lowerBound;
		decimal? upperBound;
		int decimalPlaces;
	}
}
