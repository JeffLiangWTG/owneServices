using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Billing.Business;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class GLJournalForADAWDataImporter : MultiCompaniesGLJournalFlatFileDataImporter
	{
		public GLJournalForADAWDataImporter(IBusinessObjectCollection collection)
		{
			Collection = collection;
		}

		readonly IBusinessObjectCollection Collection;

		protected override string ImportTypeForDuplicatesPrevention => "GLJForADAW";

		protected override string UploadGLJournalCountFeatureCode => UsageFeatures.Codes.UploadGLJournalViaADAWCount;

		protected override string UploadGLJournalDetailsFeatureCode => UsageFeatures.Codes.UploadGLJournalViaADAWDetails;

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			Converter = new GLJournalForADAWConverter(notificationSubscriber, Collection.Factory, Collection);
			return Converter;
		}
	}
}
