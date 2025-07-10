using System;
using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Enterprise.Build.Database.Script.Testing.Internal.BusinessIntelligence
{
	abstract class RuleVisitor : TSqlFragmentVisitor
	{
		public HashSet<NameAndLocation> Objects
		{
			get;
			private set;
		} = new HashSet<NameAndLocation>();

		public abstract string Message { get; }

		public virtual string[] BaseLine { get; } = Array.Empty<string>();
	}

	struct NameAndLocation
	{
		public string Name { get; set; }
		public int Line { get; set; }

		public override readonly string ToString()
		{
			return $"{Name}, Line: {Line}";
		}
	}
}
