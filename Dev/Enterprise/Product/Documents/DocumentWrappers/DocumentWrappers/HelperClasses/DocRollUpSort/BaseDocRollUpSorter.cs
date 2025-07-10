using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.DocRollUpSort;

namespace Enterprise.DocumentWrappers.HelperClasses.DocRollUpSort
{
	abstract class BaseDocRollUpSorter<TID, TDescription, TDocHeader, TDocLineList>
		where TID : IZType
		where TDescription : IZType
		where TDocLineList : ISortableDocLineList
		where TDocHeader : IDocHeader
	{
		public BaseDocRollUpSorter(TDocHeader docHeader, BusinessObjectFactory factory)
		{
			Factory = factory;
			DocHeader = docHeader;
		}

		protected TDocHeader DocHeader { get; }

		protected BusinessObjectFactory Factory { get; }

		public TDocLineList GroupAndSortLines(ZString display, ZString style, TDocLineList linesToSort, IEnumerable<TDocLineList> linesToRollup)
		{
			var result = linesToSort;
			var groupedLines = default(TDocLineList);

			switch (display)
			{
				case OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical:
					groupedLines = linesToSort;
					groupedLines.Sort(GetAlphabeticComparer());
					break;

				case OrgConstants.GroupOrSubTotalCharges.Code.Sequence:
					groupedLines = linesToSort;
					groupedLines.Sort(GetSequenceSortComparer());
					break;

				case OrgConstants.GroupOrSubTotalCharges.Code.RollUp:
				case OrgConstants.GroupOrSubTotalCharges.Code.RollUpEntireConsol:
					groupedLines = GetLinesForRollUp(linesToRollup, style);
					if (style == OrgConstants.InvoiceLineGroupings.Code.CLC)
					{
						groupedLines.Sort(GetUserEnteredComparer());
					}
					break;

				case OrgConstants.GroupOrSubTotalCharges.Code.RollupAndSequence:
					groupedLines = GetLinesForRollUp(linesToRollup, style);
					groupedLines.Sort(GetSequenceSortComparer());
					break;

				case OrgConstants.GroupOrSubTotalCharges.Code.User:
					groupedLines = linesToSort;
					groupedLines.Sort(GetUserEnteredComparer());
					break;

				default:
					var otherResult = GroupAndSortLinesCore(display, style, linesToSort);
					groupedLines = otherResult ?? GetLinesForRollUp(linesToRollup, style);
					break;
			}

			if (groupedLines != null && groupedLines.Count > 0)
			{
				result = groupedLines;
			}

			return result;
		}

		protected virtual TDocLineList GroupAndSortLinesCore(ZString display, ZString style, TDocLineList lines) => default(TDocLineList);

		abstract protected System.Collections.IComparer GetAlphabeticComparer();

		abstract protected System.Collections.IComparer GetSequenceSortComparer();

		abstract protected System.Collections.IComparer GetUserEnteredComparer();

		#region Roll Up

		TDocLineList GetLinesForRollUp(IEnumerable<TDocLineList> groupedLines, ZString rollUpCodeFromOrganisation)
		{
			var result = GetNewLineList();

			foreach (var groupLine in groupedLines)
			{
				var rolledUpLines = GetLinesForRollUp(groupLine, rollUpCodeFromOrganisation);
				if (rolledUpLines != null)
				{
					foreach (var line in rolledUpLines)
					{
						result.Add(line);
					}
				}
			}

			return result;
		}

		protected abstract TDocLineList GetNewLineList();

		protected TDocLineList GetLinesForRollUp(TDocLineList lines, ZString organisationRollUpCode)
		{
			var result = default(TDocLineList);
			var charges = new List<ZString>();
			var mapping = new Dictionary<ZString, ZString>();

			switch (organisationRollUpCode)
			{
				case OrgConstants.InvoiceLineGroupings.Code.All:
					AddAllChargeCodesExceptCustomsDuty(charges);
					result = GetChargeDocRollUpper(DocHeader, Factory, lines, organisationRollUpCode, DocRollUpConstants.RollupAndSubTotalGroups.AllChargesExceptCustomsDutyAndTax, charges.ToArray())
						.RollUp();
					break;

				case OrgConstants.InvoiceLineGroupings.Code.OFD:
					AddChargeGroupsMapping(mapping, DocRollUpConstants.RollupAndSubTotalGroups.Origin, ChargeCodeGroupList.Codes.Origin, ChargeCodeGroupList.Codes.Loading);
					AddChargeGroupsMapping(mapping, DocRollUpConstants.RollupAndSubTotalGroups.FreightAndInsurance, ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Insurance);
					AddChargeGroupsMapping(mapping, DocRollUpConstants.RollupAndSubTotalGroups.Destination, ChargeCodeGroupList.Codes.Destination, ChargeCodeGroupList.Codes.Unloading);

					result = GetChargeDocRollUpper(DocHeader, Factory, lines, organisationRollUpCode, mapping)
						.RollUp();
					break;

				case OrgConstants.InvoiceLineGroupings.Code.OFO:
					AddChargeGroupsMapping(mapping, DocRollUpConstants.RollupAndSubTotalGroups.Origin, ChargeCodeGroupList.Codes.Origin, ChargeCodeGroupList.Codes.Loading);
					AddChargeGroupsMapping(mapping, DocRollUpConstants.RollupAndSubTotalGroups.FreightAndInsurance, ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Insurance);

					result = GetChargeDocRollUpper(DocHeader, Factory, lines, organisationRollUpCode, mapping)
						.RollUp();
					break;

				case OrgConstants.InvoiceLineGroupings.Code.OFF:
					AddChargeGroupsMapping(mapping, DocRollUpConstants.RollupAndSubTotalGroups.Origin, ChargeCodeGroupList.Codes.Origin, ChargeCodeGroupList.Codes.Loading);
					AddChargeGroupsMapping(mapping, DocRollUpConstants.RollupAndSubTotalGroups.Destination, ChargeCodeGroupList.Codes.Destination, ChargeCodeGroupList.Codes.Unloading);
					mapping.Add(DocRollUpConstants.RollupAndSubTotalGroups.Insurance, ChargeCodeGroupList.Codes.Insurance);
					mapping.Add(DocRollUpConstants.RollupAndSubTotalGroups.Freight, ChargeCodeGroupList.Codes.Freight);

					result = GetChargeDocRollUpper(DocHeader, Factory, lines, organisationRollUpCode, mapping)
						.RollUp();
					break;

				case OrgConstants.InvoiceLineGroupings.Code.OFI:
					AddChargeGroupsMapping(mapping, DocRollUpConstants.RollupAndSubTotalGroups.Origin, ChargeCodeGroupList.Codes.Origin, ChargeCodeGroupList.Codes.Loading);
					mapping.Add(DocRollUpConstants.RollupAndSubTotalGroups.Insurance, ChargeCodeGroupList.Codes.Insurance);
					mapping.Add(DocRollUpConstants.RollupAndSubTotalGroups.Freight, ChargeCodeGroupList.Codes.Freight);

					result = GetChargeDocRollUpper(DocHeader, Factory, lines, organisationRollUpCode, mapping)
						.RollUp();
					break;

				case OrgConstants.InvoiceLineGroupings.Code.AEC:
					charges.Add(ChargeCodeGroupList.Codes.Origin);
					charges.Add(ChargeCodeGroupList.Codes.Loading);
					charges.Add(ChargeCodeGroupList.Codes.Destination);
					charges.Add(ChargeCodeGroupList.Codes.Unloading);
					charges.Add(ChargeCodeGroupList.Codes.Insurance);
					charges.Add(ChargeCodeGroupList.Codes.Freight);

					result = GetChargeDocRollUpper(DocHeader, Factory, lines, organisationRollUpCode, DocRollUpConstants.RollupAndSubTotalGroups.OriginFreightInsuranceAndDestination, charges.ToArray())
						.RollUp();
					break;

				case OrgConstants.InvoiceLineGroupings.Code.OandF:
					charges.Add(ChargeCodeGroupList.Codes.Origin);
					charges.Add(ChargeCodeGroupList.Codes.Loading);
					charges.Add(ChargeCodeGroupList.Codes.Insurance);
					charges.Add(ChargeCodeGroupList.Codes.Freight);

					result = GetChargeDocRollUpper(DocHeader, Factory, lines, organisationRollUpCode, DocRollUpConstants.RollupAndSubTotalGroups.OriginFreightAndInsurance, charges.ToArray())
						.RollUp();
					break;

				case OrgConstants.InvoiceLineGroupings.Code.ORF:
					charges.Add(ChargeCodeGroupList.Codes.Origin);
					charges.Add(ChargeCodeGroupList.Codes.Freight);

					result = GetChargeDocRollUpper(DocHeader, Factory, lines, organisationRollUpCode, DocRollUpConstants.RollupAndSubTotalGroups.OriginAndFreight, charges.ToArray())
						.RollUp();
					break;

				case OrgConstants.InvoiceLineGroupings.Code.FRT:
					result = GetChargeDocRollUpper(DocHeader, Factory, lines, organisationRollUpCode, DocRollUpConstants.RollupAndSubTotalGroups.Freight, ChargeCodeGroupList.Codes.Freight)
						.RollUp();
					break;

				case OrgConstants.InvoiceLineGroupings.Code.FandD:
					charges.Add(ChargeCodeGroupList.Codes.Destination);
					charges.Add(ChargeCodeGroupList.Codes.Unloading);
					charges.Add(ChargeCodeGroupList.Codes.Insurance);
					charges.Add(ChargeCodeGroupList.Codes.Freight);

					result = GetChargeDocRollUpper(DocHeader, Factory, lines, organisationRollUpCode, DocRollUpConstants.RollupAndSubTotalGroups.FreightInsuranceAndDestination, charges.ToArray())
						.RollUp();
					break;

				case OrgConstants.InvoiceLineGroupings.Code.CCG:
					AddAllChangeCodesGroupsForIndependentRollUp(mapping);
					result = GetChargeDocRollUpper(DocHeader, Factory, lines, organisationRollUpCode, mapping)
						.RollUp();
					break;
				default:
					var otherResult = GetLinesForRollUpCore(lines, organisationRollUpCode);
					if (otherResult != null)
					{
						result = otherResult;
					}
					break;
			}

			return result;
		}

		protected virtual TDocLineList GetLinesForRollUpCore(TDocLineList lines, ZString rollUpCodeFromOrganistaion) => lines;

		protected abstract BaseDocRollUpper<TID, TDescription, TDocLineList> GetChargeDocRollUpper(TDocHeader header, BusinessObjectFactory factory, TDocLineList lines, ZString rollUpStyleId, ZString rollUpGroupId, params ZString[] chargeGroups);

		protected abstract BaseDocRollUpper<TID, TDescription, TDocLineList> GetChargeDocRollUpper(TDocHeader header, BusinessObjectFactory factory, TDocLineList lines, ZString rollUpStyleId, Dictionary<ZString, ZString> chargeGroupToGroupIdMap);

		void AddAllChargeCodesExceptCustomsDuty(List<ZString> listOfCharges)
		{
			var chargeCodes = new ChargeCodeGroupList();
			foreach (ICodeDescription codeDescription in chargeCodes)
			{
				if (codeDescription.Code != ChargeCodeGroupList.Codes.CustomsDuty)
				{
					listOfCharges.Add(codeDescription.Code);
				}
			}
		}

		void AddChargeGroupsMapping(Dictionary<ZString, ZString> mapping, ZString groupId, params ZString[] chargeGroups)
		{
			foreach (var chargeGroup in chargeGroups)
			{
				mapping.Add(chargeGroup, groupId);
			}
		}

		void AddAllChangeCodesGroupsForIndependentRollUp(Dictionary<ZString, ZString> mapping)
		{
			var chargeCodeGroups = new ChargeCodeGroupList();
			foreach (ICodeDescription group in chargeCodeGroups)
			{
				mapping.Add(group.Code, group.Code);
			}
		}

		#endregion
	}
}
