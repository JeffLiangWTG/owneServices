using System;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class NZEntryPaymentStatusCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList eCIConsignmentStatusPairList = (CodeDescriptionPairList)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.ILowValueConsignmentStatusList>());
			CodeDescriptionPairList formalEntryStatusPairList = (CodeDescriptionPairList)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.IFormalEntryStatusList>());
			CodeDescriptionPairList nZEntryPaymentStatusCode = new CodeDescriptionPairList(eCIConsignmentStatusPairList + formalEntryStatusPairList);
			return nZEntryPaymentStatusCode;
		}
	}
}
