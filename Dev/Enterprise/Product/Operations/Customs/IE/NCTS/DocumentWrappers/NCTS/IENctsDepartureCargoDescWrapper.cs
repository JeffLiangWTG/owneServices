using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;

namespace Enterprise.Customs.IE.NCTS.DocumentWrappers
{
	public class IENctsDepartureCargoDescWrapper : NctsDepartureCargoDescWrapper
	{
		protected IENctsDepartureCargoDescWrapper(NctsDepartureCargoDesc line, BusinessObjectFactory factory) : base(line, factory)
		{
		}

		public static new IENctsDepartureCargoDescWrapper New(NctsDepartureCargoDesc line, BusinessObjectFactory factory)
		{
			return new IENctsDepartureCargoDescWrapper(line, factory);
		}

		protected override ZString GetBox32ItemNumber() => $"{Line.BY_LineNo} / {Line.BY_DeclarationGoodsItemNumber}";

		protected override ZString GetSupportingDocuments()
		{
			var supportingDocuments = GetAllSupportingDocuments();
			return new IE.DocumentWrappers.IECompactProducedDocumentsCertificatesBuilder().GetProducedDocumentsCertificatesFormatted(supportingDocuments, Line.IsPhase5Departure);
		}
	}
}
