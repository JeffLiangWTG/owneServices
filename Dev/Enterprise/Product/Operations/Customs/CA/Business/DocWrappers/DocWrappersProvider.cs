using System;
using CargoWise.EntityFramework;
using static Enterprise.Integration.DocumentWrappers;

namespace Enterprise.Customs.CA.Business
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
