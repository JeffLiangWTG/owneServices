using System;
using System.Linq;

using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class StringArrayRegistryDataType : RegistryDataType<string[]>
	{
		protected StringArrayRegistryDataType(string code)
			: base(code, Array.Empty<string>())
		{
		}

		/// <summary>
		/// The maximum length of the string value
		/// </summary>
		public int MaximumLength { get; set; } = 256;

		public CharacterCase CharacterCase { get; set; } = CharacterCase.Normal;

		protected override bool ValuesAreEqualCore(string[] a, string[] b)
		{
			return a.Length == b.Length && a.SequenceEqual(b);
		}

		protected override bool HasDefaultEditorInfoCore
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		protected override void ValidateCore(IRegistryItem registryItem, string[] proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			foreach (string stringValue in proposedValue)
			{
				if (stringValue.Length > MaximumLength)
				{
					throw new RegistryValidationException("Values maximum length cannot be more than " + MaximumLength.ToString() + ".");
				}
			}
		}

		protected override string[] CloneValue(string[] value)
		{
			return (string[])value.Clone();
		}
	}
}
