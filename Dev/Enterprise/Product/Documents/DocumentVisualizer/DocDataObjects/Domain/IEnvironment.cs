using System.Collections.Generic;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	[CodeAlive("API work in progress")]
	public interface IEnvironment
	{
		IUser CurrentUser { get; }
		ICompany Company { get; }
		IBranch Branch { get; }
		ZString LocalCurrency { get; }
		IReadOnlyCollection<ICompany> Companies { get; }

		object GetRegistryItem(string nameOrPath);
	}
}