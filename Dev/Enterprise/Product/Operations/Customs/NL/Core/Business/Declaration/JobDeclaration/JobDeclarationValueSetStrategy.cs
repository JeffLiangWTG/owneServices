using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration;

public class JobDeclarationValueSetStrategy : EU.Business.Declaration.JobDeclarationValueSetStrategy
{
	public JobDeclarationValueSetStrategy(JobDeclaration declaration)
		: base(declaration)
	{
	}

	protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
	{
		base.ValueSetCore(valueThatHasChanged, oldValue);

		if (valueThatHasChanged.Name == JobDeclaration.Schema.JE_DeclarantType)
		{
			UpdateAddressesDependingOnDeclarantType((ZString)valueThatHasChanged.Value);
		}
	}

	internal void UpdateAddressesDependingOnDeclarantType(ZString declarantType, bool forceUpdateAddress = true)
	{
		if (Declaration.IsExport)
		{
			if (declarantType == RepresentationTypeList.Codes._1Self)
			{
				UpdateAddressInfoWithOrgProxy(Declaration.JE_OA_DeclarantAddressInfo, forceUpdateAddress);
				Declaration.ExporterDocAddress.OrganisationPK = Declaration.Branch?.OrgProxy?.PK ?? ZGuid.Empty;
				Declaration.JE_OA_Representative = ZGuid.Empty;
				Declaration.JE_OA_Representative_ZAddress.OrgPK = ZGuid.Empty;
			}
			else if (declarantType == RepresentationTypeList.Codes._2Direct)
			{
				if (!Declaration.SupplierDocumentaryAddress.IsEmpty)
				{
					Declaration.JE_OA_DeclarantAddress = Declaration.SupplierDocumentaryAddress.E2_OA_Address;
				}
				UpdateAddressInfoWithOrgProxy(Declaration.JE_OA_RepresentativeInfo, forceUpdateAddress);
				Declaration.ExporterDocAddress.OrganisationPK = Declaration.SupplierDocumentaryAddress.OrganisationPK;
			}
			else if (declarantType == RepresentationTypeList.Codes._3Indirect)
			{
				UpdateAddressInfoWithOrgProxy(Declaration.JE_OA_DeclarantAddressInfo, forceUpdateAddress);
				Declaration.ExporterDocAddress.OrganisationPK = Declaration.SupplierDocumentaryAddress.OrganisationPK;
				UpdateAddressInfoWithOrgProxy(Declaration.JE_OA_RepresentativeInfo, forceUpdateAddress);
			}
		}
	}

	void UpdateAddressInfoWithOrgProxy(ZPropertyInfo property, bool forceUpdateValue)
	{
		if (forceUpdateValue || property is ZPropertyInfoGuid guidInfo && guidInfo.Value.IsEmpty)
		{
			var orgProxyMainAddressPK = Declaration.Branch.OrgProxy?.MainAddress?.PK ?? ZGuid.Empty;
			property.Value = orgProxyMainAddressPK;
		}
	}

	new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
}
