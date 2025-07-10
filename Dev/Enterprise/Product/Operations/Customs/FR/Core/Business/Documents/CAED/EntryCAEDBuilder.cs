using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class EntryCAEDBuilder : CAEDBuilder
	{
		public EntryCAEDBuilder(CusEntryHeader header) : base(header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		readonly new CusEntryHeader header;

		JobDeclaration Declaration => header.Declaration;
		CusEntryInstruction EntryInstruction => header.EntryInstruction;

		protected override ZString PortOfDispatch => Declaration.IsImport ? Declaration.JE_RL_NKPortOfArrival : Declaration.JE_RL_NKPortOfLoading;

		protected override ZString JobNumber => header.CH_BGMReference;

		protected override ZString DeclarationType => Declaration.JE_EntryStyle + EntryInstruction?.CEI_SubStyle;

		protected override ZString DeclarationStatus => header.CH_EntryStatus;

		protected override ICodeDescription CustomsOfficeCodeOfDeparture => GetOffice();

		protected override ICodeDescription PackageType => GetPackType();

		protected override ZInt TotalNumberOfPacks => header.PackagesCount;

		protected override List<ZString> Containers => GetContainer();

		protected override ZString ContainerNumbers => GetContainerNumber();

		protected override ZString DeclarantsSIRETNumber => GetDeclarantsSIRETNumber();

		protected override ZString CommonAccessRef => GetCommonAccessRef();

		protected override ZString Port => Declaration.JE_RL_NKPortOfArrival;

		protected override ZDecimal PortDuesAmount => header.Charges.Cast<CusEntryHeaderCharges>().Sum(x => x.C1_ChargeAmount);

		protected override ZString CTOPartySICCode => Declaration.JE_SubLocationOfGoods;

		ZString GetCommonAccessRef()
		{
			var result = ZString.Empty;

			var rcaEntryNum = Declaration.AdditionalReferenceNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == FranceAdditionalReferenceNumberTypes.Codes.CommonAccessReference);
			if (rcaEntryNum != null)
			{
				result = rcaEntryNum.CE_EntryNum;
			}
			else
			{
				var previousDocument = Declaration.PreviousDocuments.Cast<PreviousDocument>().FirstOrDefault(x => x.CSI_Code == PreviousDocumentCodeList.Codes.ZZZ && x.CSI_SubType == PreviousDocumentClassList.Codes.PreviousDocument);
				if (previousDocument != null)
				{
					result = previousDocument.CSI_ReferenceNumber;
				}
			}
			return result;
		}

		ICodeDescription GetOffice()
		{
			CodeDescription result;

			var office = Declaration.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => (Declaration.IsImport && x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent)
				|| (!Declaration.IsImport && x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit));

			if (office != null)
			{
				var departureOfficeCodes = office.Lookups.OfficeCodeList;

				result = new CodeDescription(departureOfficeCodes)
				{
					Code = office.CY_Data
				};
			}
			else
			{
				result = new CodeDescription(new CodeDescriptionPairList())
				{
					Code = ZString.Empty
				};
			}

			return result;
		}

		ICodeDescription GetPackType()
		{
			var package = header.Packages.FirstOrDefault();
			return new CodeDescription(Declaration.Lookups.PackingUnitTypesList ?? new CodeDescriptionPairList())
			{
				Code = package?.CW_PackType ?? ZString.Empty
			};
		}

		IEnumerable<Customs.Business.BaseCusContainer> ContainersList => header.Containers.Where(x => !x.CO_ContainerNumber.IsEmpty);

		List<ZString> GetContainer()
		{
			var listofNumber = new List<ZString>();
			foreach (var container in ContainersList)
			{
				listofNumber.Add(container.CO_ContainerNumber);
			}
			return listofNumber;
		}

		ZString GetContainerNumber()
		{
			var number = new ZStringBuilder();
			foreach (var container in ContainersList)
			{
				number.Append(container.CO_ContainerNumber);
			}
			return number.ToStringWithDelimiterBetweenAppends(" / ");
		}

		ZString GetDeclarantsSIRETNumber()
		{
			var declarant = Declaration.Declarant?.Header;
			var siret = ZString.Empty;
			if (declarant != null)
			{
				siret = EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(declarant, OrgCusCode.FranceCodeTypes.Siret);
			}
			return siret;
		}
	}
}
