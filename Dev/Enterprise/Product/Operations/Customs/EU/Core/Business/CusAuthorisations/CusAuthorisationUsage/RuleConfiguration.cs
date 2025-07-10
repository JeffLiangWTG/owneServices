using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class RuleConfiguration
	{
		internal Func<CusEntryInstruction, bool> Required;
		internal Func<CusEntryInstruction, IEnumerable<(ZGuid holder, ZString number)>> Calculate;

		public RuleConfiguration When(Func<CusEntryInstruction, bool> func)
		{
			Required = func;
			return this;
		}

		public RuleConfiguration ReturnHolderAndNumber(Func<CusEntryInstruction, (ZGuid holder, ZString number)> func)
		{
			Calculate = (CusEntryInstruction entryInstruction) => new[] { func(entryInstruction) };
			return this;
		}

		public RuleConfiguration ReturnHolderAndNumber(Func<CusEntryInstruction, IEnumerable<(ZGuid holder, ZString number)>> func)
		{
			Calculate = func;
			return this;
		}
	}
}
