using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public static class CusEntryInstructionExtension
{
	public static bool AnyEntryInstructionIsNonWarehouseProcedure(this IEnumerable<CusEntryInstruction> entryInstructions)
	{
		Argument.NotNull(entryInstructions, nameof(entryInstructions));
		return entryInstructions.Any(x => !x.HasIntoWarehouseProcedure);
	}
	public static bool AnyEntryInstructionIsNonWarehouseProcedure(this CusEntryInstructionCollection entryInstructions)
	{
		Argument.NotNull(entryInstructions, nameof(entryInstructions));
		return entryInstructions.Cast<CusEntryInstruction>().AnyEntryInstructionIsNonWarehouseProcedure();
	}

	public static bool AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(this CusEntryInstructionCollection entryInstructions, ZGuid ownerAddressFromDeclaration)
	{
		Argument.NotNull(entryInstructions, nameof(entryInstructions));
		return entryInstructions.Cast<CusEntryInstruction>().AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ownerAddressFromDeclaration);
	}

	public static bool AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(this IEnumerable<CusEntryInstruction> entryInstructions, ZGuid ownerAddressFromDeclaration)
	{
		Argument.NotNull(entryInstructions, nameof(entryInstructions));
		return entryInstructions.Any() && (ownerAddressFromDeclaration.IsEmpty || entryInstructions.AnyEntryInstructionWithDPOAndDifferentOwnerThanDeclaration(ownerAddressFromDeclaration));
	}

	public static bool AnyEntryInstructionIsAtCustoms(this CusEntryInstructionCollection entryInstructions)
	{
		return AnyEntryInstructionIsAt(entryInstructions, SADDeclarationTypeList.Codes.ProceduraOrdinariaCODogana);
	}

	public static bool AnyEntryInstructionIsAtPlace(this CusEntryInstructionCollection entryInstructions)
	{
		return AnyEntryInstructionIsAt(entryInstructions, SADDeclarationTypeList.Codes.ProceduraOrdinariaCOLuogo);
	}

	static bool AnyEntryInstructionIsAt(this CusEntryInstructionCollection entryInstructions, ZString style)
	{
		Argument.NotNull(entryInstructions, nameof(entryInstructions));
		return entryInstructions.Cast<CusEntryInstruction>().Any(x => x.CEI_Style == style);
	}

	static bool AnyEntryInstructionWithDPOAndDifferentOwnerThanDeclaration(this IEnumerable<CusEntryInstruction> entryInstructions, ZGuid ownerPKFromDeclaration)
	{
		Argument.NotNull(entryInstructions, nameof(entryInstructions));
		return !entryInstructions.All(e => e.CusAuthorizationUsages.Any(a => a.AGC_Code == CusAuthorizationHeaderTypeList.Codes.DeferredPayment && a.AGC_OH_Owner == ownerPKFromDeclaration));
	}
}
