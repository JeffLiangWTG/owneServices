namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class OutturnLine : AutoOutturnLine
	{
		protected OutturnLine()
		{
		}

		public IScanHouseBillProvider HouseBill { get; set; }

		public CusUnderbond Underbond { get; set; }

		public OutturnLine DeepCopy()
		{
			var result = GetNewOutturnLine();
			result.ManifestInfo = ManifestInfo;
			result.ConsignmentRef = this.ConsignmentRef;
			result.Status = this.Status;
			result.ScannedDateTime = this.ScannedDateTime;
			result.Count = this.Count;
			result.HouseBill = this.HouseBill;
			result.Underbond = this.Underbond;
			return result;
		}

		public IManifestInfo ManifestInfo { get; set; }

		protected abstract OutturnLine GetNewOutturnLine();

		protected override int Status_MaxLength { get { return ScanForOutturnManager.MaifestStatusesMaxLength; } }
	}
}
