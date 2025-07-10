using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class NctsCAEDBuilder : CAEDBuilder
	{
		public NctsCAEDBuilder(NctsHeader header) : base(header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}
		readonly new NctsHeader header;

		protected override ZString PortOfDispatch => header.PortOfDispatch;

		protected override ZString JobNumber => header.JobNumber;

		protected override ZString DeclarationType => header.MovementHeader is NctsDepartureMovementHeader movementHeader ? movementHeader.BM_InBondEntryType : ZString.Empty;

		protected ZString bAEConstant = new ZString("BAE");

		protected override ZString DeclarationStatus => header.MovementHeader is NctsDepartureMovementHeader movementHeader
			&& movementHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture ? bAEConstant : ZString.Empty;

		protected override ICodeDescription CustomsOfficeCodeOfDeparture => GetCustomsOfficeCodeOfDeparture();

		protected override ICodeDescription PackageType
		{
			get
			{
				if (packages?.FirstOrDefault() is NctsPackage package)
				{
					var unitType = !header.IsPhase4 && packages.Any(p => p.B5_UnitType != package.B5_UnitType) ? (ZString)"MLT" : package.B5_UnitType;
					return new CodeDescription(package.Lookups.UnitTypeList) { Code = unitType };
				}
				return null;
			}
		}

		protected override ZInt TotalNumberOfPacks => packages?.Sum(y => (int)y.B5_UnitCount) ?? 0;

		protected override List<ZString> Containers => GetContainer();

		protected override ZString ContainerNumbers => GetContainerNumber();

		protected override ZString DeclarantsSIRETNumber => GetDeclarantsSIRETNumber();

		protected override ZString CommonAccessRef
		{
			get
			{
				var result = ZString.Empty;

				if (header.MovementHeader is NctsDepartureMovementHeader movementHeader)
				{
					if (header.IsPhase4)
					{
						result = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().SelectMany(x => x.PreviousDocuments.Cast<NctsPreviousDocument>()).FirstOrDefault(x => x.CSI_Code == PreviousDocumentCodeList.Codes.ZZZ)?.CSI_ReferenceNumber ?? ZString.Empty;
					}
					else
					{
						result = movementHeader.SupportingDocuments.FirstOrDefault(x => x.CSI_Code == FRConstants.CAED.CommonAccessDocumentCode)?.CSI_ReferenceNumber ?? ZString.Empty;
					}
				}
				return result;
			}
		}

		IEnumerable<NctsPackage> packages
		{
			get
			{
				if (header.MovementHeader is NctsDepartureMovementHeader movementHeader)
				{
					var goodsItems = header.IsPhase4 ? movementHeader.GoodsItems : header.Bills.SelectMany(x => x.GoodsItems);
					return goodsItems.SelectMany(x => x.Packages).Cast<NctsPackage>();
				}

				return null;
			}
		}

		IEnumerable<EU.NCTS.Business.NctsDepartureHeaderContainer> ContainersList => header.DepartureHeaderContainers?.Cast<EU.NCTS.Business.NctsDepartureHeaderContainer>().Where(x => !x.BC_ContainerNum.IsEmpty);

		protected override ZString Port => header.BH_RL_NKImportLoadPort;

		protected override ZDecimal PortDuesAmount
		{
			get
			{
				var amount = 0m;

				if (header.MovementHeader is NctsDepartureMovementHeader movementHeader)
				{
					var goodsItems = header.IsPhase4 ? movementHeader.GoodsItems : header.Bills.SelectMany(x => x.GoodsItems);
					return goodsItems.Sum(x => x.HarbourTaxAmountInLocalCurrency);
				}

				return amount;
			}
		}

		protected override ZString CTOPartySICCode => ZString.Empty;

		List<ZString> GetContainer()
		{
			var listofNumber = new List<ZString>();
			foreach (var container in ContainersList)
			{
				listofNumber.Add(container.BC_ContainerNum);
			}
			return listofNumber;
		}

		ZString GetContainerNumber()
		{
			var number = new ZStringBuilder();
			foreach (var container in ContainersList)
			{
				number.Append(container.BC_ContainerNum);
			}
			return number.ToStringWithDelimiterBetweenAppends(" / ");
		}

		ZString GetDeclarantsSIRETNumber()
		{
			var declarant = header.IsPhase4 ? header.Declarant?.Organisation : header.CommonMovementHeader.Representative?.Organisation ?? header.Principal?.Organisation;
			var siret = ZString.Empty;
			if (declarant != null)
			{
				siret = EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(declarant, OrgCusCode.FranceCodeTypes.Siret);
			}
			return siret;
		}

		ICodeDescription GetCustomsOfficeCodeOfDeparture()
		{
			var customsOffices = header.IsPhase5 ? header.CommonMovementHeader.CustomsOffices : header.CustomsOffices;
			var departureOffice = customsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture);

			var departureOfficeCodes = departureOffice?.Lookups.OfficeCodeList ?? EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(context.Factory, EuOfficeCodesTypes.Codes.OfficeOfDeparture);

			return new CodeDescription(departureOfficeCodes)
			{
				Code = departureOffice?.CY_Data ?? ZString.Empty
			};
		}
	}
}
