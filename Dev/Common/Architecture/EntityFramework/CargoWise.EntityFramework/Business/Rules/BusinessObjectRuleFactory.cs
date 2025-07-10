using System;
using WTG.Rules.Engine;
using WTG.Rules.Engine.Impl;

namespace CargoWise.EntityFramework.Business.Rules
{
	class BusinessObjectRuleFactory : IRuleFactory
	{
		readonly IRuleMapperAdapter adapter;

		public BusinessObjectRuleFactory(IRuleMapperAdapter adapter)
		{
			this.adapter = adapter;
		}

		public IRule Create(Type type, IRuleInfo ruleInfo)
		{
			return ruleInfo.ToRule(adapter);
		}
	}
}
