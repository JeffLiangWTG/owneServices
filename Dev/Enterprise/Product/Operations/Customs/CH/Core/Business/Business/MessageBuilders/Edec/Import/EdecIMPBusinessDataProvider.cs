using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class EdecIMPBusinessDataProvider : EdecBusinessDataProvider
{
	public static EdecIMPBusinessDataProvider New(CusEntryHeader entryHeader) => entryHeader == null ? null : new EdecIMPBusinessDataProvider(entryHeader);

	EdecIMPBusinessDataProvider(CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	public override string CustomsAccount => GetAccount(Declaration.JE_PaymentMethod, () => Declaration.DutyPaidByAccountNo);

	public override string VATAccount => GetAccount(Declaration.JE_VATPaidBy, () => Declaration.VATPaidByAccountNo);

	string GetAccount(ZString paymentMethod, Func<ZString> accountNoGetter)
	{
		ZString account;
		if (paymentMethod == DeclarationPayerList.Codes.Cash)
		{
			account = "0";
		}
		else
		{
			account = accountNoGetter();
			if (account.IsEmpty)
			{
				account = "-1";
			}
		}
		return account;
	}

	protected override OrgHeader VATOrganisation => Declaration.Importer;
}
