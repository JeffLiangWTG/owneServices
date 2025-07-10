using CargoWise.Types;

using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class ShipmentDetailsLine : UPEDataLine
	{
		public ShipmentDetailsLine(UPEManifestImporter importer, ExportCustomsManifestHeader header, string line)
			: base(importer, header, line)
		{
		}

		internal bool IsLineCountryDifferentToCurrentCountry()
		{
			return line.Substring(0, 2) != GlbCompany.CurrentCompany.Country.Code;
		}

		public override ZBool ValidateLine()
		{
			if (line.Length < 76)
			{
				return ZBool.False;
			}

			return ZBool.True;
		}

		#region Implementation

		internal void CreateExportManifestLine()
		{
			SetForEmptyCAN();
			SetDescription("Documents Only");
		}

		protected override void DoProcessing()
		{
			SetNoOfContainers();
			SetNoOfPacks(SafeSubstring(line, 74, 2));
		}

		protected void SetDescription(ZString description)
		{
			if (header.CurrentManifestLine != null)
			{
				header.CurrentManifestLine.EL_GoodsDescription = description;
			}
		}

		internal protected void SetNoOfContainers()
		{
			if (header.CurrentManifestLine != null)
			{
				header.CurrentManifestLine.EL_NumberOfContainers = 0;
			}
		}

		internal protected void SetNoOfPacks(ZString strNoOfPacks)
		{
			if (header.CurrentManifestLine != null && header.CurrentManifestLine.EL_NumberOfPackages.IsEmpty)
			{
				ZShort noOfPacks;
				ZShort.TryParse(strNoOfPacks, out noOfPacks);
				header.CurrentManifestLine.EL_NumberOfPackages = noOfPacks;
			}
		}

		#endregion
		#region Implementation
		#endregion

	}
}
