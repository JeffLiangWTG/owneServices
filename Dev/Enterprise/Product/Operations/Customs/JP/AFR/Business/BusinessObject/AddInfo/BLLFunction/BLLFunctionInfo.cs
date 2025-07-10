using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class BLLFunctionInfo : AutoBLLFunctionInfo
	{
		public BLLFunctionInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IReadOnlyList<ZString> LinkedBills
		{
			get => JP_LinkedBills.Trim().Split(SplitChar);
			set => JP_LinkedBills = string.Join(SplitChar, value);
		}

		const string SplitChar = "$";
	}
}
