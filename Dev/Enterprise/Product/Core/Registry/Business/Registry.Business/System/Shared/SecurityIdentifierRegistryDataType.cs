using System;
using System.Security.Principal;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business;

public class SecurityIdentifierRegistryDataType : StringRegistryDataType
{
	protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
	{
		base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		_ = Convert(proposedValue);
	}

	protected override string DeserialiseCore(byte[] value)
	{
		var result = base.DeserialiseCore(value);
		if (!ValidationSuspended)
		{
			_ = Convert(result);
		}
		return result;
	}

	public static SecurityIdentifier Convert(string proposedValue)
	{
		if (string.IsNullOrEmpty(proposedValue))
		{
			return null;
		}
		try
		{
			var account = new NTAccount(proposedValue);
			return (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
		}
		catch (Exception e) when (!e.IsCriticalException())
		{
			throw new RegistryValidationException($"AD name '{proposedValue}' not found.", e);
		}
	}
}
