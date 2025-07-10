using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IScanMasterBillProvider
	{
		ZString MasterBill { get; }
		ZString MasterHouseBill { get; }
		ZBool IsStandAlone { get; }

		IEnumerable<CusUnderbond> Underbonds { get; }
		IEnumerable<IScanHouseBillProvider> GetChildBills(CusUnderbond underbond);
		void CreateSurplusConsignment(BusinessObjectFactory factory, OutturnLine outturn);

		BusinessObjectFactory Factory { get; }
	}

	public interface IScanHouseBillProvider
	{
		ZString ShipmentType { get; }
		ZString HouseBill { get; }
		ZString ConsigneeName { get; }
		ZString ConsignorName { get; }
		ZString GoodsDescription { get; }
		IManifestInfo GetManifestInformation(CusUnderbond underbond);
		ZBool ShouldScan(CusUnderbond underbond);
		void LogReadyForLocalDeliveryIfIsCargoStatusClear();
		void ResetCargoReceivedAtDepotLogs(string reference);
	}

	public interface IManifestInfo
	{
		ZInt Quantity { get; }
		ZString UQ { get; }
		ZString GoodsDescription { get; }
		ZString MarksAndNumbers { get; }
		ZString CustomsStatus { get; }
		ZGuid PK { get; }
		string TablePrefix { get; }
	}
}
