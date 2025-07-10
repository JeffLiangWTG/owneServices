using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
namespace Enterprise.Customs.IT.Business;

public class DispatchCountryCodeResolver
{
	public DispatchCountryCodeResolver(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
	}

	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;
	readonly CusEntryInstruction entryInstruction;

	public ZString GetDispatchCountryCodeForHeader()
	{
		var dispatchCountryCode = declaration.JE_RL_NKOrigin.Left(2);
		if (IsParticipantBuyerConsole)
		{
			if (HasMoreThanOneSupplierDistinctCountryCodes)
			{
				dispatchCountryCode = ZString.Empty;
			}
			else if (dispatchCountryCode.IsEmpty && SupplierDistinctCountryCodes.Any())
			{
				dispatchCountryCode = SupplierDistinctCountryCodes.Single();
			}
		}
		return dispatchCountryCode;
	}

	public ZString GetDispatchCountryCodeForLine(CusEntryLine entryLine)
	{
		var dispatchCountryCode = ZString.Empty;
		if (IsParticipantBuyerConsole && HasMoreThanOneSupplierDistinctCountryCodes)
		{
			dispatchCountryCode = entryLine.RandomLine.InvoiceHeader?.Supplier.MainAddress.OA_RN_NKCountryCode ?? ZString.Empty;
		}
		return dispatchCountryCode;
	}

	#region Implementation

	IEnumerable<ZString> SupplierDistinctCountryCodes => supplierDistinctCountryCodes ?? (supplierDistinctCountryCodes = entryHeader.Suppliers.Cast<OrgHeader>().Select(x => x.MainAddress.OA_RN_NKCountryCode).Distinct());
	IEnumerable<ZString> supplierDistinctCountryCodes;
	ZBool HasMoreThanOneSupplierDistinctCountryCodes => (hasMoreThanOneSupplierDistinctCountryCodes ?? (hasMoreThanOneSupplierDistinctCountryCodes = SupplierDistinctCountryCodes.Count() > 1)).Value;
	bool? hasMoreThanOneSupplierDistinctCountryCodes;
	ZBool IsParticipantBuyerConsole => entryInstruction.ZG_ParticipantType == ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;

	#endregion
}
