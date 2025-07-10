using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public abstract class UPEFileExporter
	{
		public void ResetFactory()
		{
			factory = null;
		}

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = NewFactory()); }
		}
		BusinessObjectFactory factory;

		protected virtual BusinessObjectFactory NewFactory()
		{
			BusinessObjectFactory result = new BusinessObjectFactory();
			result.RefreshEnabled = false;
			return result;
		}
	}
}
