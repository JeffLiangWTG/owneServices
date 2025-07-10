namespace Enterprise.Customs.CA.Business
{
	using System;
	using CargoWise.EntityFramework;

	public sealed class ReleaseStatusToPrintCollection : NonPersistentBusinessObjectCollection<ReleaseStatus>
	{
		public ReleaseStatusToPrintCollection(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			var releaseStatuses = declaration.ReleaseStatuses;
			foreach (var releaseStatus in releaseStatuses.Where(r => r.CanBePrinted))
			{
				Add(releaseStatus);
			}
			if (Count == 0 && releaseStatuses.LastCommonReleaseMessage != null)
			{
				Add(new ReleaseStatus(releaseStatuses.LastCommonReleaseMessage));
			}
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
