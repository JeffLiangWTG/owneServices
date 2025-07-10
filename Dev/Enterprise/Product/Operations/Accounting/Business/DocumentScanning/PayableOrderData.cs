using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(PayableOrderData),
	Enterprise.Core.Constants.DocManagerCodes.PayableOrder)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Accounting.Business.PayableOrder;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class PayableOrderData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get
			{
				return typeof(AccPayableOrderHeader);
			}
		}

		protected override Type CollectionType
		{
			get
			{
				return typeof(AccPayableOrderHeaderCollection);
			}
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccPayableOrderHeaderCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AccPayableOrder;
			}
		}

		public override string ReferenceType
		{
			get
			{
				return Core.Constants.ReferenceTypes.Accounting;
			}
		}

		public override MultilingualString HumanReadableName
		{
			get
			{
				return ResString.GetMultilingualString("1cd4d30d-a081-4b31-af79-5076e021fd6d", "Payable Order");
			}
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get
			{
				return true;
			}
		}
	}
}
