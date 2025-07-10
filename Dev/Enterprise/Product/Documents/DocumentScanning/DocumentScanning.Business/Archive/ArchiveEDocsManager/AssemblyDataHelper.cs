using System;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentScanning.Business
{
	public class AssemblyDataHelper : IAssemblyDataHelper
	{
		public Type GetBusinessObjectType(string storageMainType)
		{
			return AssemblyDataLookup.AllAssemblyDataRegardlessOfCompany.GetAssemblyDataFromDocManagerCode(storageMainType).BusinessObjectType;
		}
	}
}
