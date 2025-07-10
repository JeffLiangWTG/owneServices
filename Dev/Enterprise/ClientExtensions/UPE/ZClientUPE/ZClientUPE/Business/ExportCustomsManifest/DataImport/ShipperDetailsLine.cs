using CargoWise.Types;

using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class ShipperDetailsLine : UPEDataLine
	{
		public ShipperDetailsLine(UPEManifestImporter importer, ExportCustomsManifestHeader header, string line)
			: base(importer, header, line)
		{
		}

		public override ZBool ValidateLine()
		{
			if (line.Length < 103)
			{
				return ZBool.False;
			}

			return ZBool.True;
		}

		#region Implementation

		protected override void DoProcessing()
		{
			SetOwnerName(SafeSubstring(line, 68, 35).Trim(' '));
		}

		internal protected void SetOwnerName(ZString ownerName)
		{
			if (header.CurrentManifestLine != null && header.CurrentManifestLine.EL_GoodsOwner.IsEmpty)
			{
				header.CurrentManifestLine.EL_GoodsOwner = ownerName;
			}
		}

		#endregion
		#region Implementation
		#endregion

	}
}
