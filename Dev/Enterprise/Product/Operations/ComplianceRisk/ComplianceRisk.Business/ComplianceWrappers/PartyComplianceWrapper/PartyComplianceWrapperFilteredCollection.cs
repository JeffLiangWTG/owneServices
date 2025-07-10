using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ComplianceRisk.Business
{
	public class PartyComplianceWrapperFilteredCollection : NonPersistentBusinessObject
	{
		public PartyComplianceWrapperFilteredCollection(ScreeningParty[] screeningParties)
		{
			this.screeningParties = screeningParties;
		}

		readonly ScreeningParty[] screeningParties;

		public PartyComplianceWrapperCollection ProcessedParties => processedParties ?? (processedParties = CreateFromScreeningParties(true));
		PartyComplianceWrapperCollection processedParties;

		public PartyComplianceWrapperCollection UnprocessedParties => unprocessedParties ?? (unprocessedParties = CreateFromScreeningParties(false));
		PartyComplianceWrapperCollection unprocessedParties;

		PartyComplianceWrapperCollection CreateFromScreeningParties(bool isScreeningStatusValid)
		{
			var result = new PartyComplianceWrapperCollection();
			foreach (var screeningParty in screeningParties)
			{
				screeningParty.CalculateScreeningStatusIsValid();
				if (screeningParty.IsCurrentScreeningStatusValid == isScreeningStatusValid)
				{
					var wrapper = new PartyComplianceWrapper(screeningParty);
					if (!screeningParty.IsActive)
					{
						wrapper.AddRowWarning(Res.GetString("6cc719ab-f7f9-4948-8b99-490a269c1be4", "This party is inactive and will not be screened."));
					}
					result.Add(wrapper);
				}
			}
			return result;
		}
	}
}
