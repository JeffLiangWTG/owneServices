using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using static Enterprise.Integration.DocumentWrappers;

namespace Enterprise.DocumentWrappers.Customs.NZ
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
