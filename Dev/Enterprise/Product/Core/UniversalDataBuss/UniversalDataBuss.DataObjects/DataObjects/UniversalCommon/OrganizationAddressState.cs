using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code", true)]
	public class OrganizationAddressState : ICodeDataObject
	{
		[Mandatory, MaxLength(25)]
		public ZString? Code { get; set; }

		[MaxLength(80)]
		public ZString? Description { get; set; }

		public static OrganizationAddressState New(ZString? state, Func<ZString, ZString?> descriptionFunc)
		{
			return state.HasValue ? new OrganizationAddressState { Code = state, Description = !state.Value.IsEmpty ? descriptionFunc?.Invoke(state.Value) : null } : null;
		}

		public static implicit operator OrganizationAddressState(ZString? state)
		{
			return state.HasValue ? new OrganizationAddressState { Code = state } : null;
		}

		public static implicit operator OrganizationAddressState(string state)
		{
			return state != null ? new OrganizationAddressState { Code = state } : null;
		}

		public static implicit operator ZString?(OrganizationAddressState organizationState)
		{
			return organizationState?.Code;
		}

		public static implicit operator string(OrganizationAddressState organizationState)
		{
			return organizationState?.Code;
		}

		public static bool operator ==(OrganizationAddressState a, ZString? b)
		{
			return (ZString?)a == b;
		}

		public static bool operator !=(OrganizationAddressState a, ZString? b)
		{
			return (ZString?)a != b;
		}

		public static bool operator ==(ZString? a, OrganizationAddressState b)
		{
			return a == (ZString?)b;
		}

		public static bool operator !=(ZString? a, OrganizationAddressState b)
		{
			return a != (ZString?)b;
		}

		public static bool operator ==(OrganizationAddressState a, string b)
		{
			return (ZString?)a == (ZString?)b;
		}

		public static bool operator !=(OrganizationAddressState a, string b)
		{
			return (ZString?)a != (ZString?)b;
		}

		public static bool operator ==(string a, OrganizationAddressState b)
		{
			return (ZString?)a == (ZString?)b;
		}

		public static bool operator !=(string a, OrganizationAddressState b)
		{
			return (ZString?)a != (ZString?)b;
		}

		public static bool operator ==(OrganizationAddressState a, OrganizationAddressState b)
		{
			return (ZString?)a == (ZString?)b;
		}

		public static bool operator !=(OrganizationAddressState a, OrganizationAddressState b)
		{
			return (ZString?)a != (ZString?)b;
		}

		public override bool Equals(object obj)
		{
			return (obj as OrganizationAddressState == this)
				|| (obj as string == this)
				|| (obj as ZString? == Code);
		}

		public bool IsEmpty()
		{
			return ((IZType)Code).IsEmpty;
		}

		public override int GetHashCode()
		{
			return Code.GetHashCode();
		}

		public override string ToString() => Code.GetValueOrDefault();
	}
}
