using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class ReleaseTypeXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ReleaseTypeXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit, nameof(Xsd.ReleaseType.BRR));
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.BankSightDraft, nameof(Xsd.ReleaseType.BSD));
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.BankTimeDraft, nameof(Xsd.ReleaseType.BTD));
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.CashDoc, nameof(Xsd.ReleaseType.CAD));
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.Cheque, nameof(Xsd.ReleaseType.CSH));
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.ExpressBofL, nameof(Xsd.ReleaseType.EBL));
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.Indemnity, nameof(Xsd.ReleaseType.LOI));
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.OriginalReqSurrender, nameof(Xsd.ReleaseType.OBO));
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.OriginalReq, nameof(Xsd.ReleaseType.OBR));
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.NonNegotiable, nameof(Xsd.ReleaseType.NON));
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.SeaWaybill, nameof(Xsd.ReleaseType.SWB));
		}

		public static readonly ReleaseTypeXmlCodeMappings Instance = new ReleaseTypeXmlCodeMappings();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded name string")]
		protected override string Name
		{
			get { return "Release Type"; }
		}

		public new Xsd.ReleaseType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ReleaseType.OBR, errorContext, notify);
		}
	}
}
