using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class EXSHeaderWrapper : IEXSHeader
	{
		public EXSHeaderWrapper(CusEntryHeader cusEntryHeader, ZString mopValueForHeader, string messageSubType = null)
		{
			entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			jobDeclaration = Argument.NotNull(cusEntryHeader.Declaration, nameof(entryHeader.Declaration));
			mesSubType = messageSubType;
			this.mopValueForHeader = mopValueForHeader;
		}
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration jobDeclaration;
		readonly ZString mesSubType;
		readonly ZString mopValueForHeader;

		public ZString ReferenceNumber => entryHeader.CH_BGMReference;

		public ZString GoodsLocation
		{
			get
			{
				var result = entryHeader.EntryInstruction.GoodsLocation.Address.AuthorisationNumber;
				return result.Length > 10 ? result.SubstringSafe(4) : result;
			}
		}

		public ZInt TotalLinesNum => entryHeader.MergedLines.Count;

		public ZInt TotalPackagesQty => entryHeader.PackagesCount;

		public ZDecimal TotalGrossWeight => (ZDecimal)entryHeader.MergedLines.Sum(x => x.EffectiveGrossWeight.InKilogramsSafe);

		public ZDateTime DeclarationDate => ZDateTime.Now;

		public ZString DeclarationPlace
		{
			get
			{
				var agent = jobDeclaration.JE_GS_NKCusAgent;
				return !agent.IsEmpty ? jobDeclaration.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, agent)?.GS_City ?? ZString.Empty : ZString.Empty;
			}
		}

		public ZString SpecificCircumstanceInd => specificCircumstanceIndicatorAcceptedCodesList.Contains(jobDeclaration.ZG_SpecificCircumstanceIndicator) ? jobDeclaration.ZG_SpecificCircumstanceIndicator : ZString.Empty;

		public ZString NatSpecificCircumstanceInd => ZString.Empty;

		public ZString DocumentOperationIndicator
		{
			get
			{
				var indicador = ZString.Empty;
				if (mesSubType == DeclarationMessageSubTypeList.Codes.Amendment)
				{
					indicador = Modificacion;
				}
				else if (mesSubType == DeclarationMessageSubTypeList.Codes.Cancellation)
				{
					indicador = Anulacion;
				}
				return indicador;
			}
		}

		public ZString DocumentReferenceNumber => !DocumentOperationIndicator.IsEmpty ? entryHeader.MovementReferenceNumber : ZString.Empty;

		public ZString MethodOfPayment => mopValueForHeader;

		readonly List<string> specificCircumstanceIndicatorAcceptedCodesList = new List<string>() { SpecificCircumstanceIndicator.Codes.PostalAndExpressConsignments, SpecificCircumstanceIndicator.Codes.ShipAndAircraftSupplies, SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators };

		const string Modificacion = "M";
		const string Anulacion = "A";
	}
}
