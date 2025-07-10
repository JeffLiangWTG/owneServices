using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects
{
	public class DeclarationExporter : NonPersistentBusinessObject
	{
		public DeclarationExporter(IExporterDeclaration exporterDeclaration, BusinessObjectFactory factory, DeclarationExporterMaxLength maxLengthInfo) : base(factory)
		{
			this.exporterDeclaration = Argument.NotNull(exporterDeclaration, nameof(exporterDeclaration));
			this.maxLengthInfo = maxLengthInfo;
		}

		readonly IExporterDeclaration exporterDeclaration;
		readonly DeclarationExporterMaxLength maxLengthInfo;

		#region Place

		[BusinessObjectMaxLengthTestExclude]
		public ZString Place
		{
			get => (place ?? (place = exporterDeclaration.Place)).Value.Substring(0, maxLengthInfo?.PlaceInDocument ?? 100).ToUpperInvariant();
			set => SetNonPersistentPropertyValue(PlaceInfo, ref place, value);
		}

		ZString? place;

		public ZPropertyInfo PlaceInfo => GetZPropertyInfo(nameof(Place));

		#endregion

		#region ReferenceDate

		public ZDate ReferenceDate
		{
			get => (referenceDate ?? (referenceDate = exporterDeclaration.ReferenceDate)).Value;
			set => SetNonPersistentPropertyValue(ReferenceDateInfo, ref referenceDate, value);
		}

		ZDate? referenceDate;

		public ZPropertyInfo ReferenceDateInfo => GetZPropertyInfo(nameof(ReferenceDate));

		#endregion

		#region Supplier Details

		[BusinessObjectMaxLengthTestExclude]
		public ZString SupplierDetails
		{
			get => (supplierDetails ?? (supplierDetails = exporterDeclaration.ExporterDetails)).Value.Substring(0, maxLengthInfo?.SupplierDetailsInDocument ?? 100).ToUpperInvariant();
			set => SetNonPersistentPropertyValue(SupplierDetailsInfo, ref supplierDetails, value);
		}

		ZString? supplierDetails;

		public ZPropertyInfo SupplierDetailsInfo => GetZPropertyInfo(nameof(SupplierDetails));

		#endregion
	}
}
