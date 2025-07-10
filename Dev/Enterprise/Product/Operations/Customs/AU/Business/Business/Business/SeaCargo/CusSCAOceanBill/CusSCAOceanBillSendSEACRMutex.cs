
namespace Enterprise.Customs.AU.Declaration.Business
{
	using Enterprise.ZArchitecture.Modules;

	public class CusSCAOceanBillSendSEACRMutex : MutexID
	{
		public static CusSCAOceanBillSendSEACRMutex Instance { get; } = new CusSCAOceanBillSendSEACRMutex();

		protected CusSCAOceanBillSendSEACRMutex()
			: base("CusSCAOceanBillSendSEACRMutex", "CusSCAOceanBill SEACR Message Sending Lock.")
		{
		}
	}
}
