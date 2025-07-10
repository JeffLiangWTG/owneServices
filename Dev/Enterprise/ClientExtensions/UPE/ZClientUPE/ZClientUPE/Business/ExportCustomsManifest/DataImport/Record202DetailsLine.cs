using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class Record202DetailsLine : UPEDataLine
	{
		public Record202DetailsLine(UPEManifestImporter importer, ExportCustomsManifestHeader header, string line)
			: base(importer, header, line)
		{
		}

		public override ZBool ValidateLine()
		{
			if (line.Length < 85)
			{
				return ZBool.False;
			}

			return ZBool.True;
		}

		#region Implementation

		internal ZString PackageTrackingNumber
		{
			get { return SafeSubstring(line, 50, 35); }
		}

		protected override void DoProcessing()
		{
			SetEL_AirWayBill(PackageTrackingNumber);
		}

		internal protected void SetEL_AirWayBill(ZString packageTrackingNumber)
		{
			if (header.CurrentManifestLine != null && header.CurrentManifestLine.EL_AirWayBill.IsEmpty)
			{
				header.CurrentManifestLine.EL_AirWayBill = packageTrackingNumber;
			}
		}

		#endregion
		#region Implementation
		#endregion

	}
}
