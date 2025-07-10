using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocExportCustomsManifestLine : DocumentWrapper
	{
		public static DocExportCustomsManifestLine New(ExportCustomsManifestLines line, BusinessObjectFactory factory)
		{
			return new DocExportCustomsManifestLine(line, factory);
		}

		DocExportCustomsManifestLine(ExportCustomsManifestLines line, BusinessObjectFactory factory)
			: base(line, factory)
		{
		}

		public ZString AirWayBill
		{
			get { return Line.EL_AirWayBill; }
		}

		public ZString GoodsDescription
		{
			get { return Line.EL_GoodsDescription; }
		}

		public ZInt PiecesManifested
		{
			get { return Line.EL_NumberOfPackages; }
		}

		public ZDecimal Weight
		{
			get { return Line.EL_Weight; }
		}

		public ZString WeightUnit
		{
			get { return Line.EL_WeightUQ; }
		}

		public ZDecimal Volume
		{
			get { return Line.EL_Volume; }
		}

		public ZString VolumeUnit
		{
			get { return Line.EL_VolumeUQ; }
		}

		public DocUNLOCO Origin
		{
			get { return DocUNLOCO.New(Line.ConsignorDocumentaryAddress, Factory); }
		}

		public DocUNLOCO Destination
		{
			get { return DocUNLOCO.New(Line.ConsigneeDocumentaryAddress, Factory); }
		}

		public DocDocAddress ConsignorDocumentaryAddress
		{
			get { return DocDocAddress.New(Line.ConsignorDocumentaryAddress, Factory); }
		}

		public DocDocAddress ConsigneeDocumentaryAddress
		{
			get { return DocDocAddress.New(Line.ConsigneeDocumentaryAddress, Factory); }
		}

		#region Implementation

		ExportCustomsManifestLines Line
		{
			get { return (ExportCustomsManifestLines)WrappedObject; }
		}

		#endregion
	}
}
