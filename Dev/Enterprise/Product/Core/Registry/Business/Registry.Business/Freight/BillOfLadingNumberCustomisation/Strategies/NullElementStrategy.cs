using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.BillCustomisationStrategies
{
	internal class NullElementStrategy : ElementStrategy
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Hard-coded constant")]
		public override string Key
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return "<null>"; }
		}
		public override string Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (NoResString)"<null>"; }
		}

		public override int CalcMaxGeneratedLength(BillOfLadingNumberCustomisationElement parent)
		{
			return 0;
		}

		public override string GetRegExForDataType(BillOfLadingNumberCustomisationElement parent) => throw new NotImplementedException();
	}
}
