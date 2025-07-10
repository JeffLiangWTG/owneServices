using System;
using System.Text;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class GuidRegistryDataType : RegistryDataType<Guid>
	{
		public GuidRegistryDataType()
			: base(RegistryDataTypes.Codes.Guid, Guid.Empty)
		{
		}

		public override bool IsDefaultValueImmutable => true;

		protected override bool HasDefaultEditorInfoCore
		{
			get { return false; }
		}

		protected override bool ValuesAreEqualCore(Guid a, Guid b)
		{
			return Equals(a, b);
		}

		protected override byte[] SerialiseCore(Guid value)
		{
			return Encoding.Unicode.GetBytes(value.ToString());
		}

		protected override Guid DeserialiseCore(byte[] value)
		{
			return new Guid(Encoding.Unicode.GetString(value));
		}

		protected override void ValidateCore(IRegistryItem registryItem, Guid proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (proposedValue == Guid.Empty && IsValueMandatory(registryItem))
			{
				throw new RegistryValidationException(Res.GetString("f496397c-1eeb-419a-9b82-c50ea56cd79c", "Please select a valid selection."));
			}
		}

		bool IsValueMandatory(IRegistryItem registryItem)
		{
			return (registryItem != null) && registryItem.IsValueMandatory;
		}

		protected override Guid GetGuidValue(object value)
		{
			return (value is Guid) ? (Guid)value : Guid.Empty;
		}
	}
}
