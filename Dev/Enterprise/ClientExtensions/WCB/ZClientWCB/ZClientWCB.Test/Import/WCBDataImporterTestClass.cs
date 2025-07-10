using CargoWise.ComponentModel;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Client.WCB.Testing
{
	public class WCBDataImporterTestClass : WCBDataImporter
	{
		public WCBDataImporterTestClass(JobDeclaration jobDec) : base(jobDec)
		{
		}

		public new bool ExtractToDataAdapter(IValueObject xSD, INotifications notificationSubscriber)
		{
			return base.ExtractToDataAdapter(xSD, notificationSubscriber);
		}

		public new BaseJobDeclaration JobDeclaration
		{
			get
			{
				return base.JobDeclaration;
			}
		}
	}
}
