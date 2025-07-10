using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEVATDeferStrategy : VATDeferStrategy
	{
		public DeltaIEVATDeferStrategy(JobDeclaration declaration) : base(declaration)
		{
		}

		public override void OnDeclarantTypeChanged()
		{
			DefaultDefermentAccountNumber();
		}

		protected override OrgHeader GetSourceForDeferment(ZString deferType)
		{
			OrgHeader source = null;
			var declaration = Declaration;
			if (declaration.IsImport)
			{
				var declarantType = declaration.JE_DeclarantType;
				if (declarantType == RepresentationTypeList.Codes.DIR)
				{
					source = declaration.Importer;
				}
				else if (declarantType == RepresentationTypeList.Codes.IND || declarantType == RepresentationTypeList.Codes.SEL)
				{
					source = declaration.Declarant?.Header;
				}
			}

			if (source == null)
			{
				source = base.GetSourceForDeferment(deferType);
			}
			return source;
		}

		protected override ZString GetDefermentAccountNumber(OrgHeader header)
		{
			var declaration = Declaration;
			if (declaration.IsImport)
			{
				return declaration.Lookups.DefermentCustomsGuarantees.FirstOrDefault(g => g.CPH_OH_PermitHolder == header?.PK)?.CPH_Number ?? ZString.Empty;
			}
			return base.GetDefermentAccountNumber(header);
		}
	}
}
