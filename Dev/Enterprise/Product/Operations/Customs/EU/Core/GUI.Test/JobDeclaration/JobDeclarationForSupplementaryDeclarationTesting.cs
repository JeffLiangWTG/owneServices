using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI.Testing;

public class JobDeclarationForSupplementaryDeclarationTesting : JobDeclaration
{
	public JobDeclarationForSupplementaryDeclarationTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override bool IsSupplementaryMenuVisibleCore => true;
}
