using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class RegistryItemDependentNumericRegistryDataType<T> : RegistryDataType<T>
	{
		protected RegistryItemDependentNumericRegistryDataType(string code, IRegistryItem lowerBoundRegistryItem, IRegistryItem upperBoundRegistryItem, bool lowerBoundInclusive, bool upperBoundInclusive, T defaultValue)
			: base(code, defaultValue)
		{
			this.lowerBoundRegistryItem = lowerBoundRegistryItem;
			this.upperBoundRegistryItem = upperBoundRegistryItem;
			this.lowerBoundInclusive = lowerBoundInclusive;
			this.upperBoundInclusive = upperBoundInclusive;
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new NumericRegistryEditorInfo(0);
		}

		protected sealed override void ValidateCore(IRegistryItem registryItem, T proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			IComparable<T> comparable = (IComparable<T>)proposedValue;

			if (lowerBoundRegistryItem != null)
			{
				T lowerBound = GetLowerBoundRegistryItemValue(companyPK, branchPK, departmentPK);
				int compareResult = comparable.CompareTo(lowerBound);
				int minAllowableResult = lowerBoundInclusive ? 0 : 1;

				if (compareResult < minAllowableResult)
				{
					throw new RegistryValidationException(GetErrorMessage(companyPK, branchPK, departmentPK));
				}
			}

			if (upperBoundRegistryItem != null)
			{
				T upperBound = GetUpperBoundRegistryItemValue(companyPK, branchPK, departmentPK);
				int compareResult = comparable.CompareTo(upperBound);
				int maxAllowableResult = upperBoundInclusive ? 0 : -1;

				if (compareResult > maxAllowableResult)
				{
					throw new RegistryValidationException(GetErrorMessage(companyPK, branchPK, departmentPK));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String to build up sentence")]
		protected string GetErrorMessage(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			string errorMessage = null;
			if (lowerBoundRegistryItem != null)
			{
				string tail = lowerBoundInclusive ? "greater than or equal to " : "greater than ";
				errorMessage += "Number must be " + tail + lowerBoundRegistryItem.Caption + " (" + GetLowerBoundRegistryItemValue(companyPK, branchPK, departmentPK) + ")";
			}
			if (upperBoundRegistryItem != null)
			{
				string header = (errorMessage == null) ? "Number must be " : " and ";
				string tail = (upperBoundInclusive) ? "less than or equal to " : "less than ";
				errorMessage += header + tail + upperBoundRegistryItem.Caption + " (" + GetUpperBoundRegistryItemValue(companyPK, branchPK, departmentPK) + ")";
			}
			errorMessage += ".";
			return errorMessage;
		}

		protected T GetLowerBoundRegistryItemValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return (T)((IRegistryItemInternals)lowerBoundRegistryItem).GetCurrentValueFromProposedValueAccessor(companyPK, branchPK, departmentPK);
		}

		protected T GetUpperBoundRegistryItemValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return (T)((IRegistryItemInternals)upperBoundRegistryItem).GetCurrentValueFromProposedValueAccessor(companyPK, branchPK, departmentPK);
		}

		readonly IRegistryItem lowerBoundRegistryItem;
		readonly IRegistryItem upperBoundRegistryItem;
		readonly bool lowerBoundInclusive;
		readonly bool upperBoundInclusive;
	}
}
