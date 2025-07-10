using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public class CusTempStorageRegLineTransaction : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction, ICusTempStorageRegLineTransaction
{
	public CusTempStorageRegLineTransaction(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	#region Type Decider

	[ThreadSafe]
	public new static readonly CusTempStorageRegLineTransactionTypeDecider TypeDecider = new();

	#endregion

	public override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine RegLine => regLine ?? Factory.Load<CusTempStorageRegLine>(SRT_SRL);
	readonly CusTempStorageRegLine regLine;
}
