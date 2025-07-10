using System;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
typeof(EDIInterchangeAssemblyData),
Enterprise.Core.Constants.DocManagerCodes.InterchangeAttachments)]

namespace Enterprise.Messaging.Business
{
	class EDIInterchangeAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(EDIInterchange); }
		}

		protected override Type CollectionType
		{
			get { return null; }
		}

		public override string ReferenceType
		{
			get { return Core.Constants.ReferenceTypes.Unallocated; }
		}
	}
}
