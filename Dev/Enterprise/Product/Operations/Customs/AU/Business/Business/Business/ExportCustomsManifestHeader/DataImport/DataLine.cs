using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class DataLine
	{
		public DataLine(ManifestImporter importer, ExportCustomsManifestHeader header, string line)
		{
			this.importer = importer;
			this.header = header;
			this.line = line;
		}

		public virtual void Process()
		{
			DoProcessing();
		}

		protected internal void SetCAN(ZString cAN)
		{
			if (cAN.Length == 4 && new CMRExportExemptionCodesList().ContainsCode(cAN))
			{
				header.CreateExemptLineAndSetCurrentManifestLine(cAN);
				UpdateLineAndSetNewLineAdded();
			}
			else if (!cAN.IsEmpty)
			{
				header.LoadOrCreateLineWithCANIntoCurrentManifestLine(cAN);
				UpdateLineAndSetNewLineAdded();
			}
		}

		protected void SetForEmptyCAN()
		{
			header.CreateEmptyCANLineAndSetCurrentManifestLine();
			UpdateLineAndSetNewLineAdded();
		}

		void UpdateLineAndSetNewLineAdded()
		{
			header.CurrentManifestLine.EL_RN_NKCountryOfDestination = importer.CurrentCountryOfDestination;
			header.CurrentManifestLine.EL_GoodsOwner = importer.CurrentOwnerName;
			importer.NewLineAdded = true;
		}

		protected abstract void DoProcessing();

		protected internal ZString SafeSubstring(ZString source, int start, int length)
		{
			if (start >= source.Length)
			{
				return ZString.Empty;
			}
			else if (start + length >= source.Length)
			{
				return source.Substring(start).Trim(' ');
			}
			else
			{
				return source.Substring(start, length).Trim(' ');
			}
		}

		protected ManifestImporter importer;
		protected ExportCustomsManifestHeader header;
		protected string line;
	}
}
