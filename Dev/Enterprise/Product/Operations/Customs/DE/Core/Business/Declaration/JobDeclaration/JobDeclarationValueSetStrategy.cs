using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	sealed class JobDeclarationValueSetStrategy : EU.Business.Declaration.JobDeclarationValueSetStrategy
	{
		internal JobDeclarationValueSetStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);

			switch (valueThatHasChanged.Name)
			{
				case JobDeclaration.Schema.JE_MessageType:
					UpdateAddressesDependingOnDeclarantType(Declaration.JE_DeclarantType);
					break;
				case JobDeclaration.Schema.JE_DeclarantType:
					UpdateAddressesDependingOnDeclarantType((ZString)valueThatHasChanged.Value);
					break;
			}
		}

		protected override void DefaultImporterChanged()
		{
			base.DefaultImporterChanged();

			UpdateFinalDestinationPortFromImporter();
		}

		internal void UpdateAddressesDependingOnDeclarantType(ZString declarantType, bool forceUpdateAddress = true)
		{
			if (Declaration.IsImport)
			{
				if (declarantType == RepresentationTypeList.Codes._1Self)
				{
					UpdateAddressInfoWithOrgProxy(Declaration.JE_OA_DeclarantAddressInfo, forceUpdateAddress);
				}
				else if (declarantType == RepresentationTypeList.Codes._2Direct)
				{
					UpdateAddressInfoWithOrgProxy(Declaration.JE_OA_RepresentativeInfo, forceUpdateAddress);
					if (!Declaration.ImporterDocumentaryAddress.IsEmpty)
					{
						Declaration.JE_OA_DeclarantAddress = Declaration.ImporterDocumentaryAddress.E2_OA_Address;
					}
				}
				else if (declarantType == RepresentationTypeList.Codes._3Indirect)
				{
					UpdateAddressInfoWithOrgProxy(Declaration.JE_OA_DeclarantAddressInfo, forceUpdateAddress);
					if (!Declaration.ImporterDocumentaryAddress.IsEmpty)
					{
						Declaration.JE_OA_BuyingAgentAddress = Declaration.ImporterDocumentaryAddress.E2_OA_Address;
					}
				}
			}
		}

		internal void UpdateFinalDestinationPortFromImporter()
		{
			if (Declaration.IsImport)
			{
				var importer = Declaration.Importer;
				if (importer != null)
				{
					var importerPort = importer.OH_RL_NKClosestPort;
					if (importerPort.StartsWith(Declaration.CountryCode, StringComparison.CurrentCulture) && Declaration.JE_RL_NKFinalDestination != importerPort)
					{
						Declaration.JE_RL_NKFinalDestination = importerPort;
					}
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
}
