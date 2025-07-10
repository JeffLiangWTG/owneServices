using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public sealed class TermsOfDeliveryWrapper : ITermsOfDelivery
{
	TermsOfDeliveryWrapper(JobComInvoiceHeader invoiceHeader)
	{
		this.invoiceHeader = invoiceHeader;
		lazyIsAgreedPlaceCodeValidUNLocode = new Lazy<bool>(GetIsAgreedPlaceCodeValidUNLocode);
		lazyAdditionalTerms = new Lazy<string>(GetAdditionalTerms);
	}

	readonly JobComInvoiceHeader invoiceHeader;
	readonly Lazy<bool> lazyIsAgreedPlaceCodeValidUNLocode;
	readonly Lazy<string> lazyAdditionalTerms;

	public static TermsOfDeliveryWrapper NewOrNull(JobComInvoiceHeader invoiceHeader)
	{
		Argument.NotNull(invoiceHeader, nameof(invoiceHeader));

		if (invoiceHeader.JZ_IncoTerm.IsEmpty && invoiceHeader.JZ_IncoTermPlace.IsEmpty && invoiceHeader.ZG_AgreedPlaceCode.IsEmpty)
		{
			return null;
		}
		return new TermsOfDeliveryWrapper(invoiceHeader);
	}

	string ITermsOfDelivery.CountryCode => !IsAgreedPlaceCodeValidUNLocode ? AgreedPlaceCode.SubstringSafe(0, 2) : ZString.Empty;

	string ITermsOfDelivery.IncotermCode => IncotermCode;

	string ITermsOfDelivery.Location => invoiceHeader.JZ_IncoTermPlace;

	string ITermsOfDelivery.UNLocode => IsAgreedPlaceCodeValidUNLocode ? AgreedPlaceCode : ZString.Empty;

	string ITermsOfDelivery.AdditionalTerms => lazyAdditionalTerms.Value;

	#region Implementation

	bool IsAgreedPlaceCodeValidUNLocode => lazyIsAgreedPlaceCodeValidUNLocode.Value;

	bool GetIsAgreedPlaceCodeValidUNLocode()
	{
		return invoiceHeader.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, invoiceHeader.ZG_AgreedPlaceCode)?.IsActive ?? false;
	}

	string GetAdditionalTerms()
	{
		return IncotermCode == Core.Constants.IncoTerms.Other
			? invoiceHeader.JZ_AdditionalTerms
			: ZString.Empty;
	}

	ZString AgreedPlaceCode => invoiceHeader.ZG_AgreedPlaceCode;

	ZString IncotermCode => invoiceHeader.JZ_IncoTerm;

	#endregion
}
