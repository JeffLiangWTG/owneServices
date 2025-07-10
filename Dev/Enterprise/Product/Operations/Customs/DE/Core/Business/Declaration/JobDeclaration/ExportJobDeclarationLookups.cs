using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class ExportJobDeclarationLookups : JobDeclarationLookups
	{
		public ExportJobDeclarationLookups(JobDeclaration parent) : base(parent)
		{
		}

		public override CustomsOfficeCodeCollection CustomsOffices
		{
			get
			{
				var declaration = Parent;
				var role = new ZString[] { declaration.CustomsOfficeRequirementHelper.MainOffice?.OfficeRole ?? ZString.Empty };

				if (declaration.IsExport)
				{
					role = new ZString[] { declaration.CustomsOfficeRequirementHelper.MainOffice?.OfficeRole ?? ZString.Empty, EuOfficeCodesTypes.Codes.OfficeOfExit };
				}
				var isLocalCountryOnly = Parent.CustomsOfficeRequirementHelper.MainOffice?.IsLocalCountryOnly ?? false;
				var isForeignCountryOnly = Parent.CustomsOfficeRequirementHelper.MainOffice?.IsForeignCountryOnly ?? false;
				return isLocalCountryOnly
					? CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Parent.CountryCode, role)
					: (isForeignCountryOnly
						? EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRolesExceptLocal(Factory, Parent.CountryCode, role)
						: EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, role));
			}
		}

		protected override ZString EntryStatusListCodeType => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus;

		protected override ZQuery OriginPortFilter() => PortQuery(string.Empty, PortLocation.All);

		protected override ZString GoodsOriginDirection => GetOriginAndDestinationDirection();

		protected override ZString GoodsDestinationDirection => GetOriginAndDestinationDirection();

		protected override ZQuery FinalDestinationPortFilter() => PortQuery(string.Empty, PortLocation.All);

		ZString GetOriginAndDestinationDirection()
		{
			var entryStyle = Parent.JE_EntryStyle;
			return entryStyle == EntryStyleListExport.Codes.ExportToSpecialTerritory || entryStyle == EntryStyleListExport.Codes.ExportToEFTAMember ?
				(ZString)EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export : base.GoodsOriginDirection;
		}
	}
}
