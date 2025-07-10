using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using static Enterprise.Integration.DocumentWrappers;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocWrappersProvider : IDocWrappersProvider
	{
		public Type DocJobDeclarationType => typeof(DocDeclaration);

		public IDocBaseJobDeclaration NewDocDeclarationWrapper(Integration.Customs.IBaseJobDeclaration declaration, BusinessObjectFactory factoryToWrap)
		{
			return DocDeclaration.New(declaration as JobDeclaration, factoryToWrap);
		}
	}
}
