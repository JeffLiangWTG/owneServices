using System;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing.JobInvoicing
{
	[TestedType(typeof(ApportionmentForCommonWorkSheetController))]
	public class ApportionmentForCommonWorkSheetControllerTest : ApportionmentControllerTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(ApportionmentForCommonWorkSheetController); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ApportionmentForCommonWorkSheet;
		}
	}
}
