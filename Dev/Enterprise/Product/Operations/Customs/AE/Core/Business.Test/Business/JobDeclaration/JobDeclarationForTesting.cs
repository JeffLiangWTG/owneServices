using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Business.Testing;

public class JobDeclarationForTesting : JobDeclaration
{
	public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public ZString LocalCurrencyCodeCoreExposed
	{
		get
		{
			return LocalCurrencyCodeCore;
		}
	}
}
