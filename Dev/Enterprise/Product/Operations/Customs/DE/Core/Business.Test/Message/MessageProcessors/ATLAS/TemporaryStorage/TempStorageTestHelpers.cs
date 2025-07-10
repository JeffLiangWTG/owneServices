using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	static class TempStorageTestHelpers
	{
		internal static CusTempStorageRegLineTransaction GetRegLineTransaction(CusTempStorageRegLine line, ZString transactionType)
		{
			return line.CusTempStorageRegLineTransactions
				.Cast<CusTempStorageRegLineTransaction>()
				.Single(x => x.SRT_TransactionType == transactionType);
		}

		internal static GlbGroup CreateGlbGroup(BusinessObjectFactory factory)
		{
			var emailGroup = factory.NewWithValidTestData<GlbGroup>();
			var staff1 = factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@group-suma.com";
			emailGroup.Staff.Add(staff1);
			var staff2 = factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "staff2@group-suma.com";
			emailGroup.Staff.Add(staff2);
			factory.Save();
			return emailGroup;
		}
	}
}
