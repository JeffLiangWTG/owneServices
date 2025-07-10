using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaBillCollection : ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>
	{
		public AsycudaBillCollection(AsycudaManifestHeader master)
			: base(master)
		{
			EnableMaxCountValidation();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((AsycudaBill)child).DefaultTransportDocumentType(Master.AMA_TransportMode, Master.SpecificCircumstanceIndicator, Master.AMA_AgentType);
		}

		public void RefreshMaxCountValidation() => this.EnableMaxCountValidation();

		void EnableMaxCountValidation()
		{
			int maxCount = 99;
			var specificCircumstance = Master.SpecificCircumstanceIndicator;
			string errorMessage;
			if (specificCircumstance.In(specificCircumstancesWithMaxOneBill))
			{
				maxCount = 1;
				errorMessage = Res.GetString("C7ED80C7-6EEE-4411-A6D2-3B86F2D01EDD", "For Specific Circumstance '{0}' only one house bill is allowed.", specificCircumstance);
			}
			else
			{
				errorMessage = Res.GetString("2EFC8994-2083-4B86-A90D-0393E734C194", "You may enter a maximum of {0} Bills.", maxCount);
			}
			this.EnableMaxCountValidation(maxCount, warnAtHalfway: false, NotificationType.Error, errorMessage);
		}

		readonly ZString[] specificCircumstancesWithMaxOneBill = new ZString[] { EUICS2SpecificCircumstanceList.Codes.F15, EUICS2SpecificCircumstanceList.Codes.F16, EUICS2SpecificCircumstanceList.Codes.F17, EUICS2SpecificCircumstanceList.Codes.F25 };

		protected override bool AllowNewCore => base.AllowNewCore && !Master.ShouldSynchroniseWithConsol;
	}
}
