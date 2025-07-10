using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

[assembly: MailSubscriber(typeof(Enterprise.ServiceManager.Tasks.XMLAutomation.LocalCartageBookingEmailReader))]
namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class LocalCartageBookingEmailReader : EmailImportBatchProcessor
	{
		public LocalCartageBookingEmailReader(StringRegistryItem path, NotificationBuffer buffer)
			: base(path, buffer)
		{
		}

		protected override IMailFilter MailFilter { get; } = CreateMailFilter();

		[MailFilter(MailFilterCodes.LocalCartageBooking)]
		public static IMailFilter CreateMailFilter()
			=> new QueryMailFilter(MailFilterCodes.LocalCartageBooking, subjectComparison: SQLComparisonOperator.Contains, subject: FreightConstants.LocalCartageXmlEmailSubject);
	}
}
