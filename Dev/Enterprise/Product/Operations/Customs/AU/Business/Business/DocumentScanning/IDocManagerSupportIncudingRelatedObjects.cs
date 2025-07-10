using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IDocManagerSupportIncudingRelatedObjects : IDocManagerSupport
	{
		BusinessObject SelfReference { get; }
		IEnumerable<BusinessObject> GetRelatedBusinessObjects();
	}
}
