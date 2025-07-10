using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class RFAttrConfirmToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		RFAttrConfirmToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(RFAttributeConfirmCode.Codes.None, nameof(Xsd.RelatedOrganisationRFAttributeConfirm.NON));
			yield return new Mapping(RFAttributeConfirmCode.Codes.PartAttribute1, nameof(Xsd.RelatedOrganisationRFAttributeConfirm.AT1));
			yield return new Mapping(RFAttributeConfirmCode.Codes.PartAttribute2, nameof(Xsd.RelatedOrganisationRFAttributeConfirm.AT2));
			yield return new Mapping(RFAttributeConfirmCode.Codes.PartAttribute3, nameof(Xsd.RelatedOrganisationRFAttributeConfirm.AT3));
		}

		public static readonly RFAttrConfirmToXmlCodeMappings Instance = new RFAttrConfirmToXmlCodeMappings();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded name string")]
		protected override string Name
		{
			get { return "RF Attribute Confirm"; }
		}

		public new Xsd.RelatedOrganisationRFAttributeConfirm GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.RelatedOrganisationRFAttributeConfirm.NON, errorContext, notify);
		}
	}
}
