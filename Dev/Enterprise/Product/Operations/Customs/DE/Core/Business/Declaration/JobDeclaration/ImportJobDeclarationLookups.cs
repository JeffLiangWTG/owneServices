using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class ImportJobDeclarationLookups : JobDeclarationLookups
	{
		public ImportJobDeclarationLookups(JobDeclaration parent) : base(parent)
		{
		}

		public override ICodeDescriptionPairList DeclarantTypeList
		{
			get
			{
				var isINDNotAllowed = UseDeclarationTypeNotAllowingIND || (UseIntoWarehouseCPC && DoesDeclarantHaveCWPAuthorization);
				return Factory.GetCachedValue(string.Join("_", "DEJobDeclarationLookups.DeclarantTypeList", isINDNotAllowed), () =>
				{
					var result = new RepresentationTypeList();
					if (isINDNotAllowed)
					{
						result.RemoveCode(RepresentationTypeList.Codes._3Indirect);
					}
					return result;
				});
			}
		}

		protected override ZString GoodsOriginDirection
		{
			get
			{
				var entryStyle = Parent.JE_EntryStyle;
				return entryStyle == EntryStyleListImport.Codes.ImportFromSpecialTerritory || entryStyle == EntryStyleListImport.Codes.ImportFromEFTAMember ?
					(ZString)EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import : base.GoodsOriginDirection;
			}
		}

		protected override ZString GoodsDestinationDirection => EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import;

		protected override ZString GoodsDestinationCodeType => EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17;

		protected override ICollection TransportCountryListCore => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Germany,
						new ZString[] { EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM15, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15 }, ZDateTime.Today, null, false);

		protected override ZQuery OriginPortFilter() => PortQuery(string.Empty, PortLocation.All);

		protected override ZQuery FinalDestinationPortFilter() => null;

		protected override ZQuery DischargePortFilter() => null;

		bool UseDeclarationTypeNotAllowingIND => Parent.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => ImportDeclarationTypeList.IsIndirectRepresentationNotAllowed(x.CEI_Style));

		bool UseIntoWarehouseCPC => Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.EntryInstruction != null && (x.CusProcedure?.IsIntoWarehouse() ?? ZBool.False));

		bool DoesDeclarantHaveCWPAuthorization => (Parent.DeclarantOrgAddress?.Header).HasAuthorization(new ZString[] { CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP });
	}
}
