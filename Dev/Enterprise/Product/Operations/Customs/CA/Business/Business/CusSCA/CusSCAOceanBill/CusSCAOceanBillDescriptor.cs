using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAOceanBillDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.CusSCAOceanBillDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("18919247-9CC3-499C-A09F-47B6C4142DFB", "ACI"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(CusSCAOceanBill); }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}
		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.CA.CusSCAOceanBill; }
		}
	}
}
