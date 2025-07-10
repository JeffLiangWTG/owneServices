using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CalcExportManifestLineCollection : NonPersistentBusinessObjectCollection<CalcExportManifestLine>
	{
		public CalcExportManifestLineCollection(CalcExportManifestHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CalcExportManifestLine(header);
		}

		readonly CalcExportManifestHeader header;

		#endregion
	}
}
