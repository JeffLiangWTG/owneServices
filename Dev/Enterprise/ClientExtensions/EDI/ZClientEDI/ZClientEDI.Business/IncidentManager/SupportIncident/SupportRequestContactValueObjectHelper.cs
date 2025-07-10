using Enterprise.DataTransfer.DataAdapters;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	public class SupportRequestContactValueObjectHelper : ContactValueObjectHelper
	{
		public SupportRequestContactValueObjectHelper(string errorContext)
			: base(errorContext)
		{
		}

		protected override bool DeactivateExistingContact
		{
			get { return false; }
		}
	}
}

