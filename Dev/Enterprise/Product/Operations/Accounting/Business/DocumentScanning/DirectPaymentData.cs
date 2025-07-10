using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(DirectPaymentData),
	Enterprise.Core.Constants.DocManagerCodes.CashBook)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Accounting.Business.CashBook.DirectPayment;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class DirectPaymentData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(DirectPayment); } }
		protected override Type CollectionType
		{
			get { return typeof(DirectPaymentCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new DirectPaymentCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.CashbookTransaction; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("01286f04-912f-4783-ba42-b3c4a550ef29", "Direct Payment"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
