using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers
{
	public class GbCDSExportEntryHeaderWrapper : GbCDSImportEntryHeaderWrapper, IUkCinvWrapper
	{
		public GbCDSExportEntryHeaderWrapper(CusEntryHeader entryHeader) : base(entryHeader)
		{
			ExportHeaderWrapper = new GbCDSExportHeader(entryHeader);
		}

		public GbCDSExportHeader ExportHeaderWrapper { get; private set; }

		protected override GbCDSImportEntryLineWrapper GetLineWrapper(CusEntryLine entryLine)
		{
			return new GbCDSExportEntryLineWrapper(entryLine);
		}

		protected override ITransportMeans GetDepartureTransportMeans() => ((IConsignment)this).ArrivalTransportMeans;

		public ZString MasterUniqueConsignmentReference => ExportHeaderWrapper.MasterUniqueConsignmentReference;

		ZString IUkCinvWrapper.LocationOfGoods
		{
			get
			{
				var iConsignment = (IConsignment)this;
				var gl = iConsignment.GoodsLocation;
				return gl.CountryCode + gl.TypeCode + gl.AddressTypeCode + gl.Name;
			}
		}

		public ZString ShedCode => ExportHeaderWrapper.ShedCode;

		public ZString DeclarationUniqueConsignmentReference => CusEntryHeader.UCRReferencePlaceHolderXmlFriendly;

		public ZString DeclarationUniqueConsignmentReferencePartSuffix => ExportHeaderWrapper.HasDeclarationUCRPartSuffix ? (ZString)CusEntryHeader.UCRPartPlaceHolderXmlFriendly : ZString.Empty;

		public ZDateTime DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises => ExportHeaderWrapper.DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises;

		public ZDateTime DateAndTimeTheGoodsWillBeLeavingLCPPremises => ExportHeaderWrapper.DateAndTimeTheGoodsWillBeLeavingLCPPremises;

		public ZString TransportModeAtTheBorderBox25 => ExportHeaderWrapper.TransportModeAtTheBorderBox25;

		public ZString TransportNationalityAtTheBorderBox21 => ExportHeaderWrapper.TransportNationalityAtTheBorderBox21;

		public ZString TransportIdentityAtTheBorderBox21 => ExportHeaderWrapper.TransportIdentityAtTheBorderBox21;

		public ZString MasterOpt => ExportHeaderWrapper.MasterOpt;

		public ZString MovementReference => ExportHeaderWrapper.MovementReference;

		public ZString CDSDeclarationUniqueConsignmentReference
		{
			get
			{
				var results = CDSDUCRAutomationSetting == (ZString)CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields ?
					DeclarationUniqueConsignmentReference : (ZString)CusEntryHeader.BGMReferencePlaceHolderXmlFriendly;
				return results;
			}
		}

		public ZString CDSDeclarationUniqueConsignmentReferencePartSuffix
		{
			get
			{
				var results = CDSDUCRAutomationSetting == (ZString)CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields ?
						DeclarationUniqueConsignmentReferencePartSuffix : ZString.Empty;
				return results;
			}
		}

		//todo: implement in the future
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "To do comment indicates this will be implemented later")]
		protected override IEnumerable<IParty> GetAEOMutualRecognitionParties()
		{
			return base.GetAEOMutualRecognitionParties();
		}
	}
}
