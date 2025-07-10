using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryPayInfo : Customs.Business.CusEntryPayInfo
{
	public CusEntryPayInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoCusEntryPayInfo.Schema
	{
		public const string A93Number = "A93Number";
		public const string MethodOfPayment = "MethodOfPayment";
	}

	public ZString A93Number => C9_IncomingPayResponseNo;
	public ZString MethodOfPayment => C9_PaymentParty;
}
