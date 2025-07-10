using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobDeclarationGoodsLocationFieldsDefaulter
{
	public JobDeclarationGoodsLocationFieldsDefaulter(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}
	readonly JobDeclaration declaration;

	public void AssignOfficeOfPresentationToLocationOfGoodsIfAvailable()
	{
		var officeOfPresentation = declaration.JE_CustomsOffice;
		if (!officeOfPresentation.IsEmpty)
		{
			declaration.JE_LocationOfGoods = officeOfPresentation;
		}
	}

	public void EmptyGoodsLocationFields()
	{
		declaration.JE_LocationQualifier = ZString.Empty;
		EmptyGoodsLocationSubFields();
	}

	public void EmptyGoodsLocationSubFields()
	{
		declaration.JE_SubLocationOfGoods = ZString.Empty;
		declaration.GoodsLocationAddress.OrganisationPK = ZGuid.Empty;
		declaration.JE_LocationOfGoods = ZString.Empty;
		declaration.JE_LocationOtherInformation = ZString.Empty;
	}

	public void DefaultLocationOfGoodsIfNeeded()
	{
		var locationsCount = declaration.Lookups.Locations.Count;

		if (Authorisation != null && locationsCount == 1)
		{
			declaration.JE_LocationOfGoods = ((CodeDescriptionPairList)declaration.Lookups.Locations)[0].Code;
		}
		else if (Authorisation == null)
		{
			EmptyGoodsLocationFields();
		}
		else if (locationsCount == 0)
		{
			EmptyGoodsLocationSubFields();
		}
		SetLocationQualifierIfEmptyBasedOnAuthorizationType();
	}

	void SetLocationQualifierIfEmptyBasedOnAuthorizationType()
	{
		if (declaration.JE_LocationQualifier.IsEmpty && Authorisation != null)
		{
			if (Authorisation.SupportImportLocationQualifierLB())
			{
				declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace;
			}
			else if (Authorisation.SupportImportLocationQualifierLC())
			{
				declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace;
			}
		}
	}

	CusAuthorisationHeader Authorisation => declaration.Authorization;
}
