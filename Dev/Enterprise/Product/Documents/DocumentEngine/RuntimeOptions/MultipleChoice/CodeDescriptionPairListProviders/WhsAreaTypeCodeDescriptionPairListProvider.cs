using System;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class WhsAreaTypeCodeDescriptionPairListProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			ReadOnlyCodeDescriptionPairList whsAreaTypesPairList = (ReadOnlyCodeDescriptionPairList)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Warehouse.Integration.IAreaTypes>());
			return whsAreaTypesPairList;
		}
	}
}
