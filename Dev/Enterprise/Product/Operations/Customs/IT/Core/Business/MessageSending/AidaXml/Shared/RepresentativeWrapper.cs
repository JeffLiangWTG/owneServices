using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public class RepresentativeWrapper : TraderWrapper, IRepresentative
{
	RepresentativeWrapper(JobDeclaration declaration)
		: base(declaration?.Representative)
	{
		this.declaration = declaration;

		lazyType = new Lazy<int?>(GetRepresentativeType);
		lazyIdentificationNumber = new Lazy<string>(GetIdentificationNumber);
	}

	readonly JobDeclaration declaration;

	public static RepresentativeWrapper NewOrNull(JobDeclaration declaration)
	{
		Argument.NotNull(declaration, nameof(declaration));

		if (declaration.JE_DeclarantType.IsEmpty && declaration.Representative is null)
		{
			return null;
		}
		return new RepresentativeWrapper(declaration);
	}

	int? IRepresentative.RepresentativeType => lazyType.Value;
	readonly Lazy<int?> lazyType;

	string ITrader.IdentificationNumber => lazyIdentificationNumber.Value;
	readonly Lazy<string> lazyIdentificationNumber;

	#region Implementation

	int? GetRepresentativeType()
	{
		if (IsDirectOrIndirectRepresentative && int.TryParse(CustomsRulesProvider.ConvertRepresentativeTypeToItalianCustomsFormat(DeclarantType), out var result))
		{
			return result;
		}
		return null;
	}

	string GetIdentificationNumber()
	{
		return IsDirectOrIndirectRepresentative
			? IdentificationNumber
			: null;
	}

	bool IsDirectOrIndirectRepresentative => DeclarantType == RepresentationTypeList.Codes._2Direct || declarantType == RepresentationTypeList.Codes._3Indirect;

	ZString DeclarantType => declarantType ?? (declarantType = declaration.JE_DeclarantType);
	string declarantType;

	#endregion
}
