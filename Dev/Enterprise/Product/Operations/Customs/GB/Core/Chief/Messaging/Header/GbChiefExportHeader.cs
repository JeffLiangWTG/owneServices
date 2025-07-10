using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Messaging;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.Chief.Messaging
{
	public class GbChiefExportHeader : GbChiefHeader, IExportHeader, IUkCinvWrapper
	{
		public GbChiefExportHeader(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public override ZString JobType => "E";

		protected override IDocAddress Customer => ShipperDocAddress;

		public ZBool RequestProgressReport => GB.Registry.GBCustomsDataRegistry.Instance.ProcInst_PRG.Value;

		public ZBool FECCountryOfDestination => EntryHeader.Declaration.JE_FecDST;

		public ZBool FECProgressReport => ZBool.True;

		public ZString CTStatusID => EntryHeader.Declaration.ZG_CTStatusID;

		protected override GbLine GetNewGbLine(CusEntryLine entryLine)
		{
			return new GbChiefExportLine(this, (Business.Declaration.CusEntryLine)entryLine);
		}

		#region Unused EU.Intergration.SadH.IHeader members

		public IOrganisation NotifyParty => null;

		#endregion

		public override ZString HMRC_ASG_CODE(CusDecMessageTypeFunction originalOrReplacement)
		{
			int asnCode = 0;
			if (IsELP)
			{ asnCode = 734; }
			else if (IsESP)
			{ asnCode = 736; }
			else if (IsESD)
			{ asnCode = 741; }
			else if (IsEFD)
			{ asnCode = 730; }
			else if (IsECR)
			{ asnCode = 738; }
			else if (IsEXS)
			{ asnCode = 732; }

			switch (originalOrReplacement)
			{
				case CusDecMessageTypeFunction.Original:
					return asnCode.ToString("D3");
				case CusDecMessageTypeFunction.Replacement:
					return (asnCode + 1).ToString("D3");
				case CusDecMessageTypeFunction.Delete:
					return "760"; // delete
				default:
					return ZString.Empty;
			}
		}

		public ZString MasterOpt => ZString.Empty;

		/// <summary>
		/// For ECS. Already converted (e.g. PR-->US)
		/// </summary>
		public new List<ZString> CountriesOfRouting  //ROUTE-CNTRY (many-of)
		{
			get
			{
				return EntryHeader.CountriesOfRouting;
			}
		}

		public ZString MovementReference
		{
			get
			{
				var result = ZString.Empty;
				var arrivalDateTime = DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises;
				if (!arrivalDateTime.IsEmpty)
				{
					result = arrivalDateTime.ToString("ddMMMHHmm", CultureInfo.InvariantCulture);
				}
				return result;
			}
		}

		public ZBool HasDeclarationUCRPartSuffix => !EntryHeader.DeclarationUCRPartSuffix.IsEmpty;

		public ZString CDSDeclarationUniqueConsignmentReference => CusEntryHeader.UCRReferencePlaceHolder;

		public ZString CDSDeclarationUniqueConsignmentReferencePartSuffix => CusEntryHeader.UCRPartPlaceHolder;
	}
}
